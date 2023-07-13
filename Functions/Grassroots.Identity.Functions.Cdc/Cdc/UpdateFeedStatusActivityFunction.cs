using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Common.PublishEvents.ChangeTrack;
using Grassroots.Identity.API.PayLoadModel.PayLoads;
using Grassroots.Identity.Functions.Cdc.Common;

namespace Grassroots.Identity.Functions.Cdc.Cdc
{
    public class UpdateFeedStatusActivityFunction
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IRawFeedProcessor _rawFeedProcessor;

        public UpdateFeedStatusActivityFunction(ITelemetryHandler telemetryHandler
            , IRawFeedProcessor rawFeedProcessor)
        {
            _telemetryHandler = telemetryHandler;
            _rawFeedProcessor = rawFeedProcessor;
        }

        [FunctionName("UpdateFeedStatusActivityFunction")]
        public async Task<bool> Run([ActivityTrigger]IDurableActivityContext activityContext)
        {
            long feedId = activityContext.GetInput<long>();
            _telemetryHandler.TrackTraceInfo($"Updating Feed Status to success. Feed Id - {feedId}");
            await _rawFeedProcessor.SetRawFeedStatusToSuccess(feedId);
            _telemetryHandler.TrackTraceInfo($"Updated the Feed Status to success. Feed Id - {feedId}");
            return true;
        }
    }
}
