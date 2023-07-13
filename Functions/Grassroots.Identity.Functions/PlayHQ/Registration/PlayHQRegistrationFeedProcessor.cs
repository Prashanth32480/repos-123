using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Grassroots.Identity.Functions.Common.Models;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant.RequestModel;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Common.PublishEvents.ChangeTrack;
using Grassroots.Identity.API.PayLoadModel.PayLoads;
using ServiceStack;
using Newtonsoft.Json;
using System.Linq;
using Grassroots.Identity.API.PayLoadModel.Static;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping.RequestModel;

namespace Grassroots.Identity.Functions.PlayHQ.Registration
{
    public class PlayHQRegistrationFeedProcessor : IPlayHQRegistrationFeedProcessor
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigProvider _configuration;
        private readonly IParticipantOperations _participantOperations;
        private readonly IParticipantMappingOperations _participantMappingOperations;
        private readonly IChangeTrack _changeTrack;

        public PlayHQRegistrationFeedProcessor(ITelemetryHandler telemetryHandler
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

       

        #region Create

        public async Task<Guid> CreateParticipant(PlayHQData playHqParticipant, FeedType feedType, long rawFeedId)
        {
            _telemetryHandler.TrackTraceInfo($"Processing Create Registration feed started for  UID - {playHqParticipant.Profile.AccountHolderExternalAccountId}, PlayHQId - {playHqParticipant.Profile.AccountHolderProfileId}, RawFeedId - {rawFeedId}");
            ValidatePlayHqCreateFeed(playHqParticipant, rawFeedId);
            Guid participantGuid = await ProcessCreateFeed(playHqParticipant, feedType, rawFeedId);

            _telemetryHandler.TrackTraceInfo($"Processing Create Registration feed Completed");

            return participantGuid;
        }

        private async Task<Guid> ProcessCreateFeed(PlayHQData feed, FeedType feedType, long rawFeedId)
        {
            var participantGuid = new Guid();
            var isSearchable = false;
            string eventType = EventType.KondoParticipantCreated;
            List<int> legacyPlayerId = new List<int>();

            var dbParticipant = await _participantOperations.GetParticipantByPlayHQProfileId(feed.Profile.Id);
           
            if (dbParticipant != null)
            {
                _telemetryHandler.TrackTraceInfo($"Participant found in DB with participantId - {dbParticipant.ParticipantGuid}");
                Guid? externalAccountId = null;
                eventType = EventType.KondoParticipantUpdated;

                if (!string.IsNullOrWhiteSpace(feed.Profile.ExternalAccountId))
                    externalAccountId = new Guid(feed.Profile.ExternalAccountId);

                if (dbParticipant.CricketId != null && externalAccountId != null && !externalAccountId.Equals(dbParticipant.CricketId))
                {
                    var error = $"CricketId Mismatch for PlayHQ Cricket ID - {feed.Profile.ExternalAccountId} . RawFeedId: {rawFeedId}";
                    throw new ApplicationException(error);
                }
                participantGuid = dbParticipant.ParticipantGuid;

                var dbParticipantByParticipantid = await _participantOperations.GetParticipantByParticipantId(participantGuid.ToString());

                foreach (var mapping in dbParticipantByParticipantid)
                {
                    if (mapping.LegacyPlayerId != null && mapping.LegacyPlayerId != 0)
                        legacyPlayerId.Add(Convert.ToInt32(mapping.LegacyPlayerId));
                }
            }

            if (feedType == FeedType.Competition && feed.Profile.ProfileVisible != null)
                isSearchable = feed.Profile.ProfileVisible ?? false;

            Guid? cricketId = null;
            Guid? parentCricketId = null;

            if (!string.IsNullOrWhiteSpace(feed.Profile.ExternalAccountId))
                cricketId = new Guid(feed.Profile.ExternalAccountId);

            if (!feed.Profile.AccountHolder)
                parentCricketId = new Guid(feed.Profile.AccountHolderExternalAccountId);


            var participant = new ParticipantSaveModel
            {
                ParticipantGuid = participantGuid,
                CricketId = cricketId,
                FirstName = feed.Profile.Participant.FirstName,
                LastName = feed.Profile.Participant.LastName,
                IsNameVisible = feed.Profile.ProfileVisible ?? false,
                IsSearchable = isSearchable,
                FeedId = rawFeedId,
                ParentCricketId = parentCricketId
            };           

            participantGuid = await _participantOperations.SaveParticipant(participant);
            participant.ParticipantGuid = participantGuid;

            var participantMapping = new ParticipantMappingSaveModel
            {
                ParticipantGuid = participantGuid,
                PlayHQProfileId = new Guid(feed.Profile.Id),
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

            var searchResult = await ValidateDataInCdc(feed);
            await UpdatePlayHqIdInCDC(feed, rawFeedId, participantGuid.ToString(), searchResult);

            await PublishCreateEvent(participant, participantMapping, eventType, legacyPlayerId);

            return participantGuid;
        }
        private async Task PublishCreateEvent(Database.Model.DbEntity.Participant participant, ParticipantMappingSaveModel participantMapping, string eventType, List<int> legacyPlayerId)
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
        private void ValidatePlayHqCreateFeed(PlayHQData participant, long rawFeedId)
        {
            if (string.IsNullOrWhiteSpace(participant.Profile.Participant.FirstName))
            {
                var error = $"Required Value missing for PlayHQ Registration Feed (Data.Profile.Participant.FirstName). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(participant.Profile.Participant.LastName))
            {
                var error = $"Required Value missing for PlayHQ Registration Feed (Data.Profile.Participant.LastName). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if (participant.Profile.ProfileVisible == null)
            {
                var error = $"Required Value missing for PlayHQ Registration Feed (Data.Profile.ProfileVisible). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(participant.Profile.AccountHolderProfileId))
            {
                var error = $"Required Value missing for PlayHQ Registration Feed (Data.Profile.AccountHolderProfileId). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(participant.Profile.AccountHolderExternalAccountId))
            {
                var error = $"Required Value missing for PlayHQ Registration Feed (Data.Profile.AccountHolderExternalAccountId). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(participant.Profile.Id))
            {
                var error = $"Required Value missing for PlayHQ Registration Feed (Data.Profile.Id). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }
        }

        #endregion

        #region Update

        public async Task<Guid> UpdateParticipant(PlayHQData playHqParticipant, FeedType feedType, long rawFeedId)
        {
            _telemetryHandler.TrackTraceInfo($"Processing Update Registration feed started for  UID - {playHqParticipant.Profile.AccountHolderExternalAccountId}, PlayHQId - {playHqParticipant.Profile.AccountHolderProfileId}, RawFeedId - {rawFeedId}");

            var dbParticipant = await ValidatePlayHqUpdateFeed(playHqParticipant, rawFeedId);
            if (dbParticipant.ParticipantGuid != Guid.Empty)
            {
                _telemetryHandler.TrackTraceInfo($"Participant found in DB with participantId - {dbParticipant.ParticipantGuid}");

                await ProcessUpdateFeed(playHqParticipant, feedType, dbParticipant, rawFeedId);
            }
            else
                dbParticipant.ParticipantGuid = await CreateParticipant(playHqParticipant, feedType, rawFeedId);

            _telemetryHandler.TrackTraceInfo($"Processing Update Registration feed Completed");
            return dbParticipant.ParticipantGuid;
        }
        private async Task<Guid> ProcessUpdateFeed(PlayHQData feed, FeedType feedType, Database.Model.DbEntity.Participant dbParticipant, long rawFeedId)
        {
            var isSearchable = false;
            Guid? externalAccountId = null;
            List<int> legacyPlayerId = new List<int>();

            if (!string.IsNullOrWhiteSpace(feed.Profile.ExternalAccountId))
                externalAccountId = new Guid(feed.Profile.ExternalAccountId);

            if (dbParticipant.CricketId != null && externalAccountId != null && !externalAccountId.Equals(dbParticipant.CricketId))
            {
                var error = $"CricketId Mismatch for PlayHQ Cricket ID - {feed.Profile.ExternalAccountId} . RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }
            var participantGuid = dbParticipant.ParticipantGuid;

            var dbParticipantByParticipantid = await _participantOperations.GetParticipantByParticipantId(participantGuid.ToString());

            foreach (var mapping in dbParticipantByParticipantid)
            {
                if (mapping.LegacyPlayerId != null && mapping.LegacyPlayerId != 0)
                    legacyPlayerId.Add(Convert.ToInt32(mapping.LegacyPlayerId));
            }

            if (feedType == FeedType.Competition && feed.Profile.ProfileVisible != null)
                isSearchable = feed.Profile.ProfileVisible ?? false;

            Guid? cricketId = null;

            Guid? parentCricketId = null;

            if (!string.IsNullOrWhiteSpace(feed.Profile.ExternalAccountId))
                cricketId = new Guid(feed.Profile.ExternalAccountId);

            if (!feed.Profile.AccountHolder)
                parentCricketId = new Guid(feed.Profile.AccountHolderExternalAccountId);

            var participant = new ParticipantSaveModel
            {
                ParticipantGuid = participantGuid,
                CricketId = cricketId,
                FirstName = feed.Profile.Participant.FirstName,
                LastName = feed.Profile.Participant.LastName,
                IsNameVisible = feed.Profile.ProfileVisible ?? false,
                IsSearchable = isSearchable,
                FeedId = rawFeedId,
                ParentCricketId = parentCricketId
            };

            participantGuid = await _participantOperations.SaveParticipant(participant);
            participant.ParticipantGuid = participantGuid;
            _telemetryHandler.TrackTraceInfo($"Participant updated in DB with participantId - {participantGuid}");

            var participantMapping = new ParticipantMappingSaveModel
            {
                ParticipantGuid = participantGuid,
                PlayHQProfileId = new Guid(feed.Profile.Id),
                FeedId = rawFeedId
            };

            var searchResult = await ValidateDataInCdc(feed);
            await UpdatePlayHqIdInCDC(feed, rawFeedId, dbParticipant.ParticipantGuid.ToString(), searchResult);

            await PublishUpdateEvent(participant, participantMapping, legacyPlayerId);

            return participantGuid;
        }

        private async Task PublishUpdateEvent(Database.Model.DbEntity.Participant participant,  ParticipantMappingSaveModel participantMapping, List<int> legacyPlayerId)
        {
            var participantPayload = participant.ConvertTo<ParticipantPayload>();
            participantPayload.ParticipantId = participant.ParticipantGuid;
            participantPayload.IsNameSuppressed = participant.IsNameVisible;
            List<Guid?> playHqProfileIdList = new List<Guid?>();
            //List<int> legacyPlayerId = new List<int>();

            if (participantMapping.PlayHQProfileId != null)
                playHqProfileIdList.Add(participantMapping.PlayHQProfileId);

            participantPayload.PlayHQProfileId = playHqProfileIdList;
            participantPayload.LegacyPlayerId = legacyPlayerId;

            _telemetryHandler.TrackTraceInfo($"Sending feed to grassroots shared event grid");
            await _changeTrack.TrackChange(participantPayload, API.PayLoadModel.Static.EventType.KondoParticipantUpdated, $"/Participant/{participant.ParticipantGuid}");
        }

        private async Task<Database.Model.DbEntity.Participant> ValidatePlayHqUpdateFeed(PlayHQData participant, long rawFeedId)
        {
            if (string.IsNullOrWhiteSpace(participant.Profile.Participant.FirstName))
            {
                var error = $"Required Value missing for PlayHQ Competition Feed (Data.Profile.Participant.FirstName). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(participant.Profile.Participant.LastName))
            {
                var error = $"Required Value missing for PlayHQ Competition Feed (Data.Profile.Participant.LastName). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if (participant.Profile.ProfileVisible == null)
            {
                var error = $"Required Value missing for PlayHQ Competition Feed (Data.Profile.ProfileVisible). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(participant.Profile.AccountHolderProfileId))
            {
                var error = $"Required Value missing for PlayHQ Competition Feed (Data.Profile.AccountHolderProfileId). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(participant.Profile.AccountHolderExternalAccountId))
            {
                var error = $"Required Value missing for PlayHQ Competition Feed (Data.Profile.AccountHolderExternalAccountId). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            if (string.IsNullOrWhiteSpace(participant.Profile.Id))
            {
                var error = $"Required Value missing for PlayHQ Competition Feed (Data.Profile.Id). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            var dbParticipant = await _participantOperations.GetParticipantByPlayHQProfileId(participant.Profile.Id);

            if (dbParticipant == null)
            {
                var error = $"Create event not received. RawFeedId: {rawFeedId}";
                _telemetryHandler.TrackTraceError(error);
                return new Database.Model.DbEntity.Participant();
            }

            return dbParticipant;

        }
        #endregion

        #region Delete

        public async Task<Guid> DeleteParticipant(PlayHQData playHqParticipant, long rawFeedId)
        {
            _telemetryHandler.TrackTraceInfo($"Processing Delete Registration feed started for  UID - {playHqParticipant.Profile.AccountHolderExternalAccountId}, PlayHQId - {playHqParticipant.Profile.AccountHolderProfileId}, RawFeedId - {rawFeedId}");

            var participantGuid = await ValidatePlayHqDeleteFeed(playHqParticipant, rawFeedId);
            await ProcessDeleteFeed(rawFeedId, participantGuid);
            _telemetryHandler.TrackTraceInfo($"Processing Delete Registration feed Completed");
            return participantGuid;
            
        }

        private async Task ProcessDeleteFeed(long rawFeedId, Guid participantGuid)
        {
            var participant = new ParticipantSaveModel
            {
                ParticipantGuid = participantGuid,
                FeedId = rawFeedId
            };

            var participantMapping = new ParticipantMappingSaveModel
            {
                ParticipantGuid = participantGuid,
                FeedId = rawFeedId
            };

            await _participantOperations.DeleteParticipant(participant);
            await _participantMappingOperations.DeleteParticipantMapping(participantMapping);
            _telemetryHandler.TrackTraceInfo($"Participant deleted in DB with participantId - {participantGuid}");
            await PublishDeleteEvent(participant);
        }

        private async Task PublishDeleteEvent(Database.Model.DbEntity.Participant participant)
        {
            var participantPayload = new ParticipantDeletePayload();
            participantPayload.ParticipantId = participant.ParticipantGuid;
            _telemetryHandler.TrackTraceInfo($"Sending feed to grassroots shared event grid.");
            await _changeTrack.TrackChange(participantPayload, API.PayLoadModel.Static.EventType.KondoParticipantDeleted, $"/Participant/{participant.ParticipantGuid}");
        }

        private async Task<Guid> ValidatePlayHqDeleteFeed(PlayHQData participant, long rawFeedId)
        {
            if (string.IsNullOrWhiteSpace(participant.Profile.Id))
            {
                var error = $"Required Value missing for PlayHQ Participant Feed (Data.Profile.Id). RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            var dbParticipant = await _participantOperations.GetParticipantByPlayHQProfileId(participant.Profile.Id);

            if (dbParticipant == null)
            {
                var error = $"Could not locate PlayHQ Profile Id {participant.Profile.Id}. RawFeedId: {rawFeedId}";
                throw new ApplicationException(error);
            }

            return  dbParticipant.ParticipantGuid;


        }
        #endregion

        #region CDC 

        public async Task<List<CdcSearchResultData>> ValidateDataInCdc(PlayHQData playHqProfile)
        {
            _telemetryHandler.TrackTraceInfo("Getting the Search Result from CDC.");
            var param = new Dictionary<string, string>
            {
                {"apiKey", _configuration.GetValue(AppSettingsKey.CDCApiKey)},
                {"userKey", _configuration.GetValue(AppSettingsKey.CDCUserKey)},
                {"secret", _configuration.GetValue(AppSettingsKey.CDCSecretKey)},
                {"query", $"select UID, data.child, data.id.playHQ, data.playHQId, data.id.participant from accounts where data.id.playHQ = \"{playHqProfile.Profile.AccountHolderProfileId}\" OR data.playHQId = \"{playHqProfile.Profile.AccountHolderProfileId}\" OR UID = \"{playHqProfile.Profile.AccountHolderExternalAccountId}\""}
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
                var error = $"Error received from CDC - {parsedResult.CallId + " | " + parsedResult.ErrorDetails + " | " + parsedResult.ErrorMessage} for UID: {playHqProfile.Profile.AccountHolderExternalAccountId}";
                throw new ApplicationException(error);
            }

            _telemetryHandler.TrackTraceInfo("Received the search result response from CDC.");
            return parsedResult.Results;
        }

        public async Task UpdatePlayHqIdInCDC(PlayHQData playHqFeed, long rawFeedId, string participantGuid, List<CdcSearchResultData> cdcSearchResult)
        {
            _telemetryHandler.TrackTraceInfo($"Updating the data in CDC for UID - {playHqFeed.Profile.AccountHolderExternalAccountId}");
            if (!string.IsNullOrWhiteSpace(playHqFeed.Profile.AccountHolderExternalAccountId))
            {
                var param = new Dictionary<string, string>();
                var request = new CdcDataModel()
                {
                    Id = new CdcIdFields()
                };

                if (!playHqFeed.Profile.AccountHolder)//Update child details in CDC if profile is of dependent
                {
                    _telemetryHandler.TrackTraceInfo("Setting up the dependent request to update in CDC");
                    List<CdcChild> childArray = new List<CdcChild>();

                    if (cdcSearchResult != null && cdcSearchResult.Count > 0)
                    {
                        int childId = 1;
                        bool addNewChild = true;

                        var userRec = cdcSearchResult.Where(x => x.Uid == playHqFeed.Profile.AccountHolderExternalAccountId);

                        if (userRec.Any() && userRec.FirstOrDefault().Data != null && userRec.FirstOrDefault().Data.ChildArray != null && userRec.FirstOrDefault().Data.ChildArray.Count > 0)
                        {
                            _telemetryHandler.TrackTraceInfo("Children list found in CDC");
                            childId = userRec.FirstOrDefault().Data.ChildArray.Count + 1;

                            foreach (var child in userRec.FirstOrDefault().Data.ChildArray)
                            {
                                if (child.Id.PlayHQ == playHqFeed.Profile.Id)
                                {
                                    addNewChild = false;
                                    child.FirstName = playHqFeed.Profile.Participant.FirstName;
                                    child.LastName = playHqFeed.Profile.Participant.LastName;
                                    child.Id = new CdcIdFields()
                                    {
                                        Participant = participantGuid,
                                        PlayHQ = playHqFeed.Profile.Id
                                    };
                                    childArray.Add(child);
                                    _telemetryHandler.TrackTraceInfo($"Updating the existing child in CDC with playHQ Id - {child.Id.PlayHQ}");
                                }
                                else
                                    childArray.Add(child);
                            }
                        }

                        if (addNewChild)
                        {
                            CdcChild child = new CdcChild()
                            {
                                ChildId = childId,
                                FirstName = playHqFeed.Profile.Participant.FirstName,
                                LastName = playHqFeed.Profile.Participant.LastName,
                                Id = new CdcIdFields()
                                {
                                    Participant = participantGuid,
                                    PlayHQ = playHqFeed.Profile.Id
                                }
                            };
                            childArray.Add(child);
                        }
                    }

                    request.ChildArray = childArray;
                    _telemetryHandler.TrackTraceInfo("Child Array Prepared");

                }
                else //account holder (Parent)
                {
                    var user = cdcSearchResult.Where(x => x.Uid.Equals(playHqFeed.Profile.AccountHolderExternalAccountId));
                    if(!(user != null && user.FirstOrDefault().Data != null && user.FirstOrDefault().Data.Id != null && !string.IsNullOrEmpty(user.FirstOrDefault().Data.Id.Participant)))
                        request.Id.Participant = participantGuid;
                }

                //Update PlayHQ ID if the playHQProfileId received in request is not already mapped with any user in CDC
                if (cdcSearchResult != null && cdcSearchResult.Count == 1)
                {
                    string playHQId = null;
                    if (cdcSearchResult.FirstOrDefault().Data != null && (cdcSearchResult.FirstOrDefault().Data.PlayHQId != null || cdcSearchResult.FirstOrDefault().Data.Id.PlayHQ != null))
                        playHQId = cdcSearchResult.FirstOrDefault().Data.PlayHQId != null ? cdcSearchResult.FirstOrDefault().Data.PlayHQId : cdcSearchResult.FirstOrDefault().Data.Id.PlayHQ;

                    if((cdcSearchResult.FirstOrDefault().Data != null && !string.IsNullOrEmpty(cdcSearchResult.FirstOrDefault().Data.PlayHQId)) || (cdcSearchResult.FirstOrDefault().Data != null && cdcSearchResult.FirstOrDefault().Data.Id != null && !string.IsNullOrEmpty(cdcSearchResult.FirstOrDefault().Data.Id.PlayHQ)))
                        _telemetryHandler.TrackTraceInfo($"PlayHQ Id already mapped for user with UID - {playHqFeed.Profile.AccountHolderExternalAccountId} in CDC.");                    
                    else 
                    {
                        _telemetryHandler.TrackTraceInfo("Setting up the new playHQ Id in request object");
                        request.PlayHQId = playHqFeed.Profile.AccountHolderProfileId;
                    }

                }
                else if (cdcSearchResult != null && cdcSearchResult.Count > 1)
                {
                    _telemetryHandler.TrackTraceInfo("PlayHQ Id provided in request already mapped with some other user");
                }
                else
                    _telemetryHandler.TrackTraceInfo($"User not found in CDC with UID - {playHqFeed.Profile.AccountHolderExternalAccountId}");


                param.Add("apiKey", _configuration.GetValue(AppSettingsKey.CDCApiKey));
                param.Add("userKey", _configuration.GetValue(AppSettingsKey.CDCUserKey));
                param.Add("secret", _configuration.GetValue(AppSettingsKey.CDCSecretKey));
                param.Add("UID", playHqFeed.Profile.AccountHolderExternalAccountId);
                param.Add("data", JsonConvert.SerializeObject(request));
                
                var client = _httpClientFactory.CreateClient();
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _configuration.GetValue(AppSettingsKey.CDCAccountBaseUrl) + "accounts.setAccountInfo");
                httpRequestMessage.Content = new FormUrlEncodedContent(param);
                var response = await client.SendAsync(httpRequestMessage);
                var result = await response.Content.ReadAsStringAsync();

                var playHqResponse = JsonHelper.DeserializeJsonObject<Response>(result);

                if (playHqResponse.errorCode == 0)
                {
                    _telemetryHandler.TrackTraceInfo($"Data updated in CDC Successfully for {rawFeedId}");
                }
                else
                {
                    var error = $"Error occurred while saving Data in CDC. Call Id: {playHqResponse.callId}. Error Message: {playHqResponse.errorMessage}. ErrorCode: {playHqResponse.errorCode} RawFeedId: {rawFeedId}";
                    throw new ApplicationException(error);
                }
            }
        }
        #endregion

    }
}
