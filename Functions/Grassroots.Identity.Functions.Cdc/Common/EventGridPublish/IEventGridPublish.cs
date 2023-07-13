using System.Threading.Tasks;

namespace Grassroots.Identity.Functions.Cdc.Common.EventGridPublish
{
    public interface IEventGridPublish
    {
        Task PushMessage<T>(T changedObject, string eventType, string subject, string eventGridEndPoint
            , string eventGridKey, int publishEventToBeRetrySeconds, int publishEventToBeRetryCount) where T : class, new();


    }
}
