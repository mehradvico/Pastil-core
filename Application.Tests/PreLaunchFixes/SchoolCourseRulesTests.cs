using Application.Services.SchoolSrvs.SchoolCourseSrv;
using System;
using Xunit;

namespace Application.Tests.PreLaunchFixes;

public class SchoolCourseRulesTests
{
    [Fact]
    public void TwoSessionsTwoDaysApart_AreTwoPerWeek_NotSeven()
    {
        var (perWeek, weeks) = SchoolCourseRules.WeeklySummary(new[] { new DateTime(2026, 10, 4), new DateTime(2026, 10, 6) });
        Assert.Equal(2, perWeek);
        Assert.Equal(1, weeks);
    }

    [Fact]
    public void EightSessionsTwicePerWeekOverFourWeeks()
    {
        var start = new DateTime(2026, 10, 3);
        var dates = new[] { 0, 3, 7, 10, 14, 17, 21, 24 };
        var (perWeek, weeks) = SchoolCourseRules.WeeklySummary(Array.ConvertAll(dates, d => start.AddDays(d)));
        Assert.Equal(2, perWeek);
        Assert.Equal(4, weeks);
    }

    [Fact]
    public void RegistrationClosesWhenFirstSessionStarted_EvenIfSessionsWereSavedOutOfOrder()
    {
        var now = new DateTime(2026, 10, 12, 10, 0, 0);
        // جلسه‌ی «۱» تاریخ ۱۴ مهر و جلسه‌ی «۲» تاریخ ۱۲ مهر ساعت ۹ (قبل از الان)
        var first = SchoolCourseRules.FirstSessionStart(new[] { (new DateTime(2026, 10, 14), "09:00"), (new DateTime(2026, 10, 12), "09:00") });

        Assert.Equal(new DateTime(2026, 10, 12, 9, 0, 0), first);
        Assert.False(SchoolCourseRules.IsRegistrationOpenByDate(first, now));
        Assert.True(SchoolCourseRules.IsRegistrationOpenByDate(first, now.AddHours(-2)));
        Assert.True(SchoolCourseRules.IsRegistrationOpenByDate(null, now));
    }
}
