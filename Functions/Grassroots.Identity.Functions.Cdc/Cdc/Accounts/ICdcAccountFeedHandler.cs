using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts
{
    public interface ICdcAccountFeedHandler
    {
        Task HandleFeed(EventGridEvent eventGridEvent, IDurableOrchestrationClient starter);
    }
}