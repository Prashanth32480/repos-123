using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Identity.Functions.Cdc.Cdc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Grassroots.Identity.Functions.Cdc.Common;
using Xunit;

namespace Grassroots.Identity.Functions.Cdc.Test.Cdc
{
    public class UpdateFeedStatusActivityFunctionTests
    {
        [Fact]
        public async void UpdateFeedStatusActivityFunction_returns_output()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();

            _rawFeedProcessor.Setup(db =>
                    db.SetRawFeedStatusToSuccess(It.IsAny<long>()))
                .ReturnsAsync(1);

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<long>()).Returns(1);
            var function = new UpdateFeedStatusActivityFunction(_telemetry.Object, _rawFeedProcessor.Object);
            var result = await function.Run(durableActivityContextMock.Object);
            Assert.True(result);
        }

        [Fact]
        public async void UpdateFeedStatusActivityFunction_returns_failure()
        {
            var _telemetry = new Mock<ITelemetryHandler>();
            var _rawFeedProcessor = new Mock<IRawFeedProcessor>();

            _rawFeedProcessor.Setup(db =>
                    db.SetRawFeedStatusToSuccess(It.IsAny<long>()))
                .ReturnsAsync(0);

            var durableActivityContextMock = new Mock<IDurableActivityContext>();
            durableActivityContextMock.Setup(x => x.GetInput<long>()).Returns(1);
            var function = new UpdateFeedStatusActivityFunction(_telemetry.Object, _rawFeedProcessor.Object);
            var result = await function.Run(durableActivityContextMock.Object);
            Assert.True(result);
        }
    }
}