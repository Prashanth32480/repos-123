using System;

// ReSharper disable once CheckNamespace
namespace Grassroots.Identity.Database.AccessLayer.Sql.RequestModel
{
    public class FeedEventRequestModel
    {
        public long? Id { get; set; }
        public Guid? KondoEntityGuid { get; set; }
        public Guid? SourceEntityGuid { get; set; }
    }
}
