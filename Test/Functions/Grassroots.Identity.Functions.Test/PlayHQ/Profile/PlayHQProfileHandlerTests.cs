using System;
using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Identity.Functions.Common;
using Grassroots.Identity.Functions.Common.Models;
using Grassroots.Identity.Functions.PlayHQ.Profile;
using Grassroots.Identity.Functions.PlayHQ.Profile.Models;
using Microsoft.Azure.EventGrid.Models;
using Moq;
using Xunit;

namespace Grassroots.Identity.Functions.Test.PlayHQ.Profile
{
    public class PlayHQProfileHandlerTests
    {
        #region Variables and Constructor
        private readonly Mock<IFeedEventProcessor> _feedEventProcessorMock = new Mock<IFeedEventProcessor>();

        public PlayHQProfileHandlerTests()
        {
            _feedEventProcessorMock.Setup(x => x.ShouldFeedBeProcessed(It.IsAny<string>(), It.IsAny<FeedType>(),
                    It.IsAny<SourceSystem>(), It.IsAny<Guid>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);
            _feedEventProcessorMock.Setup(x => x.UpdateEventRaisedDateTime(It.IsAny<Guid>()));
        }
        #endregion

        [Fact]
        public async void PlayHQProfile_HandleFeed_ShouldCall_ProcessFeed()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQProfileFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.ProcessFeed(It.IsAny<PlayHQProfileData>(), It.IsAny<long>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQProfileData>()
                {
                    MessageId = Guid.NewGuid(),
                    EventType = "PROFILE.UPDATED",
                    Data = new PlayHQProfileData()
                    {
                        Id = Guid.NewGuid().ToString(),                       
                        AccountHolderExternalAccountId = "TEST",
                        AccountHolderProfileId = "TEST"                       
                    }
                }),
                EventType = "PROFILE.UPDATED"
            };

            // Act
            var service = new PlayHQProfileFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            await service.HandleFeed(eventGridEvent);

