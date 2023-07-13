using System;
using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.FeatureFlags;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Identity.Functions.Common;
using Grassroots.Identity.Functions.Common.Models;
using Grassroots.Identity.Functions.PlayHQ.Profile;
using Grassroots.Identity.Functions.PlayHQ.Registration;
using Microsoft.Azure.EventGrid.Models;
using Moq;
using Xunit;

namespace Grassroots.Identity.Functions.Test.PlayHQ.Registration
{
    public class PlayHQRegistrationHandlerTests
    {
        #region Variables and Constructor
        private readonly Mock<IFeedEventProcessor> _feedEventProcessorMock = new Mock<IFeedEventProcessor>();

        public PlayHQRegistrationHandlerTests()
        {
            _feedEventProcessorMock.Setup(x => x.ShouldFeedBeProcessed(It.IsAny<string>(), It.IsAny<FeedType>(),
                    It.IsAny<SourceSystem>(), It.IsAny<Guid>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);
            _feedEventProcessorMock.Setup(x => x.UpdateEventRaisedDateTime(It.IsAny<Guid>()));
        }
        #endregion

        [Fact]
        public async void PlayHQProfile_HandleFeed_ShouldCall_CreateParticpant()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQRegistrationFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.CreateParticipant(It.IsAny<PlayHQData>(), FeedType.Competition, It.IsAny<long>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>()
                {
                    MessageId = Guid.NewGuid(),
                    EventType = "COMPETITION_REGISTRATION_TO_SEASON.CREATED",
                    Data = new PlayHQData()
                    {
                        Id = new Guid(),
                        Profile = new PlayHQProfile()
                        {
                            AccountHolderExternalAccountId = "TEST",
                            AccountHolderProfileId = "TEST"
                        }
                    }
                }),
                EventType = "COMPETITION_REGISTRATION_TO_SEASON.CREATED"
            };

            // Act
            var service = new PlayHQRegistrationFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            await service.HandleFeed(eventGridEvent);

