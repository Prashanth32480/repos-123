using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Grassroots.Common.Helpers.Telemetry;
using System.Net.Http;
using Grassroots.Common.Helpers.Configuration;
using System.Collections.Generic;
using Grassroots.Common.Helpers;
using System;
using System.Linq;

namespace Grassroots.Identity.Functions.Cdc.Cdc
{
    public class CdcGetAccountInfoActivityFunction
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigProvider _configuration;

        public CdcGetAccountInfoActivityFunction(ITelemetryHandler telemetryHandler
            , IHttpClientFactory httpClientFactory
            , IConfigProvider configuration)
        {
            _telemetryHandler = telemetryHandler;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [FunctionName("CdcGetAccountInfoActivityFunction")]
        public async Task<CdcGetAccountInfoResponse> Run([ActivityTrigger]IDurableActivityContext activityContext)
        {
            var request = activityContext.GetInput<CdcGetAccountInfoRequest>();
            _telemetryHandler.TrackTraceInfo($"Getting User's account info details. Feed Id - {request.FeedId}. Uid - {request.Uid}.");

            if (!string.IsNullOrWhiteSpace(request.Uid))
            {
                if (request.HasFullAccount == true)
                    return await GetFullAccountInfoAsync(request);
                else
                    return await GetLiteAccountInfoAsync(request);
                
            }

            return null;
        }

        private async Task<CdcGetAccountInfoResponse> GetFullAccountInfoAsync(CdcGetAccountInfoRequest request)
        {
            var param = new Dictionary<string, string>();
            param.Add("apiKey", _configuration.GetValue(AppSettingsKey.CDCApiKey));
            param.Add("userKey", _configuration.GetValue(AppSettingsKey.CDCUserKey));
            param.Add("secret", _configuration.GetValue(AppSettingsKey.CDCSecretKey));
            param.Add("UID", request.Uid);

            var client = _httpClientFactory.CreateClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.GetValue(AppSettingsKey.CDCAccountApiBaseUrl) + "accounts.getAccountInfo");
            httpRequestMessage.Content = new FormUrlEncodedContent(param);
            var response = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var cdcResponse = JsonHelper.DeserializeJsonObject<CdcGetAccountInfoResponse>(result);

            if (cdcResponse.ErrorCode == 0)
            {
                _telemetryHandler.TrackTraceInfo($"Received Users details from CDC. Feed Id - {request.FeedId}. Uid - {request.Uid}.");
                return cdcResponse;
            }
            else
            {
                var error = $"Error occurred while getting user details from CDC. Call Id: {cdcResponse.CallId}. Error Message: {cdcResponse.ErrorMessage}. ErrorCode: {cdcResponse.ErrorCode}. Feed Id - {request.FeedId}. Uid - {request.Uid}.";
                throw new ApplicationException(error);
            }
        }
        private async Task<CdcGetAccountInfoResponse> GetLiteAccountInfoAsync(CdcGetAccountInfoRequest request)
        {
            var param = new Dictionary<string, string>();
            CdcGetAccountInfoResponse returnResponse = new CdcGetAccountInfoResponse();
            param.Add("apiKey", _configuration.GetValue(AppSettingsKey.CDCApiKey));
            param.Add("userKey", _configuration.GetValue(AppSettingsKey.CDCUserKey));
            param.Add("secret", _configuration.GetValue(AppSettingsKey.CDCSecretKey));
            param.Add("query", $"SELECT data, created, lastUpdated FROM emailAccounts WHERE UID=\"{request.Uid}\"");

            var client = _httpClientFactory.CreateClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.GetValue(AppSettingsKey.CDCAccountApiBaseUrl) + "accounts.search");
            httpRequestMessage.Content = new FormUrlEncodedContent(param);
            var response = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var cdcResponse = JsonHelper.DeserializeJsonObject<CdcGetEmailAccountsResponse>(result);

            if (cdcResponse.ErrorCode == 0)
            {
                _telemetryHandler.TrackTraceInfo($"Received Users details from CDC. Feed Id - {request.FeedId}. Uid - {request.Uid}.");

                if (cdcResponse.Results.Any())
                {
                    returnResponse.Created = cdcResponse.Results.FirstOrDefault().Created;
                    returnResponse.LastUpdated = cdcResponse.Results.FirstOrDefault().LastUpdated;
                    returnResponse.Data = cdcResponse.Results.FirstOrDefault().Data;
                    returnResponse.UID = request.Uid;
                }

                return returnResponse;
            }
            else
            {
                var error = $"Error occurred while getting user details from CDC. Call Id: {cdcResponse.CallId}. Error Message: {cdcResponse.ErrorMessage}. ErrorCode: {cdcResponse.ErrorCode}. Feed Id - {request.FeedId}. Uid - {request.Uid}.";
                throw new ApplicationException(error);
            }
        }
    }
}
