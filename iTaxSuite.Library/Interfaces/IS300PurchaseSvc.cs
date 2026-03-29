using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;

namespace iTaxSuite.Library.Interfaces
{
    public interface IS300PurchaseSvc
    {
        TaxDeviceType GetDeviceType();
        Task<Result<PurchSalesWrapper, string>> FetchETRInvoices();
        Task<Result<Sage.CA.SBS.ERP.Sage300.AP.WebApi.Models.Vendor, string>> GetAPVendor(SageDocFilter sageFilter);
        Task<Result<PagedResult<PurchTransact>, string>> GetPurchases(PurchaseFilter filter);
        Task<Result<PurchTransact, string>> AcceptPurchaseTrx(PurchTransact purchTransact);
        Task<Result<TrnsPurchaseSalesResp, string>> ImportPurchases(DateTime? filter);
    }
}
