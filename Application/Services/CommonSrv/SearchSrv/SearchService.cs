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
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Iface;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using Application.Common.Enumerable;
using Persistence.Interface;
using Entities.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services.CommonSrv.SearchSrv
{
    public class SearchService : ISearchService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IProductService _productService;
        private readonly IDataBaseContext _context;
        private readonly ISearchSemanticReranker _semanticReranker;
        private readonly ILogger<SearchService> _logger;

        public SearchService(
            IServiceScopeFactory scopeFactory,
            IProductService productService,
            IDataBaseContext context,
            ISearchSemanticReranker semanticReranker,
            ILogger<SearchService> logger)
        {
            _scopeFactory = scopeFactory;
            _productService = productService;
            _context = context;
            _semanticReranker = semanticReranker;
            _logger = logger;
        }

        private async Task<List<TDto>> RunScoped<TService, TDto>(Func<TService, Task<List<TDto>>> action)
            where TService : notnull
        {
            using var scope = _scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<TService>();
            return await action(svc);
        }

        public async Task<BaseResultDto<SearchDto>> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var originalQuery = request.Q?.Trim() ?? string.Empty;
            request.Q = SearchNormalizeHelper.Normalize(request.Q);
            request.ClampCounts();
            request.SearchTerms = SearchNormalizeHelper.BuildTerms(request.Q, request.EnableFuzzy);

            if (request.Q.Length < 2)
                return new BaseResultDto<SearchDto>(false, "عبارت جستجو باید حداقل دو کاراکتر باشد.", new SearchDto());

            cancellationToken.ThrowIfCancellationRequested();

            var productsT = request.ProductCount > 0
                ? _productService.SearchMinAsync(request, cancellationToken)
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

            var packagesT = request.PackageCount > 0
                ? RunScoped<ICompanionAssistancePackageService, SearchCompanionAssistancePackageDto>(s => s.SearchMinAsync(request, cancellationToken))
                : Task.FromResult<List<SearchCompanionAssistancePackageDto>>(null);

            await Task.WhenAll(productsT, pansionsT, storesT, categoriesT, brandsT, featuresT, companionsT, assistancesT, packagesT)
                .WaitAsync(cancellationToken);

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
                Packages = packagesT.Result,
                Query = originalQuery,
                NormalizedQuery = request.Q,
            };

            var rankedItems = SearchItemsBuilder.Build(result, request);
            rankedItems = await _semanticReranker.RerankAsync(request.Q, rankedItems, cancellationToken);
            result.TotalCount = rankedItems.Count;
            result.Items = rankedItems.Take(request.TotalCount).ToList();
            result.Products = RankGroup(result.Products, rankedItems, SearchItemType.Product, request.ProductCount, item => item.Id);
            result.Categories = RankGroup(result.Categories, rankedItems, SearchItemType.Category, request.CategoryCount, item => item.Id);
            result.Brands = RankGroup(result.Brands, rankedItems, SearchItemType.Brand, request.BrandCount, item => item.Id);
            result.Feature = RankGroup(result.Feature, rankedItems, SearchItemType.FeatureItem, request.FeatureCount, item => item.Id);
            result.Companions = RankGroup(result.Companions, rankedItems, SearchItemType.Companion, request.CompanionCount, item => item.Id);
            result.Assistances = RankGroup(result.Assistances, rankedItems, SearchItemType.Assistance, request.AssistanceCount, item => item.Id);
            result.Stores = RankGroup(result.Stores, rankedItems, SearchItemType.Store, request.StoreCount, item => item.Id);
            result.Pansions = RankGroup(result.Pansions, rankedItems, SearchItemType.Pansion, request.PansionCount, item => item.Id);
            result.Packages = RankGroup(result.Packages, rankedItems, SearchItemType.CompanionAssistancePackage, request.PackageCount, item => item.Id);
            result.Suggestions = request.SearchTerms
                .Where(term => !term.Equals(request.Q, StringComparison.OrdinalIgnoreCase))
                .Where(term => term.Length >= 3)
                .Take(5)
                .ToList();
            stopwatch.Stop();
            result.TookMilliseconds = stopwatch.ElapsedMilliseconds;

            try
            {
                _context.SearchQueryLogs.Add(new SearchQueryLog
                {
                    Query = originalQuery,
                    NormalizedQuery = request.Q,
                    Channel = "App",
                    ResultCount = result.TotalCount,
                    TookMilliseconds = result.TookMilliseconds,
                    CreateDateUtc = DateTime.UtcNow
                });
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Search analytics could not be persisted for query {Query}.", request.Q);
            }

            return new BaseResultDto<SearchDto>(true, data: result);
        }

        private static List<T> RankGroup<T>(
            List<T> values,
            IReadOnlyCollection<SearchItemDto> rankedItems,
            SearchItemType type,
            int count,
            Func<T, long> idSelector)
        {
            if (values == null || count <= 0) return values;
            var scores = rankedItems.Where(item => item.Type == type).ToDictionary(item => item.Id, item => item.Score);
            return values
                .Where(item => scores.ContainsKey(idSelector(item)))
                .OrderByDescending(item => scores.GetValueOrDefault(idSelector(item)))
                .Take(count)
                .ToList();
        }
    }
}
