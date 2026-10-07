using Application.Services.TripSrv.TripOngoingSrv;
using System;
using Xunit;

namespace Application.Tests;

public class TripOngoingRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 6, 9, 17, 0);

    [Theory]
    [InlineData(0.0, 1)]      // همیشه حداقل ۱ دقیقه
    [InlineData(2.5, 5)]      // ۲٫۵ کیلومتر با ۳۰ کیلومتر بر ساعت = ۵ دقیقه
    [InlineData(5.1, 11)]     // رو به بالا گرد می‌شود
    [InlineData(-3, 1)]
    [InlineData(double.NaN, 1)]
    public void Eta_is_estimated_from_distance(double km, int expected)
        => Assert.Equal(expected, TripOngoingRules.EtaMinutes(km));

    [Fact]
    public void Eta_text_shows_minutes_in_persian_digits()
    {
        Assert.Equal("۹ دقیقه", TripOngoingRules.EtaText(9, Now));
        Assert.Equal("۱ ساعت و ۵ دقیقه", TripOngoingRules.EtaText(65, Now));
        Assert.Equal("نامشخص", TripOngoingRules.EtaText(null, Now));
    }

    [Fact]
    public void First_update_is_always_sent()
        => Assert.True(TripOngoingRules.ShouldSendUpdate(null, null, "x", Now));

    [Fact]
    public void Updates_are_throttled_and_only_sent_when_the_text_changed()
    {
        // کمتر از ۲ دقیقه از قبلی: نه، حتی اگر متن فرق کند
        Assert.False(TripOngoingRules.ShouldSendUpdate("a", Now.AddMinutes(-1), "b", Now));
        // بعد از ۲ دقیقه: فقط اگر متن عوض شده
        Assert.True(TripOngoingRules.ShouldSendUpdate("a", Now.AddMinutes(-3), "b", Now));
        Assert.False(TripOngoingRules.ShouldSendUpdate("a", Now.AddMinutes(-3), "a", Now));
        // متن ثابت هم هر ۱۰ دقیقه تازه می‌شود (اگر کاربر اعلان را بسته باشد برگردد)
        Assert.True(TripOngoingRules.ShouldSendUpdate("a", Now.AddMinutes(-10), "a", Now));
    }

    [Fact]
    public void Trip_key_separates_the_return_leg_and_parses_back()
    {
        Assert.Equal("42", TripOngoingRules.TripKey(42));
        Assert.Equal("42r", TripOngoingRules.TripKey(42, true));
        Assert.True(TripOngoingRules.TryParseTripId("42r", out var id));
        Assert.Equal(42, id);
        Assert.False(TripOngoingRules.TryParseTripId("abc", out _));
        Assert.False(TripOngoingRules.TryParseTripId(null, out _));
    }
}
