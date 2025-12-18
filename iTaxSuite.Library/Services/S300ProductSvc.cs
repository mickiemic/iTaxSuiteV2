using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace iTaxSuite.Library.Services
{
    public interface IS300ProductSvc
    {
        Task<Result<StockMovement, string>> CreateStockMovement(StockIORequest stockIORequest);
        Task<ICItems> FetchProducts();
        Task<Result<PagedResult<Product>, string>> GetProducts(ProductFilter filter);
        Task<Result<PagedResult<StockItem>, string>> GetStockItems(StockFilter filter);
        Task<Result<PagedResult<StockMovement>, string>> GetStockMovements(MovementFilter filter);
        Task<Result<EtimsTransact, string>> ProcessSaveProduct(EtimsTransact transact);
        Task<Result<EtimsTransact, string>> ProcessSaveStockIO(EtimsTransact transact);
        Task<Result<EtimsTransact, string>> QueueSaveProduct(BranchStockKey filter);
        Task<Result<Product, string>> ReFetchProduct(ProductKey productKey);
        Task<Result<BranchStockLevel, string>> SaveStockLevel(SaveStockLevel saveStockLevel);
    }
    public class S300ProductSvc : IS300ProductSvc
    {
        private readonly ETimsDBContext _dbContext;
        private readonly IDatabase _baseDb;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ExtSystConfig _extSystConfig;
        private readonly IMasterDataSvc _masterDataSvc;
        private readonly IEtimsService _etimsService;

        private readonly Dictionary<string, SyncChannel> _syncChannelMap;
        private readonly ClientBranch _clientBranch = null;

        public S300ProductSvc(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, ExtSystConfig extSystConfig, 
            IMasterDataSvc masterDataSvc, IHttpClientFactory httpClientFactory, IEtimsService etimsService)
        {
            _dbContext = dbContext;
            _baseDb = multiplexer.GetDatabase();
            _extSystConfig = extSystConfig;
            _masterDataSvc = masterDataSvc;

            _syncChannelMap = _masterDataSvc.GetChannelsAsync().GetAwaiter().GetResult();
            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();
            _httpClientFactory = httpClientFactory;
            _etimsService = etimsService;
        }

        public async Task<Dictionary<string,StockItemKey>> GetCacheStockItems(bool reload = false)
        {
            string _method_ = "GetCacheStockItems";
            var productMap = new Dictionary<string, StockItemKey>();
            try
            {
                string _hashKey_ = CacheConst.PRODUCT_HASHKEY;

                if (!reload)
                {
                    var result = await _baseDb.HashGetAllAsync(CacheConst.PRODUCT_HASHKEY);
                    foreach (var item in result)
                    {
                        var product = JsonConvert.DeserializeObject<StockItemKey>(item.Value);
                        productMap[item.Name] = product;
                    }
                }

                if (reload || !productMap.Any())
                {
                    if (_baseDb.KeyExists(_hashKey_))
                    {
                        _baseDb.KeyDelete(_hashKey_);
                    }
                    
                    foreach (var item in _dbContext.StockItems.Include(e => e.Product).Select(x => new StockItemKey(x)).AsNoTracking())
                    {
                        productMap[item.ProductCode] = item;
                        if (!_baseDb.SetHashValue(_hashKey_, item.ProductCode, item))
                        {
                            UI.Error($"RCacheService::{_method_} failed for Product code:{_hashKey_}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                throw;
            }

            return productMap;
        }

        public async Task<Result<PagedResult<Product>,string>> GetProducts(ProductFilter filter)
        {
            string _method_ = "GetProducts";
            PagedResult<Product> result = new();
            try
            {
                var query = _dbContext.Products.AsNoTracking().AsQueryable();
                if (filter != null && filter.RecordGroup != RecordStatusGroup.ALL)
                {
                    if (filter.RecordGroup == RecordStatusGroup.FAILED)
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_FAIL ||
                            f.RecordStatus == RecordStatus.INVALID);
                    else if (filter.RecordGroup == RecordStatusGroup.SUCCESSFUL)
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_OK ||
                            f.RecordStatus == RecordStatus.POST_DUPL);
                    else if (filter.RecordGroup == RecordStatusGroup.QUEUED)
                        query = query.Where(f => f.RecordStatus == RecordStatus.QUEUEDOUT ||
                            f.RecordStatus == RecordStatus.MANUALOUT);
                }

                if (!string.IsNullOrWhiteSpace(filter.ProductCode))
                    query = query.Where(x => x.ProductCode.Equals(filter.ProductCode));
                if (!string.IsNullOrWhiteSpace(filter.Description))
                    query = query.Where(x => x.Description.Contains(filter.Description));

                result.Count = await query.CountAsync();
                if (filter.Sort != null)
                    query = filter.PageAndOrder(query);
                else
                    query = filter.PageAndOrderByStamp(query);

                result.Result = await query.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<PagedResult<StockItem>,string>> GetStockItems(StockFilter filter)
        {
            string _method_ = "GetStockItems";
            PagedResult<StockItem> result = new();
            try
            {
                var query = _dbContext.StockItems.Include(e => e.Product).AsQueryable();
                if (filter != null && filter.RecordGroup != RecordStatusGroup.ALL)
                {
                    if (filter.RecordGroup == RecordStatusGroup.FAILED)
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_FAIL ||
                            f.RecordStatus == RecordStatus.INVALID);
                    else if (filter.RecordGroup == RecordStatusGroup.SUCCESSFUL)
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_OK ||
                            f.RecordStatus == RecordStatus.POST_DUPL);
                    else if (filter.RecordGroup == RecordStatusGroup.QUEUED)
                        query = query.Where(f => f.RecordStatus == RecordStatus.QUEUEDOUT ||
                            f.RecordStatus == RecordStatus.MANUALOUT);
                }

                if (!string.IsNullOrWhiteSpace(filter.ProductCode))
                    query = query.Where(x => x.ProductCode.Equals(filter.ProductCode));
                if (!string.IsNullOrWhiteSpace(filter.BranchCode))
                    query = query.Where(x => x.BranchCode.Equals(filter.BranchCode));
                if (!string.IsNullOrWhiteSpace(filter.TaxItemCode))
                    query = query.Where(x => x.TaxItemCode.Equals(filter.TaxItemCode));
                if (!string.IsNullOrWhiteSpace(filter.Description))
                    query = query.Where(x => x.Product.Description.Contains(filter.Description));

                result.Count = await query.CountAsync();
                if (filter.Sort != null)
                    query = filter.PageAndOrder(query);
                else
                    query = filter.PageAndOrderByStamp(query);

                result.Result = await query.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<PagedResult<StockMovement>, string>> GetStockMovements(MovementFilter filter)
        {
            string _method_ = "GetStockMovements";
            PagedResult<StockMovement> result = new();
            try
            {
                var query = _dbContext.StockMovement.AsQueryable();
                if (filter != null && filter.RecordGroup != RecordStatusGroup.ALL)
                {
                    if (filter.RecordGroup == RecordStatusGroup.FAILED)
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_FAIL ||
                            f.RecordStatus == RecordStatus.INVALID);
                    else if (filter.RecordGroup == RecordStatusGroup.SUCCESSFUL)
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_OK ||
                            f.RecordStatus == RecordStatus.POST_DUPL);
                    else if (filter.RecordGroup == RecordStatusGroup.QUEUED)
                        query = query.Where(f => f.RecordStatus == RecordStatus.QUEUEDOUT ||
                            f.RecordStatus == RecordStatus.MANUALOUT);
                }

                if (!string.IsNullOrWhiteSpace(filter.BranchCode))
                    query = query.Where(x => x.BranchCode.Equals(filter.BranchCode));
                if (!string.IsNullOrWhiteSpace(filter.DocNumber))
                    query = query.Where(x => x.DocNumber.Equals(filter.DocNumber));

                if (filter.HasAnyDate())
                {
                    string _dtFilterError = filter.GetDatesError();
                    if (!string.IsNullOrWhiteSpace(_dtFilterError))
                    {
                        return _dtFilterError;
                    }
                    query = query.Where(x => x.DocDate >= filter.StartTime.Value
                        && x.DocDate <= filter.EndTime.Value);
                }

                result.Count = await query.CountAsync();
                if (filter.Sort != null)
                    query = filter.PageAndOrder(query);
                else
                    query = filter.PageAndOrderByStamp(query);

                result.Result = await query.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<StockMovement, string>> CreateStockMovement(StockIORequest stockIORequest)
        {
            string _method_ = "CreateStockMovement";
            StockMovement stockMovement = null;
            string _strError = string.Empty;
            try
            {
                var stockItem = await _dbContext.StockItems.Include(e => e.Product)
                    .FirstOrDefaultAsync(x => x.BranchCode == _clientBranch.BranchCode 
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
        private async Task<bool> CacheSaveStockItem(string streamName, StockItem stockItem)
        {
            var res = await _baseDb.SetHashValueAsync(CacheConst.PRODUCT_HASHKEY, stockItem.CacheKey, new StockItemKey(stockItem));
            return res;
        }
        public async Task<ICItems> FetchProducts()
        {
            string _method_ = "FetchProducts";
            ICItems result = null;
            string _strError = string.Empty;
            try
            {
                var syncChannel = _syncChannelMap[GeneralConst.PRODUCT_SYNC];
                var productMap = await GetCacheStockItems();
                var qParams = new Dictionary<string, string>();
                
                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/IC/ICItems");

                bool loop = true;
                while (loop)
                {
                    qParams["$skip"] = syncChannel.OffSet.ToString();
                    result = await client.ProcessGetReqBasicAsync<ICItems>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                        null, qParams);
                    if (result == null)
                        throw new Exception("Null ICItems response from Sage");
                    loop = (result.nextLink != null);
                    syncChannel.IncrOffSet(result.Items.Count);

                    result.Items.RemoveAll(i => productMap.ContainsKey(i.ItemNumber));

                    foreach (var item in result.Items)
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
                                    throw new Exception($"Product {product.ProductCode} saving to database failed");
                                }

                                if (_dbContext.StockItems.AddIfNotExists(stockItem, x => x.ProductCode == stockItem.ProductCode
                                    && x.BranchCode == stockItem.BranchCode) == null)
                                {
                                    UI.Warn($"StockItem {stockItem.CacheKey} Already Exists");
                                    continue;
                                }
                                if (_dbContext.SaveChanges() < 1)
                                {
                                    throw new Exception($"StockItem {stockItem.CacheKey} saving to database failed");
                                }

                                if (!await CacheSaveStockItem(GeneralConst.PRODUCT_SYNC, stockItem))
                                {
                                    throw new Exception($"Product {product.ProductCode} saving to cache failed");
                                }

                                syncChannel.UpdateTracker(item.ItemNumber);
                                _clientBranch.ProductSeq = (_etrSeqValue + 1);
                                
                                await _dbTrans.CommitAsync();
                                _dbContext.ChangeTracker.Clear();
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

                    if (!await _masterDataSvc.SaveSyncChannel(syncChannel))
                    {
                        UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating ItemsSync");
                    }
                    if (!await _masterDataSvc.UpdateBranchAsync(_clientBranch))
                    {
                        UI.Error($"{_method_} - UpdateBranchAsync : Failed Updating ClientBranch Details");
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }

            return result;
        }

        public async Task<Result<Product, string>> ReFetchProduct(ProductKey productKey)
        {
            string _method_ = "ReFetchProduct";
            Product product = null;
            string _strError = string.Empty;
            try
            {
                if (productKey == null || string.IsNullOrWhiteSpace(productKey.ProductCode))
                {
                    _strError = $"Invalid filter for ICItem => {JsonConvert.SerializeObject(productKey)}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/IC/ICItems");

                product = await _dbContext.Products.Include(p => p.ProductData)
                    .FirstOrDefaultAsync(p => p.ProductCode == productKey.ProductCode);
                if (product is null)
                {
                    _strError = $"Invalid or missing Product {productKey.ProductCode} in Products data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                var productData = product.ProductData;
                if (productData is null)
                {
                    _strError = $"Invalid or missing ProductData {productKey.ProductCode} in ProductData data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = $"ItemNumber eq '{product.ProductCode}'";

                var result = await client.ProcessGetReqBasicAsync<ICItems>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);
                if (result == null && result.Items.Count == 0)
                {
                    _strError = $"Not Found ICItems response from Sage for ProductCode {product.ProductCode}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var icItem = result.Items.FirstOrDefault(i => i.ItemNumber == product.ProductCode);
                
                var _newProduct = new Product(icItem);
                var _newStockItem = new StockItem(product, _clientBranch);
                var _newProductData = new ProductData(_clientBranch, _newStockItem, icItem);
                if (!_newProductData.SourceStamp.HasValue || productData.SourceStamp.Value >= _newProductData.SourceStamp.Value)
                {
                    _strError = $"Not Updates in Sage for ProductCode {product.ProductCode}";
                    UI.Info($"{_method_} error : {_strError}");
                    return product;
                }

                product.ItemClassCode = _newProduct.ItemClassCode;
                product.RecordStatus = RecordStatus.QUEUEDOUT;
                product.UpdatedOn = DateTime.Now;
                product.UpdatedBy = "Sys-Admin";

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

                product.ProductData.SourceStamp = _newProductData.SourceStamp;
                product.ProductData.SourcePayload = _newProductData.SourcePayload;
                product.ProductData.SaveItemReq = _newProductData.SaveItemReq;
                product.ProductData.RequestPayload = _newProductData.RequestPayload;
                product.ProductData.UpdatedOn = DateTime.Now;
                product.ProductData.UpdatedBy = "Sys-Admin";

                int affected = await _dbContext.SaveChangesAsync();
                UI.Info($"{_method_} update {affected} records updated.");
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }

            return product;

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
                    .FirstOrDefaultAsync();
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
                    .AsNoTracking().FirstOrDefaultAsync();
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
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                            await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, _strError)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                            await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.Remark, _strError)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                            await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.Remark, _strError)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
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
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ResponsePayload, saveItemResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, saveItemResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, saveItemResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
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
                    .OrderBy(e => e.CreatedOn).AsNoTracking().FirstOrDefaultAsync();
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
                    .AsNoTracking().FirstOrDefaultAsync();
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
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                            await _dbContext.StockMovData.Where(e => e.MovementID == stockMovement.MovementID).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, _strError)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                            await _dbContext.StockMovement.Where(e => e.MovementID == stockMovement.MovementID)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.Remark, _strError)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
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
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.StockMovData.Where(e => e.MovementID == stockMovement.MovementID).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ResponsePayload, stockIOSaveResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.StockMovement.Where(e => e.MovementID == stockMovement.MovementID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, stockIOSaveResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
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

    }
}
