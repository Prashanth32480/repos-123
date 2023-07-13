using System;
using System.Collections.Generic;

namespace Grassroots.Identity.API.PayLoadModel.PayLoads
{
    public class ProfileClaimPayload
    {
        public Guid ParticipantId { get; set; }
        public List<Guid?> PlayHQProfileId { get; set; }
        public List<Guid?> ClaimedPlayHQProfileId { get; set; }

    }
}