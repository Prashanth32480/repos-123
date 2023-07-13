using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.External.Insider.Models
{
    public  class InsiderRequestUser
    {

        [JsonProperty(PropertyName = "identifiers", NullValueHandling = NullValueHandling.Ignore)]
        public InsiderRequestIdentifiers Identifiers { get; set; }

        [JsonProperty(PropertyName = "attributes", NullValueHandling = NullValueHandling.Ignore)]
        public InsiderRequestAttributes Attributes { get; set; }

        [JsonProperty(PropertyName = "not_append")]
        public bool NotAppend { get; set; } = true;
    }
}
