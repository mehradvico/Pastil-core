using Application.Services.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductItemSrv;
using Application.Services.ProductSrvs.ProductSrv;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Tests.Clinic;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using Xunit;

namespace Application.Tests;

// صفحه‌ی کالا: همه‌ی فروشگاه‌های دارای کالا از ارزان به گران + پیش‌فرضِ فروشگاهی که کاربر از صفحه‌ی آن آمده؛
// صفحه‌ی فروشگاه: قیمت هر کارت = قیمتِ همان فروشگاه.
public class ProductStoreOffersTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;
    public ProductStoreOffersTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    private static T Stub<T>() where T : class => DispatchProxy.Create<T, StoreCommentVisibilityTests.NullProxy>();

    private record World(long ProductId, long CheapStore, long MidStore, long PriceyStore, long StockoutStore, long InactiveStore);

    private World Seed()
    {
        using var c = _fixture.NewContext();
        var tag = Random.Shared.NextInt64(1_000_000, 9_999_999).ToString();
        var storeType = new Code { Name = "st" + tag, Label = "StoreType_Test", Active = true };
        c.Codes.Add(storeType);
        c.SaveChanges();
        Store NewStore(string name, bool active) => new() { Name = name + tag, ReferralCode = Guid.NewGuid().ToString("N")[..8], Active = active, TypeId = storeType.Id };
        var cheap = NewStore("cheap", true); var mid = NewStore("mid", true); var pricey = NewStore("pricey", true);
        var stockout = NewStore("stockout", true); var inactive = NewStore("inactive", false);
        c.Stores.AddRange(cheap, mid, pricey, stockout, inactive);
        var available = new Code { Name = "av" + tag, Label = "ProductStatus_Available", Active = true };
        var type = new Code { Name = "ty" + tag, Label = "ProductType_Normal", Active = true };
        c.Codes.AddRange(available, type);
        c.SaveChanges();
        var product = new Product { Name = "P" + tag, ProductLabel = "p" + tag, Active = true, StatusId = available.Id, TypeId = type.Id, Price = 100, CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
        c.Products.Add(product);
        c.SaveChanges();
        ProductItem Item(Store s, long price, int qty, bool sys = true) => new()
        { ProductId = product.Id, StoreId = s.Id, BasePrice = price, Price = price, Quantity = qty, Active = true, SystemActive = sys };
        c.ProductItems.AddRange(
            Item(pricey, 900_000, 5), Item(cheap, 500_000, 5), Item(cheap, 450_000, 2),   // cheap دو آیتم دارد؛ ارزان‌ترین ۴۵۰٬۰۰۰ است
            Item(mid, 700_000, 5), Item(stockout, 300_000, 0), Item(inactive, 100_000, 5));
        c.SaveChanges();
        // EF مقدار false را با پیش‌فرضِ ستون جایگزین می‌کند؛ پس مثل procedure قیمت، بعد از درج صفر می‌شود
        c.ProductItems.Where(i => i.StoreId == stockout.Id).ExecuteUpdate(u => u.SetProperty(i => i.SystemActive, false));
        return new World(product.Id, cheap.Id, mid.Id, pricey.Id, stockout.Id, inactive.Id);
    }

    private ProductItemService ItemService() => new(_fixture.NewContext(), _fixture.Mapper,
        Stub<Application.Services.ProductSrvs.ProductSrv.Iface.IProductService>(),
        Stub<Application.Services.ProductSrvs.ProductStockAlertSrv.Iface.IProductStockAlertService>(),
        NullLogger<ProductItemService>.Instance);

    [Fact]
    public async Task Product_detail_lists_every_selling_store_cheapest_first_and_prefers_the_store_user_came_from()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var res = await ItemService().GetForProductVDto(w.ProductId, w.PriceyStore);
        Assert.True(res.IsSuccess);
        var data = res.Data;

        // آیتم‌ها از کم به زیاد؛ ناموجود/فروشگاه غیرفعال نمی‌آید
        Assert.Equal(new long[] { 450_000, 500_000, 700_000, 900_000 }, data.ProductItems.Select(i => i.Price).ToArray());
        // فروشگاه‌ها: هر فروشگاه یک‌بار با ارزان‌ترین آیتمش، از کم به زیاد
        Assert.Equal(new[] { w.CheapStore, w.MidStore, w.PriceyStore }, data.Offers.Select(o => o.StoreId).ToArray());
        Assert.Equal(450_000, data.Offers[0].Price);
        // پیش‌فرض = فروشگاهی که کاربر از صفحه‌اش آمده، با قیمت خودش (نه ارزان‌ترین)
        Assert.Equal(w.PriceyStore, data.PreferredStoreId);
        var preferred = Assert.Single(data.Offers.Where(o => o.IsPreferred));
        Assert.Equal(900_000, preferred.Price);
    }

    [Fact]
    public async Task Preferred_store_is_ignored_when_it_does_not_sell_the_product_now()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        Assert.Null((await ItemService().GetForProductVDto(w.ProductId, w.StockoutStore)).Data.PreferredStoreId);   // ناموجود
        Assert.Null((await ItemService().GetForProductVDto(w.ProductId, w.InactiveStore)).Data.PreferredStoreId);  // فروشگاه غیرفعال
        Assert.Null((await ItemService().GetForProductVDto(w.ProductId, null)).Data.PreferredStoreId);             // بدون storeId: ارزان‌ترین بالا می‌آید
        Assert.False((await ItemService().GetForProductVDto(long.MaxValue)).IsSuccess);
    }

    private ProductService ProductSvc() => new(_fixture.NewContext(),
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { ["connection"] = "Server=.;Database=none" }).Build(),
        _fixture.Mapper,
        Stub<Application.Services.CategorySrv.Iface.ICategoryService>(),
        Stub<Application.Services.ProductSrvs.ProductCategorySrv.Iface.IProductCategoryService>(),
        Stub<Application.Services.ProductSrvs.ProductPictureSrv.Iface.IProductPictureService>(),
        Stub<Application.Services.CommonSrv.SearchSrv.Iface.ISearchVocabularyService>());

    [Fact]
    public void Store_page_shows_the_stores_own_price_and_sorts_by_it_only_when_asked()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        // قیمتِ ثبت‌شده روی خودِ محصول ارزان‌ترین بین همه‌ی فروشگاه‌هاست؛ صفحه‌ی فروشگاه باید قیمتِ همان فروشگاه را نشان دهد
        using (var c = _fixture.NewContext())
            c.Products.Where(p => p.Id == w.ProductId).ExecuteUpdate(s => s.SetProperty(p => p.Price, 450_000L));

        var pricey = ProductSvc().Search(new ProductInputDto { StoreId = w.PriceyStore, ApplyStorePrice = true, Available = true, PageIndex = 1, PageSize = 20 });
        var card = Assert.Single(pricey.List);
        Assert.Equal(900_000, card.Price);
        Assert.True(card.StoreItemAvailable);

        var notApplied = ProductSvc().Search(new ProductInputDto { StoreId = w.PriceyStore, ApplyStorePrice = false, Available = true, PageIndex = 1, PageSize = 20 });
        Assert.Equal(450_000, Assert.Single(notApplied.List).Price);    // پنل فروشنده/ادمین مثل قبل
        Assert.Null(notApplied.List[0].StoreItemAvailable);

        var stockout = ProductSvc().Search(new ProductInputDto { StoreId = w.StockoutStore, ApplyStorePrice = true, Available = true, PageIndex = 1, PageSize = 20 });
        var outCard = Assert.Single(stockout.List);
        Assert.Equal(0, outCard.Price);
        Assert.False(outCard.StoreItemAvailable);
    }
}
