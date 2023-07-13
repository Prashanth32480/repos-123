using System;

namespace Grassroots.Identity.Database.AccessLayer.Sql.RequestModel
{
    public class ExternalFeedEventRequestModel
    {
        public long? Id { get; set; }
        public Guid? DestinationEntityGuid { get; set; }
        public Guid? SourceEntityGuid { get; set; }
    }
}
