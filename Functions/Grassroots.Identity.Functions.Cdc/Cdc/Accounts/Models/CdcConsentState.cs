using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class CdcConsentState
    {
        [JsonProperty("IsConsentGranted")]
        public bool IsConsentGranted { get; set; }


    }
}
