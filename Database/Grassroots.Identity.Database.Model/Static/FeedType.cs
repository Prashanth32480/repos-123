using System.ComponentModel;

namespace Grassroots.Identity.Database.Model.Static
{
    public enum FeedType : byte
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Competition")]
        Competition = 1,
        [Description("Program")]
        Program = 2,
        [Description("Account")]
        Account = 3,
        [Description("Profile")]
        Profile = 4,
        [Description("Insider")]
        Insider = 5,
        [Description("OneCustomer")]
        OneCustomer = 6
    }
}