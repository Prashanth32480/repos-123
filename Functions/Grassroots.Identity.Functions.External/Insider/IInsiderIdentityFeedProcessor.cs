using System;
using System.Threading.Tasks;
using Grassroots.Identity.Functions.External.Common.Model;

namespace Grassroots.Identity.Functions.External.Insider
{
    public interface IInsiderIdentityFeedProcessor
    {
        /// <summary>
        /// Processes Identity External Insider Feeds
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task ProcessEvent(IdentityExternalFeedRequest data);
    }
}
