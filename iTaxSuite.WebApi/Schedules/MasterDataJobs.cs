using Hangfire;
using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Services;

namespace iTaxSuite.WebApi.Schedules
{
    public class ProductsFetcher
    {
        public const string JobId = $"job-{CacheConst.PRODUCT_HASHKEY}";
        private readonly IS300ProductSvc _productSvc;

        public ProductsFetcher(IS300ProductSvc productSvc)
        {
            _productSvc = productSvc;
        }

        [JobDisplayName(JobId + "-executeasync")]
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string _method_ = $"{JobId}:ExecuteAsync";
            if (!stoppingToken.IsCancellationRequested)
            {
                var products = await _productSvc.FetchProducts();
                if (products != null && products.Items.Count > 0)
                    UI.Info($"{_method_} products to process count:{products.Items.Count}");
            }
        }
    }

    /*public class PurchasesFetcher
    {
        public const string JobId = $"job-{CacheConst.TXPURCHASE_HASHKEY}";
        private readonly IS300PurchaseSvc _purchaseSvc;

        public PurchasesFetcher(IS300PurchaseSvc purchaseSvc)
        {
            _purchaseSvc = purchaseSvc;
        }

        [JobDisplayName(JobId + "-executeasync")]
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string _method_ = $"{JobId}:ExecuteAsync";
            if (!stoppingToken.IsCancellationRequested)
            {
                var pResult = await _purchaseSvc.FetchPOInvoices();
                if (pResult.IsSuccess)
                {
                    var purchases = pResult.GetValue();
                    if (purchases != null && purchases.Invoices.Count > 0)
                        UI.Info($"{_method_} POInvoices to process count:{purchases.Invoices.Count}");
                }
                else
                {
                    throw new Exception($"{_method_} FetchPOInvoices error:{pResult.GetError()}");
                }
            }
        }
    }*/

    public class OESalesFetcher
    {
        public const string JobId = $"job-{CacheConst.TXSALES_HASHKEY}";
        private readonly IS300SaleService _saleSvc;

        public OESalesFetcher(IS300SaleService saleSvc)
        {
            _saleSvc = saleSvc;
        }

        [JobDisplayName(JobId + "-executeasync")]
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string _method_ = $"{JobId}:ExecuteAsync";
            if (!stoppingToken.IsCancellationRequested)
            {
                var pResult = await _saleSvc.FetchOEInvoices();
                if (pResult.IsSuccess)
                {
                    var oeSales = pResult.GetValue();
                    if (oeSales != null && oeSales.Invoices.Count > 0)
                        UI.Info($"{_method_} OEInvoices to process count:{oeSales.Invoices.Count}");
                }
                else
                {
                    throw new Exception($"{_method_} FetchOEInvoices error:{pResult.GetError()}");
                }
            }
        }
    }

    public class PurchasesFetcher
    {
        public const string JobId = $"job-{CacheConst.TXPURCHASE_HASHKEY}";
        private readonly IS300PurchaseSvc _purchaseSvc;

        public PurchasesFetcher(IS300PurchaseSvc purchaseSvc)
        {
            _purchaseSvc = purchaseSvc;
        }

        [JobDisplayName(JobId + "-executeasync")]
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string _method_ = $"{JobId}:ExecuteAsync";
            if (!stoppingToken.IsCancellationRequested)
            {
                var pResult = await _purchaseSvc.FetchETRInvoices();
                if (pResult.IsSuccess)
                {
                    var purchaseWrapper = pResult.GetValue();
                    if (purchaseWrapper != null && purchaseWrapper.PurchSalesList.Count > 0)
                        UI.Info($"{_method_} ETR Purchases list to process count:{purchaseWrapper.PurchSalesList.Count}");
                }
                else
                {
                    throw new Exception($"{_method_} FetchETRInvoices error:{pResult.GetError()}");
                }
            }
        }
    }

}
