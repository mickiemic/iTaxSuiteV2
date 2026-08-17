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
    public abstract class S300BaseProductSvc
    {
        protected readonly IDatabase _baseDb;
        protected readonly IHttpClientFactory _httpClientFactory;

        protected readonly ETimsDBContext _dbContext;
        protected readonly ExtSystConfig _extSystConfig;
        protected readonly IMasterDataSvc _masterDataSvc;

        protected ClientBranch _clientBranch = null;
        protected Dictionary<string, SyncChannel> _syncChannelMap;

        protected S300BaseProductSvc(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, ExtSystConfig extSystConfig,
            IMasterDataSvc masterDataSvc, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _baseDb = multiplexer.GetDatabase();
            _extSystConfig = extSystConfig;
            _masterDataSvc = masterDataSvc;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<PagedResult<Product>, string>> GetProducts(ProductFilter filter)
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
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_OK);
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

        public async Task<Result<PagedResult<StockItem>, string>> GetStockItems(StockFilter filter)
        {
            string _method_ = "GetStockItems";
            PagedResult<StockItem> result = new();
            try
            {
                var query = _dbContext.StockItems.Include(e => e.Product).AsSplitQuery().AsQueryable();
                if (filter != null && filter.RecordGroup != RecordStatusGroup.ALL)
                {
                    if (filter.RecordGroup == RecordStatusGroup.FAILED)
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_FAIL ||
                            f.RecordStatus == RecordStatus.INVALID);
                    else if (filter.RecordGroup == RecordStatusGroup.SUCCESSFUL)
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_OK);
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
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_OK);
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

        public async Task<Result<Product, string>> ReFetchProduct(ProductKey productKey)
        {
            string _method_ = "ReFetchICProduct";
            Product product = null;
            string _strError = string.Empty;
            int changes = 0;
            try
            {
                if (productKey == null || string.IsNullOrWhiteSpace(productKey.ProductCode))
                {
                    _strError = $"Invalid filter for Items => {JsonConvert.SerializeObject(productKey)}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var client = _httpClientFactory.CreateClient();

                product = await _dbContext.Products.Include(p => p.ProductData).AsSplitQuery()
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

                var defAuthResult = await _masterDataSvc.GetDefaultAuthority();
                if (defAuthResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                var defTaxAuth = defAuthResult.GetValue();

                Product _newProduct = null;
                StockItem _newStockItem = null;
                ProductData _newProductData = null;

                if (product.SourceApp == "IC")
                {
                    string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/IC/ICItems");
                    var qParams = new Dictionary<string, string>();
                    qParams["$filter"] = $"ItemNumber eq '{product.ProdRawCode}'";

                    var result = await client.ProcessGetReqBasicAsync<ICItems>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);
                    if (result == null && result.Items.Count == 0)
                    {
                        _strError = $"Not Found ICItems response from Sage for ProductCode {product.ProductCode}";
                        UI.Error($"{_method_} error : {_strError}");
                        return _strError;
                    }

                    var icItem = result.Items.FirstOrDefault(i => i.ItemNumber == product.ProdRawCode);
                    if (icItem == null)
                    {
                        _strError = $"Missing ICItem response from Sage for ProductCode {product.ProductCode}";
                        UI.Error($"{_method_} error : {_strError}");
                        return _strError;
                    }

                    _newProduct = new Product(icItem);
                    _newStockItem = new StockItem(_newProduct, _clientBranch);
                    _newProductData = new ProductData(_clientBranch, _newStockItem, icItem);
                    /*if (!_newProductData.SourceStamp.HasValue || productData.SourceStamp.Value >= _newProductData.SourceStamp.Value)*/
                    if (productData.HasEqualValue(_newProductData))
                    {
                        _strError = $"No Updates in Sage for ProductCode {product.ProductCode}";
                        UI.Info($"{_method_} error : {_strError}");
                        return product;
                    }
                }
                else if (product.SourceApp == "AR")
                {
                    string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/AR/ARItems");
                    var qParams = new Dictionary<string, string>();
                    qParams["$filter"] = $"ItemNumber eq '{product.ProdRawCode}'";

                    var result = await client.ProcessGetReqBasicAsync<ARItems>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);
                    if (result == null && result.Items.Count == 0)
                    {
                        _strError = $"Not Found ARItems response from Sage for ProductCode {product.ProductCode}";
                        UI.Error($"{_method_} error : {_strError}");
                        return _strError;
                    }

                    var arItem = result.Items.FirstOrDefault(i => i.ItemNumber == product.ProdRawCode);
                    if (arItem == null)
                    {
                        _strError = $"Missing ARItem response from Sage for ProductCode {product.ProductCode}";
                        UI.Error($"{_method_} error : {_strError}");
                        return _strError;
                    }

                    _newProduct = new Product(arItem);
                    _newStockItem = new StockItem(_newProduct, _clientBranch);
                    _newProductData = new ProductData(_clientBranch, _newStockItem, arItem, defTaxAuth);
                    //if ((!_newProductData.SourceStamp.HasValue || productData.SourceStamp.Value >= _newProductData.SourceStamp.Value)
                    if (productData.HasEqualValue(_newProductData))
                    {
                        _strError = $"No Updates in Sage for ProductCode {product.ProductCode}";
                        UI.Info($"{_method_} error : {_strError}");
                        return product;
                    }

                }
                else if (product.SourceApp == "GL")
                {
                    string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/GL/GLAccountGroups");
                    var qParams = new Dictionary<string, string>();
                    qParams["$filter"] = "GroupCategory eq 'Revenue' or GroupCategory eq 'OtherRevenue'";
                    var accGroups = await client.ProcessGetReqBasicAsync<GLAccountGroups>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                        null, qParams);
                    if (accGroups == null || accGroups.AccountGroups.Count != 2)
                    {
                        throw new Exception($"Null/Invalid GLAccountGroups response from Sage, count: {accGroups?.AccountGroups?.Count}");
                    }
                    var groupCodes = new HashSet<string>();
                    accGroups.AccountGroups.ForEach(x => groupCodes.Add(x.AccountGroupCode.ToString()));

                    _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/GL/GLAccounts");
                    var strFilter = "(AccountGroupCode eq '" + string.Join("' or AccountGroupCode eq '", groupCodes)
                        + "') and (AccountNumber eq '" + product.ProdRawCode + "')";
                    qParams["$filter"] = strFilter;

                    var result = await client.ProcessGetReqBasicAsync<GLAccounts>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);
                    if (result == null && result.Accounts.Count == 0)
                    {
                        _strError = $"Not Found GLAccounts response from Sage for ProductCode {product.ProductCode}";
                        UI.Error($"{_method_} error : {_strError}");
                        return _strError;
                    }

                    var account = result.Accounts.FirstOrDefault(i => i.AccountNumber == product.ProdRawCode);
                    if (account == null)
                    {
                        _strError = $"Missing GLAccount response from Sage for ProductCode {product.ProductCode}";
                        UI.Error($"{_method_} error : {_strError}");
                        return _strError;
                    }

                    _newProduct = new Product(account);
                    _newStockItem = new StockItem(_newProduct, _clientBranch);
                    _newProductData = new ProductData(_clientBranch, _newStockItem, account);

                    // SourceStamp == DateCreated not reflective of changes
                    if (productData.HasEqualValue(_newProductData))
                    {
                        _strError = $"No Updates in Sage for ProductCode {product.ProductCode}";
                        UI.Info($"{_method_} error : {_strError}");
                        return product;
                    }

                }
                else
                {
                    _strError = $"Invalid or missing SourceApp {product.SourceApp} for {productKey.ProductCode}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                product.UpdateByProduct(_newProduct);
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
                
                product.ProductData.UpdateByData(_clientBranch, _newProductData, product);
                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    var tStamp = DateTime.Now;
                    try
                    {
                        changes += await _dbContext.Products.Where(e => e.ProductCode == product.ProductCode)
                                    .ExecuteUpdateAsync(x => x
                                    .SetProperty(x => x.ItemClassCode, _newProduct.ItemClassCode)
                                    .SetProperty(x => x.UpdatedOn, tStamp)
                                    .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                                );
                        changes += await _dbContext.ProductData.Where(e => e.ProductCode == product.ProductCode)
                                    .ExecuteUpdateAsync(x => x
                                    .SetProperty(x => x.SourcePayload, product.ProductData.SourcePayload)
                                    .SetProperty(x => x.RequestPayload, product.ProductData.RequestPayload)
                                    .SetProperty(x => x.UpdatedOn, tStamp)
                                    .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                                );


                        await _dbTrans.CommitAsync();
                        UI.Info($"{_method_} update {changes} records updated.");
                    }
                    catch (Exception iex)
                    {
                        await _dbTrans.RollbackAsync();
                        UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }

            return product;

        }

    }
}
