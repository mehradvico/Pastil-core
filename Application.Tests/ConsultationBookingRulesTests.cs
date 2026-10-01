using Application.Common.Enumerable;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Window = Application.Services.ConsultationSrvs.ConsultationBookingSrv.ConsultationBookingRules.Window;

namespace Application.Tests;

public class ConsultationBookingRulesTests
{
    // شنبه ۱۴۰۵/۰۷/۱۱ = 2026-10-03 (Saturday)
    private static readonly DateTime Now = new(2026, 10, 3, 10, 0, 0);
    private static readonly DateTime Saturday = new(2026, 10, 3);

    private static readonly Window[] Afternoon = { new(15 * 60, 19 * 60, 1) };

    [Theory]
    [InlineData(2026, 10, 3, 1)]  // Saturday
    [InlineData(2026, 10, 4, 2)]  // Sunday
    [InlineData(2026, 10, 7, 5)]  // Wednesday
    [InlineData(2026, 10, 9, 7)]  // Friday
    public void WeekDayId_follows_the_project_convention_saturday_first(int y, int m, int d, int expected)
        => Assert.Equal(expected, ConsultationBookingRules.WeekDayId(new DateTime(y, m, d)));

    [Theory]
    [InlineData("16:00", 960)]
    [InlineData("00:00", 0)]
    [InlineData("24:00", 1440)]
    [InlineData(" 09:15 ", 555)]
    public void ParseTime_accepts_valid_times(string text, int expected)
        => Assert.Equal(expected, ConsultationBookingRules.ParseTime(text));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("16")]
    [InlineData("25:00")]
    [InlineData("24:30")]
    [InlineData("12:60")]
    [InlineData("-1:00")]
    [InlineData("ab:cd")]
    public void ParseTime_rejects_invalid_times(string text)
        => Assert.Null(ConsultationBookingRules.ParseTime(text));

    [Fact]
    public void FormatTime_round_trips()
    {
        Assert.Equal("16:00", ConsultationBookingRules.FormatTime(960));
        Assert.Equal("09:15", ConsultationBookingRules.FormatTime(555));
        Assert.Equal(555, ConsultationBookingRules.ParseTime(ConsultationBookingRules.FormatTime(555)));
    }

    [Theory]
    [InlineData(1, 900, 1140, 1, ConsultationBookingRules.WindowProblem.None)]
    [InlineData(0, 900, 1140, 1, ConsultationBookingRules.WindowProblem.BadWeekDay)]
    [InlineData(8, 900, 1140, 1, ConsultationBookingRules.WindowProblem.BadWeekDay)]
    [InlineData(1, 905, 1140, 1, ConsultationBookingRules.WindowProblem.BadTime)]   // خارج از شبکه‌ی ۱۵ دقیقه
    [InlineData(1, 900, 1500, 1, ConsultationBookingRules.WindowProblem.BadTime)]   // بعد از ۲۴:۰۰
    [InlineData(1, 1140, 900, 1, ConsultationBookingRules.WindowProblem.EndNotAfterStart)]
    [InlineData(1, 900, 900, 1, ConsultationBookingRules.WindowProblem.EndNotAfterStart)]
    [InlineData(1, 900, 1140, 0, ConsultationBookingRules.WindowProblem.BadCapacity)]
    [InlineData(1, 900, 1140, 21, ConsultationBookingRules.WindowProblem.BadCapacity)]
    public void ValidateWindow_checks_each_field(int day, int start, int end, int capacity, ConsultationBookingRules.WindowProblem expected)
        => Assert.Equal(expected, ConsultationBookingRules.ValidateWindow(day, start, end, capacity));

    [Fact]
    public void Windows_of_the_same_day_must_not_overlap_but_may_touch()
    {
        Assert.False(ConsultationBookingRules.HasOverlap(new[] { (1, 540, 780), (1, 780, 1140), (2, 540, 780) }));
        Assert.True(ConsultationBookingRules.HasOverlap(new[] { (1, 540, 800), (1, 780, 1140) }));
        Assert.False(ConsultationBookingRules.HasOverlap(new[] { (1, 540, 800), (2, 780, 1140) }));
    }

    [Fact]
    public void A_consultation_must_fit_entirely_inside_one_window()
    {
        var windows = new[] { new Window(9 * 60, 13 * 60, 2), new Window(15 * 60, 19 * 60, 1) };

        Assert.Equal(2, ConsultationBookingRules.CapacityFor(windows, 9 * 60, 30));
        Assert.Equal(2, ConsultationBookingRules.CapacityFor(windows, 12 * 60 + 30, 30)); // دقیقاً تا انتهای بازه
        Assert.Equal(0, ConsultationBookingRules.CapacityFor(windows, 12 * 60 + 45, 30)); // از انتهای بازه می‌زند بیرون
        Assert.Equal(0, ConsultationBookingRules.CapacityFor(windows, 13 * 60, 30));       // وسط تعطیلی
        Assert.Equal(1, ConsultationBookingRules.CapacityFor(windows, 16 * 60, 60));
        Assert.Equal(0, ConsultationBookingRules.CapacityFor(windows, 14 * 60 + 45, 30)); // قبل از شروع بازه
    }

    [Fact]
    public void Overlap_counting_treats_touching_bookings_as_non_overlapping()
    {
        var existing = new List<(DateTime, DateTime)>
        {
            (Saturday.AddHours(15), Saturday.AddHours(15.5)),
            (Saturday.AddHours(16), Saturday.AddHours(17)),
        };

        Assert.Equal(0, ConsultationBookingRules.CountOverlaps(existing, Saturday.AddHours(15.5), Saturday.AddHours(16)));
        Assert.Equal(1, ConsultationBookingRules.CountOverlaps(existing, Saturday.AddHours(15.25), Saturday.AddHours(15.75)));
        Assert.Equal(2, ConsultationBookingRules.CountOverlaps(existing, Saturday.AddHours(15.25), Saturday.AddHours(16.25)));
    }

    [Fact]
    public void Slots_respect_the_grid_duration_lead_time_and_capacity()
    {
        // پکیج ۶۰ دقیقه‌ای، بازه‌ی ۱۵ تا ۱۹، ظرفیت ۱؛ هر ۱۵ دقیقه یک شروع ⇒ آخرین شروع ۱۸:۰۰
        var existing = new List<(DateTime, DateTime)> { (Saturday.AddHours(16), Saturday.AddHours(17)) };
        var slots = ConsultationBookingRules.BuildSlots(Saturday, 60, Now, Afternoon, existing);

        Assert.Equal(Saturday.AddHours(15), slots.First().Start);
        Assert.Equal(Saturday.AddHours(18), slots.Last().Start);
        Assert.Equal(13, slots.Count);

        // ۱۶:۰۰ و هر شروعی که با رزرو ۱۶–۱۷ هم‌پوشان است پر است؛ ۱۷:۰۰ آزاد
        Assert.False(slots.Single(s => s.Start == Saturday.AddHours(16)).Available);
        Assert.False(slots.Single(s => s.Start == Saturday.AddHours(15.25)).Available);
        Assert.False(slots.Single(s => s.Start == Saturday.AddHours(16.75)).Available);
        Assert.True(slots.Single(s => s.Start == Saturday.AddHours(15)).Available);
        Assert.True(slots.Single(s => s.Start == Saturday.AddHours(17)).Available);
    }

    [Fact]
    public void Slots_closer_than_the_minimum_lead_time_are_not_offered()
    {
        var now = Saturday.AddHours(15).AddMinutes(10); // الان ۱۵:۱۰ ⇒ حداقل شروع ۱۵:۴۰ ⇒ اولین اسلات ۱۵:۴۵
        var slots = ConsultationBookingRules.BuildSlots(Saturday, 30, now, Afternoon, new List<(DateTime, DateTime)>());

        Assert.Equal(Saturday.AddHours(15.75), slots.First().Start);
    }

    [Fact]
    public void Higher_capacity_keeps_a_slot_open_until_it_is_really_full()
    {
        var windows = new[] { new Window(15 * 60, 19 * 60, 2) };
        var one = new List<(DateTime, DateTime)> { (Saturday.AddHours(16), Saturday.AddHours(16.5)) };
        var two = new List<(DateTime, DateTime)> { one[0], (Saturday.AddHours(16), Saturday.AddHours(16.5)) };

        Assert.True(ConsultationBookingRules.BuildSlots(Saturday, 30, Now, windows, one).Single(s => s.Start == Saturday.AddHours(16)).Available);
        Assert.False(ConsultationBookingRules.BuildSlots(Saturday, 30, Now, windows, two).Single(s => s.Start == Saturday.AddHours(16)).Available);
    }

    [Fact]
    public void Days_beyond_the_booking_horizon_have_no_slots()
    {
        var far = Now.Date.AddDays(ConsultationRules.BookingMaxDaysAhead + 1);
        Assert.Empty(ConsultationBookingRules.BuildSlots(far, 30, Now, Afternoon, new List<(DateTime, DateTime)>()));
    }

    [Fact]
    public void CheckSlot_reports_the_first_problem()
    {
        var ok = Saturday.AddHours(16);
        Assert.Equal(ConsultationBookingRules.SlotProblem.None, ConsultationBookingRules.CheckSlot(ok, 60, Now, Afternoon, 0));
        Assert.Equal(ConsultationBookingRules.SlotProblem.NotOnGrid, ConsultationBookingRules.CheckSlot(ok.AddMinutes(7), 60, Now, Afternoon, 0));
        Assert.Equal(ConsultationBookingRules.SlotProblem.NotOnGrid, ConsultationBookingRules.CheckSlot(ok.AddSeconds(5), 60, Now, Afternoon, 0));
        Assert.Equal(ConsultationBookingRules.SlotProblem.TooSoon, ConsultationBookingRules.CheckSlot(Now.AddMinutes(15), 60, Now, Afternoon, 0));
        Assert.Equal(ConsultationBookingRules.SlotProblem.TooFar,
            ConsultationBookingRules.CheckSlot(Now.Date.AddDays(ConsultationRules.BookingMaxDaysAhead + 1).AddHours(16), 60, Now, Afternoon, 0));
        Assert.Equal(ConsultationBookingRules.SlotProblem.OutsideAvailability, ConsultationBookingRules.CheckSlot(Saturday.AddHours(20), 60, Now, Afternoon, 0));
        Assert.Equal(ConsultationBookingRules.SlotProblem.Full, ConsultationBookingRules.CheckSlot(ok, 60, Now, Afternoon, 1));
    }

    [Fact]
    public void Agent_may_start_from_ten_minutes_before_the_booked_time()
    {
        var booked = Saturday.AddHours(16);
        Assert.False(ConsultationBookingRules.IsStartAllowed(booked, booked.AddMinutes(-11)));
        Assert.True(ConsultationBookingRules.IsStartAllowed(booked, booked.AddMinutes(-10)));
        Assert.True(ConsultationBookingRules.IsStartAllowed(booked, booked.AddMinutes(5)));
        Assert.True(ConsultationBookingRules.IsStartAllowed(null, booked.AddDays(-3))); // خرید فوری: همیشه
    }

    [Fact]
    public void Start_deadline_is_booked_time_plus_grace_so_late_agents_trigger_the_refund_job()
    {
        var booked = Saturday.AddHours(16);
        var deadline = ConsultationBookingRules.StartDeadline(booked);

        Assert.Equal(booked.AddMinutes(15), deadline);
        Assert.True(ConsultationPurchaseRules.IsStartOverdue((int)ConsultationPurchaseStatusEnum.Paid, deadline, booked.AddMinutes(16)));
        Assert.False(ConsultationPurchaseRules.IsStartOverdue((int)ConsultationPurchaseStatusEnum.Paid, deadline, booked.AddMinutes(14)));
    }

    [Fact]
    public void User_cancel_is_free_until_two_hours_before_and_unrestricted_for_instant_purchases()
    {
        var booked = Saturday.AddHours(16);
        var paid = (int)ConsultationPurchaseStatusEnum.Paid;

        Assert.True(ConsultationPurchaseRules.CanUserCancel(paid, booked, booked.AddHours(-2)));
        Assert.False(ConsultationPurchaseRules.CanUserCancel(paid, booked, booked.AddHours(-2).AddMinutes(1)));
        Assert.True(ConsultationPurchaseRules.CanUserCancel(paid, null, booked.AddDays(5)));
        Assert.False(ConsultationPurchaseRules.CanUserCancel((int)ConsultationPurchaseStatusEnum.Active, null, booked));
    }

    [Fact]
    public void CanStart_for_a_booking_combines_the_deadline_and_the_early_start_rule()
    {
        var booked = Saturday.AddHours(16);
        var deadline = ConsultationBookingRules.StartDeadline(booked);
        var paid = (int)ConsultationPurchaseStatusEnum.Paid;

        Assert.False(ConsultationPurchaseRules.CanStart(paid, deadline, booked.AddHours(-1), booked));
        Assert.True(ConsultationPurchaseRules.CanStart(paid, deadline, booked, booked));
        Assert.False(ConsultationPurchaseRules.CanStart(paid, deadline, booked.AddMinutes(16), booked));
    }

    [Fact]
    public void Reminder_is_due_only_in_the_thirty_minutes_before_the_booking()
    {
        var booked = Saturday.AddHours(16);
        Assert.False(ConsultationBookingRules.IsReminderDue(booked, booked.AddMinutes(-31)));
        Assert.True(ConsultationBookingRules.IsReminderDue(booked, booked.AddMinutes(-30)));
        Assert.True(ConsultationBookingRules.IsReminderDue(booked, booked.AddMinutes(-1)));
        Assert.False(ConsultationBookingRules.IsReminderDue(booked, booked));
        Assert.False(ConsultationBookingRules.IsReminderDue(null, booked));
    }

    [Fact]
    public void Pending_booking_holds_capacity_only_for_the_hold_window()
    {
        var created = Now;
        Assert.True(ConsultationBookingRules.PendingStillHolds(created, created.AddMinutes(19)));
        Assert.False(ConsultationBookingRules.PendingStillHolds(created, created.AddMinutes(21)));
    }
}
