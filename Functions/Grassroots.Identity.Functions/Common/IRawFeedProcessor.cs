using System;
using System.Threading.Tasks;
using Grassroots.Identity.Database.Model.Static;

namespace Grassroots.Identity.Functions.Common
{
    public interface IRawFeedProcessor
    {
        /// <summary>
        /// Inserts Raw feed in raw.Feed table in the database and returns the RawFeedId as a response.
        /// </summary>
        /// <param name="feedType">The source of the feed from where it is coming. Eg: CRM, PlayHQ, etc.</param>
        /// <param name="messageId">The main ID of the message against which it is processed. Eg: CRMId</param>
        /// <param name="blobId">The Guid name of the feed that is stored in the blob storage or the blob file name.</param>
        /// <param name="category">The category of the feed. Eg: Organisation, Product, Registration, etc.</param>
        /// <param name="eventRaisedDateTime">Event Raided Date Time from the received feed.</param>
        /// <returns></returns>
        Task<long> InsertRawFeed(FeedType feedType, string messageId, string blobId, string category, DateTime eventRaisedDateTime);
        
        Task<long> SetRawFeedStatusToSuccess(long rawFeedId);

        Task<bool> CheckRawFeedExists(string messageId);
    }
}