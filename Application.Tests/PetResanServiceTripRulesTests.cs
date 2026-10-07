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
