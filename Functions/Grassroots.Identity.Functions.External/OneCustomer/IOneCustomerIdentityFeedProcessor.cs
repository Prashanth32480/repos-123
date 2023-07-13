using System;
using System.Threading.Tasks;
using Grassroots.Identity.Functions.External.Common.Model;

namespace Grassroots.Identity.Functions.External.OneCustomer
{
    public interface IOneCustomerIdentityFeedProcessor
    {
        /// <summary>
        /// Processes Identity External OneCustomer Feeds
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task ProcessEvent(IdentityExternalFeedRequest data);
    }
}
