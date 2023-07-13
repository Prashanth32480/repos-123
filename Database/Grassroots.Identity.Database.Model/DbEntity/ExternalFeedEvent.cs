using System;

namespace Grassroots.Identity.Database.Model.DbEntity
{
    public class ExternalFeedEvent
    {
        public long Id { get; set; }
        public string EventType { get; set; }
        public string FeedType { get; set; }
        public string SourceSystem { get; set; }
        public Guid SourceEntityGuid { get; set; }
        public Guid DestinationEntityGuid { get; set; }
        public DateTime LastEventRaisedDateTime { get; set; }
    }
}
