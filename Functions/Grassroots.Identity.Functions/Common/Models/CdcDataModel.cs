using System.Collections.Generic;
using Newtonsoft.Json;

namespace Grassroots.Identity.Functions.Common.Models
{
    public class CdcDataModel
    {
        [JsonProperty(PropertyName = "playHQId", NullValueHandling = NullValueHandling.Ignore)]
        public string PlayHQId { get; set; }

        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public CdcIdFields Id { get; set; }

        [JsonProperty(PropertyName = "child", NullValueHandling = NullValueHandling.Ignore)]
        public List<CdcChild> ChildArray { get; set; }
    }

    public class CdcGetAccountInfoResponse
    {
        public string CallId { get; set; }
        public int StatusCode { get; set; }
        public string StatusReason { get; set; }
        public string UID { get; set; }
        public CdcDataModel Data { get; set; }
        public CdcGetAccountInfoProfile Profile { get; set; }
        public string ErrorDetails { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }

    }

    public class CdcGetAccountInfoProfile
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }
    }
}
