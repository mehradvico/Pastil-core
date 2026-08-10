using Application.Services.CategorySrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.ProductSrvs.BrandSrv.Dto;
using Application.Services.ProductSrvs.FeatureSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.CommonSrv.SearchSrv.Dto
{
    public class SearchDto
    {
        public List<SearchProductDto> Products { get; set; }
        public List<SearchCategoryDto> Categories { get; set; }
        public List<SearchBrandDto> Brands { get; set; }
        public List<SearchFeatureItemDto> Feature { get; set; }
        public List<SearchCompanionDto> Companions { get; set; }
        public List<SearchAssistanceDto> Assistances { get; set; }
        public List<SearchStoreDto> Stores { get; set; }
        public List<SearchPansionDto> Pansions { get; set; }
        public List<SearchCompanionAssistancePackageDto> Packages { get; set; }
        public List<SearchItemDto> Items { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public string Query { get; set; }
        public string NormalizedQuery { get; set; }
        public int TotalCount { get; set; }
        public long TookMilliseconds { get; set; }
    }

}
