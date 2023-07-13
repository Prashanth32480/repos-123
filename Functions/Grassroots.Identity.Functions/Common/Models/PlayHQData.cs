using System;
using Newtonsoft.Json;


namespace Grassroots.Identity.Functions.Common.Models
{
    public class PlayHQData
    {        
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("eventRaisedDateTime")]
        public string EventRaisedDateTime { get; set; }

        [JsonProperty("profile")]
        public PlayHQProfile Profile { get; set; }
    }
}
