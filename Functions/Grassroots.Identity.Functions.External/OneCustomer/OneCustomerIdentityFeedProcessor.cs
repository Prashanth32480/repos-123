using Grassroots.Common.Helpers.Configuration;
using Grassroots.Common.Helpers.Telemetry;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Grassroots.Identity.Functions.External.Common.Model;

namespace Grassroots.Identity.Functions.External.OneCustomer
{
    public class OneCustomerIdentityFeedProcessor : IOneCustomerIdentityFeedProcessor
    {
        private readonly ITelemetryHandler _telemetryHandler;
        private readonly IConfigProvider _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public OneCustomerIdentityFeedProcessor(ITelemetryHandler telemetryHandler
            , IConfigProvider configuration
            , IHttpClientFactory httpClientFactory)
        {
            _telemetryHandler = telemetryHandler;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task ProcessEvent(IdentityExternalFeedRequest data)
        {
            ValidateOneCustomerFeed(data);

            
        }

        private void ValidateOneCustomerFeed(IdentityExternalFeedRequest data)
        {
            if (string.IsNullOrWhiteSpace(data.Uid))
            {
                var error = $"Required Value missing for Identity OneCustomer Feed (Uid)";
                throw new ApplicationException(error);
            }
        }
        
    }
}
