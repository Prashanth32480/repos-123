using System;
using System.Collections.Generic;

namespace Grassroots.Identity.API.PayLoadModel.PayLoads
{
    public class ParticipantPayload
    {
        public Guid ParticipantId { get; set; }
        public Guid? CricketId { get; set; }
        public List<Guid?> PlayHQProfileId { get; set; }
        public List<int> LegacyPlayerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsNameVisible { get; set; }
        public bool IsNameSuppressed { get; set; }
        public bool IsSearchable { get; set; }
        public Guid? ParentCricketId { get; set; }
    }
}