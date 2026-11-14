using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.CategorySrv.Iface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductCategorySrv.Iface;
using Application.Services.ProductSrvs.ProductPictureSrv.Iface;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using AutoMapper;
using Dapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using Persistence.Interface;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
namespace Application.Services.ProductSrvs.ProductSrv
{
    public class ProductService : IProductService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICategoryService _categoryService;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IProductPictureService _productPictureService;
        // سقف کلیدواژه‌های یکتا در جست‌وجوی دسته‌ای؛ هر کلیدواژه یک پاس رشته‌ای روی کل جدول است.
        private const int MaxBatchSearchTerms = 40;

        private readonly string connectionString;

        private readonly Application.Services.CommonSrv.SearchSrv.Iface.ISearchVocabularyService _searchVocabulary;

        public ProductService(IDataBaseContext _context, IConfiguration config, IMapper mapper, ICategoryService categoryService, IProductCategoryService productCategoryService, IProductPictureService productPictureService, Application.Services.CommonSrv.SearchSrv.Iface.ISearchVocabularyService searchVocabulary)
        {
            this._searchVocabulary = searchVocabulary;
            this._context = _context;
            this.mapper = mapper;
            this._categoryService = categoryService;
            this._productCategoryService = productCategoryService;
            this._productPictureService = productPictureService;
            this.connectionString = config.GetValue<string>(
            "connection");
        }

        // ستونی که جستجوی نام روی آن انجام می‌شود: نام + نام دوم، با «ي/ك» عربی یکسان‌شده
        private static readonly System.Linq.Expressions.Expression<Func<Product, string>> SearchableName =
            s => (s.Name + " " + (s.SecondName ?? "")).Replace("ي", "ی").Replace("ك", "ک");

        private sealed class SearchTermHolder(string value)
        {
            // از طریق property خوانده می‌شود تا EF آن را پارامتر کند، نه مقدار ثابتِ داخل SQL
            public string Value { get; } = value;
        }

