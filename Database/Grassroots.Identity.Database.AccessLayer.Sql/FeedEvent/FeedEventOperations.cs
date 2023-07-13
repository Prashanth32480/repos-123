
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grassroots.Database.Infrastructure.Sql;
using Grassroots.Identity.Common;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Grassroots.Identity.Database.Model.DbEntity;

// ReSharper disable once CheckNamespace
namespace Grassroots.Identity.Database.AccessLayer.Sql
{
    public class FeedEventOperations : IFeedEventOperations
    {
        private readonly IDatabaseConnectionFactory _factory;
        public FeedEventOperations(IDatabaseConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<FeedEvent> GetFeedEventByKondoEntityGuid(Guid kondoEntityGuid)
        {
            if (kondoEntityGuid == Guid.Empty)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(kondoEntityGuid));

            return (await GetFeedEvent(new FeedEventRequestModel
            {
                KondoEntityGuid = kondoEntityGuid
            })).FirstOrDefault();
        }
#nullable enable
        public async Task<FeedEvent?> GetFeedEventBySourceEntityGuid(Guid sourceEntityGuid)
        {
            if (sourceEntityGuid == Guid.Empty)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(sourceEntityGuid));

            return (await GetFeedEvent(new FeedEventRequestModel
            {
                SourceEntityGuid = sourceEntityGuid
            })).FirstOrDefault();
        }
#nullable disable
        public async Task<FeedEvent> GetFeedEventById(long feedEventId)
        {
            if (feedEventId <= 0)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(feedEventId));

            return (await GetFeedEvent(new FeedEventRequestModel
            {
                Id = feedEventId
            })).FirstOrDefault();
        }

        private async Task<IEnumerable<FeedEvent>> GetFeedEvent(FeedEventRequestModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();
            var feedEvents = await connection.ExecuteResultCollectionAsync<FeedEvent, FeedEventRequestModel>(DatabaseStoreProcedures.FeedEventGet, request);

            return feedEvents;
        }

        public async Task<long> SaveFeedEvent(FeedEventSaveModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();
            var resultFeedEventId = await connection.ExecuteResultLongAsync(DatabaseStoreProcedures.FeedEventSave, request);

            if (resultFeedEventId == null || resultFeedEventId <= 0)
                throw new ApplicationException(ApplicationStrings.ErrorInSaveFeedEvent);

            return resultFeedEventId.Value;
        }

        public async Task<long> UpdateLastEventRaisedDateTime(long feedEventId, DateTime eventRaisedDateTime)
        {
            if (feedEventId <= 0)
                throw new ArgumentOutOfRangeException(nameof(feedEventId), ApplicationStrings.ArgumentExceptionNullObject);

            using var connection = _factory.CreateConnection();
            var resultFeedEventId = await connection.ExecuteResultLongAsync(DatabaseStoreProcedures.FeedEventSave, new FeedEventSaveModel { Id = feedEventId, LastEventRaisedDateTime = eventRaisedDateTime });

            if (resultFeedEventId == null || resultFeedEventId <= 0)
                throw new ApplicationException(ApplicationStrings.ErrorInUpdateLastRaisedDateTimeFeedEvent);

            return resultFeedEventId.Value;
        }

        public async Task<long> DeleteFeedEvent(Guid kondoEntityGuid)
        {
            if (kondoEntityGuid == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(kondoEntityGuid), ApplicationStrings.ArgumentExceptionNullObject);

            using var connection = _factory.CreateConnection();
            var resultFeedEventId = await connection.ExecuteResultLongAsync(DatabaseStoreProcedures.FeedEventDelete, new { KondoEntityGuid = kondoEntityGuid });

            if (resultFeedEventId == null || resultFeedEventId <= 0)
                throw new ApplicationException(ApplicationStrings.ErrorInUpdateLastRaisedDateTimeFeedEvent);

            return resultFeedEventId.Value;
        }

        //External Feed

        public async Task<ExternalFeedEvent> GetExternalFeedEventByDestinationEntityGuid(Guid destinationEntityGuid)
        {
            if (destinationEntityGuid == Guid.Empty)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(destinationEntityGuid));

            return (await GetExternalFeedEvent(new ExternalFeedEventRequestModel
            {
                DestinationEntityGuid = destinationEntityGuid
            })).FirstOrDefault();
        }

        public async Task<ExternalFeedEvent?> GetExternalFeedEventBySourceEntityGuid(Guid sourceEntityGuid)
        {
            if (sourceEntityGuid == Guid.Empty)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(sourceEntityGuid));

            return (await GetExternalFeedEvent(new ExternalFeedEventRequestModel
            {
                SourceEntityGuid = sourceEntityGuid
            })).FirstOrDefault();
        }

        public async Task<ExternalFeedEvent> GetExternalFeedEventById(long feedEventId)
        {
            if (feedEventId <= 0)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(feedEventId));

            return (await GetExternalFeedEvent(new ExternalFeedEventRequestModel
            {
                Id = feedEventId
            })).FirstOrDefault();
        }

        private async Task<IEnumerable<ExternalFeedEvent>> GetExternalFeedEvent(ExternalFeedEventRequestModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();
            var feedEvents = await connection.ExecuteResultCollectionAsync<ExternalFeedEvent, ExternalFeedEventRequestModel>(DatabaseStoreProcedures.ExternalFeedEventGet, request);

            return feedEvents;
        }

        public async Task<long> SaveExternalFeedEvent(ExternalFeedEventSaveModel request)
        {
            if (request == null)
                throw new ArgumentException(ApplicationStrings.ArgumentExceptionNullObject, nameof(request));

            using var connection = _factory.CreateConnection();
            var resultFeedEventId = await connection.ExecuteResultLongAsync(DatabaseStoreProcedures.ExternalFeedEventSave, request);

            if (resultFeedEventId == null || resultFeedEventId <= 0)
                throw new ApplicationException(ApplicationStrings.ErrorInSaveFeedEvent);

            return resultFeedEventId.Value;
        }

        public async Task<long> UpdateExternalFeedLastEventRaisedDateTime(long feedEventId, DateTime eventRaisedDateTime)
        {
            if (feedEventId <= 0)
                throw new ArgumentOutOfRangeException(nameof(feedEventId), ApplicationStrings.ArgumentExceptionNullObject);

            using var connection = _factory.CreateConnection();
            var resultFeedEventId = await connection.ExecuteResultLongAsync(DatabaseStoreProcedures.ExternalFeedEventSave, new ExternalFeedEventSaveModel { Id = feedEventId, LastEventRaisedDateTime = eventRaisedDateTime });

            if (resultFeedEventId == null || resultFeedEventId <= 0)
                throw new ApplicationException(ApplicationStrings.ErrorInUpdateLastRaisedDateTimeFeedEvent);

            return resultFeedEventId.Value;
        }

        public async Task<long> DeleteExternalFeedEvent(Guid destinationEntityGuid)
        {
            if (destinationEntityGuid == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(destinationEntityGuid), ApplicationStrings.ArgumentExceptionNullObject);

            using var connection = _factory.CreateConnection();
            var resultFeedEventId = await connection.ExecuteResultLongAsync(DatabaseStoreProcedures.ExternalFeedEventDelete, new { DestinationEntityGuid = destinationEntityGuid });

            if (resultFeedEventId == null || resultFeedEventId <= 0)
                throw new ApplicationException(ApplicationStrings.ErrorInUpdateLastRaisedDateTimeFeedEvent);

            return resultFeedEventId.Value;
        }
    }
}