            // Assert
            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce());
            feedProcessor.Verify(
                x => x.CreateParticipant(It.IsAny<PlayHQData>(), FeedType.Competition, It.IsAny<long>()),
                Times.AtLeastOnce());
            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQProfile_HandleFeed_ShouldCall_UpdateParticpant()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQRegistrationFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.CreateParticipant(It.IsAny<PlayHQData>(), FeedType.Program, It.IsAny<long>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>()
                {
                    MessageId = Guid.NewGuid(),
                    EventType = "SHARED_PROGRAM_REGISTRATION.UPDATED",
                    Data = new PlayHQData()
                    {
                        Id = new Guid(),
                        Profile = new PlayHQProfile()
                        {
                            AccountHolderExternalAccountId = "TEST",
                            AccountHolderProfileId = "TEST"
                        }
                    }
                }),
                EventType = "SHARED_PROGRAM_REGISTRATION.UPDATED"
            };

            // Act
            var service = new PlayHQRegistrationFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            await service.HandleFeed(eventGridEvent);

            // Assert
            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce());
            feedProcessor.Verify(
                x => x.UpdateParticipant(It.IsAny<PlayHQData>(), FeedType.Program, It.IsAny<long>()),
                Times.AtLeastOnce());
            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQProfile_HandleFeed_ShouldCall_DeleteParticpant()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQRegistrationFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.CreateParticipant(It.IsAny<PlayHQData>(), FeedType.Program, It.IsAny<long>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>()
                {
                    MessageId = Guid.NewGuid(),
                    EventType = "SHARED_PROGRAM_REGISTRATION.DELETED",
                    Data = new PlayHQData()
                    {
                        Id = new Guid(),
                        Profile = new PlayHQProfile()
                        {
                            AccountHolderExternalAccountId = "TEST",
                            AccountHolderProfileId = "TEST"
                        }
                    }
                }),
                EventType = "SHARED_PROGRAM_REGISTRATION.DELETED"
            };

            // Act
            var service = new PlayHQRegistrationFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            await service.HandleFeed(eventGridEvent);

            // Assert
            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce());
            feedProcessor.Verify(
                x => x.DeleteParticipant(It.IsAny<PlayHQData>(), It.IsAny<long>()),
                Times.AtLeastOnce());
            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQProfile_HandleFeed_Throws_UnknownEventType()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQRegistrationFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            
            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>()
                {
                    MessageId = Guid.NewGuid(),
                    EventType = "SHARED_PROGRAM_REGISTRATION.UNKNOWN",
                    Data = new PlayHQData()
                    {
                        Id = new Guid(),
                        Profile = new PlayHQProfile()
                        {
                            AccountHolderExternalAccountId = "TEST",
                            AccountHolderProfileId = "TEST"
                        }
                    }
                }),
                EventType = "SHARED_PROGRAM_REGISTRATION.UNKNOWN"
            };

            // Act
            var service = new PlayHQRegistrationFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            // Assert
            await service.HandleFeed(eventGridEvent);

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce);
            feedProcessor.Verify(
                x => x.CreateParticipant(It.IsAny<PlayHQData>(), FeedType.Competition, It.IsAny<long>()),
                Times.Never());
            feedProcessor.Verify(
                x => x.UpdateParticipant(It.IsAny<PlayHQData>(), FeedType.Competition, It.IsAny<long>()),
                Times.Never());
            feedProcessor.Verify(
                x => x.DeleteParticipant(It.IsAny<PlayHQData>(), It.IsAny<long>()),
                Times.Never());

            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);

        }

        [Fact]
        public async void PlayHQProfile_HandleFeed_ShouldNotCall_ProcessEvent_InvalidMessageIdThrows()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQRegistrationFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>(); 
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.CreateParticipant(It.IsAny<PlayHQData>(), FeedType.Competition, It.IsAny<long>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>() { EventType = "SHARED_PROGRAM_REGISTRATION.CREATED" })
            };

            // Act
            var service = new PlayHQRegistrationFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            // Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => service.HandleFeed(eventGridEvent));

            Assert.StartsWith("Received invalid or empty PlayHQ Registration Feed.", exception.Message);

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Never);
            feedProcessor.Verify(
                x => x.CreateParticipant(It.IsAny<PlayHQData>(), FeedType.Competition, It.IsAny<long>()),
                Times.Never());

            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);

        }

        [Fact]
        public async void PlayHQProfile_HandleFeed_ShouldNotCall_ProcessEvent_MessageIdExistsInRawFeed()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQRegistrationFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db => db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(1);
            rawFeed.Setup(db => db.CheckRawFeedExists(It.IsAny<string>())).ReturnsAsync(true);
            feedProcessor.Setup(db => db.CreateParticipant(It.IsAny<PlayHQData>(), FeedType.Competition, It.IsAny<long>()));

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>() { MessageId = Guid.NewGuid(), EventType = "SHARED_PROGRAM_REGISTRATION.CREATED" })
            };

            // Act
            var service = new PlayHQRegistrationFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            // Assert
            await Assert.ThrowsAsync<ApplicationException>(() => service.HandleFeed(eventGridEvent));

            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Never());
            feedProcessor.Verify(
                x => x.CreateParticipant(It.IsAny<PlayHQData>(), FeedType.Competition, It.IsAny<long>()),
                Times.Never());

            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never());
        }

        [Fact]
        public async void PlayHQProfile_HandleFeed_ShouldNotCall_ProcessEvent_UpdateRawFeed()
        {
            // Setup
            var feedProcessor = new Mock<IPlayHQRegistrationFeedProcessor>();
            var rawFeed = new Mock<IRawFeedProcessor>();
            var telemetry = new Mock<ITelemetryHandler>();
            var featureFlag = new Mock<IFeatureFlag>();

            rawFeed.Setup(db =>
                    db.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);
            feedProcessor.Setup(db =>
                db.CreateParticipant(It.IsAny<PlayHQData>(), FeedType.Program, It.IsAny<long>())).Throws(new ApplicationException());

            var eventGridEvent = new EventGridEvent
            {
                Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>()
                {
                    MessageId = Guid.NewGuid(),
                    EventType = "SHARED_PROGRAM_REGISTRATION.CREATED",
                    Data = new PlayHQData()
                    { Id = new Guid(), Profile= new PlayHQProfile()
                    { 
                        AccountHolderExternalAccountId = "TEST",
                        AccountHolderProfileId = "TEST"
                    } }
                }),
                EventType = "SHARED_PROGRAM_REGISTRATION.CREATED"
            };

            // Act
            var service = new PlayHQRegistrationFeedHandler(feedProcessor.Object, rawFeed.Object, _feedEventProcessorMock.Object, telemetry.Object, featureFlag.Object);

            await Assert.ThrowsAsync<ApplicationException>(() => service.HandleFeed(eventGridEvent));

            // Assert
            rawFeed.Verify(
                x => x.InsertRawFeed(It.IsAny<FeedType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce());
            feedProcessor.Verify(
                x => x.CreateParticipant(It.IsAny<PlayHQData>(), FeedType.Program, It.IsAny<long>()),
                Times.AtLeastOnce());
            rawFeed.Verify(
                x => x.SetRawFeedStatusToSuccess(It.IsAny<long>()),
                Times.Never);
        }
    }
}
