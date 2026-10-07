using Application.Services.Order.ShippingSrv;
using System;
using Xunit;

namespace Application.Tests;

public class ShippingSlotRulesTests
{
    // ۱۴۰۵/۰۷/۱۳ ≈ 2026-10-05، ساعت ۱۰:۰۰ به وقت تهران (UTC+3:30 در این تاریخ؛ ایران DST ندارد)
    private static readonly DateTime NowUtc = new(2026, 10, 5, 6, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime Today = new(2026, 10, 5);

    [Fact]
    public void Tehran_local_times_convert_to_utc_without_dst()
    {
        var utc = ShippingSlotRules.ToUtc(Today, TimeSpan.FromHours(9));
        Assert.Equal(new DateTime(2026, 10, 5, 5, 30, 0), utc);
        Assert.Equal(new TimeSpan(10, 0, 0), ShippingSlotRules.ToTehran(NowUtc).TimeOfDay);
    }

    [Fact]
    public void Only_tomorrow_and_the_following_days_inside_the_horizon_are_selectable()
    {
        Assert.False(ShippingSlotRules.IsDateInWindow(Today, NowUtc, 3));              // همان روز نداریم
        Assert.True(ShippingSlotRules.IsDateInWindow(Today.AddDays(1), NowUtc, 3));    // فردا اولین روز است
        Assert.True(ShippingSlotRules.IsDateInWindow(Today.AddDays(3), NowUtc, 3));
        Assert.False(ShippingSlotRules.IsDateInWindow(Today.AddDays(4), NowUtc, 3));
        Assert.False(ShippingSlotRules.IsDateInWindow(Today.AddDays(-1), NowUtc, 3));
    }

    [Fact]
    public void A_slot_is_selectable_only_if_the_goods_can_be_ready_and_handed_to_the_courier_before_its_end()
    {
        // الان ۱۰:۰۰ تهران، آماده‌سازی ۱۲۰ دقیقه، اطمینان ۳۰، حداقل رساندن ۶۰: آماده‌سازی تا ۱۲:۳۰ ← پایان بازه باید ≥ ۱۳:۳۰ باشد
        Assert.True(ShippingSlotRules.IsFeasible(Today, TimeSpan.FromHours(17), NowUtc, 120, 30, 60));   // ۱۳–۱۷
        Assert.True(ShippingSlotRules.IsFeasible(Today, new TimeSpan(13, 30, 0), NowUtc, 120, 30, 60));  // دقیقاً مرز
        Assert.False(ShippingSlotRules.IsFeasible(Today, TimeSpan.FromHours(13), NowUtc, 120, 30, 60));  // ۹–۱۳ دیر است
    }

    [Fact]
    public void Longer_preparation_pushes_the_first_selectable_slot_to_later_days()
    {
        // آماده‌سازی ۱ روز (۱۴۴۰): امروز هیچ بازه‌ای نیست، فردا صبح هم نه، ولی فردا بعدازظهر بله
        Assert.False(ShippingSlotRules.IsFeasible(Today, TimeSpan.FromHours(21), NowUtc, 1440, 30, 60));
        Assert.False(ShippingSlotRules.IsFeasible(Today.AddDays(1), TimeSpan.FromHours(11), NowUtc, 1440, 30, 60));
        Assert.True(ShippingSlotRules.IsFeasible(Today.AddDays(1), TimeSpan.FromHours(17), NowUtc, 1440, 30, 60));
    }

    [Fact]
    public void The_selectable_horizon_grows_with_the_preparation_time()
    {
        Assert.Equal(3, ShippingSlotRules.HorizonDays(3, 0));
        Assert.Equal(4, ShippingSlotRules.HorizonDays(3, 120));
        Assert.Equal(4, ShippingSlotRules.HorizonDays(3, 1440));
        Assert.Equal(5, ShippingSlotRules.HorizonDays(3, 1441));
        Assert.Equal(1, ShippingSlotRules.HorizonDays(0, 0));
    }

    [Fact]
    public void The_seller_must_hand_over_to_the_courier_before_end_minus_delivery_time()
        => Assert.Equal(NowUtc.AddHours(4), ShippingSlotRules.ReadyDeadline(NowUtc.AddHours(5), 60));

    [Fact]
    public void Seller_deadline_is_the_earlier_of_the_confirm_window_and_the_slot_limit()
    {
        var slotEnd = NowUtc.AddHours(5);
        Assert.Equal(NowUtc.AddMinutes(60), ShippingSlotRules.SellerConfirmDeadline(NowUtc, slotEnd, 60, 60));

        var soonSlotEnd = NowUtc.AddMinutes(100); // پایان بازه‌ی نزدیک: ۱۰۰ - (۶۰+۱۰) = ۳۰ دقیقه
        Assert.Equal(NowUtc.AddMinutes(30), ShippingSlotRules.SellerConfirmDeadline(NowUtc, soonSlotEnd, 60, 60));
    }

    [Fact]
    public void Pickup_defaults_to_shortly_before_the_slot_but_never_in_the_past()
    {
        var slotStart = NowUtc.AddHours(4);
        Assert.Equal(slotStart.AddMinutes(-30), ShippingSlotRules.DefaultPickup(slotStart, NowUtc, 30));
        Assert.Equal(NowUtc.AddMinutes(15), ShippingSlotRules.DefaultPickup(NowUtc.AddMinutes(20), NowUtc, 30));
        Assert.Equal(NowUtc.AddHours(4), ShippingSlotRules.LatestPickup(NowUtc.AddHours(5), 60));
    }

    [Theory]
    [InlineData("09:00", true)]
    [InlineData("21:30", true)]
    [InlineData("24:00", false)]
    [InlineData("9", false)]
    [InlineData("", false)]
    public void Times_are_parsed_in_hh_mm_format(string value, bool ok)
        => Assert.Equal(ok, ShippingSlotRules.TryParseTime(value, out _));
}
