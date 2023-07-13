using System;
using System.Collections.Generic;
using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts;
using Grassroots.Identity.Functions.Cdc.Common;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;
using Grassroots.Identity.Functions.Cdc.Cdc.Common.Model;

namespace Grassroots.Identity.Functions.Test.Cdc.Accounts
{
    public class CdcAccountFeedHandlerTests
    {
        #region Variables and Constructor
        private readonly Mock<IFeedEventProcessor> _feedEventProcessorMock = new Mock<IFeedEventProcessor>();

        public CdcAccountFeedHandlerTests()
        {
            _feedEventProcessorMock.Setup(x => x.ShouldFeedBeProcessed(It.IsAny<string>(), It.IsAny<FeedType>(),
                    It.IsAny<SourceSystem>(), It.IsAny<Guid>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);
            _feedEventProcessorMock.Setup(x => x.UpdateEventRaisedDateTime(It.IsAny<Guid>()));
        }
        #endregion

        [Fact]
        public async void CdcAccounts_HandleFeed_ShouldCall_ProcessEvent()
        {
            // Setup
            var feedProcessor = new Mock<ICdcAccountFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();
            var client = new Mock<IDurableOrchestrationClient>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);

            feedProcessor.Setup(db =>
                db.ProcessEvent(It.IsAny<CdcEvent>(), It.IsAny<IDurableOrchestrationClient>()));

            var lstEvents = new List<CdcEvent>();
            CdcEvent cdcEvent = new CdcEvent()
            {
                ApiKey = "Test ApiKey",
                CallId = "Test",
                Data = new CdcEventData()
                {
                    AccountType = "Test Account",
                    UId = "Test Uid"
                },
                Id = "Test Id"
            };

            lstEvents.Add(cdcEvent);


            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new CdcAccountFeed()
                {
                    Events = lstEvents
                }),
                EventType = "Identity.Account.BatchedEvents",
                Id = Guid.NewGuid().ToString()
            };

            // Act
            var service = new CdcAccountFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            await service.HandleFeed(eventGridEvent, client.Object);

            // Assert
            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce());

            feedProcessor.Verify(
                x => x.ProcessEvent(It.IsAny<CdcEvent>(), It.IsAny<IDurableOrchestrationClient>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void CdcAccounts_HandleFeed_ShouldNotCall_ProcessEvent_InvalidFeedThrows()
        {
            // Setup
            var feedProcessor = new Mock<ICdcAccountFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();
            var client = new Mock<IDurableOrchestrationClient>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);

            feedProcessor.Setup(db =>
                db.ProcessEvent(It.IsAny<CdcEvent>(), It.IsAny<IDurableOrchestrationClient>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new CdcAccountFeed()
                {
                    Events = null
                }),
                EventType = "Identity.Account.BatchedEvents",
                Id = Guid.NewGuid().ToString()
            };

            // Act
            var service = new CdcAccountFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            // Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => service.HandleFeed(eventGridEvent, client.Object));

            Assert.StartsWith("Received invalid or empty CDC Accounts Feed.", exception.Message);

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Never());

            feedProcessor.Verify(
                x => x.ProcessEvent(It.IsAny<CdcEvent>(), It.IsAny<IDurableOrchestrationClient>()),
                Times.Never());

            rawFeed.Verify(
               x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
               Times.Never());
        }
    }
}
