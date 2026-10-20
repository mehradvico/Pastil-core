using Application.Services.ProviderSrvs.ProviderSubscriptionSrv;
using Xunit;

namespace Application.Tests;

public class ProviderSubscriptionRulesTests
{
    private static readonly DateTime Start = new(2026, 10, 1);
    private static readonly DateTime End = new(2027, 1, 1);

    [Fact]
    public void NoSubscription_ClearsBothLegacyFields()
    {
        var (gold, silver, create) = ProviderSubscriptionRules.ToLegacyAccountDates(null, Start, End);
        Assert.Null(gold);
        Assert.Null(silver);
        Assert.Null(create);
    }

    [Fact]
    public void SilverLevel_SetsSilverOnly()
    {
        var (gold, silver, create) = ProviderSubscriptionRules.ToLegacyAccountDates(ProviderSubscriptionRules.SilverLevel, Start, End);
        Assert.Null(gold);
        Assert.Equal(End, silver);
        Assert.Equal(Start, create);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void GoldAndAbove_SetGoldOnly(int level)
    {
        var (gold, silver, create) = ProviderSubscriptionRules.ToLegacyAccountDates(level, Start, End);
        Assert.Equal(End, gold);
        Assert.Null(silver);
        Assert.Null(create);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BelowSilver_IsTreatedAsNone(int level)
    {
        var (gold, silver, _) = ProviderSubscriptionRules.ToLegacyAccountDates(level, Start, End);
        Assert.Null(gold);
        Assert.Null(silver);
    }

    [Theory]
    [InlineData(-1, 1, true)]
    [InlineData(0, 1, true)]
    [InlineData(1, 1, false)]
    [InlineData(2, 1, false)]
    public void IsActiveAt_UsesInclusiveStartExclusiveEnd(int startOffsetDays, int endOffsetDays, bool expected)
    {
        var now = new DateTime(2026, 10, 18);
        Assert.Equal(expected, ProviderSubscriptionRules.IsActiveAt(now.AddDays(startOffsetDays), now.AddDays(endOffsetDays), now));
    }

    [Fact]
    public void Period_MustEndAfterStart()
    {
        Assert.True(ProviderSubscriptionRules.IsValidPeriod(Start, End));
        Assert.False(ProviderSubscriptionRules.IsValidPeriod(End, Start));
        Assert.False(ProviderSubscriptionRules.IsValidPeriod(Start, Start));
    }
}
