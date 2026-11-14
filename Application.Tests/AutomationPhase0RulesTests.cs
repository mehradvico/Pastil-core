using Application.Common.Enumerable.Code;
using Application.Services.MarketingSrv;
using Application.Services.Order.CartSrv;
using Xunit;

namespace Application.Tests;

public class AutomationPhase0RulesTests
{
    private static readonly DateTime Now = new(2026, 11, 15, 12, 0, 0); // شنبه

    // ---------------- سبد رها شده: ۱ ساعت پوش، ۳ ساعت پوش، ۲۴ ساعت پیامک

    [Fact]
    public void Cart_first_push_after_one_hour()
    {
        Assert.Equal(0, AbandonedCartReminderRules.NextPush(Now.AddMinutes(-50), Now, 0, null));
        Assert.Equal(1, AbandonedCartReminderRules.NextPush(Now.AddMinutes(-61), Now, 0, null));
    }

    [Fact]
    public void Cart_second_push_after_three_hours_and_two_hours_after_the_first()
    {
        Assert.Equal(0, AbandonedCartReminderRules.NextPush(Now.AddHours(-2), Now, 1, Now.AddHours(-1)));
        Assert.Equal(2, AbandonedCartReminderRules.NextPush(Now.AddHours(-3).AddMinutes(-1), Now, 1, Now.AddHours(-2)));
        // پوش اول به خاطر ساعت سکوت دیر رفته: پوش دوم دو ساعت بعد از آن
        Assert.Equal(0, AbandonedCartReminderRules.NextPush(Now.AddHours(-11), Now, 1, Now.AddMinutes(-10)));
        Assert.Equal(2, AbandonedCartReminderRules.NextPush(Now.AddHours(-13), Now, 1, Now.AddHours(-2)));
    }

    [Fact]
    public void Cart_never_more_than_two_pushes_and_none_after_24h()
    {
        Assert.Equal(0, AbandonedCartReminderRules.NextPush(Now.AddHours(-5), Now, 2, Now.AddHours(-3)));
        Assert.Equal(0, AbandonedCartReminderRules.NextPush(Now.AddHours(-25), Now, 0, null));
    }

    [Fact]
    public void Cart_one_sms_after_24h_within_three_days()
    {
        Assert.False(AbandonedCartReminderRules.IsSmsDue(Now.AddHours(-23), Now, 0));
        Assert.True(AbandonedCartReminderRules.IsSmsDue(Now.AddHours(-24), Now, 0));
        Assert.False(AbandonedCartReminderRules.IsSmsDue(Now.AddHours(-30), Now, 1));
        Assert.False(AbandonedCartReminderRules.IsSmsDue(Now.AddDays(-4), Now, 0));
    }

    // ---------------- ساعت سکوت

    [Theory]
    [InlineData(8, 59, false)]
    [InlineData(9, 0, true)]
    [InlineData(21, 29, true)]
    [InlineData(21, 30, false)]
    [InlineData(2, 0, false)]
    public void Push_quiet_hours(int hour, int minute, bool allowed)
        => Assert.Equal(allowed, MarketingQuietHours.IsPushAllowed(new DateTime(2026, 11, 15, hour, minute, 0)));

    [Fact]
    public void Sms_quiet_hours_and_friday_morning()
    {
        Assert.True(MarketingQuietHours.IsSmsAllowed(new DateTime(2026, 11, 15, 9, 0, 0)));
        Assert.False(MarketingQuietHours.IsSmsAllowed(new DateTime(2026, 11, 15, 21, 0, 0)));
        var friday = new DateTime(2026, 11, 20, 10, 30, 0);
        Assert.Equal(DayOfWeek.Friday, friday.DayOfWeek);
        Assert.False(MarketingQuietHours.IsSmsAllowed(friday));
        Assert.True(MarketingQuietHours.IsSmsAllowed(friday.AddMinutes(30)));
    }

    [Fact]
    public void Cart_sms_only_between_10_and_19()
    {
        Assert.False(MarketingQuietHours.IsCartSmsAllowed(new DateTime(2026, 11, 15, 9, 30, 0)));
        Assert.True(MarketingQuietHours.IsCartSmsAllowed(new DateTime(2026, 11, 15, 10, 0, 0)));
        Assert.False(MarketingQuietHours.IsCartSmsAllowed(new DateTime(2026, 11, 15, 19, 0, 0)));
        Assert.False(MarketingQuietHours.IsCartSmsAllowed(new DateTime(2026, 11, 20, 10, 30, 0))); // جمعه
    }

    // ---------------- پرداخت ناموفق و رزرو پرداخت‌نشده

    [Fact]
    public void Failed_payment_push_after_5_minutes_abandoned_after_20()
    {
        Assert.False(PaymentReminderRules.IsFailedPushDue(false, Now.AddMinutes(-4), Now));
        Assert.True(PaymentReminderRules.IsFailedPushDue(false, Now.AddMinutes(-5), Now));
        Assert.False(PaymentReminderRules.IsFailedPushDue(null, Now.AddMinutes(-10), Now));
        Assert.True(PaymentReminderRules.IsFailedPushDue(null, Now.AddMinutes(-20), Now));
        Assert.False(PaymentReminderRules.IsFailedPushDue(true, Now.AddMinutes(-30), Now));
        Assert.False(PaymentReminderRules.IsFailedPushDue(false, Now.AddHours(-25), Now));
    }

    [Fact]
    public void Failed_payment_sms_one_hour_after_the_push()
    {
        Assert.False(PaymentReminderRules.IsFailedSmsDue(Now.AddHours(-2), Now.AddMinutes(-30), Now));
        Assert.True(PaymentReminderRules.IsFailedSmsDue(Now.AddHours(-2), Now.AddHours(-1), Now));
    }

    [Fact]
    public void Unpaid_reserve_push_30_minutes_sms_3_hours()
    {
        Assert.False(PaymentReminderRules.IsUnpaidPushDue(Now.AddMinutes(-29), Now));
        Assert.True(PaymentReminderRules.IsUnpaidPushDue(Now.AddMinutes(-30), Now));
        Assert.False(PaymentReminderRules.IsUnpaidSmsDue(Now.AddHours(-2), Now.AddHours(-1.5), Now));
        Assert.True(PaymentReminderRules.IsUnpaidSmsDue(Now.AddHours(-3), Now.AddHours(-2.5), Now));
        Assert.False(PaymentReminderRules.IsUnpaidSmsDue(Now.AddHours(-3), Now.AddHours(-1), Now)); // پوش دیر رفته
        Assert.False(PaymentReminderRules.IsUnpaidPushDue(Now.AddHours(-25), Now));
    }

    [Theory]
    [InlineData("ProductOrder", "x", "/orders")]
    [InlineData("ClinicReserve", "5", "/reserve")]
    [InlineData("Trip", "12", "/trip/12")]
    [InlineData("Wallet", null, "/wallet")]
    [InlineData("ConsultationPurchase", "3", "/consultations")]
    [InlineData("Nope", null, "/")]
    public void Retry_path_by_payment_kind(string label, string id, string expected)
        => Assert.Equal(expected, PaymentReminderRules.RetryPath(label, id));

    [Fact]
    public void New_push_type_ids_stay_stable()
    {
        Assert.Equal(175, (int)PushTypeEnum.PushClubPetProfileGift);
        Assert.Equal(176, (int)PushTypeEnum.PushPaymentFailed);
        Assert.Equal(177, (int)PushTypeEnum.PushUnpaidReserve);
        Assert.Equal(178, (int)PushTypeEnum.PushAbandonedCartSecond);
    }
}
