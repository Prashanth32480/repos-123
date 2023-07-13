using Newtonsoft.Json;
using System.Collections.Generic;

namespace Grassroots.Identity.Functions.External.Insider.Models
{
    public class InsiderRequest
    {

        [JsonProperty(PropertyName = "users", NullValueHandling = NullValueHandling.Ignore)]
        public List<InsiderRequestUser> Users { get; set; }
    }
}
