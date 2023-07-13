using Grassroots.Common.Helpers;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Functions.Cdc.Cdc;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Grassroots.Identity.Functions.Cdc.Cdc.Common.Model;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using System.Collections.Generic;
using Grassroots.Common.Helpers.Configuration;
using Xunit;

namespace Grassroots.Identity.Functions.Cdc.Test.Cdc
{
    public class CdcIdentityFeedFunctionTests
    {

        [Fact]
        public async void CdcIdentityFeedHandlerMain_ShouldCall_ProcessPlayHqProfileFeed()
        {
            // Setup
            var cdcAccountFeedHandler = new Mock<ICdcAccountFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();
            var client = new Mock<IDurableOrchestrationClient>();
            var config = new Mock<IConfigProvider>();

            cdcAccountFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>(), It.IsAny<IDurableOrchestrationClient>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "Identity.Account.BatchedEvents",
                Data = new EventGridEvent
                {
                    EventType = "Identity.Account.BatchedEvents",
                    Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<CdcAccountFeed>())
                }
            };

            // Act
            var function = new CdcIdentityFeedFunction(telemetry.Object, cdcAccountFeedHandler.Object, config.Object);

            await function.Run(eventGridEvent, client.Object);

            // Assert
            cdcAccountFeedHandler.Verify(
                x => x.HandleFeed(It.IsAny<EventGridEvent>(), It.IsAny<IDurableOrchestrationClient>()),
            Times.AtLeastOnce());
        }


        [Fact]
        public async void CdcIdentityFeedHandlerMain_LogTraceEvent_InvalidEventType()
        {
            // Setup
            var cdcAccountFeedHandler = new Mock<ICdcAccountFeedHandler>();
            var telemetry = new Mock<ITelemetryHandler>();
            var client = new Mock<IDurableOrchestrationClient>();
            var config = new Mock<IConfigProvider>();

            cdcAccountFeedHandler.Setup(x =>
                x.HandleFeed(It.IsAny<EventGridEvent>(), It.IsAny<IDurableOrchestrationClient>()));

            var eventGridEvent = new EventGridEvent
            {
                EventType = "Invalid",
                Data = new EventGridEvent
                {
                    EventType = "Invalid",
                    Data = JsonHelper.SerializeJsonObject(new PlayHQFeed<CdcAccountFeed>())
                }
            };
            

            // Act
            var function = new CdcIdentityFeedFunction(telemetry.Object, cdcAccountFeedHandler.Object, config.Object);

            await function.Run(eventGridEvent, client.Object);

            // Assert
            telemetry.Verify(x => x.TrackEvent(It.Is<string>(y => y.StartsWith("Unexpected Event Type")),
                It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()));
        }
    }
}