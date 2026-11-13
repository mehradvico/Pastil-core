using Application.Services.ProductSrvs.ProductSrv;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Xunit;

namespace Application.Tests.ProductRelations;

// «کالاهای مشابه» و «در کنارش خریداری شده»: هیچ‌وقت کالای بی‌ربط (خاک گربه ↔ پد سگ) نشان داده نشود.
public class ProductRelationEngineTests
{
    private static long _id = 1000;
    private const long Available = ProductRelationEngine.AvailableStatusId;

    private static ProductCandidate P(string name, string leaf, string parent, long leafId, long parentId, long price = 100000, long status = Available, long? brand = null, int sells = 0) =>
        new ProductCandidate { Id = ++_id, Name = name, LeafName = leaf, ParentName = parent, CategoryId = leafId, ParentCategoryId = parentId, Price = price, StatusId = status, BrandId = brand, SellCount = sells };

    private const string CatCare = "بهداشت، مراقبت و نگهداری حیوانات";
    private const string DogCare = "بهداشت، مراقبت و نگهداری حیوانات";

    private static List<ProductCandidate> World() => new()
    {
        // گربه
        P("خاک گربه میوکا گرانول وزن 10 کیلوگرم", CatCare, "گربه", 10107, 9),
        P("خاک گربه لیمو کربن دار", CatCare, "گربه", 10107, 9),
        P("خاک گربه کلیرکت مدل لوندر", CatCare, "گربه", 10107, 9),
        P("ظرف خاک مسقف میشا با بیلچه", CatCare, "گربه", 10107, 9),
        P("مت ظرف خاک گربه پتوپولی", CatCare, "گربه", 10107, 9),
        P("بیلچه خاک گربه هپی پت", CatCare, "گربه", 10107, 9),
        P("بوگیر خاک گربه زیوا", CatCare, "گربه", 10107, 9),
        P("جای خواب SHUTTEL صورتی 50*50*60", CatCare, "گربه", 10107, 9),
        P("ظرف آب و غذا استیل", CatCare, "گربه", 10107, 9),
        P("اسکرچر چوبی گربه", CatCare, "گربه", 10107, 9),
        P("تشویقی گربه دریمیز طعم سالمون", "تشویقی گربه", "گربه", 10064, 9),
        P("غذای خشک گربه رویال کنین Kitten", "غذای خشک گربه", "گربه", 10066, 9),
        // سگ
        P("پد بهداشتی سگ کندی بسته ۱۰ عددی سایز ۶۰×۹۰", "زیرانداز بهداشتی سگ", "سگ", 10098, 7),
        P("پد زیر انداز 60*90 shawn", DogCare, "سگ", 10108, 7),
        P("جای خواب مخصوص سگ سایز XL مدل Walter", DogCare, "سگ", 10108, 7),
        P("جای خواب مخصوص سگ و گربه سایز M", DogCare, "سگ", 10108, 7),
        P("تشویقی سگ ونپی مدل Jerky", "تشویقی سگ", "سگ", 8, 7),
        P("تشویقی سگ تریکسی Premio", "تشویقی سگ", "سگ", 8, 7),
        P("قلاده پلیس سایز M مشکی", "قلاده سگ", "سگ", 10120, 7),
        P("لیش جیبی", "قلاده سگ", "سگ", 10120, 7),
        P("اسباب بازی توپ سگ و گربه", "اسباب بازی سگ و گربه", "سگ", 10087, 7),
        P("ظرف آب و غذا سگ مدل دوقلو", DogCare, "سگ", 10108, 7),
        P("غذای خشک سگ رویال کنین مینی", "غذای خشک سگ", "سگ", 10062, 7),
        P("کنسرو سگ پترسون", "غذای مرطوب، کنسرو و پوچ سگ", "سگ", 17, 7),
        // پرنده
        P("اسباب بازی پرندگان کد 31", "آموزش، سرگرمی و اکسسوری حیوانات", "پرنده", 10119, 15),
    };

    private static ProductCandidate By(List<ProductCandidate> w, string name) => w.First(p => p.Name.StartsWith(name));

    private static List<ProductCandidate> Pool(List<ProductCandidate> w, ProductCandidate src, bool wide)
    {
        var words = ProductRelationEngine.SpeciesLeafWords(ProductRelationEngine.Profile(src).Species);
        return w.Where(p => p.CategoryId == src.CategoryId || p.ParentCategoryId == src.ParentCategoryId ||
            (wide && words.Any(x => (p.LeafName ?? "").Contains(x)))).ToList();
    }

