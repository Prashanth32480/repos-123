using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;

namespace Grassroots.Identity.Functions.External.Insider
{
    public interface IInsiderIdentityFeedHandler
    {
        Task HandleFeed(EventGridEvent eventGridEvent);
    }
}