using System;
using System.Collections.Generic;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using Moq;
using Xunit;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Grassroots.Identity.Functions.Cdc.Common;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Grassroots.Identity.Functions.Test.Cdc.Accounts
{
    public class CdcAccountFeedProcessorTests
    {
        private readonly Mock<ITelemetryHandler> _telemetryHandlerMock = new Mock<ITelemetryHandler>();
        private readonly Mock<IRawFeedProcessor> _rawFeedProcessor = new Mock<IRawFeedProcessor>();
        private readonly Mock<IConfigProvider> _configurationMock = new Mock<IConfigProvider>();

        [Fact]
        public async void CdcAccountsFeed_ShouldCall_PublishEvent()
        {
            // Setup
            _configurationMock.Setup(x => x.GetValue(Functions.Cdc.AppSettingsKey.CdcApiKeysToProcessFeeds))
                .Returns(() => "ApiKey");

            var lstEvents = new List<CdcEvent>();
            var client = new Mock<IDurableOrchestrationClient>();
            CdcEvent cdcEvent = new CdcEvent()
            {
                ApiKey = "ApiKey",
                CallId = "Test",
                Data = new CdcEventData()
                {
                    AccountType = "Test Account",
                    UId = "Test Uid"
                },
                Id = "Test Id",
                Type = "AccountUpdated"
            };

            lstEvents.Add(cdcEvent);


            var data = new CdcAccountFeed()
            {
                Events = lstEvents

            };
               
            // Act
            var processor = new CdcAccountFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _rawFeedProcessor.Object);

            await processor.ProcessEvent(cdcEvent, client.Object);

            // Assert
            //_rawFeedProcessor.Verify(mock => mock.TrackChange(It.IsAny<ParticipantPayload>(),
            //    It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async void CdcAccountsFeed_ShouldCall_SetRawStatus()
        {
            // Setup
            _configurationMock.Setup(x => x.GetValue(Functions.Cdc.AppSettingsKey.CdcApiKeysToProcessFeeds))
                .Returns(() => "ApiKey");

            var lstEvents = new List<CdcEvent>();
            var client = new Mock<IDurableOrchestrationClient>();
            CdcEvent cdcEvent = new CdcEvent()
            {
                ApiKey = "Test",
                CallId = "Test",
                Data = new CdcEventData()
                {
                    AccountType = "Test Account",
                    UId = "Test Uid"
                },
                Id = "Test Id",
                Type = "AccountUpdated"
            };

            lstEvents.Add(cdcEvent);


            var data = new CdcAccountFeed()
            {
                Events = lstEvents

            };

            _rawFeedProcessor.Setup(db =>
                    db.SetRawFeedStatusToSuccess(It.IsAny<long>()))
                .ReturnsAsync(1);

            // Act
            var processor = new CdcAccountFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _rawFeedProcessor.Object);

            await processor.ProcessEvent(cdcEvent, client.Object);

            // Assert
            _rawFeedProcessor.Verify(mock => mock.SetRawFeedStatusToSuccess(It.IsAny<long>()), Times.Once);
        }

        [Fact]
        public async void CdcAccountsFeed_ProcessEvent_ShouldThrowException_EventTypeInvalid()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(Functions.Cdc.AppSettingsKey.CdcApiKeysToProcessFeeds))
                .Returns(() => "ApiKey");

            var lstEvents = new List<CdcEvent>();
            var client = new Mock<IDurableOrchestrationClient>();
            CdcEvent cdcEvent = new CdcEvent()
            {
                ApiKey = "ApiKey",
                CallId = "Test",
                Data = new CdcEventData()
                {
                    AccountType = "Test Account",
                    UId = "Test Uid"
                },
                Id = "Test Id",
                Type = "Test"
            };

            lstEvents.Add(cdcEvent);


            var data = new CdcAccountFeed()
            {
                Events = lstEvents

            };

            // Act
            var processor = new CdcAccountFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _rawFeedProcessor.Object);


            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessEvent(cdcEvent, client.Object));
        }

        [Fact]
        public async void CdcAccountsFeed_ProcessEvent_ShouldThrowException_EventTypeNull()
        {
            // Setup

            _configurationMock.Setup(x => x.GetValue(Functions.Cdc.AppSettingsKey.CdcApiKeysToProcessFeeds))
                .Returns(() => "ApiKey");

            var lstEvents = new List<CdcEvent>();
            var client = new Mock<IDurableOrchestrationClient>();
            CdcEvent cdcEvent = new CdcEvent()
            {
                ApiKey = "ApiKey",
                CallId = "Test",
                Data = new CdcEventData()
                {
                    AccountType = "Test Account",
                    UId = "Test Uid"
                },
                Id = "Test Id"
            };

            lstEvents.Add(cdcEvent);


            var data = new CdcAccountFeed()
            {
                Events = lstEvents

            };

            // Act
            var processor = new CdcAccountFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _rawFeedProcessor.Object);


            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessEvent(cdcEvent, client.Object));
        }

        [Fact]
        public async void CdcAccountsFeed_ProcessEvent_ShouldThrowException_EventIdNull()
        {
            // Setup
            _configurationMock.Setup(x => x.GetValue(Functions.Cdc.AppSettingsKey.CdcApiKeysToProcessFeeds))
                .Returns(() => "ApiKey");

            var lstEvents = new List<CdcEvent>();
            var client = new Mock<IDurableOrchestrationClient>();
            CdcEvent cdcEvent = new CdcEvent()
            {
                ApiKey = "ApiKey",
                CallId = "Test",
                Data = new CdcEventData()
                {
                    AccountType = "Test Account",
                    UId = "Test Uid"
                },
                Type = "Test Type"
            };

            lstEvents.Add(cdcEvent);


            var data = new CdcAccountFeed()
            {
                Events = lstEvents

            };

            // Act
            var processor = new CdcAccountFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _rawFeedProcessor.Object);


            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessEvent(cdcEvent, client.Object));
        }

        [Fact]
        public async void CdcAccountsFeed_ProcessEvent_ShouldThrowException_EventApiKeyNull()
        {
            // Setup
            _configurationMock.Setup(x => x.GetValue(Functions.Cdc.AppSettingsKey.CdcApiKeysToProcessFeeds))
                .Returns(() => "ApiKey");

            var lstEvents = new List<CdcEvent>();
            var client = new Mock<IDurableOrchestrationClient>();
            CdcEvent cdcEvent = new CdcEvent()
            {
                Type = "Type",
                CallId = "Test",
                Data = new CdcEventData()
                {
                    AccountType = "Test Account",
                    UId = "Test Uid"
                },
                Id = "Test Id"
            };

            lstEvents.Add(cdcEvent);


            var data = new CdcAccountFeed()
            {
                Events = lstEvents

            };

            // Act
            var processor = new CdcAccountFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _rawFeedProcessor.Object);


            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessEvent(cdcEvent, client.Object));
        }

        [Fact]
        public async void CdcAccountsFeed_ProcessEvent_ShouldThrowException_UidNull()
        {
            // Setup
            _configurationMock.Setup(x => x.GetValue(Functions.Cdc.AppSettingsKey.CdcApiKeysToProcessFeeds))
                .Returns(() => "ApiKey");

            var lstEvents = new List<CdcEvent>();
            var client = new Mock<IDurableOrchestrationClient>();
            CdcEvent cdcEvent = new CdcEvent()
            {
                ApiKey = "ApiKey",
                Type = "Test Type",
                Data = new CdcEventData()
                {
                    AccountType = "Test Account"
                },
                Id = "Test Id"
            };

            lstEvents.Add(cdcEvent);


            var data = new CdcAccountFeed()
            {
                Events = lstEvents

            };

            // Act
            var processor = new CdcAccountFeedProcessor(_telemetryHandlerMock.Object, _configurationMock.Object, _rawFeedProcessor.Object);


            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => processor.ProcessEvent(cdcEvent, client.Object));
        }

    }
}
