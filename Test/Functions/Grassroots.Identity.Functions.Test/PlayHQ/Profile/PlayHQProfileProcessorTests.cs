using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Common.PublishEvents.ChangeTrack;
using Moq;
using Moq.Protected;
using Xunit;
using System.Threading.Tasks;
using Grassroots.Identity.Functions.PlayHQ.Profile;
using Grassroots.Identity.Database.AccessLayer.Sql.Participant;
using Grassroots.Identity.Database.AccessLayer.Sql.ParticipantMapping;
using Grassroots.Identity.Functions.Common.Models;
using Grassroots.Identity.Functions.PlayHQ.Profile.Models;
using Grassroots.Identity.Database.Model.DbEntity;
using System.Collections.Generic;

namespace Grassroots.Identity.Functions.Test.PlayHQ.Profile
{
    public class PlayHQProfileProcessorTests
    {
        private readonly Mock<ITelemetryHandler> _telemetryHandlerMock = new Mock<ITelemetryHandler>();
        private readonly Mock<IConfigProvider> _configurationMock = new Mock<IConfigProvider>();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        private readonly Mock<IParticipantOperations> _participantOperationsMock = new Mock<IParticipantOperations>();
        private readonly Mock<IParticipantMappingOperations> _participantMappingOperationsMock = new Mock<IParticipantMappingOperations>();
        private readonly Mock<IChangeTrack> _changeTrackMock = new Mock<IChangeTrack>();

        [Fact]
        public async void PlayHQProfileFeed_Participant_Create_Return_Success()
        {
            // Setup
            MockConfigurationKey();

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

            var data = SetPlayHQProfileDataTestData(Guid.NewGuid().ToString());

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            var result = await processor.ProcessFeed(data, 1);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async void PlayHQProfileClaimFeed_Process_Return_Success()
        {
            // Setup
            MockConfigurationKey();
            var participant = new Participant() { CricketId = Guid.NewGuid() };
            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);

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

            var data = SetPlayHQProfileClaimTestData(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            var result = await processor.ProcessClaimFeed(data, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async void PlayHQProfileClaimFeed_Process_WhenParentCricketId_Return_Success()
        {
            // Setup
            MockConfigurationKey();
            var participant = new Participant() { ParentCricketId = Guid.NewGuid() };
            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);
            var claimedProfileId = "5a7f37de2293418b90d379aed6e3523d";
            var destinationProfileId = Guid.NewGuid().ToString();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"Data\":{\"Child\":[{\"Id\":{\"PlayHq\":\"5a7f37de2293418b90d379aed6e3523d\"}}]}}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = SetPlayHQProfileClaimTestData(claimedProfileId, destinationProfileId);

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            var result = await processor.ProcessClaimFeed(data, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async void PlayHQProfileClaimFeed_Process_WhenParentCricketId_ReturnInvalidCdcResponse_Fail()
        {
            // Setup
            MockConfigurationKey();
            var participant = new Participant() { ParentCricketId = Guid.NewGuid() };
            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);
            var claimedProfileId = "5a7f37de2293418b90d379aed6e3523d";
            var destinationProfileId = Guid.NewGuid().ToString();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"Data\":null}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = SetPlayHQProfileClaimTestData(claimedProfileId, destinationProfileId);

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            var result = await processor.ProcessClaimFeed(data, 1);
            // Assert
            _telemetryHandlerMock.Verify(x => x.TrackTraceWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void PlayHQProfileClaimFeed_Process_WhenParentCricketId_ReturnInvalidCdc_Child_Response_Fail()
        {
            // Setup
            MockConfigurationKey();
            var participant = new Participant() { ParentCricketId = Guid.NewGuid() };
            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);
            var claimedProfileId = "5a7f37de2293418b90d379aed6e3533d";
            var destinationProfileId = Guid.NewGuid().ToString();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":0,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\",\"Data\":{\"Child\":[{\"Id\":null}]}}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = SetPlayHQProfileClaimTestData(claimedProfileId, destinationProfileId);

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);
            var result = await processor.ProcessClaimFeed(data, 1);
            // Assert
            _telemetryHandlerMock.Verify(x => x.TrackTraceWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void PlayHQProfileClaimFeed_Process_ClaimProfileIdNotInDB_Return_Fail()
        {
            // Setup
            MockConfigurationKey();
            Participant participant = null;
            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);

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

            var data = SetPlayHQProfileClaimTestData(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessClaimFeed(data, 1));
        }

        [Fact]
        public async void PlayHQProfileClaimFeed_Process_Fail_CDC_ErrorCode_1()
        {
            // Setup
            MockConfigurationKey();
            var participant = new Participant() { ParentCricketId = Guid.NewGuid() };
            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"callId\":\"5a7f37de2293418b90d379aed6e3523d\",\"errorCode\":1,\"apiVersion\":2,\"statusCode\":200,\"statusReason\":\"OK\",\"time\":\"2021-05-07T09:19:26.696Z\"}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var data = SetPlayHQProfileClaimTestData(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessClaimFeed(data, 1));
        }

        [Fact]
        public async void PlayHQProfileClaimFeed_Process_claimProfileIdIsNull_Return_Fail()
        {
            // Setup
            MockConfigurationKey();
            var participant = new Participant() { CricketId = Guid.NewGuid() };
            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);

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

            var data = SetPlayHQProfileClaimTestData(null, Guid.NewGuid().ToString());

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessClaimFeed(data, 1));
        }

        [Fact]
        public async void PlayHQProfileClaimFeed_Process_claimProfileIdIsInvalid_Return_Fail()
        {
            // Setup
            MockConfigurationKey();
            var participant = new Participant() { CricketId = Guid.NewGuid() };
            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);

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

            var data = SetPlayHQProfileClaimTestData("Invalid", Guid.NewGuid().ToString());

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessClaimFeed(data, 1));
        }

