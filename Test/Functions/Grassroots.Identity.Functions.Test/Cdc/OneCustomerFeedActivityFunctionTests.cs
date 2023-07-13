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
    public class OneCustomerFeedActivityFunctionTests
    {
        [Fact]
        public async void OneCustomerFeedActivityFunctionTests_Return_Success()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _changeTrack = new Mock<IEventGridPublish>();
            var _config = new Mock<IConfigProvider>();

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                Uid = "TestUid",
                CdcApiKey = "TestApiKeyDiff"
            };

            _config.Setup(x => x.GetValue(AppSettingsKey.OneCustomerCdcApiKey))
                .Returns(() => "TestApiKey");

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new OneCustomerFeedActivityFunction(_telemetry.Object, _changeTrack.Object, _config.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(1, result);
        }

        [Fact]
        public async void OneCustomerFeedActivityFunctionTests_Return_Failure()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _changeTrack = new Mock<IEventGridPublish>();
            var _config = new Mock<IConfigProvider>();

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                Uid = "TestUid",
                CdcApiKey = "TestApiKey"
            };

            _config.Setup(x => x.GetValue(AppSettingsKey.OneCustomerCdcApiKey))
                .Returns(() => "TestApiKey");

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new OneCustomerFeedActivityFunction(_telemetry.Object, _changeTrack.Object, _config.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(0, result);
        }

        [Fact]
        public async void OneCustomerFeedActivityFunctionTests_Return_Failure_When_CdcApiKey_Not_Passed()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _changeTrack = new Mock<IEventGridPublish>();
            var _config = new Mock<IConfigProvider>();

            FeedActivityFunctionRequest request = new FeedActivityFunctionRequest()
            {
                Uid = "TestUid"
            };

            _config.Setup(x => x.GetValue(AppSettingsKey.OneCustomerCdcApiKey))
                .Returns(() => "TestApiKey");

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<FeedActivityFunctionRequest>()).Returns(request);
            var function = new OneCustomerFeedActivityFunction(_telemetry.Object, _changeTrack.Object, _config.Object);

            var result = await function.Run(durableActivityContextMock.Object);
            Assert.Equal(0, result);
        }


    }
}