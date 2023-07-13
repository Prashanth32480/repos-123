// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using System.Threading.Tasks;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Functions.PlayHQ.Profile;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Grassroots.Identity.Functions.PlayHQ.Registration;
using Grassroots.Common.Helpers.Configuration;

namespace Grassroots.Identity.Functions.PlayHQ
{
    public class PlayHQIdentityFeedFunction
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IPlayHQProfileFeedHandler _profileFeed;
        private readonly IPlayHQRegistrationFeedHandler _registrationFeed;
        private readonly IConfigProvider _configuration;

        public PlayHQIdentityFeedFunction(ITelemetryHandler telemetryHandler
            , IPlayHQProfileFeedHandler profileFeed
            , IPlayHQRegistrationFeedHandler registrationFeed
            , IConfigProvider configuration)
        {
            _profileFeed = profileFeed;
            _telemetryHandler = telemetryHandler;
            _registrationFeed = registrationFeed;
            _configuration = configuration;
        }

        [FunctionName("PlayHQIdentityFeedFunction")]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            _telemetryHandler.TrackEvent($"Received message. InternalEventId: {eventGridEvent.Id}, EventType: {eventGridEvent.EventType}");

            try
            {
                var eventObject = eventGridEvent.EventType.IndexOf('.') > 0 ? eventGridEvent.EventType.Substring(0, eventGridEvent.EventType.IndexOf('.')).ToUpper() : eventGridEvent.EventType;

                switch (eventObject)
                {
                    case "COMPETITION_REGISTRATION_TO_SEASON":
                        await _registrationFeed.HandleFeed(eventGridEvent);
                        break;
                    case "COMPETITION_REGISTRATION_TO_CLUB":
                        await _registrationFeed.HandleFeed(eventGridEvent);
                        break;
                    case "COMPETITION_REGISTRATION_TO_TEAM":
                        await _registrationFeed.HandleFeed(eventGridEvent);
                        break;
                    case "COMPETITION_REGISTRATION_TO_CLUB_TEAM":
                        await _registrationFeed.HandleFeed(eventGridEvent);
                        break;
                    case "SHARED_PROGRAM_REGISTRATION":
                        await _registrationFeed.HandleFeed(eventGridEvent);
                        break;
                    case "PROFILE":
                        if (eventGridEvent.EventType == "PROFILE.CLAIMED")
                            await _profileFeed.HandleClaimFeed(eventGridEvent);
                        else
                        {
                            await Task.Delay(Convert.ToInt32(_configuration.GetValue(AppSettingsKey.PhqProfileUpdatedFeedDelay)));//putting delay to avoid out of order issue
                            await _profileFeed.HandleFeed(eventGridEvent);
                        }
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

            _telemetryHandler.TrackEvent($"Completed Function: {eventGridEvent.Id}");
        }
    }
}