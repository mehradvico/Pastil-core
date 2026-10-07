using Application.Services.Order.ShippingSrv;
using Xunit;

namespace Application.Tests;

public class ShippingGeoRulesTests
{
    [Fact]
    public void Tehran_east_to_west_is_inside_the_default_miare_range()
    {
        // تهران‌پارس ← صادقیه: حدود ۲۰ تا ۲۵ کیلومتر هوایی
        var km = ShippingGeoRules.DistanceKm(35.7380, 51.5200, 35.7000, 51.3300);
        Assert.InRange(km, 15, 30);
        Assert.True(ShippingGeoRules.IsWithinRange(km, 45));
    }

    [Fact]
    public void Tehran_to_shiraz_is_far_outside_the_range()
    {
        var km = ShippingGeoRules.DistanceKm(35.7219, 51.4090, 29.6100, 52.5300);
        Assert.InRange(km, 650, 720);
        Assert.False(ShippingGeoRules.IsWithinRange(km, 45));
    }

    [Fact]
    public void A_non_positive_limit_means_unlimited()
    {
        Assert.True(ShippingGeoRules.IsWithinRange(5000, 0));
        Assert.True(ShippingGeoRules.IsWithinRange(5000, -1));
    }

    [Fact]
    public void Same_point_is_zero()
        => Assert.Equal(0, ShippingGeoRules.DistanceKm(35.7, 51.4, 35.7, 51.4), 6);
}
