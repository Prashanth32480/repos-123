using System;
using System.Collections.Generic;
using System.Text;

namespace Grassroots.Identity.Functions.Common.Models
{
    public class Response
    {
        public string callId { get; set; }
        public int errorCode { get; set; }
        public int apiVersion { get; set; }
        public int statusCode { get; set; }
        public string statusReason { get; set; }
        public DateTime time { get; set; }
        public string errorDetails { get; set; }
        public string errorMessage { get; set; }

    }
}
