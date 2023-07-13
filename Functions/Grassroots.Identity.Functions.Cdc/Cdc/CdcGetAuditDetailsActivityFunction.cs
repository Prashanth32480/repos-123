using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Grassroots.Common.Helpers.Telemetry;
using System.Net.Http;
using Grassroots.Common.Helpers.Configuration;
using System.Collections.Generic;
using Grassroots.Common.Helpers;
using System;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;

namespace Grassroots.Identity.Functions.Cdc.Cdc
{
    public class CdcGetAuditDetailsActivityFunction
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigProvider _configuration;

        public CdcGetAuditDetailsActivityFunction(ITelemetryHandler telemetryHandler
            , IHttpClientFactory httpClientFactory
            , IConfigProvider configuration)
        {
            _telemetryHandler = telemetryHandler;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [FunctionName("CdcGetAuditDetailsActivityFunction")]
        public async Task<CdcAuditSearchResponse> Run([ActivityTrigger]IDurableActivityContext activityContext)
        {
            var request = activityContext.GetInput<CdcAuditSearchRequest>();
            _telemetryHandler.TrackTraceInfo($"Getting Audit details from CDC. CallId - {request.CallId}. Feed Id - {request.FeedId}. Uid - {request.Uid}.");

            if (!string.IsNullOrWhiteSpace(request.CallId))
            {
                var param = new Dictionary<string, string>();
                param.Add("apiKey", request.ApiKey);
                param.Add("userKey", _configuration.GetValue(AppSettingsKey.CDCUserKey));
                param.Add("secret", _configuration.GetValue(AppSettingsKey.CDCSecretKey));
                param.Add("query", $"SELECT * FROM auditLog WHERE callID = '{request.CallId}'");

                var client = _httpClientFactory.CreateClient();
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.GetValue(AppSettingsKey.CDCAuditApiBaseUrl) + "audit.search");
                httpRequestMessage.Content = new FormUrlEncodedContent(param);
                var response = await client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var cdcResponse = JsonHelper.DeserializeJsonObject<CdcAuditSearchResponse>(result);

                if (cdcResponse.ErrorCode == 0)
                {
                    _telemetryHandler.TrackTraceInfo($"Received audit details from CDC. CallId - {request.CallId}. Feed Id - {request.FeedId}. Uid - {request.Uid}.");
                    return cdcResponse;
                }
                else
                {
                    var error = $"Error occurred while getting audit details from CDC. Call Id: {cdcResponse.CallId}. Error Message: {cdcResponse.StatusReason}. ErrorCode: {cdcResponse.ErrorCode}. CallId - {request.CallId}. Feed Id - {request.FeedId}. Uid - {request.Uid}.";
                    throw new ApplicationException(error);
                }
            }
            return null ;
        }
    }
}
