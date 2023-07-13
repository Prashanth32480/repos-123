using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.External.Insider.Models
{
    public  class InsiderRequestAttributes
    {
        [JsonProperty(PropertyName = "custom", NullValueHandling = NullValueHandling.Ignore)]
        public InsiderRequestCustomAttributes Custom { get; set; }

        [JsonProperty(PropertyName = "gender", NullValueHandling = NullValueHandling.Ignore)]
        public string Gender { get; set; }

        [JsonProperty(PropertyName = "email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "surname", NullValueHandling = NullValueHandling.Ignore)]
        public string Surname { get; set; }

        [JsonProperty(PropertyName = "phone_number", NullValueHandling = NullValueHandling.Ignore)]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "gdpr_optin", NullValueHandling = NullValueHandling.Ignore)]
        public bool? GdprOptIn { get; set; }

        [JsonProperty(PropertyName = "email_optin", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EmailOptIn { get; set; }

        [JsonProperty(PropertyName = "birthday", NullValueHandling = NullValueHandling.Ignore)]
        public string BirthDay { get; set; }
    }
}
