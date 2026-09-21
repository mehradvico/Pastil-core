using Application.Common.Enumerable;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv;
using System;
using Xunit;

namespace Application.Tests;

public class ConsultationPurchaseRulesTests
{
    private static readonly DateTime Now = new(2026, 9, 21, 12, 0, 0);
    private const int Pending = (int)ConsultationPurchaseStatusEnum.PendingPayment;
    private const int Paid = (int)ConsultationPurchaseStatusEnum.Paid;
    private const int Active = (int)ConsultationPurchaseStatusEnum.Active;
    private const int Completed = (int)ConsultationPurchaseStatusEnum.Completed;
    private const int Cancelled = (int)ConsultationPurchaseStatusEnum.Cancelled;

    [Fact]
    public void Gateway_only_purchase_pays_the_whole_price_through_the_gateway()
    {
        var a = ConsultationPurchaseRules.ComputeAmounts(150_000, 0, fromWallet: false, walletBalance: 500_000);

        Assert.Equal((150_000d, 0d, 150_000d, 0d, 150_000d), (a.Price, a.RebatePrice, a.Payable, a.Wallet, a.Gateway));
    }

    [Fact]
    public void Wallet_that_covers_everything_leaves_nothing_for_the_gateway()
    {
        var a = ConsultationPurchaseRules.ComputeAmounts(150_000, 0, fromWallet: true, walletBalance: 200_000);

        Assert.Equal(150_000, a.Wallet);
        Assert.Equal(0, a.Gateway);
    }

    [Fact]
    public void A_wallet_top_up_never_leaves_the_gateway_below_its_minimum()
    {
        // موجودی ۱۴۵٬۰۰۰ از ۱۵۰٬۰۰۰: سهم درگاه ۵٬۰۰۰ می‌شد (کمتر از حداقل ۱۰٬۰۰۰) ⇒ کیف پول فقط تا ۱۴۰٬۰۰۰ برداشت می‌شود
        var a = ConsultationPurchaseRules.ComputeAmounts(150_000, 0, fromWallet: true, walletBalance: 145_000);

        Assert.Equal(140_000, a.Wallet);
        Assert.Equal(10_000, a.Gateway);
        Assert.Equal(a.Payable, a.Wallet + a.Gateway);
    }

    [Fact]
    public void Rebate_reduces_the_payable_amount_and_is_capped_at_the_price()
    {
        var partial = ConsultationPurchaseRules.ComputeAmounts(100_000, 30_000, false, 0);
        Assert.Equal((30_000d, 70_000d, 70_000d), (partial.RebatePrice, partial.Payable, partial.Gateway));

        var oversized = ConsultationPurchaseRules.ComputeAmounts(100_000, 999_999, false, 0);
        Assert.Equal((100_000d, 0d, 0d), (oversized.RebatePrice, oversized.Payable, oversized.Gateway));

        var negative = ConsultationPurchaseRules.ComputeAmounts(100_000, -5, false, 0);
        Assert.Equal((0d, 100_000d), (negative.RebatePrice, negative.Payable));
    }

    [Fact]
    public void Amounts_always_add_up()
    {
        foreach (var (price, rebate, wallet) in new[] { (200_000d, 0d, 0d), (200_000d, 20_000d, 50_000d), (90_000d, 90_000d, 10_000d), (400_000d, 100_000d, 1_000_000d) })
        {
            var a = ConsultationPurchaseRules.ComputeAmounts(price, rebate, true, wallet);
            Assert.Equal(a.Payable, a.Wallet + a.Gateway, 6);
            Assert.Equal(a.Price, a.Payable + a.RebatePrice, 6);
        }
    }

    [Theory]
    [InlineData(Paid, true)]
    [InlineData(Pending, false)]
    [InlineData(Active, false)]      // بعد از شروع لغو کاربر ممکن نیست
    [InlineData(Completed, false)]
    [InlineData(Cancelled, false)]
    public void Only_a_paid_and_not_started_purchase_can_be_cancelled_by_the_user(int status, bool expected)
    {
        Assert.Equal(expected, ConsultationPurchaseRules.CanUserCancel(status));
    }

    [Fact]
    public void Start_is_allowed_only_for_paid_purchases_until_the_deadline()
    {
        var deadline = Now.AddHours(24);

        Assert.True(ConsultationPurchaseRules.CanStart(Paid, deadline, Now));
        Assert.True(ConsultationPurchaseRules.CanStart(Paid, deadline, deadline));           // دقیقاً لحظه‌ی مهلت هنوز مجاز
        Assert.False(ConsultationPurchaseRules.CanStart(Paid, deadline, deadline.AddSeconds(1)));
        Assert.False(ConsultationPurchaseRules.CanStart(Active, deadline, Now));             // شروع مجدد ممکن نیست
        Assert.False(ConsultationPurchaseRules.CanStart(Pending, deadline, Now));
        Assert.False(ConsultationPurchaseRules.CanStart(Cancelled, deadline, Now));
    }

