using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Common.PublishEvents.ChangeTrack;
using Grassroots.Identity.Functions.Cdc.Cdc;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Grassroots.Identity.Functions.Cdc.Common.EventGridPublish;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Grassroots.Identity.Functions.Cdc.Test.Cdc
{
    public class InsiderFeedActivityFunctionTests
    {
        [Fact]
        public async void InsiderFeedActivityFunctionTests_Return_Success()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _eventGridPublish = new Mock<IEventGridPublish>();
            var _configuration = new Mock<IConfigProvider>();

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                Uid = "TestUid"
            };

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new InsiderFeedActivityFunction(_telemetry.Object, _eventGridPublish.Object, _configuration.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1, result);
        }

        [Fact]
        public async void InsiderFeedActivityFunctionTests_Return_Failure()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _eventGridPublish = new Mock<IEventGridPublish>();
            var _configuration = new Mock<IConfigProvider>();

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest();

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new InsiderFeedActivityFunction(_telemetry.Object, _eventGridPublish.Object, _configuration.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(0, result);
        }


    }
}