    private static List<string> Names(List<ProductCandidate> w, IEnumerable<long> ids) => ids.Select(i => w.First(p => p.Id == i).Name).ToList();

    [Theory]
    [InlineData("خاک گربه میوکا", "ظرف خاک")] // خاک گربه هیچ‌وقت پد سگ/جای خواب نشان نمی‌دهد
    public void Similar_of_cat_litter_are_only_other_cat_litters(string src, string _)
    {
        var w = World();
        var s = By(w, src);
        var names = Names(w, ProductRelationEngine.Similar(s, Pool(w, s, false), 12));
        Assert.Equal(2, names.Count);
        Assert.All(names, n => Assert.StartsWith("خاک گربه", n));
    }

    [Fact]
    public void Similar_never_crosses_species_or_kind()
    {
        var w = World();
        var dogPad = By(w, "پد بهداشتی سگ");
        var names = Names(w, ProductRelationEngine.Similar(dogPad, Pool(w, dogPad, false), 12));
        Assert.Contains("پد زیر انداز 60*90 shawn", names);                 // پد سگ ↔ پد
        Assert.DoesNotContain(names, n => n.Contains("خاک") || n.Contains("جای خواب") || n.Contains("تشویقی"));

        var dogBed = By(w, "جای خواب مخصوص سگ سایز XL");
        var bedNames = Names(w, ProductRelationEngine.Similar(dogBed, Pool(w, dogBed, false), 12));
        Assert.Contains("جای خواب مخصوص سگ و گربه سایز M", bedNames);
        Assert.DoesNotContain(bedNames, n => n.Contains("پد") || n.Contains("ظرف"));
        // جای خواب گربه برای جای خواب سگ پیشنهاد نمی‌شود (در پول سگ اصلاً نیست) ولی حتی اگر در پول بود حیوان ناسازگار بود
        var withCat = Pool(w, dogBed, false).Concat(new[] { By(w, "جای خواب SHUTTEL") }).ToList();
        Assert.DoesNotContain("جای خواب SHUTTEL صورتی 50*50*60", Names(w, ProductRelationEngine.Similar(dogBed, withCat, 12)));
    }

    [Fact]
    public void Similar_puts_available_first_and_skips_nothing_related()
    {
        var w = World();
        var treat = By(w, "تشویقی سگ ونپی");
        w.First(p => p.Name.StartsWith("تشویقی سگ تریکسی")).StatusId = 7; // ناموجود
        w.First(p => p.Name.StartsWith("تشویقی سگ تریکسی")).Price = 0;
        var ids = ProductRelationEngine.Similar(treat, Pool(w, treat, false), 12);
        Assert.Single(ids);
        Assert.StartsWith("تشویقی سگ تریکسی", Names(w, ids)[0]); // ناموجود فقط وقتی جایگزین موجود نیست
    }

    [Fact]
    public void BoughtTogether_cat_litter_completes_with_litter_accessories_only()
    {
        var w = World();
        var s = By(w, "خاک گربه میوکا");
        var names = Names(w, ProductRelationEngine.BoughtTogether(s, Pool(w, s, true), new Dictionary<long, int>(), 8));
        Assert.Contains("ظرف خاک مسقف میشا با بیلچه", names);
        Assert.Contains("مت ظرف خاک گربه پتوپولی", names);
        Assert.Contains("بیلچه خاک گربه هپی پت", names);
        Assert.Contains("بوگیر خاک گربه زیوا", names);
        Assert.DoesNotContain(names, n => n.Contains("سگ") || n.StartsWith("خاک گربه")); // هم‌نوع = «مشابه»، سگ = بی‌ربط
    }

    [Fact]
    public void BoughtTogether_dog_bed_gives_pad_and_treat_but_never_cat_or_bird_items()
    {
        var w = World();
        var s = By(w, "جای خواب مخصوص سگ سایز XL");
        var names = Names(w, ProductRelationEngine.BoughtTogether(s, Pool(w, s, true), new Dictionary<long, int>(), 8));
        Assert.Contains(names, n => n.StartsWith("پد"));
        Assert.Contains(names, n => n.StartsWith("تشویقی سگ"));
        Assert.DoesNotContain(names, n => n.Contains("گربه") && !n.Contains("سگ و گربه"));
        Assert.DoesNotContain(names, n => n.Contains("پرندگان") || n.Contains("جای خواب") || n.Contains("اسکرچر"));
    }

