using System;

namespace Application.Services.CommonSrv.SearchSrv.Dto
{
    public class SearchAnalyticsDto
    {
        public string Query { get; set; }
        public string Channel { get; set; }
        public int SearchCount { get; set; }
        public int ZeroResultCount { get; set; }
        public double AverageResultCount { get; set; }
        public double AverageTookMilliseconds { get; set; }
        public DateTime LastSearchDateUtc { get; set; }
    }
}
