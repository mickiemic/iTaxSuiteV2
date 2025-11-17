using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace iTaxSuite.Library.Models.ViewModels
{
    // ItemSave Models
    public class ProductFilter : APagedFilter
    {
        public bool IsValid => true;
        public RecordStatusGroup RecordGroup { get; set; } = RecordStatusGroup.ALL;
        [StringLength(32)]
        public string ProductCode { get; set; }
        [StringLength(128)]
        public string Description { get; set; }
    }
    public class ProductKey
    {
        [StringLength(32)]
        public string ProductCode { get; set; }
    }

    public class StockItemKey : BaseCacheKey
    {
        [StringLength(32)]
        public string ProductCode { get; set; }
        public int EtrSeqNumber { get; set; }
        [StringLength(64)]
        public string TaxItemCode { get; set; }
        [StringLength(16)]
        public string ItemClassCode { get; set; }
        [StringLength(128)]
        public string Description { get; set; }
        [StringLength(32)]
        public string PackageUnit { get; set; }
        [StringLength(32)]
        public string QuantityUnit { get; set; }
        public bool IsStockable { get; set; }
        public StockItemKey()
        {
            RecordStatus = RecordStatus.NONE;
        }

        public StockItemKey(StockItem stockItem)
            : this()
        {
            RecordStatus = stockItem.Product.RecordStatus;
            ProductCode = stockItem.Product.ProductCode;
            TaxItemCode = stockItem.TaxItemCode;
            Description = stockItem.Product.Description;
            PackageUnit = stockItem.Product.PackageUnit;
            QuantityUnit = stockItem.Product.QuantityUnit;
            IsStockable = stockItem.Product.IsStockable;
            ItemClassCode = stockItem.Product.ItemClassCode;
            EtrSeqNumber = stockItem.EtrSeqNumber;
            RecordStatus = stockItem.RecordStatus;
        }
    }

    public class StockFilter : ProductFilter
    {
        [StringLength(2)]
        public string BranchCode { get; set; }
        [StringLength(64)]
        public string TaxItemCode { get; set; }
    }
    public class BranchStockKey
    {
        [StringLength(2)]
        public string BranchCode { get; set; } = "00";
        [StringLength(32)]
        public string ProductCode { get; set; }
    }

    public class SaveItemReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public int _offset { get; set; }
        [Newtonsoft.Json.JsonProperty("itemCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemCd")]
        public string ItemCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemClsCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemClsCd")]
        public string ItemClassCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemTyCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemTyCd")]
        public string ItemTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemNm")]
        [System.Text.Json.Serialization.JsonPropertyName("itemNm")]
        public string ItemName { get; set; }
        [Newtonsoft.Json.JsonProperty("itemStdNm")]
        [System.Text.Json.Serialization.JsonPropertyName("itemStdNm")]
        public string ItemStdName { get; set; }
        [Newtonsoft.Json.JsonProperty("orgnNatCd")]
        [System.Text.Json.Serialization.JsonPropertyName("orgnNatCd")]
        public string OriginNatCode { get; set; } = "KE";
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string _pkgUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("pkgUnitCd")]
        public string PkgUnitCode { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string _qtyUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("qtyUnitCd")]
        public string QtyUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("taxTyCd")]
        public string TaxTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("btchNo")]
        public string BatchNo { get; set; }
        [Newtonsoft.Json.JsonProperty("bcd")]
        public string BarCode { get; set; }
        [Newtonsoft.Json.JsonProperty("dftPrc")]
        public double DftPrc { get; set; }
        [Newtonsoft.Json.JsonProperty("grpPrcL1")]
        public double GroupPrcL1 { get; set; }
        [Newtonsoft.Json.JsonProperty("grpPrcL2")]
        public double GroupPrcL2 { get; set; }
        [Newtonsoft.Json.JsonProperty("grpPrcL3")]
        public double GroupPrcL3 { get; set; }
        [Newtonsoft.Json.JsonProperty("grpPrcL4")]
        public double GroupPrcL4 { get; set; }
        [Newtonsoft.Json.JsonProperty("grpPrcL5")]
        public string GroupPrcL5 { get; set; }
        [Newtonsoft.Json.JsonProperty("addInfo")]
        public string AdditionalInfo { get; set; }
        [Newtonsoft.Json.JsonProperty("sftyQty")]
        public string SafetyQty { get; set; }
        [Newtonsoft.Json.JsonProperty("isrcAplcbYn")]
        public char InsuranceAplcb { get; set; } = 'N';
        [Newtonsoft.Json.JsonProperty("useYn")]
        public char Use { get; set; } = 'Y';
        [Newtonsoft.Json.JsonProperty("regrNm")]
        public string RegistrantName { get; set; } = ETIMSConst.DEFAULT_USER;
        [Newtonsoft.Json.JsonProperty("regrId")]
        public string RegistrantID { get; set; } = ETIMSConst.DEFAULT_USER;
        [Newtonsoft.Json.JsonProperty("modrNm")]
        public string ModifierName { get; set; } = ETIMSConst.DEFAULT_USER;
        [Newtonsoft.Json.JsonProperty("modrId")]
        public string ModifierID { get; set; } = ETIMSConst.DEFAULT_USER;

        public SaveItemReq()
        {
        }
        public SaveItemReq(ClientBranch clientBranch, StockItem stockItem, Sage.CA.SBS.ERP.Sage300.IC.WebApi.Models.Item item)
        {
            PIN = clientBranch.TaxClient.TaxNumber;
            _offset = stockItem.EtrSeqNumber;
            AdditionalInfo = stockItem.ProductCode;
            if (item.StockItem)
                ItemTypeCode = "2";
            else
                ItemTypeCode = "3";
            ItemName = ItemStdName = item.Description;
            PkgUnitCode = stockItem.Product.PackageUnit;
            QtyUnitCode = stockItem.Product.QuantityUnit;
            TaxTypeCode = "B";
            ItemClassCode = stockItem.Product.ItemClassCode;
            ItemCode = stockItem.TaxItemCode;

            string newItemClassCode = string.Empty;
            if (item.ItemOptionalFields != null && item.ItemOptionalFields.Count > 0)
            {
                var optTaxClass = item.ItemOptionalFields.FirstOrDefault(f => f.OptionalField
                    .Equals(GeneralConst.OPTFLD_TAXCLASSCODE, StringComparison.InvariantCultureIgnoreCase));
                if (optTaxClass != null && !string.IsNullOrWhiteSpace(optTaxClass.Value))
                {
                    newItemClassCode = optTaxClass.Value;
                }
            }
            if (newItemClassCode != ItemClassCode)
                ItemClassCode = newItemClassCode;
            UpdateItemCode();
        }
        public void UpdateItemCode()
        {
            ItemCode = $"{OriginNatCode}{ItemTypeCode}{PkgUnitCode}{QtyUnitCode}{_offset:0000000}";
        }
        public void UpdateAttributes((string PkgUnitCode, string QtyUnitCode) unitCodes)
        {
            PkgUnitCode = unitCodes.PkgUnitCode;
            QtyUnitCode = unitCodes.QtyUnitCode;
        }
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ItemClassCode);
        }
    }

    public class SaveItemResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        public JObject data { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string RawResponse { get; set; }
    }

    // Stock In/Out Models
    [NotMapped]
    public class StockIOSaveReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("sarNo")]
        public int StockRecNum { get; set; }
        [Newtonsoft.Json.JsonProperty("orgSarNo")]
        public int OrigStockRecNum { get; set; }
        [Newtonsoft.Json.JsonProperty("regTyCd")]
        public string RegistrationTypeCode { get; set; }    // 4.12 Transaction Progress
        [Newtonsoft.Json.JsonProperty("custTin")]
        public string CustomerPIN { get; set; }             // TaxRegistrationNumber1 - Customer
        [Newtonsoft.Json.JsonProperty("custNm")]
        public string CustomerName { get; set; }
        [Newtonsoft.Json.JsonProperty("custBhfId")]
        public string CustomerBranchID { get; set; }
        [Newtonsoft.Json.JsonProperty("sarTyCd")]
        public string StockRecordType { get; set; }
        [Newtonsoft.Json.JsonProperty("ocrnDt")]
        public string StockTrxDate { get; set; }
        [Newtonsoft.Json.JsonProperty("totItemCnt")]
        public int TotalItemCount { get; set; }
        [Newtonsoft.Json.JsonProperty("totTaxblAmt")]
        public decimal TotalTaxableAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("totTaxAmt")]
        public decimal TotalTaxAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("totAmt")]
        public decimal TotalAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("remark")]
        public string Remark { get; set; }
        [Newtonsoft.Json.JsonProperty("regrId")]
        public string RegistrantID { get; set; }
        [Newtonsoft.Json.JsonProperty("regrNm")]
        public string RegistrantName { get; set; }
        [Newtonsoft.Json.JsonProperty("modrId")]
        public string ModifierID { get; set; }  // Modifier ID
        [Newtonsoft.Json.JsonProperty("modrNm")]
        public string ModifierName { get; set; }  // Modifier Name

        [Newtonsoft.Json.JsonProperty("itemList")]
        public List<StockIOItem> ItemList { get; set; } = new();

        public StockIOSaveReq()
        {
        }

        /*public StockIOSaveReq(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.PO.WebApi.Models.Invoice pInvoice, TrnsPurchaseSaveReq eTimsPurch)
            : this()
        {
            RegistrationTypeCode = eTimsPurch.RegistrationTypeCode;
            Remark = eTimsPurch.Remark;
            RegistrantID = eTimsPurch.RegistrantID;
            RegistrantName = eTimsPurch.RegistrantName;
            ModifierID = eTimsPurch.ModifierID;
            ModifierName = eTimsPurch.ModifierName;

            CustomerBranchID = "00";
            CustomerPIN = PIN = clientBranch.TaxClient.TaxNumber;
            CustomerName = config.ClientName;
            StockTrxDate = pInvoice.ReceiptDate.Value.ToString(ETIMSConst.FMT_DATEONLY);
            StockRecordType = StockMovementType.Purchase.GetEnumMemberValue();

            foreach (var pItem in eTimsPurch.ItemList)
            {
                var ioItem = new StockIOItem(pItem);
                TotalTaxableAmount += ioItem.TaxableAmount;
                TotalTaxAmount += ioItem.TaxAmount;
                TotalAmount += ioItem.TotalAmount;
                ItemList.Add(ioItem);
            }

            TotalItemCount = ItemList.Count;
        }*/

        public StockIOSaveReq(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice oeInvoice, TrnsSalesSaveReq eTimsSale)
            : this()
        {
            PIN = clientBranch.TaxClient.TaxNumber;
            RegistrationTypeCode = ETimsUtils.GetRegistrationType();
            Remark = eTimsSale.Remark;
            RegistrantID = eTimsSale.RegistrantID;
            RegistrantName = eTimsSale.RegistrantName;
            ModifierID = eTimsSale.ModifierID;
            ModifierName = eTimsSale.ModifierName;

            CustomerBranchID = "00";
            CustomerPIN = eTimsSale.CustomerPIN;
            CustomerName = eTimsSale.CustomerName;
            StockTrxDate = oeInvoice.ShipmentDate.Value.ToString(ETIMSConst.FMT_DATEONLY);
            StockRecordType = StockMovementType.Sale.GetEnumMemberValue();

            foreach (var sItem in eTimsSale.ItemList)
            {
                var ioItem = new StockIOItem(sItem);
                TotalTaxableAmount += ioItem.TaxableAmount;
                TotalTaxAmount += ioItem.TaxAmount;
                TotalAmount += ioItem.TotalAmount;
                ItemList.Add(ioItem);
            }

            TotalItemCount = ItemList.Count;
        }

    }
    public class StockIOSaveResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        public Newtonsoft.Json.Linq.JObject data { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string RawResponse { get; set; }
    }

    [NotMapped]
    public class StockIOItem : EtimsBaseItemReq
    {
        [Newtonsoft.Json.JsonProperty("totDcAmt")]
        public decimal DiscountAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("itemExprDt")]
        public string ItemExpiredDate { get; set; }

        public StockIOItem()
        {
        }

        /*public StockIOItem(EtimsPurchaseItem pItem)
        {
            ItemSeqNumber = pItem.ItemSeqNumber;
            ItemCode = pItem.ItemCode;
            ItemClassCode = pItem.ItemClassCode;
            ItemTypeCode = pItem.ItemTypeCode;
            ItemName = pItem.ItemName;
            ItemExpiredDate = pItem.ItemExpiredDate;
            Barcode = pItem.Barcode;
            PkgUnitCode = pItem.PkgUnitCode;
            Package = pItem.Package;
            QtyUnitCode = pItem.QtyUnitCode;
            Quantity = pItem.Quantity;
            UnitPrice = pItem.UnitPrice;
            SupplyPrice = pItem.SupplyPrice;
            DiscountAmount = pItem.DiscountAmount;

            TaxableAmount = pItem.TaxableAmount;
            TaxTypeCode = pItem.TaxTypeCode;
            TaxAmount = pItem.TaxAmount;
            TotalAmount = pItem.TotalAmount;
        }*/

        public StockIOItem(EtimsSaleItem sItem)
        {
            ItemSeqNumber = sItem.ItemSeqNumber;
            ItemCode = sItem.ItemCode;
            ItemClassCode = sItem.ItemClassCode;
            ItemTypeCode = sItem.ItemTypeCode;
            ItemName = sItem.ItemName;
            //ItemExpiredDate = sItem.ItemExpiredDate;
            Barcode = sItem.Barcode;
            PkgUnitCode = sItem.PkgUnitCode;
            Package = sItem.Package;
            QtyUnitCode = sItem.QtyUnitCode;
            Quantity = sItem.Quantity;
            UnitPrice = sItem.UnitPrice;
            SupplyPrice = sItem.SupplyPrice;
            DiscountAmount = sItem.DiscountAmount;

            TaxableAmount = sItem.TaxableAmount;
            TaxTypeCode = sItem.TaxTypeCode;
            TaxAmount = sItem.TaxAmount;
            TotalAmount = sItem.TotalAmount;
        }
    }

    public class TrnsPurchaseSaveReq : ETIMSBaseReq
    {

        [Newtonsoft.Json.JsonProperty("invcNo")]
        [System.Text.Json.Serialization.JsonPropertyName("invcNo")]
        public int EtimsInvoiceNum { get; set; }            // Internal
        [Newtonsoft.Json.JsonProperty("orgInvcNo")]
        [System.Text.Json.Serialization.JsonPropertyName("orgInvcNo")]
        public int OriginalEtimsInvNum { get; set; }
        [Newtonsoft.Json.JsonProperty("spplrTin")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrTin")]
        public string SupplierPIN { get; set; }
        [Newtonsoft.Json.JsonProperty("spplrBhfId")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrBhfId")]
        public string SupplierBranchID { get; set; }
        [Newtonsoft.Json.JsonProperty("spplrNm")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrNm")]
        public string SupplierName { get; set; }
        [Newtonsoft.Json.JsonProperty("spplrInvcNo")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrInvcNo")]
        public int SupplierInvoiceNum { get; set; }
        [Newtonsoft.Json.JsonProperty("regTyCd")]
        [System.Text.Json.Serialization.JsonPropertyName("regTyCd")]
        public string RegistrationTypeCode { get; set; }    // 4.12 Transaction Progress
        [Newtonsoft.Json.JsonProperty("pchsTyCd")]
        [System.Text.Json.Serialization.JsonPropertyName("pchsTyCd")]
        public string PurchaseTypeCode { get; set; }        // 4.8 Transaction Type
        [Newtonsoft.Json.JsonProperty("rcptTyCd")]
        [System.Text.Json.Serialization.JsonPropertyName("rcptTyCd")]
        public string ReceiptTypeCode { get; set; }         // 4.13 Purchase Receipt Type
        [Newtonsoft.Json.JsonProperty("pmtTyCd")]
        [System.Text.Json.Serialization.JsonPropertyName("pmtTyCd")]
        public string PaymentTypeCode { get; set; }         // 4.10. Payment Method
        [Newtonsoft.Json.JsonProperty("pchsSttsCd")]
        [System.Text.Json.Serialization.JsonPropertyName("pchsSttsCd")]
        public string PurchaseStatusCode { get; set; }      // 4.11 Transaction Progress
        [Newtonsoft.Json.JsonProperty("cfmDt")]
        [System.Text.Json.Serialization.JsonPropertyName("cfmDt")]
        public string ValidateDate { get; set; }
        [Newtonsoft.Json.JsonProperty("pchsDt")]
        [System.Text.Json.Serialization.JsonPropertyName("pchsDt")]
        public string PurchaseDate { get; set; }
        [Newtonsoft.Json.JsonProperty("wrhsDt")]
        [System.Text.Json.Serialization.JsonPropertyName("wrhsDt")]
        public string WarehousingDate { get; set; }
        [Newtonsoft.Json.JsonProperty("cnclReqDt")]                         // CreditNote posting date
        [System.Text.Json.Serialization.JsonPropertyName("cnclReqDt")]
        public string CancelReqDate { get; set; }
        [Newtonsoft.Json.JsonProperty("cnclDt")]                            // for CreditNoteDate
        [System.Text.Json.Serialization.JsonPropertyName("cnclDt")]
        public string CanceledDate { get; set; }
        [Newtonsoft.Json.JsonProperty("rfdDt")]
        [System.Text.Json.Serialization.JsonPropertyName("rfdDt")]
        public string CreditNoteDate { get; set; }
        [Newtonsoft.Json.JsonProperty("totItemCnt")]
        [System.Text.Json.Serialization.JsonPropertyName("totItemCnt")]
        public int TotalItemCount { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtA")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmtA")]
        public decimal TaxableAmountA { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtB")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmtB")]
        public decimal TaxableAmountB { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtC")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmtC")]
        public decimal TaxableAmountC { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtD")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmtD")]
        public decimal TaxableAmountD { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtE")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmtE")]
        public decimal TaxableAmountE { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtA")]
        [System.Text.Json.Serialization.JsonPropertyName("taxRtA")]
        public decimal TaxRateA { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtB")]
        [System.Text.Json.Serialization.JsonPropertyName("taxRtB")]
        public decimal TaxRateB { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtC")]
        [System.Text.Json.Serialization.JsonPropertyName("taxRtC")]
        public decimal TaxRateC { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtD")]
        [System.Text.Json.Serialization.JsonPropertyName("taxRtD")]
        public decimal TaxRateD { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtE")]
        [System.Text.Json.Serialization.JsonPropertyName("taxRtE")]
        public decimal TaxRateE { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtA")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmtA")]
        public decimal TaxAmountA { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtB")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmtB")]
        public decimal TaxAmountB { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtC")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmtC")]
        public decimal TaxAmountC { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtD")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmtD")]
        public decimal TaxAmountD { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtE")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmtE")]
        public decimal TaxAmountE { get; set; }
        [Newtonsoft.Json.JsonProperty("totTaxblAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totTaxblAmt")]
        public decimal TotalTaxableAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("totTaxAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totTaxAmt")]
        public decimal TotalTaxAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("totAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totAmt")]
        public decimal TotalAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("remark")]
        [System.Text.Json.Serialization.JsonPropertyName("remark")]
        public string Remark { get; set; }
        [Newtonsoft.Json.JsonProperty("regrId")]
        [System.Text.Json.Serialization.JsonPropertyName("regrId")]
        public string RegistrantID { get; set; }
        [Newtonsoft.Json.JsonProperty("regrNm")]
        [System.Text.Json.Serialization.JsonPropertyName("regrNm")]
        public string RegistrantName { get; set; }
        [Newtonsoft.Json.JsonProperty("modrId")]
        [System.Text.Json.Serialization.JsonPropertyName("modrId")]
        public string ModifierID { get; set; }  // Modifier ID
        [Newtonsoft.Json.JsonProperty("modrNm")]
        [System.Text.Json.Serialization.JsonPropertyName("modrNm")]
        public string ModifierName { get; set; }  // Modifier Name

        [Newtonsoft.Json.JsonProperty("itemList")]
        [System.Text.Json.Serialization.JsonPropertyName("itemList")]
        public List<EtimsPurchaseItem> ItemList { get; set; } = new();

        public TrnsPurchaseSaveReq()
        {
        }

        public TrnsPurchaseSaveReq(ClientBranch clientBranch, PurchaseSale purchaseSale)
            : this()
        {
            PIN = clientBranch.TaxClient.TaxNumber;
            EtimsInvoiceNum = clientBranch.PurchInvoiceSeq;
            BranchID = clientBranch.BranchCode;

            // Supplier Details
            SupplierBranchID = purchaseSale.SupplierBranchID;
            SupplierPIN = purchaseSale.SupplierPIN;
            SupplierName = purchaseSale.SupplierName;
            SupplierInvoiceNum = purchaseSale.SupplierInvoiceNum;
            OriginalEtimsInvNum = purchaseSale.SupplierInvoiceNum;

            DateTime? validateDate = null;
            if (!string.IsNullOrWhiteSpace(purchaseSale.ValidateDate))
            {
                validateDate = DateTime.ParseExact(purchaseSale.ValidateDate, ETIMSConst.STRUCT_DATETIME, CultureInfo.InvariantCulture);
            }
            if (validateDate.HasValue)
                ValidateDate = validateDate.Value.ToString(ETIMSConst.FMT_DATETIME);
            PaymentTypeCode = purchaseSale.PaymentTypeCode;
            ReceiptTypeCode = "P";
            Remark = purchaseSale.Remark;
            PurchaseDate = purchaseSale.SalesDate;
            PurchaseTypeCode = ETimsUtils.ConvertTransactionType(DocumentType.INVOICE);

            RegistrantID = ModifierID = "S300ETRBridge";
            RegistrantName= ModifierName = "S300ETRBridge";
            PurchaseStatusCode = ETimsUtils.ConvertTransactionStatus(Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice.InvoiceStatusEnum.Documentcosted);
            RegistrationTypeCode = ETimsUtils.GetRegistrationType(true);

            DateTime wareHseDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(purchaseSale.StockReleaseDate))
                wareHseDate = DateTime.ParseExact(purchaseSale.StockReleaseDate, ETIMSConst.STRUCT_DATETIME, CultureInfo.InvariantCulture);
            else if (!string.IsNullOrWhiteSpace(purchaseSale.SalesDate))
                wareHseDate = DateTime.ParseExact(purchaseSale.SalesDate, ETIMSConst.FMT_DATEONLY, CultureInfo.InvariantCulture);
            WarehousingDate = wareHseDate.ToString(ETIMSConst.FMT_DATETIME);

            TaxableAmountA = purchaseSale.TaxableAmountA;
            TaxableAmountB = purchaseSale.TaxableAmountB;
            TaxableAmountC = purchaseSale.TaxableAmountC;
            TaxableAmountD = purchaseSale.TaxableAmountD;

            TaxAmountA = purchaseSale.TaxAmountA;
            TaxAmountB = purchaseSale.TaxAmountB;
            TaxAmountC = purchaseSale.TaxAmountC;
            TaxAmountD = purchaseSale.TaxAmountD;

            TaxRateA = purchaseSale.TaxRateA;
            TaxRateB = purchaseSale.TaxRateB;
            TaxRateC = purchaseSale.TaxRateC;
            TaxRateD = purchaseSale.TaxRateD;

            TotalTaxAmount = purchaseSale.TotalAmount;
            TotalTaxableAmount = purchaseSale.TotalTaxableAmount;

            foreach (var item in purchaseSale.PurchSalesItems)
            {
                //var purchaseItem = new EtimsPurchaseItem(pInvoice, item, taxGroup, taxAuthKeys);
                var purchaseItem = new EtimsPurchaseItem(item);
                ItemList.Add(purchaseItem);
            }

            TotalItemCount = ItemList.Count;

        }

        /*public TrnsPurchaseSaveReq(VSCUConfig config, Sage.CA.SBS.ERP.Sage300.PO.WebApi.Models.Invoice pInvoice,
            S300TaxGroup taxGroup, HashSet<string> taxAuthKeys,
            Sage.CA.SBS.ERP.Sage300.AP.WebApi.Models.Vendor vendor)
            : base()
        {
            PIN = config.PINNumber;

            //SupplierInvoiceNum = pInvoice.InvoiceNumber;
            //SupplierBranchID = "00";

            RegistrationTypeCode = ETimsUtils.GetRegistrationType();
            PurchaseTypeCode = ETimsUtils.ConvertTransactionType(DocumentType.INVOICE);
            ReceiptTypeCode = ETimsUtils.GetPurchseReceiptType();
            PaymentTypeCode = ETimsUtils.ConvertPaymentMethod(pInvoice.Payments);
            if (pInvoice.ReceiptDate.HasValue)
                WarehousingDate = pInvoice.ReceiptDate.Value.ToString(ETIMSConst.FMT_DATETIME);
            PurchaseDate = pInvoice.InvoiceDate.Value.ToString(ETIMSConst.FMT_DATEONLY);
            PurchaseStatusCode = "02";

            // Creator / Modifier
            ModifierID = ModifierName = pInvoice.EnteredBy;

            Remark = pInvoice.Description;
            //RegistrantID = invoice.Salesperson1;
            //RegistrantName = invoice.SalespersonName1;
            if (string.IsNullOrWhiteSpace(RegistrantID))
            {
                RegistrantID = RegistrantName = pInvoice.EnteredBy;
            }
            *//*if (vendor is not null)
            {
                SupplierName = vendor.VendorName.Trim();
                SupplierPIN = vendor.TaxRegistrationCode1.Trim();
            }*//*

            foreach (var item in pInvoice.InvoiceLines)
            {
                var purchaseItem = new EtimsPurchaseItem(pInvoice, item, taxGroup, taxAuthKeys);

                TotalAmount += purchaseItem.TotalAmount;

                switch (purchaseItem.TaxTypeCode)
                {
                    case "A":
                        {
                            TaxableAmountA += purchaseItem.TaxableAmount;
                            TaxAmountA += purchaseItem.TaxAmount;
                        }
                        break;
                    case "B":
                        {
                            TaxableAmountB += purchaseItem.TaxableAmount;
                            TaxAmountB += purchaseItem.TaxAmount;
                        }
                        break;
                    case "C":
                        {
                            TaxableAmountC += purchaseItem.TaxableAmount;
                            TaxAmountC += purchaseItem.TaxAmount;
                        }
                        break;
                    case "D":
                        {
                            TaxableAmountD += purchaseItem.TaxableAmount;
                            TaxAmountD += purchaseItem.TaxAmount;
                        }
                        break;
                    case "E":
                        {
                            TaxableAmountE += purchaseItem.TaxableAmount;
                            TaxAmountE += purchaseItem.TaxAmount;
                        }
                        break;
                }

                ItemList.Add(purchaseItem);
            }

            TotalItemCount = ItemList.Count;

            TotalTaxAmount = TaxAmountA + TaxAmountB + TaxAmountC + TaxAmountD + TaxAmountE;
            TotalTaxableAmount = TaxableAmountA + TaxableAmountB + TaxableAmountC + TaxableAmountD + TaxableAmountE;
        }*/
    }
    public class EtimsPurchaseItem : EtimsBaseItemReq
    {
        [Newtonsoft.Json.JsonProperty("spplrItemClsCd")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrItemClsCd")]
        public string SupplierItemClassCode { get; set; }
        [Newtonsoft.Json.JsonProperty("spplrItemCd")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrItemCd")]
        public string SupplierItemCode { get; set; }
        [Newtonsoft.Json.JsonProperty("spplrItemNm")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrItemNm")]
        public string SupplierItemName { get; set; }
        [Newtonsoft.Json.JsonProperty("dcRt")]
        [System.Text.Json.Serialization.JsonPropertyName("dcRt")]
        public decimal DiscountRate { get; set; }
        [Newtonsoft.Json.JsonProperty("dcAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("dcAmt")]
        public decimal DiscountAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("itemExprDt")]
        [System.Text.Json.Serialization.JsonPropertyName("itemExprDt")]
        public string ItemExpiredDate { get; set; }

        public EtimsPurchaseItem()
        {
        }

        public EtimsPurchaseItem(PurchSaleItem item)
            : this()
        {
            SupplierItemCode = item.ItemCode;
            SupplierItemClassCode = item.ItemClassCode;
            SupplierItemName = item.ItemName;

            ItemSeqNumber = item.ItemSeqNumber;
            ItemClassCode = item.ItemClassCode;
            ItemCode = item.ItemCode;
            ItemName = item.ItemName;
            Package = item.Package;
            PkgUnitCode = item.PkgUnitCode;
            Quantity = item.Quantity;
            QtyUnitCode = item.QtyUnitCode;

            if (Quantity > 0)
                ItemTypeCode = "2";     // Finished Product
            else
                ItemTypeCode = "3";     // Service without stock

            TaxTypeCode = item.TaxTypeCode;
            TaxableAmount = item.TaxableAmount;
            TaxAmount = item.TaxAmount;
            TotalAmount = item.TotalAmount;

            DiscountRate = item.DiscountRate;
            DiscountAmount = item.DiscountAmount;
        }

        /*public EtimsPurchaseItem(Sage.CA.SBS.ERP.Sage300.PO.WebApi.Models.Invoice pInvoice, Sage.CA.SBS.ERP.Sage300.PO.WebApi.Models.InvoiceLine item, S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
            : base()
        {
            ItemSeqNumber = -1; // Internal
            _icItemNumber = item.ItemNumber;

            ItemCode = item.ItemNumber;
            ItemName = item.ItemDescription;
            SupplierItemName = ItemName;
            ItemTypeCode = "2";

            _pkgUnitCode = item.UnitOfMeasure;
            if (string.IsNullOrWhiteSpace(_pkgUnitCode))
            {
                _pkgUnitCode = "Service";
            }
            Package = item.StockingQuantityReceived;

            _qtyUnitCode = item.RCPUNIT;
            Quantity = item.QuantityReceived;

            SupplyPrice = item.ExtendedCost;
            UnitPrice = (item.ExtendedCost != 0) ? item.ExtendedCost : item.PaymentDiscountBaseWithoutTa;
            if (SupplyPrice == 0)
                SupplyPrice = UnitPrice;

            decimal amtValue = UnitPrice;
            UnitPrice = SupplyPrice = amtValue;

            if (!string.IsNullOrWhiteSpace(pInvoice.TaxAuthority1) && taxAuthKeys.Contains(pInvoice.TaxAuthority1))
            {
                TaxAmount += item.TaxAmount1;
                if (string.IsNullOrWhiteSpace(TaxTypeCode))
                {
                    var tClass = taxGroup.Authorities[pInvoice.TaxAuthority1].Classes.FirstOrDefault(c => c.ClassKey == pInvoice.TaxClass1 && c.TransactionType == Enum.GetName(Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxClass.TransactionTypeEnum.Sales));
                    if (tClass is null)
                        throw new Exception($"AR Item {item.ItemNumber} has an invalid TaxClass: {pInvoice.TaxClass1}");
                    var tRate = taxGroup.Authorities[pInvoice.TaxAuthority1].Rates.FirstOrDefault(r => r.ItemRate1 == item.TaxRate1);
                    if (tRate is null)
                        throw new Exception($"AR Item {item.ItemNumber} has an invalid Rate: {item.TaxRate1}");
                    TaxTypeCode = ETimsUtils.GetTaxRate(tClass, tRate);
                }
            }
            else
            {
                //TODO: throw error if TaxAuthority1 invalid
            }
            if (!string.IsNullOrWhiteSpace(pInvoice.TaxAuthority2) && taxAuthKeys.Contains(pInvoice.TaxAuthority2))
            {
                TaxAmount += item.TaxAmount2;
            }
            if (!string.IsNullOrWhiteSpace(pInvoice.TaxAuthority3) && taxAuthKeys.Contains(pInvoice.TaxAuthority3))
            {
                TaxAmount += item.TaxAmount3;
            }
            if (!string.IsNullOrWhiteSpace(pInvoice.TaxAuthority4) && taxAuthKeys.Contains(pInvoice.TaxAuthority4))
            {
                TaxAmount += item.TaxAmount4;
            }
            if (!string.IsNullOrWhiteSpace(pInvoice.TaxAuthority5) && taxAuthKeys.Contains(pInvoice.TaxAuthority5))
            {
                TaxAmount += item.TaxAmount5;
            }

            TaxableAmount = item.TaxBase1;
            //TaxAmount = item.TaxAmount1;
            //TotalAmount = UnitPrice * Quantity;
            TotalAmount = TaxableAmount + TaxAmount;
            if (TotalAmount == 0)
                TotalAmount = TaxableAmount = UnitPrice;

            DiscountRate = 0;
            DiscountAmount = 0;
        }*/
    }

    public class TrnsPurchaseSaveResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public JObject Data { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string RawResponse { get; set; }
    }

    // Save Stock Master
    public class SaveStockLevel
    {
        [StringLength(2)]
        public string BranchCode { get; set; } = "00";
        [StringLength(32)]
        public string ProductCode { get; set; }
        public decimal StockLevel { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(BranchCode) && !string.IsNullOrWhiteSpace(ProductCode) && StockLevel > 0;
        }
    }
    public class BranchStockLevel
    {
        public string BranchCode { get; set; } = "00";
        public string ProductCode { get; set; }
        public DateTime? LastChecked { get; set; }
    }
    public class StockMstSaveReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("itemCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemCd")]
        public string ItemCode { get; set; }
        [Newtonsoft.Json.JsonProperty("rsdQty")]
        [System.Text.Json.Serialization.JsonPropertyName("rsdQty")]
        public decimal RemainQuantity { get; set; }

        [Newtonsoft.Json.JsonProperty("regrNm")]
        [System.Text.Json.Serialization.JsonPropertyName("regrNm")]
        public string RegistrantName { get; set; } = ETIMSConst.DEFAULT_USER;
        [Newtonsoft.Json.JsonProperty("regrId")]
        [System.Text.Json.Serialization.JsonPropertyName("regrId")]
        public string RegistrantID { get; set; } = ETIMSConst.DEFAULT_USER;
        [Newtonsoft.Json.JsonProperty("modrNm")]
        [System.Text.Json.Serialization.JsonPropertyName("modrNm")]
        public string ModifierName { get; set; } = ETIMSConst.DEFAULT_USER;
        [Newtonsoft.Json.JsonProperty("modrId")]
        [System.Text.Json.Serialization.JsonPropertyName("modrId")]
        public string ModifierID { get; set; } = ETIMSConst.DEFAULT_USER;

        public StockMstSaveReq()
        {            
        }
        public StockMstSaveReq(ClientBranch clientBranch, StockItem stockItem, SaveStockLevel saveStockLevel)
            : this()
        {
            PIN = clientBranch.TaxClient.TaxNumber;
            ItemCode = stockItem.TaxItemCode;
            RemainQuantity = saveStockLevel.StockLevel;
        }
    }
    public class StockMstSaveResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public JObject Data { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string RawResponse { get; set; }
    }

}
