using System.Collections.Generic;

namespace Grassroots.Identity.Functions.External.Common.Model
{
    public class CdcEmailAccountsResponse
    {
        public List<CdcEmailAccountsResponseResults> Results { get; set; }
        public string CallId { get; set; }
        public int StatusCode { get; set; }
        public string StatusReason { get; set; }
        public string Uid { get; set; }
        public string ErrorDetails { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
        
    }

    public class CdcEmailAccountsResponseResults
    {
        public string Email { get; set; }
        public bool? HasFullAccount { get; set; }
        public bool? HasLiteAccount { get; set; }
        public CdcGetAccountInfoData Data { get; set; }
        public CdcGetAccountInfoProfile Profile { get; set; }
        public CdcGetAccountInfoPreferences Preferences { get; set; }
        public dynamic Subscriptions { get; set; }
    }
}
