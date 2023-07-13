using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class ActivityFunctionMapping
    {
        public string Property { get; set; }
        public string ActivityFunction { get; set; }
        public string AdditionalData { get; set; }
        public string Subscriptions { get; set; }
        public string Consent { get; set; }
    }
}
