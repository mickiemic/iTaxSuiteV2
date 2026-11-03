
namespace iTaxSuite.Library.Models.ViewModels
{
    public abstract class SageX3ERPResp
    {
        [Newtonsoft.Json.JsonProperty("$links")]
        [System.Text.Json.Serialization.JsonPropertyName("$links")]
        public Links Links { get; set; }
    }
    public class Links
    {
        [Newtonsoft.Json.JsonProperty("$itemsPerPage")]
        [System.Text.Json.Serialization.JsonPropertyName("$itemsPerPage")]
        public int ItemsPerPage { get; set; }

        [Newtonsoft.Json.JsonProperty("$last")]
        [System.Text.Json.Serialization.JsonPropertyName("$last")]
        public Last Last { get; set; }
    }
    public class Last
    {
        [Newtonsoft.Json.JsonProperty("$url")]
        [System.Text.Json.Serialization.JsonPropertyName("$url")]
        public string Url { get; set; }

        [Newtonsoft.Json.JsonProperty("$type")]
        [System.Text.Json.Serialization.JsonPropertyName("$type")]
        public string Type { get; set; }

        [Newtonsoft.Json.JsonProperty("$method")]
        [System.Text.Json.Serialization.JsonPropertyName("$method")]
        public string Method { get; set; }
    }

    public class SageX3Items : SageX3ERPResp
    {
        [Newtonsoft.Json.JsonProperty("$resources")]
        [System.Text.Json.Serialization.JsonPropertyName("$resources")]
        public List<SageX3Item> Items { get; set; }
    }
    public class SageX3Item
    {
        [Newtonsoft.Json.JsonProperty("$uuid")]
        [System.Text.Json.Serialization.JsonPropertyName("$uuid")]
        public string uuid { get; set; }

        [Newtonsoft.Json.JsonProperty("$etag")]
        [System.Text.Json.Serialization.JsonPropertyName("$etag")]
        public DateTime etag { get; set; }

        [Newtonsoft.Json.JsonProperty("TCLCOD")]
        [System.Text.Json.Serialization.JsonPropertyName("TCLCOD")]
        public string TCLCOD { get; set; }

        [Newtonsoft.Json.JsonProperty("ITMREF")]
        [System.Text.Json.Serialization.JsonPropertyName("ITMREF")]
        public string ITMREF { get; set; }

        [Newtonsoft.Json.JsonProperty("DES1AXX")]
        [System.Text.Json.Serialization.JsonPropertyName("DES1AXX")]
        public string DES1AXX { get; set; }

        [Newtonsoft.Json.JsonProperty("ITMSTA")]
        [System.Text.Json.Serialization.JsonPropertyName("ITMSTA")]
        public int ITMSTA { get; set; }

        [Newtonsoft.Json.JsonProperty("IMG")]
        [System.Text.Json.Serialization.JsonPropertyName("IMG")]
        public object IMG { get; set; }
    }
}
