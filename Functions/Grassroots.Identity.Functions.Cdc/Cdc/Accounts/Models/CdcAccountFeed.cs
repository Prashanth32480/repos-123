using Newtonsoft.Json;
using System.Collections.Generic;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class CdcAccountFeed
    {
        [JsonProperty("events")]
        public List<CdcEvent> Events { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }
    }
}
