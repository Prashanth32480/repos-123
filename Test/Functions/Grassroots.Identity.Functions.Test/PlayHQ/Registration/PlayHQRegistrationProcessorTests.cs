using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Common.PublishEvents.ChangeTrack;
using Moq;
using Moq.Protected;
using Xunit;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;
using Grassroots.Identity.Functions.PlayHQ.Profile;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant.RequestModel;
using Grassroots.Identity.API.PayLoadModel.PayLoads;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping;
using Grassroots.Identity.Functions.Common.Models;
using Grassroots.Identity.Functions.PlayHQ.Registration;

namespace Grassroots.Identity.Functions.Test.PlayHQ.Registration
{
    public class PlayHQRegistrationProcessorTests
    {
        private readonly Mock<ITelemetryHandler> _telemetryHandlerMock = new Mock<ITelemetryHandler>();
        private readonly Mock<IChangeTrack> _changeTrackMock = new Mock<IChangeTrack>();
        private readonly Mock<IConfigProvider> _configurationMock = new Mock<IConfigProvider>();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        private readonly Mock<IParticipantOperations> _participantOperationsMock = new Mock<IParticipantOperations>();
        private readonly Mock<IParticipantMappingOperations> _participantMappingOperationsMock = new Mock<IParticipantMappingOperations>();

        #region Create
        [Fact]
        public async void PlayHQProfileFeed_ShouldCall_SaveParticipant()
        {
            // Setup

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    { 
                        FirstName = "Test",
                        LastName = "Test",
                        Address = "Test",
                        Country = "Test",
                        DateOfBirth = "",
                        EmailAddress = "",
                        Gender = "",
                        MobileCountryCode = "",
                        MobilePhone = "",
                        Postcode = "",
                        State = "",
                        Suburb = ""
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            await processor.CreateParticipant(data, Database.Model.Static.FeedType.Program, 1);

            // Assert
            _participantOperationsMock.Verify(
                x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQProfileFeed_ShouldCall_SaveParticipant_And_PublishCreateEvent()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test",
                        Address = "Test",
                        Country = "Test",
                        DateOfBirth = "",
                        EmailAddress = "",
                        Gender = "",
                        MobileCountryCode = "",
                        MobilePhone = "",
                        Postcode = "",
                        State = "",
                        Suburb = ""
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            await processor.CreateParticipant(data, Database.Model.Static.FeedType.Program, 1);

            // Assert
            _participantOperationsMock.Verify(
                x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()),
                Times.AtLeastOnce()); 
            
            //_changeTrackMock.Verify(mock => mock.TrackChange(It.IsAny<ParticipantPayload>(),
            //    It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void PlayHQProfileFeed_SaveParticipant_ShouldThrowException_FirstNameNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        LastName = "Test"
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.CreateParticipant(data, Database.Model.Static.FeedType.Program,  1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }
        [Fact]
        public async void PlayHQProfileFeed_SaveParticipant_ShouldThrowException_LastNameNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test"
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.CreateParticipant(data, Database.Model.Static.FeedType.Program, 1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }
        [Fact]
        public async void PlayHQProfileFeed_SaveParticipant_ShouldThrowException_ProfileVisibleNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test"
                    },
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.CreateParticipant(data, Database.Model.Static.FeedType.Program, 1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }
        [Fact]
        public async void PlayHQProfileFeed_SaveParticipant_ShouldThrowException_AccountHolderProfileIdNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test"
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.CreateParticipant(data, Database.Model.Static.FeedType.Program, 1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }

        [Fact]
        public async void PlayHQProfileFeed_SaveParticipant_ShouldThrowException_AccountHolderExternalAccountIdNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test"
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.CreateParticipant(data, Database.Model.Static.FeedType.Program, 1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }
        [Fact]
        public async void PlayHQProfileFeed_SaveParticipant_ShouldThrowException_ProfileIdNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test"
                    },
                    ProfileVisible = true,
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.CreateParticipant(data, Database.Model.Static.FeedType.Program, 1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }




        #endregion

        #region Update

