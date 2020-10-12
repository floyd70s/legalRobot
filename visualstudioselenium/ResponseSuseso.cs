using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace suseso
{
    public class ResponseSuseso
    {
        [JsonProperty("cid")]
        public int cid { get; set; }

        [JsonProperty("pid")]
        public string pid { get; set; }

        [JsonProperty("property-value_620_pvid")]
        public string property_value_620_pvid   { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("property-value_532")]
        public string comment { get; set; }

        [JsonProperty("abstract")]
        public string abstrac   { get; set; }

        [JsonProperty("property-value_620_name")]
        public string theme   { get; set; }

        [JsonProperty("extended-property-value_pvid")]
        public string linkedCirculars { get; set; }

        [JsonProperty("property-value_620_pid")]
        public string property_value_620_pid   { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("property-value_546_iso8601")]
        public string sentenceDate { get; set; }

        [JsonProperty("binary_id")]
        public string binary_id { get; set; }

        [JsonProperty("aid")]
        public string aid { get; set; }

        [JsonProperty("title")]
        public string title { get; set; }

        [JsonProperty("iid")]
        public string iid { get; set; }

        [JsonProperty("score")]
        public string score { get; set; }

        [JsonProperty("property-value_525")]
        public string property_value_525 { get; set; }

        [JsonProperty("hl1")]
        public string hl1 { get; set; }

        [JsonProperty("using_cids")]
        public string using_cids { get; set; }


    public ResponseSuseso()
        {

        }
    }
}
