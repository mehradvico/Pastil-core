using Application.Services.Order.ProductOrderSrv;
using Xunit;
using static Application.Services.Order.ProductOrderSrv.ProductOrderDeliveryRules;

namespace Application.Tests;

public class ProductOrderDeliveryRulesTests
{
    private const string Send = "ProductOrderStatus_Send";
    private const string Normal = "ProductOrderState_Normal";
    private const string Canceled = "ProductOrderState_Canceled";

    [Fact]
    public void A_paid_sent_non_cancelled_order_can_be_answered()
        => Assert.Equal(Decision.Allowed, Decide(true, Send, Normal, null));

    [Fact]
    public void A_not_received_answer_can_be_changed_until_the_order_is_final()
    {
        Assert.Equal(Decision.Allowed, Decide(true, Send, Normal, false));
        Assert.Equal(Decision.AlreadyConfirmed, Decide(true, "ProductOrderStatus_Delivered", Normal, true));
    }

    [Theory]
    [InlineData(false, "ProductOrderStatus_Send", "ProductOrderState_Normal")]     // پرداخت‌نشده
    [InlineData(true, "ProductOrderStatus_Insert", "ProductOrderState_Normal")]   // هنوز ارسال نشده
    [InlineData(true, "ProductOrderStatus_Proccess", "ProductOrderState_Normal")]
    [InlineData(true, "ProductOrderStatus_Send", "ProductOrderState_Canceled")]   // لغو شده
    [InlineData(true, "ProductOrderStatus_Delivered", "ProductOrderState_Normal")] // قبلاً (با روش قدیمی) نهایی شده
    public void The_customer_cannot_answer_outside_the_sent_status(bool isPaid, string status, string state)
        => Assert.Equal(Decision.NotAllowed, Decide(isPaid, status, state, null));

    [Fact]
    public void An_edited_order_can_still_be_answered()
        => Assert.Equal(Decision.Allowed, Decide(true, Send, "ProductOrderState_Edited", null));

    [Fact]
    public void Staff_cannot_set_the_delivered_status()
    {
        // 15 درج، 16 در حال پردازش، 17 ارسال، 18 تحویل
        Assert.False(StaffMayChangeStatus(17, 18, 18, null));
        Assert.False(StaffMayChangeStatus(17, 18, 18, false));
        Assert.True(StaffMayChangeStatus(17, 16, 18, null));
        Assert.True(StaffMayChangeStatus(16, 17, 18, null));
    }

    [Fact]
    public void A_legacy_delivered_order_keeps_its_status_and_a_user_confirmed_order_is_locked()
    {
        Assert.True(StaffMayChangeStatus(18, 18, 18, null));   // بی‌اثر
        Assert.True(StaffMayChangeStatus(18, 17, 18, null));   // سفارش قدیمی که ادمین «تحویل» زده بود هنوز قابل اصلاح است
        Assert.False(StaffMayChangeStatus(18, 17, 18, true));  // کاربر تأیید کرده ⇒ قفل
        Assert.True(StaffMayChangeStatus(18, 18, 18, true));
    }

    private static readonly System.DateTime Sent = new(2026, 10, 1, 12, 0, 0);

    [Fact]
    public void Auto_confirm_is_due_exactly_seven_days_after_the_order_was_sent_when_unanswered()
    {
        Assert.False(AutoConfirmDue(Sent, null, Sent.AddDays(7).AddMinutes(-1)));
        Assert.True(AutoConfirmDue(Sent, null, Sent.AddDays(7)));
        Assert.True(AutoConfirmDue(Sent, null, Sent.AddDays(30)));
        Assert.Equal(Sent.AddDays(7), AutoConfirmDate(Sent));
    }

    [Fact]
    public void A_not_received_answer_is_never_auto_confirmed()
    {
        Assert.False(AutoConfirmDue(Sent, false, Sent.AddDays(60)));
        Assert.False(AutoConfirmDue(Sent, true, Sent.AddDays(60)));
    }

    [Fact]
    public void An_order_without_a_sent_date_is_not_auto_confirmed_until_the_job_stamps_it()
        => Assert.False(AutoConfirmDue(null, null, Sent.AddDays(60)));

    [Fact]
    public void Warning_window_is_day_five_up_to_the_auto_confirm_moment_for_unanswered_orders()
    {
        Assert.False(WarnDue(Sent, null, Sent.AddDays(5).AddMinutes(-1)));
        Assert.True(WarnDue(Sent, null, Sent.AddDays(5)));
        Assert.True(WarnDue(Sent, null, Sent.AddDays(7).AddMinutes(-1)));
        Assert.False(WarnDue(Sent, null, Sent.AddDays(7)));
        Assert.False(WarnDue(Sent, false, Sent.AddDays(6)));
        Assert.False(WarnDue(Sent, true, Sent.AddDays(6)));
        Assert.False(WarnDue(null, null, Sent.AddDays(6)));
    }

    [Fact]
    public void Push_keys_are_unique_per_order_and_kind()
    {
        Assert.Equal("orderwarn:abc", WarnKey("abc"));
        Assert.Equal("orderauto:abc", AutoKey("abc"));
        Assert.NotEqual(WarnKey("abc"), WarnKey("abd"));
        Assert.NotEqual(WarnKey("abc"), AutoKey("abc"));
        Assert.Equal("ordernotrecv:abc", NotReceivedKey("abc"));
        Assert.NotEqual(NotReceivedKey("abc"), WarnKey("abc"));
    }
}
