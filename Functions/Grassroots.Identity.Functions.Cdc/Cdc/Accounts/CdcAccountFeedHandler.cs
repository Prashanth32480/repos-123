using System;
using System.Threading.Tasks;
using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Grassroots.Identity.Functions.Cdc.Common;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts
{
    public class CdcAccountFeedHandler : ICdcAccountFeedHandler
    {

        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IRawFeedProcessor _rawFeedProcessor;
        private readonly ICdcAccountFeedProcessor _feedProcessor;
        private readonly IFeatureFlag _featureFlag;
        private readonly IFeedEventProcessor _feedEventProcessor;

        private const SourceSystem SourceSystem = Database.Model.Static.SourceSystem.CDC;

        public CdcAccountFeedHandler(ICdcAccountFeedProcessor feedProcessor, IRawFeedProcessor rawFeedProcessor, IFeedEventProcessor feedEventProcessor, ITelemetryHandler telemetryHandler, IFeatureFlag featureFlag)
        {
            _featureFlag = featureFlag;
            _rawFeedProcessor = rawFeedProcessor;
            _telemetryHandler = telemetryHandler;
            _feedProcessor = feedProcessor;
            _feedEventProcessor = feedEventProcessor;
        }

        public async Task HandleFeed(EventGridEvent eventGridEvent, IDurableOrchestrationClient starter)
        {
            CdcAccountFeed feed;

            try
            {
                _telemetryHandler.TrackTraceInfo($"eventGridEvent.Data : {eventGridEvent.Data.ToString()}");
                feed = JsonHelper.DeserializeJsonObject<CdcAccountFeed>(eventGridEvent.Data.ToString());
                
            }
            catch (Exception ex)
            {
                _telemetryHandler.TrackTraceInfo($"Error => {ex.Message}");
                ex.Data.Add(nameof(eventGridEvent.Id), eventGridEvent.Id);
                throw;
            }

            if (feed.Events == null)
            {
                throw new ApplicationException($"Received invalid or empty CDC Accounts Feed.");
            }


            foreach (var cdcEvent in feed.Events)
            {
                var messageId = cdcEvent.Id;

                if (await _rawFeedProcessor.CheckRawFeedExists(messageId))
                {
                    _telemetryHandler.TrackTraceInfo($"Duplicate MessageId. {messageId} has already been received.");
                }
                else
                {
                    var rawFeedId = await _rawFeedProcessor.InsertRawFeed(FeedType.Account, messageId, eventGridEvent.Id,
                    FeedType.Account.ToString(), eventGridEvent.EventTime);

                    cdcEvent.FeedId = rawFeedId;

                    await _feedProcessor.ProcessEvent(cdcEvent, starter);
                    _telemetryHandler.TrackTraceInfo($"Processed CDC Accounts Feed for message Id {messageId}. RawFeedId: {rawFeedId}.");
                }
            }
        }
       
    }
}