using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.Common.Models
{
    public class CdcIdFields
    {
        [JsonProperty(PropertyName = "participant", NullValueHandling = NullValueHandling.Ignore)]
        public string Participant { get; set; }

        [JsonProperty(PropertyName = "myCricket", NullValueHandling = NullValueHandling.Ignore)]
        public string MyCricket { get; set; }

        [JsonProperty(PropertyName = "playHQ", NullValueHandling = NullValueHandling.Ignore)]
        public string PlayHQ { get; set; }
    }
}
