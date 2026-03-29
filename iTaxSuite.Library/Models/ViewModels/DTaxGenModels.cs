namespace iTaxSuite.Library.Models.ViewModels
{
    public class DTaxBranchResp : DTaxBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        public List<DTaxBranch> Branches { get; set; }
        public new bool IsSuccess => ((Branches?.Count ?? 0) > 0);
    }
    public class DTaxBranch
    {
        public string tax_pin { get; set; }
        public string branch_office_id { get; set; }
        public string branch_office_name { get; set; }
        public string branch_status_code { get; set; }
        public string county_name { get; set; }
        public string sub_county_name { get; set; }
        public string tax_locality_name { get; set; }
        public string location_description { get; set; }
        public string manager_name { get; set; }
        public string manager_contact { get; set; }
        public string manager_email { get; set; }
        public bool is_head_office { get; set; }
        public string business_id { get; set; }
    }

    public class DTaxNoticeItem
    {
        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("notice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("notice_number")]
        public string NoticeNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("notice_date")]
        [System.Text.Json.Serialization.JsonPropertyName("notice_date")]
        public DateTime NoticeDate { get; set; }
        [Newtonsoft.Json.JsonProperty("title")]
        [System.Text.Json.Serialization.JsonPropertyName("title")]
        public string Title { get; set; }
        [Newtonsoft.Json.JsonProperty("detail")]
        [System.Text.Json.Serialization.JsonPropertyName("detail")]
        public string Detail { get; set; }
        [Newtonsoft.Json.JsonProperty("by")]
        [System.Text.Json.Serialization.JsonPropertyName("by")]
        public string By { get; set; }
        [Newtonsoft.Json.JsonProperty("link")]
        [System.Text.Json.Serialization.JsonPropertyName("link")]
        public string Link { get; set; }
    }

    public class DTaxNoticeResp : DTaxBaseResp
    {
        [Newtonsoft.Json.JsonProperty("meta")]
        [System.Text.Json.Serialization.JsonPropertyName("meta")]
        public DTaxMeta meta { get; set; }
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public List<DTaxNoticeItem> Notices { get; set; }
    }

}
