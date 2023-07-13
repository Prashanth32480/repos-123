namespace Grassroots.Identity.Functions.Cdc.Cdc.Accounts.Models
{
    public class CdcGetAccountInfoRequest
    {
        public string Uid { get; set; }
        public long FeedId { get; set; }
        public bool? HasFullAccount { get; set; }

    }
   
}
