using Application.Services.Order.ShippingSrv;
using System;
using System.Collections.Generic;
using Xunit;

namespace Application.Tests;

public class ShippingExpressRulesTests
{
    // ۱۲:۰۰ تهران = ۰۸:۳۰ UTC
    private static DateTime TehranToUtc(int hour, int minute = 0) => new DateTime(2026, 10, 10, hour, minute, 0, DateTimeKind.Utc).AddMinutes(-210);

    [Theory]
    [InlineData(9, 0, true)]     // شروع بازه
    [InlineData(12, 30, true)]
    [InlineData(19, 59, true)]
    [InlineData(20, 0, false)]   // پایان بازه شامل نمی‌شود
    [InlineData(8, 59, false)]
    [InlineData(2, 0, false)]
    public void Express_is_only_open_in_tehran_working_hours(int hour, int minute, bool expected)
        => Assert.Equal(expected, ShippingExpressRules.IsOpen(TehranToUtc(hour, minute), "09:00", "20:00"));

    [Fact]
    public void Invalid_hours_fall_back_to_nine_to_twenty()
    {
        Assert.True(ShippingExpressRules.IsOpen(TehranToUtc(10), "x", ""));
        Assert.False(ShippingExpressRules.IsOpen(TehranToUtc(21), null, null));
    }

    private static readonly IReadOnlyList<(int Level, double Amount)> Levels = new List<(int, double)>
    {
        (1, 5000), (2, 10000), (3, 15000), (4, 20000), (5, 25000)
    };

    [Fact]
    public void The_configured_boost_level_is_used_when_it_exists()
        => Assert.Equal((3, 15000d), ShippingExpressRules.ChooseBoost(Levels, 3));

    [Fact]
    public void A_missing_level_falls_back_to_the_highest_lower_level()
        => Assert.Equal((5, 25000d), ShippingExpressRules.ChooseBoost(Levels, 9));

    [Fact]
    public void A_level_below_all_available_uses_the_lowest()
        => Assert.Equal((2, 10000d), ShippingExpressRules.ChooseBoost(new List<(int, double)> { (2, 10000), (3, 15000) }, 1));

    [Fact]
    public void No_levels_or_disabled_config_means_no_boost()
    {
        Assert.Null(ShippingExpressRules.ChooseBoost(new List<(int, double)>(), 3));
        Assert.Null(ShippingExpressRules.ChooseBoost(null, 3));
        Assert.Null(ShippingExpressRules.ChooseBoost(Levels, 0));
        Assert.Null(ShippingExpressRules.ChooseBoost(new List<(int, double)> { (1, 0) }, 1)); // مبلغ صفر = اولویتی نیست
    }

    [Fact]
    public void Surcharge_is_the_miare_boost_amount_when_available_otherwise_the_fixed_pastil_fee()
    {
        Assert.Equal(15000, ShippingExpressRules.Surcharge((3, 15000d), 100000));
        Assert.Equal(100000, ShippingExpressRules.Surcharge(null, 100000));
        Assert.Equal(100000, ShippingExpressRules.Surcharge((1, 0d), 100000));
        Assert.Equal(0, ShippingExpressRules.Surcharge(null, -5));
    }

    [Fact]
    public void Express_price_is_the_miare_estimate_plus_the_surcharge()
    {
        Assert.Equal(155000, ShippingExpressRules.ExpressPrice(55000, 100000));
        Assert.Equal(70000, ShippingExpressRules.ExpressPrice(55000, 15000));
        Assert.Equal(100000, ShippingExpressRules.ExpressPrice(-1, 100000));
    }

    [Fact]
    public void Promise_and_seller_deadline_are_measured_from_payment()
    {
        var paid = new DateTime(2026, 10, 10, 8, 0, 0, DateTimeKind.Utc);
        Assert.Equal(paid.AddHours(3), ShippingExpressRules.PromiseEnd(paid, 3));
        Assert.Equal(paid.AddHours(1), ShippingExpressRules.PromiseEnd(paid, 0));       // حداقل ۱ ساعت
        Assert.Equal(paid.AddMinutes(20), ShippingExpressRules.SellerDeadline(paid, 20));
        Assert.Equal(paid.AddMinutes(5), ShippingExpressRules.SellerDeadline(paid, 1)); // حداقل ۵ دقیقه
    }

    // مهلت فروشنده برای آماده‌کردن کالا و «درخواست پیک» ۱ ساعت است؛ ۳۰ دقیقه بعد از پرداخت به ادمین هشدار می‌رود
    [Fact]
    public void Default_seller_window_is_one_hour_and_admin_is_alerted_after_half_of_it()
    {
        var options = new Application.Services.Order.ShippingSrv.ShippingOptions();
        Assert.Equal(60, options.Express.SellerConfirmMinutes);
        Assert.Equal(30, options.SellerNotReadyAdminAlertMinutes);

        var paid = new DateTime(2026, 10, 10, 8, 0, 0, DateTimeKind.Utc);
        var deadline = ShippingExpressRules.SellerDeadline(paid, options.Express.SellerConfirmMinutes);
        Assert.Equal(paid.AddHours(1), deadline);
        Assert.True(deadline < ShippingExpressRules.PromiseEnd(paid, options.Express.DeliveryMaxHours)); // قول تحویل (۳ ساعت) بعد از مهلت فروشنده است
    }

    [Fact]
    public void Delivery_text_is_two_to_three_working_hours_in_persian_digits()
    {
        Assert.Equal("۲ تا ۳ ساعت کاری", ShippingExpressRules.DeliveryText(2, 3));
        Assert.Equal("۳ تا ۳ ساعت کاری", ShippingExpressRules.DeliveryText(3, 1));
    }
}

public class MiareRequestFormatTests
{
    [Fact]
    public void Pickup_deadline_is_sent_in_utc_with_z_not_a_colon_offset()
    {
        // میاره «+00:00» را با parse_error رد می‌کند؛ فقط «Z» یا «+0000» را می‌پذیرد
        var text = Application.Services.Order.ShippingSrv.Provider.MiareShippingProvider.FormatDeadline(new DateTime(2026, 10, 10, 12, 30, 5, DateTimeKind.Utc));
        Assert.Equal("2026-10-10T12:30:05Z", text);
        Assert.DoesNotContain("+00:00", text);
    }

    [Fact]
    public void Unspecified_kind_is_treated_as_utc()
        => Assert.Equal("2026-10-10T08:00:00Z",
            Application.Services.Order.ShippingSrv.Provider.MiareShippingProvider.FormatDeadline(new DateTime(2026, 10, 10, 8, 0, 0, DateTimeKind.Unspecified)));
}
