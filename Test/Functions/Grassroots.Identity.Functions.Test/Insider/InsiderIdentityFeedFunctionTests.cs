using System.Collections.Generic;
using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Functions.External.Common.Model;
using Grassroots.Identity.Functions.External.Insider;
using Microsoft.Azure.EventGrid.Models;
using Moq;
using Xunit;

namespace Grassroots.Identity.Functions.Test.Insider
{
    public class InsiderIdentityFeedFunctionTests
    {
        [Fact]
        public async void InsiderIdentityFeedHandlerMain_ShouldCall_ProcessEvent()
        {
            // Setup
            var insiderFeedHandler = new Mock<IInsiderIdentityFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();

            insiderFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "INSIDER.ACCOUNT.UPDATED",
                Data = new EventGridEvent
                {
                    EventType = "INSIDER.ACCOUNT.UPDATED",
                    Data = JsonHelper.SerializeJsonObject(new IdentityExternalFeedRequest())
                }
            };

            // Act
            var function = new InsiderIdentityFeedFunction(telemetry.Object,insiderFeedHandler.Object);

            await function.Run(eventGridEvent);

            // Assert
            insiderFeedHandler.Verify(
                x => x.HandleFeed(It.IsAny<EventGridEvent>()),
                Times.AtLeastOnce());
        }

       
       
       [Fact]
        public async void PlayHQCompetitionFeedHandlerMain_LogTraceEvent_InvalidEventType()
        {
            // Setup
            var insiderFeedHandler = new Mock<IInsiderIdentityFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();

            insiderFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "UNKNOWN",
                Data = new EventGridEvent
                {
                    EventType = "UNKNOWN",
                    Data = JsonHelper.SerializeJsonObject(new IdentityExternalFeedRequest())
                }
            };

            // Act
            var function = new InsiderIdentityFeedFunction(telemetry.Object, insiderFeedHandler.Object);

            await function.Run(eventGridEvent);

            // Assert
            telemetry.Verify(x => x.TrackEvent(It.Is<string>(y => y.StartsWith("Unexpected Event Type")),
                It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()));
        }
    }
}
