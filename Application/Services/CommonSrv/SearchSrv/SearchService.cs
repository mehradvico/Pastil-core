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
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Iface;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services.CommonSrv.SearchSrv
{
    public class SearchService : ISearchService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IProductService _productService;
        private readonly IDataBaseContext _context;
        private readonly ISearchSemanticReranker _semanticReranker;
        private readonly ILogger<SearchService> _logger;
        private readonly IMemoryCache _cache;

        public SearchService(
            IServiceScopeFactory scopeFactory,
            IProductService productService,
            IDataBaseContext context,
            ISearchSemanticReranker semanticReranker,
            ILogger<SearchService> logger,
            IMemoryCache cache)
        {
            _scopeFactory = scopeFactory;
            _productService = productService;
            _context = context;
            _semanticReranker = semanticReranker;
            _logger = logger;
            _cache = cache;
        }

        private async Task<List<TDto>> RunScoped<TService, TDto>(Func<TService, Task<List<TDto>>> action)
            where TService : notnull
        {
            using var scope = _scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<TService>();
            return await action(svc);
        }

        private static readonly TimeSpan SearchTimeout = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan SearchCacheDuration = TimeSpan.FromMinutes(5);

        // جستجو گلوگاه اصلی زیر بار بود (۱۰ کاربر هم‌زمان ⇒ تا ۱۲ ثانیه): نتیجه‌ی هر q نرمال‌شده ۵ دقیقه کش می‌شود، اول بدون فازی
        // اجرا می‌شود (فازی فقط وقتی هیچ نتیجه‌ای نبود) و کل کار سقف ۳ ثانیه دارد؛ بعد از آن بخش‌های تمام‌شده برمی‌گردند، نه اتصال معلق.
        public async Task<BaseResultDto<SearchDto>> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var originalQuery = request.Q?.Trim() ?? string.Empty;
            // [StringLength] روی Q به‌خاطر SuppressModelStateInvalidFilter اعمال نمی‌شد؛ عبارت ۵۰۰۰ حرفی هم پردازش می‌شد
            if (originalQuery.Length > SearchRequestDto.MaxQueryLength)
                return new BaseResultDto<SearchDto>(false, Resource.Notification.SearchQueryTooLong, new SearchDto());

            request.Q = SearchNormalizeHelper.Normalize(request.Q);
            request.ClampCounts();

            if (request.Q.Length < 2)
                return new BaseResultDto<SearchDto>(false, Resource.Notification.SearchQueryTooShort, new SearchDto());

            cancellationToken.ThrowIfCancellationRequested();

            var cacheKey = BuildCacheKey(request);
            SearchDto result;
            var fromCache = _cache.TryGetValue(cacheKey, out SearchDto cached) && cached != null;
            if (fromCache)
            {
                result = cached;
            }
            else
            {
                using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutSource.CancelAfter(SearchTimeout);

                request.SearchTerms = SearchNormalizeHelper.BuildTerms(request.Q, enableFuzzy: false);
                result = await RunSearchAsync(request, originalQuery, timeoutSource.Token, cancellationToken);

                if (result.TotalCount == 0 && !result.IsPartial && request.EnableFuzzy && !timeoutSource.IsCancellationRequested)
                {
                    var fuzzyTerms = SearchNormalizeHelper.BuildTerms(request.Q, enableFuzzy: true);
                    if (fuzzyTerms.Length > request.SearchTerms.Length)
                    {
                        request.SearchTerms = fuzzyTerms;
                        result = await RunSearchAsync(request, originalQuery, timeoutSource.Token, cancellationToken);
                    }
                }

                if (!result.IsPartial)
                {
                    _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = SearchCacheDuration,
                        Size = 1
                    });
                }
            }

            // شیء کش‌شده بین درخواست‌ها مشترک است؛ فیلدهای مخصوص همین پاسخ روی یک کپی نوشته می‌شوند
            var response = result.ShallowCopy();
            response.Query = originalQuery;
            response.FromCache = fromCache;
            stopwatch.Stop();
            response.TookMilliseconds = stopwatch.ElapsedMilliseconds;

            var channel = NormalizeChannel(request.Channel);
            try
            {
                _context.SearchQueryLogs.Add(new SearchQueryLog
                {
                    Query = originalQuery,
                    NormalizedQuery = request.Q,
                    Channel = channel,
                    ResultCount = response.TotalCount,
                    TookMilliseconds = response.TookMilliseconds,
                    CreateDateUtc = DateTime.UtcNow
                });
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Search analytics could not be persisted for query {Query}.", request.Q);
            }

            return new BaseResultDto<SearchDto>(true, data: response);
        }

        private static string BuildCacheKey(SearchRequestDto request) =>
            string.Join("|",
                "search", request.Q, request.EnableFuzzy, request.ProductNotId, request.TotalCount,
                request.ProductCount, request.BrandCount, request.CategoryCount, request.FeatureCount,
                request.CompanionCount, request.AssistanceCount, request.StoreCount, request.PansionCount,
                request.PackageCount, request.SchoolCount, request.SchoolCourseCount, request.ConsultationPackageCount);

        private static List<T> CompletedOrNull<T>(Task<List<T>> task) =>
            task.IsCompletedSuccessfully ? task.Result : null;

        private async Task<SearchDto> RunSearchAsync(
            SearchRequestDto request,
            string originalQuery,
            CancellationToken timeoutToken,
            CancellationToken requestToken)
        {
            // محصولات هم در scope جدا اجرا می‌شوند تا پرس‌وجوی جامانده بعد از timeout روی DbContext همین درخواست (ثبت آمار) کار نکند
            var productsT = request.ProductCount > 0
                ? RunScoped<IProductService, SearchProductDto>(s => s.SearchMinAsync(request, timeoutToken))
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
                ? RunScoped<ICompanionAssistancePackageService, SearchCompanionAssistancePackageDto>(s => s.SearchMinAsync(request, timeoutToken))
                : Task.FromResult<List<SearchCompanionAssistancePackageDto>>(null);

            var schoolsT = request.SchoolCount > 0
                ? RunScoped<ISchoolService, SearchSchoolDto>(s => s.SearchMinAsync(request))
                : Task.FromResult<List<SearchSchoolDto>>(null);

            var schoolCoursesT = request.SchoolCourseCount > 0
                ? RunScoped<ISchoolCourseService, SearchSchoolCourseDto>(s => s.SearchMinAsync(request))
                : Task.FromResult<List<SearchSchoolCourseDto>>(null);

            var consultationPackagesT = request.ConsultationPackageCount > 0
                ? RunScoped<IConsultationPackageService, SearchConsultationPackageDto>(s => s.SearchMinAsync(request))
                : Task.FromResult<List<SearchConsultationPackageDto>>(null);

            var isPartial = false;
            try
            {
                await Task.WhenAll(productsT, pansionsT, storesT, categoriesT, brandsT, featuresT, companionsT, assistancesT, packagesT, schoolsT, schoolCoursesT, consultationPackagesT)
                    .WaitAsync(timeoutToken);
            }
            catch (OperationCanceledException) when (!requestToken.IsCancellationRequested)
            {
                isPartial = true;
                _logger.LogWarning("Search for {Query} hit the {Seconds}s limit; returning the finished groups only.", request.Q, SearchTimeout.TotalSeconds);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // یک بخش خراب کل جستجو را ۵۰۰ نمی‌کند؛ بقیه‌ی بخش‌ها برمی‌گردند
                isPartial = true;
                _logger.LogError(exception, "A search group failed for query {Query}.", request.Q);
            }

            var result = new SearchDto
            {
                Products = CompletedOrNull(productsT),
                Pansions = CompletedOrNull(pansionsT),
                Stores = CompletedOrNull(storesT),
                Categories = CompletedOrNull(categoriesT),
                Brands = CompletedOrNull(brandsT),
                Feature = CompletedOrNull(featuresT),
                Companions = CompletedOrNull(companionsT),
                Assistances = CompletedOrNull(assistancesT),
                Packages = CompletedOrNull(packagesT),
                Schools = CompletedOrNull(schoolsT),
                SchoolCourses = CompletedOrNull(schoolCoursesT),
                ConsultationPackages = CompletedOrNull(consultationPackagesT),
                Query = originalQuery,
                NormalizedQuery = request.Q,
                IsPartial = isPartial,
            };

            var rankedItems = SearchItemsBuilder.Build(result, request);
            rankedItems = await _semanticReranker.RerankAsync(request.Q, rankedItems, requestToken);
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
            result.Schools = RankGroup(result.Schools, rankedItems, SearchItemType.School, request.SchoolCount, item => item.Id);
            result.SchoolCourses = RankGroup(result.SchoolCourses, rankedItems, SearchItemType.SchoolCourse, request.SchoolCourseCount, item => item.Id);
            result.ConsultationPackages = RankGroup(result.ConsultationPackages, rankedItems, SearchItemType.ConsultationPackage, request.ConsultationPackageCount, item => item.Id);
            result.Suggestions = SearchNormalizeHelper.BuildSuggestions(request.Q, request.SearchTerms, result.TotalCount);
            return result;
        }

        private const string ShopChannel = "Shop";
        private const string AppChannel = "App";

        private static string NormalizeChannel(string channel) =>
            string.Equals(channel?.Trim(), ShopChannel, StringComparison.OrdinalIgnoreCase) ? ShopChannel : AppChannel;

        public async Task<BaseResultDto<List<string>>> GetPopularAsync(string channel, int days, int take, CancellationToken cancellationToken = default)
        {
            channel = NormalizeChannel(channel);
            days = Math.Clamp(days, 1, 365);
            take = Math.Clamp(take, 1, 20);
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var terms = await _context.SearchQueryLogs
                .AsNoTracking()
                .Where(item => item.Channel == channel && item.CreateDateUtc >= fromDate && item.ResultCount > 0)
                .GroupBy(item => item.NormalizedQuery)
                .OrderByDescending(group => group.Count())
                .ThenByDescending(group => group.Max(item => item.CreateDateUtc))
                .Select(group => group.Key)
                .Take(take)
                .ToListAsync(cancellationToken);

            return new BaseResultDto<List<string>>(true, data: terms);
        }

        /// <summary>
        /// پیشنهاد عبارت جستجوی فروشگاه: عبارت‌ها فقط از نام محصولاتی ساخته می‌شوند که لیست فروشگاه
        /// (Active، غیر پیش‌نویس، Name/SecondName شامل Q) همان‌ها را نشان می‌دهد؛ پس هر پیشنهاد حتماً نتیجه دارد.
        /// عبارت = از ابتدای کلمه‌ی منطبق تا انتهای همان کلمه، و یک نسخه با کلمه‌ی بعدی؛ مرتب بر اساس تکرار.
        /// </summary>
        public async Task<BaseResultDto<List<string>>> SuggestAsync(string q, int take, CancellationToken cancellationToken = default)
        {
            q = (q ?? string.Empty).Trim();
            take = Math.Clamp(take, 1, 20);
            if (q.Length < 2 || q.Length > 100)
                return new BaseResultDto<List<string>>(true, data: new List<string>());

            // دقیقاً همان فیلتری که لیست محصولات فروشگاه (ProductService.BaseSaerch با Available=true) اعمال می‌کند،
            // از جمله !Deleted؛ بدون آن محصول حذف‌شده پیشنهاد می‌شد ولی در لیست نبود.
            var availableLabel = ProductStatusEnum.ProductStatus_Available.ToString();
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => !p.Deleted && p.Active && p.StatusId != (long)ProductStatusEnum.ProductStatus_Draft
                            && (p.Name.Contains(q) || p.SecondName.Contains(q)))
                .OrderByDescending(p => p.Status.Label == availableLabel)
                .ThenByDescending(p => p.SellCount)
                .Select(p => new { p.Name, InStock = p.Status.Label == availableLabel })
                .Take(300)
                .ToListAsync(cancellationToken);

            // امتیاز: محصولِ موجود ۳، ناموجود ۱؛ عبارت‌های پرتکرار و موجود بالاتر می‌آیند
            var score = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var product in products)
            {
                var name = product.Name;
                if (string.IsNullOrWhiteSpace(name)) continue;
                var weight = product.InStock ? 3 : 1;
                var from = 0;
                while (from < name.Length)
                {
                    var idx = name.IndexOf(q, from, StringComparison.OrdinalIgnoreCase);
                    if (idx < 0) break;
                    from = idx + 1;
                    if (idx > 0 && name[idx - 1] != ' ') continue; // فقط ابتدای کلمه
                    // عبارت‌ها عیناً زیررشته‌ی نام می‌مانند (بدون نرمال‌سازی فاصله) تا Contains لیست حتماً پیدایشان کند
                    var end = name.IndexOf(' ', idx + q.Length);
                    if (end < 0) end = name.Length;
                    var one = name[idx..end].Trim();
                    if (!string.Equals(one, q, StringComparison.OrdinalIgnoreCase))
                        score[one] = score.GetValueOrDefault(one) + weight;
                    if (end < name.Length - 1 && name[end + 1] != ' ')
                    {
                        var end2 = name.IndexOf(' ', end + 1);
                        if (end2 < 0) end2 = name.Length;
                        var two = name[idx..end2].Trim();
                        score[two] = score.GetValueOrDefault(two) + weight;
                    }
                    break;
                }
            }

            var suggestions = score
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key.Length)
                .Take(take)
                .Select(kv => kv.Key)
                .ToList();

            return new BaseResultDto<List<string>>(true, data: suggestions);
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