    [Fact]
    public void BoughtTogether_pad_is_never_suggested_for_cats()
    {
        var w = World();
        var s = By(w, "جای خواب SHUTTEL");
        var names = Names(w, ProductRelationEngine.BoughtTogether(s, Pool(w, s, true), new Dictionary<long, int>(), 8));
        Assert.DoesNotContain(names, n => n.StartsWith("پد"));
        Assert.Contains(names, n => n.StartsWith("تشویقی گربه"));
    }

    [Fact]
    public void BoughtTogether_hides_unavailable_and_unknown_kind_products()
    {
        var w = World();
        var s = By(w, "خاک گربه میوکا");
        By(w, "ظرف خاک مسقف").StatusId = 7;
        By(w, "ظرف خاک مسقف").Price = 0;
        var ids = ProductRelationEngine.BoughtTogether(s, Pool(w, s, true), new Dictionary<long, int>(), 8);
        Assert.DoesNotContain("ظرف خاک مسقف میشا با بیلچه", Names(w, ids));
    }

    [Fact]
    public void BoughtTogether_unknown_source_kind_needs_real_purchase_evidence()
    {
        var w = World();
        var unknown = P("کالای ناشناخته مدل X", CatCare, "گربه", 10107, 9);
        w.Add(unknown);
        Assert.Empty(ProductRelationEngine.BoughtTogether(unknown, Pool(w, unknown, true), new Dictionary<long, int>(), 8));
        var scoop = By(w, "بیلچه خاک گربه");
        var co = new Dictionary<long, int> { [scoop.Id] = 4 };
        Assert.Equal(new[] { scoop.Id }, ProductRelationEngine.BoughtTogether(unknown, Pool(w, unknown, true), co, 8));
    }

    [Theory]
    [InlineData("خاک گربه میوکا", "litter")]
    [InlineData("ظرف خاک گربه مدل X", "litter_box")]
    [InlineData("مت ظرف خاک گربه", "litter_mat")]
    [InlineData("بیلچه خاک گربه هپی پت", "litter_scoop")]
    [InlineData("پد بهداشتی سگ", "pad")]
    [InlineData("پد انگشتی گوش GOLD PAD", "wipes")]
    [InlineData("شیر خشک توله سگ", "milk")]
    [InlineData("غذای خشک سگ رویال کنین", "dry_food")]
    [InlineData("حجم 250 میلی لیتر شامپو سگ", "shampoo")]
    [InlineData("جلیقه جین زنجیر دار بیلی لند", "clothes")]
    [InlineData("اسباب بازی توپ تنیس بند دار", "toy")]
    [InlineData("تشویقی قطره ای ماست", "treat")]
    public void Kind_detection_on_real_world_names(string name, string kind)
    {
        Assert.Equal(kind, ProductRelationEngine.KindOfText(name));
    }

    [Fact]
    public void Flavor_words_do_not_change_species()
    {
        var food = ProductRelationEngine.Profile(new ProductCandidate { Name = "غذای خشک سگ طعم میکس گوشت پرندگان", LeafName = "غذای خشک سگ", ParentName = "سگ" });
        Assert.Equal(PetSpecies.Dog, food.Species);
        var cat = ProductRelationEngine.Profile(new ProductCandidate { Name = "پوچ گربه عقیم‌شده مونژه ماهی قزل‌آلا", LeafName = "غذای مرطوب", ParentName = "گربه" });
        Assert.Equal(PetSpecies.Cat, cat.Species);
    }

