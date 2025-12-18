using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iTaxSuite.Library.Models.Entities
{
    [Table("SalesTransact")]
    public class SalesTransact : BaseTransact
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SalesTrxID { get; set; }
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string BranchCode { get; set; }
        [Required]
        [StringLength(64)]
        public string DocNumber { get; set; }
        [Required]
        [NotMinValue]
        public DateTime DocStamp { get; set; }
        [Required]
        public DocumentType DocType { get; set; }
        [Required]
        public ETIMSReqType ReqType { get; set; }
        [Required]
        [StringLength(3)]
        public string SourceApp { get; set; }
        public int EtrSeqNumber { get; set; }
        [StringLength(64)]
        public string CustNumber { get; set; }
        [StringLength(100)]
        public string CustName { get; set; }
        [StringLength(32)]
        public string CustTaxNumber { get; set; }
        [StringLength(128)]
        public string Description { get; set; }
        [StringLength(64)]
        public string RefInvNumber { get; set; }
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
        [StringLength(64)]
        public string CUNumber { get; set; }
        [StringLength(64)]
        public string RefCUNumber { get; set; }
        [StringLength(256)]
        public string QRText { get; set; }
        public DateTime? QRTime { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public byte[] QRImage { get; set; }
        [StringLength(64)]
        public string SDCID { get; set; }
        [StringLength(64)]
        public string InternalData { get; set; }
        [StringLength(64)]
        public int ReceiptNumber { get; set; }
        [StringLength(64)]
        public string ReceiptSignature { get; set; }
        [StringLength(1)]
        public string EtrTaxClass { get; set; }
        public SalesTrxData? SalesTrxData { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public override string CacheKey => $"{BranchCode}:{DocNumber}";
        public virtual ClientBranch ClientBranch { get; set; }

        public SalesTransact()
        {
        }
        public SalesTransact(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer, 
            Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice invoice,
            HashSet<string> taxAuthKeys)
            : this()
        {
            BranchCode = clientBranch.BranchCode;
            DocNumber = invoice.InvoiceNumber;
            DocType = DocumentType.INVOICE;
            ReqType = ETIMSReqType.SAVE_SALE;
            DocStamp = invoice.InvoiceDate.Value;
            SourceApp = "OE";
            EtrSeqNumber = clientBranch.SaleInvoiceSeq;

            CustNumber = invoice.CustomerNumber;
            CustName = invoice.BillTo;
            if (customer != null)
            {
                CustName = customer.CustomerName.Trim();
                CustTaxNumber = customer.TaxRegistrationNumber1.Trim();
            }

            DocSrcCurr = invoice.InvoiceSourceCurrency;
            DocHomeCurr = invoice.InvoiceHomeCurrency;
            DocExchRate = invoice.InvoiceRate;
            DocRateDate = invoice.InvoiceRateDate.Value;
            SrcTotAmtWTax = invoice.InvoiceTotalWithTax;
            HomeTotAmtWTax = invoice.InvoiceTotalWithTax * DocExchRate;
            SrcDiscAmt = invoice.InvoiceDiscountAmount;
            HomeDiscAmt = invoice.InvoiceDiscountAmount * DocExchRate;
            SrcTBaseAmt = invoice.InvoiceRunningTotal;
            HomeTBaseAmt = invoice.InvoiceRunningTotal * DocExchRate;

            if (!string.IsNullOrWhiteSpace(invoice.TaxAuthority1) && taxAuthKeys.Contains(invoice.TaxAuthority1))
            {
                SrcTotTaxAmt += invoice.TotalTaxAmount1;
            }
            if (!string.IsNullOrWhiteSpace(invoice.TaxAuthority2) && taxAuthKeys.Contains(invoice.TaxAuthority2))
            {
                SrcTotTaxAmt += invoice.TotalTaxAmount2;
            }
            if (!string.IsNullOrWhiteSpace(invoice.TaxAuthority3) && taxAuthKeys.Contains(invoice.TaxAuthority3))
            {
                SrcTotTaxAmt += invoice.TotalTaxAmount3;
            }
            if (!string.IsNullOrWhiteSpace(invoice.TaxAuthority4) && taxAuthKeys.Contains(invoice.TaxAuthority4))
            {
                SrcTotTaxAmt += invoice.TotalTaxAmount4;
            }
            if (!string.IsNullOrWhiteSpace(invoice.TaxAuthority5) && taxAuthKeys.Contains(invoice.TaxAuthority5))
            {
                SrcTotTaxAmt += invoice.TotalTaxAmount5;
            }
            HomeTotTaxAmt = SrcTotTaxAmt * DocExchRate;

            CreatedBy = "Sys-Admin";
        }
        public bool IsValid()
        {
            return true;
        }
        public EtimsTransact GetSalesTransact(ClientBranch clientBranch)
        {
            /*if (RecordStatus == RecordStatus.POST_OK || RecordStatus == RecordStatus.POST_FAIL
                || RecordStatus == RecordStatus.POST_DUPL || !IsValid())
                return null;*/

            var etimsTransact = new EtimsTransact()
            {
                BranchCode = BranchCode,
                ReqType = ETIMSReqType.SAVE_SALE,
                DocNumber = CacheKey,
                DocStamp = SalesTrxData?.SourceStamp ?? DateTime.Now,
                SourceApp = SourceApp,
                ReqAddress = $"{clientBranch.EtrAddress}/trnsSales/saveSales",
                ReqKey = $"{nameof(ETIMSReqType.SAVE_SALE)}:{CacheKey}",
                ParentKey = null,
                IsNewRequest = true,
                NextSeqNumber = -1
            };
            return etimsTransact;
        }
        public EtimsTransact GetStockTransact(ClientBranch clientBranch)
        {
            /*if (RecordStatus == RecordStatus.POST_OK || RecordStatus == RecordStatus.POST_FAIL
                || RecordStatus == RecordStatus.POST_DUPL || !IsValid())
                return null;*/

            var etimsTransact = new EtimsTransact()
            {
                BranchCode = BranchCode,
                ReqType = ETIMSReqType.SAVE_STOCKIO,
                DocNumber = CacheKey,
                DocStamp = SalesTrxData?.SourceStamp ?? DateTime.Now,
                SourceApp = SourceApp,
                ReqAddress = $"{clientBranch.EtrAddress}/stock/saveStockItems",
                ReqKey = $"{nameof(ETIMSReqType.SAVE_STOCKIO)}:{CacheKey}",
                ParentKey = $"{nameof(ETIMSReqType.SAVE_SALE)}:{CacheKey}",
                IsNewRequest = true,
                NextSeqNumber = -1
            };
            return etimsTransact;
        }
    }
    [Table("SalesTrxData")]
    public class SalesTrxData : BaseDataEntity
    {
        [Key]
        [Required]
        public int SalesTrxID { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public SalesTransact SalesTransact { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public TrnsSalesSaveReq TrnsSalesSaveReq { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public TrnsSalesSaveResp TrnsSalesSaveResp { get; set; }
        public SalesTrxData()
        {
        }
        public SalesTrxData(SalesTransact salesTransact, TrnsSalesSaveReq trnsSalesSave, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice invoice)
            : this()
        {
            SourceStamp = invoice.InvoiceDate.Value;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(invoice);
            TrnsSalesSaveReq = trnsSalesSave;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(trnsSalesSave, new DecimalFormatConverter());
        }
        public TrnsSalesSaveReq GetEtimsRequest()
        {
            if (string.IsNullOrWhiteSpace(RequestPayload))
                return null;
            TrnsSalesSaveReq = Newtonsoft.Json.JsonConvert.DeserializeObject<TrnsSalesSaveReq>(RequestPayload);
            return TrnsSalesSaveReq;
        }

    }

}
