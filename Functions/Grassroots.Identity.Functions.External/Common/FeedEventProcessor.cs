using System;
using System.Threading.Tasks;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.AccessLayer.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;
using Grassroots.Identity.Database.Model.Static;
using ServiceStack;

namespace Grassroots.Identity.Functions.External.Common
{
    public class FeedEventProcessor : IFeedEventProcessor
    {
        private readonly IFeedEventOperations _feedEventOperations;
        private readonly ITelemetryHandler _telemetryHandler;
        private ExternalFeedEventSaveModel _feedEventToSave;

        public FeedEventProcessor(IFeedEventOperations feedEventOperations, ITelemetryHandler telemetryHandler)
        {
            _feedEventToSave = null;
            _feedEventOperations = feedEventOperations;
            _telemetryHandler = telemetryHandler;
        }

        // Null checks not required in this method for input parameters as that is being already taken care of at multiple places.
        public async Task<bool> ShouldFeedBeProcessed(string eventType, FeedType feedType, SourceSystem sourceSystem, Guid sourceEntityGuid, DateTime eventRaisedDateTime)
        {
            var feedEvent = await _feedEventOperations.GetExternalFeedEventBySourceEntityGuid(sourceEntityGuid);

            if (IsCreateEvent(eventType))
            {
                if (feedEvent == null)
                {
                    _feedEventToSave = new ExternalFeedEventSaveModel()
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
                        _feedEventToSave = feedEvent.ConvertTo<ExternalFeedEventSaveModel>();
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
                    _feedEventToSave = new ExternalFeedEventSaveModel()
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
                    _feedEventToSave = feedEvent.ConvertTo<ExternalFeedEventSaveModel>();
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
                    _feedEventToSave = feedEvent.ConvertTo<ExternalFeedEventSaveModel>();
                    _feedEventToSave.EventType = eventType;
                    _feedEventToSave.LastEventRaisedDateTime = eventRaisedDateTime;
                    return true;
                }
            }

            return false;
        }

        public async Task UpdateEventRaisedDateTime(Guid destinationEntityGuid)
        {
            if (_feedEventToSave != null)
            {
                _feedEventToSave.DestinationEntityGuid = destinationEntityGuid;
                await _feedEventOperations.SaveExternalFeedEvent(_feedEventToSave);
            }
            else
            {
                _telemetryHandler.TrackTraceWarning($"{nameof(UpdateEventRaisedDateTime)} method called before {nameof(ShouldFeedBeProcessed)} method for {nameof(destinationEntityGuid)}: {destinationEntityGuid}");
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