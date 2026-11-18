using Application.Services.Order.ShippingSrv;
using System;
using Xunit;

namespace Application.Tests;

public class StoreClosedDateRulesTests
{
    [Fact]
    public void ParsesOnlyDateWithoutOffsetOrTime()
    {
        Assert.True(StoreClosedDateRules.TryParse("2026-11-23", out var date));
        Assert.Equal(new DateTime(2026, 11, 23), date);
        Assert.False(StoreClosedDateRules.TryParse("2026-11-23T00:00:00Z", out _));
    }

    [Fact]
    public void ChangeWindowUsesTehranCalendarDay()
    {
        var today = new DateTime(2026, 11, 18);
        Assert.False(StoreClosedDateRules.CanChange(today.AddDays(-1), today));
        Assert.True(StoreClosedDateRules.CanChange(today, today));
        Assert.True(StoreClosedDateRules.CanChange(today.AddDays(90), today));
        Assert.False(StoreClosedDateRules.CanChange(today.AddDays(91), today));
    }

    [Fact]
    public void ClosedDateMatchesExactCalendarDay()
    {
        Assert.True(StoreClosedDateRules.IsClosed(new DateTime(2026, 11, 23, 18, 0, 0),
            new[] { new DateTime(2026, 11, 23) }));
        Assert.False(StoreClosedDateRules.IsClosed(new DateTime(2026, 11, 24),
            new[] { new DateTime(2026, 11, 23) }));
    }
}
