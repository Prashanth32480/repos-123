using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;

namespace Grassroots.Identity.Functions.PlayHQ.Profile
{
    public interface IPlayHQProfileFeedHandler
    {
        Task HandleFeed(EventGridEvent eventGridEvent);
        Task HandleClaimFeed(EventGridEvent eventGridEvent);
    }
}