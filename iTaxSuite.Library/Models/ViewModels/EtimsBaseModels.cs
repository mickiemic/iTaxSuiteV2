using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace iTaxSuite.Library.Models.ViewModels
{
    public enum DocumentType
    {
        [EnumMember(Value = "ALL")]
        ALLDOCS = -1,
        [EnumMember(Value = "INVOICE")]
        INVOICE,
        [EnumMember(Value = "CREDITNOTE")]
        CREDITNOTE,
        [EnumMember(Value = "DEBITNOTE")]
        DEBITNOTE
    }
    public enum StockMovementType
    {
        // INCOMING STOCK
        [EnumMember(Value = "01")]  // Incoming-Import
        Import = 1,
        [EnumMember(Value = "02")]  // Incoming-Purchase
        Purchase = 2,
        [EnumMember(Value = "03")]  // Incoming-Return
        ReturnInwards = 3,
        [EnumMember(Value = "04")]  // Incoming-Stock Movement
        StockMovement = 4,
        [EnumMember(Value = "05")]  // Incoming-Processing
        IncomingProcessing = 5,
        [EnumMember(Value = "06")]  // Incoming-Adjustment
        IncomingAdjustment = 6,
        // OUTGOING STOCK
        [EnumMember(Value = "11")]  // Outgoing-Sale
        Sale = 11,
        [EnumMember(Value = "13")]  // Outgoing-Return
        ReturnOutwards = 12,
        [EnumMember(Value = "13")]  // Outgoing-Stock Movement
        OutgoingStock = 13,
        [EnumMember(Value = "14")]  // Outgoing-Processing
        Processing = 14,
        [EnumMember(Value = "15")]  // Outgoing-Discarding
        Discarding = 15,
        [EnumMember(Value = "16")]  // Outgoing-Adjustment
        Adjustment = 16
    }

    public enum ETRReverseReason
    {
        [EnumMember(Value = "01")]
        MissingQuantity = 1,
        [EnumMember(Value = "02")]
        MissingItem = 2,
        [EnumMember(Value = "03")]
        Damaged = 3,
        [EnumMember(Value = "04")]
        WastedWasted = 4,
        [EnumMember(Value = "05")]
        RawMaterialShortage = 5,
        [EnumMember(Value = "06")]
        Refund = 6,
        [EnumMember(Value = "07")]
        WrongCustomerPIN = 7,
        [EnumMember(Value = "08")]
        WrongCustomerName = 8,
        [EnumMember(Value = "09")]
        WrongAmountORPrice = 9,
        [EnumMember(Value = "10")]
        WrongQuantity = 10,
        [EnumMember(Value = "11")]
        WrongItems = 11,
        [EnumMember(Value = "12")]
        WrongTaxType = 12
    }

    public enum ETRImportItemStatus
    {
        [EnumMember(Value = "1")]
        Unsent = 1,
        [EnumMember(Value = "2")]
        Waiting = 2,
        [EnumMember(Value = "3")]
        Approved = 3,
        [EnumMember(Value = "4")]
        Cancelled = 4
    }

    public enum ETIMSReqType
    {
        NONE = 0,
        CREATE_ITEMS = 101,
        SAVE_SALE = 201,
        SAVE_PURCHASE = 301,
        SAVE_STOCKIO = 401,
        SAVE_STOCKMASTER = 501
    }

    public abstract class ETIMSBaseReq
    {
        [Required]
        [StringLength(11)]
        [Newtonsoft.Json.JsonProperty("tin")]
        [System.Text.Json.Serialization.JsonPropertyName("tin")]
        public string PIN { get; set; }
        [Required]
        [StringLength(2)]
        [Newtonsoft.Json.JsonProperty("bhfId")]
        [System.Text.Json.Serialization.JsonPropertyName("bhfId")]
        public string BranchID { get; set; } = "00";
    }

    public abstract class ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("resultCd")]
        [System.Text.Json.Serialization.JsonPropertyName("resultCd")]
        public string ResultCode { get; set; }
        [Newtonsoft.Json.JsonProperty("resultMsg")]
        [System.Text.Json.Serialization.JsonPropertyName("resultMsg")]
        public string ResultMsg { get; set; }
        [Newtonsoft.Json.JsonProperty("resultDt")]
        [System.Text.Json.Serialization.JsonPropertyName("resultDt")]
        public string ResultDate { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsSuccess => (ResultCode == "000");
    }

    public abstract class EtimsBaseItemReq
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string _icItemNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("itemSeq")]
        [System.Text.Json.Serialization.JsonPropertyName("itemSeq")]
        public int ItemSeqNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("itemCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemCd")]
        public string ItemCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemTyCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemTyCd")]
        public string ItemTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemClsCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemClsCd")]
        public string ItemClassCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemNm")]
        [System.Text.Json.Serialization.JsonPropertyName("itemNm")]
        public string ItemName { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string _pkgUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("pkgUnitCd")]
        [System.Text.Json.Serialization.JsonPropertyName("pkgUnitCd")]
        public string PkgUnitCode { get; set; }             // 4.5. Packaging Unit
        [Newtonsoft.Json.JsonProperty("pkg")]
        [System.Text.Json.Serialization.JsonPropertyName("pkg")]
        public decimal Package { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string _qtyUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("qtyUnitCd")]
        [System.Text.Json.Serialization.JsonPropertyName("qtyUnitCd")]
        public string QtyUnitCode { get; set; }             // 4.6. Unit of Quantity
        [Newtonsoft.Json.JsonProperty("qty")]
        [System.Text.Json.Serialization.JsonPropertyName("qty")]
        public decimal Quantity { get; set; }
        [Newtonsoft.Json.JsonProperty("bcd")]
        [System.Text.Json.Serialization.JsonPropertyName("bcd")]
        public string Barcode { get; set; }
        [Newtonsoft.Json.JsonProperty("taxTyCd")]           // Tax-Class / Tax-Rate Convert
        [System.Text.Json.Serialization.JsonPropertyName("taxTyCd")]
        public string TaxTypeCode { get; set; }             // 4.1. Tax Type
        [Newtonsoft.Json.JsonProperty("prc")]
        [System.Text.Json.Serialization.JsonPropertyName("prc")]
        public decimal UnitPrice { get; set; }
        [Newtonsoft.Json.JsonProperty("splyAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("splyAmt")]
        public decimal SupplyPrice { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmt")]
        public decimal TaxableAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmt")]
        public decimal TaxAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("totAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totAmt")]
        public decimal TotalAmount { get; set; }
    }

}
