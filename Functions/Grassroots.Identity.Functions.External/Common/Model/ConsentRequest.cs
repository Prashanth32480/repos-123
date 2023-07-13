using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grassroots.Identity.Functions.External.Common.Model
{
    public class ConsentRequest
    {
        public string Consent { get; set; }
        public bool IsConsentGranted { get; set; }
    }
}
