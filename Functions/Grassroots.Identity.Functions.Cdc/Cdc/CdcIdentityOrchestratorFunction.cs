using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Common.Helpers;
using System;
using Grassroots.Identity.Functions.Cdc.Common.Enum;
using System.Reflection;

namespace Grassroots.Identity.Functions.Cdc.Cdc
{
    public class CdcIdentityOrchestratorFunction
    {
        private readonly ITelemetryHandler _telemetryHandler;

        public CdcIdentityOrchestratorFunction(ITelemetryHandler telemetryHandler)
        {
            _telemetryHandler = telemetryHandler;
        }

        [FunctionName("CdcIdentityOrchestratorFunction")]
        public async Task<int> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            try
            {
                CdcGetAccountInfoResponse userDetails = null;
                FeedActivityFunctionRequest feedRequest = null;
                string updatedData = null;
                string updatedProfile = null;
                SubscriptionRequest updatedSubscription = null;
                ConsentRequest updatedConsent = null;
                bool? hasFullAccount = null;

                CdcEvent data = context.GetInput<CdcEvent>();
                var parallelTasks = new List<Task<int>>();

                if (!context.IsReplaying)
                    _telemetryHandler.TrackTraceInfo(
                        $"Starting with Orchestrator Function. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");
                if (!context.IsReplaying)
                    _telemetryHandler.TrackTraceInfo(
                        $"Data Received in Orchestrator function => {JsonHelper.SerializeJsonObject(data)}. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");

                if (data.Data.AccountType != null && data.Data.AccountType.ToLower().Equals("full"))
                    hasFullAccount = true;
                else if ((data.Data.AccountType != null && data.Data.AccountType.ToLower().Equals("lite")))
                    hasFullAccount = false;

                if (data.Type.ToLower() == CdcEventType.AccountUpdated.ToString().ToLower())
                {
                    if (!context.IsReplaying)
                        _telemetryHandler.TrackTraceInfo($"Getting Cdc Audit Details for callId {data.CallId}. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");

                    CdcAuditSearchRequest auditRequest = new CdcAuditSearchRequest()
                    {
                        CallId = data.CallId,
                        ApiKey = data.ApiKey,
                        FeedId = data.FeedId,
                        Uid = data.Data.UId
                    };

                    var retryOptionsForAuditFunction =
                        new RetryOptions(firstRetryInterval: TimeSpan.FromSeconds(5), maxNumberOfAttempts: 3);

                    var auditResponse =
                        await context.CallActivityWithRetryAsync<CdcAuditSearchResponse>(
                            "CdcGetAuditDetailsActivityFunction", retryOptionsForAuditFunction, auditRequest);

                    if (auditResponse != null && auditResponse.Results.Any() && auditResponse.Results.FirstOrDefault().Params != null)
                    {
                        if (auditResponse.Results.FirstOrDefault().Params.Data != null)
                        {
                            updatedData = auditResponse.Results.FirstOrDefault().Params.Data;
                            _telemetryHandler.TrackTraceInfo($"Cdc Data field updated by user. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");
                        }

                        if (auditResponse.Results.FirstOrDefault().Params.Profile != null)
                        {
                            updatedProfile = auditResponse.Results.FirstOrDefault().Params.Profile;
                            _telemetryHandler.TrackTraceInfo($"Cdc Data field updated by user. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");
                        }
                    }
                    else
                        _telemetryHandler.TrackTraceInfo($"Not received the response from Audit call. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");
                }
                else if (data.Type.ToLower() == CdcEventType.ConsentUpdated.ToString().ToLower())
                {
                    updatedConsent = new ConsentRequest()
                    {
                        Consent = data.Data.ConsentId,
                        IsConsentGranted = data.Data.NewConsentState.IsConsentGranted
                    };
                    _telemetryHandler.TrackTraceInfo($"consent updated by user. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");
                }
                else if (data.Type.ToLower() == CdcEventType.SubscriptionUpdated.ToString().ToLower())
                {
                    updatedSubscription = new SubscriptionRequest();

                    foreach (var parent in data.Data.Subscription.Children())
                    {
                        updatedSubscription.Subscription = parent.Name;
                        var isSubscribed = parent.Value["email"]["isSubscribed"].Value;

                        var doubleOptInStatus = string.Empty;

                        foreach (var sub in parent.Value["email"]["doubleOptIn"])
                        {
                            if (sub.Name.Equals("status"))
                            {
                                doubleOptInStatus = sub.Value.Value;
                                break;
                            }
                        }

                        if (isSubscribed && (doubleOptInStatus == "Confirmed" || doubleOptInStatus == "NotConfirmed"))
                            updatedSubscription.IsSubscribed = true;
                        else
                            updatedSubscription.IsSubscribed = false;
                    }
                    _telemetryHandler.TrackTraceInfo($"Subscription updated by user. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");
                }


                if (!string.IsNullOrEmpty(updatedData) || !string.IsNullOrEmpty(updatedProfile) || updatedConsent != null || updatedSubscription != null)
                {
                    _telemetryHandler.TrackTraceInfo($"Processing the user updates to downstream systems started. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");
                    List<ActivityFunctionMapping> activityFunctionConfig = await context.CallActivityAsync<List<ActivityFunctionMapping>>("GetConfigurationActivityFunction", null);
                    int dataUpdated = 0;
                    var updatedDataObject = !string.IsNullOrWhiteSpace(updatedData) ? JsonHelper.DeserializeJsonObject<CdcGetAccountInfoData>(updatedData) : null;
                    var updatedProfileObject = !string.IsNullOrWhiteSpace(updatedProfile) ? JsonHelper.DeserializeJsonObject<CdcGetAccountInfoProfile>(updatedProfile) : null;

                    if (!string.IsNullOrEmpty(updatedData))
                        updatedDataObject = UpdateFavTeam(updatedData, updatedDataObject);

                    if (!string.IsNullOrEmpty(updatedProfile))
                        updatedProfileObject = UpdateState(updatedProfile, updatedProfileObject);
                    

                    foreach (var config in activityFunctionConfig)
                    {
                        var property = !string.IsNullOrWhiteSpace(config.Property) ? config.Property.Split(',').ToList() : null;
                        var consent = !string.IsNullOrWhiteSpace(config.Consent) ? config.Consent.Split(',').ToList() : null;
                        var subscriptions = !string.IsNullOrWhiteSpace(config.Subscriptions) ? config.Subscriptions.Split(',').ToList() : null;
                        if ((!string.IsNullOrWhiteSpace(updatedData) && property != null && property.Any(x => updatedData.Contains(x.ToString()))) || (!string.IsNullOrWhiteSpace(updatedProfile) && property != null && property.Any(x => updatedProfile.Contains(x.ToString()))) || (updatedConsent != null && consent != null && consent.Any(x => updatedConsent.Consent.Contains(x.ToString()))) || (updatedSubscription != null && subscriptions != null && subscriptions.Any(x => updatedSubscription.Subscription.Contains(x.ToString()))))
                        {
                            if (userDetails == null && !string.IsNullOrWhiteSpace(config.AdditionalData) && data.Type.ToLower() == CdcEventType.AccountUpdated.ToString().ToLower())
                            {
                                CdcGetAccountInfoRequest userInfoRequest = new CdcGetAccountInfoRequest
                                {
                                    FeedId = data.FeedId,
                                    Uid = data.Data.UId,
                                    HasFullAccount = hasFullAccount
                                };

                                userDetails = await context.CallActivityAsync<CdcGetAccountInfoResponse>("CdcGetAccountInfoActivityFunction", userInfoRequest);
                            }

                            dataUpdated++;

                            feedRequest = new FeedActivityFunctionRequest
                            {
                                UserAccountInfo = !string.IsNullOrWhiteSpace(config.AdditionalData) && data.Type.ToLower() == CdcEventType.AccountUpdated.ToString().ToLower() ? GetUserDetails(config, userDetails) : null,
                                FeedId = data.FeedId,
                                Data = ConvertToNull(updatedDataObject),
                                Profile = ConvertToNull(updatedProfileObject),
                                Preferences = updatedConsent,
                                Subscriptions = updatedSubscription,
                                Uid = data.Data.UId,
                                CdcApiKey = data.ApiKey,
                                HasFullAccount = hasFullAccount
                            };

                            _telemetryHandler.TrackTraceInfo($"Calling Activity Function {config.ActivityFunction}. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");
                            Task<int> task = context.CallActivityAsync<int>($"{config.ActivityFunction}", feedRequest);
                            parallelTasks.Add(task);
                        }
                    }

                    if (dataUpdated == 0)
                    {
                        _telemetryHandler.TrackTraceInfo($"Configured data is not updated by user. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");
                        await context.CallActivityAsync<bool>("UpdateFeedStatusActivityFunction", data.FeedId);
                        return 0;
                    }
                }
                else
                {
                    _telemetryHandler.TrackTraceInfo($"No data has been updated by user.  Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");
                    await context.CallActivityAsync<bool>("UpdateFeedStatusActivityFunction", data.FeedId);
                    return 0;
                }

                await Task.WhenAll(parallelTasks);
                await context.CallActivityAsync<bool>("UpdateFeedStatusActivityFunction", data.FeedId);
                _telemetryHandler.TrackTraceInfo($"Complete Executing Orchestrator Function. Feed Id - {data.FeedId}. Uid - {data.Data.UId}.");

                // Aggregate all N outputs and return the response
                return parallelTasks.Sum(t => t.Result);
            }
            catch (Exception ex)
            {
                _telemetryHandler.TrackException(ex);
                return 0;
            }
        }

