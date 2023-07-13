using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class CdcChild
    {
        [JsonProperty(PropertyName = "childId", NullValueHandling = NullValueHandling.Ignore)]
        public int ChildId { get; set; }

        [JsonProperty(PropertyName = "firstName", NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName", NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public CdcIdFields Id { get; set; }
    }
}
