using Grassroots.Identity.Functions.Common.Models;
using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.PlayHQ.Profile.Models
{
    public class PlayHQProfileData
    {
        [JsonProperty("accountHolderProfileId")]
        public string AccountHolderProfileId { get; set; }

        [JsonProperty("accountHolderExternalAccountId")]
        public string AccountHolderExternalAccountId { get; set; }

        [JsonProperty("participant")]
        public PlayHqParticipant Participant { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("profileVisible")]
        public bool? ProfileVisible { get; set; }

        [JsonProperty("accountHolder")]
        public bool AccountHolder { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }

        [JsonProperty("externalAccountId")]
        public string ExternalAccountId { get; set; }
    }
}