        private static CdcGetAccountInfoProfile UpdateState(string updatedProfile, CdcGetAccountInfoProfile updatedProfileObject)
        {
            if (updatedProfile.Contains("state") && string.IsNullOrEmpty(updatedProfileObject.State))
                updatedProfileObject.State = "Unknown";
            return updatedProfileObject;
        }

        private static CdcGetAccountInfoData UpdateFavTeam(string updatedData, CdcGetAccountInfoData updatedDataObject)
        {
            if (updatedData.Contains("favTeam") && !updatedDataObject.FavTeam.Any())
                updatedDataObject.FavTeam = new List<CdcFavTeamModel>() { new CdcFavTeamModel() { Name = "I don't have a favourite team", IsSelected = true } };
            return updatedDataObject;
        }

        private CdcGetAccountInfoResponse GetUserDetails(ActivityFunctionMapping config, CdcGetAccountInfoResponse cdcUserDetails)
        {
            if (string.IsNullOrWhiteSpace(config.AdditionalData))
                return null;

            CdcGetAccountInfoResponse response = new CdcGetAccountInfoResponse();
            response.Data = new CdcGetAccountInfoData();
            response.Data.Id = new CdcIdFields();
            response.Data.Child = new List<CdcChild>();
            response.UID = cdcUserDetails.UID;

            var configList = config.AdditionalData.Split(',');

            foreach (var property in configList)
            {
                if (property.Equals("ParticipantId") && cdcUserDetails.Data != null && cdcUserDetails.Data.Id != null && cdcUserDetails.Data.Id.Participant != null)
                    response.Data.Id.Participant = cdcUserDetails.Data.Id.Participant;

                if (property.Equals("Created") && cdcUserDetails.Created != null)
                    response.Created = cdcUserDetails.Created;

                if (property.Equals("LastUpdated") && cdcUserDetails.LastUpdated != null)
                    response.LastUpdated = cdcUserDetails.LastUpdated;
            }

            return response;
        }

        public T ConvertToNull<T>(T model) where T : class
        {
            if (model == null) return null;
            Type type = model.GetType();
            PropertyInfo[] properties = type.GetProperties();

            var valueTypes = properties.Where(p => p.PropertyType.Assembly != type.Assembly);
            var nonValueTypes = properties.Where(p => p.PropertyType.Assembly == type.Assembly);

            foreach (var nonValueType in nonValueTypes)
                nonValueType.SetValue(model, ConvertToNull(nonValueType.GetValue(model)));

            if (valueTypes.All(z => z.GetValue(model) == null) && nonValueTypes.All(z => z.GetValue(model) == null))
                return null;
            else
                return model;
        }
    }
}