    [Fact]
    public void Relation_queries_translate_to_sql()
    {
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseSqlServer("Server=.;Database=none;Encrypt=false", o => o.UseNetTopologySuite())
            .Options;
        using var context = new DataBaseContext(options);
        long? leafId = 10107, parentId = 9;
        string w0 = "گربه", w1 = null;

        var pool = context.Products.AsNoTracking()
            .Where(p => !p.Deleted && p.Active && p.CategoryId != null && p.StatusId != 4 && p.Id != 5)
            .Where(p => p.CategoryId == leafId || (parentId != null && p.Category.ParentId == parentId) || (w0 != null && p.Category.Name.Contains(w0)) || (w1 != null && p.Category.Name.Contains(w1)))
            .Select(p => new ProductCandidate { Id = p.Id, Name = p.Name, SecondName = p.SecondName, CategoryId = p.CategoryId, BrandId = p.BrandId, Price = p.Price, StatusId = p.StatusId, SellCount = p.SellCount, RateAvg = p.RateAvg, LeafName = p.Category.Name, ParentCategoryId = p.Category.ParentId, ParentName = p.Category.Parent.Name })
            .Take(4000).ToQueryString();
        Assert.Contains("SELECT TOP", pool);

        var canceled = "ProductOrderState_Canceled";
        var orders = context.ProductOrderItems.AsNoTracking()
            .Where(i => !i.Deleted && i.ProductItem.ProductId == 5 && !i.ProductOrderStore.Deleted && !i.ProductOrderStore.ProductOrder.Deleted
                && i.ProductOrderStore.ProductOrder.IsPaid && i.ProductOrderStore.ProductOrder.ProductOrderState.Label != canceled)
            .Select(i => i.ProductOrderStore.ProductOrderId).Distinct().Take(3000).ToQueryString();
        Assert.Contains("DISTINCT", orders);

        var ids = new List<string> { "a", "b" };
        var pairs = context.ProductOrderItems.AsNoTracking()
            .Where(i => !i.Deleted && !i.ProductOrderStore.Deleted && ids.Contains(i.ProductOrderStore.ProductOrderId) && i.ProductItem.ProductId != 5)
            .Select(i => new { OrderId = i.ProductOrderStore.ProductOrderId, i.ProductItem.ProductId }).Distinct().ToQueryString();
        Assert.Contains("DISTINCT", pairs);
    }
    private static List<string> CartNames(List<ProductCandidate> w, IEnumerable<ProductCandidate> cart, bool wide = true, IReadOnlyDictionary<long, int> co = null, int take = 8)
    {
        var cartList = cart.ToList();
        var pool = w.Where(p => cartList.All(c => c.Id != p.Id)).ToList();
        return ProductRelationEngine.CartSuggestions(cartList, pool, co ?? new Dictionary<long, int>(), take).Select(x => w.First(p => p.Id == x.ProductId).Name).ToList();
    }

    [Fact]
    public void Cart_with_cat_litter_suggests_litter_accessories_not_a_second_litter_or_dog_items()
    {
        var w = World();
        var names = CartNames(w, new[] { By(w, "خاک گربه میوکا") });
        Assert.Contains("ظرف خاک مسقف میشا با بیلچه", names);
        Assert.Contains("بیلچه خاک گربه هپی پت", names);
        Assert.DoesNotContain(names, n => n.StartsWith("خاک گربه"));      // خود خاک را دارد
        Assert.DoesNotContain(names, n => n.Contains("سگ") && !n.Contains("سگ و گربه"));
        Assert.DoesNotContain(names, n => n.Contains("پرندگان") || n.StartsWith("پد"));
    }

    [Fact]
    public void Cart_does_not_suggest_a_kind_it_already_has()
    {
        var w = World();
        var names = CartNames(w, new[] { By(w, "خاک گربه میوکا"), By(w, "ظرف خاک مسقف") });
        Assert.DoesNotContain(names, n => n.StartsWith("ظرف خاک"));
        Assert.Contains("بیلچه خاک گربه هپی پت", names);
    }

    [Fact]
    public void Cart_with_dog_bed_and_dog_treat_gets_pad_and_toy_and_no_cat_items()
    {
        var w = World();
        var names = CartNames(w, new[] { By(w, "جای خواب مخصوص سگ سایز XL"), By(w, "تشویقی سگ ونپی") });
        Assert.Contains(names, n => n.StartsWith("پد"));
        Assert.Contains(names, n => n.StartsWith("اسباب بازی"));
        Assert.DoesNotContain(names, n => n.Contains("گربه") && !n.Contains("سگ و گربه"));
    }

    [Fact]
    public void Cart_suggestion_needed_by_several_items_ranks_first()
    {
        var w = World();
        // ظرف غذا را هم جای خواب ندارد و هم تشویقی/غذای خشک می‌خواهد؛ غذای خشک و تشویقی هر دو «ظرف» را نامزد می‌کنند
        var cart = new[] { By(w, "غذای خشک سگ رویال"), By(w, "تشویقی سگ ونپی") };
        var picks = ProductRelationEngine.CartSuggestions(cart, w.Where(p => cart.All(c => c.Id != p.Id)).ToList(), new Dictionary<long, int>(), 8);
        Assert.Equal("bowl", picks[0].Kind);
    }

    [Fact]
    public void Cart_suggestions_skip_unavailable_overpriced_and_unknown()
    {
        var w = World();
        var litter = By(w, "خاک گربه میوکا"); litter.Price = 100000;
        By(w, "ظرف خاک مسقف").Price = 900000;                      // بیش از ۴ برابر سبد
        By(w, "بیلچه خاک گربه").StatusId = 7;                      // ناموجود
        var names = CartNames(w, new[] { litter });
        Assert.DoesNotContain("ظرف خاک مسقف میشا با بیلچه", names);
        Assert.DoesNotContain("بیلچه خاک گربه هپی پت", names);
        Assert.Empty(ProductRelationEngine.CartSuggestions(new List<ProductCandidate>(), w, new Dictionary<long, int>(), 8));
    }

