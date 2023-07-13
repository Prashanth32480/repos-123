using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using System;
using System.Threading.Tasks;
using Grassroots.Common.PublishEvents.ChangeTrack;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Grassroots.Identity.Functions.Cdc.Common;
using Grassroots.Identity.Functions.Cdc.Common.Enum;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts
{
    public class CdcAccountFeedProcessor : ICdcAccountFeedProcessor
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IConfigProvider _configuration;
        private readonly IRawFeedProcessor _rawFeedProcessor;

        public CdcAccountFeedProcessor(ITelemetryHandler telemetryHandler
            , IConfigProvider configuration
            , IRawFeedProcessor rawFeedProcessor)
        {
            _telemetryHandler = telemetryHandler;
            _configuration = configuration;
            _rawFeedProcessor = rawFeedProcessor;
        }

        public async Task ProcessEvent(CdcEvent cdcEvent, IDurableOrchestrationClient starter)
        {
            ValidatePlayHqCreateFeed(cdcEvent);

            var isProcessFeed = _configuration.GetValue(AppSettingsKey.CdcApiKeysToProcessFeeds).Contains(cdcEvent.ApiKey);
            if (isProcessFeed)
                await PublishEvent(cdcEvent, starter);
            else
            {
                await _rawFeedProcessor.SetRawFeedStatusToSuccess(cdcEvent.FeedId);
                _telemetryHandler.TrackTraceInfo($"Feeds from ApiKey {cdcEvent.ApiKey} are not configured to be processed. Feed Id - {cdcEvent.FeedId}");
            }

            
        }

        private async Task PublishEvent(CdcEvent cdcEvent, IDurableOrchestrationClient starter)
        {            
            string json = JsonConvert.SerializeObject(cdcEvent);
            var requestData = new StringContent(json, Encoding.UTF8, "application/json");
            
            HttpRequestMessage req = new HttpRequestMessage();
            req.Content = requestData;

            
            string instanceId = await starter.StartNewAsync("CdcIdentityOrchestratorFunction", await req.Content.ReadAsAsync<object>());
            _telemetryHandler.TrackTraceInfo($"Stating Orchestrator function. Instance Id - {instanceId}");
            starter.CreateCheckStatusResponse(req, instanceId);
            _telemetryHandler.TrackTraceInfo($"Completed executing Orchestrator function.");

        }

        private void ValidatePlayHqCreateFeed(CdcEvent cdcEvent)
        {
            if (string.IsNullOrWhiteSpace(cdcEvent.Type))
            {
                var error = $"Required Value missing for CDC Account Feed (Data.Events.Type)";
                throw new ApplicationException(error);
            }

            if (cdcEvent.Type.ToLower() != CdcEventType.AccountUpdated.ToString().ToLower() && cdcEvent.Type.ToLower() != CdcEventType.ConsentUpdated.ToString().ToLower() && cdcEvent.Type.ToLower() != CdcEventType.SubscriptionUpdated.ToString().ToLower())
            {
                var error = $"Currently only processing accountUpdated/ ConsentUpdated/ SubscriptionUpdated  CDC Events. Received CDC Event Type is {cdcEvent.Type}";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(cdcEvent.Id))
            {
                var error = $"Required Value missing for CDC Account Feed (Data.Events.Id)";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(cdcEvent.ApiKey))
            {
                var error = $"Required Value missing for CDC Account Feed (Data.Events.ApiKey)";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(cdcEvent.Data.UId))
            {
                var error = $"Required Value missing for CDC Account Feed (Data.Events.Data.Uid)";
                throw new ApplicationException(error);
            }
        }

    }
}
