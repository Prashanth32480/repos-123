using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grassroots.Common.Helpers.Telemetry;
using Grassroots.Common.PublishEvents.Feed;
using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.Cdc.Common.EventGridPublish
{
    public class EventGridPublish : IEventGridPublish
    {
        private const string KeyName = "aeg-sas-key";
        private readonly ITelemetryHandler _telemetryHandler;
        public EventGridPublish(ITelemetryHandler telemetryHandler)
        {
            _telemetryHandler = telemetryHandler;
        }

        public async Task PushMessage<T>(T changedObject, string eventType, string subject, string eventGridEndPoint, string eventGridKey, int publishEventToBeRetrySeconds, int publishEventToBeRetryCount)
            where T : class, new()
        {
            var events = new List<EventMessage<T>>();
            var customEvent = new EventMessage<T>
            {
                Subject = subject,
                EventType = eventType,
                Data = changedObject
            };
            events.Add(customEvent);

            await PushToEventGridAsync(JsonConvert.SerializeObject(events), eventGridEndPoint, eventGridKey, publishEventToBeRetrySeconds, publishEventToBeRetryCount);
        }

        private async Task PushToEventGridAsync(string message, string eventGridEndPoint, string eventGridKey, int publishEventToBeRetrySeconds, int publishEventToBeRetryCount)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add(KeyName, eventGridKey);
            var content = new StringContent(message, Encoding.UTF8, "application/json");
            var attempts = 0;
            do
            {
                attempts++;
                var result = await httpClient.PostAsync(eventGridEndPoint, content);
                if (result.IsSuccessStatusCode)
                {
                    _telemetryHandler.TrackTraceInfo($"Event pushed into event grid successfully, message : {message}");
                    break;
                }
                _telemetryHandler.TrackTraceInfo($"Event pushed into event grid was unsuccessful, message : {result.ReasonPhrase}");
                Task.Delay(publishEventToBeRetrySeconds * 1000).Wait();

            } while (attempts < publishEventToBeRetryCount);
        }
    }
}
