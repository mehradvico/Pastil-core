using Application.Common.Enumerable.Code;
using Application.Services.PansionSrvs.PansionReserveSrv;
using System;
using Xunit;

namespace Application.Tests;

public class PansionReserveApprovalRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 7, 10, 0, 0);

    [Fact]
    public void Deadline_is_twelve_hours_when_the_reserve_is_far_away()
        => Assert.Equal(Now.AddHours(12), PansionReserveApprovalRules.Deadline(Now, Now.AddDays(5)));

    [Fact]
    public void Deadline_is_capped_one_hour_before_the_start()
        => Assert.Equal(Now.AddHours(4), PansionReserveApprovalRules.Deadline(Now, Now.AddHours(5)));

    [Fact]
    public void Deadline_never_goes_below_the_minimum_window()
    {
        // شروع ۲۰ دقیقه‌ی دیگر: سقف «یک ساعت قبل از شروع» گذشته؛ حداقل ۳۰ دقیقه مهلت می‌ماند
        Assert.Equal(Now.AddMinutes(30), PansionReserveApprovalRules.Deadline(Now, Now.AddMinutes(20)));
        Assert.Equal(Now.AddMinutes(30), PansionReserveApprovalRules.Deadline(Now, Now.AddHours(-3)));
    }

    [Fact]
    public void Deadline_without_a_known_start_is_the_plain_window()
        => Assert.Equal(Now.AddHours(12), PansionReserveApprovalRules.Deadline(Now, null));

    [Theory]
    [InlineData(true, false, 1, true)]    // پرداخت‌شده، لغو‌نشده، منتظر پاسخ
    [InlineData(false, false, 1, false)]  // پرداخت‌نشده
    [InlineData(true, true, 1, false)]    // لغو‌شده
    [InlineData(true, false, 0, false)]   // قدیمی/نیاز به تأیید ندارد
    [InlineData(true, false, 2, false)]   // قبلاً تأیید شده
    [InlineData(true, false, 3, false)]   // قبلاً رد شده
    [InlineData(true, false, 4, false)]   // قبلاً منقضی شده
    public void Only_a_paid_pending_reserve_can_be_decided(bool reserved, bool cancelled, int decision, bool expected)
        => Assert.Equal(expected, PansionReserveApprovalRules.CanDecide(reserved, cancelled, decision));

    [Theory]
    [InlineData(0, true)]   // رزروهای قبل از قابلیت مثل قبل
    [InlineData(2, true)]
    [InlineData(1, false)]  // منتظر تأیید: تکمیل نمی‌شود
    [InlineData(3, false)]
    [InlineData(4, false)]
    public void Only_approved_or_legacy_reserves_can_be_completed(int decision, bool expected)
        => Assert.Equal(expected, PansionReserveApprovalRules.CanComplete(decision));

    [Fact]
    public void Overdue_only_applies_to_pending_reserves_past_the_deadline()
    {
        Assert.True(PansionReserveApprovalRules.IsOverdue((int)PansionReserveOwnerDecisionEnum.Pending, Now.AddMinutes(-1), Now));
        Assert.False(PansionReserveApprovalRules.IsOverdue((int)PansionReserveOwnerDecisionEnum.Pending, Now.AddMinutes(1), Now));
        Assert.False(PansionReserveApprovalRules.IsOverdue((int)PansionReserveOwnerDecisionEnum.Approved, Now.AddMinutes(-1), Now));
        Assert.False(PansionReserveApprovalRules.IsOverdue((int)PansionReserveOwnerDecisionEnum.Pending, null, Now));
    }

    [Fact]
    public void Refund_ledger_is_unique_per_reserve()
    {
        Assert.Equal("PansionReserveRefund:5", PansionReserveApprovalRules.RefundLedgerName(5));
        Assert.NotEqual(PansionReserveApprovalRules.RefundLedgerName(5), PansionReserveApprovalRules.RefundLedgerName(6));
    }
}
