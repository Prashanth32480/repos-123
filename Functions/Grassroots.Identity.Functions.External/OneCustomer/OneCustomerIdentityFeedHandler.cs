using System;
using System.Threading.Tasks;
using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Identity.Functions.External.Common;
using Grassroots.Identity.Functions.External.Common.Model;
using Microsoft.Azure.EventGrid.Models;
namespace Grassroots.Identity.Functions.External.OneCustomer
{
    public class OneCustomerIdentityFeedHandler : IOneCustomerIdentityFeedHandler
    {

        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IRawFeedProcessor _rawFeedProcessor;
        private readonly IOneCustomerIdentityFeedProcessor _feedProcessor;
        private readonly IFeatureFlag _featureFlag;
        private readonly IFeedEventProcessor _feedEventProcessor;

        //private const SourceSystem SourceSystem = SourceSystem.CDC;

        public OneCustomerIdentityFeedHandler(IOneCustomerIdentityFeedProcessor feedProcessor, IRawFeedProcessor rawFeedProcessor, IFeedEventProcessor feedEventProcessor, ITelemetryHandler telemetryHandler, IFeatureFlag featureFlag)
        {
            _featureFlag = featureFlag;
            _rawFeedProcessor = rawFeedProcessor;
            _telemetryHandler = telemetryHandler;
            _feedProcessor = feedProcessor;
            _feedEventProcessor = feedEventProcessor;
        }

        public async Task HandleFeed(EventGridEvent eventGridEvent)
        {
            IdentityExternalFeedRequest feed;

            try
            {
                _telemetryHandler.TrackTraceInfo($"eventGridEvent.Data : {eventGridEvent.Data.ToString()}");
                feed = JsonHelper.DeserializeJsonObject<IdentityExternalFeedRequest>(eventGridEvent.Data.ToString());

            }
            catch (Exception ex)
            {
                _telemetryHandler.TrackTraceInfo($"Error => {ex.Message}");
                ex.Data.Add(nameof(eventGridEvent.Id), eventGridEvent.Id);
                throw;
            }

            if (string.IsNullOrWhiteSpace(eventGridEvent.Id))
            {
                throw new ApplicationException($"Received invalid or empty identity OneCustomer Feed.");
            }


            var messageId = eventGridEvent.Id;

            if (await _rawFeedProcessor.CheckRawFeedExists(messageId))
            {
                throw new ApplicationException($"Duplicate MessageId. {messageId} has already been received.");
            }

            string blobId = string.Empty;

            if (_featureFlag.FlagEnabled("st-2023-05-10-save-one-customer-feeds-into-blob-storage"))
                blobId = eventGridEvent.Id;

            var rawFeedId = await _rawFeedProcessor.InsertRawFeed(FeedType.OneCustomer, messageId, blobId,
            FeedType.OneCustomer.ToString(), eventGridEvent.EventTime);


            feed.FeedId = rawFeedId;

            await _feedProcessor.ProcessEvent(feed);

            await _rawFeedProcessor.SetRawFeedStatusToSuccess(rawFeedId);

            _telemetryHandler.TrackTraceInfo($"Processed OneCustomer Feed for message Id {messageId}. RawFeedId: {rawFeedId}.");
            
        }

    }
}