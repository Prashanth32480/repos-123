using System;

namespace Grassroots.Identity.API.PayLoadModel.PayLoads
{
    public class OneCustomerPayload
    {
        public string Uid { get; set; }
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
