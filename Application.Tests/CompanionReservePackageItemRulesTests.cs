using Application.Common.Enumerable;
using Application.Services.CompanionSrvs.CompanionReservePackageItemSrv;
using System;
using Xunit;

namespace Application.Tests;

public class CompanionReservePackageItemRulesTests
{
    [Theory]
    [InlineData(1, 2, true)]
    [InlineData(1, 3, true)]
    [InlineData(2, 3, true)]
    [InlineData(2, 2, false)]
    [InlineData(2, 1, false)]
    [InlineData(3, 2, false)]
    [InlineData(3, 3, false)]
    public void Package_status_transitions(int from, int to, bool allowed)
        => Assert.Equal(allowed, CompanionReservePackageItemRules.CanTransition(from, to));

    [Theory]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(1, false)]
    [InlineData(0, false)]
    [InlineData(9, false)]
    public void Only_approve_and_cancel_are_valid_targets(int status, bool valid)
        => Assert.Equal(valid, CompanionReservePackageItemRules.IsValidTarget(status));

    [Fact]
    public void Ten_equal_packages_four_cancelled_refunds_proportionally()
    {
        // ۱۰ پکیج هم‌وزن، ۱٬۰۰۰٬۰۰۰ پرداخت‌شده: لغو اولی = ۱۰۰٬۰۰۰ از باقی‌مانده‌ی ۱٬۰۰۰٬۰۰۰
        double remaining = 1_000_000;
        double total = 10 * 100_000;
        var refunded = 0d;
        for (var active = 10; active > 6; active--)
        {
            var r = CompanionReservePackageItemRules.RefundShare(remaining, 100_000, 100_000 * active, false);
            refunded += r;
            remaining -= r;
        }
        Assert.Equal(400_000, refunded);
        Assert.Equal(600_000, remaining);
    }

    [Fact]
    public void Unequal_weights_are_proportional_to_prepayment()
    {
        // پرداخت‌شده ۳۰۰٬۰۰۰ = وزن‌های ۱۰۰٬۰۰۰ و ۲۰۰٬۰۰۰ ⇒ لغو پکیج ۲۰۰٬۰۰۰ = ۲۰۰٬۰۰۰
        Assert.Equal(200_000, CompanionReservePackageItemRules.RefundShare(300_000, 200_000, 300_000, false));
    }

    [Fact]
    public void Rebate_reduces_refund_proportionally()
    {
        // پیش‌پرداخت اسمی ۲۰۰٬۰۰۰ ولی با تخفیف ۱۰۰٬۰۰۰ پرداخت شده ⇒ هر پکیج ۵۰٬۰۰۰ برمی‌گردد
        Assert.Equal(50_000, CompanionReservePackageItemRules.RefundShare(100_000, 100_000, 200_000, false));
    }

    [Fact]
    public void Last_active_package_refunds_everything_that_remains()
    {
        Assert.Equal(333_334, CompanionReservePackageItemRules.RefundShare(333_334, 1, 3, true));
    }

    [Fact]
    public void Package_added_after_payment_refunds_nothing_unless_last()
    {
        Assert.Equal(0, CompanionReservePackageItemRules.RefundShare(500_000, 0, 500_000, false));
        Assert.Equal(500_000, CompanionReservePackageItemRules.RefundShare(500_000, 0, 0, true));
    }

    [Fact]
    public void Refund_never_exceeds_remaining_and_nothing_remaining_means_zero()
    {
        Assert.Equal(0, CompanionReservePackageItemRules.RefundShare(0, 100, 100, true));
        Assert.Equal(100, CompanionReservePackageItemRules.RefundShare(100, 500, 500, false));
    }

    [Theory]
    [InlineData(false, 31, 34, true)]   // پیش‌پرداخت‌شده، خدمت انجام نشده
    [InlineData(false, 30, 34, true)]   // ثبت‌شده
    [InlineData(true, 31, 34, false)]   // لغوشده
    [InlineData(false, 32, 34, false)]  // نهایی‌شده
    [InlineData(false, 33, 34, false)]  // کامل
    [InlineData(false, 31, 35, false)]  // اپراتور «کامل» زده
    [InlineData(false, 31, 36, false)]  // اپراتور «لغو» زده
    public void Reserve_modifiable_rules(bool isCancel, long state, long operatorState, bool expected)
        => Assert.Equal(expected, CompanionReservePackageItemRules.ReserveIsModifiable(isCancel, state, operatorState, null));

    [Fact]
    public void Done_reserve_is_not_modifiable()
        => Assert.False(CompanionReservePackageItemRules.ReserveIsModifiable(false, 31, 34, new DateTime(2026, 10, 4)));

    [Fact]
    public void Refund_ledger_name_is_per_item()
        => Assert.Equal("CompanionPackageRefund:7", CompanionReservePackageItemRules.RefundLedgerName(7));
}
