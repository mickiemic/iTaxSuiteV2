using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace iTaxSuite.WinForms.Clients
{
    public partial class ETIMSClient
    {
        private async Task GenSalesItems()
        {
            string _method_ = "GenSalesItems";
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            try
            {
                string docNumber = string.Empty;// "IN000001";
                var salesTransacts = await _dbContext.SalesTransact
                    .Include(x => x.SalesTrxData).Include(x => x.SalesItems)
                    .Where(x => (x.DocNumber == docNumber || string.IsNullOrWhiteSpace(docNumber))).ToListAsync();
                if (salesTransacts == null || salesTransacts.Count == 0)
                {
                    return;
                }

                var gResult = await _masterDataSvc.GetTaxGroups();
                if (gResult.IsError)
                {
                    throw new Exception("Invalid TaxGroup Cache Setup");
                }
                taxGroupMap = gResult.GetValue();

                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    throw new Exception("Invalid TaxAuth Cache Setup");
                }
                taxAuthKeys = authResult.GetValue();

                foreach (var saleTrx in salesTransacts.Where(x => x.SalesItems?.Count == 0).ToList())
                {
                    UI.Info($"Generating SalesItems for: {saleTrx.DocNumber}");
                    var invoice = JsonConvert.DeserializeObject<Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice>(saleTrx.SalesTrxData.SourcePayload);

                    string strTaxKey = $"{invoice.TaxGroup}:{invoice.TaxReportingTRCurrency}:Sales";
                    if (!taxGroupMap.ContainsKey(strTaxKey))
                    {
                        throw new Exception($"Tax Setup Missing GroupKey {strTaxKey}");
                    }
                    var _taxGroup = taxGroupMap[strTaxKey];

                    List<SalesItem> salesItems = new();
                    foreach (var line in invoice.InvoiceDetails)
                    {
                        var salesItem = new SalesItem(saleTrx, line, _taxGroup, taxAuthKeys);
                        salesItems.Add(salesItem);
                    }
                    saleTrx.SalesItems.AddRange(salesItems);

                    var mapResult = await _masterDataSvc.MapSalesInvcAttribs(saleTrx);
                    if (mapResult.IsError)
                    {
                        throw new Exception($"{_method_} MapSalesInvcAttribs error : {mapResult.GetError()}");
                    }
                    saleTrx.SalesItems = mapResult.GetValue().SalesItems;
                    int changes = await _dbContext.SaveChangesAsync();
                    UI.Info($"{_method_} DocNumber:{saleTrx.DocNumber}, changes: {changes}");
                }
            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "OE Invoice Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SyncLocalProducts(SelectItemResp selectItemResp)
        {
            string _method_ = "SyncLocalProducts";
            try
            {
                if (selectItemResp is null || selectItemResp?.Data?.ItemList?.Count == 0)
                {
                    return;
                }
                var okStatii = new List<RecordStatus>() { };// RecordStatus.POST_OK, RecordStatus.POST_DUPL };

                var dbItemMap = await _dbContext.StockItems.Include(e => e.Product).Include(e => e.Product.ProductData)
                    .Where(e => !okStatii.Contains(e.RecordStatus)).OrderBy(e => e.CreatedOn)
                    .AsNoTracking().ToDictionaryAsync(x => new { x.ProductCode, x });
                if (dbItemMap is null || dbItemMap.Count == 0)
                    return;

                var kraItemMap = new Dictionary<string, ETimsItem>();
                selectItemResp.Data.ItemList.ForEach(x => kraItemMap.Add(x.AdditionalInfo, x));
                foreach (var itemKey in dbItemMap.Keys.Where(x => kraItemMap.ContainsKey(x.ProductCode)))
                {
                    UI.Info($"ItemCode: {itemKey} can be fixed locally");
                }

            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "OE Invoice Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }

    class TestData
    {
        public string ICItem { get; set; }
        public string OEInvoice { get; set; }
        public string OECreditNote { get; set; }
        public string ARBatch { get; set; }
        public string ARInvoice { get; set; }
        public string ARCreditNote { get; set; }
        public string POReceipt { get; set; }
    }
}
