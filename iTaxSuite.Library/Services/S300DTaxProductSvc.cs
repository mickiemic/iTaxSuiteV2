using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Interfaces;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Sage.CA.SBS.ERP.Sage300.Common.Models;
using StackExchange.Redis;

namespace iTaxSuite.Library.Services
{
    public class S300DTaxProductSvc : S300BaseProductSvc, IS300ProductSvc
    {
        private readonly IDigiTaxService _dTaxService;

        public S300DTaxProductSvc(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, IHttpClientFactory httpClientFactory, 
            ExtSystConfig extSystConfig, IMasterDataSvc masterDataSvc, IDigiTaxService dTaxService) 
            : base(dbContext, multiplexer, extSystConfig, masterDataSvc, httpClientFactory)
        {
            _syncChannelMap = _masterDataSvc.GetChannelsAsync().GetAwaiter().GetResult();
            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();
            
            _dTaxService = dTaxService;
        }

        public TaxDeviceType GetDeviceType()
        {
            return TaxDeviceType.DIGITAX;
        }

        public async Task<List<Product>> FetchICProducts()
        {
            string _method_ = "FetchICProducts";
            List<Product> products = new();
            string _strError = string.Empty;
            try
            {
                var syncChannel = _syncChannelMap[GeneralConst.IC_PRODUCT_SYNC];
                var productMap = await _masterDataSvc.GetCacheStockItems();
                var qParams = new Dictionary<string, string>();

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/IC/ICItems");

                bool loop = true;
                while (loop)
                {
                    qParams["$skip"] = syncChannel.OffSet.ToString();
                    var icItems = await client.ProcessGetReqBasicAsync<ICItems>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                        null, qParams);
                    if (icItems == null)
                        throw new Exception("Null ICItems response from Sage");
                    loop = (icItems.nextLink != null);
                    syncChannel.IncrOffSet(icItems.Items.Count);

                    icItems.Items.RemoveAll(i => productMap.ContainsKey($"IC:{i.ItemNumber}"));

                    foreach (var item in icItems.Items)
                    {
                        var product = new Product(item);
                        var mapResult = await _masterDataSvc.MapItemAttribs(product._pkgUnitCode, product._qtyUnitCode);
                        if (mapResult.IsError)
                        {
                            _strError = mapResult.GetError();
                            UI.Error($"{_method_} error : {_strError}");
                            product.PackageUnit = product.QuantityUnit = ETIMSConst.NOUNIT_CODE;
                            product.RecordStatus = RecordStatus.INVALID;
                        }
                        else
                        {
                            product.UpdateAttributes(mapResult.GetValue());
                        }
                        products.Add(product);

                        var stockItem = new StockItem(product, _clientBranch);

                        var productData = new ProductData(_clientBranch, stockItem, item);
                        product.ProductData = productData;

                        using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                        {
                            int _etrSeqValue = _clientBranch.ProductSeq;
                            try
                            {
                                if (_dbContext.Products.AddIfNotExists(product, p => p.ProductCode == product.ProductCode) == null)
                                {
                                    UI.Warn($"Product {product.ProductCode} Already Exists");
                                    continue;
                                }
                                _dbContext.Attach(_clientBranch);
                                if (_dbContext.SaveChanges() < 1)
                                {
                                    throw new Exception($"IC Product {product.ProductCode} saving to database failed");
                                }
                                if (_dbContext.StockItems.AddIfNotExists(stockItem, x => x.ProductCode == stockItem.ProductCode
                                    && x.BranchCode == stockItem.BranchCode) == null)
                                {
                                    UI.Warn($"StockItem {stockItem.CacheKey} Already Exists");
                                    continue;
                                }

                                syncChannel.UpdateTracker(item.ItemNumber);
                                _clientBranch.ProductSeq = (_etrSeqValue + 1);
                                if (!await _masterDataSvc.UpdateBranchTrxAsync(_clientBranch, _dbContext))
                                {
                                    throw new Exception($"{_method_} - UpdateBranchTrxAsync : Failed Updating ClientBranch Details");
                                }
                                if (!await _masterDataSvc.SaveSyncTrxChannel(syncChannel, _dbContext))
                                {
                                    UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating ItemsSync");
                                }

                                if (_dbContext.SaveChanges() < 1)
                                {
                                    throw new Exception($"StockItem {stockItem.CacheKey} saving to database failed");
                                }

                                if (!await _masterDataSvc.CacheSaveStockItem(GeneralConst.IC_PRODUCT_SYNC, stockItem))
                                {
                                    throw new Exception($"IC Product {product.ProductCode} saving to cache failed");
                                }

                                await _dbTrans.CommitAsync();
                                _dbContext.ChangeTracker.Clear();

                                await _masterDataSvc.UpdateSyncTrxTracker(syncChannel);
                            }
                            catch (Exception iex)
                            {
                                await _dbTrans.RollbackAsync();
                                _dbContext.ChangeTracker.Clear();
                                _clientBranch.ProductSeq = _etrSeqValue;
                                UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                                continue;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }

            return products;
        }
        public async Task<List<Product>> FetchARProducts()
        {
            string _method_ = "FetchARProducts";
            List<Product> products = new();
            string _strError = string.Empty;
            try
            {
                var syncChannel = _syncChannelMap[GeneralConst.AR_PRODUCT_SYNC];
                var productMap = await _masterDataSvc.GetCacheStockItems();
                var qParams = new Dictionary<string, string>();

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/AR/ARItems");

                bool loop = true;
                while (loop)
                {
                    qParams["$skip"] = syncChannel.OffSet.ToString();
                    var arItems = await client.ProcessGetReqBasicAsync<ARItems>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                        null, qParams);
                    if (arItems == null)
                        throw new Exception("Null ARItems response from Sage");
                    loop = (arItems.nextLink != null);
                    syncChannel.IncrOffSet(arItems.Items.Count);

                    arItems.Items.RemoveAll(i => productMap.ContainsKey($"AR:{i.ItemNumber}"));

                    foreach (var item in arItems.Items)
                    {
                        var product = new Product(item);
                        var mapResult = await _masterDataSvc.MapItemAttribs(product._pkgUnitCode, product._qtyUnitCode);
                        if (mapResult.IsError)
                        {
                            _strError = mapResult.GetError();
                            UI.Error($"{_method_} error : {_strError}");
                            product.PackageUnit = product.QuantityUnit = ETIMSConst.NOUNIT_CODE;
                            product.RecordStatus = RecordStatus.INVALID;
                        }
                        else
                        {
                            product.UpdateAttributes(mapResult.GetValue());
                        }
                        products.Add(product);

                        var stockItem = new StockItem(product, _clientBranch);

                        var productData = new ProductData(_clientBranch, stockItem, item);
                        product.ProductData = productData;

                        using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                        {
                            int _etrSeqValue = _clientBranch.ProductSeq;
                            try
                            {
                                if (_dbContext.Products.AddIfNotExists(product, p => p.ProductCode == product.ProductCode) == null)
                                {
                                    UI.Warn($"Product {product.ProductCode} Already Exists");
                                    continue;
                                }
                                if (_dbContext.SaveChanges() < 1)
                                {
                                    throw new Exception($"Product {product.ProductCode} saving to database failed");
                                }
                                if (_dbContext.StockItems.AddIfNotExists(stockItem, x => x.ProductCode == stockItem.ProductCode
                                    && x.BranchCode == stockItem.BranchCode) == null)
                                {
                                    UI.Warn($"StockItem {stockItem.CacheKey} Already Exists");
                                    continue;
                                }

                                _clientBranch.ProductSeq = (_etrSeqValue + 1);
                                if (!await _masterDataSvc.UpdateBranchTrxAsync(_clientBranch, _dbContext))
                                {
                                    throw new Exception($"{_method_} - UpdateBranchTrxAsync : Failed Updating ClientBranch Details");
                                }
                                if (!await _masterDataSvc.SaveSyncTrxChannel(syncChannel, _dbContext))
                                {
                                    UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating ItemsSync");
                                }

                                if (_dbContext.SaveChanges() < 1)
                                {
                                    throw new Exception($"StockItem {stockItem.CacheKey} saving to database failed");
                                }

                                if (!await _masterDataSvc.CacheSaveStockItem(GeneralConst.IC_PRODUCT_SYNC, stockItem))
                                {
                                    throw new Exception($"Product {product.ProductCode} saving to cache failed");
                                }

                                syncChannel.UpdateTracker(product.ProductCode);
                                await _dbTrans.CommitAsync();

                                _dbContext.ChangeTracker.Clear();
                                await _masterDataSvc.UpdateSyncTrxTracker(syncChannel);
                            }
                            catch (Exception iex)
                            {
                                await _dbTrans.RollbackAsync();
                                _dbContext.ChangeTracker.Clear();
                                _clientBranch.ProductSeq = _etrSeqValue;
                                UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                                continue;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }

            return products;
        }
        public async Task<List<Product>> FetchGLProducts()
        {
            string _method_ = "FetchGLProducts";
            List<Product> products = new();
            string _strError = string.Empty;
            try
            {
                var syncChannel = _syncChannelMap[GeneralConst.GL_PRODUCT_SYNC];
                var productMap = await _masterDataSvc.GetCacheStockItems();
                var qParams = new Dictionary<string, string>();

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/GL/GLAccountGroups");
                qParams["$filter"] = "GroupCategory eq 'Revenue' or GroupCategory eq 'OtherRevenue'";
                var accGroups = await client.ProcessGetReqBasicAsync<GLAccountGroups>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                        null, qParams);
                if (accGroups == null || accGroups.AccountGroups.Count != 2)
                {
                    throw new Exception($"Null/Invalid GLAccountGroups response from Sage, count: {accGroups?.AccountGroups?.Count}");
                }
                var groupCodes = new HashSet<string>();
                accGroups.AccountGroups.ForEach(x => groupCodes.Add(x.AccountGroupCode.ToString()));
                var strFilter = "AccountGroupCode eq '" + string.Join("' or AccountGroupCode eq '", groupCodes) + "'";

                bool loop = true;
                _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/GL/GLAccounts");
                qParams["$filter"] = strFilter;

                while (loop)
                {
                    qParams["$skip"] = syncChannel.OffSet.ToString();
                    var glAccounts = await client.ProcessGetReqBasicAsync<GLAccounts>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                        null, qParams);
                    if (glAccounts == null)
                        throw new Exception("Null GLAccounts response from Sage");
                    loop = (glAccounts.nextLink != null);
                    syncChannel.IncrOffSet(glAccounts.Accounts.Count);

                    glAccounts.Accounts.RemoveAll(i => productMap.ContainsKey($"GL:{i.AccountNumber}"));
                    foreach (var account in glAccounts.Accounts)
                    {
                        var product = new Product(account);
                        var mapResult = await _masterDataSvc.MapItemAttribs(product._pkgUnitCode, product._qtyUnitCode);
                        if (mapResult.IsError)
                        {
                            _strError = mapResult.GetError();
                            UI.Error($"{_method_} error : {_strError}");
                            product.PackageUnit = product.QuantityUnit = ETIMSConst.NOUNIT_CODE;
                            product.RecordStatus = RecordStatus.INVALID;
                        }
                        else
                        {
                            product.UpdateAttributes(mapResult.GetValue());
                        }
                        products.Add(product);

                        var stockItem = new StockItem(product, _clientBranch);

                        var productData = new ProductData(_clientBranch, stockItem, account);
                        product.ProductData = productData;

                        using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                        {
                            int _etrSeqValue = _clientBranch.ProductSeq;
                            try
                            {
                                if (_dbContext.Products.AddIfNotExists(product, p => p.ProductCode == product.ProductCode) == null)
                                {
                                    UI.Warn($"Product {product.ProductCode} Already Exists");
                                    continue;
                                }
                                if (_dbContext.SaveChanges() < 1)
                                {
                                    throw new Exception($"GL Product {product.ProductCode} saving to database failed");
                                }
                                if (_dbContext.StockItems.AddIfNotExists(stockItem, x => x.ProductCode == stockItem.ProductCode
                                    && x.BranchCode == stockItem.BranchCode) == null)
                                {
                                    UI.Warn($"StockItem {stockItem.CacheKey} Already Exists");
                                    continue;
                                }

                                _clientBranch.ProductSeq = (_etrSeqValue + 1);
                                if (!await _masterDataSvc.UpdateBranchTrxAsync(_clientBranch, _dbContext))
                                {
                                    throw new Exception($"{_method_} - UpdateBranchTrxAsync : Failed Updating ClientBranch Details");
                                }
                                if (!await _masterDataSvc.SaveSyncTrxChannel(syncChannel, _dbContext))
                                {
                                    UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating ItemsSync");
                                }

                                if (_dbContext.SaveChanges() < 1)
                                {
                                    throw new Exception($"StockItem {stockItem.CacheKey} saving to database failed");
                                }

                                if (!await _masterDataSvc.CacheSaveStockItem(GeneralConst.IC_PRODUCT_SYNC, stockItem))
                                {
                                    throw new Exception($"GL Product {product.ProductCode} saving to cache failed");
                                }

                                syncChannel.UpdateTracker(product.ProductCode);
                                await _dbTrans.CommitAsync();

                                _dbContext.ChangeTracker.Clear();
                                await _masterDataSvc.UpdateSyncTrxTracker(syncChannel);
                            }
                            catch (Exception iex)
                            {
                                await _dbTrans.RollbackAsync();
                                _dbContext.ChangeTracker.Clear();
                                _clientBranch.ProductSeq = _etrSeqValue;
                                UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }

            return products;
        }
        
        public async Task<Result<List<string>, string>> SyncTaxProducts()
        {
            string _method_ = "SyncTaxProducts";
            string _strError = string.Empty;
            List<string> result = new();
            try
            {
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL };
                
                var stockItemMap = await _dbContext.StockItems.Include(stock => stock.Product)
                        .ThenInclude(prod => prod.ProductData)
                        .Where(x => !completeStatii.Contains(x.RecordStatus))
                        .AsSplitQuery().ToDictionaryAsync(x => x.ProductCode, x => x);
                if (stockItemMap == null || stockItemMap.Count == 0)
                {
                    return "There are no items to update";
                }

                var dTaxRes = await _dTaxService.GetDTaxProducts();
                if (dTaxRes.IsError)
                {
                    _strError = dTaxRes.GetError();
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }
                var dTaxMap = dTaxRes.GetValue().Items.ToDictionary(x => x.BarCode, x => x);

                var commonKeys = dTaxMap.Keys.Intersect(stockItemMap.Keys);
                foreach (var commonKey in commonKeys)
                {
                    var dTaxItem = dTaxMap[commonKey];
                    var stockItem = stockItemMap[commonKey];

                    if (dTaxItem.Status == DTaxSyncStatus.COMPLETED && stockItem.RecordStatus != RecordStatus.POST_OK)
                    {
                        using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                var tStamp = DateTime.Now;

                                await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode).ExecuteUpdateAsync(x => x
                                    .SetProperty(x => x.ResponsePayload, JsonConvert.SerializeObject(dTaxItem))
                                    .SetProperty(x => x.ResponseTime, tStamp)
                                    .SetProperty(x => x.UpdatedOn, tStamp)
                                    .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                                );
                                await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                                    .ExecuteUpdateAsync(x => x
                                    .SetProperty(x => x.ExternalID, dTaxItem.ID)
                                    .SetProperty(x => x.TaxItemCode, dTaxItem.EtimsItemCode)
                                    .SetProperty(x => x.Remark, "Synced Successfully")
                                    .SetProperty(x => x.RecordStatus, RecordStatus.POST_OK)
                                    .SetProperty(x => x.Tries, x => x.Tries + 1)
                                    .SetProperty(x => x.LastTry, tStamp)
                                    .SetProperty(x => x.UpdatedOn, tStamp)
                                    .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                                );
                                await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                                    .ExecuteUpdateAsync(x => x
                                    .SetProperty(x => x.Remark, "Synced Successfully")
                                    .SetProperty(x => x.RecordStatus, RecordStatus.POST_OK)
                                    .SetProperty(x => x.Tries, x => x.Tries + 1)
                                    .SetProperty(x => x.LastTry, tStamp)
                                    .SetProperty(x => x.UpdatedOn, tStamp)
                                    .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                                );

                                await _dbTrans.CommitAsync();
                                result.Add(commonKey);
                            }
                            catch (Exception iex)
                            {
                                await _dbTrans.RollbackAsync();
                                UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                                throw;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }

            return result;
        }

        public async Task<Result<StockMovement, string>> CreateStockMovement(StockIORequest stockIORequest)
        {
            string _method_ = "CreateStockMovement";
            StockMovement stockMovement = null;
            string _strError = string.Empty;
            try
            {
                var stockItem = await _dbContext.StockItems.Include(e => e.Product).AsSplitQuery()
                    .FirstOrDefaultAsync(x => x.BranchCode == _clientBranch.BranchCode
                    && x.ProductCode == stockIORequest.ProductCode);
                if (stockItem is null)
                {
                    _strError = $"StockItem with ProductCode:{stockIORequest.ProductCode} not found";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }

                stockMovement = new StockMovement(_clientBranch, stockIORequest);
                var stockIOSaveReq = new DTaxStockAdjustReq(_clientBranch, stockIORequest, stockItem);
                var stockTrxData = new StockMovData(stockMovement, stockIORequest, stockIOSaveReq);
                stockMovement.StockMovData = stockTrxData;

                var strJson = JsonConvert.SerializeObject(stockIOSaveReq);

                return stockMovement;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }
        public Task<Result<EtimsTransact, string>> ProcessSaveProduct(EtimsTransact transact)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<EtimsTransact, string>> ProcessSaveStockIO(EtimsTransact transact)
        {
            string _method_ = "ProcessQueueSaveProduct";
            string _strError = string.Empty;
            try
            {
                var _docParts = transact.DocNumber.Split(":");

                // Get Item Status
                var stockItem = await _dbContext.StockItems.Include(e => e.Product).ThenInclude(p => p.ProductData)
                    .Where(e => e.BranchCode == _docParts[0] && e.ProductCode == _docParts[1]).OrderBy(e => e.CreatedOn)
                    .AsSplitQuery().AsNoTracking().FirstOrDefaultAsync();
                var dTaxRequest = stockItem.Product.ProductData.GetDTaxRequest();

                var dTaxResp = await _dTaxService.CreateDTaxItem(dTaxRequest);
                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var tStamp = DateTime.Now;
                        var recordStatus = RecordStatus.POST_FAIL;

                        if (dTaxResp.IsError)
                        {
                            _strError = dTaxResp.GetError();
                            UI.Error($"Saving Stock Item:{stockItem.CacheKey} failed: {dTaxResp.GetError()}");
                            transact.RespPayload = _strError;

                            // Update & Save Transact Changes
                            await _dbContext.EtimsTransacts.Where(x => x.DocNumber == transact.DocNumber && x.ReqType == transact.ReqType
                                && x.BranchCode == transact.BranchCode && x.DocStamp == transact.DocStamp).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );
                            await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, _strError)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );
                            await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.Remark, _strError)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );
                            await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.Remark, _strError)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );

                            await _dbTrans.CommitAsync();
                            return _strError;
                        }

                        DTaxCreateItemResp createItemResp = dTaxResp.GetValue();
                        transact.RespPayload = createItemResp.RawResponse;
                        if (createItemResp.IsSuccess)
                            recordStatus = RecordStatus.POST_OK;

                        var _remark = $"{createItemResp.Status.ToString()} on {tStamp.ToString("s")}";

                        // Update & Save Transact Changes
                        await _dbContext.EtimsTransacts.Where(x => x.DocNumber == transact.DocNumber && x.ReqType == transact.ReqType
                            && x.BranchCode == transact.BranchCode && x.DocStamp == transact.DocStamp).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ResponsePayload, createItemResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, _remark)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, _remark)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );

                        await _dbTrans.CommitAsync();

                    }
                    catch (Exception iex)
                    {
                        await _dbTrans.RollbackAsync();
                        UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                        throw;
                    }
                }

