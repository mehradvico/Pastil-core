using Application.Services.CompanionSrvs.CompanionReserveSrv;
using Entities.Entities;
using System;
using Xunit;

namespace Application.Tests;

public class CompanionReserveRescheduleRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 8, 9, 0, 0);

    [Fact]
    public void Start_time_comes_from_the_slot_not_from_midnight_do_date()
    {
        var start = CompanionReserveRescheduleRules.StartAt(new DateTime(2026, 10, 10), "10:30");
        Assert.Equal(new DateTime(2026, 10, 10, 10, 30, 0), start);
    }

    [Fact]
    public void Start_time_of_a_reserve_prefers_the_center_slot_then_the_legacy_assistance_slot()
    {
        var reserve = new Entities.Entities.CompanionReserve
        {
            DoDate = new DateTime(2026, 10, 10),
            CompanionTime = new CompanionTime { StartTime = "14:00" },
            CompanionAssistanceTime = new CompanionAssistanceTime { StartTime = "09:00" }
        };
        Assert.Equal(new DateTime(2026, 10, 10, 14, 0, 0), CompanionReserveRescheduleRules.StartAt(reserve));

        reserve.CompanionTime = null;
        Assert.Equal(new DateTime(2026, 10, 10, 9, 0, 0), CompanionReserveRescheduleRules.StartAt(reserve));
    }

    [Fact]
    public void A_reserve_without_any_slot_and_a_date_only_do_date_has_no_known_start()
        => Assert.Null(CompanionReserveRescheduleRules.StartAt(new Entities.Entities.CompanionReserve { DoDate = new DateTime(2026, 10, 10) }));

    [Theory]
    [InlineData(true, false, false, false, false, true)]
    [InlineData(false, false, false, false, false, false)]  // پرداخت‌نشده
    [InlineData(true, true, false, false, false, false)]    // لغو‌شده
    [InlineData(true, false, true, false, false, false)]    // انجام‌شده
    [InlineData(true, false, false, true, false, false)]    // وضعیت تکمیل
    [InlineData(true, false, false, false, true, false)]    // آنلاین فوری
    public void Only_paid_open_scheduled_reserves_can_be_rescheduled(bool reserved, bool cancelled, bool done, bool complete, bool instant, bool expected)
        => Assert.Equal(expected,
            CompanionReserveRescheduleRules.CanReschedule(reserved, cancelled, done ? Now : null, complete, instant));

    [Fact]
    public void New_time_must_be_at_least_thirty_minutes_ahead()
    {
        Assert.True(CompanionReserveRescheduleRules.IsFarEnough(Now.AddMinutes(30), Now));
        Assert.False(CompanionReserveRescheduleRules.IsFarEnough(Now.AddMinutes(29), Now));
        Assert.False(CompanionReserveRescheduleRules.IsFarEnough(Now.AddHours(-1), Now));
    }

    [Fact]
    public void Same_slot_means_same_day_and_same_time_id()
    {
        var day = new DateTime(2026, 10, 10);
        Assert.True(CompanionReserveRescheduleRules.IsSameSlot(day, 5, day.AddHours(10), 5));
        Assert.False(CompanionReserveRescheduleRules.IsSameSlot(day, 5, day, 6));
        Assert.False(CompanionReserveRescheduleRules.IsSameSlot(day, 5, day.AddDays(7), 5));
        Assert.False(CompanionReserveRescheduleRules.IsSameSlot(day, null, day, 5));
    }

    [Theory]
    [InlineData(2, 1, true)]
    [InlineData(2, 2, false)]
    [InlineData(0, 0, true)]   // ظرفیت صفر مثل بقیه‌ی کد = یک نفر
    [InlineData(0, 1, false)]
    public void Capacity_counts_other_reserves_only(int capacity, int others, bool expected)
        => Assert.Equal(expected, CompanionReserveRescheduleRules.HasCapacity(capacity, others));

    [Fact]
    public void Linked_trip_departs_lead_minutes_before_the_real_start()
    {
        var start = new DateTime(2026, 10, 10, 10, 0, 0);
        Assert.Equal(start.AddMinutes(-60), CompanionReserveRescheduleRules.DepartureFor(start, 60));
        Assert.Equal(start.AddMinutes(-120), CompanionReserveRescheduleRules.DepartureFor(start, 120));
        Assert.Equal(start.AddMinutes(-60), CompanionReserveRescheduleRules.DepartureFor(start, null));
    }

    [Fact]
    public void Reason_is_trimmed_collapsed_and_capped()
    {
        Assert.Equal("به دلیل بیماری", CompanionReserveRescheduleRules.NormalizeReason("   به   دلیل \n بیماری  "));
        Assert.Equal("", CompanionReserveRescheduleRules.NormalizeReason(null));
        Assert.Equal(CompanionReserveRescheduleRules.MaxReasonLength, CompanionReserveRescheduleRules.NormalizeReason(new string('ا', 500)).Length);
    }

    [Fact]
    public void Dates_in_sms_are_jalali_with_latin_digits_and_times_are_hh_mm()
    {
        // ۲۰۲۶‑۱۰‑۱۰ = ۱۴۰۵/۰۷/۱۸
        Assert.Equal("1405/07/18", CompanionReserveRescheduleRules.JalaliDate(new DateTime(2026, 10, 10)));
        Assert.Equal("09:05", CompanionReserveRescheduleRules.Time(new DateTime(2026, 10, 10, 9, 5, 0)));
    }
}