        [Fact]
        public async void PlayHQProfileFeed_UpdateParticipant_ShouldCall_SaveParticipant()
        {
            // Setup
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test",
                        Address = "Test",
                        Country = "Test",
                        DateOfBirth = "",
                        EmailAddress = "",
                        Gender = "",
                        MobileCountryCode = "",
                        MobilePhone = "",
                        Postcode = "",
                        State = "",
                        Suburb = ""
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()
                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            await processor.UpdateParticipant(data, Database.Model.Static.FeedType.Program, 1);

            // Assert
            _participantOperationsMock.Verify(
                x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQProfileFeed_UpdateParticipant_ShouldCall_SaveParticipant_And_PublishCreateEvent()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test",
                        Address = "Test",
                        Country = "Test",
                        DateOfBirth = "",
                        EmailAddress = "",
                        Gender = "",
                        MobileCountryCode = "",
                        MobilePhone = "",
                        Postcode = "",
                        State = "",
                        Suburb = ""
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            await processor.UpdateParticipant(data, Database.Model.Static.FeedType.Program, 1);

            // Assert
            _participantOperationsMock.Verify(
                x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()),
                Times.AtLeastOnce());

            //_changeTrackMock.Verify(mock => mock.TrackChange(It.IsAny<ParticipantPayload>(),
            //    It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void PlayHQProfileFeed_UpdateParticipant_SaveParticipant_ShouldThrowException_FirstNameNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        LastName = "Test"
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.UpdateParticipant(data, Database.Model.Static.FeedType.Program, 1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }

        [Fact]
        public async void PlayHQProfileFeed_UpdateParticipant_SaveParticipant_ShouldThrowException_LastNameNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test"
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.UpdateParticipant(data, Database.Model.Static.FeedType.Program, 1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }
        [Fact]
        public async void PlayHQProfileFeed_UpdateParticipant_SaveParticipant_ShouldThrowException_ProfileVisibleNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test"
                    },
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.UpdateParticipant(data, Database.Model.Static.FeedType.Program, 1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }
        [Fact]
        public async void PlayHQProfileFeed_UpdateParticipant_SaveParticipant_ShouldThrowException_AccountHolderProfileIdNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test"
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.UpdateParticipant(data, Database.Model.Static.FeedType.Program, 1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }

