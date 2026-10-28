using Application.Common.Enumerable.Code;
using Application.Services.SchoolSrvs.SchoolCourseSrv;
using System;
using System.Collections.Generic;
using Xunit;

namespace Application.Tests;

// قواعد ویژگی‌های تکمیلی مدرسه (۲۰۲۶-۱۰-۲۸): ثبت‌نام دیرهنگام، قیمت نسبی، درصد برگشت وجه
public class SchoolCourseFeatureRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 28, 12, 0, 0);

    // ۴ جلسه: یکی گذشته، سه تا آینده
    private static List<DateTime> Started() => new() { Now.AddDays(-2), Now.AddDays(1), Now.AddDays(3), Now.AddDays(5) };

    [Fact]
    public void Before_first_session_is_always_open_at_full_price()
    {
        var future = new List<DateTime> { Now.AddDays(1), Now.AddDays(2) };
        foreach (var mode in Enum.GetValues<SchoolLateEnrollmentEnum>())
        {
            var w = SchoolCourseRules.Enrollment(400_000, (int)mode, future, Now);
            Assert.True(w.Open);
            Assert.Equal(400_000, w.Price);
        }
    }

    [Fact]
    public void After_start_mode_decides_open_and_price()
    {
        Assert.False(SchoolCourseRules.Enrollment(400_000, (int)SchoolLateEnrollmentEnum.Closed, Started(), Now).Open);

        var full = SchoolCourseRules.Enrollment(400_000, (int)SchoolLateEnrollmentEnum.OpenFullPrice, Started(), Now);
        Assert.True(full.Open);
        Assert.Equal(400_000, full.Price);

        var prorated = SchoolCourseRules.Enrollment(400_000, (int)SchoolLateEnrollmentEnum.OpenProrated, Started(), Now);
        Assert.True(prorated.Open);
        Assert.Equal(300_000, prorated.Price); // ۳ از ۴ جلسه
        Assert.Equal(3, prorated.RemainingSessions);
    }

    [Fact]
    public void Nothing_left_means_closed_in_every_mode()
    {
        var past = new List<DateTime> { Now.AddDays(-3), Now.AddDays(-1) };
        Assert.False(SchoolCourseRules.Enrollment(400_000, (int)SchoolLateEnrollmentEnum.OpenProrated, past, Now).Open);
    }

    [Fact]
    public void Video_course_without_sessions_is_open()
    {
        var w = SchoolCourseRules.Enrollment(100_000, (int)SchoolLateEnrollmentEnum.Closed, new List<DateTime>(), Now);
        Assert.True(w.Open);
        Assert.Equal(100_000, w.Price);
    }

    [Fact]
    public void Refund_percent_follows_the_policy()
    {
        // ۴۸ ساعت مانده، سیاست ۲۴ ساعت ⇒ ۱۰۰٪
        Assert.Equal(100, SchoolCourseRules.RefundPercent(Now.AddHours(48), Now, 24, 50));
        // ۱۰ ساعت مانده ⇒ ۵۰٪
        Assert.Equal(50, SchoolCourseRules.RefundPercent(Now.AddHours(10), Now, 24, 50));
        // سیاست «بدون برگشت دیرهنگام»
        Assert.Equal(0, SchoolCourseRules.RefundPercent(Now.AddHours(10), Now, 24, 0));
        // دوره‌ی بدون جلسه ⇒ ۱۰۰٪
        Assert.Equal(100, SchoolCourseRules.RefundPercent(null, Now, 24, 50));
    }
}
