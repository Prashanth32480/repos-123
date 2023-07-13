using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grassroots.Database.Infrastructure.Sql;
using Grassroots.Identity.Common;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Grassroots.Identity.Database.Model.DbEntity;

namespace Grassroots.Identity.Database.AccessLayer.Sql
{
    public class FeedOperations : IFeedOperations
    {
        private readonly IDatabaseConnectionFactory _factory;
        public FeedOperations(IDatabaseConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<long> SaveRawFeed(RawFeedSaveRequestModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();
            var feedId = await connection.ExecuteResultLongAsync(DatabaseStoreProcedures.FeedInsert, request);

            if (feedId == null)
                throw new ApplicationException(ApplicationStrings.ErrorInSaveRawFeed);

            return feedId.Value;
        }

        public async Task<IEnumerable<RawFeed>> GetRawFeedByMessageID(string messageId)
        {
            if (messageId == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(messageId));

            using var connection = _factory.CreateConnection();
            var feed = await connection.ExecuteResultCollectionAsync<RawFeed, object>(DatabaseStoreProcedures.FeedGet, new { MessageId = messageId });

            return feed;
        }

        public async Task<long> SetProcessStatusToSuccess(long rawFeedId)
        {
            if (rawFeedId <= 0)
                throw new ArgumentOutOfRangeException(nameof(rawFeedId), ApplicationStrings.ArgumentExceptionNullObject);

            using var connection = _factory.CreateConnection();
            var feedId = await connection.ExecuteResultLongAsync(DatabaseStoreProcedures.FeedSetProcessStatusToSuccess, new { FeedId = rawFeedId });

            if (feedId == null || feedId <= 0)
                throw new ApplicationException(ApplicationStrings.ErrorInRawFeedStatusChange);

            return feedId.Value;
        }
        public async Task<long> SaveExternalRawFeed(RawFeedSaveRequestModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();
            var feedId = await connection.ExecuteResultLongAsync(DatabaseStoreProcedures.ExternalFeedInsert, request);

            if (feedId == null)
                throw new ApplicationException(ApplicationStrings.ErrorInSaveRawFeed);

            return feedId.Value;
        }

        public async Task<IEnumerable<RawFeed>> GetExternalRawFeedByMessageId(string messageId)
        {
            if (messageId == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(messageId));

            using var connection = _factory.CreateConnection();
            var feed = await connection.ExecuteResultCollectionAsync<RawFeed, object>(DatabaseStoreProcedures.ExternalFeedGet, new { MessageId = messageId });

            return feed;
        }

        public async Task<long> SetExternalFeedProcessStatusToSuccess(long rawFeedId)
        {
            if (rawFeedId <= 0)
                throw new ArgumentOutOfRangeException(nameof(rawFeedId), ApplicationStrings.ArgumentExceptionNullObject);

            using var connection = _factory.CreateConnection();
            var feedId = await connection.ExecuteResultLongAsync(DatabaseStoreProcedures.ExternalFeedSetProcessStatusToSuccess, new { FeedId = rawFeedId });

            if (feedId == null || feedId <= 0)
                throw new ApplicationException(ApplicationStrings.ErrorInRawFeedStatusChange);

            return feedId.Value;
        }
    }
}