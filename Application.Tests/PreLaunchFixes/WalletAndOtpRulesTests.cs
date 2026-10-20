using Application.Services.Accounting.OtpVerifySrv;
using Application.Services.ProductSrvs.WalletSrv;
using System;
using Xunit;

namespace Application.Tests.PreLaunchFixes;

public class WalletChargeRulesTests
{
    [Theory]
    [InlineData(10_000, true)]
    [InlineData(10_000_000, true)]
    [InlineData(250_000, true)]
    [InlineData(9_999, false)]
    [InlineData(10_000_001, false)]
    [InlineData(0, false)]
    [InlineData(-50_000, false)]
    [InlineData(100_000.5, false)]
    [InlineData(123_456_789_012_345, false)]
    public void ChargeAmount(double amount, bool valid)
    {
        Assert.Equal(valid, WalletChargeRules.IsValid(amount));
    }
}

public class OtpSendRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 12, 12, 0, 0);

    [Fact]
    public void FirstSend_IsAllowed()
    {
        Assert.True(OtpSendRules.Evaluate(Array.Empty<DateTime>(), Now).Allowed);
    }

    [Fact]
    public void SecondSendWithinTwoMinutes_IsTooSoon()
    {
        var decision = OtpSendRules.Evaluate(new[] { Now.AddSeconds(-30) }, Now);
        Assert.Equal(OtpSendRules.BlockReason.TooSoon, decision.Reason);
        Assert.Equal(TimeSpan.FromSeconds(90), decision.RetryAfter);
    }

    [Fact]
    public void SixthSendInOneHour_IsBlocked()
    {
        var sends = new[] { Now.AddMinutes(-50), Now.AddMinutes(-40), Now.AddMinutes(-30), Now.AddMinutes(-20), Now.AddMinutes(-10) };
        var decision = OtpSendRules.Evaluate(sends, Now);
        Assert.Equal(OtpSendRules.BlockReason.HourlyLimit, decision.Reason);
        Assert.Equal(TimeSpan.FromMinutes(10), decision.RetryAfter);
    }

    [Fact]
    public void SendsOlderThanAnHour_DoNotCount()
    {
        var sends = new[] { Now.AddMinutes(-70), Now.AddMinutes(-65), Now.AddMinutes(-40), Now.AddMinutes(-30), Now.AddMinutes(-20), Now.AddMinutes(-10) };
        Assert.True(OtpSendRules.Evaluate(sends, Now).Allowed);
    }
}
