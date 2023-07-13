using System;
using System.Threading.Tasks;
using Grassroots.Identity.Database.Model.Static;

namespace Grassroots.Identity.Functions.External.Common
{
    public interface IFeedEventProcessor
    {
        /// <summary>
        /// Checks whether the feed is to be processed or not.
        /// It decides this based on if the feed received as the latest or we already have another feed that had a later date.
        /// If the event type of the feed is delete then it will remove the record from FeedEvent table for that Guid.
        /// This method needs to be called before calling UpdateEventRaisedDatetime.
        /// </summary>
        /// <returns></returns>
        Task<bool> ShouldFeedBeProcessed(string eventType, FeedType feedType, SourceSystem sourceSystem, Guid sourceEntityGuid, DateTime eventRaisedDateTime);

        /// <summary>
        /// Updated the LastEventRaisedDateTime in the FeedEvent table.
        /// this method should only be called if ShouldFeedBeProcessed() method returns true.
        /// </summary>
        /// <returns></returns>
        Task UpdateEventRaisedDateTime(Guid kondoEntityGuid);
    }
}