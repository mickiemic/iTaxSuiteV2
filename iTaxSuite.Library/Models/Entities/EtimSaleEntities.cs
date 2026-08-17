using iTaxSuite.Library.Constants;
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
        [StringLength(128)]
        public string ExternalID { get; set; }
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
        [StringLength(256)]
        public string ExternalURL { get; set; }
        [StringLength(256)]
        public string OfflineURL { get; set; }
        public SalesTrxData? SalesTrxData { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public override string CacheKey => $"{BranchCode}:{DocNumber}";
        public virtual ClientBranch ClientBranch { get; set; }
        public List<SalesItem> SalesItems { get; set; } = new();
        public SalesTransact()
        {
        }
        public SalesTransact(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer, 
            Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice invoice, S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
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

            foreach(var line in invoice.InvoiceDetails)
            {
                //var salesItem = new SalesItem(this, line, taxGroup, taxAuthKeys);
                var salesItem = SalesItem.CreateItem(this, line, taxGroup, taxAuthKeys);
                if (salesItem != null)
                    SalesItems.Add(salesItem);
            }
            if (SalesItems.Count == 0)
            {
                Remark = "There are no valid lines for this invoice";
                RecordStatus = RecordStatus.INVALID;
            }

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

            CreatedBy = GeneralConst.APPLICATION_NAME;
        }

        public SalesTransact(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer, 
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice invoice, S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
            : this()
        {
            BranchCode = clientBranch.BranchCode;
            DocNumber = invoice.DocumentNumber;
            DocStamp = invoice.DocumentDate.Value;
            ReqType = ETIMSReqType.SAVE_SALE;
            if (invoice.DocumentType == Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.Invoice)
            {
                DocType = DocumentType.INVOICE;
            } 
            else if (invoice.DocumentType == Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.CreditNote)
            {
                DocType = DocumentType.CREDITNOTE;
                RefInvNumber = invoice.ApplytoDocument;
                if (string.IsNullOrWhiteSpace(RefInvNumber))
                {
                    RecordStatus = RecordStatus.INVALID;
                    Remark = $"Invalid CreditNote {DocNumber} with no reference invoice number";
                }
            }
            SourceApp = "AR";
            EtrSeqNumber = clientBranch.SaleInvoiceSeq;
            Description = invoice.InvoiceDescription;

            CustNumber = invoice.CustomerNumber;
            //CustName = invoice.BillTo;
            if (customer != null)
            {
                CustName = customer.CustomerName.Trim();
                CustTaxNumber = customer.TaxRegistrationNumber1.Trim();
            }

            DocSrcCurr = invoice.CurrencyCode;
            //TODO: fix AR Invoice HOME Currency Hack
            //DocHomeCurr = invoice.CurrencyCode;
            DocHomeCurr = "KES";
            DocExchRate = invoice.ExchangeRate;
            DocRateDate = invoice.RateDate.Value;
            SrcTotAmtWTax = invoice.DocumentTotalIncludingTax;
            HomeTotAmtWTax = invoice.DocumentTotalIncludingTax * DocExchRate;
            SrcDiscAmt = invoice.FunctionalDiscountAmount;
            HomeDiscAmt = invoice.FunctionalDiscountAmount * DocExchRate;
            SrcTBaseAmt = invoice.TaxableAmount;
            HomeTBaseAmt = invoice.TaxableAmount * DocExchRate;

            foreach(var line in invoice.InvoiceDetails)
            {
                //var salesItem = new SalesItem(this, invoice, line, taxGroup, taxAuthKeys);
                var salesItem = SalesItem.CreateItem(this, invoice, line, taxGroup, taxAuthKeys);
                if (salesItem != null)
                    SalesItems.Add(salesItem);
            }

            if (!string.IsNullOrWhiteSpace(invoice.TaxAuthority1) && taxAuthKeys.Contains(invoice.TaxAuthority1))
            {
                SrcTotTaxAmt += invoice.TaxAmount1;
            }
            if (!string.IsNullOrWhiteSpace(invoice.TaxAuthority2) && taxAuthKeys.Contains(invoice.TaxAuthority2))
            {
                SrcTotTaxAmt += invoice.TaxAmount2;
            }
            if (!string.IsNullOrWhiteSpace(invoice.TaxAuthority3) && taxAuthKeys.Contains(invoice.TaxAuthority3))
            {
                SrcTotTaxAmt += invoice.TaxAmount3;
            }
            if (!string.IsNullOrWhiteSpace(invoice.TaxAuthority4) && taxAuthKeys.Contains(invoice.TaxAuthority4))
            {
                SrcTotTaxAmt += invoice.TaxAmount4;
            }
            if (!string.IsNullOrWhiteSpace(invoice.TaxAuthority5) && taxAuthKeys.Contains(invoice.TaxAuthority5))
            {
                SrcTotTaxAmt += invoice.TaxAmount5;
            }
            HomeTotTaxAmt = SrcTotTaxAmt * DocExchRate;

            CreatedBy = GeneralConst.APPLICATION_NAME;
        }

        public SalesTransact(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer, 
            Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote crNote, S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
            : this()
        {
            BranchCode = clientBranch.BranchCode;
            DocNumber = crNote.CreditDebitNoteNumber;
            RefInvNumber = crNote.InvoiceNumber;
            if (string.IsNullOrWhiteSpace(RefInvNumber))
            {
                RecordStatus = RecordStatus.INVALID;
                Remark = $"Invalid CreditNote {DocNumber} with no reference invoice number";
            }
            DocType = DocumentType.CREDITNOTE;
            ReqType = ETIMSReqType.SAVE_SALE;
            DocStamp = crNote.InvoiceDate.Value;
            SourceApp = "OE";
            EtrSeqNumber = clientBranch.SaleInvoiceSeq;

            CustNumber = crNote.CustomerNumber;
            CustName = crNote.BillTo;
            if (customer != null)
            {
                CustName = customer.CustomerName.Trim();
                CustTaxNumber = customer.TaxRegistrationNumber1.Trim();
            }

            DocSrcCurr = crNote.InvoiceSourceCurrency;
            DocHomeCurr = crNote.InvoiceHomeCurrency;
            DocExchRate = crNote.InvoiceRate;
            DocRateDate = crNote.InvoiceRateDate.Value;
            SrcTotAmtWTax = crNote.CreditDebitNoteTotal;
            HomeTotAmtWTax = crNote.CreditDebitNoteTotal * DocExchRate;
            SrcDiscAmt = crNote.CreditDebitNoteDiscountAmt;
            HomeDiscAmt = crNote.CreditDebitNoteDiscountAmt * DocExchRate;
            SrcTBaseAmt = crNote.CreditDebitNoteTotBeforeTax;
            HomeTBaseAmt = crNote.CreditDebitNoteTotBeforeTax * DocExchRate;

            foreach (var line in crNote.CreditDebitDetails)
            {
                var salesItem = SalesItem.CreateItem(this, line, taxGroup, taxAuthKeys);
                if (salesItem is not null)
                    SalesItems.Add(salesItem);
            }
            if (SalesItems.Count == 0)
            {
                Remark = "There are no valid lines for this invoice";
                RecordStatus = RecordStatus.INVALID;
            }

            if (!string.IsNullOrWhiteSpace(crNote.TaxAuthority1) && taxAuthKeys.Contains(crNote.TaxAuthority1))
            {
                SrcTotTaxAmt += crNote.TotalTaxAmount1;
            }
            if (!string.IsNullOrWhiteSpace(crNote.TaxAuthority2) && taxAuthKeys.Contains(crNote.TaxAuthority2))
            {
                SrcTotTaxAmt += crNote.TotalTaxAmount2;
            }
            if (!string.IsNullOrWhiteSpace(crNote.TaxAuthority3) && taxAuthKeys.Contains(crNote.TaxAuthority3))
            {
                SrcTotTaxAmt += crNote.TotalTaxAmount3;
            }
            if (!string.IsNullOrWhiteSpace(crNote.TaxAuthority4) && taxAuthKeys.Contains(crNote.TaxAuthority4))
            {
                SrcTotTaxAmt += crNote.TotalTaxAmount4;
            }
            if (!string.IsNullOrWhiteSpace(crNote.TaxAuthority5) && taxAuthKeys.Contains(crNote.TaxAuthority5))
            {
                SrcTotTaxAmt += crNote.TotalTaxAmount5;
            }
            HomeTotTaxAmt = SrcTotTaxAmt * DocExchRate;

            CreatedBy = GeneralConst.APPLICATION_NAME;
        }

        public bool IsValid()
        {
            if (RecordStatus == RecordStatus.INVALID)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(DocNumber) || string.IsNullOrWhiteSpace(BranchCode))
            {
                return false;
            }
            if (DocType == DocumentType.CREDITNOTE && string.IsNullOrWhiteSpace(RefInvNumber))
            {
                return false;
            }

            return true;
        }
        public bool IsTaxComplete()
        {
            return !string.IsNullOrWhiteSpace(QRText);
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
                NextSeqNumber = -1,
                RecordStatus = RecordStatus.QUEUEDOUT
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
                NextSeqNumber = -1,
                RecordStatus = RecordStatus.QUEUEDOUT
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
        public DateTime? CallbackTime { get; set; }
        [StringLength(4000)]
        public string CallbackPayload { get; set; }
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
        public DTaxSaveSaleReq DTaxSaveSaleReq { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DTaxSaveCNoteReq DTaxSaveCNoteReq { get; set; }
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

        public SalesTrxData(SalesTransact oeSaleTrx, TrnsSalesSaveReq trnsSalesSave, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice invoice)
            : this()
        {
            SourceStamp = invoice.DocumentDate.Value;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(invoice);
            TrnsSalesSaveReq = trnsSalesSave;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(trnsSalesSave, new DecimalFormatConverter());
        }
        public SalesTrxData(SalesTransact oeSaleTrx, DTaxSaveSaleReq dTaxSaveSaleReq, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice invoice)
            : this()
        {
            SourceStamp = invoice.DocumentDate.Value;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(invoice);
            DTaxSaveSaleReq = dTaxSaveSaleReq;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(dTaxSaveSaleReq, new DecimalFormatConverter());
        }

        public SalesTrxData(SalesTransact oeSaleTrx, TrnsSalesSaveReq trnsSalesSave, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote crNote)
            : this()
        {
            SourceStamp = crNote.InvoiceDate.Value;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(crNote);
            TrnsSalesSaveReq = trnsSalesSave;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(trnsSalesSave, new DecimalFormatConverter());
        }

        public SalesTrxData(SalesTransact oeSaleTrx, DTaxSaveSaleReq dTaxSaveSaleReq, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice invoice)
            : this()
        {
            SourceStamp = invoice.InvoiceDate.Value;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(invoice);
            DTaxSaveSaleReq = dTaxSaveSaleReq;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(dTaxSaveSaleReq, new DecimalFormatConverter());
        }

        public SalesTrxData(SalesTransact oeSaleTrx, DTaxSaveSaleReq dTaxSaveSaleReq, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote crNote)
            : this()
        {
            SourceStamp = crNote.InvoiceDate.Value;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(crNote);
            DTaxSaveSaleReq = dTaxSaveSaleReq;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(dTaxSaveSaleReq, new DecimalFormatConverter());
        }

        public SalesTrxData(SalesTransact oeSaleTrx, DTaxSaveCNoteReq dTaxSaveCNoteReq, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote crNote)
            : this()
        {
            SourceStamp = crNote.InvoiceDate.Value;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(crNote);
            DTaxSaveCNoteReq = dTaxSaveCNoteReq;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(dTaxSaveCNoteReq, new DecimalFormatConverter());
        }

        public SalesTrxData(SalesTransact arSaleTrx, DTaxSaveCNoteReq dTaxSaveCNoteReq, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice crNote)
            : this()
        {
            SourceStamp = crNote.DocumentDate.Value;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(crNote);
            DTaxSaveCNoteReq = dTaxSaveCNoteReq;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(dTaxSaveCNoteReq, new DecimalFormatConverter());
        }

        public TrnsSalesSaveReq GetEtimsRequest()
        {
            if (string.IsNullOrWhiteSpace(RequestPayload))
                return null;
            TrnsSalesSaveReq = Newtonsoft.Json.JsonConvert.DeserializeObject<TrnsSalesSaveReq>(RequestPayload);
            return TrnsSalesSaveReq;
        }

        public DTaxSaveSaleReq GetDTaxInvRequest()
        {
            if (string.IsNullOrWhiteSpace(RequestPayload))
                return null;
            DTaxSaveSaleReq = Newtonsoft.Json.JsonConvert.DeserializeObject<DTaxSaveSaleReq>(RequestPayload);
            return DTaxSaveSaleReq;
        }
        public DTaxSaveCNoteReq GetDTaxCNoteRequest()
        {
            if (string.IsNullOrWhiteSpace(RequestPayload))
                return null;
            DTaxSaveCNoteReq = Newtonsoft.Json.JsonConvert.DeserializeObject<DTaxSaveCNoteReq>(RequestPayload);
            return DTaxSaveCNoteReq;
        }

        public void ClearRequestData()
        {
            RequestPayload = null;
            RequestTime = null;
        }

    }
    public class SalesItem : BaseEntity
    {
        [Key]
        [Required]
        public int SalesTrxID { get; set; }
        public int SourceLineNo { get; set; }
        [Required]
        [StringLength(64)]
        public string ProductCode { get; set; }
        [StringLength(128)]
        public string ExternalID { get; set; }
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string BranchCode { get; set; }
        [StringLength(64)]
        public string TaxItemCode { get; set; }
        [Required]
        public string TaxTypeCode { get; set; }
        public string ItemTypeCode { get; set; }
        public int ItemSeqNumber { get; set; } = -1;
        [StringLength(10)]
        public string ItemClassCode { get; set; }
        [Required]
        [StringLength(128)]
        public string Description { get; set; }
        [NotMapped]
        public string _pkgUnitCode { get; set; }
        //[Required]
        [StringLength(8)]
        public string PkgUnitCode { get; set; }
        [Precision(19, 3)]
        public decimal Package { get; set; }
        [NotMapped]
        public string _qtyUnitCode { get; set; }
        //[Required]
        [StringLength(8)]
        public string QtyUnitCode { get; set; }
        [Precision(19, 3)]
        public decimal Quantity { get; set; }
        [Required]
        public bool IsStockable { get; set; }
        [Precision(19, 3)]
        public decimal UnitPrice { get; set; }
        [Precision(19, 3)]
        public decimal DiscountAmount { get; set; }
        [Precision(19, 3)]
        public decimal DiscountRate { get; set; }
        [Precision(19, 3)]
        public decimal TaxableAmount { get; set; }
        [NotMapped]
        [Precision(19, 3)]
        public decimal TaxRate { get; set; }
        [Precision(19, 3)]
        public decimal TaxAmount { get; set; }
        [Precision(19, 3)]
        public decimal TotalAmount { get; set; }
        [Precision(19, 3)]
        public decimal SupplyPrice { get; set; }
        public RecordStatus RecordStatus { get; set; } = RecordStatus.NONE;
        
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public SalesTransact SalesTransact { get; set; }
        public SalesItem()
        {
        }

        public static SalesItem? CreateItem(SalesTransact salesTransact, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.InvoiceDetail item,
            S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
        {
            if (item.QuantityShipped == 0)
            {
                UI.Warn(salesTransact.DocNumber, $"{salesTransact.DocNumber} ItemNumber:{item.Item} has Invalid {item.QuantityShipped} quantity");
                return null;
            }
            var UnitPrice = Math.Round(item.ExtPriceNetOfDiscIncludeTax / item.QuantityShipped, 2);
            if (UnitPrice == 0)
            {
                UI.Warn(salesTransact.DocNumber, $"{salesTransact.DocNumber} ItemNumber:{item.Item} has Invalid {UnitPrice} unit price");
                return null;
            }
            return new SalesItem(salesTransact, item, taxGroup, taxAuthKeys);
        }
        public SalesItem(SalesTransact salesTransact, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.InvoiceDetail item,
            S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
            : this()
        {
            SalesTransact = salesTransact;
            BranchCode = salesTransact.BranchCode;

            SourceLineNo = item.LineUniquifier;
            ItemSeqNumber = -1;
            ProductCode = $"IC:{item.Item}";
            Description = item.Description;
            ItemTypeCode = "2";
            IsStockable = item.StockItem;

            _pkgUnitCode = item.InvoiceUnitOfMeasure;
            Package = item.QuantityShipped;

            _qtyUnitCode = item.PricingUnit;
            Quantity = item.QuantityShipped;

            SupplyPrice = item.UnitCost;
            //UnitPrice = (item.PricingUnitPrice != 0) ? item.PricingUnitPrice : item.ExtPriceNetOfDiscIncludeTax;
            UnitPrice = Math.Round(item.ExtPriceNetOfDiscIncludeTax/item.QuantityShipped, 2);
            if (SupplyPrice == 0)
                SupplyPrice = UnitPrice;

            /*
             Match TaxAuthority1 <=> TaxAuthority
             TransactionType <=> Sales/Purchases
             TaxClass1 <=> BuyersClass
             TaxRate1 <=> ItemRate1 = Check After Exempt flag
             */
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority1) && taxAuthKeys.Contains(item.TaxAuthority1))
            {
                TaxableAmount = item.TaxBase1;
                TaxAmount += item.TaxAmount1;
                if (string.IsNullOrWhiteSpace(TaxTypeCode))
                {
                    var tClass = taxGroup.Authorities[item.TaxAuthority1].Classes.FirstOrDefault(c => c.ClassKey == item.TaxClass1 && c.TransactionType == Enum.GetName(Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxClass.TransactionTypeEnum.Sales));
                    var tRate = taxGroup.Authorities[item.TaxAuthority1].Rates.FirstOrDefault(r => r.ItemRate1 == item.TaxRate1);
                    TaxTypeCode = ETimsUtils.GetTaxRate(tClass, tRate);
                }
                TaxRate = item.TaxRate1;
            }
            else
            {
                //TODO: throw error if TaxAuthority1 invalid
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority2) && taxAuthKeys.Contains(item.TaxAuthority2))
            {
                TaxableAmount = item.TaxBase2;
                TaxAmount += item.TaxAmount2;
                TaxRate = item.TaxRate2;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority3) && taxAuthKeys.Contains(item.TaxAuthority3))
            {
                TaxableAmount = item.TaxBase3;
                TaxAmount += item.TaxAmount3;
                TaxRate = item.TaxRate3;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority4) && taxAuthKeys.Contains(item.TaxAuthority4))
            {
                TaxableAmount = item.TaxBase4;
                TaxAmount += item.TaxAmount4;
                TaxRate = item.TaxRate4;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority5) && taxAuthKeys.Contains(item.TaxAuthority5))
            {
                TaxableAmount = item.TaxBase5;
                TaxAmount += item.TaxAmount5;
                TaxRate = item.TaxRate5;
            }

            //TaxAmount = item.TaxAmount1;
            //TotalAmount = UnitPrice * Quantity;
            TotalAmount = TaxableAmount + TaxAmount;

            DiscountRate = item.DiscountPercent;
            if (DiscountRate > 0)
            {
                DiscountAmount = Math.Round(DiscountRate * UnitPrice, 2);
            }

        }

        public static SalesItem? CreateItem(SalesTransact salesTransact, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice arInvoice, 
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.InvoiceDetail item, S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
        {
            var productCode = string.Empty;
            if (arInvoice.InvoiceType == Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.InvoiceTypeEnum.Item)
                productCode = $"AR:{item.ItemNumber}";
            else
                productCode = $"GL:{item.RevenueAccount}";
            if (string.IsNullOrWhiteSpace(productCode))
            {
                throw new Exception($"Invalid ItemNumber:{item.ItemNumber} for ARInvoice:{arInvoice.DocumentNumber} with Description {item.Description}");
            }
            decimal amtValue = item.ExtendedAmountWithTIP;
            if (amtValue <= 0)
                throw new Exception($"Invalid Amount for {item.Description} :: {amtValue}");
            return new SalesItem(salesTransact, arInvoice, item, taxGroup, taxAuthKeys);
        }
        public SalesItem(SalesTransact salesTransact, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice arInvoice,
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.InvoiceDetail item, S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
        {
            SalesTransact = salesTransact;
            BranchCode = salesTransact.BranchCode;

            SourceLineNo = Convert.ToInt32(item.LineNumber);
            ItemSeqNumber = -1; // Internal
            if (arInvoice.InvoiceType == Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.InvoiceTypeEnum.Item)
                ProductCode = $"AR:{item.ItemNumber}";
            else
                ProductCode = $"GL:{item.RevenueAccount}";
            ItemTypeCode = "3";
            Description = item.Description;
            IsStockable = false;
            if (string.IsNullOrWhiteSpace(ProductCode))
            {
                throw new Exception($"Invalid ItemNumber:{item.ItemNumber} for ARInvoice:{arInvoice.DocumentNumber} with Description {item.Description}");
            }

            _pkgUnitCode = item.UnitOfMeasure;
            if (string.IsNullOrWhiteSpace(_pkgUnitCode))
            {
                _pkgUnitCode = "NA";
            }
            _qtyUnitCode = _pkgUnitCode;
            Quantity = Package = (item.Quantity == 0) ? 1 : item.Quantity;

            decimal amtValue = item.ExtendedAmountWithTIP;
            if (amtValue <= 0)
                throw new Exception($"Invalid Amount for {item.Description} :: {amtValue}");

            UnitPrice = SupplyPrice = amtValue;

            if (!string.IsNullOrWhiteSpace(arInvoice.TaxAuthority1) && taxAuthKeys.Contains(arInvoice.TaxAuthority1))
            {
                TaxableAmount = item.TaxBase1;
                TaxAmount += item.TaxAmount1;
                if (string.IsNullOrWhiteSpace(TaxTypeCode))
                {
                    var tClass = taxGroup.Authorities[arInvoice.TaxAuthority1].Classes.FirstOrDefault(c => c.ClassKey == arInvoice.TaxClass1 && c.TransactionType == Enum.GetName(Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxClass.TransactionTypeEnum.Sales));
                    if (tClass is null)
                        throw new Exception($"AR Item {ProductCode} has an invalid TaxClass: {arInvoice.TaxClass1}");
                    var tRate = taxGroup.Authorities[arInvoice.TaxAuthority1].Rates.FirstOrDefault(r => r.ItemRate1 == item.TaxRate1);
                    if (tRate is null)
                        throw new Exception($"AR Item {ProductCode} has an invalid Rate: {item.TaxRate1}");
                    TaxTypeCode = ETimsUtils.GetTaxRate(tClass, tRate);
                }
                TaxRate = item.TaxRate1;
                if (item.TaxIncluded1 != Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.InvoiceDetail.TaxIncluded1Enum.Yes)
                {
                    //TODO: handle cases of calculating tax for non-tax included items
                }
            }
            else
            {
                //TODO: throw error if TaxAuthority1 invalid
            }
            if (!string.IsNullOrWhiteSpace(arInvoice.TaxAuthority2) && taxAuthKeys.Contains(arInvoice.TaxAuthority2))
            {
                TaxableAmount = item.TaxBase2;
                TaxAmount += item.TaxAmount2;
                TaxRate = item.TaxRate2;
            }
            if (!string.IsNullOrWhiteSpace(arInvoice.TaxAuthority3) && taxAuthKeys.Contains(arInvoice.TaxAuthority3))
            {
                TaxableAmount = item.TaxBase3;
                TaxAmount += item.TaxAmount3;
                TaxRate = item.TaxRate3;
            }
            if (!string.IsNullOrWhiteSpace(arInvoice.TaxAuthority4) && taxAuthKeys.Contains(arInvoice.TaxAuthority4))
            {
                TaxableAmount = item.TaxBase4;
                TaxAmount += item.TaxAmount4;
                TaxRate = item.TaxRate4;
            }
            if (!string.IsNullOrWhiteSpace(arInvoice.TaxAuthority5) && taxAuthKeys.Contains(arInvoice.TaxAuthority5))
            {
                TaxableAmount = item.TaxBase5;
                TaxAmount += item.TaxAmount5;
                TaxRate = item.TaxRate5;
            }

            TotalAmount = TaxableAmount + TaxAmount;
            if (TotalAmount == 0)
                TotalAmount = TaxableAmount = amtValue;

            DiscountRate = 0;
            DiscountAmount = 0;
        }

        public static SalesItem? CreateItem(SalesTransact salesTransact, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitDetail item,
            S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
        {
            if (item.QuantityShipped == 0)
            {
                UI.Warn(salesTransact.DocNumber, $"{salesTransact.DocNumber} ItemNumber:{item.Item} has Invalid {item.QuantityShipped} quantity");
                return null;
            }
            var unitPrice = item.PricingUnitPrice;
            if (unitPrice == 0)
            {
                UI.Warn(salesTransact.DocNumber, $"{salesTransact.DocNumber} ItemNumber:{item.Item} has Invalid {unitPrice} unit price");
                return null;
            }
            return new SalesItem(salesTransact, item, taxGroup, taxAuthKeys);
        }
        public SalesItem(SalesTransact salesTransact, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitDetail item, 
            S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
            : this()
        {
            SalesTransact = salesTransact;
            BranchCode = salesTransact.BranchCode;

            SourceLineNo = item.LineNumber;
            ItemSeqNumber = -1;
            ProductCode = $"IC:{item.Item}";
            Description = item.Description;
            ItemTypeCode = "2";
            IsStockable = item.StockItem;

            _pkgUnitCode = item.InvoiceUnitOfMeasure;
            Package = item.QuantityShipped != 0 ? item.QuantityShipped : 1;

            _qtyUnitCode = item.PricingUnit;
            Quantity = Package * item.PricingUnitConversion;

            SupplyPrice = item.UnitCost;
            UnitPrice = item.PricingUnitPrice;
            if (SupplyPrice == 0)
                SupplyPrice = UnitPrice;

            /*
             Match TaxAuthority1 <=> TaxAuthority
             TransactionType <=> Sales/Purchases
             TaxClass1 <=> BuyersClass
             TaxRate1 <=> ItemRate1 = Check After Exempt flag
             */
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority1) && taxAuthKeys.Contains(item.TaxAuthority1))
            {
                TaxAmount += item.TaxAmount1;
                if (string.IsNullOrWhiteSpace(TaxTypeCode))
                {
                    var tClass = taxGroup.Authorities[item.TaxAuthority1].Classes.FirstOrDefault(c => c.ClassKey == item.TaxClass1 && c.TransactionType == Enum.GetName(Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxClass.TransactionTypeEnum.Sales));
                    var tRate = taxGroup.Authorities[item.TaxAuthority1].Rates.FirstOrDefault(r => r.ItemRate1 == item.TaxRate1);
                    TaxTypeCode = ETimsUtils.GetTaxRate(tClass, tRate);
                }
                TaxRate = item.TaxRate1;
            }
            else
            {
                //TODO: throw error if TaxAuthority1 invalid
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority2) && taxAuthKeys.Contains(item.TaxAuthority2))
            {
                TaxAmount += item.TaxAmount2;
                TaxRate = item.TaxRate2;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority3) && taxAuthKeys.Contains(item.TaxAuthority3))
            {
                TaxAmount += item.TaxAmount3;
                TaxRate = item.TaxRate3;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority4) && taxAuthKeys.Contains(item.TaxAuthority4))
            {
                TaxAmount += item.TaxAmount4;
                TaxRate = item.TaxRate4;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority5) && taxAuthKeys.Contains(item.TaxAuthority5))
            {
                TaxAmount += item.TaxAmount5;
                TaxRate = item.TaxRate5;
            }

            TaxableAmount = item.TaxBase1;
            //TaxAmount = item.TaxAmount1;
            //TotalAmount = UnitPrice * Quantity;
            TotalAmount = TaxableAmount + TaxAmount;

            DiscountRate = item.DiscountPercent;
            DiscountAmount = item.DiscountedExtendedAmount;
        }
    }

}
