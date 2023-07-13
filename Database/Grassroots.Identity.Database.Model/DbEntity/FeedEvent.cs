using System;

namespace Grassroots.Identity.Database.Model.DbEntity
{
    public class FeedEvent
    {
        public long Id { get; set; }
        public string EventType { get; set; }
        public string FeedType { get; set; }
        public string SourceSystem { get; set; }
        public Guid SourceEntityGuid { get; set; }
        public Guid KondoEntityGuid { get; set; }
        public DateTime LastEventRaisedDateTime { get; set; }
    }
}