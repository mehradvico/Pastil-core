using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Services.CommonSrv.SearchSrv.Dto
{
    public class SearchRequestDto
    {
        public const int MaxPerTypeCount = 20;
        public const int MaxTotalCount = 50;

        public long ProductNotId { get; set; }
        public int ProductCount { get; set; } = 5;
        public int BrandCount { get; set; } = 5;
        public int CategoryCount { get; set; } = 5;
        public int FeatureCount { get; set; } = 5;
        public int CompanionCount { get; set; } = 5;
        public int AssistanceCount { get; set; } = 5;
        public int StoreCount { get; set; } = 5;
        public int PansionCount { get; set; } = 5;
        public int PackageCount { get; set; } = 5;
        public int TotalCount { get; set; } = 20;

        public bool EnableFuzzy { get; set; } = true;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Q { get; set; }

        [JsonIgnore]
        public string[] SearchTerms { get; set; } = [];

        public void ClampCounts()
        {
            ProductCount = Clamp(ProductCount);
            BrandCount = Clamp(BrandCount);
            CategoryCount = Clamp(CategoryCount);
            FeatureCount = Clamp(FeatureCount);
            CompanionCount = Clamp(CompanionCount);
            AssistanceCount = Clamp(AssistanceCount);
            StoreCount = Clamp(StoreCount);
            PansionCount = Clamp(PansionCount);
            PackageCount = Clamp(PackageCount);
            TotalCount = System.Math.Clamp(TotalCount, 1, MaxTotalCount);
        }

        private static int Clamp(int value) => System.Math.Clamp(value, 0, MaxPerTypeCount);
    }


}
