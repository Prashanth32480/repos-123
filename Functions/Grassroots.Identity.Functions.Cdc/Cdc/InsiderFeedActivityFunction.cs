using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Identity.API.PayLoadModel.Static;
using Grassroots.Identity.Functions.Cdc.Common.EventGridPublish;
    
namespace Grassroots.Identity.Functions.Cdc.Cdc
{
    public class InsiderFeedActivityFunction
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IEventGridPublish _eventGridPublish;
        private readonly IConfigProvider _configuration;

        public InsiderFeedActivityFunction(ITelemetryHandler telemetryHandler
            , IEventGridPublish eventGridPublish
            , IConfigProvider configuration)
        {
            _telemetryHandler = telemetryHandler;
            _eventGridPublish = eventGridPublish;
            _configuration = configuration;
        }

        [FunctionName("InsiderFeedActivityFunction")] public async Task<int> Run([ActivityTrigger]IDurableActivityContext activityContext)
        {
            var request = activityContext.GetInput<FeedActivityFunctionRequest>();

            request.FeedId = null;
            request.CdcApiKey = null;
            request.UserAccountInfo = null;

            if (request.Uid != null)
            {
                var eventGridConnection = _configuration.GetValue(AppSettingsKey.IdentityDomainEventGridEndPoint);
                var eventGridKey = _configuration.GetValue(AppSettingsKey.IdentityDomainEventGridKey);
                var retryCount = int.Parse(_configuration.GetValue(AppSettingsKey.IdentityPublishEventToBeRetryCount) ?? "3");
                var retrySeconds = int.Parse(_configuration.GetValue(AppSettingsKey.IdentityPublishEventToBeRetrySeconds) ?? "5");

                if (request.Preferences != null || request.Subscriptions != null || request.Profile != null || request.Data is { FavTeam: { }} || request.Data is { SyncInsiderPanels: { } })
                {
                    if(request.Data != null && request.Data.FavTeam == null && request.Data.SyncInsiderPanels == null)
                        request.Data = null;

                    _telemetryHandler.TrackTraceInfo($"Sending feed to Identity External event grid. FeedId: {request.FeedId}");

                    await _eventGridPublish.PushMessage(request, EventType.InsiderAccountUpdated,
                        $"/Account/{request.Uid}", eventGridConnection
                        , eventGridKey, retrySeconds, retryCount);
                }

                return 1;
            }

            return 0;
        }
    }
}