        [Fact]
        public async void PlayHQProfileClaimFeed_Process_DestinationProfileIdIsInvalid_Return_Fail()
        {
            // Setup
            MockConfigurationKey();
            var participant = new Participant() { CricketId = Guid.NewGuid() };
            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);

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

            var data = SetPlayHQProfileClaimTestData(Guid.NewGuid().ToString(), "Invalid");

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessClaimFeed(data, 1));
        }

        [Fact]
        public async void PlayHQProfileClaimFeed_Process_DestinationProfileIdIsNull_Return_Fail()
        {
            // Setup
            MockConfigurationKey();
            var participant = new Participant() { CricketId = Guid.NewGuid() };
            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);

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

            var data = SetPlayHQProfileClaimTestData(Guid.NewGuid().ToString(), null);

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessClaimFeed(data, 1));
        }

        [Fact]
        public async void PlayHQProfileClaimFeed_Process_ClaimAndDestinationProfileIdIsSame_Return_Fail()
        {
            // Setup
            MockConfigurationKey();
            var participant = new Participant() { CricketId = Guid.NewGuid() };
            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);

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

            var data = SetPlayHQProfileClaimTestData("5A7f37de2293418b90d379aed6e3523d", "5a7f37de2293418b90d379aed6e3523d");

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessClaimFeed(data, 1));
        }

        [Fact]
        public async void PlayHQProfileFeed_Participant_Update_Return_Success()
        {
            // Setup
            MockConfigurationKey();

            var participant = new Participant() { CricketId = Guid.NewGuid() };
            var participantDetails = new List<ParticipantDetails>() { new ParticipantDetails() { LegacyPlayerId = 111 } };

            _participantOperationsMock.Setup(x => x.GetParticipantByPlayHQProfileId(It.IsAny<string>())).ReturnsAsync(participant);
            _participantOperationsMock.Setup(x => x.GetParticipantByParticipantId(It.IsAny<string>())).ReturnsAsync(participantDetails);

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

            var data = SetPlayHQProfileDataTestData(Guid.NewGuid().ToString());

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            var result = await processor.ProcessFeed(data, 1);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async void PlayHQProfileFeed_Participant_Create_ExternalAccountId_Null_Return_Success()
        {
            // Setup
            MockConfigurationKey();

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

            var data = SetPlayHQProfileDataTestData(null);

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            var result = await processor.ProcessFeed(data, 1);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async void PlayHQProfileFeed_Participant_Create_ExternalAccountId_InvalidGuid_Return_Success()
        {
            // Setup
            MockConfigurationKey();

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

            var data = SetPlayHQProfileDataTestData("5a7f37de8b90d3793523d");

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            var result = await processor.ProcessFeed(data, 1);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async void PlayHQProfileFeed_ShouldThrowException_ExternalAccountIdNull()
        {
            // Setup
            MockConfigurationKey();

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

            var data = new PlayHQProfileData()
            {
                //Id = Guid.NewGuid().ToString(),                
                AccountHolderProfileId = "Test",
                Participant = new PlayHqParticipant()
                {
                    FirstName = "Test",
                    LastName = "Test"
                },
                ProfileVisible = true,
                ExternalAccountId = null
            };

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessFeed(data, 1));
        }

        [Fact]
        public async void PlayHQProfileFeed_UpdatePlayHqIdInCDC_ShouldThrowException_BadRequest()
        {
            // Setup
            MockConfigurationKey();

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

            var data = new PlayHQProfileData()
            {
                Id = Guid.NewGuid().ToString(),
                AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                AccountHolderProfileId = "Test",
                Participant = new PlayHqParticipant()
                {
                    LastName = "Test"
                },
                ProfileVisible = true,
                ExternalAccountId = Guid.NewGuid().ToString()
            };

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.UpdatePlayHqIdInCdc(data, 1));
        }

        [Fact]
        public async void PlayHQProfileFeed_ValidateDataInCdc_ShouldThrowException_BadRequest()
        {
            // Setup
            MockConfigurationKey();

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

            var data = new PlayHQProfileData()
            {
                Id = Guid.NewGuid().ToString(),
                AccountHolderExternalAccountId = "bc3031a7a6804f68956169019f5df6d4",
                AccountHolderProfileId = "Test",
                Participant = new PlayHqParticipant()
                {
                    LastName = "Test"
                },
                ProfileVisible = true,
                ExternalAccountId = Guid.NewGuid().ToString()
            };

            // Act
            var processor = new PlayHQProfileFeedProcessor(_telemetryHandlerMock.Object, _httpClientFactoryMock.Object, _configurationMock.Object, _participantOperationsMock.Object, _participantMappingOperationsMock.Object, _changeTrackMock.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ValidateDataInCdc(data));
        }

        #region Private 

        private PlayHQProfileData SetPlayHQProfileDataTestData(string externalAccountId)
        {
            var data = new PlayHQProfileData()
            {
                Id = Guid.NewGuid().ToString(),
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
                ExternalAccountId = externalAccountId
            };

            return data;
        }

        private PlayHQClaimData SetPlayHQProfileClaimTestData(string claimedProfileId, string destinationProfileId)
        {
            return new PlayHQClaimData()
            {
                ClaimedProfileId = claimedProfileId,
                DestinationProfileId = destinationProfileId,
                Deleted = true
            };
        }

        private void MockConfigurationKey()
        {
            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCApiKey))
               .Returns(() => "3_U0rWER7APk2WdrnQwDWuaGAhEeAxw3HES27HcgMazfXHM2q05P1vcZhRA0J_UeAd");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCUserKey))
               .Returns(() => "AIUGKBCKsn2z");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCSecretKey))
               .Returns(() => "v7JF9flW3Y704PSvox9O6oqvGevriXQ7");

            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");
            _configurationMock.Setup(x => x.GetValue(AppSettingsKey.CDCAccountApiBaseUrl))
               .Returns(() => "https://accounts.au1.gigya.com/");
        }
        #endregion
    }
}

