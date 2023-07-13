using System.Collections.Generic;
using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Functions.Common.Models;
using Grassroots.Identity.Functions.PlayHQ;
using Grassroots.Identity.Functions.PlayHQ.Profile;
using Grassroots.Identity.Functions.PlayHQ.Registration;
using Microsoft.Azure.EventGrid.Models;
using Moq;
using Xunit;

namespace Grassroots.Identity.Functions.Test.PlayHQ
{
    public class PlayHQIdentityFeedFunctionTests
    {
        [Fact]
        public async void PlayHQIdentityFeedHandlerMain_ShouldCall_ProcessPlayHqProfileFeed_For_Shared_Program_Registration_Created()
        {
            // Setup
            var playHqProfileFeedHandler = new Mock<IPlayHQProfileFeedHandler>();
            var playHqRegistrationFeedHandler = new Mock<IPlayHQRegistrationFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();
            var config = new Mock<IConfigProvider>();

            playHqProfileFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "SHARED_PROGRAM_REGISTRATION.CREATED",
                Data = new EventGridEvent
                {
                    EventType = "SHARED_PROGRAM_REGISTRATION.CREATED",
                    Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>())
                }
            };

            // Act
            var function = new PlayHQIdentityFeedFunction(telemetry.Object, playHqProfileFeedHandler.Object, playHqRegistrationFeedHandler.Object, config.Object);

            await function.Run(eventGridEvent);

