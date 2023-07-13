namespace Grassroots.Identity.Functions.External.Common.Model
{
    public class CdcRegTokenResponse
    {
        public string Token { get; set; }
        public string CallId { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
    }
}
