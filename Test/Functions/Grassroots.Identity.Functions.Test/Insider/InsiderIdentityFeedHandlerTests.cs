using System;
using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Identity.Functions.External.Common;
using Grassroots.Identity.Functions.Common.Models;
using Grassroots.Identity.Functions.External.Insider;
using Grassroots.Identity.Functions.External.Insider.Models;
using Microsoft.Azure.EventGrid.Models;
using Moq;
using Xunit;
using Grassroots.Identity.Functions.External.Common.Model;

namespace Grassroots.Identity.Functions.Test.Insider
{
    public class InsiderIdentityFeedHandlerTests
    {
        #region Variables and Constructor
        private readonly Mock<IFeedEventProcessor> _feedEventProcessorMock = new Mock<IFeedEventProcessor>();

        public InsiderIdentityFeedHandlerTests()
        {
            _feedEventProcessorMock.Setup(x => x.ShouldFeedBeProcessed(It.IsAny<string>(), It.IsAny<FeedType>(),
                    It.IsAny<SourceSystem>(), It.IsAny<Guid>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);
            _feedEventProcessorMock.Setup(x => x.UpdateEventRaisedDateTime(It.IsAny<Guid>()));
        }
        #endregion

        [Fact]
        public async void IdentityInsider_HandleFeed_ShouldCall_ProcessEvent()
        {
            // Setup
            var feedProcessor = new Mock<IInsiderIdentityFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);

            feedProcessor.Setup(db =>
                db.ProcessEvent(It.IsAny<IdentityExternalFeedRequest>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new IdentityExternalFeedRequest()
                {
                    Uid = "Test"
                }),
                EventType = "ONECUSTOMER.ACCOUNT.UPDATED",
                Id = Guid.NewGuid().ToString()
            };

            // Act
            var service = new InsiderIdentityFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            await service.HandleFeed(eventGridEvent);

            // Assert
            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce());

            feedProcessor.Verify(
                x => x.ProcessEvent(It.IsAny<IdentityExternalFeedRequest>()),
                Times.AtLeastOnce());

            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void IdentityInsider_HandleFeed_ShouldNotCall_ProcessEvent_InvalidMessageIdThrows()
        {
            // Setup
            var feedProcessor = new Mock<IInsiderIdentityFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);

            feedProcessor.Setup(db =>
                db.ProcessEvent(It.IsAny<IdentityExternalFeedRequest>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new IdentityExternalFeedRequest()
                {
                    Uid = "Test"
                }),
                EventType = "ONECUSTOMER.ACCOUNT.UPDATED"
            };

            // Act
            var service = new InsiderIdentityFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            // Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => service.HandleFeed(eventGridEvent));

            Assert.StartsWith("Received invalid or empty identity insider Feed.", exception.Message);

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Never);
            feedProcessor.Verify(
                x => x.ProcessEvent(It.IsAny<IdentityExternalFeedRequest>()),
                Times.Never());

            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);

        }

        [Fact]
        public async void IdentityInsider_HandleFeed_ShouldNotCall_ProcessEvent_MessageIdExistsInRawFeed()
        {
            // Setup
            var feedProcessor = new Mock<IInsiderIdentityFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db => db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(1);
            rawFeed.Setup(db => db.CheckRawFeedExists(It.IsAny<string>())).ReturnsAsync(true);
            feedProcessor.Setup(db => db.ProcessEvent(It.IsAny<IdentityExternalFeedRequest>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new IdentityExternalFeedRequest()
                {
                    Uid = "Test"
                }),
                EventType = "ONECUSTOMER.ACCOUNT.UPDATED",
                Id = Guid.NewGuid().ToString()
            };

            // Act
            var service = new InsiderIdentityFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => service.HandleFeed(eventGridEvent));

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Never());

            feedProcessor.Verify(
                x => x.ProcessEvent(It.IsAny<IdentityExternalFeedRequest>()),
                Times.Never());

            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never());
        }

        [Fact]
        public async void IdentityInsider_HandleFeed_ShouldNotCall_ProcessEvent_UpdateRawFeed()
        {
            // Setup
            var feedProcessor = new Mock<IInsiderIdentityFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.ProcessEvent(It.IsAny<IdentityExternalFeedRequest>())).Throws(new ApplicationException());

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new IdentityExternalFeedRequest()
                {
                    Uid = "Test"
                }),
                EventType = "ONECUSTOMER.ACCOUNT.UPDATED",
                Id = Guid.NewGuid().ToString()
            };

            // Act
            var service = new InsiderIdentityFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            await Assert.ThrowsAsync<ApplicationException>(() => service.HandleFeed(eventGridEvent));

            // Assert
            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce());
            feedProcessor.Verify(
                x => x.ProcessEvent(It.IsAny<IdentityExternalFeedRequest>()),
                Times.AtLeastOnce());
            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);
        }
    }
}
