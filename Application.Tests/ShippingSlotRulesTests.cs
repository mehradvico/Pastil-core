using Application.Services.Order.ShippingSrv;
using System;
using System.Linq;
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
    public void The_ready_button_closes_early_enough_for_the_minimum_pickup_lead()
    {
        // آخرین ساعت تحویل به پیک = پایان - ۶۰؛ چون پیک حداقل ۳۰ دقیقه بعد از درخواست می‌رسد، دکمه ۳۰ دقیقه زودتر بسته می‌شود
        var deadline = ShippingSlotRules.ReadyDeadline(NowUtc.AddHours(5), 60);
        Assert.Equal(NowUtc.AddMinutes(210), deadline);
        // آخرین لحظه‌ی مجاز: pickup = الان + ۳۰ دقیقه هنوز از آخرین ساعت تحویل به پیک جلو نمی‌افتد
        Assert.True(ShippingSlotRules.DefaultPickup(NowUtc.AddHours(1), deadline, 30) <= ShippingSlotRules.LatestPickup(NowUtc.AddHours(5), 60));
    }

    [Fact]
    public void Seller_deadline_is_the_earlier_of_the_confirm_window_and_the_slot_limit()
    {
        var slotEnd = NowUtc.AddHours(5);
        Assert.Equal(NowUtc.AddMinutes(60), ShippingSlotRules.SellerConfirmDeadline(NowUtc, slotEnd, 60, 60));

        var soonSlotEnd = NowUtc.AddMinutes(130); // پایان بازه‌ی نزدیک: مهلت آماده‌سازی = ۱۳۰ - (۶۰+۳۰) = ۴۰ ← تأیید ۵ دقیقه زودتر = ۳۵
        Assert.Equal(NowUtc.AddMinutes(35), ShippingSlotRules.SellerConfirmDeadline(NowUtc, soonSlotEnd, 60, 60));
    }

    // ساعت کاری ۰۹:۰۰ تا ۱۸:۰۰ تهران برای همه‌ی روزها (UTC+3:30)
    private static readonly (DayOfWeek, TimeSpan, TimeSpan)[] EveryDay9To18 = Enum.GetValues<DayOfWeek>()
        .Select(day => (day, TimeSpan.FromHours(9), TimeSpan.FromHours(18))).ToArray();

    private static DateTime Tehran(int day, int hour, int minute = 0) =>
        ShippingSlotRules.ToUtc(new DateTime(2026, 10, day), new TimeSpan(hour, minute, 0));

    [Fact]
    public void Working_minutes_without_store_hours_are_plain_minutes()
        => Assert.Equal(NowUtc.AddMinutes(60), ShippingSlotRules.AddWorkingMinutes(NowUtc, 60, null));

    [Fact]
    public void Working_minutes_inside_open_hours_do_not_skip_anything()
        => Assert.Equal(Tehran(5, 11), ShippingSlotRules.AddWorkingMinutes(Tehran(5, 10), 60, EveryDay9To18));

    [Fact]
    public void An_order_paid_at_night_gets_its_window_when_the_store_opens()
    {
        // ۲۲:۰۰ ← مهلت ۶۰ دقیقه‌ای از ۰۹:۰۰ فردا شروع می‌شود و ۱۰:۰۰ تمام می‌شود
        Assert.Equal(Tehran(6, 10), ShippingSlotRules.AddWorkingMinutes(Tehran(5, 22), 60, EveryDay9To18));
        // ۰۵:۰۰ صبح همان روز
        Assert.Equal(Tehran(5, 10), ShippingSlotRules.AddWorkingMinutes(Tehran(5, 5), 60, EveryDay9To18));
    }

    [Fact]
    public void A_window_that_crosses_closing_time_continues_next_working_day()
        // ۱۷:۳۰ ← ۳۰ دقیقه تا بسته‌شدن، ۳۰ دقیقه‌ی باقی‌مانده فردا ۰۹:۳۰
        => Assert.Equal(Tehran(6, 9, 30), ShippingSlotRules.AddWorkingMinutes(Tehran(5, 17, 30), 60, EveryDay9To18));

    [Fact]
    public void Closed_days_without_a_row_are_skipped()
    {
        // فقط روز بعد باز است؛ ۱۰ اکتبر ۲۰۲۶ = شنبه، ۱۱ = یکشنبه
        var onlySunday = new[] { (DayOfWeek.Sunday, TimeSpan.FromHours(9), TimeSpan.FromHours(18)) };
        Assert.Equal(Tehran(11, 10), ShippingSlotRules.AddWorkingMinutes(Tehran(10, 12), 60, onlySunday));
    }

    [Fact]
    public void The_seller_confirm_deadline_for_a_far_slot_waits_for_working_hours_but_never_passes_the_slot_limit()
    {
        var farSlotEnd = Tehran(8, 13); // سه روز بعد
        Assert.Equal(Tehran(6, 10), ShippingSlotRules.SellerConfirmDeadline(Tehran(5, 22), farSlotEnd, 60, 60, EveryDay9To18));
        // بازه‌ی فردا ۹ تا ۱۳ و پرداخت شب قبل: مهلت تأیید از سقف بازه (۱۱:۲۵) جلو نمی‌افتد
        var tomorrowEnd = Tehran(6, 13);
        var limit = ShippingSlotRules.ReadyDeadline(tomorrowEnd, 60).AddMinutes(-5);
        Assert.Equal(limit, ShippingSlotRules.SellerConfirmDeadline(Tehran(5, 22), tomorrowEnd, 600, 60, EveryDay9To18));
    }

    private static readonly (DayOfWeek, TimeSpan, TimeSpan)[] EveryDay10To17 = Enum.GetValues<DayOfWeek>()
        .Select(day => (day, TimeSpan.FromHours(10), TimeSpan.FromHours(17))).ToArray();

    private static bool Offered(int startHour, int endHour, (DayOfWeek, TimeSpan, TimeSpan)[] hours, DayOfWeek day = DayOfWeek.Monday)
        => ShippingSlotRules.IsWithinStoreHours(day, TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour), 30, 60, hours);

    [Fact]
    public void A_store_open_10_to_17_does_not_offer_the_evening_slot()
    {
        Assert.True(Offered(9, 13, EveryDay10To17));    // هم‌پوشانی دارد؛ پیک ۱۰:۰۰ می‌آید و تا ۱۲:۰۰ وقت دارد
        Assert.True(Offered(13, 17, EveryDay10To17));
        Assert.False(Offered(17, 21, EveryDay10To17));  // بعد از بسته‌شدن
        Assert.False(Offered(19, 23, EveryDay10To17));
        Assert.False(Offered(6, 9, EveryDay10To17));    // قبل از بازشدن
    }

    [Fact]
    public void The_courier_must_still_have_time_to_deliver_after_the_store_opens()
    {
        Assert.True(Offered(9, 11, EveryDay10To17));    // پیک ۱۰:۰۰ = آخرین ساعت تحویل (۱۱ − ۱) ← مرز
        Assert.False(Offered(9, 10, EveryDay10To17));   // بازه‌ی ۹ تا ۱۰: تا پیک بیاید بازه تمام شده
    }

    [Fact]
    public void Stores_without_hours_are_unrestricted_and_closed_days_are_hidden()
    {
        Assert.True(Offered(17, 21, null));
        Assert.True(Offered(17, 21, Array.Empty<(DayOfWeek, TimeSpan, TimeSpan)>()));
        var onlySunday = new[] { (DayOfWeek.Sunday, TimeSpan.FromHours(9), TimeSpan.FromHours(18)) };
        Assert.True(Offered(9, 13, onlySunday, DayOfWeek.Sunday));
        Assert.False(Offered(9, 13, onlySunday, DayOfWeek.Monday));
    }

    [Fact]
    public void The_courier_is_never_due_before_the_store_opens()
    {
        Assert.Equal(Tehran(5, 10), ShippingSlotRules.NextOpenAtOrAfter(Tehran(5, 8, 30), EveryDay10To17));   // قبل از بازشدن
        Assert.Equal(Tehran(5, 12), ShippingSlotRules.NextOpenAtOrAfter(Tehran(5, 12), EveryDay10To17));      // داخل ساعت کاری ← بدون تغییر
        Assert.Equal(Tehran(6, 10), ShippingSlotRules.NextOpenAtOrAfter(Tehran(5, 18), EveryDay10To17));      // بعد از بسته‌شدن ← فردا
        Assert.Equal(Tehran(5, 8, 30), ShippingSlotRules.NextOpenAtOrAfter(Tehran(5, 8, 30), null));
    }

    [Fact]
    public void Ready_nudge_goes_out_at_slot_start_and_again_thirty_minutes_later()
    {
        var anchor = Tehran(8, 13);                                         // شروع بازه‌ی ۱۳ تا ۱۷
        var deadline = ShippingSlotRules.ReadyDeadline(Tehran(8, 17), 60);  // ۱۵:۳۰

        Assert.Equal(0, ShippingSlotRules.ReadyNudgeStage(anchor, deadline, null, anchor.AddMinutes(-1)));
        Assert.Equal(1, ShippingSlotRules.ReadyNudgeStage(anchor, deadline, null, anchor));
        // مرحله‌ی ۱ رفت (۱۳:۰۰)؛ تا ۳۰ دقیقه بعد چیزی نمی‌رود
        var sentAt = anchor.AddMinutes(3);
        Assert.Equal(0, ShippingSlotRules.ReadyNudgeStage(anchor, deadline, sentAt, sentAt.AddMinutes(29)));
        Assert.Equal(2, ShippingSlotRules.ReadyNudgeStage(anchor, deadline, sentAt, sentAt.AddMinutes(30)));
        // مرحله‌ی ۲ هم که رفت دیگر تکرار نمی‌شود
        var urgentAt = sentAt.AddMinutes(30);
        Assert.Equal(0, ShippingSlotRules.ReadyNudgeStage(anchor, deadline, urgentAt, urgentAt.AddHours(1)));
    }

    [Fact]
    public void A_seller_who_confirms_after_slot_start_gets_one_nudge_and_a_pre_slot_reminder_does_not_count()
    {
        var anchor = Tehran(8, 13);
        var deadline = ShippingSlotRules.ReadyDeadline(Tehran(8, 17), 60);
        var lateNow = anchor.AddMinutes(45);
        Assert.Equal(1, ShippingSlotRules.ReadyNudgeStage(anchor, deadline, null, lateNow));
        Assert.Equal(0, ShippingSlotRules.ReadyNudgeStage(anchor, deadline, lateNow, lateNow.AddMinutes(40)));
        // یادآوری قدیمیِ قبل از شروع بازه (قبل از این تغییر) جلوی پیام «موعد ارسال رسید» را نمی‌گیرد
        Assert.Equal(1, ShippingSlotRules.ReadyNudgeStage(anchor, deadline, anchor.AddHours(-1), anchor));
    }

    [Fact]
    public void The_urgent_nudge_is_skipped_when_it_would_land_after_the_ready_deadline()
    {
        var anchor = Tehran(8, 13);
        var deadline = anchor.AddMinutes(20);
        var sentAt = anchor;
        Assert.Equal(0, ShippingSlotRules.ReadyNudgeStage(anchor, deadline, sentAt, sentAt.AddMinutes(31)));
    }

    [Fact]
    public void Pickup_defaults_to_shortly_before_the_slot_but_never_in_the_past()
    {
        var slotStart = NowUtc.AddHours(4);
        Assert.Equal(slotStart.AddMinutes(-30), ShippingSlotRules.DefaultPickup(slotStart, NowUtc, 30));
        // میاره pickup.deadline کمتر از ۳۰ دقیقه از الان را «deadline_too_soon» رد می‌کند
        Assert.Equal(NowUtc.AddMinutes(30), ShippingSlotRules.DefaultPickup(NowUtc.AddMinutes(20), NowUtc, 30));
        Assert.Equal(NowUtc.AddMinutes(30), ShippingSlotRules.ExpressPickup(NowUtc, 15));
        Assert.Equal(NowUtc.AddMinutes(40), ShippingSlotRules.ExpressPickup(NowUtc, 40));
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
