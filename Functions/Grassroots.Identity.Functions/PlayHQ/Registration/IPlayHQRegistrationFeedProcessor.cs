using System;
using System.Threading.Tasks;
using Grassroots.Identity.Database.Model.Static;
using Grassroots.Identity.Functions.Common.Models;

namespace Grassroots.Identity.Functions.PlayHQ.Registration
{
    public interface IPlayHQRegistrationFeedProcessor
    {
        /// <summary>
        /// Processes and Stores PlayHQId to SAP CDC from EventGrid event.
        /// </summary>
        /// <param name="eventGridEvent"></param>
        /// <returns></returns>
        Task<Guid> CreateParticipant(PlayHQData playHQFeed, FeedType feedType, long rawFeedId);
        Task<Guid> UpdateParticipant(PlayHQData playHQFeed, FeedType feedType, long rawFeedId);
        Task<Guid> DeleteParticipant(PlayHQData playHQFeed, long rawFeedId);
    }
}
