using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;

namespace iTaxSuite.Library.Interfaces
{
    public interface IS300SaleService
    {
        TaxDeviceType GetDeviceType();
        Task<Result<List<EtimsSalesView>, string>> FetchARCRNotes();
        Task<Result<List<EtimsSalesView>, string>> FetchARInvoices();
        Task<Result<List<EtimsSalesView>, string>> FetchOECRDRNotes();
        Task<Result<List<EtimsSalesView>, string>> FetchOEInvoices();
        Task<Result<Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer, string>> GetARCustomer(SageDocFilter sageFilter);
        Task<Result<EtimsSalesView, string>> GetConvertARCRNote(SaleBatchTrxKey saleBatchTrxKey);
        Task<Result<EtimsSalesView, string>> GetConvertARInvoice(SaleBatchTrxKey saleBatchTrxKey);
        Task<Result<EtimsSalesView, string>> GetConvertOECRNote(SaleTrxKey saleTrxKey);
        Task<Result<EtimsSalesView, string>> GetConvertOEInvoice(SaleTrxKey saleTrxKey);
        Task<Result<SalesTransact, string>> GetQRImage(int salesTrxId, bool updateMeta = false);
        Task<Result<PagedResult<SalesTransact>, string>> GetSales(SalesFilter filter);
        Task<Result<EtimsTransact, string>> ProcessSaveSale(EtimsTransact transactSale);
        Task<Result<SalesTransact, string>> QuerySaleTransact(SaleTrxKey saleTrxKey, bool fixTransaction = true);
        Task<Result<EtimsTransact, string>> QueueSaveSale(QueueSaveSale filter);
        Task<Result<SalesTransact, string>> ReFetchOEInvoice(SaleTrxKey saleTrxKey);
        Task<Result<int, string>> ProcessSaleCallback(SaleCallback saleCallback);
        Task<Result<int, string>> PostReadyTaxTrxs();
    }
}
