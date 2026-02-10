using System.Collections.Generic;

namespace Application.Services.CommonSrv.SearchSrv.Dto
{
    public class SearchRequestDto
    {
        public long ProductNotId { get; set; }
        public int ProductCount { get; set; } = 5;
        public int BrandCount { get; set; } = 5;
        public int CategoryCount { get; set; } = 5;
        public int FeatureCount { get; set; } = 5;
        public int CompanionCount { get; set; } = 5;
        public int AssistanceCount { get; set; } = 5;
        public int StoreCount { get; set; } = 5;
        public int PansionCount { get; set; } = 5;

        public string Q { get; set; }
    }


}
