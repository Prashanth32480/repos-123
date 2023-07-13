using System;
using System.Threading.Tasks;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.AccessLayer.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Grassroots.Identity.Database.Model.Static;
using ServiceStack;

namespace Grassroots.Identity.Functions.Common
{
    public class FeedEventProcessor : IFeedEventProcessor
    {
        private readonly IFeedEventOperations _feedEventOperations;
        private readonly ITelemetryHandler _telemetryHandler;
        private FeedEventSaveModel _feedEventToSave;

        public FeedEventProcessor(IFeedEventOperations feedEventOperations, ITelemetryHandler telemetryHandler)
        {
            _feedEventToSave = null;
            _feedEventOperations = feedEventOperations;
            _telemetryHandler = telemetryHandler;
        }

        // Null checks not required in this method for input parameters as that is being already taken care of at multiple places.
        public async Task<bool> ShouldFeedBeProcessed(string eventType, FeedType feedType, SourceSystem sourceSystem, Guid sourceEntityGuid, DateTime eventRaisedDateTime)
        {
            var feedEvent = await _feedEventOperations.GetFeedEventBySourceEntityGuid(sourceEntityGuid);

            if (IsCreateEvent(eventType))
            {
                if (feedEvent == null)
                {
                    _feedEventToSave = new FeedEventSaveModel()
                    {
                        EventType = eventType,
                        FeedType = feedType.ToString(),
                        SourceSystem = sourceSystem.ToString(),
                        SourceEntityGuid = sourceEntityGuid,
                        LastEventRaisedDateTime = eventRaisedDateTime
                    };
                    return true;
                }

                if (feedEvent.LastEventRaisedDateTime < eventRaisedDateTime)
                {
                    if (IsDeleteEvent(feedEvent.EventType))
                    {
                        _feedEventToSave = feedEvent.ConvertTo<FeedEventSaveModel>();
                        _feedEventToSave.EventType = eventType;
                        _feedEventToSave.LastEventRaisedDateTime = eventRaisedDateTime;
                        return true;
                    }
                }

                return false;
            }

            if (IsUpdateEvent(eventType))
            {
                if (feedEvent == null)
                {
                    _feedEventToSave = new FeedEventSaveModel()
                    {
                        EventType = eventType,
                        FeedType = feedType.ToString(),
                        SourceSystem = sourceSystem.ToString(),
                        SourceEntityGuid = sourceEntityGuid,
                        LastEventRaisedDateTime = eventRaisedDateTime
                    };
                    return true;
                }

                if (feedEvent.LastEventRaisedDateTime < eventRaisedDateTime)
                {
                    _feedEventToSave = feedEvent.ConvertTo<FeedEventSaveModel>();
                    _feedEventToSave.EventType = eventType;
                    _feedEventToSave.LastEventRaisedDateTime = eventRaisedDateTime;
                    return true;
                }

                return false;
            }

            if (IsDeleteEvent(eventType))
            {
                if (feedEvent == null)
                    return false;

                if (!IsDeleteEvent(feedEvent.EventType) && feedEvent.LastEventRaisedDateTime < eventRaisedDateTime)
                {
                    _feedEventToSave = feedEvent.ConvertTo<FeedEventSaveModel>();
                    _feedEventToSave.EventType = eventType;
                    _feedEventToSave.LastEventRaisedDateTime = eventRaisedDateTime;
                    return true;
                }
            }

            return false;
        }

        public async Task UpdateEventRaisedDateTime(Guid kondoEntityGuid)
        {
            if (_feedEventToSave != null)
            {
                _feedEventToSave.KondoEntityGuid = kondoEntityGuid;
                await _feedEventOperations.SaveFeedEvent(_feedEventToSave);
            }
            else
            {
                _telemetryHandler.TrackTraceWarning($"{nameof(UpdateEventRaisedDateTime)} method called before {nameof(ShouldFeedBeProcessed)} method for {nameof(kondoEntityGuid)}: {kondoEntityGuid}");
            }
        }

        #region Private methods
        private static bool IsCreateEvent(string eventType)
        {
            return eventType.ToUpper().Contains("CREATED");
        }

        private static bool IsUpdateEvent(string eventType)
        {
            return eventType.ToUpper().Contains("UPDATED");
        }

        private static bool IsDeleteEvent(string eventType)
        {
            return eventType.ToUpper().Contains("DELETED");
        }
        #endregion
    }
}