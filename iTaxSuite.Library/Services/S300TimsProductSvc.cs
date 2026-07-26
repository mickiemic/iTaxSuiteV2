using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Interfaces;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace iTaxSuite.Library.Services
{
    public class S300TimsProductSvc : S300BaseProductSvc, IS300ProductSvc
    {
        private readonly IEtimsService _etimsService;

        public S300TimsProductSvc(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, ExtSystConfig extSystConfig, 
            IMasterDataSvc masterDataSvc, IHttpClientFactory httpClientFactory, IEtimsService etimsService)
            : base(dbContext, multiplexer, extSystConfig, masterDataSvc, httpClientFactory)
        {
            _syncChannelMap = _masterDataSvc.GetChannelsAsync().GetAwaiter().GetResult();
            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();
            
            _etimsService = etimsService;
        }
        public TaxDeviceType GetDeviceType()
        {
            return TaxDeviceType.VSCU;
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

                        stockItem.TaxItemCode = productData.SaveItemReq.ItemCode;

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

                        stockItem.TaxItemCode = productData.SaveItemReq.ItemCode;

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

                        stockItem.TaxItemCode = productData.SaveItemReq.ItemCode;

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

        public async Task<Result<StockMovement, string>> CreateStockMovement(StockIORequest stockIORequest)
        {
            string _method_ = "CreateStockMovement";
            StockMovement stockMovement = null;
            string _strError = string.Empty;
            try
            {
                var stockItem = await _dbContext.StockItems.Include(e => e.Product)
                    .AsSplitQuery().FirstOrDefaultAsync(x => x.BranchCode == _clientBranch.BranchCode
                    && x.ProductCode == stockIORequest.ProductCode);
                if (stockItem is null)
                {
                    _strError = $"StockItem with ProductCode:{stockIORequest.ProductCode} not found";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }

                stockMovement = new StockMovement(_clientBranch, stockIORequest);
                var stockIOSaveReq = new StockIOSaveReq(_clientBranch, stockIORequest, stockItem);
                var stockTrxData = new StockMovData(stockMovement, stockIORequest, stockIOSaveReq);
                stockMovement.StockMovData = stockTrxData;

                var strJosn = JsonConvert.SerializeObject(stockIOSaveReq);

                return stockMovement;
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
            try
            {
                if (string.IsNullOrWhiteSpace(filter.ProductCode) || string.IsNullOrWhiteSpace(filter.BranchCode))
                    return $"Invalid Filter Provided : [{filter.BranchCode}:{filter.ProductCode}]";

                var stockItem = await _dbContext.StockItems.Include(e => e.Product).Include(e => e.Product.ProductData)
                    .Where(e => e.BranchCode.Equals(filter.BranchCode) && e.ProductCode.Equals(filter.ProductCode))
                    .AsSplitQuery().FirstOrDefaultAsync();
                if (stockItem is null)
                    return $"No valid stock item found for ProductCode: {filter.ProductCode}";
                //TODO: Check Status before queueing

                transact = stockItem.GetTransaction(_clientBranch);
                if (transact is null)
                    return $"No valid transaction generated for ProductCode: {filter.ProductCode}";
                UI.Info($"transact = {JsonConvert.SerializeObject(transact)}");

                if (_dbContext.EtimsTransacts.AddIfNotExists(transact, x => x.DocNumber == transact.DocNumber 
                    && x.ReqType == transact.ReqType && x.BranchCode == transact.BranchCode && x.DocStamp == transact.DocStamp) == null)
                {
                    return $"EtimsTransaction for ProductCode: {filter.ProductCode} Already Exists";
                }
                if (_dbContext.SaveChanges() < 1)
                {
                    throw new Exception($"EtimsTransaction {filter.ProductCode} saving to database failed");
                }
                return transact;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<EtimsTransact, string>> ProcessSaveProduct(EtimsTransact transact)
        {
            string _method_ = "ProcessQueueSaveProduct";
            string _strError = string.Empty;
            try
            {
                var _docParts = transact.DocNumber.Split(":");

                // Get Item Status
                var stockItem = await _dbContext.StockItems.Include(e => e.Product).Include(e => e.Product.ProductData)
                    .Where(e => e.BranchCode == _docParts[0] && e.ProductCode == _docParts[1]).OrderBy(e => e.CreatedOn)
                    .AsSplitQuery().AsNoTracking().FirstOrDefaultAsync();
                var etimsRequest = stockItem.Product.ProductData.GetEtimsRequest();

                var eTimsResp = await _etimsService.CreateEtimsItem(etimsRequest);
                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var tStamp = DateTime.Now;
                        var recordStatus = RecordStatus.POST_FAIL;

                        if (eTimsResp.IsError)
                        {
                            _strError = eTimsResp.GetError();
                            UI.Error($"Saving Stock Item:{stockItem.CacheKey} failed: {eTimsResp.GetError()}");
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
                        
                        SaveItemResp saveItemResp = eTimsResp.GetValue();
                        transact.RespPayload = saveItemResp.RawResponse;
                        if (saveItemResp.IsSuccess)
                            recordStatus = RecordStatus.POST_OK;

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
                            .SetProperty(x => x.ResponsePayload, saveItemResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, saveItemResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, saveItemResp.ResultMsg)
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

        public async Task<Result<BranchStockLevel, string>> SaveStockLevel(SaveStockLevel saveStockLevel)
        {
            string _method_ = "SaveStockLevel";
            BranchStockLevel result = null;
            string _strError = string.Empty;
            try
            {
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL, RecordStatus.POST_FAIL };
                // Get Item Status
                var stockItem = await _dbContext.StockItems.Include(e => e.Product).Include(e => e.Product.ProductData)
                    .Where(e => e.BranchCode == saveStockLevel.BranchCode && e.ProductCode == saveStockLevel.ProductCode)
                    .OrderBy(e => e.CreatedOn).AsSplitQuery().AsNoTracking().FirstOrDefaultAsync();
                if (stockItem is null)
                {
                    _strError = $"StockItem with BranchCode {saveStockLevel.BranchCode} and ProductCode {saveStockLevel.ProductCode} Not Found";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }
                if (!completeStatii.Contains(stockItem.RecordStatus))
                {
                    _strError = $"StockItem with BranchCode {saveStockLevel.BranchCode} and ProductCode {saveStockLevel.ProductCode} Status is not registered successfully";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }

                var stockMstSaveReq = new StockMstSaveReq(_clientBranch, stockItem, saveStockLevel);
                var eTimsResp = await _etimsService.SaveEtimsStockMaster(stockMstSaveReq);
                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var tStamp = DateTime.Now;
                        if (eTimsResp.IsError)
                        {
                            _strError = eTimsResp.GetError();
                            UI.Error($"Saving Stock Level for {stockItem.CacheKey} failed: {eTimsResp.GetError()}");
                            return _strError;
                        }

                        var stockMstSaveResp = eTimsResp.GetValue();
                        if (!stockMstSaveResp.IsSuccess)
                        {
                            _strError = stockMstSaveResp.RawResponse;
                            UI.Error($"Saving Stock Level for {stockItem.CacheKey} failed: {_strError}");
                            return _strError;
                        }

                        await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.StockCount, saveStockLevel.StockLevel)
                            .SetProperty(x => x.CountTime, tStamp)
                            .SetProperty(x => x.LastStockSave, tStamp)
                        );

                        await _dbTrans.CommitAsync();
                        result = new BranchStockLevel()
                        {
                            BranchCode = stockItem.BranchCode,
                            ProductCode = stockItem.ProductCode,
                            LastChecked = tStamp
                        };
                    }
                    catch (Exception iex)
                    {
                        await _dbTrans.RollbackAsync();
                        UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                        throw;
                    }

                }
                
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<EtimsTransact, string>> ProcessSaveStockIO(EtimsTransact transactIO)
        {
            string _method_ = "ProcessSaveStockIO";
            string _strError = string.Empty;
            try
            {
                var _ioParts = transactIO.DocNumber.Split(":");
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL, RecordStatus.POST_FAIL };

                // Get IO Transaction
                var stockMovement = await _dbContext.StockMovement.Include(e => e.StockMovData)
                    .Where(e => e.BranchCode == _ioParts[0] && e.DocNumber == transactIO.DocNumber).OrderBy(e => e.CreatedOn)
                    .AsSplitQuery().AsNoTracking().FirstOrDefaultAsync();
                var etimsReqTwo = stockMovement.StockMovData.GetEtimsRequest();

                var eTimsRespIO = await _etimsService.SaveEtimsStockIO(etimsReqTwo);
                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var tStamp = DateTime.Now;
                        var recordStatus = RecordStatus.POST_FAIL;

                        if (eTimsRespIO.IsError)
                        {
                            recordStatus = RecordStatus.POST_FAIL;
                            _strError = eTimsRespIO.GetError();
                            UI.Error($"Saving StockMovement: {stockMovement.CacheKey} failed: {eTimsRespIO.GetError()}");

                            // Update & Save Transact Changes
                            await _dbContext.EtimsTransacts.Where(x => x.DocNumber == transactIO.DocNumber && x.ReqType == transactIO.ReqType
                                && x.BranchCode == transactIO.BranchCode && x.DocStamp == transactIO.DocStamp).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );
                            await _dbContext.StockMovData.Where(e => e.MovementID == stockMovement.MovementID).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, _strError)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );
                            await _dbContext.StockMovement.Where(e => e.MovementID == stockMovement.MovementID)
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

                        StockIOSaveResp stockIOSaveResp = eTimsRespIO.GetValue();
                        if (stockIOSaveResp.IsSuccess)
                            recordStatus = RecordStatus.POST_OK;

                        await _dbContext.EtimsTransacts.Where(x => x.DocNumber == transactIO.DocNumber && x.ReqType == transactIO.ReqType
                            && x.BranchCode == transactIO.BranchCode && x.DocStamp == transactIO.DocStamp).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        await _dbContext.StockMovData.Where(e => e.MovementID == stockMovement.MovementID).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ResponsePayload, stockIOSaveResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        await _dbContext.StockMovement.Where(e => e.MovementID == stockMovement.MovementID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, stockIOSaveResp.ResultMsg)
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

                return transactIO;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public Task<Result<List<string>, string>> SyncTaxProducts()
        {
            throw new NotImplementedException();
        }

        public Task<Result<int, string>> ProcessItemCallback(ItemCallback itemCallback)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<EtimsTransact>, string>> PostPendingProducts()
        {
            throw new NotImplementedException();
        }
    }
}
