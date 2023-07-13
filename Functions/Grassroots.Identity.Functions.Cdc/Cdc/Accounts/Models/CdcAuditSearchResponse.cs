using System;
using System.Collections.Generic;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class CdcAuditSearchResponse
    {
        public List<CdcAuditSearchResult> Results { get; set; }
        public int TotalCount { get; set; }
        public int StatusCode { get; set; }
        public int ErrorCode { get; set; }
        public string StatusReason { get; set; }
        public string CallId { get; set; }
        public DateTime Time { get; set; }
        public int ObjectsCount { get; set; }
    }

    public class CdcAuditSearchResult
    {
        public string CallID { get; set; }
        public string ErrCode { get; set; }
        public string ErrMessage { get; set; }
        public string Endpoint { get; set; }
        public CdcAuditSearchParams Params { get; set; }
        public string Uid { get; set; }
        public string Apikey { get; set; }
    }

    public class CdcAuditSearchParams
    {
        public string Uid { get; set; }
        public string ApiKey { get; set; }
        public string Data { get; set; }
        public string Profile { get; set; }
        public string Preferences { get; set; }
        public string Subscriptions { get; set; }
    }
}

