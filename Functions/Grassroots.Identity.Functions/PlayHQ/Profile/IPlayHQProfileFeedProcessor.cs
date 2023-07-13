using System;
using System.Threading.Tasks;
using Grassroots.Identity.Functions.Common.Models;
using Grassroots.Identity.Functions.PlayHQ.Profile.Models;

namespace Grassroots.Identity.Functions.PlayHQ.Profile
{
    public interface IPlayHQProfileFeedProcessor
    {
        /// <summary>
        /// Processes and Stores PlayHQId to SAP CDC from EventGrid event.
        /// </summary>
        /// <param name="eventGridEvent"></param>
        /// <returns></returns>
        Task<int> ProcessFeed(PlayHQProfileData playHQFeed, long rawFeedId);

        Task<bool> ProcessClaimFeed(PlayHQClaimData playHQClaimFeed, long rawFeedId);
    }
}