            // Assert
            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce());
            feedProcessor.Verify(
                x => x.ProcessFeed(It.IsAny<PlayHQProfileData>(), It.IsAny<long>()),
                Times.AtLeastOnce());
            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQProfile_HandleClaimFeed_ShouldCall_ProcessFeed()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQProfileFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.ProcessClaimFeed(It.IsAny<PlayHQClaimData>(), It.IsAny<long>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQClaimData>()
                {
                    MessageId = Guid.NewGuid(),
                    EventType = "PROFILE.CLAIMED",
                    Data = new PlayHQClaimData()
                    {
                        ClaimedProfileId = Guid.NewGuid().ToString(),
                        DestinationProfileId = Guid.NewGuid().ToString(),
                        Deleted = false
                    }
                }),
                EventType = "PROFILE.CLAIMED"
            };

            // Act
            var service = new PlayHQProfileFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            await service.HandleClaimFeed(eventGridEvent);

            // Assert
            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce());
            feedProcessor.Verify(
                x => x.ProcessClaimFeed(It.IsAny<PlayHQClaimData>(), It.IsAny<long>()),
                Times.AtLeastOnce());
            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQProfile_HandleFeed_Throws_UnknownEventType()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQProfileFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            
            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQProfileData>()
                {
                    MessageId = Guid.NewGuid(),
                    EventType = "PROFILE.UNKNOWN",
                    Data = new PlayHQProfileData()
                    {
                        Id = Guid.NewGuid().ToString(), 
                        AccountHolderExternalAccountId = "TEST",
                        AccountHolderProfileId = "TEST"
                    }
                }),
                EventType = "PROFILE.UNKNOWN"
            };

            // Act
            var service = new PlayHQProfileFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            // Assert
            await service.HandleFeed(eventGridEvent);

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce);
            feedProcessor.Verify(
                x => x.ProcessFeed(It.IsAny<PlayHQProfileData>(), It.IsAny<long>()),
                Times.Never());
            

            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);

        }

        [Fact]
        public async void PlayHQProfile_HandleClaimFeed_Throws_UnknownEventType()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQProfileFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQProfileData>()
                {
                    MessageId = Guid.NewGuid(),
                    EventType = "PROFILE.UNKNOWN",
                    Data = new PlayHQProfileData()
                    {
                        Id = Guid.NewGuid().ToString(),
                        AccountHolderExternalAccountId = "TEST",
                        AccountHolderProfileId = "TEST"
                    }
                }),
                EventType = "PROFILE.UNKNOWN"
            };

            // Act
            var service = new PlayHQProfileFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            // Assert
            await service.HandleClaimFeed(eventGridEvent);

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce);
            feedProcessor.Verify(
                x => x.ProcessFeed(It.IsAny<PlayHQProfileData>(), It.IsAny<long>()),
                Times.Never());


            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);

        }

        [Fact]
        public async void PlayHQProfile_HandleFeed_ShouldNotCall_ProcessEvent_InvalidMessageIdThrows()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQProfileFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>(); 
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.ProcessFeed(It.IsAny<PlayHQProfileData>(), It.IsAny<long>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>() { EventType = "PROFILE.UPDATED" })
            };

            // Act
            var service = new PlayHQProfileFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            // Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => service.HandleFeed(eventGridEvent));

            Assert.StartsWith("Received invalid or empty PlayHQ Profile Feed.", exception.Message);

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Never);
            feedProcessor.Verify(
                x => x.ProcessFeed(It.IsAny<PlayHQProfileData>(), It.IsAny<long>()),
                Times.Never());

            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);

        }

        [Fact]
        public async void PlayHQProfile_HandleClaimFeed_ShouldNotCall_ProcessEvent_InvalidMessageIdThrows()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQProfileFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.ProcessClaimFeed(It.IsAny<PlayHQClaimData>(), It.IsAny<long>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQClaimData>()
                {
                    EventType = "PROFILE.CLAIMED",
                    Data = new PlayHQClaimData()
                    {
                        ClaimedProfileId = Guid.NewGuid().ToString(),
                        DestinationProfileId = Guid.NewGuid().ToString(),
                        Deleted = false
                    }
                }),
                EventType = "PROFILE.CLAIMED"
            };

            // Act
            var service = new PlayHQProfileFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            // Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => service.HandleClaimFeed(eventGridEvent));

            Assert.StartsWith("Received invalid or empty PlayHQ Profile Claim Feed.", exception.Message);

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Never);
            feedProcessor.Verify(
                x => x.ProcessClaimFeed(It.IsAny<PlayHQClaimData>(), It.IsAny<long>()),
                Times.Never());
            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async void PlayHQProfile_HandleFeed_ShouldNotCall_ProcessEvent_MessageIdExistsInRawFeed()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQProfileFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db => db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(1);
            rawFeed.Setup(db => db.CheckRawFeedExists(It.IsAny<string>())).ReturnsAsync(true);
            feedProcessor.Setup(db => db.ProcessFeed(It.IsAny<PlayHQProfileData>(), It.IsAny<long>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>() { MessageId = Guid.NewGuid(), EventType = "PROFILE.UPDATED" })
            };

            // Act
            var service = new PlayHQProfileFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => service.HandleFeed(eventGridEvent));

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Never());
            feedProcessor.Verify(
                x => x.ProcessFeed(It.IsAny<PlayHQProfileData>(), It.IsAny<long>()),
                Times.Never());

            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never());
        }

        [Fact]
        public async void PlayHQProfile_HandleFeed_ShouldNotCall_ProcessEvent_UpdateRawFeed()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQProfileFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.ProcessFeed(It.IsAny<PlayHQProfileData>(), It.IsAny<long>())).Throws(new ApplicationException());

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQProfileData>()
                {
                    MessageId = Guid.NewGuid(),
                    EventType = "PROFILE.UPDATED",
                    Data = new PlayHQProfileData()
                    { Id = Guid.NewGuid().ToString(),
                        AccountHolderExternalAccountId = "TEST",
                        AccountHolderProfileId = "TEST"
                    }
                }),
                EventType = "PROFILE.UPDATED"
            };

            // Act
            var service = new PlayHQProfileFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            await Assert.ThrowsAsync<ApplicationException>(() => service.HandleFeed(eventGridEvent));

            // Assert
            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce());
            feedProcessor.Verify(
                x => x.ProcessFeed(It.IsAny<PlayHQProfileData>(), It.IsAny<long>()),
                Times.AtLeastOnce());
            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async void PlayHQProfile_HandleClaimFeed_ShouldNotCall_ProcessClaimEvent_UpdateRawFeed()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQProfileFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.ProcessClaimFeed(It.IsAny<PlayHQClaimData>(), It.IsAny<long>())).Throws(new ApplicationException());

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQProfileData>()
                {
                    MessageId = Guid.NewGuid(),
                    EventType = "PROFILE.CLAIMED",
                    Data = new PlayHQProfileData()
                    {
                        Id = Guid.NewGuid().ToString(),
                        AccountHolderExternalAccountId = "TEST",
                        AccountHolderProfileId = "TEST"
                    }
                }),
                EventType = "PROFILE.CLAIMED"
            };

            // Act
            var service = new PlayHQProfileFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            await Assert.ThrowsAsync<ApplicationException>(() => service.HandleClaimFeed(eventGridEvent));

            // Assert
            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce());
            feedProcessor.Verify(
                x => x.ProcessClaimFeed(It.IsAny<PlayHQClaimData>(), It.IsAny<long>()),
                Times.AtLeastOnce());
            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public void PlayHQProfile_HandleClaimFeed_ShouldNotCall_ProcessEvent_InvalidClaimFeed()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQProfileFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.ProcessClaimFeed(It.IsAny<PlayHQClaimData>(), It.IsAny<long>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = null
            };

            // Act
            var service = new PlayHQProfileFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            var exception = service.HandleClaimFeed(eventGridEvent);
            // Assert
            Assert.StartsWith("One or more errors occurred.", exception.Exception.Message);

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Never);
            feedProcessor.Verify(
                x => x.ProcessClaimFeed(It.IsAny<PlayHQClaimData>(), It.IsAny<long>()),
                Times.Never());
            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);
        }
    }
}
