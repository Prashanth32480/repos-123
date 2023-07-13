using System;

namespace Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping.RequestModel
{
    public class ParticipantMappingSaveModel
    {
        public Guid? PlayHQProfileId { get; set; }
        public Guid ParticipantGuid { get; set; }
        public int? LegacyPlayerId { get; set; }
        public long FeedId { get; set; }

    }
}
