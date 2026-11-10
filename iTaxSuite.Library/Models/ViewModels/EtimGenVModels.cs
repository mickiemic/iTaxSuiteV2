using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Models.Entities;
using Newtonsoft.Json.Linq;

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
        public string locDesc { get; set; }
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
        [System.Text.Json.Serialization.JsonPropertyName("data")]
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
        [System.Text.Json.Serialization.JsonPropertyName("itemList")]
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
        public string ItemTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemNm")]
        public string ItemName { get; set; }
        [Newtonsoft.Json.JsonProperty("itemStdNm")]
        public string ItemStdName { get; set; }
        [Newtonsoft.Json.JsonProperty("orgnNatCd")]
        public string OriginNatCode { get; set; }
        [Newtonsoft.Json.JsonProperty("pkgUnitCd")]
        public string PkgUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("qtyUnitCd")]
        public string QtyUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("taxTyCd")]
        public string TaxTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("btchNo")]
        public string BatchNo { get; set; }
        [Newtonsoft.Json.JsonProperty("regBhfId")]
        public string RegBranchID { get; set; }
        [Newtonsoft.Json.JsonProperty("bcd")]
        public string Barcode { get; set; }
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
        public char InsuranceAplcb { get; set; }
        public string rraModYn { get; set; }
        [Newtonsoft.Json.JsonProperty("useYn")]
        public char Use { get; set; }
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

    // Codes List
    public class SelectCodeReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("lastReqDt")]
        public string LastRequest { get; set; }
        public SelectCodeReq()
        {
        }
        public SelectCodeReq(ClientBranch clientBranch)
            : this()
        {
            if (string.IsNullOrEmpty(clientBranch?.TaxClient?.TaxNumber))
                throw new Exception("TaxClient or TaxNumber invalid");
            PIN = clientBranch.TaxClient.TaxNumber;
        }
    }
    public class SelectCodeResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public ECodesWrapper Data { get; set; }
    }
    public class ECodesWrapper
    {
        [Newtonsoft.Json.JsonProperty("clsList")]
        [System.Text.Json.Serialization.JsonPropertyName("clsList")]
        public List<CodeHeader> CodesList { get; set; } = new();
    }
    public class CodeHeader
    {
        [Newtonsoft.Json.JsonProperty("cdCls")]
        [System.Text.Json.Serialization.JsonPropertyName("cdCls")]
        public string CodeClass { get; set; }
        [Newtonsoft.Json.JsonProperty("cdClsNm")]
        [System.Text.Json.Serialization.JsonPropertyName("cdClsNm")]
        public string CodeClassName { get; set; }
        [Newtonsoft.Json.JsonProperty("cdClsDesc")]
        [System.Text.Json.Serialization.JsonPropertyName("cdClsDesc")]
        public string ClassDescription { get; set; }
        [Newtonsoft.Json.JsonProperty("useYn")]
        [System.Text.Json.Serialization.JsonPropertyName("useYn")]
        public string IsActive { get; set; }
        [Newtonsoft.Json.JsonProperty("userDfnNm1")]
        [System.Text.Json.Serialization.JsonPropertyName("userDfnNm1")]
        public string UserDefinedName1 { get; set; }
        [Newtonsoft.Json.JsonProperty("userDfnNm2")]
        [System.Text.Json.Serialization.JsonPropertyName("userDfnNm2")]
        public string UserDefinedName2 { get; set; }
        [Newtonsoft.Json.JsonProperty("userDfnNm3")]
        [System.Text.Json.Serialization.JsonPropertyName("userDfnNm3")]
        public string UserDefinedName3 { get; set; }
        [Newtonsoft.Json.JsonProperty("dtlList")]
        [System.Text.Json.Serialization.JsonPropertyName("dtlList")]
        public List<CodeItem> CodeList { get; set; }
    }
    public class CodeItem
    {
        [Newtonsoft.Json.JsonProperty("cd")]
        [System.Text.Json.Serialization.JsonPropertyName("cd")]
        public string Code { get; set; }

        [Newtonsoft.Json.JsonProperty("cdNm")]
        [System.Text.Json.Serialization.JsonPropertyName("cdNm")]
        public string CodeName { get; set; }

        [Newtonsoft.Json.JsonProperty("cdDesc")]
        [System.Text.Json.Serialization.JsonPropertyName("cdDesc")]
        public string CodeDescription { get; set; }

        [Newtonsoft.Json.JsonProperty("useYn")]
        [System.Text.Json.Serialization.JsonPropertyName("useYn")]
        public string IsActive { get; set; }

        [Newtonsoft.Json.JsonProperty("srtOrd")]
        [System.Text.Json.Serialization.JsonPropertyName("srtOrd")]
        public int SortOrder { get; set; }

        [Newtonsoft.Json.JsonProperty("userDfnCd1")]
        [System.Text.Json.Serialization.JsonPropertyName("userDfnCd1")]
        public string UserDefinedCode1 { get; set; }

        [Newtonsoft.Json.JsonProperty("userDfnCd2")]
        [System.Text.Json.Serialization.JsonPropertyName("userDfnCd2")]
        public string UserDefinedCode2 { get; set; }

        [Newtonsoft.Json.JsonProperty("userDfnCd3")]
        [System.Text.Json.Serialization.JsonPropertyName("userDfnCd3")]
        public string UserDefinedCode3 { get; set; }
    }

    //Item Class List
    public class SelectItemClassReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("lastReqDt")]
        public string LastRequest { get; set; }
        public SelectItemClassReq()
        {
        }
        public SelectItemClassReq(ClientBranch clientBranch)
            : this()
        {
            if (string.IsNullOrEmpty(clientBranch?.TaxClient?.TaxNumber))
                throw new Exception("TaxClient or TaxNumber invalid");
            PIN = clientBranch.TaxClient.TaxNumber;
        }
    }

    public class SelectItemClassResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public ItemClassWrapper Data { get; set; }
    }
    public class ItemClassWrapper
    {
        [Newtonsoft.Json.JsonProperty("itemClsList")]
        [System.Text.Json.Serialization.JsonPropertyName("itemClsList")]
        public List<ItemClassCode> ItemClassList { get; set; }
    }
    public class ItemClassCode
    {
        [Newtonsoft.Json.JsonProperty("itemClsCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemClsCd")]
        public string ClassCode { get; set; }

        [Newtonsoft.Json.JsonProperty("itemClsNm")]
        [System.Text.Json.Serialization.JsonPropertyName("itemClsNm")]
        public string ClassName { get; set; }

        [Newtonsoft.Json.JsonProperty("itemClsLvl")]
        [System.Text.Json.Serialization.JsonPropertyName("itemClsLvl")]
        public int ClassLevel { get; set; }

        [Newtonsoft.Json.JsonProperty("taxTyCd")]
        [System.Text.Json.Serialization.JsonPropertyName("taxTyCd")]
        public string TaxTypeCode { get; set; }

        [Newtonsoft.Json.JsonProperty("mjrTgYn")]
        [System.Text.Json.Serialization.JsonPropertyName("mjrTgYn")]
        public string IsMajorTarget { get; set; }

        [Newtonsoft.Json.JsonProperty("useYn")]
        [System.Text.Json.Serialization.JsonPropertyName("useYn")]
        public string IsActive { get; set; }
    }

    // Customer List
    public class SelectCustomerReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("custmTin")]
        public string CustomerPIN { get; set; }
        public SelectCustomerReq()
        {
        }
        public SelectCustomerReq(ClientBranch clientBranch)
            : this()
        {
            if (string.IsNullOrEmpty(clientBranch?.TaxClient?.TaxNumber))
                throw new Exception("TaxClient or TaxNumber invalid");
            PIN = clientBranch.TaxClient.TaxNumber;
        }
    }
    public class SelectCustomerResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public ECustomersWrapper Data { get; set; }
    }
    public class ECustomersWrapper
    {
        [Newtonsoft.Json.JsonProperty("custList")]
        [System.Text.Json.Serialization.JsonPropertyName("custList")]
        public List<VSCUCustomer> custList { get; set; }
    }
    public class VSCUCustomer
    {
        [Newtonsoft.Json.JsonProperty("tin")]
        [System.Text.Json.Serialization.JsonPropertyName("tin")]
        public string tin { get; set; }

        [Newtonsoft.Json.JsonProperty("taxprNm")]
        [System.Text.Json.Serialization.JsonPropertyName("taxprNm")]
        public string taxprNm { get; set; }

        [Newtonsoft.Json.JsonProperty("taxprSttsCd")]
        [System.Text.Json.Serialization.JsonPropertyName("taxprSttsCd")]
        public string taxprSttsCd { get; set; }

        [Newtonsoft.Json.JsonProperty("prvncNm")]
        [System.Text.Json.Serialization.JsonPropertyName("prvncNm")]
        public string prvncNm { get; set; }

        [Newtonsoft.Json.JsonProperty("dstrtNm")]
        [System.Text.Json.Serialization.JsonPropertyName("dstrtNm")]
        public string dstrtNm { get; set; }

        [Newtonsoft.Json.JsonProperty("sctrNm")]
        [System.Text.Json.Serialization.JsonPropertyName("sctrNm")]
        public string sctrNm { get; set; }

        [Newtonsoft.Json.JsonProperty("locDesc")]
        [System.Text.Json.Serialization.JsonPropertyName("locDesc")]
        public string locDesc { get; set; }
    }

    // Save Branch Customer
    public class SaveBranchCustomerReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("custNo")]
        [System.Text.Json.Serialization.JsonPropertyName("custNo")]
        public string CustNo { get; set; }

        [Newtonsoft.Json.JsonProperty("custTin")]
        [System.Text.Json.Serialization.JsonPropertyName("custTin")]
        public string CustTin { get; set; }

        [Newtonsoft.Json.JsonProperty("custNm")]
        [System.Text.Json.Serialization.JsonPropertyName("custNm")]
        public string CustNm { get; set; }

        [Newtonsoft.Json.JsonProperty("adrs")]
        [System.Text.Json.Serialization.JsonPropertyName("adrs")]
        public string Adrs { get; set; }

        [Newtonsoft.Json.JsonProperty("telNo")]
        [System.Text.Json.Serialization.JsonPropertyName("telNo")]
        public string TelNo { get; set; }

        [Newtonsoft.Json.JsonProperty("email")]
        [System.Text.Json.Serialization.JsonPropertyName("email")]
        public string Email { get; set; }

        [Newtonsoft.Json.JsonProperty("faxNo")]
        [System.Text.Json.Serialization.JsonPropertyName("faxNo")]
        public string FaxNo { get; set; }

        [Newtonsoft.Json.JsonProperty("useYn")]
        [System.Text.Json.Serialization.JsonPropertyName("useYn")]
        public string UseYn { get; set; }

        [Newtonsoft.Json.JsonProperty("remark")]
        [System.Text.Json.Serialization.JsonPropertyName("remark")]
        public string Remark { get; set; }

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
    }
    public class SaveBranchCustomerResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public JObject Data { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string RawResponse { get; set; }
    }

    // Save Branch Users
    public class SaveBranchUserReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("userId")]
        [System.Text.Json.Serialization.JsonPropertyName("userId")]
        public string UserId { get; set; }

        [Newtonsoft.Json.JsonProperty("userNm")]
        [System.Text.Json.Serialization.JsonPropertyName("userNm")]
        public string UserNm { get; set; }

        [Newtonsoft.Json.JsonProperty("pwd")]
        [System.Text.Json.Serialization.JsonPropertyName("pwd")]
        public string Pwd { get; set; }

        [Newtonsoft.Json.JsonProperty("adrs")]
        [System.Text.Json.Serialization.JsonPropertyName("adrs")]
        public string Adrs { get; set; }

        [Newtonsoft.Json.JsonProperty("cntc")]
        [System.Text.Json.Serialization.JsonPropertyName("cntc")]
        public string Cntc { get; set; }

        [Newtonsoft.Json.JsonProperty("authCd")]
        [System.Text.Json.Serialization.JsonPropertyName("authCd")]
        public string AuthCd { get; set; }

        [Newtonsoft.Json.JsonProperty("remark")]
        [System.Text.Json.Serialization.JsonPropertyName("remark")]
        public string Remark { get; set; }

        [Newtonsoft.Json.JsonProperty("useYn")]
        [System.Text.Json.Serialization.JsonPropertyName("useYn")]
        public string UseYn { get; set; }

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
    }
    public class SaveBranchUserResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public JObject Data { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string RawResponse { get; set; }
    }

    // Save Branch Insurance
    public class SaveBranchInsurReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("isrccCd")]
        [System.Text.Json.Serialization.JsonPropertyName("isrccCd")]
        public string IsrccCd { get; set; }

        [Newtonsoft.Json.JsonProperty("isrccNm")]
        [System.Text.Json.Serialization.JsonPropertyName("isrccNm")]
        public string IsrccNm { get; set; }

        [Newtonsoft.Json.JsonProperty("isrcRt")]
        [System.Text.Json.Serialization.JsonPropertyName("isrcRt")]
        public int IsrcRt { get; set; }

        [Newtonsoft.Json.JsonProperty("useYn")]
        [System.Text.Json.Serialization.JsonPropertyName("useYn")]
        public string UseYn { get; set; }

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
    }
    public class SaveBranchInsurResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public JObject Data { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string RawResponse { get; set; }
    }

    // Import Items
    public class SelectImportItemReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("lastReqDt")]
        public string LastRequest { get; set; }
        public SelectImportItemReq()
        {
        }
        public SelectImportItemReq(ClientBranch clientBranch)
            : this()
        {
            if (string.IsNullOrEmpty(clientBranch?.TaxClient?.TaxNumber))
                throw new Exception("TaxClient or TaxNumber invalid");
            PIN = clientBranch.TaxClient.TaxNumber;
        }
    }
    public class SelectImportItemResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public ImportItemWrapper Data { get; set; }
    }
    public class ImportItemWrapper
    {
        [Newtonsoft.Json.JsonProperty("itemList")]
        [System.Text.Json.Serialization.JsonPropertyName("itemList")]
        public List<ImportItem> ItemList { get; set; }
    }
    public class ImportItem
    {
        [Newtonsoft.Json.JsonProperty("taskCd")]
        [System.Text.Json.Serialization.JsonPropertyName("taskCd")]
        public string TaskCode { get; set; }

        [Newtonsoft.Json.JsonProperty("dclNo")]
        [System.Text.Json.Serialization.JsonPropertyName("dclNo")]
        public string DeclarationNumber { get; set; }

        [Newtonsoft.Json.JsonProperty("dclDe")]
        [System.Text.Json.Serialization.JsonPropertyName("dclDe")]
        public string DeclarationDate { get; set; }

        [Newtonsoft.Json.JsonProperty("itemSeq")]
        [System.Text.Json.Serialization.JsonPropertyName("itemSeq")]
        public int ItemSeq { get; set; }

        [Newtonsoft.Json.JsonProperty("hsCd")]
        [System.Text.Json.Serialization.JsonPropertyName("hsCd")]
        public string HSCode { get; set; }

        [Newtonsoft.Json.JsonProperty("itemNm")]
        [System.Text.Json.Serialization.JsonPropertyName("itemNm")]
        public string ItemName { get; set; }

        [Newtonsoft.Json.JsonProperty("imptItemsttsCd")]
        [System.Text.Json.Serialization.JsonPropertyName("imptItemsttsCd")]
        public string ImpItemStatusCode { get; set; }

        [Newtonsoft.Json.JsonProperty("orgnNatCd")]
        [System.Text.Json.Serialization.JsonPropertyName("orgnNatCd")]
        public string OrigCountryCode { get; set; }

        [Newtonsoft.Json.JsonProperty("exptNatCd")]
        [System.Text.Json.Serialization.JsonPropertyName("exptNatCd")]
        public string ExpCountryCode { get; set; }

        [Newtonsoft.Json.JsonProperty("pkg")]
        [System.Text.Json.Serialization.JsonPropertyName("pkg")]
        public int Package { get; set; }

        [Newtonsoft.Json.JsonProperty("pkgUnitCd")]
        [System.Text.Json.Serialization.JsonPropertyName("pkgUnitCd")]
        public string PkgUnitCode { get; set; }

        [Newtonsoft.Json.JsonProperty("qty")]
        [System.Text.Json.Serialization.JsonPropertyName("qty")]
        public int Quantity { get; set; }

        [Newtonsoft.Json.JsonProperty("qtyUnitCd")]
        [System.Text.Json.Serialization.JsonPropertyName("qtyUnitCd")]
        public string QtyUnitCode { get; set; }

        [Newtonsoft.Json.JsonProperty("totWt")]
        [System.Text.Json.Serialization.JsonPropertyName("totWt")]
        public double GrossWeight { get; set; }

        [Newtonsoft.Json.JsonProperty("netWt")]
        [System.Text.Json.Serialization.JsonPropertyName("netWt")]
        public double NetWeight { get; set; }

        [Newtonsoft.Json.JsonProperty("spplrNm")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrNm")]
        public string SupplierName { get; set; }

        [Newtonsoft.Json.JsonProperty("agntNm")]
        [System.Text.Json.Serialization.JsonPropertyName("agntNm")]
        public string AgentName { get; set; }

        [Newtonsoft.Json.JsonProperty("invcFcurAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("invcFcurAmt")]
        public decimal InvForeignCurrencyAmount { get; set; }

        [Newtonsoft.Json.JsonProperty("invcFcurCd")]
        [System.Text.Json.Serialization.JsonPropertyName("invcFcurCd")]
        public string InvForeignCurrencyCode { get; set; }

        [Newtonsoft.Json.JsonProperty("invcFcurExcrt")]
        [System.Text.Json.Serialization.JsonPropertyName("invcFcurExcrt")]
        public decimal InvForeignCurrencyExchangeRate { get; set; }
    }

    // Stock Items
    public class SelectStockItemReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("lastReqDt")]
        public string LastRequest { get; set; }
        public SelectStockItemReq()
        {
        }
        public SelectStockItemReq(ClientBranch clientBranch)
            : this()
        {
            if (string.IsNullOrEmpty(clientBranch?.TaxClient?.TaxNumber))
                throw new Exception("TaxClient or TaxNumber invalid");
            PIN = clientBranch.TaxClient.TaxNumber;
        }
    }
    public class SelectStockItemResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public StockListWrapper data { get; set; }
    }
    public class StockListWrapper
    {
        [Newtonsoft.Json.JsonProperty("stockList")]
        [System.Text.Json.Serialization.JsonPropertyName("stockList")]
        public List<StockList> stockList { get; set; }
    }
    public class StockList
    {
        [Newtonsoft.Json.JsonProperty("custTin")]
        [System.Text.Json.Serialization.JsonPropertyName("custTin")]
        public string custTin { get; set; }

        [Newtonsoft.Json.JsonProperty("custBhfId")]
        [System.Text.Json.Serialization.JsonPropertyName("custBhfId")]
        public string custBhfId { get; set; }

        [Newtonsoft.Json.JsonProperty("sarNo")]
        [System.Text.Json.Serialization.JsonPropertyName("sarNo")]
        public int sarNo { get; set; }

        [Newtonsoft.Json.JsonProperty("ocrnDt")]
        [System.Text.Json.Serialization.JsonPropertyName("ocrnDt")]
        public string ocrnDt { get; set; }

        [Newtonsoft.Json.JsonProperty("totItemCnt")]
        [System.Text.Json.Serialization.JsonPropertyName("totItemCnt")]
        public int totItemCnt { get; set; }

        [Newtonsoft.Json.JsonProperty("totTaxblAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totTaxblAmt")]
        public int totTaxblAmt { get; set; }

        [Newtonsoft.Json.JsonProperty("totTaxAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totTaxAmt")]
        public double totTaxAmt { get; set; }

        [Newtonsoft.Json.JsonProperty("totAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totAmt")]
        public int totAmt { get; set; }

        [Newtonsoft.Json.JsonProperty("remark")]
        [System.Text.Json.Serialization.JsonPropertyName("remark")]
        public string remark { get; set; }

        [Newtonsoft.Json.JsonProperty("itemList")]
        [System.Text.Json.Serialization.JsonPropertyName("itemList")]
        public List<VSCUStockItem> itemList { get; set; }
    }
    public class VSCUStockItem
    {
        [Newtonsoft.Json.JsonProperty("itemSeq")]
        [System.Text.Json.Serialization.JsonPropertyName("itemSeq")]
        public int itemSeq { get; set; }

        [Newtonsoft.Json.JsonProperty("itemCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemCd")]
        public string itemCd { get; set; }

        [Newtonsoft.Json.JsonProperty("itemClsCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemClsCd")]
        public string itemClsCd { get; set; }

        [Newtonsoft.Json.JsonProperty("itemNm")]
        [System.Text.Json.Serialization.JsonPropertyName("itemNm")]
        public string itemNm { get; set; }

        [Newtonsoft.Json.JsonProperty("bcd")]
        [System.Text.Json.Serialization.JsonPropertyName("bcd")]
        public string bcd { get; set; }

        [Newtonsoft.Json.JsonProperty("pkgUnitCd")]
        [System.Text.Json.Serialization.JsonPropertyName("pkgUnitCd")]
        public string pkgUnitCd { get; set; }

        [Newtonsoft.Json.JsonProperty("pkg")]
        [System.Text.Json.Serialization.JsonPropertyName("pkg")]
        public int pkg { get; set; }

        [Newtonsoft.Json.JsonProperty("qtyUnitCd")]
        [System.Text.Json.Serialization.JsonPropertyName("qtyUnitCd")]
        public string qtyUnitCd { get; set; }

        [Newtonsoft.Json.JsonProperty("qty")]
        [System.Text.Json.Serialization.JsonPropertyName("qty")]
        public int qty { get; set; }

        [Newtonsoft.Json.JsonProperty("itemExprDt")]
        [System.Text.Json.Serialization.JsonPropertyName("itemExprDt")]
        public string itemExprDt { get; set; }

        [Newtonsoft.Json.JsonProperty("prc")]
        [System.Text.Json.Serialization.JsonPropertyName("prc")]
        public int prc { get; set; }

        [Newtonsoft.Json.JsonProperty("splyAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("splyAmt")]
        public int splyAmt { get; set; }

        [Newtonsoft.Json.JsonProperty("totDcAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totDcAmt")]
        public int totDcAmt { get; set; }

        [Newtonsoft.Json.JsonProperty("taxblAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmt")]
        public int taxblAmt { get; set; }

        [Newtonsoft.Json.JsonProperty("taxTyCd")]
        [System.Text.Json.Serialization.JsonPropertyName("taxTyCd")]
        public string taxTyCd { get; set; }

        [Newtonsoft.Json.JsonProperty("taxAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmt")]
        public double taxAmt { get; set; }

        [Newtonsoft.Json.JsonProperty("totAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totAmt")]
        public int totAmt { get; set; }
    }

}
