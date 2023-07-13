using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Identity.API.PayLoadModel.Static;
using Grassroots.Identity.Functions.Cdc.Common.EventGridPublish;
using Grassroots.Identity.API.PayLoadModel.PayLoads;
using ServiceStack;

namespace Grassroots.Identity.Functions.Cdc.Cdc
{
    public class OneCustomerFeedActivityFunction
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IEventGridPublish _eventGridPublish;
        private readonly IConfigProvider _configuration;

        public OneCustomerFeedActivityFunction(ITelemetryHandler telemetryHandler
            , IEventGridPublish eventGridPublish
            , IConfigProvider configuration)
        {
            _telemetryHandler = telemetryHandler;
            _eventGridPublish = eventGridPublish;
            _configuration = configuration;
        }

        [FunctionName("OneCustomerFeedActivityFunction")] public async Task<int> Run([ActivityTrigger]IDurableActivityContext activityContext)
        {
            var request = activityContext.GetInput<FeedActivityFunctionRequest>();

            request.FeedId = null;

            var cdcApiKey = _configuration.GetValue(AppSettingsKey.OneCustomerCdcApiKey);

            if(string.IsNullOrWhiteSpace(request.CdcApiKey))
                _telemetryHandler.TrackTraceInfo($"CdcApiKey not received from orchestrator function. FeedId: {request.FeedId}");
            else if (cdcApiKey.Equals(request.CdcApiKey) && request.Preferences != null && request.Preferences.Consent != null && request.Preferences.Consent.Contains("panel."))
            {
                _telemetryHandler.TrackTraceInfo($"Panel updated by OneCustomer. Sending it back to OneCustomer. FeedId: {request.FeedId}");

                request.Subscriptions = null;
                return await PublishEventToOneCustomerAsync(request);

            }
            else if (cdcApiKey.Equals(request.CdcApiKey))
                _telemetryHandler.TrackTraceInfo($"Feed triggered by OneCustomer. Hence not forwarding it to oneCustomer again. FeedId: {request.FeedId}");
            else
            {
                if (request.Profile != null)
                    await PublishEventToOneCustomerAsync(request, true);
                else if (request.Preferences != null || request.Subscriptions != null)
                    await PublishEventToOneCustomerAsync(request);

                return 1;

            }
            return 0;
        }

        private async Task<int> PublishEventToOneCustomerAsync(FeedActivityFunctionRequest request, bool  isProfileUpdate  = false)
        {
            var eventGridConnection = _configuration.GetValue(AppSettingsKey.IdentityDomainEventGridEndPoint);
            var eventGridKey = _configuration.GetValue(AppSettingsKey.IdentityDomainEventGridKey);
            var retryCount = int.Parse(_configuration.GetValue(AppSettingsKey.IdentityPublishEventToBeRetryCount) ?? "3");
            var retrySeconds = int.Parse(_configuration.GetValue(AppSettingsKey.IdentityPublishEventToBeRetrySeconds) ?? "5");
            dynamic oneCustomerPayload;

            if (isProfileUpdate)
            {
                oneCustomerPayload = request.ConvertTo<OneCustomerProfileUpdatePayload>();
                oneCustomerPayload.Created = request.UserAccountInfo!= null ? request.UserAccountInfo.Created : null;
                oneCustomerPayload.LastUpdated = request.UserAccountInfo != null ? request.UserAccountInfo.LastUpdated : null;
            }
            else
                oneCustomerPayload = request.ConvertTo<OneCustomerPayload>();

            _telemetryHandler.TrackTraceInfo($"Sending feed to Identity External event grid. FeedId: {request.FeedId}");

            await _eventGridPublish.PushMessage(oneCustomerPayload, EventType.OneCustomerAccountUpdated, $"/Account/{request.Uid}", eventGridConnection
                , eventGridKey, retrySeconds, retryCount);

            return 1;

        }

    }
}
