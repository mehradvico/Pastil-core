using Application.Services.FinanceSrvs.FinanceReportSrv;
using System.Linq;
using Xunit;

namespace Application.Tests;

// گزارش مالی کل: هر پولی که کاربر داده باید یک‌بار و فقط یک‌بار در دریافتی بیاید.
public class FinanceReportRulesTests
{
    [Fact]
    public void Every_payment_type_has_a_source_row_key()
    {
        foreach (var name in System.Enum.GetNames(typeof(Application.Common.Enumerable.PaymentCallbackTypeEnum)))
        {
            var key = FinanceReportRules.NormalizeSource(name);
            Assert.True(name == "Wallet" || FinanceReportRules.SourceKeys.Contains(key), $"{name} در گزارش مالی نیست");
        }
    }

    [Fact]
    public void Batch_reserve_payments_count_as_companion_reserve()
        => Assert.Equal("CompanionReserve", FinanceReportRules.NormalizeSource("CompanionReserveBatch"));

    [Fact]
    public void Received_is_gateway_plus_wallet()
        => Assert.Equal(150, FinanceReportRules.Received(100, 50));

    [Fact]
    public void Partner_share_is_never_negative()
    {
        Assert.Equal(80, FinanceReportRules.PartnerShare(100, 20));
        Assert.Equal(0, FinanceReportRules.PartnerShare(10, 20));
    }

    [Fact]
    public void Summary_nets_refunds_and_keeps_top_ups_out_of_revenue()
    {
        var rows = new[]
        {
            new FinanceReportRules.SourceRow { Source = "Trip", GatewayPaid = 100, WalletSpent = 50, Received = 150, SiteShare = 30, PartnerShare = 120 },
            new FinanceReportRules.SourceRow { Source = "Cargo", GatewayPaid = 200, WalletSpent = 0, Received = 200, SiteShare = 200, PartnerShare = 0 },
        };
        var s = FinanceReportRules.Build(rows, refundedToWallet: 40, walletTopUps: 999);
        Assert.Equal(350, s.TotalReceived);
        Assert.Equal(310, s.NetRevenue);
        Assert.Equal(230, s.TotalSiteShare);
        Assert.Equal(999, s.WalletTopUps);
    }

    [Fact]
    public void Empty_summary_is_zero()
        => Assert.Equal(0, FinanceReportRules.Build(null, 0, 0).NetRevenue);
}
