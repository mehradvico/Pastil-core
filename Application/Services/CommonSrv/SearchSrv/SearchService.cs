using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.CategorySrv.Dto;
using Application.Services.CategorySrv.Iface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CommonSrv.SearchSrv.Iface;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Iface;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Application.Services.ProductSrvs.BrandSrv.Dto;
using Application.Services.ProductSrvs.BrandSrv.Iface;
using Application.Services.ProductSrvs.FeatureSrv.Dto;
using Application.Services.ProductSrvs.FeatureSrv.Iface;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.StoreSrv.Iface;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.SearchSrv
{
    public class SearchService : ISearchService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IProductService _productService;

        public SearchService(IServiceScopeFactory scopeFactory, IProductService productService)
        {
            _scopeFactory = scopeFactory;
            _productService = productService;
        }

        private async Task<List<TDto>> RunScoped<TService, TDto>(Func<TService, Task<List<TDto>>> action)
            where TService : notnull
        {
            using var scope = _scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<TService>();
            return await action(svc);
        }

        public async Task<BaseResultDto<SearchDto>> SearchAsync(SearchRequestDto request)
        {
            request.Q = SearchNormalizeHelper.Normalize(request.Q);

            var productsT = request.ProductCount > 0
                ? _productService.SearchMinAsync(request)
                : Task.FromResult<List<SearchProductDto>>(null);

            var pansionsT = request.PansionCount > 0
                ? RunScoped<IPansionService, SearchPansionDto>(s => s.SearchMinAsync(request))
                : Task.FromResult<List<SearchPansionDto>>(null);

            var storesT = request.StoreCount > 0
                ? RunScoped<IStoreService, SearchStoreDto>(s => s.SearchMinAsync(request))
                : Task.FromResult<List<SearchStoreDto>>(null);

            var categoriesT = request.CategoryCount > 0
                ? RunScoped<ICategoryService, SearchCategoryDto>(s => s.SearchMinAsync(request))
                : Task.FromResult<List<SearchCategoryDto>>(null);

            var brandsT = request.BrandCount > 0
                ? RunScoped<IBrandService, SearchBrandDto>(s => s.SearchMinAsync(request))
                : Task.FromResult<List<SearchBrandDto>>(null);

            var featuresT = request.FeatureCount > 0
                ? RunScoped<IFeatureItemService, SearchFeatureItemDto>(s => s.SearchMinAsync(request))
                : Task.FromResult<List<SearchFeatureItemDto>>(null);

            var companionsT = request.CompanionCount > 0
                ? RunScoped<ICompanionService, SearchCompanionDto>(s => s.SearchMinAsync(request))
                : Task.FromResult<List<SearchCompanionDto>>(null);

            var assistancesT = request.AssistanceCount > 0
                ? RunScoped<IAssistanceService, SearchAssistanceDto>(s => s.SearchMinAsync(request))
                : Task.FromResult<List<SearchAssistanceDto>>(null);

            await Task.WhenAll(productsT, pansionsT, storesT, categoriesT, brandsT, featuresT, companionsT, assistancesT);

            var result = new SearchDto
            {
                Products = productsT.Result,
                Pansions = pansionsT.Result,
                Stores = storesT.Result,
                Categories = categoriesT.Result,
                Brands = brandsT.Result,
                Feature = featuresT.Result,
                Companions = companionsT.Result,
                Assistances = assistancesT.Result,
            };


            return new BaseResultDto<SearchDto>(true, data: result);
        }
    }
}
    