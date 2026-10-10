using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Services.Order.CartSrv;
using System;
using System.Linq;
using Xunit;

namespace Application.Tests;

public class AbandonedCartReminderRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 5, 18, 0, 0);

    [Fact]
    public void A_cart_idle_for_hours_gets_a_reminder()
        => Assert.True(AbandonedCartReminderRules.IsEligible(Now.AddHours(-5), Now, 0));

    [Fact]
    public void A_cart_still_being_filled_is_left_alone()
        => Assert.False(AbandonedCartReminderRules.IsEligible(Now.AddMinutes(-30), Now, 0));

    [Fact]
    public void At_most_two_reminders_per_channel_then_it_stops()
    {
        Assert.True(AbandonedCartReminderRules.IsEligible(Now.AddHours(-20), Now, 1));
        Assert.False(AbandonedCartReminderRules.IsEligible(Now.AddHours(-44), Now, 2));
    }

    [Fact]
    public void A_cart_older_than_the_safety_window_never_gets_a_reminder()
        => Assert.False(AbandonedCartReminderRules.IsEligible(Now.AddDays(-4), Now, 0));

    [Fact]
    public void Product_names_are_trimmed_on_a_word_boundary()
    {
        var shortened = AbandonedCartReminderRules.ShortProductName("غذای خشک گربه رویال کنین مدل دنتال وزن ۱.۵ کیلوگرم");
        Assert.True(shortened.Length <= AbandonedCartReminderRules.ProductNameMaxLength + 1);
        Assert.EndsWith("…", shortened);
        Assert.Equal("نام کوتاه", AbandonedCartReminderRules.ShortProductName("  نام   کوتاه "));
    }

    [Fact]
    public void Only_the_first_word_of_the_first_name_is_used()
    {
        Assert.Equal("مهراد", AbandonedCartReminderRules.FirstNameOnly("مهراد علی"));
        Assert.Null(AbandonedCartReminderRules.FirstNameOnly("  "));
        Assert.Null(AbandonedCartReminderRules.FirstNameOnly(null));
    }

    [Fact]
    public void Enum_values_used_by_the_jobs_stay_stable()
    {
        Assert.Equal(93, (int)PushTypeEnum.PushAbandonedCart);
        // enum پیامک ordinal است: مقدار جدید باید آخرین عضو بماند
        Assert.Equal(MessageTypeEnum.CompanionReserveRescheduledDriver, Enum.GetValues<MessageTypeEnum>().Last());
        Assert.Equal(MessageTypeEnum.UserAbandonedCart, Enum.GetValues<MessageTypeEnum>().SkipLast(14).Last());
    }
}
