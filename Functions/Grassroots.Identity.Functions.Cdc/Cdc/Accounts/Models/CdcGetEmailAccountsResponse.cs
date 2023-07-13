using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class CdcGetEmailAccountsResponse
    {
        public List<CdcGetEmailAccountsResponseResults> Results { get; set; }
        public string CallId { get; set; }
        public string Uid { get; set; }
        public string ErrorDetails { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
    }

    public class CdcGetEmailAccountsResponseResults
    {
        public string Email { get; set; }
        public bool? HasFullAccount { get; set; }
        public bool? HasLiteAccount { get; set; }
        public CdcGetAccountInfoData Data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Created { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastUpdated { get; set; }
    }
}
