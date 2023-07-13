using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Grassroots.Common.Helpers.Telemetry;
using System;
using System.Collections.Generic;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping.RequestModel;
using Grassroots.Identity.Database.Model.DbEntity;
using Grassroots.Common.Interact.Service.Client;
using Grassroots.Common.Interact.ServiceModel.Requests;
using Grassroots.Common.Interact.ServiceModel.Responses;
using Grassroots.Common.PublishEvents.ChangeTrack;
using Grassroots.Identity.API.PayLoadModel.PayLoads;
using Grassroots.Identity.API.PayLoadModel.Static;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant.RequestModel;
using Grassroots.Identity.Functions.Cdc.Common;
using ServiceStack;
using System.Linq;
using Grassroots.Common.Helpers.FeatureFlags;

namespace Grassroots.Identity.Functions.Cdc.Cdc
{
    public class KondoFeedActivityFunction
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IParticipantOperations _participantOperations;
        private readonly IParticipantMappingOperations _participantMappingOperations;
        private readonly IInteractServices _interactServices;
        private readonly IChangeTrack _changeTrack;
        private readonly IRawFeedProcessor _rawFeedProcessor;
        private readonly IFeatureFlag _featureFlag;

        public KondoFeedActivityFunction(ITelemetryHandler telemetryHandler
            , IParticipantOperations participantOperations
            , IParticipantMappingOperations participantMappingOperations
            , IInteractServices interactServices
            , IChangeTrack changeTrack
            , IRawFeedProcessor rawFeedProcessor
            , IFeatureFlag featureFlag)
        {
            _telemetryHandler = telemetryHandler;
            _participantOperations = participantOperations;
            _participantMappingOperations = participantMappingOperations;
            _interactServices = interactServices;
            _changeTrack = changeTrack;
            _rawFeedProcessor = rawFeedProcessor;
            _featureFlag = featureFlag;
        }

