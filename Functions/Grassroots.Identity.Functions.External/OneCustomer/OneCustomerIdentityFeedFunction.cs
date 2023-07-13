// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using System.Threading.Tasks;
using Grassroots.Common.Helpers.Telemetry;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;

namespace Grassroots.Identity.Functions.External.OneCustomer
{
    public class OneCustomerIdentityFeedFunction
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IOneCustomerIdentityFeedHandler _accountFeed;

        //public OneCustomerIdentityFeedFunction(ITelemetryHandler telemetryHandler
        //    , IOneCustomerIdentityFeedHandler accountFeed)
        //{
        //    _accountFeed = accountFeed;
        //    _telemetryHandler = telemetryHandler;
        //}

        //[FunctionName("OneCustomerIdentityFeedFunction")]
        //public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
        // {
        //    _telemetryHandler.TrackTraceInfo($"Received message. InternalEventId: {eventGridEvent.Id}, EventType: {eventGridEvent.EventType}");
            
        //    try
        //    {
        //        switch (eventGridEvent.EventType)
        //        {
        //            case "ONECUSTOMER.ACCOUNT.UPDATED":
        //                await _accountFeed.HandleFeed(eventGridEvent);
        //                break;
        //            default:
        //                _telemetryHandler.TrackEvent($"Unexpected Event Type {eventGridEvent.EventType}, Internal EventGrid Id {eventGridEvent.Id} ");
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _telemetryHandler.TrackTraceInfo($"Exception Thrown for {eventGridEvent.Id}: {ex.Message}");
        //        _telemetryHandler.TrackEvent($"Exception Thrown: {eventGridEvent.Id}");
        //        // Swallowing exceptions through logs. Throw Error reserved for function retry events.
        //        _telemetryHandler.TrackException(ex);
        //    }

        //    _telemetryHandler.TrackTraceInfo($"Completed Function: {eventGridEvent.Id}");
        //}
    }
}