using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Interfaces;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace iTaxSuite.Library.Services
{
    public class SX3DTaxProductSvc : IS300ProductSvc
    {
        protected readonly IDatabase _baseDb;
        protected readonly IHttpClientFactory _httpClientFactory;

        protected readonly ETimsDBContext _dbContext;
        protected readonly ExtSystConfig _extSystConfig;
        protected readonly IMasterDataSvc _masterDataSvc;
        private readonly IDigiTaxService _dTaxService;

        protected ClientBranch _clientBranch = null;
        protected Dictionary<string, SyncChannel> _syncChannelMap;

        public SX3DTaxProductSvc(IConnectionMultiplexer multiplexer, IHttpClientFactory httpClientFactory, ETimsDBContext dbContext, ExtSystConfig extSystConfig, IMasterDataSvc masterDataSvc, IDigiTaxService dTaxService)
        {
            _baseDb = multiplexer.GetDatabase();
            _httpClientFactory = httpClientFactory;
            _dbContext = dbContext;
            _extSystConfig = extSystConfig;
            _masterDataSvc = masterDataSvc;
            _dTaxService = dTaxService;

            _syncChannelMap = _masterDataSvc.GetChannelsAsync().GetAwaiter().GetResult();
            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();
        }

        public Task<Result<StockMovement, string>> CreateStockMovement(StockIORequest stockIORequest)
        {
            throw new NotImplementedException();
        }

        public Task<List<Product>> FetchARProducts()
        {
            throw new NotImplementedException();
        }

        public Task<List<Product>> FetchGLProducts()
        {
            throw new NotImplementedException();
        }

        public Task<List<Product>> FetchICProducts()
        {
            throw new NotImplementedException();
        }

        public Task<Result<HashSet<string>, string>> FetchSpecificProducts(SalesTransact salesTransact, HashSet<string> productList)
        {
            throw new NotImplementedException();
        }

        public TaxDeviceType GetDeviceType()
        {
            return TaxDeviceType.DIGITAX;
        }

        public Task<Result<PagedResult<Product>, string>> GetProducts(ProductFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<Result<PagedResult<StockItem>, string>> GetStockItems(StockFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<Result<PagedResult<StockMovement>, string>> GetStockMovements(MovementFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<EtimsTransact>, string>> PostPendingProducts()
        {
            throw new NotImplementedException();
        }

        public Task<Result<int, string>> ProcessItemCallback(ItemCallback itemCallback)
        {
            throw new NotImplementedException();
        }

        public Task<Result<EtimsTransact, string>> ProcessSaveProduct(EtimsTransact transact)
        {
            throw new NotImplementedException();
        }

        public Task<Result<EtimsTransact, string>> ProcessSaveStockIO(EtimsTransact transact)
        {
            throw new NotImplementedException();
        }

        public Task<Result<EtimsTransact, string>> QueueSaveProduct(BranchStockKey filter)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<Product, string>> ReFetchProduct(ProductKey productKey)
        {
            string _method_ = "ReFetchICProduct";
            Product product = null;
            string _strError = string.Empty;
            try
            {
                if (productKey == null || string.IsNullOrWhiteSpace(productKey.ProductCode))
                {
                    _strError = $"Invalid filter for Items => {JsonConvert.SerializeObject(productKey)}";
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

                var client = _httpClientFactory.CreateClient();

                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/ITMMASTER");
                var qParams = new Dictionary<string, string>();
                qParams["representation"] = "ITMMASTER.$query";
                qParams["where"] = $"ITMREF eq '{productKey.ProductCode}'";
                var result = await client.ProcessGetReqBasicAsync<SageX3Items>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);
                if (result == null && result.Items.Count == 0)
                {
                    if (result == null && result.Items.Count == 0)
                    {
                        _strError = $"Not Found ITMMASTER response from Sage for ProductCode {product.ProductCode}";
                        UI.Error($"{_method_} error : {_strError}");
                        return _strError;
                    }
                }

                var x3Item = result.Items[0];
                product = new Product(x3Item);
                var stockItem = new StockItem(product, _clientBranch);
                var productData= new ProductData(_clientBranch, stockItem, x3Item, defTaxAuth);
                UI.Info($"{_method_} : Fetched ITMMASTER from Sage for ProductCode {product.ProductCode} => {result.Items.Count}");
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }

            return product;
        }

        public Task<Result<BranchStockLevel, string>> SaveStockLevel(SaveStockLevel saveStockLevel)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<string>, string>> SyncTaxProducts()
        {
            throw new NotImplementedException();
        }
    }
}
