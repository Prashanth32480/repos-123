using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.External.Insider.Models
{
    public  class InsiderRequestIdentifiers
    {
        [JsonProperty(PropertyName = "email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "uuid", NullValueHandling = NullValueHandling.Ignore)]
        public string Uuid { get; set; }
    }
}
