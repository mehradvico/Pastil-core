using Application.Services.CompanionSrvs.CompanionReserveDebtSrv;
using System;
using Xunit;

namespace Application.Tests;

// قفل ۷ روزه‌ی رزرو برای کاربری که هزینه‌ی نهایی خدمت را پرداخت نکرده
public class CompanionReserveDebtRulesTests
{
    private static readonly DateTime Now = new(2026, 9, 25, 12, 0, 0);

    [Fact]
    public void No_debt_means_not_locked() => Assert.False(CompanionReserveDebtRules.IsLocked(null, Now));

    [Fact]
    public void A_debt_younger_than_seven_days_does_not_lock()
        => Assert.False(CompanionReserveDebtRules.IsLocked(new DateTime?[] { Now.AddDays(-6) }, Now));

    [Fact]
    public void A_debt_of_exactly_seven_days_locks()
        => Assert.True(CompanionReserveDebtRules.IsLocked(new DateTime?[] { Now.AddDays(-7) }, Now));

    [Fact]
    public void One_overdue_debt_among_fresh_ones_locks()
        => Assert.True(CompanionReserveDebtRules.IsLocked(new DateTime?[] { Now.AddDays(-1), Now.AddDays(-9) }, Now));

    [Fact]
    public void Days_left_counts_down_and_never_goes_negative()
    {
        Assert.Equal(7, CompanionReserveDebtRules.DaysLeft(Now, Now));
        Assert.Equal(3, CompanionReserveDebtRules.DaysLeft(Now.AddDays(-4), Now));
        Assert.Equal(0, CompanionReserveDebtRules.DaysLeft(Now.AddDays(-30), Now));
    }

    [Fact]
    public void Site_share_uses_the_commission_percent()
    {
        Assert.Equal(20, CompanionReserveDebtRules.SiteShareOf(100, 20m));
        Assert.Equal(0, CompanionReserveDebtRules.SiteShareOf(100, 0m));
    }
}
