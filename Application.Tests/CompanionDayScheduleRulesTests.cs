using Application.Common.Enumerable;
using Application.Services.CompanionSrvs.CompanionDayScheduleSrv;
using Application.Services.CompanionSrvs.CompanionDayScheduleSrv.Dto;
using System;
using Xunit;

namespace Application.Tests;

public class CompanionDayScheduleRulesTests
{
    private static readonly DateTime Day = new(2026, 10, 4);

    [Fact]
    public void Service_range_uses_the_chosen_time_slot_on_the_reserve_date()
    {
        var (start, end) = CompanionDayScheduleRules.ServiceRange(Day, "15:00", "16:30");

        Assert.Equal(Day.AddHours(15), start);
        Assert.Equal(Day.AddHours(16.5), end);
    }

    [Fact]
    public void Service_range_ignores_the_time_of_day_stored_in_DoDate()
    {
        var (start, _) = CompanionDayScheduleRules.ServiceRange(Day.AddHours(3), "09:00", "10:00");

        Assert.Equal(Day.AddHours(9), start);
    }

    [Fact]
    public void Service_range_falls_back_to_DoDate_when_the_slot_is_missing_or_invalid()
    {
        var doDate = Day.AddHours(11).AddMinutes(20);

        Assert.Equal((doDate, (DateTime?)null), CompanionDayScheduleRules.ServiceRange(doDate, null, null));
        Assert.Equal((doDate, (DateTime?)null), CompanionDayScheduleRules.ServiceRange(doDate, "abc", "def"));
    }

    [Fact]
    public void Service_range_keeps_only_the_start_when_only_the_start_is_valid()
    {
        var (start, end) = CompanionDayScheduleRules.ServiceRange(Day, "14:30", "nope");

        Assert.Equal(Day.AddHours(14.5), start);
        Assert.Null(end);
    }

    [Fact]
    public void Service_status_follows_cancel_done_and_the_time_window()
    {
        var start = Day.AddHours(15);
        var end = Day.AddHours(16);

        Assert.Equal(DayItemStatus.Cancelled, CompanionDayScheduleRules.ServiceStatus(true, null, start, end, Day.AddHours(15.5)));
        Assert.Equal(DayItemStatus.Cancelled, CompanionDayScheduleRules.ServiceStatus(true, Day, start, end, Day.AddHours(15.5)));
        Assert.Equal(DayItemStatus.Done, CompanionDayScheduleRules.ServiceStatus(false, Day.AddHours(15.9), start, end, Day.AddHours(15.5)));
        Assert.Equal(DayItemStatus.Upcoming, CompanionDayScheduleRules.ServiceStatus(false, null, start, end, Day.AddHours(14)));
        Assert.Equal(DayItemStatus.Now, CompanionDayScheduleRules.ServiceStatus(false, null, start, end, start));
        Assert.Equal(DayItemStatus.Now, CompanionDayScheduleRules.ServiceStatus(false, null, start, end, Day.AddHours(15.99)));
        Assert.Equal(DayItemStatus.Missed, CompanionDayScheduleRules.ServiceStatus(false, null, start, end, end));
        Assert.Equal(DayItemStatus.Missed, CompanionDayScheduleRules.ServiceStatus(false, null, start, end, Day.AddHours(20)));
    }

    [Fact]
    public void Service_without_an_end_time_is_never_reported_as_missed()
    {
        var start = Day.AddHours(11);

        Assert.Equal(DayItemStatus.Upcoming, CompanionDayScheduleRules.ServiceStatus(false, null, start, null, Day.AddHours(10)));
        Assert.Equal(DayItemStatus.Now, CompanionDayScheduleRules.ServiceStatus(false, null, start, null, Day.AddHours(23)));
    }

    [Theory]
    [InlineData((int)ConsultationPurchaseStatusEnum.Completed, DayItemStatus.Done)]
    [InlineData((int)ConsultationPurchaseStatusEnum.Active, DayItemStatus.Now)]
    [InlineData((int)ConsultationPurchaseStatusEnum.Cancelled, DayItemStatus.Cancelled)]
    [InlineData((int)ConsultationPurchaseStatusEnum.Refunded, DayItemStatus.Cancelled)]
    [InlineData((int)ConsultationPurchaseStatusEnum.Expired, DayItemStatus.Missed)]
    public void Consultation_status_maps_the_purchase_status(int purchaseStatus, int expected)
        => Assert.Equal(expected, CompanionDayScheduleRules.ConsultationStatus(purchaseStatus, Day.AddHours(16.25), Day.AddHours(10)));

    [Fact]
    public void A_paid_consultation_is_upcoming_until_its_start_deadline_then_missed()
    {
        var paid = (int)ConsultationPurchaseStatusEnum.Paid;
        var deadline = Day.AddHours(16.25);

        Assert.Equal(DayItemStatus.Upcoming, CompanionDayScheduleRules.ConsultationStatus(paid, deadline, Day.AddHours(16)));
        Assert.Equal(DayItemStatus.Upcoming, CompanionDayScheduleRules.ConsultationStatus(paid, deadline, deadline));
        Assert.Equal(DayItemStatus.Missed, CompanionDayScheduleRules.ConsultationStatus(paid, deadline, deadline.AddMinutes(1)));
    }

    [Fact]
    public void A_booked_consultation_can_be_started_only_from_ten_minutes_before_until_the_deadline()
    {
        var paid = (int)ConsultationPurchaseStatusEnum.Paid;
        var booked = Day.AddHours(16);
        var deadline = booked.AddMinutes(15);

        Assert.False(CompanionDayScheduleRules.ConsultationCanStart(paid, deadline, booked.AddMinutes(-11), booked));
        Assert.True(CompanionDayScheduleRules.ConsultationCanStart(paid, deadline, booked.AddMinutes(-10), booked));
        Assert.True(CompanionDayScheduleRules.ConsultationCanStart(paid, deadline, deadline, booked));
        Assert.False(CompanionDayScheduleRules.ConsultationCanStart(paid, deadline, deadline.AddMinutes(1), booked));
        Assert.False(CompanionDayScheduleRules.ConsultationCanStart((int)ConsultationPurchaseStatusEnum.Active, deadline, booked, booked));
    }

    [Theory]
    [InlineData(2026, 10, 4, 0)]    // امروز
    [InlineData(2026, 9, 5, 0)]     // هنوز یک ماه کامل نشده
    [InlineData(2026, 9, 4, 1)]
    [InlineData(2025, 10, 4, 12)]
    [InlineData(2024, 7, 20, 26)]   // ۲ سال و ۲ ماه و ۱۴ روز
    public void Age_in_months_counts_only_completed_months(int y, int m, int d, int expected)
        => Assert.Equal(expected, CompanionDayScheduleRules.AgeMonths(new DateTime(y, m, d), new DateTime(2026, 10, 4)));

    [Fact]
    public void Age_is_zero_for_an_empty_or_future_birthday()
    {
        var now = new DateTime(2026, 10, 4);

        Assert.Equal(0, CompanionDayScheduleRules.AgeMonths(default, now));
        Assert.Equal(0, CompanionDayScheduleRules.AgeMonths(now.AddDays(3), now));
    }

    [Fact]
    public void Time_text_is_24_hour_hh_mm_and_null_for_a_missing_value()
    {
        Assert.Equal("09:05", CompanionDayScheduleRules.TimeText(Day.AddHours(9).AddMinutes(5)));
        Assert.Equal("16:00", CompanionDayScheduleRules.TimeText(Day.AddHours(16)));
        Assert.Null(CompanionDayScheduleRules.TimeText(null));
    }
}
