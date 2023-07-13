using System;

namespace Grassroots.Identity.Database.Model.DbEntity
{
    public class ParticipantMapping
    {
        public Guid? PlayHqProfileId { get; set; }
        public Guid ParticipantGuid { get; set; }
        public int? LegacyPlayerId { get; set; }
    }
}
