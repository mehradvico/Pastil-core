using Application.Common.Helpers;
using System;
using Xunit;

namespace Application.Tests;

public class VenueRebateFundingRulesTests
{
    [Theory]
    [InlineData(0, 700000, 200000)]
    [InlineData(30, 730000, 170000)]
    [InlineData(100, 800000, 100000)]
    public void Split_ChargesDiscountToSelectedParties(int sitePercent, double provider, double site)
    {
        var result = VenueFundedRebateRules.Split(1_000_000, 100_000, 200_000, sitePercent);
        Assert.Equal(provider, result.Provider);
        Assert.Equal(site, result.Site);
        Assert.Equal(900_000d, result.Provider + result.Site);
    }

    [Fact]
    public void CanFund_RejectsDiscountExceedingEitherPartyShare()
    {
        Assert.False(VenueFundedRebateRules.CanFund(1_000, 250, 200, 100));
        Assert.False(VenueFundedRebateRules.CanFund(1_000, 850, 200, 0));
        Assert.True(VenueFundedRebateRules.CanFund(1_000, 200, 200, 50));
    }

    [Fact]
    public void Allocate_ManySmallPartsPreservesTotalAndNonnegativeShares()
    {
        var gross = new (double Provider, double Site)[101];
        for (var i = 0; i < 100; i++) gross[i] = (1, 1);
        gross[100] = (10, 10);
        var result = VenueFundedRebateRules.Allocate(gross, 100, 50);
        Assert.Equal(120d, Sum(result));
        foreach (var item in result)
        {
            Assert.True(item.Provider >= 0);
            Assert.True(item.Site >= 0);
        }
    }

    [Fact]
    public void ValidPercents_RequiresCompleteHundredPercent()
    {
        Assert.True(VenueFundedRebateRules.ValidPercents(null, null));
        Assert.True(VenueFundedRebateRules.ValidPercents(30, 70));
        Assert.False(VenueFundedRebateRules.ValidPercents(30, null));
        Assert.False(VenueFundedRebateRules.ValidPercents(60, 50));
        Assert.False(VenueFundedRebateRules.ValidPercents(0.005m, 99.995m));
    }

    private static double Sum(System.Collections.Generic.IEnumerable<(double Provider, double Site)> shares)
    {
        double total = 0;
        foreach (var item in shares) total += item.Provider + item.Site;
        return Math.Round(total, 6);
    }
}
