using Application.Services.Order.ShippingSrv;
using Entities.Entities.ShippingField;
using System;
using Xunit;

namespace Application.Tests;

public class ShipmentLifecycleRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(ShipmentStatusEnum.Requested, ShipmentStatusEnum.Accepted, true)]
    [InlineData(ShipmentStatusEnum.Accepted, ShipmentStatusEnum.PickedUp, true)]
    [InlineData(ShipmentStatusEnum.Requested, ShipmentStatusEnum.Delivered, true)]   // رویدادهای میانی از دست رفته
    [InlineData(ShipmentStatusEnum.PickedUp, ShipmentStatusEnum.Accepted, false)]    // وب‌هوک جابه‌جا
    [InlineData(ShipmentStatusEnum.PickedUp, ShipmentStatusEnum.PickedUp, false)]    // وب‌هوک تکراری
    [InlineData(ShipmentStatusEnum.Delivered, ShipmentStatusEnum.Cancelled, false)]  // نهایی عوض نمی‌شود
    [InlineData(ShipmentStatusEnum.Failed, ShipmentStatusEnum.Accepted, false)]
    [InlineData(ShipmentStatusEnum.Accepted, ShipmentStatusEnum.Cancelled, true)]
    [InlineData(ShipmentStatusEnum.PickedUp, ShipmentStatusEnum.Failed, true)]
    public void Webhook_states_only_move_forward(ShipmentStatusEnum current, ShipmentStatusEnum next, bool expected)
        => Assert.Equal(expected, ShipmentLifecycleRules.CanAdvance(current, next));

    [Fact]
    public void The_delivery_code_is_visible_only_after_the_courier_picked_up()
    {
        Assert.False(ShipmentLifecycleRules.IsCodeVisible(null));
        Assert.False(ShipmentLifecycleRules.IsCodeVisible(ShipmentStatusEnum.AwaitingSellerConfirm));
        Assert.False(ShipmentLifecycleRules.IsCodeVisible(ShipmentStatusEnum.Requested));
        Assert.False(ShipmentLifecycleRules.IsCodeVisible(ShipmentStatusEnum.Accepted));
        Assert.True(ShipmentLifecycleRules.IsCodeVisible(ShipmentStatusEnum.PickedUp));
        Assert.True(ShipmentLifecycleRules.IsCodeVisible(ShipmentStatusEnum.Delivered));
        Assert.False(ShipmentLifecycleRules.IsCodeVisible(ShipmentStatusEnum.Cancelled));
    }

    [Fact]
    public void Not_received_can_be_reported_for_three_hours_after_courier_delivery()
    {
        Assert.True(ShipmentLifecycleRules.InDisputeWindow(Now.AddHours(-2), Now));
        Assert.True(ShipmentLifecycleRules.InDisputeWindow(Now.AddHours(-3), Now));
        Assert.False(ShipmentLifecycleRules.InDisputeWindow(Now.AddHours(-3).AddMinutes(-1), Now));
        Assert.False(ShipmentLifecycleRules.InDisputeWindow(null, Now));
    }

    [Fact]
    public void A_cancelled_courier_gets_one_retry_only_while_useful_time_is_left()
    {
        var slotEnd = Now.AddHours(5);
        Assert.True(ShipmentLifecycleRules.MayRetryCourier(0, slotEnd, Now, 60, 60));
        Assert.False(ShipmentLifecycleRules.MayRetryCourier(1, slotEnd, Now, 60, 60));          // قبلاً یک‌بار
        Assert.False(ShipmentLifecycleRules.MayRetryCourier(0, Now.AddMinutes(80), Now, 60, 60)); // وقت مفید نمانده
        Assert.False(ShipmentLifecycleRules.MayRetryCourier(0, null, Now, 60, 60));
    }

    [Fact]
    public void The_received_question_is_asked_only_after_every_active_slot_has_ended()
    {
        Assert.True(ShipmentLifecycleRules.ShouldAskReceived(new DateTime?[] { Now.AddMinutes(-1) }, Now));
        Assert.True(ShipmentLifecycleRules.ShouldAskReceived(new DateTime?[] { Now, Now.AddHours(-2) }, Now));       // راس پایان بازه
        Assert.False(ShipmentLifecycleRules.ShouldAskReceived(new DateTime?[] { Now.AddMinutes(1) }, Now));          // هنوز تمام نشده
        Assert.False(ShipmentLifecycleRules.ShouldAskReceived(new DateTime?[] { Now.AddHours(-1), Now.AddHours(3) }, Now)); // چندفروشگاه: بازه‌ی دیرتر
        Assert.False(ShipmentLifecycleRules.ShouldAskReceived(new DateTime?[] { null }, Now));
        Assert.False(ShipmentLifecycleRules.ShouldAskReceived(Array.Empty<DateTime?>(), Now));
    }

    [Fact]
    public void Sms_tokens_never_contain_plain_spaces()
    {
        Assert.Equal("پت‌شاپ‌نبات", ShipmentLifecycleRules.ForSmsToken("  پت شاپ   نبات "));
        Assert.DoesNotContain(' ', ShipmentLifecycleRules.ForSmsToken("a b c"));
        Assert.Equal(10, ShipmentLifecycleRules.ForSmsToken(new string('x', 50), 10).Length);
        Assert.Equal(string.Empty, ShipmentLifecycleRules.ForSmsToken(null));
    }

    [Fact]
    public void Slot_hours_are_short_for_round_hours()
    {
        Assert.Equal("9", ShipmentLifecycleRules.FormatHour(TimeSpan.FromHours(9)));
        Assert.Equal("13", ShipmentLifecycleRules.FormatHour(TimeSpan.FromHours(13)));
        Assert.Equal("9:30", ShipmentLifecycleRules.FormatHour(new TimeSpan(9, 30, 0)));
    }
}
