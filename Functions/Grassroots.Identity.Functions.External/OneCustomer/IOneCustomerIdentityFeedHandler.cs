using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;

namespace Grassroots.Identity.Functions.External.OneCustomer
{
    public interface IOneCustomerIdentityFeedHandler
    {
        Task HandleFeed(EventGridEvent eventGridEvent);
    }
}