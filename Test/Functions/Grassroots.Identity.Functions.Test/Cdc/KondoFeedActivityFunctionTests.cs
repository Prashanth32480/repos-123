using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping.RequestModel;
using Grassroots.Identity.Database.Model.DbEntity;
using Grassroots.Identity.Functions.Cdc.Cdc;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using System;
using System.Collections.Generic;
using Grassroots.Common.Interact.Service.Client;
using Grassroots.Common.Interact.ServiceModel.Requests;
using Grassroots.Common.Interact.ServiceModel.Responses;
using Grassroots.Common.PublishEvents.ChangeTrack;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant.RequestModel;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping;
using Grassroots.Identity.Functions.Cdc.Common;
using Xunit;
using FsCheck;
using Grassroots.Common.Helpers.FeatureFlags;

namespace Grassroots.Identity.Functions.Cdc.Test.Cdc
{
    public class CdcProcessParticipantActivityFunctionTests
    {

        [Fact]
        public async void CdcProcessParticipantActivityFunction_Returns_Success()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            _participantMappingOperations.Setup(db =>
                    db.SaveParticipantMapping(It.IsAny<ParticipantMappingSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            ParticipantDetails pd = new ParticipantDetails()
            {
                CricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                FirstName = "",
                IsNameVisible = true,
                IsSearchable = true,
                LastName = "",
                LegacyPlayerId = 123,
                ParentCricketId = Guid.NewGuid()
            };

            IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { pd };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    }
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "123",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        }
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,_featureFlag.Object);
            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1,result);

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_Throws_ParticipantNotMapped_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            ParticipantDetails pd = new ParticipantDetails()
            {
                CricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                FirstName = "",
                IsNameVisible = true,
                IsSearchable = true,
                LastName = "",
                LegacyPlayerId = 123,
                ParentCricketId = Guid.NewGuid()
            };

            IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { pd };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    }
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            MyCricket = "123",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        }
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);

            await Assert.ThrowsAsync<ApplicationException>(() => function.Run(durableActivityContextMock.Object));

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_Throws_ParticipantNotExistInDb_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            IEnumerable<ParticipantDetails> pdDetails = null;

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    }
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "123",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        }
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);

            await Assert.ThrowsAsync<ApplicationException>(() => function.Run(durableActivityContextMock.Object));

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_Throws_Case1_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            ParticipantDetails pd = new ParticipantDetails()
            {
                CricketId = new Guid("6057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                FirstName = "",
                IsNameVisible = true,
                IsSearchable = true,
                LastName = "",
                LegacyPlayerId = 123,
                ParentCricketId = Guid.NewGuid()
            };

            IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { pd };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    }
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "123",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        }
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);

            await Assert.ThrowsAsync<ApplicationException>(() => function.Run(durableActivityContextMock.Object));

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_Throws_Case2_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>(); 
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            ParticipantDetails pd = new ParticipantDetails()
            {
                CricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                LegacyPlayerId = 123
            };

            _participantOperations.Setup(db =>
                    db.GetParticipantByMyCricketId(It.IsAny<int>()))
                .ReturnsAsync(pd);

            IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { pd };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    }
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "123",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        }
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);
            await Assert.ThrowsAsync<ApplicationException>(() => function.Run(durableActivityContextMock.Object));
            
        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_Throws_Case3_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>(); 
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            ParticipantDetails pd = new ParticipantDetails()
            {
                CricketId = new Guid("6057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("29ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                LegacyPlayerId = 123
            };

            ParticipantDetails participant = new ParticipantDetails()
            {
                CricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                LegacyPlayerId = 123
            };

            _participantOperations.Setup(db =>
                    db.GetParticipantByMyCricketId(It.IsAny<int>()))
                .ReturnsAsync(pd);

            IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { participant };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    }
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "123",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        }
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);
            await Assert.ThrowsAsync<ApplicationException>(() => function.Run(durableActivityContextMock.Object));

        }

        //[Fact]
        //public async void CdcProcessParticipantActivityFunction_Throws_Case4_Exception()
        //{
        //    var _telemetry = new Mock<ITelemetryHandler>();
        //    var _participantOperations = new Mock<IParticipantOperations>();
        //    var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
        //    var _interactServices_ = new Mock<IInteractServices>(); 
        //    var _changeTrack = new Mock<IChangeTrack>();
        //    var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
        //    var _featureFlag = new Mock<IFeatureFlag>();

        //    ParticipantDetails pd = new ParticipantDetails()
        //    {
        //        CricketId = null,
        //        ParticipantGuid = new Guid("29ea410d-5791-4a07-b22f-2ebf6626d9af"),
        //        PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
        //        LegacyPlayerId = 123,
        //        ParentCricketId = null
        //    };

        //    ParticipantDetails participant = new ParticipantDetails()
        //    {
        //        CricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
        //        ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
        //        PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
        //        LegacyPlayerId = 123
        //    };

        //    _participantOperations.Setup(db =>
        //            db.GetParticipantByMyCricketId(It.IsAny<int>()))
        //        .ReturnsAsync(pd);

        //    IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { participant };

        //    _participantOperations.Setup(db =>
        //            db.GetParticipantByParticipantId(It.IsAny<string>()))
        //        .ReturnsAsync(pdDetails);

        //    FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
        //    {
        //        FeedId = 1,
        //        Data = new CdcGetAccountInfoData()
        //        {
        //            Id = new CdcIdFields()
        //            {
        //                MyCricket = "123"
        //            }
        //        },
        //        Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
        //        UserAccountInfo = new CdcGetAccountInfoResponse()
        //        {
        //            Data = new CdcGetAccountInfoData()
        //            {
        //                Id = new CdcIdFields()
        //                {
        //                    Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
        //                    MyCricket = "123",
        //                    PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
        //                }
        //            }

        //        }
        //    };

        //    var durableActivityContextMock = new Mock<IDurableActivityContext>();
        //    durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
        //    var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);
        //    await Assert.ThrowsAsync<ApplicationException>(() => function.Run(durableActivityContextMock.Object));

        //}

        [Fact]
        public async void CdcProcessParticipantActivityFunction_UnlinkAccountHolderMyCricketId_ReturnSuccess()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>(); 
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            ParticipantDetails pd = new ParticipantDetails()
            {
                CricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                FirstName = "",
                IsNameVisible = true,
                IsSearchable = true,
                LastName = "",
                LegacyPlayerId = 123,
                ParentCricketId = Guid.NewGuid()
            };

            Participant participant = new Participant()
            {
                CricketId = Guid.NewGuid(),
                ParticipantGuid = Guid.NewGuid()
            };

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = ""
                    }
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        }
                    }

                }
            };

            ParticipantMappingSaveModel participantMapping = new ParticipantMappingSaveModel();

            PlayerResponse playerResponse = new PlayerResponse()
            {
                FName = "TestF",
                LName = "TestL",
                Id = 123
            };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantIdForUnlink(It.IsAny<string>()))
                .ReturnsAsync(pd);

            _participantMappingOperations.Setup(db =>
                    db.UnlinkMyCricketId(It.IsAny<ParticipantMappingSaveModel>()));

            _interactServices_.Setup(db =>
                    db.GetPlayer(It.IsAny<PlayerRequest>()))
                .ReturnsAsync(playerResponse);

            _participantOperations.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(new Guid());

            _participantMappingOperations.Setup(db =>
                    db.SaveParticipantMapping(It.IsAny<ParticipantMappingSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _interactServices_.Setup(db =>
                    db.GetPlayer(It.IsAny<PlayerRequest>()))
                .ReturnsAsync(playerResponse);

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object, _featureFlag.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1,result);

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_UnlinkAccountHolderMyCricketId__Throws_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            ParticipantDetails pd = new ParticipantDetails()
            {
                CricketId = new Guid("6057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                FirstName = "",
                IsNameVisible = true,
                IsSearchable = true,
                LastName = "",
                LegacyPlayerId = 0,
                ParentCricketId = Guid.NewGuid()
            };

            Participant participant = new Participant()
            {
                CricketId = Guid.NewGuid(),
                ParticipantGuid = Guid.NewGuid()
            };

            _participantOperations.Setup(db =>
                    db.GetParticipantByMyCricketId(It.IsAny<int>()))
                .ReturnsAsync(pd);

            _participantOperations.Setup(db =>
                    db.GetParticipantByPlayHQProfileId(Guid.NewGuid().ToString()))
                .ReturnsAsync(participant);

            _participantMappingOperations.Setup(db =>
                    db.SaveParticipantMapping(It.IsAny<ParticipantMappingSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = ""
                    }
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        }
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);

            await Assert.ThrowsAsync<ApplicationException>(() => function.Run(durableActivityContextMock.Object));

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_linkChildMyCricketId_Returns_Success()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            _participantMappingOperations.Setup(db =>
                    db.SaveParticipantMapping(It.IsAny<ParticipantMappingSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            ParticipantDetails pd = new ParticipantDetails()
            {
                ParentCricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                FirstName = "",
                IsNameVisible = true,
                IsSearchable = true,
                LastName = "",
                LegacyPlayerId = 123
            };

            IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { pd };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            CdcChild child = new CdcChild()
            {
                FirstName = "First",
                LastName = "Last",
                ChildId = 1,
                Id = new CdcIdFields()
                {
                    MyCricket = "300",
                    PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd",
                    Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af"
                }
            };

            List<CdcChild> children = new List<CdcChild>();
            children.Add(child);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    },
                    Child = children
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "1211",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        },
                        Child = children
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object, _featureFlag.Object);
            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1, result);

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_linkChildMyCricketId_Throws_ParticipantNotMapped_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            ParticipantDetails pd = new ParticipantDetails()
            {
                CricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                FirstName = "",
                IsNameVisible = true,
                IsSearchable = true,
                LastName = "",
                LegacyPlayerId = 123,
                ParentCricketId = Guid.NewGuid()
            };

            IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { pd };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            CdcChild child = new CdcChild()
            {
                FirstName = "First",
                LastName = "Last",
                ChildId = 1,
                Id = new CdcIdFields()
                {
                    MyCricket = "300",
                    PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                }
            };

            List<CdcChild> children = new List<CdcChild>();
            children.Add(child);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    },
                    Child = children
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "1211",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        },
                        Child = children
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1, result);

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_linkChildMyCricketId_Throws_ParticipantNotExistInDb_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            IEnumerable<ParticipantDetails> pdDetails = null;

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            CdcChild child = new CdcChild()
            {
                FirstName = "First",
                LastName = "Last",
                ChildId = 1,
                Id = new CdcIdFields()
                {
                    MyCricket = "300",
                    PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd",
                    Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af"
                }
            };

            List<CdcChild> children = new List<CdcChild>();
            children.Add(child);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    },
                    Child = children
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "1211",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        },
                        Child = children
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1, result);

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_linkChildMyCricketId_Throws_Case1_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

           ParticipantDetails pd = new ParticipantDetails()
            {
                ParentCricketId = new Guid("6057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                FirstName = "",
                IsNameVisible = true,
                IsSearchable = true,
                LastName = "",
                LegacyPlayerId = 123
            };

            IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { pd };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            CdcChild child = new CdcChild()
            {
                FirstName = "First",
                LastName = "Last",
                ChildId = 1,
                Id = new CdcIdFields()
                {
                    MyCricket = "300",
                    PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd",
                    Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af"
                }
            };

            List<CdcChild> children = new List<CdcChild>();
            children.Add(child);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    },
                    Child = children
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "1211",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        },
                        Child = children
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1, result);

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_linkChildMyCricketId_Throws_Case2_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            ParticipantDetails pd = new ParticipantDetails()
            {
                ParentCricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                LegacyPlayerId = 123
            };

            _participantOperations.Setup(db =>
                    db.GetParticipantByMyCricketId(It.IsAny<int>()))
                .ReturnsAsync(pd);

            IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { pd };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            CdcChild child = new CdcChild()
            {
                FirstName = "First",
                LastName = "Last",
                ChildId = 1,
                Id = new CdcIdFields()
                {
                    MyCricket = "300",
                    PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd",
                    Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af"
                }
            };

            List<CdcChild> children = new List<CdcChild>();
            children.Add(child);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    },
                    Child = children
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "1211",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        },
                        Child = children
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1, result);

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_linkChildMyCricketId_Throws_Case3_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            ParticipantDetails pd = new ParticipantDetails()
            {
                ParentCricketId = new Guid("6057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("29ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                LegacyPlayerId = 123
            };

            ParticipantDetails participant = new ParticipantDetails()
            {
                ParentCricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                LegacyPlayerId = 123
            };

            _participantOperations.Setup(db =>
                    db.GetParticipantByMyCricketId(It.IsAny<int>()))
                .ReturnsAsync(pd);

            IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { participant };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            CdcChild child = new CdcChild()
            {
                FirstName = "First",
                LastName = "Last",
                ChildId = 1,
                Id = new CdcIdFields()
                {
                    MyCricket = "300",
                    PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd",
                    Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af"
                }
            };

            List<CdcChild> children = new List<CdcChild>();
            children.Add(child);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    },
                    Child = children
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "1211",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        },
                        Child = children
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object,  _featureFlag.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1, result);

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_linkChildMyCricketIdThrows_Case4_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            ParticipantDetails pd = new ParticipantDetails()
            {
                CricketId = null,
                ParticipantGuid = new Guid("29ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                LegacyPlayerId = 123,
                ParentCricketId = null
            };

            ParticipantDetails participant = new ParticipantDetails()
            {
                ParentCricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                LegacyPlayerId = 123
            };

            _participantOperations.Setup(db =>
                    db.GetParticipantByMyCricketId(It.IsAny<int>()))
                .ReturnsAsync(pd);

            IEnumerable<ParticipantDetails> pdDetails = new List<ParticipantDetails>() { participant };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantId(It.IsAny<string>()))
                .ReturnsAsync(pdDetails);

            CdcChild child = new CdcChild()
            {
                FirstName = "First",
                LastName = "Last",
                ChildId = 1,
                Id = new CdcIdFields()
                {
                    MyCricket = "300",
                    PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd",
                    Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af"
                }
            };

            List<CdcChild> children = new List<CdcChild>();
            children.Add(child);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    },
                    Child = children
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "1211",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        },
                        Child = children
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object, _featureFlag.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1, result);

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_UnlinkChildMyCricketId__Returns_Success()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            _participantMappingOperations.Setup(db =>
                    db.SaveParticipantMapping(It.IsAny<ParticipantMappingSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            ParticipantDetails pd = new ParticipantDetails()
            {
                ParentCricketId = new Guid("5057fce3-b797-4d4e-a008-2003ef02d87c"),
                ParticipantGuid = new Guid("19ea410d-5791-4a07-b22f-2ebf6626d9af"),
                PlayHQProfileId = new Guid("c38aca4a-24d9-4350-a2e2-03c9f7f932cd"),
                FirstName = "",
                IsNameVisible = true,
                IsSearchable = true,
                LastName = "",
                LegacyPlayerId = 123
            };

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantIdForUnlink(It.IsAny<string>()))
                .ReturnsAsync(pd);

            CdcChild child = new CdcChild()
            {
                FirstName = "First",
                LastName = "Last",
                ChildId = 1,
                Id = new CdcIdFields()
                {
                    PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd",
                    Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af"
                }
            };

            List<CdcChild> children = new List<CdcChild>();
            children.Add(child);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    },
                    Child = children
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "1211",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        },
                        Child = children
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object, _featureFlag.Object);
            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1, result);

        }

        [Fact]
        public async void CdcProcessParticipantActivityFunction_UnlinkChildMyCricketId__Throws_Exception()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _participantOperations = new Mock<IParticipantOperations>();
            var _participantMappingOperations = new Mock<IParticipantMappingOperations>();
            var _interactServices_ = new Mock<IInteractServices>();
            var _changeTrack = new Mock<IChangeTrack>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();
            var _featureFlag = new Mock<IFeatureFlag>();

            _participantMappingOperations.Setup(db =>
                    db.SaveParticipantMapping(It.IsAny<ParticipantMappingSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            ParticipantDetails pd = null;

            _participantOperations.Setup(db =>
                    db.GetParticipantByParticipantIdForUnlink(It.IsAny<string>()))
                .ReturnsAsync(pd);

            CdcChild child = new CdcChild()
            {
                FirstName = "First",
                LastName = "Last",
                ChildId = 1,
                Id = new CdcIdFields()
                {
                    PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd",
                    Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af"
                }
            };

            List<CdcChild> children = new List<CdcChild>();
            children.Add(child);

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                FeedId = 1,
                Data = new CdcGetAccountInfoData()
                {
                    Id = new CdcIdFields()
                    {
                        MyCricket = "123"
                    },
                    Child = children
                },
                Uid = "5057fce3-b797-4d4e-a008-2003ef02d87c",
                UserAccountInfo = new CdcGetAccountInfoResponse()
                {
                    Data = new CdcGetAccountInfoData()
                    {
                        Id = new CdcIdFields()
                        {
                            Participant = "19ea410d-5791-4a07-b22f-2ebf6626d9af",
                            MyCricket = "1211",
                            PlayHq = "c38aca4a-24d9-4350-a2e2-03c9f7f932cd"
                        },
                        Child = children
                    }

                }
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new KondoFeedActivityFunction(_telemetry.Object, _participantOperations.Object, _participantMappingOperations.Object, _interactServices_.Object, _changeTrack.Object, _rawFeedProcessor.Object, _featureFlag.Object);
            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1, result);

        }
    }
}