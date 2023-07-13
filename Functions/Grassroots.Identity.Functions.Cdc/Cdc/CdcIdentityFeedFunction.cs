// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using System.Threading.Tasks;
using Grassroots.Common.Helpers.Telemetry;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts;
using Grassroots.Common.Helpers.Configuration;

namespace Grassroots.Identity.Functions.Cdc.Cdc
{
    public class CdcIdentityFeedFunction
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly ICdcAccountFeedHandler _accountFeed;
        private readonly IConfigProvider _configuration;

        public CdcIdentityFeedFunction(ITelemetryHandler telemetryHandler, ICdcAccountFeedHandler accountFeed, IConfigProvider configuration)
        {
            _accountFeed = accountFeed;
            _telemetryHandler = telemetryHandler;
            _configuration = configuration;
        }

        [FunctionName("CdcIdentityFeedFunction")]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, [DurableClient] IDurableOrchestrationClient starter)
        {
            _telemetryHandler.TrackTraceInfo($"Received message. InternalEventId: {eventGridEvent.Id}, EventType: {eventGridEvent.EventType}");
            
            try
            {
                switch (eventGridEvent.EventType)
                {
                    case "Identity.Account.BatchedEvents":
                        await Task.Delay(Convert.ToInt32(_configuration.GetValue(AppSettingsKey.CDCAuditCallDelay)));//putting delay to receive the audit details from cdc
                        await _accountFeed.HandleFeed(eventGridEvent, starter);
                        break;
                    default:
                        _telemetryHandler.TrackEvent($"Unexpected Event Type {eventGridEvent.EventType}, Internal EventGrid Id {eventGridEvent.Id} ");
                        break;
                }
            }
            catch (Exception ex)
            {
                _telemetryHandler.TrackTraceInfo($"Exception Thrown for {eventGridEvent.Id}: {ex.Message}");
                _telemetryHandler.TrackEvent($"Exception Thrown: {eventGridEvent.Id}");
                // Swallowing exceptions through logs. Throw Error reserved for function retry events.
                _telemetryHandler.TrackException(ex);
            }

            _telemetryHandler.TrackTraceInfo($"Completed Function: {eventGridEvent.Id}");
        }
    }
}