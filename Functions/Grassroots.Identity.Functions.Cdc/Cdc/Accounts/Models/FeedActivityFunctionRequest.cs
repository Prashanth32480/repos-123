using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class FeedActivityFunctionRequest
    {
        public string Uid { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? FeedId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CdcGetAccountInfoResponse UserAccountInfo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CdcApiKey { get; set; }

        public CdcGetAccountInfoData Data { get; set; }
        public CdcGetAccountInfoProfile Profile { get; set; }
        public ConsentRequest Preferences { get; set; }
        public SubscriptionRequest Subscriptions { get; set; }
        public bool? HasFullAccount { get; set; }
    }

    public class SubscriptionRequest
    {
        public string Subscription { get; set; }
        public bool IsSubscribed { get; set; }
    }

    public class ConsentRequest
    {
        public string Consent { get; set; }
        public bool IsConsentGranted { get; set; }
    }
}
