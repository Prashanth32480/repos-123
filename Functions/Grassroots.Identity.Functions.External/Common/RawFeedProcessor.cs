using System;
using System.Linq;
using System.Threading.Tasks;
using Grassroots.Identity.Database.AccessLayer.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Grassroots.Identity.Database.Model.Static;

namespace Grassroots.Identity.Functions.External.Common
{
    public class RawFeedProcessor : IRawFeedProcessor
    {

        private readonly IFeedOperations _feedOperations;

        public RawFeedProcessor(IFeedOperations feedOperations)
        {
            _feedOperations = feedOperations;
        }

        public async Task<long> InsertRawFeed(FeedType feedType, string messageId, string blobId, string category, DateTime eventRaisedDateTime)
        {
            // Returns RawFeedId
            return await _feedOperations.SaveExternalRawFeed(new RawFeedSaveRequestModel
            {
                FeedTypeId = feedType,
                MessageId = messageId,
                BlobId = blobId,
                Category = category,
                EventRaisedDateTime = eventRaisedDateTime
            });
        }

        public async Task<long> SetRawFeedStatusToSuccess(long rawFeedId)
        {
            // Returns RawFeedId
            return await _feedOperations.SetExternalFeedProcessStatusToSuccess(rawFeedId);
        }

        public async Task<bool> CheckRawFeedExists(string messageId)
        {

            var feed = await _feedOperations.GetExternalRawFeedByMessageId(messageId);

            return feed.Any();
        }
    }
}