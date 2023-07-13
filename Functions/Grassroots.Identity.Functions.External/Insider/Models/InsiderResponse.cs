namespace Grassroots.Identity.Functions.External.Insider.Models
{
    public class InsiderResponse
    {
        public InsiderResponseData Data { get; set; }
        public string Error { get; set; }
    }

    public class InsiderResponseData
    {
        public InsiderResponseSuccessful Successful { get; set; }
        public InsiderResponseFail Fail { get; set; }
    }

    public class InsiderResponseSuccessful
    {
        public int Count { get; set; }
    }

    public class InsiderResponseFail
    {
        public int Count { get; set; }
    }
}
