using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models;
using Grassroots.Common.Helpers.Telemetry;
using System.Net.Http;
using Grassroots.Common.Helpers.Configuration;
using System.Collections.Generic;
using Grassroots.Common.Helpers;
using System;

namespace Grassroots.Identity.Functions.Cdc.Cdc
{
    public class GetConfigurationActivityFunction
    {
        private readonly IConfigProvider _configuration;

        public GetConfigurationActivityFunction(IConfigProvider configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("GetConfigurationActivityFunction")]
        public async Task<List<ActivityFunctionMapping>> Run([ActivityTrigger]IDurableActivityContext activityContext)
        {

            return JsonHelper.DeserializeJsonObject<List<ActivityFunctionMapping>>(
                _configuration.GetValue(AppSettingsKey.ActivityFunctionMapping));
        }
    }
}
