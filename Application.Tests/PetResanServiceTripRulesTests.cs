using Application.Services.TripSrv.TripSrv;
using System;
using Xunit;

namespace Application.Tests;

public class PetResanServiceTripRulesTests
{
    [Theory]
    [InlineData(9, false)]    // قبل از job: خود job امشب نوبت فردا را می‌سازد
    [InlineData(19, false)]
    [InlineData(20, true)]    // از ساعت job به بعد: باید همان لحظه ساخته شود
    [InlineData(23, true)]
    public void New_services_get_tomorrows_occurrence_immediately_only_after_the_daily_job_ran(int hour, bool expected)
        => Assert.Equal(expected, PetResanServiceTripRules.DailyJobHasRun(new DateTime(2026, 10, 6, hour, 15, 0)));

    private static readonly DateTime Sat = new(2026, 10, 17, 8, 0, 0);

    [Fact]
    public void One_service_day_without_a_driver_does_not_stop_the_service()
        => Assert.False(PetResanServiceTripRules.ShouldStopForNoDriver(new[] { (Sat, true) }));

    [Fact]
    public void Two_consecutive_service_days_without_a_driver_stop_the_service()
        => Assert.True(PetResanServiceTripRules.ShouldStopForNoDriver(new[] { (Sat, true), (Sat.AddDays(2), true) }));

    [Fact]
    public void Outbound_and_return_legs_of_the_same_day_count_as_one_day()
        => Assert.False(PetResanServiceTripRules.ShouldStopForNoDriver(new[] { (Sat, true), (Sat.AddHours(9), true) }));

    [Fact]
    public void A_completed_leg_on_the_latest_day_breaks_the_streak()
        => Assert.False(PetResanServiceTripRules.ShouldStopForNoDriver(new[] { (Sat, true), (Sat.AddDays(2), true), (Sat.AddDays(2).AddHours(9), false) }));

    [Fact]
    public void A_manual_cancel_between_breaks_the_streak()
        => Assert.False(PetResanServiceTripRules.ShouldStopForNoDriver(new[] { (Sat, true), (Sat.AddDays(2), false), (Sat.AddDays(7), true) }));

    [Fact]
    public void Only_the_latest_days_matter()
        => Assert.True(PetResanServiceTripRules.ShouldStopForNoDriver(new[] { (Sat.AddDays(-7), false), (Sat, true), (Sat.AddDays(2), true) }));

    [Fact]
    public void Tehran_now_is_utc_plus_three_thirty()
    {
        var tehran = PetResanServiceTripRules.TehranNow(new DateTime(2026, 10, 6, 12, 0, 0, DateTimeKind.Utc));
        Assert.Equal(new DateTime(2026, 10, 6, 15, 30, 0), tehran);
    }

    [Fact]
    public void Tehran_now_crosses_midnight_correctly()
    {
        // ۲۰:۳۰ UTC = ۰۰:۰۰ تهران روز بعد
        var tehran = PetResanServiceTripRules.TehranNow(new DateTime(2026, 10, 6, 20, 30, 0, DateTimeKind.Utc));
        Assert.Equal(new DateTime(2026, 10, 7, 0, 0, 0), tehran);
    }
}