    [Fact]
    public void Cart_unknown_kind_items_only_suggest_with_strong_purchase_evidence()
    {
        var w = World();
        var unknown = P("کالای ناشناخته مدل X", CatCare, "گربه", 10107, 9);
        var scoop = By(w, "بیلچه خاک گربه");
        Assert.Empty(CartNames(w, new[] { unknown }));
        Assert.Empty(CartNames(w, new[] { unknown }, co: new Dictionary<long, int> { [scoop.Id] = 3 }));
        Assert.Equal(new[] { scoop.Name }, CartNames(w, new[] { unknown }, co: new Dictionary<long, int> { [scoop.Id] = 4 }));
    }

    [Fact]
    public void Cart_queries_translate_to_sql()
    {
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseSqlServer("Server=.;Database=none;Encrypt=false", o => o.UseNetTopologySuite())
            .Options;
        using var context = new DataBaseContext(options);
        long? userId = 5; string uniqueId = "x";
        var ids = context.CartItems.AsNoTracking()
            .Where(i => userId.HasValue ? i.CartStore.Cart.UserId == userId : i.CartStore.Cart.UniqueId == uniqueId)
            .Select(i => i.ProductItem.ProductId).Distinct().Take(50).ToQueryString();
        Assert.Contains("DISTINCT", ids);

        var productIds = new List<long> { 1, 2 }; var parentIds = new List<long> { 9 };
        string w0 = "گربه", w1 = null;
        var pool = context.Products.AsNoTracking()
            .Where(p => !p.Deleted && p.Active && p.CategoryId != null && !productIds.Contains(p.Id)
                && ((p.Category.ParentId != null && parentIds.Contains(p.Category.ParentId.Value)) || (w0 != null && p.Category.Name.Contains(w0)) || (w1 != null && p.Category.Name.Contains(w1))))
            .Select(p => new ProductCandidate { Id = p.Id, Name = p.Name, LeafName = p.Category.Name, ParentCategoryId = p.Category.ParentId, ParentName = p.Category.Parent.Name })
            .Take(4000).ToQueryString();
        Assert.Contains("SELECT TOP", pool);

        var orders = context.ProductOrderItems.AsNoTracking()
            .Where(i => !i.Deleted && productIds.Contains(i.ProductItem.ProductId) && !i.ProductOrderStore.Deleted)
            .Select(i => i.ProductOrderStore.ProductOrderId).Distinct().Take(3000).ToQueryString();
        Assert.Contains("DISTINCT", orders);
    }
    [Fact]
    public void Cart_items_of_other_stores_do_not_nominate_but_block_duplicates()
    {
        var w = World();
        var litter = By(w, "خاک گربه میوکا");
        var otherStoreBox = By(w, "ظرف خاک مسقف");
        var pool = w.Where(p => p.Id != litter.Id && p.Id != otherStoreBox.Id).ToList();
        var picks = ProductRelationEngine.CartSuggestions(new[] { litter }, pool, new Dictionary<long, int>(), 8, new[] { litter, otherStoreBox });
        Assert.DoesNotContain(picks, x => x.Kind == "litter_box");
        Assert.Contains(picks, x => x.Kind == "litter_scoop");
        Assert.All(picks, x => Assert.Equal(litter.Id, x.ForProductId));
    }

    [Fact]
    public void Store_filtered_pool_query_translates_to_sql()
    {
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseSqlServer("Server=.;Database=none;Encrypt=false", o => o.UseNetTopologySuite())
            .Options;
        using var context = new DataBaseContext(options);
        long sid = 3;
        var sql = context.Products.AsNoTracking()
            .Where(p => p.ProductItems.Any(x => x.StoreId == sid && x.Active && x.SystemActive && !x.Deleted && x.Quantity > 0 && x.Store.Active && !x.Store.Deleted))
            .Select(p => p.Id).ToQueryString();
        Assert.Contains("EXISTS", sql);
        var rows = context.CartItems.AsNoTracking().Where(i => i.CartStore.Cart.UserId == 5)
            .Select(i => new { i.ProductItem.ProductId, i.CartStore.StoreId }).Distinct().Take(100).ToQueryString();
        Assert.Contains("DISTINCT", rows);
    }
}
