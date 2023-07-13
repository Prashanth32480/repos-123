namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class CdcAuditSearchRequest
    {
        public string CallId { get; set; }
        public string ApiKey { get; set; }
        public long FeedId { get; set; }
        public string Uid { get; set; }
    }
}