        [FunctionName("KondoFeedActivityFunction")]
        public async Task<int> Run([ActivityTrigger]IDurableActivityContext activityContext)
        {
           var request = activityContext.GetInput<FeedActivityFunctionRequest>();
           _telemetryHandler.TrackTraceInfo($"Starting with processing the request for Kondo. Feed Id - {request.FeedId}. Uid - {request.Uid}.");
            return await ProcessCdcEventAsync(request);
        }
        private async Task<int> ProcessCdcEventAsync(FeedActivityFunctionRequest data)
        {
            List<int> legacyPlayerIdList = new List<int>();
            Guid playHqProfileId = new Guid();
            
            int legacyPlayerId = 0;

            if (data.Data.Child != null && data.Data.Child.Any())
            {
                foreach (var child in data.Data.Child)
                {
                    ParticipantPayload participant = new ParticipantPayload();
                    int childLegacyPlayerid = 0;
                    Guid childPlayHqProfileId = new Guid();
                    List<int> childLegacyPlayerIdList = new List<int>();

                    participant.ParentCricketId = new Guid(data.Uid);

                    if (child.Id != null && child.Id.MyCricket != null)
                        Int32.TryParse(child.Id.MyCricket, out childLegacyPlayerid);

                    //Calculate PlayHQId
                    if (child.Id != null && child.Id.PlayHq != null)
                        childPlayHqProfileId = new Guid(child.Id.PlayHq);

                    //Calculate ParticipantId
                    if (child.Id != null && child.Id.Participant != null)
                        participant.ParticipantId = new Guid(child.Id.Participant);
                    _telemetryHandler.TrackTraceInfo($"LegacyPlayerId received in request - {childLegacyPlayerid}, Participant Id mapped in CDC - {participant.ParticipantId} for child {child.ChildId}. Feed Id - {data.FeedId}. Uid - {data.Uid}.");
                    if (childLegacyPlayerid != 0)
                        childLegacyPlayerIdList.Add(Convert.ToInt32(childLegacyPlayerid));

                    participant.LegacyPlayerId = childLegacyPlayerIdList;
                    participant.PlayHQProfileId = new List<Guid?>();

                    if (childLegacyPlayerid != 0)
                        await LinkChildMyCricketidAsync(participant, Convert.ToInt32(data.FeedId));
                    else
                        await UnLinkChildMyCricketidAsync(participant, Convert.ToInt32(data.FeedId));
                }

                await _rawFeedProcessor.SetRawFeedStatusToSuccess(Convert.ToInt32(data.FeedId));
                _telemetryHandler.TrackTraceInfo($"Setting Raw Feed Status to success. Feed Id - {data.FeedId}. Uid - {data.Uid}.");

                return 1;
            }
            else
            {
                ParticipantPayload participant = new ParticipantPayload();
                participant.CricketId = new Guid(data.Uid);

                //Calculate MyCricketId
                if (data.Data != null && data.Data.Id != null && data.Data.Id.MyCricket != null)
                    Int32.TryParse(data.Data.Id.MyCricket, out legacyPlayerId);
                else if (data.Data != null && data.Data.MyCricketId != null)
                    Int32.TryParse(data.Data.MyCricketId, out legacyPlayerId);

                //Calculate ParticipantId
                if (data.UserAccountInfo.Data != null && data.UserAccountInfo.Data.Id != null && data.UserAccountInfo.Data.Id.Participant != null)
                    participant.ParticipantId = new Guid(data.UserAccountInfo.Data.Id.Participant);

                _telemetryHandler.TrackTraceInfo($"LegacyPlayerId received in request - {legacyPlayerId}, Participant Id mapped in CDC - {participant.ParticipantId}. Feed Id - {data.FeedId}. Uid - {data.Uid}.");

                if (legacyPlayerId != 0)
                    legacyPlayerIdList.Add(Convert.ToInt32(legacyPlayerId));

                participant.LegacyPlayerId = legacyPlayerIdList;
                participant.PlayHQProfileId = new List<Guid?>();

                if (legacyPlayerId != 0)
                    return await LinkParentMyCricketidAsync(participant, Convert.ToInt32(data.FeedId));
                else
                    return await UnLinkParentMyCricketidAsync(participant, Convert.ToInt32(data.FeedId));
            }
        }
        private async Task<int> LinkParentMyCricketidAsync(ParticipantPayload participant, long feedId)
        {
            var error = string.Empty;
            ParticipantDetails dbParticipant = null;
            int legacyPlayerId = participant.LegacyPlayerId.Any() ? Convert.ToInt32(participant.LegacyPlayerId.FirstOrDefault()) : 0;

            _telemetryHandler.TrackTraceInfo($"Processing Account Holder link MyCricketId request. Feed Id - {feedId}.");
            if (legacyPlayerId != 0)
                dbParticipant = await _participantOperations.GetParticipantByMyCricketId(legacyPlayerId);

            var dbParticipantByParticipantid = await _participantOperations.GetParticipantByParticipantId(participant.ParticipantId.ToString());

            if (participant.ParticipantId == new Guid())
            {
                error = $"Participant Id is not mapped with user in CDC. MyCricketId: {legacyPlayerId}. FeedId: {feedId}";
            }
            else if (dbParticipantByParticipantid == null || !dbParticipantByParticipantid.Any())
            {
                error = $"Participant does not exist in DB mapped with Participant Id : {participant.ParticipantId}. FeedId: {feedId}";
            }
            else if (dbParticipantByParticipantid.FirstOrDefault().CricketId != participant.CricketId) // Case #1 CricketId and ParticipantId do not match
            {
                error = $"CricketId and ParticipantId do not match existing Participant record. CricketId: {participant.CricketId}. MyCricketId: {legacyPlayerId}. FeedId: {feedId}";
            }
            else if (dbParticipant != null && dbParticipant.ParticipantGuid == participant.ParticipantId) // Case #2 MyCricket ID already mapped to same Participant
            {
                error = $"MyCricketId - {legacyPlayerId} already associated with the same participant {dbParticipant.ParticipantGuid}. FeedId: {feedId}";
            }
            else if (dbParticipant != null && dbParticipant.ParticipantGuid != participant.ParticipantId &&
                     dbParticipant.CricketId == null && dbParticipant.ParentCricketId == null) //Case #4 Records require merging
            {
                participant.IsNameSuppressed = dbParticipantByParticipantid.FirstOrDefault().IsNameVisible;
                participant.IsNameVisible = dbParticipantByParticipantid.FirstOrDefault().IsNameVisible;
                participant.IsSearchable = dbParticipantByParticipantid.FirstOrDefault().IsSearchable;
                participant.FirstName = dbParticipantByParticipantid.FirstOrDefault().FirstName;
                participant.LastName = dbParticipantByParticipantid.FirstOrDefault().LastName;
                participant.PlayHQProfileId = new List<Guid?>();

                foreach (var mapping in dbParticipantByParticipantid)
                {
                    if (mapping.LegacyPlayerId != null && mapping.LegacyPlayerId != 0)
                        participant.LegacyPlayerId.Add(Convert.ToInt32(mapping.LegacyPlayerId));

                    if (mapping.PlayHQProfileId != null)
                        participant.PlayHQProfileId.Add(mapping.PlayHQProfileId);
                }

                await HandleMergeAccountAsync(dbParticipant, participant, feedId);
                return 1;
            }
            else if (dbParticipant != null && dbParticipant.ParticipantGuid != participant.ParticipantId &&
                     dbParticipant.CricketId != participant.CricketId) //Case #3 MyCricket ID already mapped to a different Participant
            {
                error = $"MyCricketId is already mapped to another Participant. CricketId: {participant.CricketId}. MyCricketId: {legacyPlayerId}. FeedId: {feedId}";
            }


            if (!string.IsNullOrWhiteSpace(error))
                throw new ApplicationException(error);

            var participantMapping = new ParticipantMappingSaveModel
            {
                ParticipantGuid = participant.ParticipantId,
                LegacyPlayerId = legacyPlayerId,
                FeedId = feedId
            };

            await _participantMappingOperations.SaveParticipantMapping(participantMapping);
            _telemetryHandler.TrackTraceInfo($"Participant Mapping saved in DB with participantId - {participant.ParticipantId}, MyCricketId - {legacyPlayerId}. FeedId: {feedId}");

            participant.IsNameSuppressed = dbParticipantByParticipantid.FirstOrDefault().IsNameVisible;
            participant.IsNameVisible = dbParticipantByParticipantid.FirstOrDefault().IsNameVisible;
            participant.IsSearchable = dbParticipantByParticipantid.FirstOrDefault().IsSearchable;
            participant.FirstName = dbParticipantByParticipantid.FirstOrDefault().FirstName;
            participant.LastName = dbParticipantByParticipantid.FirstOrDefault().LastName;
            participant.PlayHQProfileId = new List<Guid?>();

            foreach (var mapping in dbParticipantByParticipantid)
            {
                if (mapping.LegacyPlayerId != null && mapping.LegacyPlayerId != 0)
                    participant.LegacyPlayerId.Add(Convert.ToInt32(mapping.LegacyPlayerId));

                if (mapping.PlayHQProfileId != null)
                    participant.PlayHQProfileId.Add(mapping.PlayHQProfileId);
            }



            await PublishUpdatedEvent(participant, feedId);

            await _rawFeedProcessor.SetRawFeedStatusToSuccess(feedId);
            _telemetryHandler.TrackTraceInfo($"Setting Raw Feed Status to success. FeedId: {feedId}");
            return 1;
        }
        private async Task<int> UnLinkParentMyCricketidAsync(ParticipantPayload participant, long feedId)
        {
            var error = string.Empty;
            ParticipantDetails dbParticipant = null;
            PlayerResponse interactResponse = null;
            int legacyPlayerId = participant.LegacyPlayerId.Any() ? Convert.ToInt32(participant.LegacyPlayerId.FirstOrDefault()) : 0;

            _telemetryHandler.TrackTraceInfo($"Processing Account Holder Unlink MyCricketId request. FeedId: {feedId}");

            dbParticipant = await _participantOperations.GetParticipantByParticipantIdForUnlink(participant.ParticipantId.ToString());

            if (dbParticipant != null)
            {
                _telemetryHandler.TrackTraceInfo($"Participant Found in DB with Cricket id - {participant.CricketId}. participantId - {dbParticipant.ParticipantGuid}. FeedId: {feedId}");

                var participantMapping = new ParticipantMappingSaveModel
                {
                    ParticipantGuid = dbParticipant.ParticipantGuid,
                    FeedId = feedId
                };

                await _participantMappingOperations.UnlinkMyCricketId(participantMapping);
                _telemetryHandler.TrackTraceInfo($"MyCricketId Unlinked in DB with participantId - {participant.ParticipantId}. FeedId: {feedId}");

                var dbParticipantByLegacyPlayerId = await _participantOperations.GetParticipantByMyCricketId(Convert.ToInt32(dbParticipant.LegacyPlayerId));

                if (dbParticipantByLegacyPlayerId != null)
                {
                    throw new ApplicationException($"Duplicate participant found in DB with with LegacyPlayerId: {dbParticipant.LegacyPlayerId} . FeedId: {feedId}");
                }

                if (dbParticipant.LegacyPlayerId != null && dbParticipant.LegacyPlayerId > 0)
                    interactResponse = await GetParticipantFromInteractAsync(dbParticipant.LegacyPlayerId, feedId);

                if (interactResponse != null && interactResponse.Id > 0)
                {
                    _telemetryHandler.TrackTraceInfo($"Received participant details from Interact API for LegacyPlayerId - {interactResponse.Id}. FeedId: {feedId}");
                    await CreateParticipantAsync(interactResponse, feedId);
                }
                else
                    _telemetryHandler.TrackTraceInfo($"Received null response from Interact API for LegacyPlayerId - {legacyPlayerId}");

                _telemetryHandler.TrackTraceInfo($"Processing Account Holder Unlink MyCricketId request completed");
            }
            else
                throw new ApplicationException($"Participant Not Found in DB with Cricket id - {participant.CricketId} mapped with any LegacyPlayerId. FeedId: {feedId}");

            await _rawFeedProcessor.SetRawFeedStatusToSuccess(feedId);
            _telemetryHandler.TrackTraceInfo($"Setting Raw Feed Status to success. FeedId: {feedId}");
            return 1;
        }
        private async Task<int> LinkChildMyCricketidAsync(ParticipantPayload participant, long feedId)
        {
            var error = string.Empty;
            ParticipantDetails dbParticipant = null;
            int legacyPlayerId = participant.LegacyPlayerId.Any() ? Convert.ToInt32(participant.LegacyPlayerId.FirstOrDefault()) : 0;

            _telemetryHandler.TrackTraceInfo($"Processing Child link MyCricketId request. FeedId: {feedId}");
            if (legacyPlayerId != 0)
                dbParticipant = await _participantOperations.GetParticipantByMyCricketId(legacyPlayerId);

            var dbParticipantByParticipantGuid = await _participantOperations.GetParticipantByParticipantId(participant.ParticipantId.ToString());// Update this to get bby participantGuid

            if (participant.ParticipantId == new Guid())
            {
                error = $"Participant Id is not mapped with user in CDC. MyCricketId: {legacyPlayerId}. FeedId: {feedId}";
            }
            else if (dbParticipantByParticipantGuid == null || !dbParticipantByParticipantGuid.Any())
            {
                error = $"Participant does not exist in DB mapped with Participant Guid : {participant.ParticipantId}. FeedId: {feedId}";
            }
            else if (dbParticipantByParticipantGuid.FirstOrDefault().ParentCricketId != participant.ParentCricketId) // Case #1 CricketId and ParticipantId do not match
            {
                error = $"ParentCricketId and ParticipantId do not match existing Participant record. ParentCricketId: {participant.ParentCricketId}. MyCricketId: {legacyPlayerId}. FeedId: {feedId}";
            }
            else if (dbParticipant != null && dbParticipant.ParticipantGuid == participant.ParticipantId) // Case #2 MyCricket ID already mapped to same Participant
            {
                error = $"MyCricketId - {legacyPlayerId} already associated with the same participant {dbParticipant.ParticipantGuid}. FeedId: {feedId}";
            }
            else if (dbParticipant != null && dbParticipant.ParticipantGuid != participant.ParticipantId &&
                     dbParticipant.CricketId == null && dbParticipant.ParentCricketId == null) //Case #4 Records require merging
            {
                participant.IsNameSuppressed = dbParticipantByParticipantGuid.FirstOrDefault().IsNameVisible;
                participant.IsNameVisible = dbParticipantByParticipantGuid.FirstOrDefault().IsNameVisible;
                participant.IsSearchable = dbParticipantByParticipantGuid.FirstOrDefault().IsSearchable;
                participant.FirstName = dbParticipantByParticipantGuid.FirstOrDefault().FirstName;
                participant.LastName = dbParticipantByParticipantGuid.FirstOrDefault().LastName;
                participant.PlayHQProfileId = new List<Guid?>();

                foreach (var mapping in dbParticipantByParticipantGuid)
                {
                    if (mapping.LegacyPlayerId != null && mapping.LegacyPlayerId != 0)
                        participant.LegacyPlayerId.Add(Convert.ToInt32(mapping.LegacyPlayerId));

                    if (mapping.PlayHQProfileId != null)
                        participant.PlayHQProfileId.Add(mapping.PlayHQProfileId);
                }

                await HandleMergeAccountAsync(dbParticipant, participant, feedId);
                return 1;
            }
            else if (dbParticipant != null && dbParticipant.ParticipantGuid != participant.ParticipantId &&
                     dbParticipant.ParentCricketId != participant.ParentCricketId) //Case #3 MyCricket ID already mapped to a different Participant
            {
                error = $"MyCricketId is already mapped to another Participant. ParentCricketId: {participant.ParentCricketId}. MyCricketId: {legacyPlayerId}. FeedId: {feedId}";
            }


            if (!string.IsNullOrWhiteSpace(error))
            {
                _telemetryHandler.TrackTraceInfo(error);
                return 0;
            }

            var participantMapping = new ParticipantMappingSaveModel
            {
                ParticipantGuid = participant.ParticipantId,
                LegacyPlayerId = legacyPlayerId,
                FeedId = feedId
            };

            await _participantMappingOperations.SaveParticipantMapping(participantMapping);
            _telemetryHandler.TrackTraceInfo($"Participant Mapping saved in DB with participantId - {participant.ParticipantId}, MyCricketId - {legacyPlayerId}. FeedId: {feedId}");

            participant.IsNameSuppressed = dbParticipantByParticipantGuid.FirstOrDefault().IsNameVisible;
            participant.IsNameVisible = dbParticipantByParticipantGuid.FirstOrDefault().IsNameVisible;
            participant.IsSearchable = dbParticipantByParticipantGuid.FirstOrDefault().IsSearchable;
            participant.FirstName = dbParticipantByParticipantGuid.FirstOrDefault().FirstName;
            participant.LastName = dbParticipantByParticipantGuid.FirstOrDefault().LastName;
            participant.PlayHQProfileId = new List<Guid?>();

            foreach (var mapping in dbParticipantByParticipantGuid)
            {
                if (mapping.LegacyPlayerId != null && mapping.LegacyPlayerId != 0)
                    participant.LegacyPlayerId.Add(Convert.ToInt32(mapping.LegacyPlayerId));

                if (mapping.PlayHQProfileId != null)
                    participant.PlayHQProfileId.Add(mapping.PlayHQProfileId);
            }

            await PublishUpdatedEvent(participant, feedId);

            await _rawFeedProcessor.SetRawFeedStatusToSuccess(feedId);
            _telemetryHandler.TrackTraceInfo($"Setting Raw Feed Status to success. FeedId: {feedId}");
            return 1;
        }
        private async Task<int> UnLinkChildMyCricketidAsync(ParticipantPayload participant, long feedId)
        {
            var error = string.Empty;
            ParticipantDetails dbParticipant = null;
            PlayerResponse interactResponse = null;
            int legacyPlayerId = participant.LegacyPlayerId.Any() ? Convert.ToInt32(participant.LegacyPlayerId.FirstOrDefault()) : 0;

            _telemetryHandler.TrackTraceInfo($"Processing Child Unlink MyCricketId request. FeedId: {feedId}");

            dbParticipant = await _participantOperations.GetParticipantByParticipantIdForUnlink(participant.ParticipantId.ToString());

            if (dbParticipant != null)
            {
                _telemetryHandler.TrackTraceInfo($"Participant Found in DB with ParentCricket id - {participant.ParentCricketId}. participantId - {dbParticipant.ParticipantGuid}. FeedId: {feedId}");

                var participantMapping = new ParticipantMappingSaveModel
                {
                    ParticipantGuid = dbParticipant.ParticipantGuid,
                    FeedId = feedId
                };

                await _participantMappingOperations.UnlinkMyCricketId(participantMapping);
                _telemetryHandler.TrackTraceInfo($"MyCricketId Unlinked in DB with participantId - {participant.ParticipantId}. FeedId: {feedId}");

                var dbParticipantByLegacyPlayerId = await _participantOperations.GetParticipantByMyCricketId(Convert.ToInt32(dbParticipant.LegacyPlayerId));

                if (dbParticipantByLegacyPlayerId != null)
                {
                    _telemetryHandler.TrackTraceInfo($"Duplicate participant found in DB with with LegacyPlayerId: {dbParticipant.LegacyPlayerId} . FeedId: {feedId}");
                    return 0;
                }

                if (dbParticipant.LegacyPlayerId != null && dbParticipant.LegacyPlayerId > 0)
                    interactResponse = await GetParticipantFromInteractAsync(dbParticipant.LegacyPlayerId, feedId);

                if (interactResponse != null && interactResponse.Id > 0)
                {
                    _telemetryHandler.TrackTraceInfo($"Received participant details from Interact API for LegacyPlayerId - {interactResponse.Id}. FeedId: {feedId}");
                    await CreateParticipantAsync(interactResponse, feedId);
                }
                else
                    _telemetryHandler.TrackTraceInfo($"Received null response from Interact API for LegacyPlayerId - {legacyPlayerId}. FeedId: {feedId}");

                _telemetryHandler.TrackTraceInfo($"Processing Child Unlink MyCricketId request completed. FeedId: {feedId}");
            }
            else
            {
                _telemetryHandler.TrackTraceInfo($"Participant Not Found in DB with Participant Guid - {participant.ParticipantId} mapped with any LegacyPlayerId. FeedId: {feedId}");
                return 0;
            }

            return 1;
        }
        private async Task<int> HandleMergeAccountAsync(ParticipantDetails existingParticipant, ParticipantPayload participant, long feedId)
        {
            var existingParticipantMapping = new ParticipantMappingSaveModel
            {
                ParticipantGuid = existingParticipant.ParticipantGuid,
                FeedId = feedId,
                LegacyPlayerId = Convert.ToInt32(participant.LegacyPlayerId.FirstOrDefault())
            };

            await _participantMappingOperations.UnlinkMyCricketId(existingParticipantMapping);

            var participantMapping = new ParticipantMappingSaveModel
            {
                ParticipantGuid = participant.ParticipantId,
                LegacyPlayerId = Convert.ToInt32(participant.LegacyPlayerId.FirstOrDefault()),
                FeedId = feedId
            };

            await _participantMappingOperations.SaveParticipantMapping(participantMapping);

            var dbParticipant = await _participantMappingOperations.GetParticipantMappingByParticipanyGuid(Convert.ToString(existingParticipant.ParticipantGuid));

            if (dbParticipant == null)
            {
                var participantToDelete = new ParticipantSaveModel
                {
                    ParticipantGuid = existingParticipant.ParticipantGuid,
                    FeedId = feedId
                };

                await _participantOperations.DeleteParticipant(participantToDelete);

            }
            else
            {
                _telemetryHandler.TrackTraceInfo($"Existing Participant with Participant Guid - {existingParticipant.ParticipantGuid} found mapped with other ids. FeedId: {feedId}");
            }

            _telemetryHandler.TrackTraceInfo($"Sending feed to Grassroots shared event grid");
            await _changeTrack.TrackChange(participant, EventType.KondoParticipantUpdated, $"/Participant/{participant.ParticipantId}");

            _telemetryHandler.TrackTraceInfo($"Merge Completed for existing DB participant - {existingParticipant.ParticipantGuid}. New Participant - {participant.ParticipantId}. LegacyPlayerId - {Convert.ToInt32(participant.LegacyPlayerId.FirstOrDefault())}. FeedId: {feedId}");

            await _rawFeedProcessor.SetRawFeedStatusToSuccess(feedId);
            _telemetryHandler.TrackTraceInfo($"Setting Raw Feed Status to success. FeedId:  {feedId}");

            return 1;
        }
        private async Task<PlayerResponse> GetParticipantFromInteractAsync(int? legacyPlayerId, long feedId)
        {
            _telemetryHandler.TrackTraceInfo($"Getting participant details from Interact API for LegacyPlayerId - {legacyPlayerId}. FeedId: {feedId}");
            var playerRequest = new PlayerRequest
            {
                PlayerId = Convert.ToInt32(legacyPlayerId)
            };

            var response =  await _interactServices.GetPlayer(playerRequest);
            return response;

        }
        private async Task CreateParticipantAsync(PlayerResponse interactResponse, long feedId)
        {
            var participantGuid = new Guid();

            var participant = new ParticipantSaveModel
            {
                ParticipantGuid = participantGuid,
                FirstName = interactResponse.FName,
                LastName = interactResponse.LName,
                IsNameVisible = false,
                IsSearchable = false,
                FeedId = feedId
            };

            participantGuid = await _participantOperations.SaveParticipant(participant);
            _telemetryHandler.TrackTraceInfo($"Participant saved in DB with participantId - {participantGuid}. FeedId: {feedId}");
            participant.ParticipantGuid = participantGuid;

            var participantMapping = new ParticipantMappingSaveModel
            {
                ParticipantGuid = participantGuid,
                LegacyPlayerId = interactResponse.Id,
                FeedId = feedId
            };

            await _participantMappingOperations.SaveParticipantMapping(participantMapping);
            _telemetryHandler.TrackTraceInfo($"Participant Mapping saved in DB with participantId - {participantGuid}. FeedId: {feedId}");
            await PublishCreateEvent(participant, participantMapping, feedId);

        }
        private async Task PublishCreateEvent(Participant participant, ParticipantMappingSaveModel participantMapping, long feedId)
        {
            var participantPayload = participant.ConvertTo<ParticipantPayload>();
            participantPayload.ParticipantId = participant.ParticipantGuid;
            participantPayload.IsNameSuppressed = participant.IsNameVisible;
            List<Guid?> playHqProfileIdList = new List<Guid?>();
            List<int> legacyPlayerId = new List<int>();

            if (participantMapping.PlayHQProfileId != null)
                playHqProfileIdList.Add(participantMapping.PlayHQProfileId);

            if (participantMapping.LegacyPlayerId != null && participantMapping.LegacyPlayerId != 0)
                legacyPlayerId.Add(Convert.ToInt32(participantMapping.LegacyPlayerId));

            participantPayload.PlayHQProfileId = playHqProfileIdList;
            participantPayload.LegacyPlayerId = legacyPlayerId;

            _telemetryHandler.TrackTraceInfo($"Sending create feed to Grassroots shared event grid. FeedId: {feedId}");
            await _changeTrack.TrackChange(participantPayload, EventType.KondoParticipantCreated, $"/Participant/{participant.ParticipantGuid}");
        }
        private async Task PublishUpdatedEvent(ParticipantPayload participant, long feedId)
        {
            _telemetryHandler.TrackTraceInfo($"Sending update feed to Grassroots shared event grid. FeedId: {feedId}");
            await _changeTrack.TrackChange(participant, EventType.KondoParticipantUpdated, $"/Participant/{participant.ParticipantId}");
        }

    }
}
