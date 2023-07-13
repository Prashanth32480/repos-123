using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.PlayHQ.Profile.Models
{
    public class PlayHQClaimData
    {
        [JsonProperty("claimedProfileId")]
        public string ClaimedProfileId { get; set; }

        [JsonProperty("destinationProfileId")]
        public string DestinationProfileId { get; set; }

        [JsonProperty("deleted")]
        public bool? Deleted  { get; set; }
    }
}
