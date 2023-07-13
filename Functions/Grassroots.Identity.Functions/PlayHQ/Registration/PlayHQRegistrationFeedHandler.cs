using System;
using System.Threading.Tasks;
using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Identity.Functions.Common;
using Grassroots.Identity.Functions.Common.Models;
using Microsoft.Azure.EventGrid.Models;

namespace Grassroots.Identity.Functions.PlayHQ.Registration
{
    public class PlayHQRegistrationFeedHandler : IPlayHQRegistrationFeedHandler
    {

        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IRawFeedProcessor _rawFeedProcessor;
        private readonly IPlayHQRegistrationFeedProcessor _feedProcessor;
        private readonly IFeatureFlag _featureFlag;
        private readonly IFeedEventProcessor _feedEventProcessor;

        private const string CreateEvent = "CREATED";
        private const string UpdateEvent = "UPDATED";
        private const string DeleteEvent = "DELETED";
        private const SourceSystem SourceSystem = Database.Model.Static.SourceSystem.PlayHQ;

        public PlayHQRegistrationFeedHandler(IPlayHQRegistrationFeedProcessor feedProcessor, IRawFeedProcessor rawFeedProcessor, IFeedEventProcessor feedEventProcessor, ITelemetryHandler telemetryHandler, IFeatureFlag featureFlag)
        {
            _featureFlag = featureFlag;
            _rawFeedProcessor = rawFeedProcessor;
            _telemetryHandler = telemetryHandler;
            _feedProcessor = feedProcessor;
            _feedEventProcessor = feedEventProcessor;
        }

        public async Task HandleFeed(EventGridEvent eventGridEvent)
        {
            _telemetryHandler.TrackEvent($"Starting handling PlayHQ Registration Feed.");
            //if (_featureFlag.FlagEnabled("KP-1031-TestFlagForAPIEndpoints"))
            //{
            //    _telemetryHandler.TrackTraceInfo("Feature Flag triggered");
            //}
            //_telemetryHandler.TrackEvent("Begin processing PlayHQ Profile Feed");

            PlayHQFeed<PlayHQData> feed;

            try
            {
                feed = JsonHelper.DeserializeJsonObject<PlayHQFeed<PlayHQData>>(eventGridEvent.Data.ToString());
            }
            catch (Exception ex)
            {
                ex.Data.Add(nameof(eventGridEvent.Id), eventGridEvent.Id);
                throw;
            }

            if (feed.MessageId == Guid.Empty)
            {
                throw new ApplicationException($"Received invalid or empty PlayHQ Registration Feed.");
            }

            var messageId = feed.MessageId.ToString();

            if (await _rawFeedProcessor.CheckRawFeedExists(messageId))
            {
                throw new ApplicationException($"Duplicate MessageId. {messageId} has already been received. ");
            }

            FeedType feedType = GetFeedType(eventGridEvent);

            var rawFeedId = await _rawFeedProcessor.InsertRawFeed(feedType, messageId, eventGridEvent.Id,
                feedType.ToString(), eventGridEvent.EventTime);

            //if (feed.EventRaisedDateTime != null && !await _feedEventProcessor.ShouldFeedBeProcessed(feed.EventType, feedType, SourceSystem,
            //    feed.Data.Id, feed.EventRaisedDateTime.Value))
            //{
            //    throw new ApplicationException($"Event feed received out of order. FeedId: {rawFeedId}.");
            //}

            _telemetryHandler.TrackTraceInfo($"Record Inserted in DB. RawFeedId - {rawFeedId}");

            Guid participantGuid;
            
            switch (feed.EventType.Split('.')[1])
            {
                case CreateEvent:
                    participantGuid = await _feedProcessor.CreateParticipant(feed.Data, feedType, rawFeedId);
                    _telemetryHandler.TrackTraceInfo($"Record successfully Created for PlayHQ Registration feed. RawFeedId: {rawFeedId}. Result: {participantGuid}.");
                    break;
                case UpdateEvent:
                    participantGuid = await _feedProcessor.UpdateParticipant(feed.Data, feedType, rawFeedId);
                    _telemetryHandler.TrackTraceInfo($"Record successfully Updated for PlayHQ Registration feed. RawFeedId: {rawFeedId}. Result: {participantGuid}.");
                    break;
                case DeleteEvent:
                    participantGuid = await _feedProcessor.DeleteParticipant(feed.Data, rawFeedId);
                    _telemetryHandler.TrackTraceInfo($"Record successfully Deleted for PlayHQ Registration feed. RawFeedId: {rawFeedId}. Result: {participantGuid}.");
                    break;
                default:
                    _telemetryHandler.TrackTraceInfo($"Unknown Event Type. RawFeedId: {rawFeedId}.");
                    return;
            }


            //await _feedEventProcessor.UpdateEventRaisedDateTime(participantGuid);

            await _rawFeedProcessor.SetRawFeedStatusToSuccess(rawFeedId);

            _telemetryHandler.TrackEvent($"Processed PlayHQ Registration Feed {rawFeedId}");
        }

        private FeedType GetFeedType(EventGridEvent eventGridEvent)
        {
            var eventObject = eventGridEvent.EventType.IndexOf('.') > 0 ? eventGridEvent.EventType.Substring(0, eventGridEvent.EventType.IndexOf('.')).ToUpper() : eventGridEvent.EventType;

            if (eventObject.Equals("SHARED_PROGRAM_REGISTRATION"))
                return FeedType.Program;
            else
                return FeedType.Competition;
        }
    }
}