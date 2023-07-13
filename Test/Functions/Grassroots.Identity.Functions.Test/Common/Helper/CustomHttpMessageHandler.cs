using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Grassroots.Identity.Functions.Test.Common.Helper
{
    public class CustomHttpMessageHandler : HttpMessageHandler
    {
        private readonly IDictionary<string, HttpResponseMessage> messages;

        public CustomHttpMessageHandler(IDictionary<string, HttpResponseMessage> messages)
        {
            this.messages = messages;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            if (messages.ContainsKey(request.RequestUri.ToString()))
                response = messages[request.RequestUri.ToString()] ?? new HttpResponseMessage(HttpStatusCode.NoContent);
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }
    
}
