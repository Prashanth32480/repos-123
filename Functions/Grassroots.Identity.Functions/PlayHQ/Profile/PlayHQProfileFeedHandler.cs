using System;
using System.Threading.Tasks;
using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Identity.Functions.Common;
using Grassroots.Identity.Functions.Common.Models;
using Grassroots.Identity.Functions.PlayHQ.Profile.Models;
using Microsoft.Azure.EventGrid.Models;

namespace Grassroots.Identity.Functions.PlayHQ.Profile
{
    public class PlayHQProfileFeedHandler : IPlayHQProfileFeedHandler
    {

        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IRawFeedProcessor _rawFeedProcessor;
        private readonly IPlayHQProfileFeedProcessor _feedProcessor;
        private readonly IFeatureFlag _featureFlag;
        private readonly IFeedEventProcessor _feedEventProcessor;

        private const string UpdateEvent = "UPDATED";
        private const string ClaimEvent = "CLAIMED";

        //private const SourceSystem SourceSystem = Database.Model.Static.SourceSystem.PlayHQ;

        public PlayHQProfileFeedHandler(IPlayHQProfileFeedProcessor feedProcessor, IRawFeedProcessor rawFeedProcessor, IFeedEventProcessor feedEventProcessor, ITelemetryHandler telemetryHandler, IFeatureFlag featureFlag)
        {
            _featureFlag = featureFlag;
            _rawFeedProcessor = rawFeedProcessor;
            _telemetryHandler = telemetryHandler;
            _feedProcessor = feedProcessor;
            _feedEventProcessor = feedEventProcessor;
        }

        public async Task HandleFeed(EventGridEvent eventGridEvent)
        {
            _telemetryHandler.TrackTraceInfo($"Starting handling proflie feed");

            PlayHQFeed<PlayHQProfileData> feed;
            try
            {
                feed = JsonHelper.DeserializeJsonObject<PlayHQFeed<PlayHQProfileData>>(eventGridEvent.Data.ToString());
            }
            catch (Exception ex)
            {
                ex.Data.Add(nameof(eventGridEvent.Id), eventGridEvent.Id);
                throw;
            }

            if (feed.MessageId == Guid.Empty)
            {
                throw new ApplicationException($"Received invalid or empty PlayHQ Profile Feed.");
            }

            var messageId = feed.MessageId.ToString();

            long rawFeedId = await ValidateAndSaveRawFeed(eventGridEvent, messageId);

            switch (feed.EventType.Split('.')[1])
            {
                case UpdateEvent:
                    await _feedProcessor.ProcessFeed(feed.Data, rawFeedId);
                    _telemetryHandler.TrackTraceInfo($"Record successfully Updated for PlayHQ profile feed. RawFeedId: {rawFeedId}.");
                    break;
                default:
                    _telemetryHandler.TrackTraceInfo($"Unknown Event Type. RawFeedId: {rawFeedId}.");
                    return;
            }

            await _rawFeedProcessor.SetRawFeedStatusToSuccess(rawFeedId);

            _telemetryHandler.TrackTraceInfo($"Processed PlayHQ Profile Feed {rawFeedId}");
        }

        public async Task HandleClaimFeed(EventGridEvent eventGridEvent)
        {
            _telemetryHandler.TrackTraceInfo($"Starting handling proflie feed");

            PlayHQFeed<PlayHQClaimData> claimFeed;

            try
            {
                claimFeed = JsonHelper.DeserializeJsonObject<PlayHQFeed<PlayHQClaimData>>(eventGridEvent.Data.ToString());
            }
            catch (Exception ex)
            {
                ex.Data.Add(nameof(eventGridEvent.Id), eventGridEvent.Id);
                throw;
            }

            if (claimFeed.MessageId == Guid.Empty)
            {
                throw new ApplicationException($"Received invalid or empty PlayHQ Profile Claim Feed.");
            }

            var messageId = claimFeed.MessageId.ToString();

            long rawFeedId = await ValidateAndSaveRawFeed(eventGridEvent, messageId);

            switch (claimFeed.EventType.Split('.')[1])
            {
                case ClaimEvent:
                    await _feedProcessor.ProcessClaimFeed(claimFeed.Data, rawFeedId);
                    _telemetryHandler.TrackTraceInfo($"Record successfully Claimed for PlayHQ profile feed. RawFeedId: {rawFeedId}.");
                    break;
                default:
                    _telemetryHandler.TrackTraceInfo($"Unknown Event Type. RawFeedId: {rawFeedId}.");
                    return;
            }

            await _rawFeedProcessor.SetRawFeedStatusToSuccess(rawFeedId);

            _telemetryHandler.TrackTraceInfo($"Processed PlayHQ Profile Feed {rawFeedId}");
        }

        private async Task<long> ValidateAndSaveRawFeed(EventGridEvent eventGridEvent, string messageId)
        {
            if (await _rawFeedProcessor.CheckRawFeedExists(messageId))
            {
                throw new ApplicationException($"Duplicate MessageId. {messageId} has already been received. ");
            }

            FeedType feedType = FeedType.Profile;

            var rawFeedId = await _rawFeedProcessor.InsertRawFeed(feedType, messageId, eventGridEvent.Id,
                feedType.ToString(), eventGridEvent.EventTime);

            _telemetryHandler.TrackTraceInfo($"Feed Saved in DB. RawFeedId: {rawFeedId}.");
            return rawFeedId;
        }
    }
}