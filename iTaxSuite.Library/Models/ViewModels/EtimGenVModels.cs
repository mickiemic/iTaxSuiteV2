using iTaxSuite.Library.Models.Entities;

namespace iTaxSuite.Library.Models.ViewModels
{
    // Setup Models
    public class SelectBranchReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("lastReqDt")]
        public string LastRequest { get; set; } = "20191130000000";
        public SelectBranchReq()
        {
        }
        public SelectBranchReq(ClientBranch clientBranch)
            : this()
        {
            if (string.IsNullOrEmpty(clientBranch?.TaxClient?.TaxNumber))
                throw new Exception("TaxClient or TaxNumber invalid");
            PIN = clientBranch.TaxClient.TaxNumber;
        }
    }
    public class SelectBranchResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        public EBranchesWrapper Data { get; set; }
    }
    public class EBranchesWrapper
    {
        [Newtonsoft.Json.JsonProperty("bhfList")]
        public List<EtimsBranchItem> Branches { get; set; }
    }
    public class EtimsBranchItem
    {
        public string tin { get; set; }
        public string bhfId { get; set; }
        public string bhfNm { get; set; }
        public string bhfSttsCd { get; set; }
        public string prvncNm { get; set; }
        public string dstrtNm { get; set; }
        public string sctrNm { get; set; }
        public object locDesc { get; set; }
        public string mgrNm { get; set; }
        public string mgrTelNo { get; set; }
        public string mgrEmail { get; set; }
        public string hqYn { get; set; }
    }
    public class SelectItemReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("lastReqDt")]
        public string LastRequest { get; set; }
        public SelectItemReq()
        {
        }
        public SelectItemReq(ClientBranch clientBranch)
            : this()
        {
            if (string.IsNullOrEmpty(clientBranch?.TaxClient?.TaxNumber))
                throw new Exception("TaxClient or TaxNumber invalid");
            PIN = clientBranch.TaxClient.TaxNumber;
        }
    }
    public class SelectItemResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        public EItemsWrapper Data { get; set; }

        public int GetItemCount()
        {
            if (Data == null || Data.ItemList == null)
                return -2;
            return Data.ItemList.Count;
        }
    }
    public class EItemsWrapper
    {
        [Newtonsoft.Json.JsonProperty("itemList")]
        public List<ETimsItem> ItemList { get; set; } = new();
    }
    public class ETimsItem
    {

        [Newtonsoft.Json.JsonProperty("tin")]
        public string PIN { get; set; }
        [Newtonsoft.Json.JsonProperty("itemCd")]
        public string ItemCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemClsCd")]
        public string ItemClassCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemTyCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemTyCd")]
        public string ItemTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemNm")]
        public string ItemName { get; set; }
        public string itemStdNm { get; set; }
        public string orgnNatCd { get; set; }
        [Newtonsoft.Json.JsonProperty("pkgUnitCd")]
        public string PkgUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("qtyUnitCd")]
        public string QtyUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("taxTyCd")]
        public string TaxTypeCode { get; set; }
        public string btchNo { get; set; }
        public string regBhfId { get; set; }
        [Newtonsoft.Json.JsonProperty("bcd")]
        public string Barcode { get; set; }
        public double dftPrc { get; set; }
        public double grpPrcL1 { get; set; }
        public double grpPrcL2 { get; set; }
        public double grpPrcL3 { get; set; }
        public double grpPrcL4 { get; set; }
        public double grpPrcL5 { get; set; }
        public string addInfo { get; set; }
        public double sftyQty { get; set; }
        public string isrcAplcbYn { get; set; }
        public string rraModYn { get; set; }
        public string useYn { get; set; }
    }

    // Notices Models
    public class NoticeReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("lastReqDt")]
        [System.Text.Json.Serialization.JsonPropertyName("lastReqDt")]
        public string LastRequest { get; set; }
    }
    public class NoticeResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        public NoticeWrapper NoticeData { get; set; }
    }
    public class NoticeWrapper
    {
        [Newtonsoft.Json.JsonProperty("noticeList")]
        public List<NoticeItem> NoticeList { get; set; }
    }
    public class NoticeItem
    {
        [Newtonsoft.Json.JsonProperty("noticeNo")]
        [System.Text.Json.Serialization.JsonPropertyName("noticeNo")]
        public int NoticeNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("regrtitleNm")]
        [System.Text.Json.Serialization.JsonPropertyName("title")]
        public string Title { get; set; }
        [Newtonsoft.Json.JsonProperty("cont")]
        [System.Text.Json.Serialization.JsonPropertyName("cont")]
        public string Content { get; set; }
        [Newtonsoft.Json.JsonProperty("dtlUrl")]
        [System.Text.Json.Serialization.JsonPropertyName("dtlUrl")]
        public string DetailURL { get; set; }
        [Newtonsoft.Json.JsonProperty("regrNm")]
        [System.Text.Json.Serialization.JsonPropertyName("regrNm")]
        public string RegistrantName { get; set; }
        [Newtonsoft.Json.JsonProperty("regDt")]
        [System.Text.Json.Serialization.JsonPropertyName("regDt")]
        public string RegistrationDateTime { get; set; }
    }

}