                return transact;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<EtimsTransact, string>> QueueSaveProduct(BranchStockKey filter)
        {
            string _method_ = "QueueSaveProduct";
            EtimsTransact transact = null;
            string _strError = string.Empty;
            try
            {
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL, RecordStatus.DEPENDS };

                if (string.IsNullOrWhiteSpace(filter.ProductCode) || string.IsNullOrWhiteSpace(filter.BranchCode))
                    return $"Invalid Filter Provided : [{filter.BranchCode}:{filter.ProductCode}]";

                var stockItem = await _dbContext.StockItems.Include(e => e.Product).ThenInclude(p => p.ProductData)
                    .AsSplitQuery().FirstOrDefaultAsync(e => e.BranchCode.Equals(filter.BranchCode) && e.ProductCode.Equals(filter.ProductCode)
                    && !completeStatii.Contains(e.RecordStatus));
                if (stockItem is null)
                    return $"No valid stock item found for ProductCode: {filter.ProductCode}";

                var dTaxRequest = stockItem.Product.ProductData.GetDTaxRequest();
                if (!dTaxRequest.IsValid())
                    return $"Invalid DigiTax CreateItem request for ProductCode: {filter.ProductCode}";

                var dTaxResp = await _dTaxService.CreateDTaxItem(dTaxRequest);
                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    var tStamp = DateTime.Now;
                    var recordStatus = RecordStatus.POST_FAIL;
                    try
                    {
                        if (dTaxResp.IsError)
                        {
                            _strError = dTaxResp.GetError();
                            UI.Error($"Saving Stock Item:{stockItem.CacheKey} failed: {dTaxResp.GetError()}");

                            // Update & Save Transact Changes
                            await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, _strError)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );
                            await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.Remark, _strError)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );
                            await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.Remark, _strError)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );

                            await _dbTrans.CommitAsync();
                            return _strError;
                        }

                        DTaxCreateItemResp createItemResp = dTaxResp.GetValue();
                        if (createItemResp.IsSuccess)
                            recordStatus = RecordStatus.POST_OK;

                        var _remark = $"{createItemResp.Status.ToString()} on {tStamp.ToString("s")}";

                        // Update & Save Transact Changes
                        await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ResponsePayload, createItemResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ExternalID, createItemResp.ID)
                            .SetProperty(x => x.TaxItemCode, createItemResp.EtimsItemCode)
                            .SetProperty(x => x.Remark, _remark)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, _remark)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );

                        await _dbTrans.CommitAsync();
                    }
                    catch (Exception iex)
                    {
                        await _dbTrans.RollbackAsync();
                        UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                        throw;
                    }
                }

                return transact;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<List<EtimsTransact>,string>> PostPendingProducts()
        {
            string _method_ = "PostPendingProducts";
            var result = new List<EtimsTransact>();
            try
            {
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL, RecordStatus.DEPENDS };
                var pendingItems = await _dbContext.StockItems.Include(e => e.Product).ThenInclude(p => p.ProductData)
                    .AsSplitQuery().Where(e => !completeStatii.Contains(e.RecordStatus) 
                    && !string.IsNullOrWhiteSpace(e.Product.ProductData.RequestPayload)).ToListAsync();
                foreach(var stockItem in pendingItems)
                {
                    var stockKey = new BranchStockKey()
                    {
                        ProductCode = stockItem.ProductCode
                    };
                    var processResult = await QueueSaveProduct(stockKey);
                    if (processResult.IsError)
                    {
                        UI.Error($"{_method_} error: {processResult.GetError()}");
                        continue;
                    }
                    result.Add(processResult.GetValue());
                }

                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<int,string>> ProcessItemCallback(ItemCallback itemCallback)
        {
            string _method_ = "ProcessItemCallback";
            try
            {
                int changes = 0;
                UI.Info($"{_method_} >> {JsonConvert.SerializeObject(itemCallback)}");
                var stockItem = await _dbContext.StockItems.Include(e => e.Product).ThenInclude(p => p.ProductData)
                    .AsSplitQuery().FirstOrDefaultAsync(e => e.ExternalID.Equals(itemCallback.CBData.ID));
                if (stockItem is null)
                {
                    return $"Invalid DigiTax CreateItem request for ExternalID: {itemCallback.CBData.ID}";
                }

                var tStamp = DateTime.Now;
                stockItem.Remark = stockItem.Product.Remark = $"Callback Complete at {tStamp.ToString("s")}";
                stockItem.UpdatedOn = stockItem.Product.UpdatedOn = tStamp;
                changes += await _dbContext.SaveChangesAsync();

                changes += await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.CallbackTime, tStamp)
                            .SetProperty(x => x.CallbackPayload, JsonConvert.SerializeObject(itemCallback))
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );

                return changes;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public Task<Result<BranchStockLevel, string>> SaveStockLevel(SaveStockLevel saveStockLevel)
        {
            throw new NotImplementedException();
        }
    }
}
