using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace iTaxSuite.Library.Models.Entities
{
    [Table("PurchTransact")]
    public class PurchTransact : BaseTransact
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PurchaseID { get; set; }
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string BranchCode { get; set; }
        [Required]
        [StringLength(64)]
        public string DocNumber { get; set; }
        [Required]
        [NotMinValue]
        public DateTime DocStamp { get; set; }
        [StringLength(128)]
        public string Description { get; set; }
        [StringLength(3)]
        public string SourceApp { get; set; }
        public int EtrSeqNumber { get; set; }
        [StringLength(32)]
        public string VendorNumber { get; set; }
        [StringLength(100)]
        public string VendorName { get; set; }
        [StringLength(32)]
        public string VendorTaxNumber { get; set; }
        [StringLength(64)]
        public string Reference { get; set; }
        // *** Doc Amounts ***/
        [StringLength(3)]
        public string DocSrcCurr { get; set; }
        [StringLength(3)]
        public string DocHomeCurr { get; set; }
        [Precision(19, 3)]
        public decimal DocExchRate { get; set; }
        [Required]
        [NotMinValue]
        public DateTime DocRateDate { get; set; }
        // *** Doc Amounts ***/
        [Precision(19, 3)]
        public decimal SrcTBaseAmt { get; set; }
        [Precision(19, 3)]
        public decimal HomeTBaseAmt { get; set; }
        [Precision(19, 3)]
        public decimal SrcDiscAmt { get; set; }
        [Precision(19, 3)]
        public decimal HomeDiscAmt { get; set; }
        [Precision(19, 3)]
        public decimal SrcTotTaxAmt { get; set; }
        [Precision(19, 3)]
        public decimal HomeTotTaxAmt { get; set; }
        [Precision(19, 3)]
        public decimal SrcTotAmtWTax { get; set; }
        [Precision(19, 3)]
        public decimal HomeTotAmtWTax { get; set; }
        // *** EOF Doc Amounts ***/
        [StringLength(32)]
        public string Location { get; set; }
        public PurchTrxData? PurchTrxData { get; set; }
        public virtual ClientBranch ClientBranch { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public override string CacheKey => $"{BranchCode}:{DocNumber}";

        public PurchTransact()
        {
        }
        /*public PurchTransact(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.AP.WebApi.Models.Vendor vendor, Sage.CA.SBS.ERP.Sage300.PO.WebApi.Models.Invoice invoice)
            : this()
        {
            BranchCode = clientBranch.BranchCode;
            DocNumber = invoice.InvoiceNumber;
            EtrSeqNumber = clientBranch.PurchInvoiceSeq;
            DocStamp = invoice.InvoiceDate.Value;
            Description = invoice.Description;
            VendorNumber = vendor.VendorNumber;
            VendorName = vendor.VendorName;
            Reference = invoice.Reference;
            Location = invoice.ShipToLocation;
            CreatedBy = "Sys-Admin";

            DocSrcCurr = DocHomeCurr = clientBranch.TaxClient.Currency;
            DocExchRate = 1;
            DocRateDate = DateTime.Today;

            SrcTotAmtWTax = invoice.Total;
            HomeTotAmtWTax = invoice.Total * DocExchRate;

        }*/

        public PurchTransact(ClientBranch clientBranch, PurchaseSale purchaseSale)
            : this()
        {
            BranchCode = clientBranch.BranchCode;
            DocNumber = $"{clientBranch.BranchCode}:{clientBranch.PurchInvoiceSeq}";
            EtrSeqNumber = clientBranch.PurchInvoiceSeq;
            SourceApp = "KRA";

            DateTime documentDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(purchaseSale.StockReleaseDate))
                documentDate = DateTime.ParseExact(purchaseSale.StockReleaseDate, ETIMSConst.STRUCT_DATETIME, CultureInfo.InvariantCulture);
            else if (!string.IsNullOrWhiteSpace(purchaseSale.SalesDate))
                documentDate = DateTime.ParseExact(purchaseSale.SalesDate, ETIMSConst.FMT_DATEONLY, CultureInfo.InvariantCulture);
            DocStamp = documentDate;

            Description = purchaseSale.Remark;
            VendorNumber = purchaseSale.SupplierPIN;
            VendorName = purchaseSale.SupplierName;
            VendorTaxNumber = purchaseSale.SupplierPIN;
            Reference = purchaseSale.Reference;
            CreatedBy = "Sys-Admin";

            DocSrcCurr = DocHomeCurr = clientBranch.TaxClient.Currency;
            DocExchRate = 1;
            DocRateDate = DateTime.Today;

            SrcTBaseAmt = purchaseSale.TotalTaxableAmount;
            HomeTBaseAmt = purchaseSale.TotalTaxableAmount * DocExchRate;
            SrcTotTaxAmt = purchaseSale.TotalTaxAmount;
            HomeTotTaxAmt = purchaseSale.TotalTaxAmount * DocExchRate;
            SrcTotAmtWTax = purchaseSale.TotalAmount;
            HomeTotAmtWTax = purchaseSale.TotalAmount * DocExchRate;
        }

        public bool IsValid()
        {
            return true;
        }
    }
    [Table("PurchTrxData")]
    public class PurchTrxData : BaseDataEntity
    {
        [Key]
        [Required]
        public int PurchaseID { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public PurchTransact PurchTransact { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public TrnsPurchaseSaveReq TrnsPurchaseSaveReq { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public TrnsPurchaseSaveResp TrnsPurchaseSaveResp { get; set; }
        public PurchTrxData()
        {
        }
        public PurchTrxData(PurchTransact purchase, TrnsPurchaseSaveReq purchaseSaveReq, Sage.CA.SBS.ERP.Sage300.PO.WebApi.Models.Invoice invoice)
            : this()
        {
            SourceStamp = invoice.InvoiceDate.Value;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(invoice);
            TrnsPurchaseSaveReq = purchaseSaveReq;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(purchaseSaveReq, new DecimalFormatConverter());
        }

        public PurchTrxData(PurchTransact purchTransact, TrnsPurchaseSaveReq purchaseSaveReq, PurchaseSale purchaseSale)
        {
            SourceStamp = purchTransact.DocStamp;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(purchaseSale);
            TrnsPurchaseSaveReq = purchaseSaveReq;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(purchaseSaveReq, new DecimalFormatConverter());
        }

        public TrnsPurchaseSaveReq GetEtimsRequest()
        {
            if (string.IsNullOrWhiteSpace(RequestPayload))
                return null;
            TrnsPurchaseSaveReq = Newtonsoft.Json.JsonConvert.DeserializeObject<TrnsPurchaseSaveReq>(RequestPayload);
            return TrnsPurchaseSaveReq;
        }

    }

    public class PurchaseFilter : APDatedFilter
    {
        public bool IsValid => true;
        public RecordStatusGroup RecordGroup { get; set; } = RecordStatusGroup.ALL;
        [StringLength(32)]
        public string DocNumber { get; set; }
    }

}
