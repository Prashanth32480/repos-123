using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;

namespace Grassroots.Identity.Functions.PlayHQ.Registration
{
    public interface IPlayHQRegistrationFeedHandler
    {
        Task HandleFeed(EventGridEvent eventGridEvent);
    }
}