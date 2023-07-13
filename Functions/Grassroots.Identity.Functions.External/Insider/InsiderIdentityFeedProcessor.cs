using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Grassroots.Identity.Functions.External.Insider.Models;
using Grassroots.Common.Helpers;
using System.Collections.Generic;
using System.Linq;
using Grassroots.Identity.Functions.External.Common.Model;
using System.Reactive.Linq;

namespace Grassroots.Identity.Functions.External.Insider
{
    public class InsiderIdentityFeedProcessor : IInsiderIdentityFeedProcessor
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IConfigProvider _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public InsiderIdentityFeedProcessor(ITelemetryHandler telemetryHandler
            , IConfigProvider configuration
            , IHttpClientFactory httpClientFactory)
        {
            _telemetryHandler = telemetryHandler;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task ProcessEvent(IdentityExternalFeedRequest data)
        {
            ValidateInsiderFeed(data);
            var includeParam = "preferences";


            if (data.Preferences is { Consent: { } } && data.Preferences.Consent.Contains("subscribe."))
            {
                _telemetryHandler.TrackTraceInfo($"User updated subscribe consent. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                var panelName = data.Preferences.Consent.Split('.')[1];
                var insiderReq = GenerateInsiderRequestForSubscribeConsent(data, data.Preferences.IsConsentGranted, panelName);
                UpdateInsiderAsync(insiderReq, data.FeedId, GetInsiderApiKey(panelName), GetInsiderPartnerName(panelName));
            }
            else if (data.Preferences is { Consent: { } } && data.Preferences.Consent.Contains("panel."))
            {
                if (data.Preferences.IsConsentGranted)
                {
                    _telemetryHandler.TrackTraceInfo($"User Registered for a new Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                    includeParam = "preferences,subscriptions,data,profile,regSource";

                    var emailAccountQuery = $"SELECT hasFullAccount, hasLiteAccount, email, data, profile, subscriptions, preferences FROM emailAccounts WHERE UID=\"{data.Uid}\"";

                    if (data.HasFullAccount == true)
                    {
                        var cdcResponse = await GetAccountInfoForFullAccountFromCdcAsync(data.Uid, data.FeedId, includeParam);
                        var insideReq = GenerateInsiderRequestForNewPanel(data, cdcResponse);
                        CallInsiderPanelsForNewPanel(insideReq, data, cdcResponse.Preferences);
                    }
                    else
                    {
                        var cdcEmailAccountsResponse = await GetEmailAccountInfoFromCdcAsync(data.Uid, data.FeedId, emailAccountQuery);
                        if (cdcEmailAccountsResponse.Results.Any())
                        {
                            var insideReq = GenerateInsiderRequestForNewPanel(data, cdcEmailAccountsResponse.Results.FirstOrDefault());
                            CallInsiderPanelsForNewPanel(insideReq, data, cdcEmailAccountsResponse.Results.FirstOrDefault().Preferences);
                        }
                    }
                }
                else
                {
                    _telemetryHandler.TrackTraceInfo(
                        $"User Unsubscribed for Panel. Not processing it further. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                }
            }
            else if (data.Subscriptions != null)//TODO: Fix this after business discussion
            {
                _telemetryHandler.TrackTraceInfo($"User updated the subscriptions. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                var emailAccountQuery = $"SELECT hasFullAccount, hasLiteAccount, email, preferences FROM emailAccounts WHERE UID=\"{data.Uid}\"";
                var cdcEmailAccountsResponse = await GetEmailAccountInfoFromCdcAsync(data.Uid, data.FeedId, emailAccountQuery);

                if (cdcEmailAccountsResponse.Results.Any() && cdcEmailAccountsResponse.Results.FirstOrDefault().HasFullAccount == false)
                {
                    //var cdcResponse = await GetAccountInfoForFullAccountFromCdcAsync(data.Uid, data.FeedId, includeParam);
                    if (cdcEmailAccountsResponse.Results.FirstOrDefault().Preferences != null && cdcEmailAccountsResponse.Results.FirstOrDefault().Preferences.Panel != null)
                    {
                        var insideReq = GenerateInsiderRequestForDataOrSubscriptionUpdates(data);
                        CallInsiderPanels(cdcEmailAccountsResponse.Results.FirstOrDefault().Preferences, insideReq, data);
                    }
                }
                else
                {
                    var cdcResponse = await GetAccountInfoForFullAccountFromCdcAsync(data.Uid, data.FeedId, includeParam);
                    if (cdcResponse is { Preferences: { Panel: { } } })
                    {
                        var insideReq = GenerateInsiderRequestForDataOrSubscriptionUpdates(data);
                        CallInsiderPanels(cdcResponse.Preferences, insideReq, data);
                    }
                }
            }
            else if (data.Data is { SyncInsiderPanels: { } })
            {
                if (data.Data.SyncInsiderPanels == true)
                {
                    _telemetryHandler.TrackTraceInfo($"SyncInsiderPanels flag is true. Syncing user in all consented panels. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                    includeParam = "preferences,subscriptions,data,profile,regSource";

                    var emailAccountQuery = $"SELECT hasFullAccount, hasLiteAccount, email, data, profile, subscriptions, preferences FROM emailAccounts WHERE UID=\"{data.Uid}\"";
                    var insideReq = new InsiderRequest();
                    if (data.HasFullAccount == true)
                    {
                        var cdcResponse = await GetAccountInfoForFullAccountFromCdcAsync(data.Uid, data.FeedId, includeParam);
                        insideReq = GenerateInsiderRequestForNewPanel(data, cdcResponse, true);

                        if (cdcResponse is { Preferences: { Panel: { } } })
                        {
                            CallInsiderPanelsForSync(cdcResponse.Preferences, insideReq, data);
                        }
                    }
                    else
                    {
                        var cdcEmailAccountsResponse = await GetEmailAccountInfoFromCdcAsync(data.Uid, data.FeedId, emailAccountQuery);
                        if (cdcEmailAccountsResponse.Results.Any())
                        {
                            //insideReq = GenerateInsiderRequestForNewPanel(data, cdcEmailAccountsResponse.Results.FirstOrDefault());
                            //CallInsiderPanelsForNewPanel(insideReq, data);
                            if (cdcEmailAccountsResponse.Results.FirstOrDefault().Preferences != null && cdcEmailAccountsResponse.Results.FirstOrDefault().Preferences.Panel != null)
                            {
                                insideReq = GenerateInsiderRequestForNewPanel(data, cdcEmailAccountsResponse.Results.FirstOrDefault(), true);
                                CallInsiderPanelsForSync(cdcEmailAccountsResponse.Results.FirstOrDefault().Preferences, insideReq, data);
                            }
                        }
                    }
                }
                else
                {
                    _telemetryHandler.TrackTraceInfo(
                        $"SyncInsiderPanels flag is false. Not processing it further. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                }
            }
            else
            {
                _telemetryHandler.TrackTraceInfo($"User updated the profile data. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                var emailAccountQuery = $"SELECT hasFullAccount, hasLiteAccount, email, preferences FROM emailAccounts WHERE UID=\"{data.Uid}\"";

                if (data.HasFullAccount == true)
                {
                    var cdcResponse = await GetAccountInfoForFullAccountFromCdcAsync(data.Uid, data.FeedId, includeParam);
                    if (cdcResponse is { Preferences: { Panel: { } } })
                    {
                        var insideReq = GenerateInsiderRequestForDataOrSubscriptionUpdates(data);
                        CallInsiderPanels(cdcResponse.Preferences, insideReq, data);
                    }
                }
                else
                {
                    var cdcEmailAccountResponse = await GetEmailAccountInfoFromCdcAsync(data.Uid, data.FeedId, emailAccountQuery);

                    if (cdcEmailAccountResponse.Results.Any())
                    {
                        if (cdcEmailAccountResponse.Results.FirstOrDefault().Preferences != null && cdcEmailAccountResponse.Results.FirstOrDefault().Preferences.Panel != null)
                        {
                            var insideReq = GenerateInsiderRequestForDataOrSubscriptionUpdates(data);
                            CallInsiderPanels(cdcEmailAccountResponse.Results.FirstOrDefault().Preferences, insideReq, data);
                        }
                    }
                }
            }
        }
        private void ValidateInsiderFeed(IdentityExternalFeedRequest data)
        {
            if (data == null)
            {
                var error = $"Received Null Request";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(data.Uid))
            {
                var error = $"Required Value missing for Identity Insider Feed (Uid)";
                throw new ApplicationException(error);
            }
        }
        private async void UpdateInsiderAsync(InsiderRequest request, long feedId, string apiKey, string partnerName)
        {
            _telemetryHandler.TrackTraceInfo($"Start updating the request in Insider. Feed Id - {feedId}.");

            var client = _httpClientFactory.CreateClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post,
                _configuration.GetValue(AppSettingsKey.InsiderUpsertDataUrl));
            httpRequestMessage.Headers.Add("X-PARTNER-NAME", partnerName);
            httpRequestMessage.Headers.Add("X-REQUEST-TOKEN", apiKey);
            httpRequestMessage.Content = new StringContent(JsonHelper.SerializeJsonObject(request));

            var response = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);

            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var insiderResponse = JsonHelper.DeserializeJsonObject<InsiderResponse>(result);

            if (insiderResponse is { Data: { Successful: { Count: > 0 } } })
            {
                _telemetryHandler.TrackTraceInfo($"Updated the user details in Insider. Feed Id - {feedId}.");
            }
            else
            {
                throw new ApplicationException(
                    $"Error occurred while saving data in Insider. Feed Id - {feedId}. {JsonHelper.SerializeJsonObject(insiderResponse)}");
            }

        }
        private async Task<CdcGetAccountInfoResponse> GetAccountInfoForFullAccountFromCdcAsync(string uid, long feedId, string includeParam)
        {
            _telemetryHandler.TrackTraceInfo($"Getting Users details from CDC. Feed Id - {feedId}. Uid - {uid}.");
            var param = new Dictionary<string, string>
            {
                { "apiKey", _configuration.GetValue(AppSettingsKey.CDCApiKey) },
                { "userKey", _configuration.GetValue(AppSettingsKey.CDCUserKey) },
                { "secret", _configuration.GetValue(AppSettingsKey.CDCSecretKey) },
                { "include", includeParam },
                { "UID", uid }
            };

            var client = _httpClientFactory.CreateClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.GetValue(AppSettingsKey.CDCAccountApiBaseUrl) + "accounts.getAccountInfo");
            httpRequestMessage.Content = new FormUrlEncodedContent(param);
            var response = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var cdcResponse = JsonHelper.DeserializeJsonObject<CdcGetAccountInfoResponse>(result);

            if (cdcResponse.ErrorCode == 0)
            {
                _telemetryHandler.TrackTraceInfo($"Received Users details from CDC. Feed Id - {feedId}. Uid - {uid}.");
                return cdcResponse;
            }
            else
            {
                var error = $"Error occurred while getting user details from CDC. Call Id: {cdcResponse.CallId}. Error Message: {cdcResponse.ErrorMessage}. ErrorCode: {cdcResponse.ErrorCode}. Feed Id - {feedId}. Uid - {uid}.";
                throw new ApplicationException(error);
            }
        }
        private async Task<CdcEmailAccountsResponse> GetEmailAccountInfoFromCdcAsync(string uid, long feedId, string emailAccountQuery)
        {
            _telemetryHandler.TrackTraceInfo($"Getting emailAccounts details from CDC. Feed Id - {feedId}. Uid - {uid}.");
            var param = new Dictionary<string, string>
            {
                { "apiKey", _configuration.GetValue(AppSettingsKey.CDCApiKey) },
                { "userKey", _configuration.GetValue(AppSettingsKey.CDCUserKey) },
                { "secret", _configuration.GetValue(AppSettingsKey.CDCSecretKey) },
                { "query", emailAccountQuery }
            };

            var client = _httpClientFactory.CreateClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.GetValue(AppSettingsKey.CDCAccountApiBaseUrl) + "accounts.search");
            httpRequestMessage.Content = new FormUrlEncodedContent(param);
            var response = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var cdcResponse = JsonHelper.DeserializeJsonObject<CdcEmailAccountsResponse>(result);

            if (cdcResponse.ErrorCode == 0)
            {
                _telemetryHandler.TrackTraceInfo($"Received emailAccounts details from CDC. Feed Id - {feedId}. Uid - {uid}.");
                return cdcResponse;
            }
            else
            {
                var error = $"Error occurred while getting emailAccounts details from CDC. Call Id: {cdcResponse.CallId}. Error Message: {cdcResponse.ErrorMessage}. ErrorCode: {cdcResponse.ErrorCode}. Feed Id - {feedId}. Uid - {uid}.";
                throw new ApplicationException(error);
            }
        }
        private InsiderRequest GenerateInsiderRequestForDataOrSubscriptionUpdates(IdentityExternalFeedRequest data)
        {
            _telemetryHandler.TrackTraceInfo($"Starting generating the request for Insider. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
            InsiderRequestUser insiderUser = new InsiderRequestUser()
            {
                Attributes = new InsiderRequestAttributes()
                {
                    Custom = new InsiderRequestCustomAttributes()
                },
                Identifiers = new InsiderRequestIdentifiers()
                {
                    Uuid = data.Uid
                }
            };

            if (data.Profile is { FirstName: { } })
                insiderUser.Attributes.Name = data.Profile.FirstName;

            if (data.Profile is { LastName: { } })
                insiderUser.Attributes.Surname = data.Profile.LastName;

            if (data.Profile is { Email: { } })
                insiderUser.Attributes.Email = data.Profile.Email;

            //if (data.Profile is { Gender: { } })
            //    insiderUser.Attributes.Gender = data.Profile.Gender;

            if (data.Profile is { Country: { } })
                insiderUser.Attributes.Country = data.Profile.Country;

            //if (data.Profile is { Phones: { Number: { } } })
            //    insiderUser.Attributes.PhoneNumber = data.Profile.Phones.Number;

            if (data.Profile is { State: { } })
                insiderUser.Attributes.Custom.State = data.Profile.State;

            if (data.Data is { FavTeam: { } })
                insiderUser.Attributes.Custom.FavouriteTeam = ProcessFavTeam(data);

            //if (data.Profile is { Zip: { } })
            //    insiderUser.Attributes.Custom.Postcode = data.Profile.Zip;

            if (data.Subscriptions is { Subscription: "adelaideStrikers.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionStrikersGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "adelaideStrikers.membership" })
                insiderUser.Attributes.Custom.SubscriptionStrikersMembership = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "adelaideStrikers.tickets" })
                insiderUser.Attributes.Custom.SubscriptionStrikersTickets = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "brisbaneHeat.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionHeatGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "brisbaneHeat.membership" })
                insiderUser.Attributes.Custom.SubscriptionHeatMembership = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "brisbaneHeat.tickets" })
                insiderUser.Attributes.Custom.SubscriptionHeatTickets = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "hobartHurricanes.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionHurricanesGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "hobartHurricanes.membership" })
                insiderUser.Attributes.Custom.SubscriptionHurricanesMembership = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "hobartHurricanes.tickets" })
                insiderUser.Attributes.Custom.SubscriptionHurricanesTickets = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "melbourneRenegades.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionRenegadesGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "melbourneRenegades.membership" })
                insiderUser.Attributes.Custom.SubscriptionRenegadesMembership = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "melbourneRenegades.tickets" })
                insiderUser.Attributes.Custom.SubscriptionRenegadesTickets = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "melbourneStars.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionStarsGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "melbourneStars.membership" })
                insiderUser.Attributes.Custom.SubscriptionStarsMembership = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "melbourneStars.tickets" })
                insiderUser.Attributes.Custom.SubscriptionStarsTickets = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "perthScorchers.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionScorchersGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "perthScorchers.membership" })
                insiderUser.Attributes.Custom.SubscriptionScorchersMembership = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "perthScorchers.tickets" })
                insiderUser.Attributes.Custom.SubscriptionScorchersTickets = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "sydneyThunder.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionThunderGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "sydneyThunder.membership" })
                insiderUser.Attributes.Custom.SubscriptionThunderMembership = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "sydneyThunder.tickets" })
                insiderUser.Attributes.Custom.SubscriptionThunderTickets = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "sydneySixers.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionSixersGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "sydneySixers.membership" })
                insiderUser.Attributes.Custom.SubscriptionSixersMembership = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "sydneySixers.tickets" })
                insiderUser.Attributes.Custom.SubscriptionSixersTickets = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "cricketAustralia.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionCaGeneraMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "cricketAustralia.shop" })
                insiderUser.Attributes.Custom.SubscriptionCaShop = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "cricketAustralia.tickets" })
                insiderUser.Attributes.Custom.SubscriptionCaTickets = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "cricketAustralia.travelOffice" })
                insiderUser.Attributes.Custom.SubscriptionCaTravelOffice = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "ACT.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionActGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "NSW.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionNswGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "NT.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionNtGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "QLD.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionQldGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "SA.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionSaGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "TAS.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionTasGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "VIC.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionVicGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "WA.generalMarketing" })
                insiderUser.Attributes.Custom.SubscriptionWaGeneralMarketing = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "cricketAustralia.coachingNewsletter" })
                insiderUser.Attributes.Custom.SubscriptionCaCoachingNewsletter = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "cricketAustralia.umpiringNewsletter" })
                insiderUser.Attributes.Custom.SubscriptionCaUmpiringNewsletter = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "cricketAustralia.schoolsNewsletter" })
                insiderUser.Attributes.Custom.SubscriptionCaSchoolsNewsletter = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "cricketAustralia.adminsNewsletter" })
                insiderUser.Attributes.Custom.SubscriptionCaAdminsNewsletter = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "cricketAustralia.blastNewsletter" })
                insiderUser.Attributes.Custom.SubscriptionCaBlastNewsletter = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "cricketAustralia.theWicket" })
                insiderUser.Attributes.Custom.SubscriptionCaTheWicket = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "ACT.participation" })
                insiderUser.Attributes.Custom.SubscriptionActParticipation = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "cricketAustralia.participation" })
                insiderUser.Attributes.Custom.SubscriptionCaParticipant = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "NSW.participation" })
                insiderUser.Attributes.Custom.SubscriptionNswParticipation = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "NT.participation" })
                insiderUser.Attributes.Custom.SubscriptionNtParticipation = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "QLD.participation" })
                insiderUser.Attributes.Custom.SubscriptionQldParticipation = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "SA.participation" })
                insiderUser.Attributes.Custom.SubscriptionSaParticipation = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "TAS.participation" })
                insiderUser.Attributes.Custom.SubscriptionTasParticipant = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "VIC.participation" })
                insiderUser.Attributes.Custom.SubscriptionVicParticipant = data.Subscriptions.IsSubscribed;

            if (data.Subscriptions is { Subscription: "WA.participation" })
                insiderUser.Attributes.Custom.SubscriptionWaParticipant = data.Subscriptions.IsSubscribed;

            List<InsiderRequestUser> lstInsiderUser = new List<InsiderRequestUser>
            {
                (insiderUser)
            };

            InsiderRequest insidereq = new InsiderRequest()
            {
                Users = lstInsiderUser
            };

            _telemetryHandler.TrackTraceInfo($"Request Generated for Insider. Feed Id - {data.FeedId}. Uid - {data.Uid}.");

            return insidereq;
        }
        private InsiderRequest GenerateInsiderRequestForSubscribeConsent(IdentityExternalFeedRequest data, bool isEmailOptIn, string panelName)
        {
            _telemetryHandler.TrackTraceInfo($"Starting generating the request for Insider. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
            InsiderRequestUser insiderUser = new InsiderRequestUser()
            {
                Attributes = new InsiderRequestAttributes()
                {
                    Custom = new InsiderRequestCustomAttributes()
                },
                Identifiers = new InsiderRequestIdentifiers()
                {
                    Uuid = data.Uid
                }
            };

            insiderUser.Attributes.EmailOptIn = isEmailOptIn;
            _telemetryHandler.TrackTraceInfo($"Setting EmailOpt In as {isEmailOptIn} for panel - {panelName}. Feed Id - {data.FeedId}. Uid - {data.Uid}.");


            List<InsiderRequestUser> lstInsiderUser = new List<InsiderRequestUser>
            {
                (insiderUser)
            };

            InsiderRequest insidereq = new InsiderRequest()
            {
                Users = lstInsiderUser
            };

            _telemetryHandler.TrackTraceInfo($"Request Generated for Insider. Feed Id - {data.FeedId}. Uid - {data.Uid}.");

            return insidereq;
        }
        private InsiderRequest GenerateInsiderRequestForNewPanel(IdentityExternalFeedRequest request, CdcGetAccountInfoResponse data, bool isSyncRequest = false)
        {
            _telemetryHandler.TrackTraceInfo($"Starting generating the request for Insider. Feed Id - {request.FeedId}. Uid - {request.Uid}.");
            InsiderRequestUser insiderUser = new InsiderRequestUser()
            {
                Attributes = new InsiderRequestAttributes()
                {
                    Custom = new InsiderRequestCustomAttributes()
                },
                Identifiers = new InsiderRequestIdentifiers()
                {
                    Uuid = request.Uid
                }
            };

            insiderUser.Attributes.GdprOptIn = true;
            insiderUser.Attributes.EmailOptIn = true;

            if (!isSyncRequest && (request.Preferences.Consent.Equals("panel.melbournestars") || request.Preferences.Consent.Equals("panel.melbournerenegades") || request.Preferences.Consent.Equals("panel.hobarthurricanes")))
                insiderUser.Attributes.EmailOptIn = false;


            //if (!string.IsNullOrWhiteSpace(data.RegSource))
            //    insiderUser.Attributes.Custom.RegistrationSource = data.RegSource;

            if (data.Profile is { FirstName: { } })
                insiderUser.Attributes.Name = data.Profile.FirstName;

            if (data.Profile is { LastName: { } })
                insiderUser.Attributes.Surname = data.Profile.LastName;

            if (data.Profile is { Email: { } })
                insiderUser.Attributes.Email = data.Profile.Email;

            if (data.Data is { FavTeam: { } })
                insiderUser.Attributes.Custom.FavouriteTeam = ProcessFavTeam(data);

            //if (data.Profile is { Gender: { } })
            //    insiderUser.Attributes.Gender = data.Profile.Gender;

            if (data.Profile is { Country: { } })
                insiderUser.Attributes.Country = data.Profile.Country;

            //if (data.Profile is { Phones: { Number: { } } })
            //    insiderUser.Attributes.PhoneNumber = data.Profile.Phones.Number;

            if (data.Profile is { State: { } })
                insiderUser.Attributes.Custom.State = data.Profile.State;

            //if (data.Profile is { Zip: { } })
            //    insiderUser.Attributes.Custom.Postcode = data.Profile.Zip;

            if (data.Subscriptions != null)
            {
                foreach (var parent in data.Subscriptions.Children())
                {

                    foreach (var child in parent.Children())
                    {

                        foreach (var subchild in child.Children())
                        {
                            var subsciption = subchild.Path;
                            var isSubscribed = subchild.Value["email"]["isSubscribed"].Value;

                            var doubleOptInStatus = string.Empty;

                            foreach (var sub in subchild.Value["email"]["doubleOptIn"])
                            {
                                if (sub.Name.Equals("status"))
                                {
                                    doubleOptInStatus = sub.Value.Value;
                                    break;
                                }
                            }

                            if (isSubscribed && (doubleOptInStatus == "Confirmed" || doubleOptInStatus == "NotConfirmed"))
                                isSubscribed = true;
                            else
                                isSubscribed = false;


                            if (subsciption.Equals("adelaideStrikers.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionStrikersGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("adelaideStrikers.membership"))
                                insiderUser.Attributes.Custom.SubscriptionStrikersMembership = isSubscribed;

                            if (subsciption.Equals("adelaideStrikers.tickets"))
                                insiderUser.Attributes.Custom.SubscriptionStrikersTickets = isSubscribed;

                            if (subsciption.Equals("brisbaneHeat.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionHeatGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("brisbaneHeat.membership"))
                                insiderUser.Attributes.Custom.SubscriptionHeatMembership = isSubscribed;

                            if (subsciption.Equals("brisbaneHeat.tickets"))
                                insiderUser.Attributes.Custom.SubscriptionHeatTickets = isSubscribed;

                            if (subsciption.Equals("hobartHurricanes.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionHurricanesGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("hobartHurricanes.membership"))
                                insiderUser.Attributes.Custom.SubscriptionHurricanesMembership = isSubscribed;

                            if (subsciption.Equals("hobartHurricanes.tickets"))
                                insiderUser.Attributes.Custom.SubscriptionHurricanesTickets = isSubscribed;

                            if (subsciption.Equals("melbourneRenegades.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionRenegadesGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("melbourneRenegades.membership"))
                                insiderUser.Attributes.Custom.SubscriptionRenegadesMembership = isSubscribed;

                            if (subsciption.Equals("melbourneRenegades.tickets"))
                                insiderUser.Attributes.Custom.SubscriptionRenegadesTickets = isSubscribed;

                            if (subsciption.Equals("melbourneStars.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionStarsGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("melbourneStars.membership"))
                                insiderUser.Attributes.Custom.SubscriptionStarsMembership = isSubscribed;

                            if (subsciption.Equals("melbourneStars.tickets"))
                                insiderUser.Attributes.Custom.SubscriptionStarsTickets = isSubscribed;

                            if (subsciption.Equals("perthScorchers.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionScorchersGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("perthScorchers.membership"))
                                insiderUser.Attributes.Custom.SubscriptionScorchersMembership = isSubscribed;

                            if (subsciption.Equals("perthScorchers.tickets"))
                                insiderUser.Attributes.Custom.SubscriptionScorchersTickets = isSubscribed;

                            if (subsciption.Equals("sydneyThunder.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionThunderGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("sydneyThunder.membership"))
                                insiderUser.Attributes.Custom.SubscriptionThunderMembership = isSubscribed;

                            if (subsciption.Equals("sydneyThunder.tickets"))
                                insiderUser.Attributes.Custom.SubscriptionThunderTickets = isSubscribed;

                            if (subsciption.Equals("sydneySixers.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionSixersGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("sydneySixers.membership"))
                                insiderUser.Attributes.Custom.SubscriptionSixersMembership = isSubscribed;

                            if (subsciption.Equals("sydneySixers.tickets"))
                                insiderUser.Attributes.Custom.SubscriptionSixersTickets = isSubscribed;

                            if (subsciption.Equals("cricketAustralia.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionCaGeneraMarketing = isSubscribed;

                            if (subsciption.Equals("cricketAustralia.shop"))
                                insiderUser.Attributes.Custom.SubscriptionCaShop = isSubscribed;

                            if (subsciption.Equals("cricketAustralia.tickets"))
                                insiderUser.Attributes.Custom.SubscriptionCaTickets = isSubscribed;

                            if (subsciption.Equals("cricketAustralia.travelOffice"))
                                insiderUser.Attributes.Custom.SubscriptionCaTravelOffice = isSubscribed;

                            if (subsciption.Equals("ACT.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionActGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("NSW.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionNswGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("NT.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionNtGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("QLD.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionQldGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("SA.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionSaGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("TAS.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionTasGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("VIC.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionVicGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("WA.generalMarketing"))
                                insiderUser.Attributes.Custom.SubscriptionWaGeneralMarketing = isSubscribed;

                            if (subsciption.Equals("cricketAustralia.coachingNewsletter"))
                                insiderUser.Attributes.Custom.SubscriptionCaCoachingNewsletter = isSubscribed;

                            if (subsciption.Equals("cricketAustralia.umpiringNewsletter"))
                                insiderUser.Attributes.Custom.SubscriptionCaUmpiringNewsletter = isSubscribed;

                            if (subsciption.Equals("cricketAustralia.schoolsNewsletter"))
                                insiderUser.Attributes.Custom.SubscriptionCaSchoolsNewsletter = isSubscribed;

                            if (subsciption.Equals("cricketAustralia.adminsNewsletter"))
                                insiderUser.Attributes.Custom.SubscriptionCaAdminsNewsletter = isSubscribed;

                            if (subsciption.Equals("cricketAustralia.blastNewsletter"))
                                insiderUser.Attributes.Custom.SubscriptionCaBlastNewsletter = isSubscribed;

                            if (subsciption.Equals("cricketAustralia.theWicket"))
                                insiderUser.Attributes.Custom.SubscriptionCaTheWicket = isSubscribed;

                            if (subsciption.Equals("ACT.participation"))
                                insiderUser.Attributes.Custom.SubscriptionActParticipation = isSubscribed;

                            if (subsciption.Equals("cricketAustralia.participation"))
                                insiderUser.Attributes.Custom.SubscriptionCaParticipant = isSubscribed;

                            if (subsciption.Equals("NSW.participation"))
                                insiderUser.Attributes.Custom.SubscriptionNswParticipation = isSubscribed;

                            if (subsciption.Equals("NT.participation"))
                                insiderUser.Attributes.Custom.SubscriptionNtParticipation = isSubscribed;

                            if (subsciption.Equals("QLD.participation"))
                                insiderUser.Attributes.Custom.SubscriptionQldParticipation = isSubscribed;

                            if (subsciption.Equals("SA.participation"))
                                insiderUser.Attributes.Custom.SubscriptionSaParticipation = isSubscribed;

                            if (subsciption.Equals("TAS.participation"))
                                insiderUser.Attributes.Custom.SubscriptionTasParticipant = isSubscribed;

                            if (subsciption.Equals("VIC.participation"))
                                insiderUser.Attributes.Custom.SubscriptionVicParticipant = isSubscribed;

                            if (subsciption.Equals("WA.participation"))
                                insiderUser.Attributes.Custom.SubscriptionWaParticipant = isSubscribed;
                        }
                    }
                }
            }

            List<InsiderRequestUser> lstInsiderUser = new List<InsiderRequestUser>
            {
                (insiderUser)
            };

            InsiderRequest insidereq = new InsiderRequest()
            {
                Users = lstInsiderUser
            };

            _telemetryHandler.TrackTraceInfo($"Request Generated for Insider. Feed Id - {request.FeedId}. Uid - {request.Uid}.");

            return insidereq;
        }
        private InsiderRequest GenerateInsiderRequestForNewPanel(IdentityExternalFeedRequest request, CdcEmailAccountsResponseResults data, bool isSyncRequest = false)
        {
            _telemetryHandler.TrackTraceInfo($"Starting generating the request for Insider. Feed Id - {request.FeedId}. Uid - {request.Uid}.");
            InsiderRequestUser insiderUser = new InsiderRequestUser()
            {
                Attributes = new InsiderRequestAttributes()
                {
                    Custom = new InsiderRequestCustomAttributes()
                },
                Identifiers = new InsiderRequestIdentifiers()
                {
                    Uuid = request.Uid
                }
            };

            insiderUser.Attributes.GdprOptIn = true;
            insiderUser.Attributes.EmailOptIn = true;

            if (!isSyncRequest && (request.Preferences.Consent.Equals("panel.melbournestars") || request.Preferences.Consent.Equals("panel.melbournerenegades") || request.Preferences.Consent.Equals("panel.hobarthurricanes")))
                insiderUser.Attributes.EmailOptIn = false;


            //if (!string.IsNullOrWhiteSpace(data.RegSource))
            //    insiderUser.Attributes.Custom.RegistrationSource = data.RegSource;

            if (data.Profile is { FirstName: { } })
                insiderUser.Attributes.Name = data.Profile.FirstName;

            if (data.Profile is { LastName: { } })
                insiderUser.Attributes.Surname = data.Profile.LastName;

            if (data.Profile is { Email: { } })
                insiderUser.Attributes.Email = data.Profile.Email;

            if (data.Data is { FavTeam: { } })
                insiderUser.Attributes.Custom.FavouriteTeam = ProcessFavTeam(data);

            //if (data.Profile is { Gender: { } })
            //    insiderUser.Attributes.Gender = data.Profile.Gender;

            if (data.Profile is { Country: { } })
                insiderUser.Attributes.Country = data.Profile.Country;

            //if (data.Profile is { Phones: { Number: { } } })
            //    insiderUser.Attributes.PhoneNumber = data.Profile.Phones.Number;

            if (data.Profile is { State: { } })
                insiderUser.Attributes.Custom.State = data.Profile.State;

            //if (data.Profile is { Zip: { } })
            //    insiderUser.Attributes.Custom.Postcode = data.Profile.Zip;

            if (data.Subscriptions != null)
            {

                foreach (var parent in data.Subscriptions.Children())
                {
                    var subsciption = parent.Name;
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
                        isSubscribed = true;
                    else
                        isSubscribed = false;

                    if (subsciption.Equals("adelaideStrikers.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionStrikersGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("adelaideStrikers.membership"))
                        insiderUser.Attributes.Custom.SubscriptionStrikersMembership = isSubscribed;

                    if (subsciption.Equals("adelaideStrikers.tickets"))
                        insiderUser.Attributes.Custom.SubscriptionStrikersTickets = isSubscribed;

                    if (subsciption.Equals("brisbaneHeat.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionHeatGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("brisbaneHeat.membership"))
                        insiderUser.Attributes.Custom.SubscriptionHeatMembership = isSubscribed;

                    if (subsciption.Equals("brisbaneHeat.tickets"))
                        insiderUser.Attributes.Custom.SubscriptionHeatTickets = isSubscribed;

                    if (subsciption.Equals("hobartHurricanes.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionHurricanesGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("hobartHurricanes.membership"))
                        insiderUser.Attributes.Custom.SubscriptionHurricanesMembership = isSubscribed;

                    if (subsciption.Equals("hobartHurricanes.tickets"))
                        insiderUser.Attributes.Custom.SubscriptionHurricanesTickets = isSubscribed;

                    if (subsciption.Equals("melbourneRenegades.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionRenegadesGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("melbourneRenegades.membership"))
                        insiderUser.Attributes.Custom.SubscriptionRenegadesMembership = isSubscribed;

                    if (subsciption.Equals("melbourneRenegades.tickets"))
                        insiderUser.Attributes.Custom.SubscriptionRenegadesTickets = isSubscribed;

                    if (subsciption.Equals("melbourneStars.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionStarsGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("melbourneStars.membership"))
                        insiderUser.Attributes.Custom.SubscriptionStarsMembership = isSubscribed;

                    if (subsciption.Equals("melbourneStars.tickets"))
                        insiderUser.Attributes.Custom.SubscriptionStarsTickets = isSubscribed;

                    if (subsciption.Equals("perthScorchers.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionScorchersGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("perthScorchers.membership"))
                        insiderUser.Attributes.Custom.SubscriptionScorchersMembership = isSubscribed;

                    if (subsciption.Equals("perthScorchers.tickets"))
                        insiderUser.Attributes.Custom.SubscriptionScorchersTickets = isSubscribed;

                    if (subsciption.Equals("sydneyThunder.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionThunderGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("sydneyThunder.membership"))
                        insiderUser.Attributes.Custom.SubscriptionThunderMembership = isSubscribed;

                    if (subsciption.Equals("sydneyThunder.tickets"))
                        insiderUser.Attributes.Custom.SubscriptionThunderTickets = isSubscribed;

                    if (subsciption.Equals("sydneySixers.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionSixersGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("sydneySixers.membership"))
                        insiderUser.Attributes.Custom.SubscriptionSixersMembership = isSubscribed;

                    if (subsciption.Equals("sydneySixers.tickets"))
                        insiderUser.Attributes.Custom.SubscriptionSixersTickets = isSubscribed;

                    if (subsciption.Equals("cricketAustralia.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionCaGeneraMarketing = isSubscribed;

                    if (subsciption.Equals("cricketAustralia.shop"))
                        insiderUser.Attributes.Custom.SubscriptionCaShop = isSubscribed;

                    if (subsciption.Equals("cricketAustralia.tickets"))
                        insiderUser.Attributes.Custom.SubscriptionCaTickets = isSubscribed;

                    if (subsciption.Equals("cricketAustralia.travelOffice"))
                        insiderUser.Attributes.Custom.SubscriptionCaTravelOffice = isSubscribed;

                    if (subsciption.Equals("ACT.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionActGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("NSW.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionNswGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("NT.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionNtGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("QLD.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionQldGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("SA.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionSaGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("TAS.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionTasGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("VIC.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionVicGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("WA.generalMarketing"))
                        insiderUser.Attributes.Custom.SubscriptionWaGeneralMarketing = isSubscribed;

                    if (subsciption.Equals("cricketAustralia.coachingNewsletter"))
                        insiderUser.Attributes.Custom.SubscriptionCaCoachingNewsletter = isSubscribed;

                    if (subsciption.Equals("cricketAustralia.umpiringNewsletter"))
                        insiderUser.Attributes.Custom.SubscriptionCaUmpiringNewsletter = isSubscribed;

                    if (subsciption.Equals("cricketAustralia.schoolsNewsletter"))
                        insiderUser.Attributes.Custom.SubscriptionCaSchoolsNewsletter = isSubscribed;

                    if (subsciption.Equals("cricketAustralia.adminsNewsletter"))
                        insiderUser.Attributes.Custom.SubscriptionCaAdminsNewsletter = isSubscribed;

                    if (subsciption.Equals("cricketAustralia.blastNewsletter"))
                        insiderUser.Attributes.Custom.SubscriptionCaBlastNewsletter = isSubscribed;

                    if (subsciption.Equals("cricketAustralia.theWicket"))
                        insiderUser.Attributes.Custom.SubscriptionCaTheWicket = isSubscribed;

                    if (subsciption.Equals("ACT.participation"))
                        insiderUser.Attributes.Custom.SubscriptionActParticipation = isSubscribed;

                    if (subsciption.Equals("cricketAustralia.participation"))
                        insiderUser.Attributes.Custom.SubscriptionCaParticipant = isSubscribed;

                    if (subsciption.Equals("NSW.participation"))
                        insiderUser.Attributes.Custom.SubscriptionNswParticipation = isSubscribed;

                    if (subsciption.Equals("NT.participation"))
                        insiderUser.Attributes.Custom.SubscriptionNtParticipation = isSubscribed;

                    if (subsciption.Equals("QLD.participation"))
                        insiderUser.Attributes.Custom.SubscriptionQldParticipation = isSubscribed;

                    if (subsciption.Equals("SA.participation"))
                        insiderUser.Attributes.Custom.SubscriptionSaParticipation = isSubscribed;

                    if (subsciption.Equals("TAS.participation"))
                        insiderUser.Attributes.Custom.SubscriptionTasParticipant = isSubscribed;

                    if (subsciption.Equals("VIC.participation"))
                        insiderUser.Attributes.Custom.SubscriptionVicParticipant = isSubscribed;

                    if (subsciption.Equals("WA.participation"))
                        insiderUser.Attributes.Custom.SubscriptionWaParticipant = isSubscribed;
                }
            }

            List<InsiderRequestUser> lstInsiderUser = new List<InsiderRequestUser>
            {
                (insiderUser)
            };

            InsiderRequest insidereq = new InsiderRequest()
            {
                Users = lstInsiderUser
            };

            _telemetryHandler.TrackTraceInfo($"Request Generated for Insider. Feed Id - {request.FeedId}. Uid - {request.Uid}.");

            return insidereq;
        }
        private void CallInsiderPanels(CdcGetAccountInfoPreferences cdcResponse, InsiderRequest insideReq, IdentityExternalFeedRequest data)
        {
            if (cdcResponse.Panel.AdelaideStrikers is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to Adelaidestrikers Panel.Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderAdelaideStrikersApiKey), _configuration.GetValue(AppSettingsKey.InsiderAdelaideStrikersPartnerName));
            }

            if (cdcResponse.Panel.BrisbaneHeat is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to BrisbaneHeat Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderBrisbaneHeatApiKey), _configuration.GetValue(AppSettingsKey.InsiderBrisbaneHeatPartnerName));
            }

            if (cdcResponse.Panel.CricketAct is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to CricketAct Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketActApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketActPartnerName));
            }

            if (cdcResponse.Panel.CricketAu is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to CricketAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketAuPartnerName));
            }

            if (cdcResponse.Panel.CricketNsw is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to CricketNsw Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketNswApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketNswPartnerName));
            }

            if (cdcResponse.Panel.CricketTas is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to CricketTas Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketTasApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketTasPartnerName));
            }

            if (cdcResponse.Panel.CricketVic is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to CricketVic Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketVicApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketVicPartnerName));
            }

            if (cdcResponse.Panel.HobartHurricanes is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to HobartHurricanes Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderHobartHurricanesApiKey), _configuration.GetValue(AppSettingsKey.InsiderHobartHurricanesPartnerName));
            }

            if (cdcResponse.Panel.MelbourneRenegades is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to MelbourneRenegades Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderMelbourneRenegadesApiKey), _configuration.GetValue(AppSettingsKey.InsiderMelbourneRenegadesPartnerName));
            }

            if (cdcResponse.Panel.MelbourneStars is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to MelbourneStars Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderMelbourneStarsApiKey), _configuration.GetValue(AppSettingsKey.InsiderMelbourneStarsPartnerName));
            }

            if (cdcResponse.Panel.NtCricket is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to NtCricket Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderNtCricketApiKey), _configuration.GetValue(AppSettingsKey.InsiderNtCricketPartnerName));
            }

            if (cdcResponse.Panel.PerthScorchers is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to PerthScorchers Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderPerthScorchersApiKey), _configuration.GetValue(AppSettingsKey.InsiderPerthScorchersPartnerName));
            }

            if (cdcResponse.Panel.PlayCricketAu is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to PlayCricketAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderPlayCricketAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderPlayCricketAuPartnerName));
            }

            if (cdcResponse.Panel.QldCricket is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to QldCricket Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderQldCricketApiKey), _configuration.GetValue(AppSettingsKey.InsiderQldCricketPartnerName));
            }

            if (cdcResponse.Panel.SacaAu is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to SacaAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderSaCaAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderSaCaAuPartnerName));
            }

            if (cdcResponse.Panel.ShopCricketAu is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to ShopCricketAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderShopCricketAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderShopCricketAuPartnerName));
            }

            if (cdcResponse.Panel.SydneySixers is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to SydneySixers Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderSydneySixersApiKey), _configuration.GetValue(AppSettingsKey.InsiderSydneySixersPartnerName));
            }

            if (cdcResponse.Panel.SydneyThunder is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to SydneyThunder Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderSydneyThunderApiKey), _configuration.GetValue(AppSettingsKey.InsiderSydneyThunderPartnerName));
            }

            if (cdcResponse.Panel.WaCricket is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to WaCricket Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderWaCricketApiKey), _configuration.GetValue(AppSettingsKey.InsiderWaCricketPartnerName));
            }
        }

        private void CallInsiderPanelsForSync(CdcGetAccountInfoPreferences cdcResponse, InsiderRequest insideReq, IdentityExternalFeedRequest data)
        {
            if (cdcResponse.Panel.AdelaideStrikers is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to Adelaidestrikers Panel.Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "adelaidestrikers", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderAdelaideStrikersApiKey), _configuration.GetValue(AppSettingsKey.InsiderAdelaideStrikersPartnerName));
            }

            if (cdcResponse.Panel.BrisbaneHeat is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to BrisbaneHeat Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "brisbaneheat", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderBrisbaneHeatApiKey), _configuration.GetValue(AppSettingsKey.InsiderBrisbaneHeatPartnerName));
            }

            if (cdcResponse.Panel.CricketAct is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to CricketAct Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "cricketact", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketActApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketActPartnerName));
            }

            if (cdcResponse.Panel.CricketAu is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to CricketAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "cricketau", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketAuPartnerName));
            }

            if (cdcResponse.Panel.CricketNsw is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to CricketNsw Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "cricketnsw", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketNswApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketNswPartnerName));
            }

            if (cdcResponse.Panel.CricketTas is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to CricketTas Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "crickettas", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketTasApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketTasPartnerName));
            }

            if (cdcResponse.Panel.CricketVic is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to CricketVic Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "cricketvic", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketVicApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketVicPartnerName));
            }

            if (cdcResponse.Panel.HobartHurricanes is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to HobartHurricanes Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "hobarthurricanes", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderHobartHurricanesApiKey), _configuration.GetValue(AppSettingsKey.InsiderHobartHurricanesPartnerName));
            }

            if (cdcResponse.Panel.MelbourneRenegades is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to MelbourneRenegades Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "melbournerenegades", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderMelbourneRenegadesApiKey), _configuration.GetValue(AppSettingsKey.InsiderMelbourneRenegadesPartnerName));
            }

            if (cdcResponse.Panel.MelbourneStars is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to MelbourneStars Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "melbournestars", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderMelbourneStarsApiKey), _configuration.GetValue(AppSettingsKey.InsiderMelbourneStarsPartnerName));
            }

            if (cdcResponse.Panel.NtCricket is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to NtCricket Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "ntcricket", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderNtCricketApiKey), _configuration.GetValue(AppSettingsKey.InsiderNtCricketPartnerName));
            }

            if (cdcResponse.Panel.PerthScorchers is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to PerthScorchers Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "perthscorchers", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderPerthScorchersApiKey), _configuration.GetValue(AppSettingsKey.InsiderPerthScorchersPartnerName));
            }

            if (cdcResponse.Panel.PlayCricketAu is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to PlayCricketAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "playcricketau", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderPlayCricketAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderPlayCricketAuPartnerName));
            }

            if (cdcResponse.Panel.QldCricket is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to QldCricket Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "qldcricket", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderQldCricketApiKey), _configuration.GetValue(AppSettingsKey.InsiderQldCricketPartnerName));
            }

            if (cdcResponse.Panel.SacaAu is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to SacaAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "sacaau", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderSaCaAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderSaCaAuPartnerName));
            }

            if (cdcResponse.Panel.ShopCricketAu is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to ShopCricketAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "shopcricketau", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderShopCricketAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderShopCricketAuPartnerName));
            }

            if (cdcResponse.Panel.SydneySixers is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to SydneySixers Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "sydneysixers", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderSydneySixersApiKey), _configuration.GetValue(AppSettingsKey.InsiderSydneySixersPartnerName));
            }

            if (cdcResponse.Panel.SydneyThunder is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to SydneyThunder Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "sydneythunder", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderSydneyThunderApiKey), _configuration.GetValue(AppSettingsKey.InsiderSydneyThunderPartnerName));
            }

            if (cdcResponse.Panel.WaCricket is { IsConsentGranted: true })
            {
                _telemetryHandler.TrackTraceInfo($"User subscribed to WaCricket Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, "wacricket", cdcResponse);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderWaCricketApiKey), _configuration.GetValue(AppSettingsKey.InsiderWaCricketPartnerName));
            }
        }

        private void CallInsiderPanelsForNewPanel(InsiderRequest insideReq, IdentityExternalFeedRequest data, CdcGetAccountInfoPreferences preferences)
        {
            string panelName = data.Preferences.Consent.Split('.')[1];

            if (data.Preferences.Consent.Equals("panel.adelaidestrikers"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to AdelaideStrikers Panel.Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderAdelaideStrikersApiKey), _configuration.GetValue(AppSettingsKey.InsiderAdelaideStrikersPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.brisbaneheat"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to BrisbaneHeat Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderBrisbaneHeatApiKey), _configuration.GetValue(AppSettingsKey.InsiderBrisbaneHeatPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.cricketact"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to CricketAct Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketActApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketActPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.cricketau"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to CricketAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketAuPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.cricketnsw"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to CricketNsw Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketNswApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketNswPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.crickettas"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to CricketTas Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketTasApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketTasPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.cricketvic"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to CricketVic Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderCricketVicApiKey), _configuration.GetValue(AppSettingsKey.InsiderCricketVicPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.hobarthurricanes"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to HobartHurricanes Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderHobartHurricanesApiKey), _configuration.GetValue(AppSettingsKey.InsiderHobartHurricanesPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.melbournerenegades"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to MelbourneRenegades Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderMelbourneRenegadesApiKey), _configuration.GetValue(AppSettingsKey.InsiderMelbourneRenegadesPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.melbournestars"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to MelbourneStars Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderMelbourneStarsApiKey), _configuration.GetValue(AppSettingsKey.InsiderMelbourneStarsPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.ntcricket"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to NtCricket Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderNtCricketApiKey), _configuration.GetValue(AppSettingsKey.InsiderNtCricketPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.perthscorchers"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to PerthScorchers Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderPerthScorchersApiKey), _configuration.GetValue(AppSettingsKey.InsiderPerthScorchersPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.playcricketau"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to PlayCricketAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderPlayCricketAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderPlayCricketAuPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.qldcricket"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to QldCricket Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderQldCricketApiKey), _configuration.GetValue(AppSettingsKey.InsiderQldCricketPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.sacaau"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to SacaAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderSaCaAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderSaCaAuPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.shopcricketau"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to ShopCricketAu Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderShopCricketAuApiKey), _configuration.GetValue(AppSettingsKey.InsiderShopCricketAuPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.sydneysixers"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to SydneySixers Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderSydneySixersApiKey), _configuration.GetValue(AppSettingsKey.InsiderSydneySixersPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.sydneythunder"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to SydneyThunder Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderSydneyThunderApiKey), _configuration.GetValue(AppSettingsKey.InsiderSydneyThunderPartnerName));
            }
            else if (data.Preferences.Consent.Equals("panel.wacricket"))
            {
                _telemetryHandler.TrackTraceInfo($"User newly subscribed to WaCricket Panel. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                insideReq = UpdateEmailOptIn(insideReq, panelName, preferences);
                UpdateInsiderAsync(insideReq, data.FeedId, _configuration.GetValue(AppSettingsKey.InsiderWaCricketApiKey), _configuration.GetValue(AppSettingsKey.InsiderWaCricketPartnerName));
            }

        }
        private string GetInsiderPartnerName(string panelName)
        {
            if (panelName == "adelaidestrikers")
                return _configuration.GetValue(AppSettingsKey.InsiderAdelaideStrikersPartnerName);

            else if (panelName == "brisbaneheat")
                return _configuration.GetValue(AppSettingsKey.InsiderBrisbaneHeatPartnerName);

            else if (panelName == "cricketact")
                return _configuration.GetValue(AppSettingsKey.InsiderCricketActPartnerName);

            else if (panelName == "cricketau")
                return _configuration.GetValue(AppSettingsKey.InsiderCricketAuPartnerName);

            else if (panelName == "cricketnsw")
                return _configuration.GetValue(AppSettingsKey.InsiderCricketNswPartnerName);

            else if (panelName == "crickettas")
                return _configuration.GetValue(AppSettingsKey.InsiderCricketTasPartnerName);

            else if (panelName == "cricketvic")
                return _configuration.GetValue(AppSettingsKey.InsiderCricketVicPartnerName);

            else if (panelName == "hobarthurricanes")
                return _configuration.GetValue(AppSettingsKey.InsiderHobartHurricanesPartnerName);

            else if (panelName == "melbournerenegades")
                return _configuration.GetValue(AppSettingsKey.InsiderMelbourneRenegadesPartnerName);

            else if (panelName == "melbournestars")
                return _configuration.GetValue(AppSettingsKey.InsiderMelbourneStarsPartnerName);

            else if (panelName == "ntcricket")
                return _configuration.GetValue(AppSettingsKey.InsiderNtCricketPartnerName);

            else if (panelName == "perthscorchers")
                return _configuration.GetValue(AppSettingsKey.InsiderPerthScorchersPartnerName);

            else if (panelName == "playcricketau")
                return _configuration.GetValue(AppSettingsKey.InsiderPlayCricketAuPartnerName);

            else if (panelName == "qldcricket")
                return _configuration.GetValue(AppSettingsKey.InsiderQldCricketPartnerName);

            else if (panelName == "sacaau")
                return _configuration.GetValue(AppSettingsKey.InsiderSaCaAuPartnerName);

            else if (panelName == "shopcricketau")
                return _configuration.GetValue(AppSettingsKey.InsiderShopCricketAuPartnerName);

            else if (panelName == "sydneysixers")
                return _configuration.GetValue(AppSettingsKey.InsiderSydneySixersPartnerName);

            else if (panelName == "sydneythunder")
                return _configuration.GetValue(AppSettingsKey.InsiderSydneyThunderPartnerName);

            else if (panelName == "wacricket")
                return _configuration.GetValue(AppSettingsKey.InsiderWaCricketPartnerName);

            return null;

        }
        private string GetInsiderApiKey(string panelName)
        {
            if (panelName == "adelaidestrikers")
                return _configuration.GetValue(AppSettingsKey.InsiderAdelaideStrikersApiKey);

            else if (panelName == "brisbaneheat")
                return _configuration.GetValue(AppSettingsKey.InsiderBrisbaneHeatApiKey);

            else if (panelName == "cricketact")
                return _configuration.GetValue(AppSettingsKey.InsiderCricketActApiKey);

            else if (panelName == "cricketau")
                return _configuration.GetValue(AppSettingsKey.InsiderCricketAuApiKey);

            else if (panelName == "cricketnsw")
                return _configuration.GetValue(AppSettingsKey.InsiderCricketNswApiKey);

            else if (panelName == "crickettas")
                return _configuration.GetValue(AppSettingsKey.InsiderCricketTasApiKey);

            else if (panelName == "cricketvic")
                return _configuration.GetValue(AppSettingsKey.InsiderCricketVicApiKey);

            else if (panelName == "hobarthurricanes")
                return _configuration.GetValue(AppSettingsKey.InsiderHobartHurricanesApiKey);

            else if (panelName == "melbournerenegades")
                return _configuration.GetValue(AppSettingsKey.InsiderMelbourneRenegadesApiKey);

            else if (panelName == "melbournestars")
                return _configuration.GetValue(AppSettingsKey.InsiderMelbourneStarsApiKey);

            else if (panelName == "ntcricket")
                return _configuration.GetValue(AppSettingsKey.InsiderNtCricketApiKey);

            else if (panelName == "perthscorchers")
                return _configuration.GetValue(AppSettingsKey.InsiderPerthScorchersApiKey);

            else if (panelName == "playcricketau")
                return _configuration.GetValue(AppSettingsKey.InsiderPlayCricketAuApiKey);

            else if (panelName == "qldcricket")
                return _configuration.GetValue(AppSettingsKey.InsiderQldCricketApiKey);

            else if (panelName == "sacaau")
                return _configuration.GetValue(AppSettingsKey.InsiderSaCaAuApiKey);

            else if (panelName == "shopcricketau")
                return _configuration.GetValue(AppSettingsKey.InsiderShopCricketAuApiKey);

            else if (panelName == "sydneysixers")
                return _configuration.GetValue(AppSettingsKey.InsiderSydneySixersApiKey);

            else if (panelName == "sydneythunder")
                return _configuration.GetValue(AppSettingsKey.InsiderSydneyThunderApiKey);

            else if (panelName == "wacricket")
                return _configuration.GetValue(AppSettingsKey.InsiderWaCricketApiKey);

            return null;

        }
        private List<string> ProcessFavTeam(dynamic data)
        {
            List<string> favTeam = new List<string>();

            foreach (var team in data.Data.FavTeam)
            {
                if (team.IsSelected)
                    favTeam.Add((team.Name));
            }

            return favTeam;
        }

        private InsiderRequest UpdateEmailOptIn(InsiderRequest insideReq, string panelName, CdcGetAccountInfoPreferences preferences)
        {
            bool isEmailOptIn = false;

            if (preferences.Subscribe == null)
            {
                insideReq.Users.FirstOrDefault().Attributes.EmailOptIn = isEmailOptIn;
                return insideReq;
            }               

            var res = preferences.Subscribe.GetType().GetProperties();
            foreach (var property in res)
            {
                if (property.Name.Equals(panelName, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (panelName.Equals("adelaidestrikers") && preferences.Subscribe.AdelaideStrikers != null)
                    {
                        isEmailOptIn = preferences.Subscribe.AdelaideStrikers.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("brisbaneheat") && preferences.Subscribe.BrisbaneHeat != null)
                    {
                        isEmailOptIn = preferences.Subscribe.BrisbaneHeat.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("cricketact") && preferences.Subscribe.CricketAct != null)
                    {
                        isEmailOptIn = preferences.Subscribe.CricketAct.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("cricketau") && preferences.Subscribe.CricketAu != null)
                    {
                        isEmailOptIn = preferences.Subscribe.CricketAu.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("cricketnsw") && preferences.Subscribe.CricketNsw != null)
                    {
                        isEmailOptIn = preferences.Subscribe.CricketNsw.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("crickettas") && preferences.Subscribe.CricketTas != null)
                    {
                        isEmailOptIn = preferences.Subscribe.CricketTas.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("cricketvic") && preferences.Subscribe.CricketVic != null)
                    {
                        isEmailOptIn = preferences.Subscribe.CricketVic.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("hobarthurricanes") && preferences.Subscribe.HobartHurricanes != null)
                    {
                        isEmailOptIn = preferences.Subscribe.HobartHurricanes.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("melbournerenegades") && preferences.Subscribe.MelbourneRenegades != null)
                    {
                        isEmailOptIn = preferences.Subscribe.MelbourneRenegades.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("melbournestars") && preferences.Subscribe.MelbourneStars != null)
                    {
                        isEmailOptIn = preferences.Subscribe.MelbourneStars.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("ntcricket") && preferences.Subscribe.NtCricket != null)
                    {
                        isEmailOptIn = preferences.Subscribe.NtCricket.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("perthscorchers") && preferences.Subscribe.PerthScorchers != null)
                    {
                        isEmailOptIn = preferences.Subscribe.PerthScorchers.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("playcricketau") && preferences.Subscribe.PlayCricketAu != null)
                    {
                        isEmailOptIn = preferences.Subscribe.PlayCricketAu.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("qldcricket") && preferences.Subscribe.QldCricket != null)
                    {
                        isEmailOptIn = preferences.Subscribe.QldCricket.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("sacaau") && preferences.Subscribe.SacaAu != null)
                    {
                        isEmailOptIn = preferences.Subscribe.SacaAu.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("shopcricketau") && preferences.Subscribe.ShopCricketAu != null)
                    {
                        isEmailOptIn = preferences.Subscribe.ShopCricketAu.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("sydneysixers") && preferences.Subscribe.SydneySixers != null)
                    {
                        isEmailOptIn = preferences.Subscribe.SydneySixers.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("sydneythunder") && preferences.Subscribe.SydneyThunder != null)
                    {
                        isEmailOptIn = preferences.Subscribe.SydneyThunder.IsConsentGranted;
                        break;
                    }
                    else if (panelName.Equals("wacricket") && preferences.Subscribe.WaCricket != null)
                    {
                        isEmailOptIn = preferences.Subscribe.WaCricket.IsConsentGranted;
                        break;
                    }
                }
            }

            insideReq.Users.FirstOrDefault().Attributes.EmailOptIn = isEmailOptIn;

            return insideReq;
        }

    }
}
