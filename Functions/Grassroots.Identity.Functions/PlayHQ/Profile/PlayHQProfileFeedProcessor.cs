using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Grassroots.Identity.Functions.Common.Models;
using Newtonsoft.Json;
using Grassroots.Identity.Functions.PlayHQ.Profile.Models;
using System.Linq;
using Grassroots.Common.PublishEvents.ChangeTrack;
using Grassroots.Identity.API.PayLoadModel.PayLoads;
using Grassroots.Identity.API.PayLoadModel.Static;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant.RequestModel;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping.RequestModel;
using ServiceStack;
using Grassroots.Identity.Database.Model.DbEntity;

namespace Grassroots.Identity.Functions.PlayHQ.Profile
{
    public class PlayHQProfileFeedProcessor : IPlayHQProfileFeedProcessor
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigProvider _configuration;
        private readonly IParticipantOperations _participantOperations;
        private readonly IParticipantMappingOperations _participantMappingOperations;
        private readonly IChangeTrack _changeTrack;

        public PlayHQProfileFeedProcessor(ITelemetryHandler telemetryHandler
            , IHttpClientFactory httpClientFactory
            , IConfigProvider configuration
            , IParticipantOperations participantOperations
            , IParticipantMappingOperations participantMappingOperations
            , IChangeTrack changeTrack)
        {
            _telemetryHandler = telemetryHandler;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _participantOperations = participantOperations;
            _participantMappingOperations = participantMappingOperations;
            _changeTrack = changeTrack;
        }

        public async Task<int> ProcessFeed(PlayHQProfileData playHqProfile, long rawFeedId)
        {
            _telemetryHandler.TrackTraceInfo($"Processing profile feed started for RawFeedId - {rawFeedId}");
            ValidatePlayHqFeed(playHqProfile, rawFeedId);
            bool updateCdc = await ProcessProfileFeed(playHqProfile, rawFeedId);

            _telemetryHandler.TrackTraceInfo("Processing Profile Feed Completed");
            return 1;
        }

