using Application.Services.Content.DeliverySrv;
using Application.Services.Order.DeliverySrv;
using Application.Services.Order.DeliverySrv.Dto;
using Application.Services.Setting.BaseDetailSrv.Iface;
using Application.Tests.Clinic;
using Entities.Entities;
using Entities.Entities.LocationField;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Xunit;

namespace Application.Tests;

// پست/تیپاکس/ماهکس/الوپیک: قیمت و پس‌کرایه با فروشگاه؛ الوپیک فقط تهران؛ بقیه بیرون از تهران
public class StorePricedShippingTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;
    public StorePricedShippingTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("تهران", true)]
    [InlineData(" تهران ", true)]
    [InlineData("تهران", true)]
    [InlineData("کرج", false)]
    [InlineData("شهرتهران", false)]
    [InlineData(null, false)]
    public void IsTehran_matches_only_the_city(string name, bool expected) =>
        Assert.Equal(expected, StoreShippingRules.IsTehran(name));

    [Fact]
    public void Geography_rules()
    {
        Assert.True(StoreShippingRules.AloPeykApplies(true));
        Assert.False(StoreShippingRules.AloPeykApplies(false));
        Assert.True(StoreShippingRules.NationalCarrierApplies(false, true));    // خارج از تهران همیشه
        Assert.True(StoreShippingRules.NationalCarrierApplies(true, false));    // تهران بدون الوپیکِ فروشگاه
        Assert.False(StoreShippingRules.NationalCarrierApplies(true, true));    // تهران با الوپیک فعال
        Assert.True(StoreShippingRules.IsStorePriced("DeliveryType_Mahex"));
        Assert.False(StoreShippingRules.IsStorePriced("DeliveryType_Miare"));
    }

    private DeliveryService Svc() => new(_fixture.NewContext(), _fixture.Mapper,
        DispatchProxy.Create<IBaseDetailService, StoreCommentVisibilityTests.NullProxy>());

    private record World(long Aloy, long Tipax, long Tehran, long Karaj, long State, long StoreId);

    private World Seed()
    {
        using var c = _fixture.NewContext();
        var tag = Random.Shared.NextInt64(1_000_000, 9_999_999).ToString();
        var alo = new Code { Name = "alo" + tag, Label = "DeliveryType_AloPeyk", Active = true };
        var tipax = new Code { Name = "tip" + tag, Label = "DeliveryType_Tipax", Active = true };
        var storeType = new Code { Name = "st" + tag, Label = "StoreType_Test", Active = true };
        c.Codes.AddRange(alo, tipax, storeType);
        var state = new State { Name = "تهران" };
        c.States.Add(state);
        c.SaveChanges();
        var tehran = new City { Name = "تهران", StateId = state.Id };
        var karaj = new City { Name = "کرج", StateId = state.Id };
        c.Cities.AddRange(tehran, karaj);
        var store = new Store { Name = "s" + tag, ReferralCode = Guid.NewGuid().ToString("N")[..8], Active = true, TypeId = storeType.Id };
        c.Stores.Add(store);
        c.SaveChanges();
        return new World(alo.Id, tipax.Id, tehran.Id, karaj.Id, state.Id, store.Id);
    }

    private static DeliveryDto Dto(long type, long price, bool afterRent = false, long? cityId = null) => new()
    {
        DeliveryTypeId = type, BasePrice = price, AfterRent = afterRent, AllowPrepaid = !afterRent, AllowReceiverPay = afterRent,
        CityId = cityId, Active = true
    };

    [Fact]
    public async Task Validation_requires_price_or_after_rent_and_no_live_provider()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        // بدون قیمت و بدون پس‌کرایه
        Assert.False((await Svc().InsertForStoreAsync(Dto(w.Tipax, 0), w.StoreId)).IsSuccess);
        // قیمت لحظه‌ای/ارائه‌دهنده برای روش‌های قیمت‌گذاری فروشگاهی ممنوع
        var live = Dto(w.Tipax, 50_000); live.LivePricing = true; live.ShippingProvider = Entities.Entities.ShippingField.ShippingProviderEnum.AloPeyk;
        Assert.False((await Svc().InsertForStoreAsync(live, w.StoreId)).IsSuccess);
        // با قیمت ثابت OK؛ پس‌کرایه بدون قیمت هم OK
        Assert.True((await Svc().InsertForStoreAsync(Dto(w.Tipax, 50_000), w.StoreId)).IsSuccess);
        var after = await Svc().InsertForStoreAsync(Dto(w.Tipax, 0, afterRent: true), w.StoreId);
        Assert.True(after.IsSuccess);
    }

    [Fact]
    public async Task AloPeyk_can_only_be_defined_for_tehran()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        Assert.False((await Svc().InsertForStoreAsync(Dto(w.Aloy, 80_000, cityId: w.Karaj), w.StoreId)).IsSuccess);
        Assert.True((await Svc().InsertForStoreAsync(Dto(w.Aloy, 80_000, cityId: w.Tehran), w.StoreId)).IsSuccess);
    }

    [Fact]
    public void Quotes_follow_the_destination_city()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        using var c = _fixture.NewContext();
        var aloDelivery = new Delivery { StoreId = w.StoreId, DeliveryTypeId = w.Aloy, BasePrice = 80_000, Active = true, AllowPrepaid = true, DeliveryType = new Code { Label = "DeliveryType_AloPeyk", Name = "x" } };
        var tipaxDelivery = new Delivery { StoreId = w.StoreId, DeliveryTypeId = w.Tipax, BasePrice = 50_000, Active = true, AllowPrepaid = true, DeliveryType = new Code { Label = "DeliveryType_Tipax", Name = "y" } };
        c.Deliveries.Add(new Delivery { StoreId = w.StoreId, DeliveryTypeId = w.Aloy, BasePrice = 80_000, Active = true, AllowPrepaid = true });
        c.SaveChanges();

        Cart CartTo(string city) => new()
        {
            Address = new Address { City = new City { Name = city } },
            CartStores = new List<CartStore> { new() { StoreId = w.StoreId, Price = 100 } }
        };

        var svc = new DeliveryService(_fixture.NewContext(), _fixture.Mapper, DispatchProxy.Create<IBaseDetailService, StoreCommentVisibilityTests.NullProxy>());
        // تهران: الوپیک هست؛ تیپاکس چون فروشگاه الوپیک فعال دارد پنهان است
        Assert.NotNull(svc.GetDelivery(CartTo("تهران"), aloDelivery, w.StoreId));
        Assert.Null(svc.GetDelivery(CartTo("تهران"), tipaxDelivery, w.StoreId));
        // کرج: الوپیک نه؛ تیپاکس بله با قیمتی که فروشگاه گذاشته
        Assert.Null(svc.GetDelivery(CartTo("کرج"), aloDelivery, w.StoreId));
        Assert.Equal(50_000, svc.GetDelivery(CartTo("کرج"), tipaxDelivery, w.StoreId).DeliveryPrice);
    }
}
