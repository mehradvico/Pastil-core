namespace Application.Services.CommonSrv.SearchSrv
{
    public class SearchHybridOptions
    {
        public const string SectionName = "Search:Hybrid";
        public bool Enabled { get; set; }
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
        public int TimeoutSeconds { get; set; } = 3;
        public double SemanticWeight { get; set; } = 0.25;
    }
}