    [Fact]
    public void Only_a_paid_purchase_past_its_deadline_is_overdue()
    {
        var deadline = Now.AddHours(-1);

        Assert.True(ConsultationPurchaseRules.IsStartOverdue(Paid, deadline, Now));
        Assert.False(ConsultationPurchaseRules.IsStartOverdue(Paid, Now.AddHours(1), Now));
        Assert.False(ConsultationPurchaseRules.IsStartOverdue(Paid, null, Now));
        Assert.False(ConsultationPurchaseRules.IsStartOverdue(Active, deadline, Now));      // مشاوره‌ی شروع‌شده هرگز به‌خاطر مهلت شروع لغو/بازپرداخت نمی‌شود
        Assert.False(ConsultationPurchaseRules.IsStartOverdue(Cancelled, deadline, Now));
    }

    [Fact]
    public void Window_is_open_only_while_active_and_before_expiry()
    {
        var expire = Now.AddMinutes(30);

        Assert.True(ConsultationPurchaseRules.IsWindowOpen(Active, expire, Now));
        Assert.False(ConsultationPurchaseRules.IsWindowOpen(Active, expire, expire));        // لحظه‌ی پایان دیگر باز نیست
        Assert.False(ConsultationPurchaseRules.IsWindowOpen(Active, expire, expire.AddMinutes(1)));
        Assert.False(ConsultationPurchaseRules.IsWindowOpen(Paid, expire, Now));
        Assert.False(ConsultationPurchaseRules.IsWindowOpen(Active, null, Now));
    }

    [Fact]
    public void Only_the_starting_agent_or_the_clinic_owner_can_re_enter_an_open_window()
    {
        var expire = Now.AddMinutes(20);
        const long starter = 10, otherStaff = 11, owner = 12;

        Assert.True(ConsultationPurchaseRules.CanAgentEnter(Active, expire, Now, starter, starter, isOwner: false));
        Assert.True(ConsultationPurchaseRules.CanAgentEnter(Active, expire, Now, starter, owner, isOwner: true));
        Assert.False(ConsultationPurchaseRules.CanAgentEnter(Active, expire, Now, starter, otherStaff, isOwner: false));
        // بعد از پایان پنجره یا قبل از شروع هیچ‌کس وارد نمی‌شود
        Assert.False(ConsultationPurchaseRules.CanAgentEnter(Active, expire, expire, starter, starter, isOwner: true));
        Assert.False(ConsultationPurchaseRules.CanAgentEnter(Paid, null, Now, null, owner, isOwner: true));
        Assert.False(ConsultationPurchaseRules.CanAgentEnter(Completed, expire, Now, starter, starter, isOwner: true));
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    public void Expiry_is_start_plus_duration(int minutes)
    {
        Assert.Equal(Now.AddMinutes(minutes), ConsultationPurchaseRules.ComputeExpireDate(Now, minutes));
    }

    [Fact]
    public void Start_deadline_is_24_hours_after_payment()
    {
        Assert.Equal(Now.AddHours(24), ConsultationPurchaseRules.ComputeStartDeadline(Now));
    }

    [Theory]
    [InlineData(120_000, 120_000)]
    [InlineData(0, 0)]
    [InlineData(-10, 0)]
    public void Refund_is_what_the_user_actually_paid(double paymentPrice, double expected)
    {
        Assert.Equal(expected, ConsultationPurchaseRules.RefundAmount(paymentPrice));
    }

    [Fact]
    public void Purchase_code_does_not_depend_on_the_server_culture()
    {
        // سرور با فرهنگ fa-IR (تقویم شمسی) اجرا می‌شود؛ کد باید همیشه میلادی و ثابت بماند
        var previous = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("fa-IR");
            Assert.Equal("CNS-20260921-1200-0042", ConsultationPurchaseRules.NewPurchaseCode(Now, 42));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Purchase_code_and_refund_ledger_name_are_stable_formats()
    {
        Assert.Equal("CNS-20260921-1200-0042", ConsultationPurchaseRules.NewPurchaseCode(Now, 42));
        Assert.Equal("CNS-20260921-1200-0042", ConsultationPurchaseRules.NewPurchaseCode(Now, 10042));
        Assert.Equal("ConsultationRefund:77", ConsultationPurchaseRules.RefundLedgerName(77));
    }
}