        private void ValidatePlayHqFeed(PlayHQProfileData playHqProfile, long rawFeedId)
        {
            if (string.IsNullOrWhiteSpace(playHqProfile.Id))
            {
                var error = $"Required Value missing for PlayHQ Competition Feed (Data.Id). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }
        }

        private async Task<bool> ProcessProfileFeed(PlayHQProfileData feed, long rawFeedId)
        {
            var participantGuid = new Guid();
            Guid? cricketId = null;
            Guid? parentCricketId = null;
            string eventType = EventType.KondoParticipantUpdated;
            bool updateCdc = false;
            List<int> legacyPlayerId = new List<int>();

            var dbParticipant = await _participantOperations.GetParticipantByPlayHQProfileId(feed.Id);

            if (dbParticipant != null)
            {
                _telemetryHandler.TrackTraceInfo($"Participant found in DB with participantId - {dbParticipant.ParticipantGuid}");

                cricketId = dbParticipant.CricketId;
                participantGuid = dbParticipant.ParticipantGuid;
                parentCricketId = dbParticipant.ParentCricketId;

                var dbParticipantByParticipantid = await _participantOperations.GetParticipantByParticipantId(dbParticipant.ParticipantGuid.ToString());

                foreach (var mapping in dbParticipantByParticipantid)
                {
                    if (mapping.LegacyPlayerId != null && mapping.LegacyPlayerId != 0)
                        legacyPlayerId.Add(Convert.ToInt32(mapping.LegacyPlayerId));
                }

            }
            else
            {
                if (string.IsNullOrWhiteSpace(feed.ExternalAccountId))
                {
                    cricketId = null;
                    _telemetryHandler.TrackTraceInfo($"Setting up cricket Id as null as its ExternalAccountId is null or empty received. FeedId: {rawFeedId}");
                }
                else
                {
                    try
                    {
                        cricketId = new Guid(feed.ExternalAccountId);
                    }
                    catch (Exception e)
                    {
                        _telemetryHandler.TrackTraceInfo($"Setting up cricket Id as null as its not a valid guid. Cricket Id received - {feed.ExternalAccountId}. FeedId: {rawFeedId}");
                        cricketId = null;
                    }

                    //updateCdc = true;
                }

                eventType = EventType.KondoParticipantCreated;
            }

            var participant = new ParticipantSaveModel
            {
                ParticipantGuid = participantGuid,
                CricketId = cricketId,
                FirstName = feed.Participant.FirstName,
                LastName = feed.Participant.LastName,
                IsNameVisible = feed.ProfileVisible ?? false,
                IsSearchable = feed.ProfileVisible ?? false,
                FeedId = rawFeedId,
                ParentCricketId = parentCricketId
            };

            participantGuid = await _participantOperations.SaveParticipant(participant);
            participant.ParticipantGuid = participantGuid;

            var participantMapping = new ParticipantMappingSaveModel
            {
                ParticipantGuid = participantGuid,
                PlayHQProfileId = new Guid(feed.Id),
                FeedId = rawFeedId
            };

            if (dbParticipant != null)
                _telemetryHandler.TrackTraceInfo($"Participant updated in DB with participantId - {participantGuid}.");
            else
            {
                _telemetryHandler.TrackTraceInfo($"Participant saved in DB with participantId - {participantGuid}");
                await _participantMappingOperations.SaveParticipantMapping(participantMapping);
                _telemetryHandler.TrackTraceInfo($"Participant Mapping saved in DB with participantId - {participantGuid}");
            }

            if (updateCdc)
            {
                await ValidateDataInCdc(feed);
                await UpdatePlayHqIdInCdc(feed, rawFeedId);
            }

            await PublishProfileEvent(participant, participantMapping, eventType, legacyPlayerId);

            return updateCdc;
        }

        public async Task ValidateDataInCdc(PlayHQProfileData playHqProfile)
        {
            _telemetryHandler.TrackTraceInfo("Validation for PlayHQId exists already in CDC Started");
            var param = new Dictionary<string, string>
            {
                {"apiKey", _configuration.GetValue(AppSettingsKey.CDCApiKey)},
                {"userKey", _configuration.GetValue(AppSettingsKey.CDCUserKey)},
                {"secret", _configuration.GetValue(AppSettingsKey.CDCSecretKey)},
                {"query", $"select UID from accounts where data.id.playHQ = \"{playHqProfile.Id}\" OR data.playHQId = \"{playHqProfile.Id}\""}
            };

            var client = _httpClientFactory.CreateClient();
            var httpRequestMessage =
                new HttpRequestMessage(HttpMethod.Post, _configuration.GetValue(AppSettingsKey.CDCAccountBaseUrl) + "accounts.search")
                {
                    Content = new FormUrlEncodedContent(param)
                };
            var response = await client.SendAsync(httpRequestMessage);
            var result = await response.Content.ReadAsStringAsync();

            var parsedResult = JsonHelper.DeserializeJsonObject<CdcSearchResponse>(result);

            if (parsedResult.ErrorCode != 0)
            {
                var error = $"Error received from CDC - {parsedResult.CallId + " | " + parsedResult.ErrorDetails + " | " + parsedResult.ErrorMessage} for UID: {playHqProfile.ExternalAccountId}";
                throw new ApplicationException(error);
            }

            if (parsedResult.Results != null && parsedResult.Results.Count > 0)
            {
                if (parsedResult.Results.FirstOrDefault().Uid == playHqProfile.ExternalAccountId)
                {
                    var error = $"PlayHQ Id provided in request already mapped with same user";
                    throw new ApplicationException(error);
                }
                else
                {
                    var error = $"PlayHQ Id provided in request already mapped with some other user";
                    throw new ApplicationException(error);
                }

            }

            _telemetryHandler.TrackTraceInfo($"Validation for checking PlayHQ ID  passed for UID: {playHqProfile.ExternalAccountId}");
        }

        public async Task UpdatePlayHqIdInCdc(PlayHQProfileData playHqFeed, long rawFeedId)
        {
            _telemetryHandler.TrackTraceInfo($"Updating the data in CDC");
            var param = new Dictionary<string, string>();
            var request = new CdcDataModel()
            {
                PlayHQId = playHqFeed.Id
            };

            param.Add("apiKey", _configuration.GetValue(AppSettingsKey.CDCApiKey));
            param.Add("userKey", _configuration.GetValue(AppSettingsKey.CDCUserKey));
            param.Add("secret", _configuration.GetValue(AppSettingsKey.CDCSecretKey));
            param.Add("UID", playHqFeed.ExternalAccountId);
            param.Add("data", JsonConvert.SerializeObject(request));

            var client = _httpClientFactory.CreateClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.GetValue(AppSettingsKey.CDCAccountBaseUrl) + "accounts.setAccountInfo");
            httpRequestMessage.Content = new FormUrlEncodedContent(param);
            var response = await client.SendAsync(httpRequestMessage);
            var result = await response.Content.ReadAsStringAsync();

            var playHqResponse = JsonHelper.DeserializeJsonObject<Response>(result);

            if (playHqResponse.errorCode == 0)
            {
                _telemetryHandler.TrackTraceInfo($"Data Updated in CDC Successfully.");
            }
            else
            {
                var error = $"Error occurred while saving PlayHQId in CDC. Call Id: {playHqResponse.callId}. Error Message: {playHqResponse.errorMessage}. ErrorCode: {playHqResponse.errorCode} RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            _telemetryHandler.TrackTraceInfo($"Processing feed Completed for {rawFeedId}");

        }

        private async Task PublishProfileEvent(Database.Model.DbEntity.Participant participant, ParticipantMappingSaveModel participantMapping, string eventType, List<int> legacyPlayerId)
        {
            var participantPayload = participant.ConvertTo<ParticipantPayload>();
            participantPayload.ParticipantId = participant.ParticipantGuid;
            participantPayload.IsNameSuppressed = participant.IsNameVisible;

            List<Guid?> playHqProfileIdList = new List<Guid?>();

            if (participantMapping.PlayHQProfileId != null)
                playHqProfileIdList.Add(participantMapping.PlayHQProfileId);

            participantPayload.PlayHQProfileId = playHqProfileIdList;
            participantPayload.LegacyPlayerId = legacyPlayerId;

            _telemetryHandler.TrackTraceInfo($"Sending feed to Grassroots shared event grid");
            await _changeTrack.TrackChange(participantPayload, eventType, $"/Participant/{participant.ParticipantGuid}");
        }

        #region Process Profile Claim
        public async Task<bool> ProcessClaimFeed(PlayHQClaimData playHqClaim, long rawFeedId)
        {
            _telemetryHandler.TrackTraceInfo($"Processing profile claim feed started for RawFeedId - {rawFeedId}");
            ValidatePlayHqClaimFeed(playHqClaim, rawFeedId);
            bool updatedCdc = await ProcessProfileClaimFeed(playHqClaim, rawFeedId);

            _telemetryHandler.TrackTraceInfo("Processing Profile Feed Completed");
            return updatedCdc;
        }

        private void ValidatePlayHqClaimFeed(PlayHQClaimData playHqClaim, long rawFeedId)
        {
            if (string.IsNullOrWhiteSpace(playHqClaim.ClaimedProfileId))
            {
                var error = $"Required Value missing for PlayHQ Claim Feed (ClaimedProfileId). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(playHqClaim.DestinationProfileId))
            {
                var error = $"Required Value missing for PlayHQ Claim Feed (DestinationProfileId). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if(playHqClaim.ClaimedProfileId.Equals(playHqClaim.DestinationProfileId, StringComparison.CurrentCultureIgnoreCase))
            {
                var error = $"ClaimedProfileId and DestinationProfileId are same for PlayHQ Claim Feed. RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            try
            {
                var claimedProfileId = new Guid(playHqClaim.ClaimedProfileId);
                var destinationProfileId = new Guid(playHqClaim.DestinationProfileId);
            }
            catch (Exception ex)
            {
                _telemetryHandler.TrackException(ex);
                var error = $"Received invalid ClaimedProfileId or DestinationProfileId for PlayHQ Profile Claim Feed. RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }
        }

        private async Task<bool> ProcessProfileClaimFeed(PlayHQClaimData feed, long rawFeedId)
        {
            string eventType = EventType.KondoParticipantMerged;
            ParticipantMappingSaveModel participantMapping = new ParticipantMappingSaveModel();
            bool isDestinationProfileIdExist = false;
            var dbClaimedParticipantMapping = await _participantOperations.GetParticipantByPlayHQProfileId(feed.ClaimedProfileId);

            if (dbClaimedParticipantMapping != null)
            {
                // Delete Participant Mapping 
                await DeleteParticipantMappingByPlayHQProfileId(new Guid(feed.ClaimedProfileId), rawFeedId, dbClaimedParticipantMapping.ParticipantGuid, participantMapping);

                var dbDestinationParticipantMapping = await _participantOperations.GetParticipantByPlayHQProfileId(feed.DestinationProfileId);

                if (dbDestinationParticipantMapping != null)
                {
                    _telemetryHandler.TrackTraceInfo($"Profile found in DB with DestinationProfileId - {feed.DestinationProfileId}");
                    isDestinationProfileIdExist = true;
                    // Delete Participant 
                    var dbParticipant = await _participantMappingOperations.GetParticipantMappingByParticipanyGuid(dbClaimedParticipantMapping.ParticipantGuid.ToString());

                    if (dbParticipant == null)
                    {
                        var participant = new ParticipantSaveModel
                        {
                            ParticipantGuid = dbClaimedParticipantMapping.ParticipantGuid,
                            FeedId = rawFeedId
                        };
                        await _participantOperations.DeleteParticipant(participant);
                    }
                }
                else
                {
                    // Added new Participant Mapping record and mapped to same ParticipantGuid
                    participantMapping.PlayHQProfileId = new Guid(feed.DestinationProfileId);
                    participantMapping.FeedId = rawFeedId;
                    participantMapping.ParticipantGuid = dbClaimedParticipantMapping.ParticipantGuid;
                    await _participantMappingOperations.SaveParticipantMapping(participantMapping);
                    _telemetryHandler.TrackTraceInfo($"Participant Mapping saved in DB with participantId - {dbClaimedParticipantMapping.ParticipantGuid}");
                }

                // Update CDC
                await UpdateProfileClaimPlayHqIdInCdc(feed, dbClaimedParticipantMapping, isDestinationProfileIdExist, rawFeedId);
                await PublishProfileClaimEvent(new Guid(feed.DestinationProfileId), new Guid(feed.ClaimedProfileId), participantMapping, eventType);
            }
            else
            {
                var error = $"Missing ClaimedProfileId in DB for PlayHQ Claim Feed . RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }
            return true;
        }

        private async Task DeleteParticipantMappingByPlayHQProfileId(Guid profileId, long rawFeedId, Guid participantGuid, ParticipantMappingSaveModel participantMapping)
        {
            participantMapping.PlayHQProfileId = profileId;
            participantMapping.ParticipantGuid = participantGuid;
            participantMapping.FeedId = rawFeedId;

            await _participantMappingOperations.DeleteParticipantMappingByPlayHQProfileId(participantMapping);
            _telemetryHandler.TrackTraceInfo($"Participant Mapping deleted in DB with participantId - {participantGuid}");
        }

        public async Task UpdateProfileClaimPlayHqIdInCdc(PlayHQClaimData playHqClaimFeed, Participant dbClaimedParticipantMapping, bool isDestinationProfileIdExist, long rawFeedId)
        {
            _telemetryHandler.TrackTraceInfo($"Updating the data in CDC");

            if (dbClaimedParticipantMapping.CricketId != null)
            {
                await UpdateClaimedProfileIdInCdcByCricketId(playHqClaimFeed, dbClaimedParticipantMapping, isDestinationProfileIdExist, rawFeedId);
            }
            else if (dbClaimedParticipantMapping.ParentCricketId != null)
            {
                await UpdateClaimedProfileIdInCdcByParentCricketId(playHqClaimFeed, dbClaimedParticipantMapping, isDestinationProfileIdExist, rawFeedId);
            }
        }

        private async Task UpdateClaimedProfileIdInCdcByParentCricketId(PlayHQClaimData playHqClaimFeed, Participant dbClaimedParticipantMapping, bool isDestinationProfileIdExist, long rawFeedId)
        {
            bool isChildExist = false;

            var result = await GetCdcDataByParentCricketId(dbClaimedParticipantMapping);

            if (result.Data != null && result.Data.ChildArray != null)
            {
                foreach (var cdcChild in result.Data.ChildArray)
                {
                    result.Data.Id = null;

                    if (cdcChild.Id != null && cdcChild.Id.PlayHQ.Equals(playHqClaimFeed.ClaimedProfileId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        isChildExist = true;
                        if (isDestinationProfileIdExist)
                        {
                             cdcChild.Id.Participant = string.Empty;
                        }
                        else
                        {
                            cdcChild.Id.Participant = dbClaimedParticipantMapping.ParticipantGuid.ToString();
                            cdcChild.Id.PlayHQ = playHqClaimFeed.DestinationProfileId.ToString();
                        }
                        await UpdateParticipantPlayHqIdInCdc(dbClaimedParticipantMapping.ParentCricketId.ToString(), rawFeedId, result.Data);
                        break;
                    }
                }
                if (!isChildExist)
                    _telemetryHandler.TrackTraceWarning($"Child not found. Unable to update in CDC. Uid - {dbClaimedParticipantMapping.ParentCricketId} RawFeedId: {rawFeedId}");
            }
            else
            {
                _telemetryHandler.TrackTraceWarning($"Child not found. Unable to update in CDC. Uid - {dbClaimedParticipantMapping.ParentCricketId} RawFeedId: {rawFeedId}");
            }
        }

        private async Task UpdateClaimedProfileIdInCdcByCricketId(PlayHQClaimData playHqClaimFeed, Participant dbClaimedParticipantMapping, bool isDestinationProfileIdExist, long rawFeedId)
        {
            var request = new CdcDataModel() { Id = new CdcIdFields() };
            if (isDestinationProfileIdExist)
            {
                request.Id.Participant = string.Empty;
            }
            else
            {
                request.Id.Participant = dbClaimedParticipantMapping.ParticipantGuid.ToString();
                request.PlayHQId = playHqClaimFeed.DestinationProfileId.ToString();
            }
            await UpdateParticipantPlayHqIdInCdc(dbClaimedParticipantMapping.CricketId.ToString(), rawFeedId, request);
        }

        private async Task UpdateParticipantPlayHqIdInCdc(string uId, long rawFeedId, object request)
        {
            if (request != null)
            {
                var param = new Dictionary<string, string>
                {
                  { "apiKey", _configuration.GetValue(AppSettingsKey.CDCApiKey) },
                  { "userKey", _configuration.GetValue(AppSettingsKey.CDCUserKey) },
                  { "secret", _configuration.GetValue(AppSettingsKey.CDCSecretKey) },
                  { "UID", uId.Replace("-","") },
                  { "data", JsonConvert.SerializeObject(request) }
                };

                var client = _httpClientFactory.CreateClient();
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.GetValue(AppSettingsKey.CDCAccountBaseUrl) + "accounts.setAccountInfo");
                httpRequestMessage.Content = new FormUrlEncodedContent(param);
                var response = await client.SendAsync(httpRequestMessage);
                var result = await response.Content.ReadAsStringAsync();

                var playHqResponse = JsonHelper.DeserializeJsonObject<Response>(result);

                if (playHqResponse.errorCode == 0)
                {
                    _telemetryHandler.TrackTraceInfo($"Data Updated in CDC Successfully.");
                }
                else
                {
                    var error = $"Error occurred while saving PlayHQId in CDC. Call Id: {playHqResponse.callId}. Error Message: {playHqResponse.errorMessage}. ErrorCode: {playHqResponse.errorCode} RawFeedId: {rawFeedId}";
                    throw new ApplicationException(error);
                }

                _telemetryHandler.TrackTraceInfo($"Processing feed Completed for {rawFeedId}");
            }
            else
            {
                var error = $"Error occurred while saving PlayHQId in CDC. RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }
        }

        private async Task<CdcGetAccountInfoResponse> GetCdcDataByParentCricketId(Participant dbClaimedParticipantMapping)
        {
            _telemetryHandler.TrackTraceInfo($"Getting User's account info details. Uid - {dbClaimedParticipantMapping.ParentCricketId}.");

            var param = new Dictionary<string, string>
                {
                    { "apiKey", _configuration.GetValue(AppSettingsKey.CDCApiKey) },
                    { "userKey", _configuration.GetValue(AppSettingsKey.CDCUserKey) },
                    { "secret", _configuration.GetValue(AppSettingsKey.CDCSecretKey) },
                    { "UID", dbClaimedParticipantMapping.ParentCricketId.ToString().Replace("-","") }
                };

            var client = _httpClientFactory.CreateClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.GetValue(AppSettingsKey.CDCAccountBaseUrl) + "accounts.getAccountInfo");
            httpRequestMessage.Content = new FormUrlEncodedContent(param);
            var response = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var cdcResponse = JsonHelper.DeserializeJsonObject<CdcGetAccountInfoResponse>(result);

            if (cdcResponse.ErrorCode == 0)
            {
                _telemetryHandler.TrackTraceInfo($"Received Users details from CDC.Uid - {dbClaimedParticipantMapping.ParentCricketId}.");
                return cdcResponse;
            }
            else
            {
                var error = $"Error occurred while getting user details from CDC. Call Id: {cdcResponse.CallId}. Error Message: {cdcResponse.ErrorMessage}. ErrorCode: {cdcResponse.ErrorCode}. Uid - {dbClaimedParticipantMapping.ParentCricketId}.";
                throw new ApplicationException(error);
            }
        }

        private async Task PublishProfileClaimEvent(Guid? destinationProfileId, Guid? claimedPlayHQProfileId, ParticipantMappingSaveModel participantMapping, string eventType)
        {
            var profileClaimPayload = new ProfileClaimPayload();
            profileClaimPayload.ParticipantId = participantMapping.ParticipantGuid;

            List<Guid?> playHqProfileIdList = new List<Guid?>();
            List<Guid?> claimedPlayHQProfileIdList = new List<Guid?>();

            if (participantMapping.PlayHQProfileId != null)
                playHqProfileIdList.Add(destinationProfileId);

            if (claimedPlayHQProfileId != null)
                claimedPlayHQProfileIdList.Add(claimedPlayHQProfileId);

            profileClaimPayload.PlayHQProfileId = playHqProfileIdList;
            profileClaimPayload.ClaimedPlayHQProfileId = claimedPlayHQProfileIdList;

            _telemetryHandler.TrackTraceInfo($"Sending feed to Grassroots shared event grid");
            await _changeTrack.TrackChange(profileClaimPayload, eventType, $"/Participant/{participantMapping.ParticipantGuid}");
        }

        #endregion
    }
}
