// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using System.Threading.Tasks;
using Grassroots.Common.BlobStorage.ServiceInterface;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Common.Helpers.Telemetry;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;

namespace Grassroots.Identity.Functions.External.OneCustomer
{
    public class OneCustomerIdentityBlobStoreFunction
    {
        private readonly IStoreFeed _storeFeed;
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IConfigProvider _configuration;
        private readonly IFeatureFlag _featureFlag;

        public OneCustomerIdentityBlobStoreFunction(IStoreFeed storeFeed
            , ITelemetryHandler telemetryHandler
            , IConfigProvider configuration
            , IFeatureFlag featureFlag)
        {
            _storeFeed = storeFeed;
            _telemetryHandler = telemetryHandler;
            _configuration = configuration;
            _featureFlag = featureFlag;
        }

        [FunctionName("OneCustomerIdentityBlobStoreFunction")]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            _telemetryHandler.TrackTraceInfo("Received OneCustomer Feed Message");
            if (_featureFlag.FlagEnabled("st-2023-05-10-save-one-customer-feeds-into-blob-storage"))
            {
                await SaveFeed(eventGridEvent);
            }
            else
            {
                _telemetryHandler.TrackTraceInfo("Feed not being saved into blob storage as feature flag is off.");
            }

            _telemetryHandler.TrackTraceInfo("Finished Processing OneCustomer Feed Message");
        }

        private async Task SaveFeed(EventGridEvent eventGridEvent)
        {
            try
            {
                _telemetryHandler.TrackTraceInfo("Saving OneCustomer Feed Message");

                var storageConnection = _configuration.GetValue(AppSettingsKey.IdentityRawFeedStorageConnection);
                if (string.IsNullOrWhiteSpace(storageConnection))
                {
                    _telemetryHandler.TrackException(
                        new ApplicationException($"Invalid storage connection string."));
                    return;
                }

                var containerName = _configuration.GetValue(AppSettingsKey.IdentityOneCustomerFeedsContainer);
                if (string.IsNullOrWhiteSpace(containerName))
                {
                    _telemetryHandler.TrackException(
                        new ApplicationException($"Invalid container name: {containerName}."));
                    return;
                }

                var feed = eventGridEvent.Data?.ToString();
                if (string.IsNullOrWhiteSpace(feed))
                {
                    _telemetryHandler.TrackException(
                        new ApplicationException($"Received invalid or empty Feed. EventGrid Id: {eventGridEvent.Id}"));
                    return;
                }

                await _storeFeed.SaveFeed(storageConnection, containerName, feed, eventGridEvent.Id);

                _telemetryHandler.TrackTraceInfo("Finished Saving Feed Message");
            }
            catch (Exception ex)
            {
                _telemetryHandler.TrackException(ex);
                _telemetryHandler.TrackTraceError("Error Saving Feed Message");
            }
        }
    }
}