        [Fact]
        public async void PlayHQProfileFeed_UpdateParticipant_SaveParticipant_ShouldThrowException_AccountHolderExternalAccountIdNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test"
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.UpdateParticipant(data, Database.Model.Static.FeedType.Program, 1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }
        [Fact]
        public async void PlayHQProfileFeed_UpdateParticipant_SaveParticipant_ShouldThrowException_ProfileIdNull()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCPlayHqIdSchemaName))
               .Returns(() => "playHQId");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test"
                    },
                    ProfileVisible = true,
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.UpdateParticipant(data, Database.Model.Static.FeedType.Program, 1));

            _participantOperationsMock.Verify(x => x.SaveParticipant(It.IsAny<ParticipantSaveModel>()), Times.Never());
            _participantOperationsMock.Verify(x => x.GetParticipantByPlayHQProfileId(string.Empty), Times.Never());
        }

        #endregion

        #region Delete 
        [Fact]
        public async void ParticipantFeed_DeleteParticipant_ShouldCallParticipant_ParticipantGuidFound()
        {
            // Setup
            _participantOperationsMock.Setup(db => db.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync((new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() }));
            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test",
                        Address = "Test",
                        Country = "Test",
                        DateOfBirth = "",
                        EmailAddress = "",
                        Gender = "",
                        MobileCountryCode = "",
                        MobilePhone = "",
                        Postcode = "",
                        State = "",
                        Suburb = ""
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            await processor.DeleteParticipant(data, 1);
            // Assert
            _participantOperationsMock.Verify(
                x => x.DeleteParticipant(It.IsAny<ParticipantSaveModel>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void ParticipantFeed_DeleteParticipant_ShouldCallDelete_ParticipantGuidNotFound()
        {
            // Setup
            _participantOperationsMock.Setup(db => db.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync((Database.Model.DbEntity.Participant) null);
            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test",
                        Address = "Test",
                        Country = "Test",
                        DateOfBirth = "",
                        EmailAddress = "",
                        Gender = "",
                        MobileCountryCode = "",
                        MobilePhone = "",
                        Postcode = "",
                        State = "",
                        Suburb = ""
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            await Assert.ThrowsAsync<ApplicationException>(() => processor.DeleteParticipant(data, 1));
            // Assert
            _participantOperationsMock.Verify(
                x => x.DeleteParticipant(It.IsAny<ParticipantSaveModel>()),
                Times.Never());
        }

        [Fact]
        public async void ParticipantFeed_DeleteOrganisationParticipant_ShouldCallDelete_ProfileIdNull()
        {
            // Setup
            _participantOperationsMock.Setup(db => db.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync((Database.Model.DbEntity.Participant)null);
            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        FirstName = "Test",
                        LastName = "Test",
                        Address = "Test",
                        Country = "Test",
                        DateOfBirth = "",
                        EmailAddress = "",
                        Gender = "",
                        MobileCountryCode = "",
                        MobilePhone = "",
                        Postcode = "",
                        State = "",
                        Suburb = ""
                    },
                    ProfileVisible = true,
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            await Assert.ThrowsAsync<ApplicationException>(() => processor.DeleteParticipant(data, 1));
            // Assert
            _participantOperationsMock.Verify(
                x => x.DeleteParticipant(It.IsAny<ParticipantSaveModel>()),
                Times.Never());
        }

        #endregion
        [Fact]
        public async void PlayHQProfileFeed_UpdatePlayHqIdInCDC__When_No_Child_Exist_In_CDC_ShouldThrowException_BadRequest()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":4321,\"apiVersion\":2,\"statusCode\":400,\"statusReason\":\"Bad Request\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        LastName = "Test"
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            var searchResultList = new List<CdcSearchResultData>();
            

            CdcSearchResultData searchResult = new CdcSearchResultData()
            {
                Uid = "bc3031a7a6804f68956169019f5df6d4",
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.UpdatePlayHqIdInCDC(data, 1, "Test", searchResultList));
        }

        [Fact]
        public async void PlayHQProfileFeed_UpdatePlayHqIdInCDC_When_Child_Exist_In_CDC_ShouldThrowException_BadRequest()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":4321,\"apiVersion\":2,\"statusCode\":400,\"statusReason\":\"Bad Request\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        LastName = "Test"
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            var searchResultList = new List<CdcSearchResultData>();
            List<CdcChild> childArray = new List<CdcChild>();
            CdcChild child = new CdcChild()
            { 
                FirstName = "Test F Child",
                LastName = "Test L Child",
                ChildId = 1,
                Id = new CdcIdFields()
                { 
                    Participant = "Test",
                    MyCricket = "Test",
                    PlayHQ = "Test"
                }
            };

            childArray.Add(child);

            CdcSearchResultData searchResult = new CdcSearchResultData()
            {
                Uid = "bc3031a7a6804f68956169019f5df6d4",
                Data = new CdcDataModel()
                { 
                    ChildArray = childArray
                }
            };


            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.UpdatePlayHqIdInCDC(data, 1, "Test", searchResultList));
        }

        [Fact]
        public async void PlayHQProfileFeed_ValidateDataInCDC_ShouldThrowException_BadRequest()
        {
            var participant = new Database.Model.DbEntity.Participant { ParticipantGuid = Guid.NewGuid() };

            _participantOperationsMock.Setup(db =>
                    db.SaveParticipant(It.IsAny<ParticipantSaveModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":4321,\"apiVersion\":2,\"statusCode\":400,\"statusReason\":\"Bad Request\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = new PlayHQData()
            {
                Id = Guid.NewGuid(),
                Profile = new PlayHQProfile()
                {
                    AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                    AccountHolderProfileId = "Test",
                    Participant = new PlayHqParticipant()
                    {
                        LastName = "Test"
                    },
                    ProfileVisible = true,
                    Id = Guid.NewGuid().ToString(),
                    ExternalAccountId = Guid.NewGuid().ToString()


                }
            };

            // Act
            var processor = new PlayHQRegistrationFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ValidateDataInCdc(data));
        }
    }
}
