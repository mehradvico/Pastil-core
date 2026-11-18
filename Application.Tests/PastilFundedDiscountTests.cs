using Application.Services.Order.ProductOrderSrv;
using Application.Services.ProductSrvs.DiscountSrv;
using Application.Services.ProductSrvs.DiscountSrv.Dto;
using Application.Tests.Clinic;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Migrations;
using System.Reflection;
using Xunit;

namespace Application.Tests;

// تخفیف محصولِ دو‌لایه: سهم فروشگاه (هر درصد) + سهم پاستیل (≤ کمیسیون). حسابداری سفارش، procedure قیمت و اعتبارسنجی ادمین.
public class PastilFundedDiscountTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;
    public PastilFundedDiscountTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    // ───────────── قواعد خالص حسابداری ─────────────

    [Fact]
    public void Without_pastil_discount_the_split_is_the_old_commission_on_payment()
    {
        var (store, site) = ProductOrderCommissionRules.Split(900_000m, 0m, 10m);
        Assert.Equal(810_000m, store);
        Assert.Equal(90_000m, site);
    }

    [Fact]
    public void Pastil_shares_the_discount_it_funds_out_of_its_own_commission()
    {
        // کالای ۱٬۰۰۰٬۰۰۰، تخفیف ۱۰٪ که ۵٪ آن با پاستیل است، کمیسیون ۱۰٪: مشتری ۹۰۰٬۰۰۰ می‌دهد
        var (store, site) = ProductOrderCommissionRules.Split(900_000m, 50_000m, 10m);
        Assert.Equal(855_000m, store);   // (۹۰۰٬۰۰۰ + ۵۰٬۰۰۰) × ۹۰٪
        Assert.Equal(45_000m, site);     // ۹۵٬۰۰۰ کمیسیون − ۵۰٬۰۰۰ تخفیف
    }

    [Fact]
    public void Pastil_funding_a_discount_equal_to_its_commission_nets_zero_and_the_store_is_untouched()
    {
        // کمیسیون ۸٪، پاستیل ۸٪ تخفیف می‌دهد (فروشگاه تخفیفی ندارد): فروشگاه همان مبلغِ بدون تخفیف را می‌گیرد
        var (store, site) = ProductOrderCommissionRules.Split(920_000m, 80_000m, 8m);
        Assert.Equal(920_000m, store);
        Assert.Equal(0m, site);
    }

    [Fact]
    public void Store_discount_and_pastil_discount_are_independent_layers()
    {
        // پایه ۱٬۰۰۰٬۰۰۰ | فروشگاه ۱۵٪ ⇒ ۸۵۰٬۰۰۰ | پاستیل ۸٪ از ۸۵۰٬۰۰۰ = ۶۸٬۰۰۰ ⇒ مشتری ۷۸۲٬۰۰۰ | کمیسیون ۸٪
        var (store, site) = ProductOrderCommissionRules.Split(782_000m, 68_000m, 8m);
        Assert.Equal(782_000m, store);    // فروشگاه = قیمتِ بعد از تخفیف خودش منهای کمیسیون (۸۵۰٬۰۰۰ × ۹۲٪)
        Assert.Equal(0m, site);           // پاستیل به‌جای کمیسیونش تخفیف داده

        // پاستیل کمتر از کمیسیون (۴٪ = ۳۴٬۰۰۰): سهم خالص پاستیل = (۸٪ − ۴٪) × ۸۵۰٬۰۰۰
        var (store2, site2) = ProductOrderCommissionRules.Split(816_000m, 34_000m, 8m);
        Assert.Equal(782_000m, store2);
        Assert.Equal(34_000m, site2);
    }

    [Fact]
    public void Order_totals_use_remaining_items_only_and_skip_unknown_stores()
    {
        var stores = new List<ProductOrderStore>
        {
            new()
            {
                PaymentPrice = 782_000, Store = new Store { CommissionPercent = 8 },
                ProductOrderItems = new List<ProductOrderItem>
                {
                    new() { Count = 1, PastilDiscountUnit = 68_000 },
                    new() { Count = 3, PastilDiscountUnit = 68_000, Deleted = true },   // حذف‌شده حساب نمی‌شود
                },
            },
            new() { PaymentPrice = 500_000, Store = null },
            new() { PaymentPrice = 100_000, Store = new Store { CommissionPercent = 150 } },   // کمیسیون نامعتبر
        };
        var (storeShare, siteShare, funded) = ProductOrderCommissionRules.ForOrder(stores);
        Assert.Equal(68_000m, funded);
        Assert.Equal(782_000m, storeShare + siteShare);                // مجموع همیشه برابر پرداخت مشتری
    }

    // ───────────── procedure قیمت (LocalDB) ─────────────

    private void InstallProcedure()
    {
        using var c = _fixture.NewContext();
        c.Database.ExecuteSqlRaw("IF OBJECT_ID('dbo.UpdateProductPrice') IS NULL EXEC('CREATE PROCEDURE dbo.UpdateProductPrice @FilterType nvarchar(50), @FilterIds nvarchar(max) AS BEGIN SET NOCOUNT ON; END')");
        c.Database.ExecuteSqlRaw(PastilDiscountProcedure.Sql);
    }

    private (long StoreId, long ProductId, long ItemId) SeedItem(long basePrice = 1_000_000)
    {
        using var c = _fixture.NewContext();
        var tag = Random.Shared.NextInt64(1_000_000, 9_999_999).ToString();
        var store = new Store { Name = "S" + tag, ReferralCode = "R" + tag, Active = true, CommissionPercent = 8 };
        c.Stores.Add(store);
        var product = new Product { Name = "P" + tag, ProductLabel = "p" + tag, Active = true, CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
        c.Products.Add(product);
        c.SaveChanges();
        var item = new ProductItem { ProductId = product.Id, StoreId = store.Id, Active = true, SystemActive = true, Quantity = 5, BasePrice = basePrice, Price = basePrice };
        c.ProductItems.Add(item);
        c.SaveChanges();
        return (store.Id, product.Id, item.Id);
    }

    private void AddDiscount(long storeId, long productId, int percent, bool pastil, int endsInDays = 10)
    {
        using var c = _fixture.NewContext();
        c.Discounts.Add(new Discount { StoreId = storeId, ProductId = productId, Percent = percent, FundedByPastil = pastil, Active = true, EndDate = DateTime.Now.Date.AddDays(endsInDays) });
        c.SaveChanges();
    }

    private void AddStoreWideDiscount(long storeId, int percent, bool pastil, DateTime? endDate)
    {
        using var c = _fixture.NewContext();
        c.Discounts.Add(new Discount { StoreId = storeId, Percent = percent, FundedByPastil = pastil, Active = true, EndDate = endDate });
        c.SaveChanges();
    }

    private T Scalar<T>(string column, long itemId)
    {
        using var c = _fixture.NewContext();
        return c.Database.SqlQueryRaw<T>("SELECT " + column + " AS [Value] FROM ProductItems WHERE Id = {0}", itemId).AsEnumerable().Single();
    }

    private (int Percent, int Pastil, long Price) Run(long productId, long itemId)
    {
        InstallProcedure();
        using (var c = _fixture.NewContext())
            c.Database.ExecuteSqlRaw("EXEC dbo.UpdateProductItemDiscount @FilterType = N'Product', @FilterIds = {0}", productId.ToString());
        return (Scalar<int>("DiscountPercent", itemId), Scalar<int>("PastilDiscountPercent", itemId), Scalar<long>("Price", itemId));
    }

    [Fact]
    public void Procedure_without_discounts_or_with_store_only_behaves_like_before()
    {
        if (!_fixture.Available) return;
        var none = SeedItem();
        Assert.Equal((0, 0, 1_000_000L), Run(none.ProductId, none.ItemId));

        var storeOnly = SeedItem();
        AddDiscount(storeOnly.StoreId, storeOnly.ProductId, 15, pastil: false);
        AddDiscount(storeOnly.StoreId, storeOnly.ProductId, 10, pastil: false);   // بیشترین برنده است (مثل قبل)
        Assert.Equal((15, 0, 850_000L), Run(storeOnly.ProductId, storeOnly.ItemId));
    }

    [Fact]
    public void Procedure_applies_store_wide_discount_to_every_item_of_the_store_even_without_end_date()
    {
        if (!_fixture.Available) return;
        var shop = SeedItem();
        AddStoreWideDiscount(shop.StoreId, 20, pastil: false, endDate: DateTime.Now.Date.AddDays(10));
        AddStoreWideDiscount(shop.StoreId, 5, pastil: true, endDate: null);   // بدون تاریخ انقضا
        Assert.Equal((25, 5, 750_000L), Run(shop.ProductId, shop.ItemId));      // ۲۰٪ + ۵٪ = ۲۵٪ (جمعی)
    }

    [Fact]
    public void Procedure_restores_base_price_when_store_wide_discounts_are_deleted_or_deactivated()
    {
        if (!_fixture.Available) return;
        var shop = SeedItem();
        AddStoreWideDiscount(shop.StoreId, 20, pastil: false, endDate: null);
        AddStoreWideDiscount(shop.StoreId, 5, pastil: true, endDate: null);
        Assert.Equal((25, 5, 750_000L), Run(shop.ProductId, shop.ItemId));

        using (var c = _fixture.NewContext())
            c.Database.ExecuteSqlRaw("UPDATE Discounts SET Deleted = 1 WHERE StoreId = {0} AND FundedByPastil = 0", shop.StoreId);
        Assert.Equal((5, 5, 950_000L), Run(shop.ProductId, shop.ItemId));      // فقط سهم پاستیل مانده

        using (var c = _fixture.NewContext())
            c.Database.ExecuteSqlRaw("UPDATE Discounts SET Active = 0 WHERE StoreId = {0}", shop.StoreId);
        Assert.Equal((0, 0, 1_000_000L), Run(shop.ProductId, shop.ItemId));    // همه برداشته شد → قیمت پایه
        Assert.Equal(0d, Scalar<double>("PastilDiscountAmount", shop.ItemId));
    }

    [Fact]
    public void Procedure_adds_store_and_pastil_layers_independently()
    {
        if (!_fixture.Available) return;
        var both = SeedItem();
        AddDiscount(both.StoreId, both.ProductId, 15, pastil: false);
        AddDiscount(both.StoreId, both.ProductId, 8, pastil: true);
        AddDiscount(both.StoreId, both.ProductId, 3, pastil: true);               // کمتر از ۸ نادیده گرفته می‌شود
        Assert.Equal((23, 8, 770_000L), Run(both.ProductId, both.ItemId));            // ۱۵٪ + ۸٪ = ۲۳٪ (جمعی)
        Assert.Equal(80_000d, Scalar<double>("PastilDiscountAmount", both.ItemId));   // ۸٪ از قیمت پایه

        var pastilOnly = SeedItem();
        AddDiscount(pastilOnly.StoreId, pastilOnly.ProductId, 8, pastil: true);
        Assert.Equal((8, 8, 920_000L), Run(pastilOnly.ProductId, pastilOnly.ItemId));

        var deep = SeedItem();
        AddDiscount(deep.StoreId, deep.ProductId, 95, pastil: false);
        AddDiscount(deep.StoreId, deep.ProductId, 8, pastil: true);
        Assert.Equal((100, 8, 0L), Run(deep.ProductId, deep.ItemId));          // ۹۵٪ + ۸٪ سقف ۱۰۰٪ می‌شود
    }

    [Fact]
    public void Procedure_ends_the_layered_discount_at_the_earliest_end_date()
    {
        if (!_fixture.Available) return;
        var w = SeedItem();
        AddDiscount(w.StoreId, w.ProductId, 15, pastil: false, endsInDays: 20);
        AddDiscount(w.StoreId, w.ProductId, 8, pastil: true, endsInDays: 5);
        Run(w.ProductId, w.ItemId);
        Assert.Equal(DateTime.Now.Date.AddDays(5), Scalar<DateTime>("DiscountEndDate", w.ItemId));
    }

    // ───────────── اعتبارسنجی DiscountService ─────────────

    public class Fake : DispatchProxy
    {
        public Func<MethodInfo, object[], object> Handler;
        protected override object Invoke(MethodInfo method, object[] args)
        {
            var value = Handler?.Invoke(method, args);
            var type = method.ReturnType;
            if (type == typeof(Task)) return Task.CompletedTask;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var inner = type.GetGenericArguments()[0];
                var result = value ?? (inner.IsValueType ? Activator.CreateInstance(inner) : null);
                return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(inner).Invoke(null, new[] { result })!;
            }
            return value ?? (type.IsValueType && type != typeof(void) ? Activator.CreateInstance(type) : null);
        }
    }

    private static T Stub<T>(Func<MethodInfo, object[], object> handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, Fake>();
        ((Fake)(object)proxy).Handler = handler;
        return proxy;
    }

    private DiscountService NewDiscountService()
    {
        var code = Stub<Application.Services.Setting.CodeSrv.Iface.ICodeService>((m, a) =>
            m.Name == "GetByIdAsync" ? new Application.Services.Setting.CodeSrv.Dto.CodeDto { Id = (long)a[0], Label = "DiscountType_Product", Name = "Product" } : null);
        return new DiscountService(_fixture.NewContext(), _fixture.Mapper,
            Stub<Application.Services.StoreSrv.Iface.IStoreService>(),
            code,
            Stub<Application.Services.ProductSrvs.ProductSrv.Iface.IProductService>());
    }

    private T DiscountScalar<T>(string column, long storeId, string where = "1=1")
    {
        using var c = _fixture.NewContext();
        return c.Database.SqlQueryRaw<T>("SELECT TOP 1 " + column + " AS [Value] FROM Discounts WHERE StoreId = {0} AND " + where + " ORDER BY Id", storeId).AsEnumerable().Single();
    }

    private long TypeId()
    {
        using var c = _fixture.NewContext();
        var code = new Code { Name = "T" + Guid.NewGuid().ToString("N"), Label = "DiscountType_Product", Active = true };
        c.Codes.Add(code);
        c.SaveChanges();
        return code.Id;
    }

    private DiscountDto Dto(long storeId, long productId, int percent, bool pastil) =>
        new() { StoreId = storeId, ProductId = productId, TypeId = TypeId(), Percent = percent, FundedByPastil = pastil, Active = true, EndDate = DateTime.Now.Date.AddDays(10), DiscountGroupId = 1 };

    [Fact]
    public async Task Admin_can_add_a_pastil_discount_only_up_to_the_stores_commission()
    {
        if (!_fixture.Available) return;
        var w = SeedItem();   // کمیسیون ۸٪
        Assert.False((await NewDiscountService().InsertAsync(Dto(w.StoreId, w.ProductId, 9, pastil: true), byAdmin: true)).IsSuccess);
        var ok8 = await NewDiscountService().InsertAsync(Dto(w.StoreId, w.ProductId, 8, pastil: true), byAdmin: true);
        Assert.True(ok8.IsSuccess, string.Join("|", ok8.Messages?.Select(m => m.Item1 + ":" + m.Item2) ?? new string[0]));
        Assert.True(DiscountScalar<bool>("FundedByPastil", w.StoreId));
        Assert.Equal(8, DiscountScalar<int>("[Percent]", w.StoreId));
    }

    [Fact]
    public async Task Sellers_can_never_create_or_touch_a_pastil_discount_and_their_own_discount_is_unlimited()
    {
        if (!_fixture.Available) return;
        var w = SeedItem();
        // فروشنده FundedByPastil=true هم بفرستد، تخفیفِ خودش (هزینه‌ی فروشگاه) ثبت می‌شود
        Assert.True((await NewDiscountService().InsertAsync(Dto(w.StoreId, w.ProductId, 40, pastil: true), byAdmin: false)).IsSuccess);
        Assert.False(DiscountScalar<bool>("FundedByPastil", w.StoreId));
        Assert.Equal(40, DiscountScalar<int>("[Percent]", w.StoreId));
        await NewDiscountService().InsertAsync(Dto(w.StoreId, w.ProductId, 5, pastil: true), byAdmin: true);
        var pastilId = DiscountScalar<long>("Id", w.StoreId, "FundedByPastil = 1");
        Assert.False((await NewDiscountService().ActiveAsync(new DiscountDto { Id = pastilId, StoreId = w.StoreId, Active = false }, byAdmin: false)).IsSuccess);
        Assert.False((await NewDiscountService().DeleteAsync(new DiscountDto { Id = pastilId, StoreId = w.StoreId }, byAdmin: false)).IsSuccess);
        Assert.True((await NewDiscountService().ActiveAsync(new DiscountDto { Id = pastilId, StoreId = w.StoreId, Active = false }, byAdmin: true)).IsSuccess);
    }

    [Fact]
    public void Order_item_snapshots_the_pastil_discount_of_the_product_item()
    {
        var cartItem = new CartItem { Count = 2, ProductItem = new ProductItem { BasePrice = 1_000_000, Price = 782_000, DiscountPercent = 22, PastilDiscountPercent = 8, PastilDiscountAmount = 68_000 } };
        var dto = _fixture.Mapper.Map<Application.Services.Order.ProductOrderItemSrv.Dto.ProductOrderItemDto>(cartItem);
        Assert.Equal(68_000d, dto.PastilDiscountUnit);
        var entity = _fixture.Mapper.Map<ProductOrderItem>(dto);
        Assert.Equal(68_000d, entity.PastilDiscountUnit);
        Assert.Equal(136_000m, ProductOrderCommissionRules.PastilFunded(new[] { entity }));   // × تعداد
    }

    [Fact]
    public void Delivery_belongs_entirely_to_pastil_and_does_not_change_store_share()
    {
        var items = new List<ProductOrderItem> { new() { Count = 1, PastilDiscountUnit = 68_000 } };
        Assert.Equal(68_000m, ProductOrderCommissionRules.PastilFunded(items));      // تخفیف فقط از آیتم‌های محصول است، نه ارسال

        var (storeNoShip, siteNoShip) = ProductOrderCommissionRules.Split(782_000m, 68_000m, 8m);
        var (storeShip, siteShip) = ProductOrderCommissionRules.Split(782_000m + 50_000m, 68_000m, 8m, 50_000m);
        Assert.Equal(storeNoShip, storeShip);
        Assert.Equal(50_000m, siteShip - siteNoShip);
    }
}
