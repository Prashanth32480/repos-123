using System.ComponentModel;

namespace Grassroots.Identity.Database.Model.Static
{
    public enum ProcessStatus : short
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Success")]
        Success = 1
    }
}