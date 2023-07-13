using System;

namespace Grassroots.Identity.Database.Model.DbEntity
{
    public class ParticipantDetails
    {
        public Guid ParticipantGuid { get; set; }
        public Guid? CricketId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsNameVisible { get; set; }
        public bool IsSearchable { get; set; }
        public Guid? ParentCricketId { get; set; }
        public Guid? PlayHQProfileId { get; set; }
        public int? LegacyPlayerId { get; set; }
    }
}
