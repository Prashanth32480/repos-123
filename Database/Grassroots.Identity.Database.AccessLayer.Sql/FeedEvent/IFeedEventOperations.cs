using System;
using System.Threading.Tasks;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Grassroots.Identity.Database.Model.DbEntity;

// ReSharper disable once CheckNamespace
namespace Grassroots.Identity.Database.AccessLayer.Sql
{
    public interface IFeedEventOperations
    {
        //raw.Feed table
        Task<FeedEvent> GetFeedEventByKondoEntityGuid(Guid kondoEntityGuid);
        #nullable enable
        Task<FeedEvent?> GetFeedEventBySourceEntityGuid(Guid sourceEntityGuid);
        #nullable disable
        Task<FeedEvent> GetFeedEventById(long feedEventId);
        Task<long> SaveFeedEvent(FeedEventSaveModel request);
        /// <summary>
        /// Updates the LastEventRaisedDateTime in the raw.FeedEvent table for the given feedEventId.
        /// Throws ApplicationException if record not found.
        /// </summary>
        /// <param name="feedEventId"></param>
        /// <param name="eventRaisedDateTime"></param>
        /// <returns></returns>
        Task<long> UpdateLastEventRaisedDateTime(long feedEventId, DateTime eventRaisedDateTime);

        Task<long> DeleteFeedEvent(Guid kondoEntityGuid);


        //raw.externalFeed table
        Task<ExternalFeedEvent> GetExternalFeedEventByDestinationEntityGuid(Guid destinationEntityGuid);
        Task<ExternalFeedEvent?> GetExternalFeedEventBySourceEntityGuid(Guid sourceEntityGuid);
        Task<ExternalFeedEvent> GetExternalFeedEventById(long feedEventId);
        Task<long> SaveExternalFeedEvent(ExternalFeedEventSaveModel request);
        Task<long> UpdateExternalFeedLastEventRaisedDateTime(long feedEventId, DateTime eventRaisedDateTime);

        Task<long> DeleteExternalFeedEvent(Guid destinationEntityGuid);
    }
}