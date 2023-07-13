using Newtonsoft.Json;
using System;

namespace Grassroots.Identity.Functions.External.Common.Model
{
    public class IdentityExternalFeedRequest
    {
        public string Uid { get; set; }
        public CdcGetAccountInfoData Data { get; set; }
        public CdcGetAccountInfoProfile Profile { get; set; }
        public ConsentRequest Preferences { get; set; }
        public SubscriptionRequest Subscriptions { get; set; }
        public long FeedId { get; set; }
        public bool? HasFullAccount { get; set; }

    }
}
