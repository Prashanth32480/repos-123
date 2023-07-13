using System;
using Grassroots.Identity.Database.Model.Static;

namespace Grassroots.Identity.Database.AccessLayer.Sql.RequestModel
{
    public class RawFeedSaveRequestModel
    {
        public FeedType FeedTypeId { get; set; }
        public string MessageId { get; set; }
        public string BlobId { get; set; }
        public string Category { get; set; }
        public DateTime EventRaisedDateTime { get; set; }
    }
}
