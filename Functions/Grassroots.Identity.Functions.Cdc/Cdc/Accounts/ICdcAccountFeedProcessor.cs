using System;
using System.Threading.Tasks;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts
{
    public interface ICdcAccountFeedProcessor
    {
        /// <summary>
        /// Processes SAP CDC feeds
        /// </summary>
        /// <param name="eventGridEvent"></param>
        /// <returns></returns>
        Task ProcessEvent(CdcEvent cdcEvent, IDurableOrchestrationClient starter);
    }
}
