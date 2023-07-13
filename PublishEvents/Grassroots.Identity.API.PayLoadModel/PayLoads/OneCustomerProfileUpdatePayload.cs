using System;

namespace Grassroots.Identity.API.PayLoadModel.PayLoads
{
    public class OneCustomerProfileUpdatePayload
    {
        public string Uid { get; set; }
        public bool? HasFullAccount { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
