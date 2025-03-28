using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnionAvatars.Metrics
{
    public class MetricData
    {
        [JsonProperty("filters", DefaultValueHandling = DefaultValueHandling.Populate)]
        public Filter[] Filters = new Filter[0];

        [JsonProperty("event_type", Required = Required.Always)]
        public string EventType { get; set; }

        [JsonProperty("extra_info", Required = Required.Always)]
        public Dictionary<string, string> ExtraInfo { get; set; }
    }

    public class ExtraInfo
    {
        [JsonProperty("company", NullValueHandling = NullValueHandling.Ignore)]
        public string Company { get; set; }

        [JsonProperty("product", NullValueHandling = NullValueHandling.Ignore)]
        public string Product { get; set; }

        [JsonProperty("product_version", NullValueHandling = NullValueHandling.Ignore)]
        public string ProductVersion { get; set; }

        [JsonProperty("engine", NullValueHandling = NullValueHandling.Ignore)]
        public string Engine { get; set; }

        [JsonProperty("sdk_version", NullValueHandling = NullValueHandling.Ignore)]
        public string SdkVersion { get; set; }

        [JsonProperty("build_target", NullValueHandling = NullValueHandling.Ignore)]
        public string BuildTarget { get; set; }

        [JsonProperty("build_type", NullValueHandling = NullValueHandling.Ignore)]
        public string BuildType { get; set; }

        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public string User { get; set; }
    }

    public class Filter
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("options")]
        public string[] Options { get; set; }
    }
}

