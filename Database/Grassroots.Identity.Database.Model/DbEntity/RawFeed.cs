using System;

namespace Grassroots.Identity.Database.Model.DbEntity
{
    public class RawFeed
    {
        public long FeedId { get; set; }
        public int FeedTypeId { get; set; }
        public string MessageId { get; set; }
        public string BlobId { get; set; }
        public string Category { get; set; }
        public DateTime ProcessingDateTime { get; set; }
    }
}