        /// <summary>شرط «یکی از معادل‌های این کلمه در نام هست»: «یا» بین معادل‌ها و «و» بین اجزای هر معادل.</summary>
        public static System.Linq.Expressions.Expression<Func<Product, bool>> NameMatchesAny(IEnumerable<string[]> likeTerms)
        {
            var contains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
            System.Linq.Expressions.Expression body = null;
            foreach (var alternative in likeTerms)
            {
                System.Linq.Expressions.Expression all = null;
                foreach (var part in alternative)
                {
                    var value = System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Constant(new SearchTermHolder(part)), nameof(SearchTermHolder.Value));
                    var call = System.Linq.Expressions.Expression.Call(SearchableName.Body, contains, value);
                    all = all == null ? call : System.Linq.Expressions.Expression.AndAlso(all, call);
                }
                if (all == null) continue;
                body = body == null ? all : System.Linq.Expressions.Expression.OrElse(body, all);
            }
            body ??= System.Linq.Expressions.Expression.Constant(true);
            return System.Linq.Expressions.Expression.Lambda<Func<Product, bool>>(body, SearchableName.Parameters[0]);
        }

        private IQueryable<Product> BaseSaerch(ProductInputDto searchDto) => BaseSaerch(searchDto, out _);

        private IQueryable<Product> BaseSaerch(ProductInputDto searchDto, out SearchQueryPlan searchPlan)
        {
            searchPlan = null;
            var query = _context.Products.Include(s => s.Category).Include(s => s.Brand).Include(s => s.Status).Include(s => s.Type).Include(s => s.Picture).Where(s => !s.Deleted).AsQueryable();
            if (searchDto.IsAdmin)
            {
                query = query.Include(s => s.Stores);
            }
            DateTime now = DateTime.Now;

            if (searchDto.Available == true)
            {
                query = query.Where(s => s.Active && s.StatusId != (long)ProductStatusEnum.ProductStatus_Draft);
            }
            else
            {
                if (searchDto.Active.HasValue)
                {
                    query = query.Where(s => s.Active == searchDto.Active.Value);
                }
            }


            if (searchDto.InStock == true)
            {
                query = query.Where(s => s.Status.Label == ProductStatusEnum.ProductStatus_Available.ToString());
            }
            else if (searchDto.InStock == false)
            {
                query = query.Where(s => s.Status.Label != ProductStatusEnum.ProductStatus_Available.ToString());
            }
            if (searchDto.CreateStoreId.HasValue)
            {
                query = query.Where(s => s.StoreId == searchDto.CreateStoreId);
            }
            if (searchDto.Status.HasValue)
            {
                query = query.Where(s => s.Status.Label == searchDto.Status.ToString());
            }
            if (searchDto.Type.HasValue)
            {
                query = query.Where(s => s.Type.Label == searchDto.Type.ToString());
            }
            if (searchDto.Distance != null && searchDto.Distance.DistanceMeter > 0)
            {
                var point = mapper.Map<Point>(searchDto.Distance);
                query = query.Where(s => s.Location != null && s.Location.Distance(point) < searchDto.Distance.DistanceMeter);
            }
            if (!string.IsNullOrEmpty(searchDto.Q))
            {
                // هر کلمه جدا باید در نام باشد؛ «ي/ك» عربی و ارقام فارسی/لاتین مانع پیدا شدن محصول نمی‌شوند (متن کپی‌شده از اکسل).
                // هر کلمه با معادل‌های واقعی‌اش در کاتالوگ هم جستجو می‌شود: «مکسی» ← «ماکسی»، «گسترو» ← «گاسترو»، غلط املایی،
                // نویسه‌ی لاتین («maxi») و کیبورد اشتباه. معادل فقط از بین کلمه‌های موجود در نام محصولات انتخاب می‌شود.
                searchPlan = (_searchVocabulary?.Products ?? SearchVocabulary.Empty).Plan(searchDto.Q);
                // کلمه‌ای که هیچ معادلی در کاتالوگ ندارد نتیجه را حتماً خالی می‌کند؛ در جستجوی مشتری کنار گذاشته می‌شود
                // به شرطی که کلمه‌های شناخته‌شده‌ی قوی مانده باشند (ادمین همان رفتار دقیق قبلی را دارد).
                foreach (var word in searchPlan.Words)
                {
                    if (searchDto.Available == true && searchPlan.IgnoredWords.Contains(word.Word)) continue;
                    query = query.Where(NameMatchesAny(word.LikeTerms));
                }
            }

            if (searchDto.PriceFrom.HasValue)
            {
                query = query.Where(s => s.Price >= searchDto.PriceFrom);
            }
            if (searchDto.PriceTo.HasValue)
            {
                query = query.Where(s => s.Price <= searchDto.PriceTo);
            }
            if (searchDto.NotId.HasValue)
            {
                query = query.Where(s => s.Id != searchDto.NotId);
            }
            if (searchDto.BrandIds != null && searchDto.BrandIds.Any())
            {
                query = query.Where(s => s.BrandId.HasValue && searchDto.BrandIds.Contains(s.BrandId.Value));
            }
            if (searchDto.HasDiscount.HasValue)
            {
                query = query.Where(s => s.DiscountGroupId.HasValue == searchDto.HasDiscount.Value);
            }
            if (!string.IsNullOrEmpty(searchDto.DiscountGroupLabel))
            {
                query = query.Where(s => s.DiscountGroupId.HasValue && (s.DiscountGroup.Label == searchDto.DiscountGroupLabel));
            }
            if (searchDto.CategoryIds != null && searchDto.CategoryIds.Any())
            {
                if (searchDto.IsAndCategories)
                    foreach (var categoryId in searchDto.CategoryIds)
                    {
                        query = query.Where(s => s.Categories.Any(a => a.Id == categoryId));
                    }
                else
                    query = query.Where(s => s.Categories.Any(x => searchDto.CategoryIds.Contains(x.Id)));

            }
            else if (searchDto.CategoryLabels != null && searchDto.CategoryLabels.Any())
            {
                if (searchDto.IsAndCategories)
                    query = query.Where(s => searchDto.CategoryLabels.All(a => s.Categories.Select(c => c.Label).Contains(a)));
                else
                    query = query.Where(s => s.Categories.Any(x => searchDto.CategoryLabels.Contains(x.Label)));

            }
            if (searchDto.FeatureItemIds != null && searchDto.FeatureItemIds.Any())
            {
                query = query.Where(s => s.ProductFeatureValues.Any(x => x.FeatureItemId.HasValue && searchDto.FeatureItemIds.Any(a => a.Equals(x.FeatureItemId.Value))));

            }
            if (searchDto.StoreId.HasValue)
            {
                // آیتم حذف‌شده/غیرفعال فروشگاه، کالا را در صفحه‌ی آن فروشگاه نشان نمی‌دهد
                query = searchDto.ApplyStorePrice
                    ? query.Where(s => s.ProductItems.Any(x => x.StoreId == searchDto.StoreId && x.Active && !x.Deleted))
                    : query.Where(s => s.ProductItems.Any(x => x.StoreId == searchDto.StoreId));

            }
            // جستجوی عمومی سایت/اپ (Available): اول محصولاتِ موجود و قیمت‌دار، بعد ناموجودها؛ مرتب‌سازیِ انتخابی کاربر داخل هر گروه اعمال می‌شود.
            IOrderedQueryable<Product> ordered = null;
            if (searchDto.Available == true)
            {
                ordered = query.OrderByDescending(s => s.StatusId == (long)ProductStatusEnum.ProductStatus_Available && s.Price > 0);
            }
            IQueryable<Product> Sort<TKey>(System.Linq.Expressions.Expression<Func<Product, TKey>> key, bool descending) =>
                ordered == null
                    ? (descending ? query.OrderByDescending(key) : query.OrderBy(key))
                    : (descending ? ordered.ThenByDescending(key) : ordered.ThenBy(key));

            switch (searchDto.SortBy)
            {

                case Common.Enumerable.SortEnum.Default:
                case Common.Enumerable.SortEnum.New:
                    query = Sort(s => s.Id, true);
                    break;
                case Common.Enumerable.SortEnum.Old:
                    query = Sort(s => s.Id, false);
                    break;
                case Common.Enumerable.SortEnum.Name:
                    query = Sort(s => s.Name, true);
                    break;
                case Common.Enumerable.SortEnum.MoreVisit:
                    query = Sort(s => s.VisitCount, true);
                    break;
                case Common.Enumerable.SortEnum.LessVisit:
                    query = Sort(s => s.VisitCount, false);
                    break;
                case Common.Enumerable.SortEnum.Expensive:
                    if (searchDto.ApplyStorePrice && searchDto.StoreId.HasValue)
                    {
                        // صفحه‌ی فروشگاه: مرتب‌سازی با قیمتِ خودِ همان فروشگاه (نه ارزان‌ترین بین همه‌ی فروشگاه‌ها)
                        var sid = searchDto.StoreId.Value;
                        query = Sort(s => s.ProductItems.Where(x => x.StoreId == sid && x.Active && x.SystemActive && !x.Deleted && x.Quantity > 0 && x.Price > 0).Select(x => (long?)x.Price).Min() ?? 0, true);
                    }
                    else
                        query = Sort(s => s.Price, true);
                    break;
                case Common.Enumerable.SortEnum.Inexpensive:
                    if (searchDto.ApplyStorePrice && searchDto.StoreId.HasValue)
                    {
                        var sid = searchDto.StoreId.Value;
                        query = Sort(s => s.ProductItems.Where(x => x.StoreId == sid && x.Active && x.SystemActive && !x.Deleted && x.Quantity > 0 && x.Price > 0).Select(x => (long?)x.Price).Min() ?? long.MaxValue, false);
                    }
                    else
                        query = Sort(s => s.Price, false);
                    break;
                case Common.Enumerable.SortEnum.MoreSell:
                    query = Sort(s => s.SellCount, true);
                    break;
                case Common.Enumerable.SortEnum.LessSell:
                    query = Sort(s => s.SellCount, false);
                    break;
                default:
                    if (ordered != null)
                        query = ordered.ThenByDescending(s => s.Id);
                    break;
            }
            return query;
        }
        public ProductSearchDto Search(ProductInputDto baseSearchDto)
        {
            var model = BaseSaerch(baseSearchDto, out var searchPlan);
            var result = new ProductSearchDto(baseSearchDto, model, mapper);
            // فقط جستجوی مشتری: ادمین عبارت را دقیق می‌خواهد و کلمه‌ای برایش کنار گذاشته نمی‌شود
            if (searchPlan != null && baseSearchDto.Available == true && result.TotalCount > 0)
            {
                result.CorrectedQ = searchPlan.CorrectedQuery;
                result.IgnoredWords = searchPlan.IgnoredWords.Count > 0 ? searchPlan.IgnoredWords.ToArray() : null;
            }
            if (baseSearchDto.ApplyStorePrice) ApplyStorePrices(result.List, baseSearchDto.StoreId);
            MarkExpressShipping(result.List);
            MarkSellerStores(result.List);
            return result;
        }

        // ───────────── کالاهای مشابه / در کنارش خریداری شده (موتور: ProductRelationEngine) ─────────────
        private const int RelationMaxTake = 24;
        private const int RelationPoolCap = 4000;

        private IQueryable<ProductCandidate> RelationCandidates(IQueryable<Product> query) =>
            query.Select(p => new ProductCandidate
            {
                Id = p.Id, Name = p.Name, SecondName = p.SecondName, CategoryId = p.CategoryId, BrandId = p.BrandId,
                Price = p.Price, StatusId = p.StatusId, SellCount = p.SellCount, RateAvg = p.RateAvg,
                LeafName = p.Category.Name, ParentCategoryId = p.Category.ParentId, ParentName = p.Category.Parent.Name,
            });

        private IQueryable<Product> RelationBase() =>
            _context.Products.AsNoTracking().Where(p => !p.Deleted && p.Active && p.CategoryId != null
                && p.StatusId != (long)ProductStatusEnum.ProductStatus_Draft);

        private async Task<(ProductCandidate Source, List<ProductCandidate> Pool)> LoadRelationPoolAsync(long productId, bool wide)
        {
            var source = await RelationCandidates(_context.Products.AsNoTracking().Where(p => p.Id == productId && !p.Deleted && p.Active && p.CategoryId != null))
                .FirstOrDefaultAsync();
            if (source == null) return (null, new List<ProductCandidate>());
            var leafId = source.CategoryId;
            var parentId = source.ParentCategoryId;
            var query = RelationBase().Where(p => p.Id != productId);
            if (wide)
            {
                // مکمل‌ها در برگ‌های دیگرِ همان حیوان‌اند؛ برگ‌های مشترک (اسباب‌بازی سگ و گربه) زیر والدِ «سگ» ثبت شده‌اند
                var words = ProductRelationEngine.SpeciesLeafWords(ProductRelationEngine.Profile(source).Species).ToList();
                var w0 = words.Count > 0 ? words[0] : null;
                var w1 = words.Count > 1 ? words[1] : null;
                var w2 = words.Count > 2 ? words[2] : null;
                query = query.Where(p => p.CategoryId == leafId
                    || (parentId != null && p.Category.ParentId == parentId)
                    || (w0 != null && p.Category.Name.Contains(w0))
                    || (w1 != null && p.Category.Name.Contains(w1))
                    || (w2 != null && p.Category.Name.Contains(w2)));
            }
            else
            {
                query = query.Where(p => p.CategoryId == leafId || (parentId != null && p.Category.ParentId == parentId));
            }
            var pool = await RelationCandidates(query).Take(RelationPoolCap).ToListAsync();
            return (source, pool);
        }

        private async Task<ProductSimilarVDto> LoadRelationResultAsync(List<long> ids)
        {
            var result = new ProductSimilarVDto();
            if (ids.Count == 0) return result;
            var entities = await _context.Products.AsNoTracking()
                .Include(s => s.Category).Include(s => s.Brand).Include(s => s.Status).Include(s => s.Type).Include(s => s.Picture)
                .Where(s => ids.Contains(s.Id)).ToListAsync();
            var byId = entities.ToDictionary(e => e.Id);
            var ordered = ids.Where(byId.ContainsKey).Select(id => byId[id]).ToList();
            var list = mapper.Map<List<ProductVDto>>(ordered);
            MarkExpressShipping(list);
            MarkSellerStores(list);
            result.List = list;
            result.TotalCount = list.Count;
            return result;
        }

        public async Task<ProductSimilarVDto> SimilarAsync(long productId, int take)
        {
            take = Math.Clamp(take <= 0 ? 12 : take, 1, RelationMaxTake);
            var (source, pool) = await LoadRelationPoolAsync(productId, wide: false);
            if (source == null) return new ProductSimilarVDto();
            return await LoadRelationResultAsync(ProductRelationEngine.Similar(source, pool, take));
        }

        public async Task<ProductSimilarVDto> BoughtTogetherAsync(long productId, int take)
        {
            take = Math.Clamp(take <= 0 ? 8 : take, 1, RelationMaxTake);
            var ids = new List<long>();

            // ۱) دست‌چینِ ادمین (ProductRelates) همیشه اول؛ فقط فعال و غیرپیش‌نویس
            var curatedIds = await _context.ProductRelates.AsNoTracking()
                .Where(r => r.ProductId == productId).Select(r => r.RelatedProductId).ToListAsync();
            if (curatedIds.Count > 0)
            {
                var ok = await RelationBase().Where(p => curatedIds.Contains(p.Id) && p.Id != productId).Select(p => p.Id).ToListAsync();
                ids.AddRange(curatedIds.Where(ok.Contains));
            }

            // ۲) هوشمند: نوع‌های مکمل + شواهد خرید هم‌زمان واقعی
            if (ids.Count < take)
            {
                var (source, pool) = await LoadRelationPoolAsync(productId, wide: true);
                if (source != null)
                {
                    var coPurchase = await LoadCoPurchaseAsync(productId);
                    var smart = ProductRelationEngine.BoughtTogether(source, pool, coPurchase, take);
                    foreach (var id in smart)
                        if (!ids.Contains(id) && ids.Count < take) ids.Add(id);
                }
            }
            return await LoadRelationResultAsync(ids.Take(take).ToList());
        }

        // productId → تعداد سفارش‌های پرداخت‌شده‌ی لغو‌نشده که هم‌زمان با (دست‌کم یکی از) محصولات مبدأ خریده شده‌اند
        private Task<Dictionary<long, int>> LoadCoPurchaseAsync(long productId) => LoadCoPurchaseAsync(new List<long> { productId });

        private async Task<Dictionary<long, int>> LoadCoPurchaseAsync(List<long> productIds)
        {
            if (productIds.Count == 0) return new Dictionary<long, int>();
            var canceled = ProductOrderStateEnum.ProductOrderState_Canceled.ToString();
            var orderIds = await _context.ProductOrderItems.AsNoTracking()
                .Where(i => !i.Deleted && productIds.Contains(i.ProductItem.ProductId)
                    && !i.ProductOrderStore.Deleted && !i.ProductOrderStore.ProductOrder.Deleted
                    && i.ProductOrderStore.ProductOrder.IsPaid
                    && i.ProductOrderStore.ProductOrder.ProductOrderState.Label != canceled)
                .Select(i => i.ProductOrderStore.ProductOrderId).Distinct().Take(3000).ToListAsync();
            if (orderIds.Count == 0) return new Dictionary<long, int>();
            var pairs = await _context.ProductOrderItems.AsNoTracking()
                .Where(i => !i.Deleted && !i.ProductOrderStore.Deleted && orderIds.Contains(i.ProductOrderStore.ProductOrderId) && !productIds.Contains(i.ProductItem.ProductId))
                .Select(i => new { OrderId = i.ProductOrderStore.ProductOrderId, i.ProductItem.ProductId })
                .Distinct().ToListAsync();
            return pairs.GroupBy(x => x.ProductId).ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<CartSuggestionsVDto> CartSuggestionsAsync(long? userId, string uniqueId, long? storeId, int take)
        {
            take = Math.Clamp(take <= 0 ? 6 : take, 1, 12);
            var empty = new CartSuggestionsVDto();
            if (!userId.HasValue && string.IsNullOrWhiteSpace(uniqueId)) return empty;

            var rows = await _context.CartItems.AsNoTracking()
                .Where(i => userId.HasValue ? i.CartStore.Cart.UserId == userId : i.CartStore.Cart.UniqueId == uniqueId)
                .Select(i => new { i.ProductItem.ProductId, i.CartStore.StoreId }).Distinct().Take(100).ToListAsync();
            var allIds = rows.Select(r => r.ProductId).Distinct().ToList();
            // با storeId: فقط اقلامِ همان فروشگاه نامزدکننده‌اند (همان چیزی که الان پرداخت می‌شود)
            var productIds = storeId.HasValue && storeId.Value > 0
                ? rows.Where(r => r.StoreId == storeId.Value).Select(r => r.ProductId).Distinct().ToList()
                : allIds;
            if (productIds.Count == 0) return empty;

            var cartAll = await RelationCandidates(_context.Products.AsNoTracking().Where(p => allIds.Contains(p.Id) && !p.Deleted && p.CategoryId != null)).ToListAsync();
            var cart = cartAll.Where(c => productIds.Contains(c.Id)).ToList();
            if (cart.Count == 0) return empty;

            // پول: همه‌ی کالاهای همان حیوان‌ها (والد دسته) + برگ‌های مشترک «سگ و گربه»
            var parentIds = cart.Where(c => c.ParentCategoryId.HasValue).Select(c => c.ParentCategoryId.Value).Distinct().ToList();
            var species = cart.Aggregate(PetSpecies.None, (acc, c) => acc | ProductRelationEngine.Profile(c).Species);
            var words = ProductRelationEngine.SpeciesLeafWords(species).Take(4).ToList();
            var w0 = words.Count > 0 ? words[0] : null;
            var w1 = words.Count > 1 ? words[1] : null;
            var w2 = words.Count > 2 ? words[2] : null;
            var w3 = words.Count > 3 ? words[3] : null;
            var poolQuery = RelationBase().Where(p => !allIds.Contains(p.Id)
                && ((p.Category.ParentId != null && parentIds.Contains(p.Category.ParentId.Value))
                    || (w0 != null && p.Category.Name.Contains(w0)) || (w1 != null && p.Category.Name.Contains(w1))
                    || (w2 != null && p.Category.Name.Contains(w2)) || (w3 != null && p.Category.Name.Contains(w3))));
            if (storeId.HasValue && storeId.Value > 0)
            {
                // فقط کالایی که همان فروشگاه همین الان با موجودی می‌فروشد (کاربر باید بتواند با همان پرداخت اضافه‌اش کند)
                var sid = storeId.Value;
                poolQuery = poolQuery.Where(p => p.ProductItems.Any(x => x.StoreId == sid && x.Active && x.SystemActive && !x.Deleted && x.Quantity > 0
                    && x.Store.Active && !x.Store.Deleted));
            }
            var pool = await RelationCandidates(poolQuery).Take(RelationPoolCap).ToListAsync();

            var coPurchase = await LoadCoPurchaseAsync(productIds);
            var picks = ProductRelationEngine.CartSuggestions(cart, pool, coPurchase, take, cartAll);
            if (picks.Count == 0) return empty;

            var loaded = await LoadRelationResultAsync(picks.Select(x => x.ProductId).ToList());
            var cartNames = cart.ToDictionary(c => c.Id, c => c.Name);
            var kindByProduct = picks.ToDictionary(x => x.ProductId);
            var result = new CartSuggestionsVDto { List = loaded.List, TotalCount = loaded.TotalCount };
            foreach (var product in loaded.List)
            {
                var pick = kindByProduct[product.Id];
                result.Reasons.Add(new CartSuggestionReasonVDto
                {
                    ProductId = product.Id, Kind = pick.Kind, ForProductId = pick.ForProductId,
                    ForProductName = cartNames.TryGetValue(pick.ForProductId, out var n) ? n : null,
                });
            }
            return result;
        }

        // لیستِ صفحه‌ی یک فروشگاه: قیمت/تخفیف هر کارت = ارزان‌ترین آیتمِ موجودِ «همان فروشگاه» (نه ارزان‌ترین بین همه‌ی فروشگاه‌ها)،
        // تا قیمتی که کاربر در صفحه‌ی فروشگاه می‌بیند با قیمتِ پیش‌فرض صفحه‌ی کالا (همان فروشگاه) یکی باشد.
        private void ApplyStorePrices(List<ProductVDto> list, long? storeId)
        {
            if (!storeId.HasValue || list == null || list.Count == 0)
                return;
            var sid = storeId.Value;
            var ids = list.Select(item => item.Id).ToList();
            var rows = _context.ProductItems.AsNoTracking()
                .Where(i => ids.Contains(i.ProductId) && i.StoreId == sid && i.Active && i.SystemActive && !i.Deleted && i.Quantity > 0 && i.Price > 0)
                .Select(i => new { i.ProductId, i.Id, i.Price, i.BasePrice, i.DiscountPercent, i.DiscountGroupId, i.DiscountEndDate })
                .ToList();
            var best = rows.GroupBy(r => r.ProductId).ToDictionary(g => g.Key, g => g.OrderBy(r => r.Price).ThenBy(r => r.Id).First());
            foreach (var item in list)
            {
                if (best.TryGetValue(item.Id, out var row))
                {
                    item.Price = row.Price;
                    item.BasePrice = row.BasePrice;
                    item.DiscountPercent = row.DiscountPercent;
                    item.DiscountGroupId = row.DiscountGroupId;
                    item.DiscountEndDate = row.DiscountEndDate;
                    item.StoreItemAvailable = true;
                }
                else
                {
                    // در این فروشگاه موجود نیست: قیمتِ فروشگاه‌های دیگر را به‌عنوان قیمت این فروشگاه نشان نمی‌دهیم
                    item.Price = 0;
                    item.BasePrice = 0;
                    item.DiscountPercent = 0;
                    item.DiscountGroupId = null;
                    item.DiscountEndDate = null;
                    item.StoreItemAvailable = false;
                }
            }
        }

        // نشان فروشگاه روی کارت محصول: فروشگاه‌هایی که الان این محصول را با موجودی می‌فروشند (ارزان‌ترین اول)؛
        // همان شرط‌های UpdateProductPrice (آیتم فعال/SystemActive/موجودی > ۰، فروشگاه فعال). یک کوئری برای کل صفحه.
        private void MarkSellerStores(List<ProductVDto> list)
        {
            if (list == null || list.Count == 0)
                return;
            var ids = list.Select(item => item.Id).ToList();
            var rows = _context.ProductItems.AsNoTracking()
                .Where(item => ids.Contains(item.ProductId) && item.Active && item.SystemActive && !item.Deleted && item.Quantity > 0
                               && item.Store.Active && !item.Store.Deleted)
                .GroupBy(item => new { item.ProductId, item.StoreId, item.Store.Name, item.Store.PictureId })
                .Select(group => new { group.Key.ProductId, group.Key.StoreId, group.Key.Name, group.Key.PictureId, MinPrice = group.Min(x => x.Price) })
                .ToList();
            if (rows.Count == 0)
                return;
            var pictureIds = rows.Where(row => row.PictureId.HasValue).Select(row => row.PictureId.Value).Distinct().ToList();
            var pictures = pictureIds.Count == 0
                ? new Dictionary<long, PictureVDto>()
                : _context.Pictures.AsNoTracking().Where(picture => pictureIds.Contains(picture.Id)).ToList()
                    .ToDictionary(picture => picture.Id, picture => mapper.Map<PictureVDto>(picture));
            var byProduct = rows.GroupBy(row => row.ProductId).ToDictionary(group => group.Key, group => group.OrderBy(row => row.MinPrice).ToList());
            foreach (var item in list)
            {
                if (!byProduct.TryGetValue(item.Id, out var sellers))
                    continue;
                item.SellerStoreCount = sellers.Count;
                item.SellerStores = sellers.Take(3).Select(row => new ProductSellerStoreVDto
                {
                    Id = row.StoreId,
                    Name = row.Name,
                    Picture = row.PictureId.HasValue && pictures.TryGetValue(row.PictureId.Value, out var picture) ? picture : null,
                }).ToList();
            }
        }

        // نشان «ارسال اکسپرس»: با یک کوئری برای کل صفحه (نه به‌ازای هر محصول)
        private void MarkExpressShipping(List<ProductVDto> list)
        {
            if (list == null || list.Count == 0)
                return;
            var ids = list.Select(item => item.Id).ToList();
            var express = _context.ProductItems.AsNoTracking()
                .Where(item => ids.Contains(item.ProductId) && item.Active && item.SystemActive && !item.Deleted && item.Quantity > 0
                               && item.Store.ExpressShippingEnabled && item.Store.Active && !item.Store.Deleted)
                .Select(item => item.ProductId)
                .Distinct()
                .ToHashSet();
            foreach (var item in list)
                item.ExpressShipping = express.Contains(item.Id);
        }
        public async Task<BaseResultDto<ProductDto>> FindAsyncDto(long id, long? storeId = null)
        {
            var item = await _context.Products.Include(s => s.ProductPictures).ThenInclude(s => s.Picture).Include(s => s.Categories).Include(s => s.Picture).SingleOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                if (storeId != null)
                {
                    if (item.StoreId != storeId || item.StatusId != (long)ProductStatusEnum.ProductStatus_Draft)
                    {
                        return new BaseResultDto<ProductDto>(false, val: Resource.Notification.AccessDenied, null);

                    }
                }
                return new BaseResultDto<ProductDto>(true, mapper.Map<ProductDto>(item));

            }
            return new BaseResultDto<ProductDto>(false, null);
        }
        public async Task<BaseResultDto<ProductDto>> InsertAsyncDto(ProductDto dto)
        {
            using var transaction = await _context.BeginTransactionAsync();
            try
            {
                var modelCheker = ModelHelper<ProductDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if (dto.Variety2Id != null && dto.VarietyId == null)
                    {
                        return new BaseResultDto<ProductDto>(false, Resource.Notification.PleaseSpecifyFirstVariationValue, dto);
                    }
                    if (dto.VarietyId.HasValue && (dto.VarietyId == dto.Variety2Id))
                    {
                        return new BaseResultDto<ProductDto>(false, Resource.Notification.VarietiesShouldBeDifferent, dto);
                    }
                    var item = mapper.Map<Product>(dto);
                    item.VarietyId = dto.VarietyId;
                    item.Variety2Id = dto.Variety2Id;
                    item.CreateDate = DateTime.Now;
                    item.UpdateDate = DateTime.Now;
                    await _context.Products.AddAsync(item);
                    await _context.SaveChangesAsync();

                    if (dto.CategoryIds == null)
                    {
                        dto.CategoryIds = new List<long>();
                    }
                    if (dto.CategoryId.HasValue)
                    {
                        var categoryParents = await _categoryService.GetAllParents(dto.CategoryId.Value);
                        foreach (var categoryParent in categoryParents)
                        {
                            dto.CategoryIds.Add(categoryParent.Id);
                        }
                    }
                    if (dto.CategoryIds.Any())
                    {
                        dto.CategoryIds = dto.CategoryIds.Distinct().ToList();
                        await _productCategoryService.InsertOrUpdateAsync(item, dto.CategoryIds);
                    }

                    //if (dto.ProductPictures.Any())
                    //{
                    //    _productPictureService.InsertOrUpdate(item, dto.ProductPictures);
                    //}
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    await UpdateProductPriceAsync(ProductUpdateTypeEnum.Product, item.Id.ToString());

                    return new BaseResultDto<ProductDto>(true, mapper.Map<ProductDto>(item));
                }

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new BaseResultDto<ProductDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }
        public async Task<BaseResultDto> UpdateDtoAsync(ProductDto dto, long? storeId = null)
        {
            using var transaction = await _context.BeginTransactionAsync();

            try
            {
                var modelCheker = ModelHelper<ProductDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = await _context.Products.Include(s => s.Categories).Include(s => s.ProductPictures).AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);
                    if (storeId != null)
                    {
                        if (item.StoreId != storeId || item.StatusId != (long)ProductStatusEnum.ProductStatus_Draft)
                        {
                            return new BaseResultDto<ProductDto>(false, val: Resource.Notification.AccessDenied, null);

                        }
                    }
                    // فروشنده (storeId != null) فقط فیلدهای محتوایی را عوض می‌کند؛ وضعیت/آمار/مالکیت/یادداشت ادمین ثابت می‌ماند.
                    var sellerEdit = storeId != null;
                    var keepStoreId = item.StoreId;
                    var keepStatusId = item.StatusId;
                    var keepSellCount = item.SellCount;
                    var keepVisitCount = item.VisitCount;
                    var keepRateAvg = item.RateAvg;
                    var keepRateCount = item.RateCount;
                    var keepAdminDescription = item.AdminDescription;
                    mapper.Map(dto, item);
                    if (sellerEdit)
                    {
                        item.StoreId = keepStoreId;
                        item.StatusId = keepStatusId;
                        item.SellCount = keepSellCount;
                        item.VisitCount = keepVisitCount;
                        item.RateAvg = keepRateAvg;
                        item.RateCount = keepRateCount;
                        item.AdminDescription = keepAdminDescription;
                    }
                    item.UpdateDate = DateTime.Now;
                    _context.Products.Update(item);
                    await _context.SaveChangesAsync();
                    if (dto.CategoryIds == null)
                    {
                        dto.CategoryIds = new List<long>();
                    }
                    if (dto.CategoryId.HasValue)
                    {
                        var categoryParents = await _categoryService.GetAllParents(dto.CategoryId.Value);
                        foreach (var categoryParent in categoryParents)
                        {
                            dto.CategoryIds.Add(categoryParent.Id);
                        }
                    }
                    dto.CategoryIds = dto.CategoryIds.Distinct().ToList();
                    await _productCategoryService.InsertOrUpdateAsync(item, dto.CategoryIds);



                    _productPictureService.InsertOrUpdate(item, dto.ProductPictures);

                    await _context.SaveChangesAsync();
                    await _context.CommitTransactionAsync();
                    await UpdateProductPriceAsync(ProductUpdateTypeEnum.Product, dto.Id.ToString());

                    return new BaseResultDto<ProductDto>(true, mapper.Map<ProductDto>(item));
                }
            }
            catch (Exception ex)
            {
                await _context.RollbackTransactionAsync();
                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }
        public BaseResultDto DeleteDto(long id)
        {
            try
            {
                var item = _context.Products.Find(id);
                item.Deleted = true;
                _context.Products.Update(item);
                _context.SaveChanges();
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }
        public async Task<BaseResultDto<ProductVDto>> FindAsyncVDto(long id, bool visit = true)
        {
            var item = await _context.Products.Include(s => s.Category).Include(s => s.Status).Include(s => s.Brand).Include(s => s.Picture).Include(s => s.ProductFiles).Include(s => s.Category).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).SingleOrDefaultAsync(s => s.Id == id && s.Active && s.Deleted != true);
            if (item != null)
            {
                if (visit)
                {
                    item.VisitCount++;
                    _context.Products.Update(item);
                    await _context.SaveChangesAsync();
                }
                return new BaseResultDto<ProductVDto>(true, mapper.Map<ProductVDto>(item));
            }
            return new BaseResultDto<ProductVDto>(false, mapper.Map<ProductVDto>(item));
        }
        public async Task<Product> GetByIdAsync(long id)
        {
            return await _context.Products.AsTracking().Include(s => s.Variety).Include(s => s.Type).Include(s => s.Status).FirstOrDefaultAsync(s => s.Id == id && s.Deleted == false);
        }
        public async Task IncreaseSellCountAsync(ProductOrder order)
        {
            // کاهش موجودی و افزایش فروش به‌صورت اتمیک در خود دیتابیس انجام می‌شود
            // (read-modify-write روی Entity باعث گم‌شدن کاهش‌ها در سفارش‌های همزمان می‌شد).
            // موجودی هیچ‌وقت منفی نمی‌شود. خطا دیگر بلعیده نمی‌شود؛ فراخواننده بعد از
            // ثبت سفارش موفق است و نباید به‌خاطر این مرحله سفارش را ناموفق کند، پس فقط
            // به Trace می‌رود تا در لاگ سرور قابل رصد باشد.
            var productIds = new List<long>();
            foreach (var itemStore in order.ProductOrderStores)
            {
                foreach (var item in itemStore.ProductOrderItems)
                {
                    try
                    {
                        var count = item.Count;
                        var productItemId = item.ProductItemId;
                        var productId = item.ProductItem.ProductId;
                        await _context.ProductItems
                            .Where(p => p.Id == productItemId)
                            .ExecuteUpdateAsync(setters => setters.SetProperty(
                                p => p.Quantity,
                                p => p.Quantity >= count ? p.Quantity - count : 0));
                        await _context.Products
                            .Where(p => p.Id == productId)
                            .ExecuteUpdateAsync(setters => setters.SetProperty(
                                p => p.SellCount,
                                p => p.SellCount + count));
                        productIds.Add(productId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError(
                            "IncreaseSellCount failed for order {0}, productItem {1}: {2}",
                            order.Id, item.ProductItemId, ex);
                    }
                }
            }

            if (productIds.Count == 0)
                return;

            try
            {
                string productIdsString = string.Join(",", productIds.Distinct());
                await UpdateProductPriceAsync(ProductUpdateTypeEnum.Product, productIdsString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "UpdateProductPrice after order {0} failed: {1}", order.Id, ex);
            }
        }
        public BaseResultDto GetSiteMap()
        {
            string sqlQuery = "SELECT p.Id, p.Name, p.Slug, p.UpdateDate, c.Label As CategoryName FROM Products p LEFT JOIN Categories c ON p.CategoryId = c.Id WHERE p.Active = 1 and p.Deleted=0";
            //var list = _context.Posts.Include(s => s.Category).Where(s => s.Deleted == false && s.Active && s.AdminConfirm == true).Select(s => new PostSiteMapDto() { Id = s.Id, Name = s.Name, CategoryName = s.Category.Label,UpdateDate=s.PublishDate }).ToList();
            var connection = new SqlConnection(connectionString);
            var posts = connection.Query<ProductSiteMapDto>(sqlQuery).ToList();
            return new BaseResultDto<List<ProductSiteMapDto>>(true, posts);
        }
        public async Task UpdateProductPriceAsync(ProductUpdateTypeEnum productUpdateType, string Id)
        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UpdateProductItemDiscount", new { FilterType = productUpdateType.ToString(), FilterIds = Id }, commandType: System.Data.CommandType.StoredProcedure);
        }
        public async Task<List<SearchProductDto>> SearchMinAsync(SearchRequestDto request, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(connectionString);

            var parameters = new
            {
                ProductCount = SearchQueryHelper.CandidateCount(request.ProductCount),
                ProductNotId = request.ProductNotId,
                Query = request.Q,
                SearchTerms = string.Join(" ", request.SearchTerms)
            };

            var query = $@"
DECLARE @Keywords TABLE (Keyword NVARCHAR(255));

INSERT INTO @Keywords (Keyword)
SELECT value
FROM STRING_SPLIT(@SearchTerms, ' ')
WHERE LEN(value) >= 2;

SELECT TOP(@ProductCount)
    p.Id,
    p.Name,
    p.SecondName,

    pi.BasePrice,
    pi.Price,
    pi.DiscountPercent,

    st.Id AS StoreId,
    ISNULL(st.Name, '') AS StoreName,

    picture.Id AS PictureId,
    picture.Url + '/' + picture.Name AS Url,
    picture.Url AS BaseUrl,
    picture.Name,
    picture.GuidName,
    picture.Extension,

    c.Label AS CategoryName,
    ISNULL(br.Name, '') AS BrandName,

    (
        SELECT COUNT(*)
        FROM @Keywords k
        WHERE p.Name COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
           OR ISNULL(p.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
           OR ISNULL(br.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
           OR ISNULL(br.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
           OR ISNULL(c.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
           OR ISNULL(p.ProductLabel, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
           OR EXISTS (
                SELECT 1
                FROM ProductFeatureValues pfv
                LEFT JOIN FeatureItems fi ON pfv.FeatureItemId = fi.Id
                WHERE pfv.ProductId = p.Id
                  AND (ISNULL(pfv.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
                    OR ISNULL(fi.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%')
           )
    ) AS MatchScore
FROM Products p

CROSS APPLY (
    SELECT TOP 1 pi.*
    FROM ProductItems pi
    WHERE pi.ProductId = p.Id
      AND pi.Active = 1
      AND pi.SystemActive = 1
      AND pi.Deleted = 0
      AND pi.Quantity > 0
    ORDER BY pi.Price ASC, pi.Quantity DESC, pi.Id ASC
) pi

INNER JOIN Stores st ON pi.StoreId = st.Id
LEFT JOIN Pictures picture ON p.PictureId = picture.Id
LEFT JOIN Categories c ON p.CategoryId = c.Id
LEFT JOIN Brands br ON p.BrandId = br.Id

WHERE
    p.Active = 1
    AND p.Deleted = 0
    AND st.Active = 1
    AND st.Deleted = 0
    AND (p.Id != @ProductNotId OR @ProductNotId IS NULL)
    AND (
        p.Name COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR ISNULL(p.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR ISNULL(br.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR ISNULL(br.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR ISNULL(c.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR ISNULL(p.ProductLabel, '') COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR EXISTS (
            SELECT 1
            FROM @Keywords k
            WHERE p.Name COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(p.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(br.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(br.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(c.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(p.ProductLabel, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR EXISTS (
                    SELECT 1
                    FROM ProductFeatureValues pfv
                    LEFT JOIN FeatureItems fi ON pfv.FeatureItemId = fi.Id
                    WHERE pfv.ProductId = p.Id
                      AND (ISNULL(pfv.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
                        OR ISNULL(fi.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%')
               )
        )
    )
-- اول محصولی که کلمه‌های بیشتری از عبارت را دارد؛ قبلاً StatusId جلوتر بود و تطبیق کامل از ۳۰ نامزد اول بیرون می‌ماند
ORDER BY MatchScore DESC, p.StatusId DESC, p.SellCount DESC;";

            var command = new CommandDefinition(query, parameters, commandTimeout: 8, cancellationToken: cancellationToken);
            var result = await connection.QueryAsync<SearchProductDto, PictureVDto, string, string, SearchProductDto>(
                command,
                (product, picture, categoryName, brandName) =>
                {
                    product.Picture = picture;
                    product.CategoryName = categoryName;
                    product.BrandName = brandName;
                    return product;
                },
                splitOn: "PictureId,CategoryName,BrandName"
            );

            return result.ToList();
        }

        // برای AiProductMatch: بر خلاف SearchMinAsync (که برای جستجوی مشتری طراحی شده و طبق CROSS APPLY
        // فقط محصولاتی با حداقل یک ProductItem موجود/فعال در یک فروشگاه فعال را برمی‌گرداند)، این متد
        // هیچ پیش‌شرط موجودی/فروشگاهی ندارد — چون هدف دقیقاً پیدا کردن محصولات کاتالوگیه که فروشنده
        // فعلی (یا هیچ فروشگاهی) هنوز موجودی‌ای برایشان ثبت نکرده، تا بتواند آیتم جدید برایشان بسازد.
        // currentStoreId: اگر داده شود، پیش‌نویسِ فروشگاه‌های دیگر (هنوز تأییدنشده) نتیجه نمی‌شود؛ null = بدون این فیلتر (ادمین).
        public async Task<List<long>> SearchCatalogProductIdsAsync(SearchRequestDto request, CancellationToken cancellationToken = default, long? currentStoreId = null)
        {
            using var connection = new SqlConnection(connectionString);

            var parameters = new
            {
                ProductCount = SearchQueryHelper.CandidateCount(request.ProductCount),
                ProductNotId = request.ProductNotId,
                Query = request.Q,
                SearchTerms = string.Join(" ", request.SearchTerms),
                StoreId = currentStoreId,
                DraftStatusId = (long)ProductStatusEnum.ProductStatus_Draft
            };

            var query = @"
DECLARE @Keywords TABLE (Keyword NVARCHAR(255));

INSERT INTO @Keywords (Keyword)
SELECT value
FROM STRING_SPLIT(@SearchTerms, ' ')
WHERE LEN(value) >= 2;

SELECT TOP(@ProductCount)
    p.Id,
    (
        -- مجموع «طول» کلمه‌های جورشده، نه تعدادشان: کلمهٔ تخصصی («پرشین»، نام کامل) وزن بیشتری از کلمهٔ
        -- عمومی («غذای»، «مدل») و دوحرفی‌های فازی دارد؛ وگرنه در کاتالوگ ده‌ها هزارتایی همه‌چیز هم‌امتیاز می‌شود.
        SELECT ISNULL(SUM(LEN(k.Keyword)), 0)
        FROM @Keywords k
        WHERE p.Name COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
           OR ISNULL(p.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
           OR ISNULL(br.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
           OR ISNULL(br.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
           OR ISNULL(c.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
           OR ISNULL(p.ProductLabel, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
    ) AS MatchScore
FROM Products p
LEFT JOIN Categories c ON p.CategoryId = c.Id
LEFT JOIN Brands br ON p.BrandId = br.Id
WHERE
    p.Active = 1
    AND p.Deleted = 0
    AND (p.Id != @ProductNotId OR @ProductNotId IS NULL)
    AND (@StoreId IS NULL OR p.StatusId <> @DraftStatusId OR p.StoreId = @StoreId)
    AND (
        p.Name COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR ISNULL(p.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR ISNULL(br.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR ISNULL(br.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR ISNULL(c.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR ISNULL(p.ProductLabel, '') COLLATE Persian_100_CI_AS LIKE '%' + @Query + '%'
        OR EXISTS (
            SELECT 1
            FROM @Keywords k
            WHERE p.Name COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(p.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(br.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(br.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(c.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(p.ProductLabel, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
        )
    )
-- عمداً p.StatusId در مرتب‌سازی نیست: در کاتالوگ، محصول فقط وقتی «موجود» می‌شود که یک فروشگاه آن را به
-- فروشگاه خودش اضافه کرده باشد؛ هدف این جست‌وجو دقیقاً پیدا کردن محصولاتی است که هنوز هیچ فروشگاهی ندارند.
ORDER BY MatchScore DESC, p.Id;";

            var command = new CommandDefinition(query, parameters, commandTimeout: 8, cancellationToken: cancellationToken);
            var result = await connection.QueryAsync<long>(command);
            return result.ToList();
        }

        // نسخهٔ دسته‌ای SearchCatalogProductIdsAsync برای AiProductMatch: به‌جای یک کوئری به‌ازای هر
        // ردیفِ تشخیص‌داده‌شده از عکس، **یک** کوئری برای همهٔ ردیف‌ها با کلیدواژه‌های یکی‌شده.
        //
        // چرا: Name/SecondName/ProductLabel از نوع nvarchar(max) اند، پس LIKE '%..%' با COLLATE روی
        // آن‌ها نه ایندکس می‌گیرد نه ارزان است؛ هزینه = تعدادردیف × تعدادکلیدواژه × تعدادستون. با ۶ عکس
        // قفسه، شش کوئری موازی همین کار را شش بار تکرار می‌کردند و هر شش تا به commandTimeout=8s
        // می‌خوردند (Execution Timeout Expired) ⇒ صفر کاندید ⇒ همهٔ ردیف‌ها CatalogMatchFailed.
        //
        // سه صرفه‌جویی هم‌زمان، بدون تغییر در کیفیت نتیجه:
        //  ۱) یک کوئری به‌جای N تا — و چون ردیف‌های یک قفسه کلمات مشترک زیادی دارند («غذای»، «خشک»،
        //     «رویال»...)، تعداد کلیدواژه‌های یکتا تقریباً ثابت می‌ماند، نه N برابر.
        //  ۲) امتیاز فقط یک‌بار حساب می‌شود (MatchScore > 0 به‌جای EXISTS جداگانه + ساب‌کوئری دوم).
        //  ۳) ستون Categories.Name از شرط حذف شده: نام دسته («غذای خشک گربه») تقریباً برای تمام
        //     اعضای همان دسته صادق است، پس قدرت تفکیک ~صفر داشت و فقط یک‌ششم کار رشته‌ای را می‌خورد.
        //
        // تخصیص کاندید به هر ردیف دیگر با MatchScore انجام نمی‌شود؛ سمت سرویس با شباهت نام نرمال‌شده
        // (AiProductMatchMatchingHelper) است که برای «همین محصول است یا نه» دقیق‌تر از جمع طول کلمات است.
        public async Task<List<CatalogShortlistItemDto>> SearchCatalogShortlistAsync(
            IReadOnlyCollection<string> searchTerms,
            int shortlistSize,
            CancellationToken cancellationToken = default,
            long? currentStoreId = null)
        {
            var terms = (searchTerms ?? Array.Empty<string>())
                .Where(term => !string.IsNullOrWhiteSpace(term) && term.Trim().Length >= 2)
                .Select(term => term.Trim())
                .Distinct()
                // کلمهٔ بلندتر = تخصصی‌تر؛ اگر مجبور به بریدن شدیم، عمومی‌ها («مدل»، «وزن») اول بریده شوند.
                .OrderByDescending(term => term.Length)
                .Take(MaxBatchSearchTerms)
                .ToList();

            if (terms.Count == 0)
                return new List<CatalogShortlistItemDto>();

            using var connection = new SqlConnection(connectionString);

            var parameters = new
            {
                ProductCount = Math.Clamp(shortlistSize, 1, 500),
                SearchTerms = string.Join(" ", terms),
                StoreId = currentStoreId,
                DraftStatusId = (long)ProductStatusEnum.ProductStatus_Draft
            };

            var query = @"
DECLARE @Keywords TABLE (Keyword NVARCHAR(255) PRIMARY KEY);

INSERT INTO @Keywords (Keyword)
SELECT DISTINCT value
FROM STRING_SPLIT(@SearchTerms, ' ')
WHERE LEN(value) >= 2;

SELECT TOP(@ProductCount)
    scored.Id,
    scored.Name,
    scored.BrandName
FROM (
    SELECT
        p.Id,
        p.Name,
        ISNULL(br.Name, '') AS BrandName,
        (
            SELECT ISNULL(SUM(LEN(k.Keyword)), 0)
            FROM @Keywords k
            WHERE p.Name COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(p.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(br.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(br.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
               OR ISNULL(p.ProductLabel, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%'
        ) AS MatchScore
    FROM Products p
    LEFT JOIN Brands br ON p.BrandId = br.Id
    WHERE
        p.Active = 1
        AND p.Deleted = 0
        -- پیش‌نویسِ تأییدنشدهٔ فروشگاه‌های دیگر به این فروشنده پیشنهاد نمی‌شود؛ پیش‌نویس خودش بله.
        AND (@StoreId IS NULL OR p.StatusId <> @DraftStatusId OR p.StoreId = @StoreId)
) AS scored
WHERE scored.MatchScore > 0
ORDER BY scored.MatchScore DESC, scored.Id;";

            var command = new CommandDefinition(query, parameters, commandTimeout: 15, cancellationToken: cancellationToken);
            var result = await connection.QueryAsync<CatalogShortlistItemDto>(command);
            return result.ToList();
        }

        public async Task<BaseResultDto<ProductDto>> DuplicateAsyncDto(ProductDuplicateDto productDuplicateDto)
        {
            var orgProduct = await _context.Products.Include(s => s.ProductPictures).Include(s => s.Categories).Include(s => s.Picture).Include(s => s.ProductFeatureValues).SingleOrDefaultAsync(s => s.Id == productDuplicateDto.ProductId);

            if (orgProduct == null)
            {
                return new BaseResultDto<ProductDto>(false, mapper.Map<ProductDto>(productDuplicateDto));
            }

            using var transaction = await _context.BeginTransactionAsync();
            var newProduct = new Product()
            {
                StoreId = productDuplicateDto.StoreId,
                Name = orgProduct.Name,
                ProductLabel = orgProduct.ProductLabel,
                SecondName = orgProduct.SecondName,
                CategoryId = orgProduct.CategoryId,
                BrandId = orgProduct.BrandId,
                Summary = orgProduct.Summary,
                Description = orgProduct.Description,
                Price = orgProduct.Price,
                BasePrice = orgProduct.BasePrice,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                StatusId = orgProduct.StatusId,
                TypeId = orgProduct.TypeId,
                VarietyId = orgProduct.VarietyId,
                Variety2Id = orgProduct.Variety2Id,
                SeoNoIndex = orgProduct.SeoNoIndex,
                SeoNoFollow = orgProduct.SeoNoFollow,
                Active = orgProduct.Active,
                SeoH1 = orgProduct.SeoH1,
                SeoMinDescription = orgProduct.SeoMinDescription,
                SeoDescription = orgProduct.SeoDescription,
                SeoTitle = orgProduct.SeoTitle,
                SeoPictureAlt = orgProduct.SeoPictureAlt,
                SeoUrlText = orgProduct.SeoUrlText,
                SeoCanonical = orgProduct.SeoCanonical,
                SellLimitCount = orgProduct.SellLimitCount,
                DiscountGroup = orgProduct.DiscountGroup,
                DiscountEndDate = orgProduct.DiscountEndDate,
                DiscountPercent = orgProduct.DiscountPercent,
                CodeValue = productDuplicateDto.CodeValue,
            };
            if (productDuplicateDto.DuplicatePicture)
            {
                newProduct.PictureId = orgProduct.PictureId;
            }
            if (productDuplicateDto.DuplicateProductPictures)
            {
                newProduct.ProductPictures = new List<ProductPicture>();

                foreach (var pp in orgProduct.ProductPictures)
                {
                    var newProductPicure = new ProductPicture();

                    newProductPicure.PictureId = pp.PictureId;
                    newProductPicure.Label = pp.Label;
                    newProduct.ProductPictures.Add(newProductPicure);

                }
            }
            if (productDuplicateDto.DuplicateProductFeatureValues)
            {
                newProduct.ProductFeatureValues = new List<ProductFeatureValue>();
                foreach (var pp in orgProduct.ProductFeatureValues)
                {
                    var newfeatureValue = new ProductFeatureValue();

                    newfeatureValue.FeatureId = pp.FeatureId;
                    newfeatureValue.FeatureItemId = pp?.FeatureItemId;
                    newfeatureValue.Name = pp?.Name;
                    newProduct.ProductFeatureValues.Add(newfeatureValue);
                }
            }
            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();

            var categoryIds = orgProduct.Categories.Select(c => c.Id).ToList();
            if (categoryIds.Any())
            {
                await _productCategoryService.InsertOrUpdateAsync(newProduct, categoryIds);
            }

            await transaction.CommitAsync();

            return new BaseResultDto<ProductDto>(true, mapper.Map<ProductDto>(newProduct));
        }
        public async Task<BaseResultDto> ChangeProductVarietiesAsync(ProductDto product)
        {
            if (product.Variety2Id != null && product.VarietyId == null)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSpecifyFirstVariationValue);
            }
            if (product.VarietyId.HasValue && (product.VarietyId == product.Variety2Id))
            {
                return new BaseResultDto(false, Resource.Notification.VarietiesShouldBeDifferent);
            }
            var productEntity = await _context.Products.FindAsync(product.Id);
            if (productEntity != null)
            {
                if (product.VarietyId != productEntity.VarietyId || product.Variety2Id != productEntity.Variety2Id)
                {
                    // اگر فروشنده‌ای آیتمی با مقدار تنوع برای این محصول دارد، ساختار تنوع قابل حذف/عوض‌شدن نیست
                    // (قبلاً همه‌ی آیتم‌های همه‌ی فروشگاه‌ها بی‌صدا حذف نرم می‌شد).
                    var hasItemsUsingVarieties = await _context.ProductItems.AsNoTracking()
                        .AnyAsync(i => i.ProductId == product.Id && !i.Deleted && (i.VarietyItemId != null || i.VarietyItem2Id != null));
                    if (Application.Services.ProductSrvs.ProductItemSrv.ProductVarietyRules.ChangeIsBlocked(
                            productEntity.VarietyId, productEntity.Variety2Id, product.VarietyId, product.Variety2Id, hasItemsUsingVarieties))
                        return new BaseResultDto(false, Resource.Notification.ProductVarietyInUseCannotBeChanged);

                    // تغییر ساختار و پاک‌شدن آیتم‌های قدیمی (آیتم‌های بدون مقدار؛ چون مقدارداری‌ها بالا رد شدند) یا هر دو انجام
                    // می‌شود یا هیچ‌کدام؛ بعدش قیمت محصول دوباره محاسبه می‌شود.
                    await using (var transaction = await _context.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted))
                    {
                        productEntity.VarietyId = product.VarietyId;
                        productEntity.Variety2Id = product.Variety2Id;
                        _context.Products.Update(productEntity);
                        await _context.SaveChangesAsync();
                        await _context.ProductItems.Where(s => s.ProductId == product.Id).ExecuteUpdateAsync(s => s.SetProperty(a => a.Deleted, true).SetProperty(a => a.Active, false).SetProperty(a => a.SystemActive, false));
                        await transaction.CommitAsync();
                    }

                    try
                    {
                        await UpdateProductPriceAsync(ProductUpdateTypeEnum.Product, product.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        // تغییر ساختار ثبت شده؛ خرابی محاسبه‌ی قیمت نباید پاسخ را ناموفق کند.
                        System.Diagnostics.Trace.TraceError("Price recalculation after variety change failed for product {0}: {1}", product.Id, ex);
                    }
                    return new BaseResultDto(true, Resource.Notification.SuccessfullyCompletedPreviousVariationsWereRemoved);
                }

                // تغییری نبود: خطا نیست (idempotent)
                return new BaseResultDto(true, Resource.Notification.NoVarietyChangeNeeded);
            }
            return new BaseResultDto(false, Resource.Notification.Unsuccess);

        }
    }
}
