using Newtonsoft.Json;
using System;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Common.Model
{
    public class PlayHQFeed<T>
    {
        [JsonProperty("messageId")]
        public Guid MessageId { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        [JsonProperty("eventRaisedDateTime")]
        public DateTime? EventRaisedDateTime { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }
}