            // Assert
            playHqRegistrationFeedHandler.Verify(
                x => x.HandleFeed(It.IsAny<EventGridEvent>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQIdentityFeedHandlerMain_ShouldCall_ProcessPlayHqProfileFeed_For_Shared_Program_Registration_Updated()
        {
            // Setup
            var playHqProfileFeedHandler = new Mock<IPlayHQProfileFeedHandler>();
            var playHqRegistrationFeedHandler = new Mock<IPlayHQRegistrationFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();
            var config = new Mock<IConfigProvider>();

            playHqProfileFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "SHARED_PROGRAM_REGISTRATION.UPDATED",
                Data = new EventGridEvent
                {
                    EventType = "SHARED_PROGRAM_REGISTRATION.UPDATED",
                    Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>())
                }
            };

            // Act
            var function = new PlayHQIdentityFeedFunction(telemetry.Object, playHqProfileFeedHandler.Object, playHqRegistrationFeedHandler.Object, config.Object);

            await function.Run(eventGridEvent);

            // Assert
            playHqRegistrationFeedHandler.Verify(
                x => x.HandleFeed(It.IsAny<EventGridEvent>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQIdentityFeedHandlerMain_ShouldCall_ProcessPlayHqProfileFeed_For_Shared_Program_Registration_Deleted()
        {
            // Setup
            var playHqProfileFeedHandler = new Mock<IPlayHQProfileFeedHandler>();
            var playHqRegistrationFeedHandler = new Mock<IPlayHQRegistrationFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();
            var config = new Mock<IConfigProvider>();

            playHqProfileFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "SHARED_PROGRAM_REGISTRATION.DELETED",
                Data = new EventGridEvent
                {
                    EventType = "SHARED_PROGRAM_REGISTRATION.DELETED",
                    Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>())
                }
            };

            // Act
            var function = new PlayHQIdentityFeedFunction(telemetry.Object, playHqProfileFeedHandler.Object, playHqRegistrationFeedHandler.Object, config.Object);

            await function.Run(eventGridEvent);

            // Assert
            playHqRegistrationFeedHandler.Verify(
                x => x.HandleFeed(It.IsAny<EventGridEvent>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQIdentityFeedHandlerMain_ShouldCall_ProcessPlayHqProfileFeed_For_Competition_Registraion_To_Season()
        {
            // Setup
            var playHqProfileFeedHandler = new Mock<IPlayHQProfileFeedHandler>();
            var playHqRegistrationFeedHandler = new Mock<IPlayHQRegistrationFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();
            var config = new Mock<IConfigProvider>();

            playHqProfileFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "COMPETITION_REGISTRATION_TO_SEASON.CREATED",
                Data = new EventGridEvent
                {
                    EventType = "COMPETITION_REGISTRATION_TO_SEASON.CREATED",
                    Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>())
                }
            };

            // Act
            var function = new PlayHQIdentityFeedFunction(telemetry.Object, playHqProfileFeedHandler.Object, playHqRegistrationFeedHandler.Object, config.Object);

            await function.Run(eventGridEvent);

            // Assert
            playHqRegistrationFeedHandler.Verify(
                x => x.HandleFeed(It.IsAny<EventGridEvent>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQIdentityFeedHandlerMain_ShouldCall_ProcessPlayHqProfileFeed_For_Competition_Registraion_To_Club()
        {
            // Setup
            var playHqProfileFeedHandler = new Mock<IPlayHQProfileFeedHandler>();
            var playHqRegistrationFeedHandler = new Mock<IPlayHQRegistrationFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();
            var config = new Mock<IConfigProvider>();

            playHqProfileFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "COMPETITION_REGISTRATION_TO_CLUB.CREATED",
                Data = new EventGridEvent
                {
                    EventType = "COMPETITION_REGISTRATION_TO_CLUB.CREATED",
                    Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>())
                }
            };

            // Act
            var function = new PlayHQIdentityFeedFunction(telemetry.Object, playHqProfileFeedHandler.Object, playHqRegistrationFeedHandler.Object, config.Object);

            await function.Run(eventGridEvent);

            // Assert
            playHqRegistrationFeedHandler.Verify(
                x => x.HandleFeed(It.IsAny<EventGridEvent>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQIdentityFeedHandlerMain_ShouldCall_ProcessPlayHqProfileFeed_For_Competition_Registraion_To_Team()
        {
            // Setup
            var playHqProfileFeedHandler = new Mock<IPlayHQProfileFeedHandler>();
            var playHqRegistrationFeedHandler = new Mock<IPlayHQRegistrationFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();
            var config = new Mock<IConfigProvider>();

            playHqProfileFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "COMPETITION_REGISTRATION_TO_TEAM.CREATED",
                Data = new EventGridEvent
                {
                    EventType = "COMPETITION_REGISTRATION_TO_TEAM.CREATED",
                    Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>())
                }
            };

            // Act
            var function = new PlayHQIdentityFeedFunction(telemetry.Object, playHqProfileFeedHandler.Object, playHqRegistrationFeedHandler.Object, config.Object);

            await function.Run(eventGridEvent);

            // Assert
            playHqRegistrationFeedHandler.Verify(
                x => x.HandleFeed(It.IsAny<EventGridEvent>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQIdentityFeedHandlerMain_ShouldCall_ProcessPlayHqProfileFeed_For_Competition_Registraion_To_Club_Team()
        {
            // Setup
            var playHqProfileFeedHandler = new Mock<IPlayHQProfileFeedHandler>();
            var playHqRegistrationFeedHandler = new Mock<IPlayHQRegistrationFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();
            var config = new Mock<IConfigProvider>();

            playHqProfileFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "COMPETITION_REGISTRATION_TO_CLUB_TEAM.CREATED",
                Data = new EventGridEvent
                {
                    EventType = "COMPETITION_REGISTRATION_TO_CLUB_TEAM.CREATED",
                    Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>())
                }
            };

            // Act
            var function = new PlayHQIdentityFeedFunction(telemetry.Object, playHqProfileFeedHandler.Object, playHqRegistrationFeedHandler.Object, config.Object);

            await function.Run(eventGridEvent);

            // Assert
            playHqRegistrationFeedHandler.Verify(
                x => x.HandleFeed(It.IsAny<EventGridEvent>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async void PlayHQIdentityFeedHandlerMain_ShouldCall_ProcessPlayHqProfileFeed_For_Profile_Updated()
        {
            // Setup
            var playHqProfileFeedHandler = new Mock<IPlayHQProfileFeedHandler>();
            var playHqRegistrationFeedHandler = new Mock<IPlayHQRegistrationFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();
            var config = new Mock<IConfigProvider>();

            playHqProfileFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "PROFILE.UPDATED",
                Data = new EventGridEvent
                {
                    EventType = "PROFILE.UPDATED",
                    Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>())
                }
            };

            // Act
            var function = new PlayHQIdentityFeedFunction(telemetry.Object, playHqProfileFeedHandler.Object, playHqRegistrationFeedHandler.Object, config.Object);

            await function.Run(eventGridEvent);

            // Assert
            playHqProfileFeedHandler.Verify(
                x => x.HandleFeed(It.IsAny<EventGridEvent>()),
                Times.AtLeastOnce());
        }


        [Fact]
        public async void PlayHQCompetitionFeedHandlerMain_LogTraceEvent_InvalidEventType()
        {
            // Setup
            var playHqProfileFeedHandler = new Mock<IPlayHQProfileFeedHandler>();
            var playHqRegistrationFeedHandler = new Mock<IPlayHQRegistrationFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();
            var config = new Mock<IConfigProvider>();

            playHqProfileFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "Invalid",
                Data = new EventGridEvent
                {
                    EventType = "Invalid",
                    Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<PlayHQData>())
                }
            };

            // Act
            var function = new PlayHQIdentityFeedFunction(telemetry.Object, playHqProfileFeedHandler.Object, playHqRegistrationFeedHandler.Object, config.Object);

            await function.Run(eventGridEvent);

            // Assert
            telemetry.Verify(x => x.TrackEvent(It.Is<string>(y => y.StartsWith("Unexpected Event Type")),
                It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()));
        }
    }
}
