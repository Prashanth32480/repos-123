using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class CdcEventData
    {
        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("uid")]
        public string UId { get; set; }

        [JsonProperty("newUid")]
        public string NewUid { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("ConsentId")]
        public string ConsentId { get; set; }

        [JsonProperty("Action")]
        public string Action { get; set; }

        [JsonProperty("NewConsentState")]
        public CdcConsentState NewConsentState { get; set; }

        [JsonProperty("subscription")]
        public dynamic Subscription { get; set; }


    }
}
