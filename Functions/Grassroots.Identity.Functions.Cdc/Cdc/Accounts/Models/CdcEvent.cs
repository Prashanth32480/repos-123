using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class CdcEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty("callId")]
        public string CallId { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("data")]
        public CdcEventData Data { get; set; }
        public long FeedId { get; set; }
    }
}
