using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace iTaxSuite.Library.Models.ViewModels
{
    public abstract class DTaxBaseReq
    {
    }
    public class DTaxPagination
    {
        [Newtonsoft.Json.JsonProperty("page_size")]
        [System.Text.Json.Serialization.JsonPropertyName("page_size")]
        public int PageSize { get; set; }
        [Newtonsoft.Json.JsonProperty("previous", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("previous")]
        public string Previous { get; set; }
        [Newtonsoft.Json.JsonProperty("next", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("next")]
        public string Next { get; set; }
    }

    public abstract class DTaxBaseItem
    {
        [Newtonsoft.Json.JsonProperty("item_class_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_class_code")]
        public string ItemClassCode { get; set; }
        [Newtonsoft.Json.JsonProperty("item_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_type_code")]
        public string ItemTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("item_name")]
        [System.Text.Json.Serialization.JsonPropertyName("item_name")]
        public string ItemName { get; set; }
        [Newtonsoft.Json.JsonProperty("origin_nation_code")]
        [System.Text.Json.Serialization.JsonPropertyName("origin_nation_code")]
        public string OriginNatCode { get; set; }
        [Newtonsoft.Json.JsonProperty("package_unit_code")]
        [System.Text.Json.Serialization.JsonPropertyName("package_unit_code")]
        public string PkgUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("quantity_unit_code")]
        [System.Text.Json.Serialization.JsonPropertyName("quantity_unit_code")]
        public string QtyUnitCode { get; set; }
    }

    public abstract class DTaxBaseResp
    {
        [Newtonsoft.Json.JsonProperty("message", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string Message { get; set; }
        [Newtonsoft.Json.JsonProperty("code", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("code")]
        public string Code { get; set; }
        [Newtonsoft.Json.JsonProperty("metadata", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("metadata")]
        public JObject Metadata { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsSuccess => (string.IsNullOrWhiteSpace(Code) || Code.StartsWith("2"));

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string RawResponse { get; set; }
    }
    public abstract class DTaxBaseItemResp : DTaxBaseResp
    {
        [Newtonsoft.Json.JsonProperty("item_class_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_class_code")]
        public string ItemClassCode { get; set; }
        [Newtonsoft.Json.JsonProperty("item_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_type_code")]
        public string ItemTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("item_name")]
        [System.Text.Json.Serialization.JsonPropertyName("item_name")]
        public string ItemName { get; set; }
        [Newtonsoft.Json.JsonProperty("origin_nation_code")]
        [System.Text.Json.Serialization.JsonPropertyName("origin_nation_code")]
        public string OriginNatCode { get; set; }
        [Newtonsoft.Json.JsonProperty("package_unit_code")]
        [System.Text.Json.Serialization.JsonPropertyName("package_unit_code")]
        public string PkgUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("quantity_unit_code")]
        [System.Text.Json.Serialization.JsonPropertyName("quantity_unit_code")]
        public string QtyUnitCode { get; set; }
    }

    public enum DTaxSyncStatus
    {
        [EnumMember(Value = "PENDING")]
        PENDING = 0,
        [EnumMember(Value = "COMPLETED")]
        COMPLETED,
        [EnumMember(Value = "FAILED")]
        FAILED
    }

    public enum DTaxStockAction
    {
        [EnumMember(Value = "ADD")]
        ADD = 0,
        [EnumMember(Value = "DEDUCT")]
        DEDUCT
    }

}
