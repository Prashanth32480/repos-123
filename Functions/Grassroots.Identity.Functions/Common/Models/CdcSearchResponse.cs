using System;
using System.Collections.Generic;

namespace Grassroots.Identity.Functions.Common.Models
{
    public class CdcSearchResponse
    {
        public string CallId { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorDetails { get; set; }
        public string ErrorMessage { get; set; }
        public int ApiVersion { get; set; }
        public int StatusCode { get; set; }
        public string StatusReason { get; set; }
        public DateTime Time { get; set; }
        public List<CdcSearchResultData> Results { get; set; }
        public int ObjectsCount { get; set; }
        public int TotalCount { get; set; }
    }

    public class CdcSearchResultData
    {
       public string Uid { get; set; }
       public CdcDataModel Data { get; set; }
    }
}
