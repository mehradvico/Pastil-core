using Application.Services.ProductSrvs.WalletSrv;
using Xunit;

namespace Application.Tests.PreLaunchFixes;

public class WalletDescriptionRulesTests
{
    private static WalletCodeLookup Codes()
    {
        var codes = new WalletCodeLookup();
        codes.Payments[12] = "PAY-14050720-1015-4821";
        codes.PansionReserves[7] = "PNS-14050720-1010-3001";
        codes.CompanionReserves[9] = "RSV-14050720-0900-5555";
        codes.Orders["14050720123"] = "ORD-14050720-1200-7777";
        codes.CompanionPackageItems[3] = ("RSV-14050720-0900-5555", "حمام و اصلاح");
        return codes;
    }

    [Fact]
    public void RefundLedgerKey_ShowsCancelledReserveCode()
    {
        var text = WalletDescriptionRules.Describe(new WalletDescribeInput { Name = "PansionReserveRefund:7", IsIncrease = true }, Codes());
        Assert.Equal(string.Format(Resource.Pattern.WalletReasonCancelRefundFor,
            string.Format(Resource.Pattern.WalletTargetPansionReserve, "PNS-14050720-1010-3001")), text);
    }

    [Fact]
    public void PaymentRefundLedgerKey_ShowsPaymentCode()
    {
        var text = WalletDescriptionRules.Describe(new WalletDescribeInput { Name = "PaymentRefund:12", IsIncrease = true, PaymentId = 12 }, Codes());
        Assert.Contains("PAY-14050720-1015-4821", text);
        Assert.DoesNotContain("PaymentRefund", text);
    }

    [Fact]
    public void UserTopUp_FromOldOnlinePaymentName()
    {
        var text = WalletDescriptionRules.Describe(new WalletDescribeInput { Name = Resource.Lang.OnlinePayment, IsIncrease = true, PaymentId = 12 }, Codes());
        Assert.Equal(string.Format(Resource.Pattern.WalletReasonUserCharge, "PAY-14050720-1015-4821"), text);
    }

    [Fact]
    public void ReserveDebit_WithoutName_ShowsReserveCode()
    {
        var text = WalletDescriptionRules.Describe(new WalletDescribeInput { CompanionReserveId = 9 }, Codes());
        Assert.Equal(string.Format(Resource.Pattern.WalletReasonPayFor,
            string.Format(Resource.Pattern.WalletTargetCompanionReserve, "RSV-14050720-0900-5555")), text);
    }

    [Fact]
    public void AdminManual_WithAndWithoutNote()
    {
        Assert.Equal(Resource.Notification.WalletReasonAdminCharge,
            WalletDescriptionRules.Describe(new WalletDescribeInput { Name = WalletDescriptionRules.AdminManualName(""), IsIncrease = true }, Codes()));
        Assert.Equal(string.Format(Resource.Pattern.WalletReasonAdminDebitNote, "جبران"),
            WalletDescriptionRules.Describe(new WalletDescribeInput { Name = WalletDescriptionRules.AdminManualName(" جبران "), IsIncrease = false }, Codes()));
    }

    [Fact]
    public void LegacyRefLessCustomName_IsTreatedAsAdminManual()
    {
        Assert.Equal(string.Format(Resource.Pattern.WalletReasonAdminChargeNote, "تراکنش دستی توسط ادمین"),
            WalletDescriptionRules.Describe(new WalletDescribeInput { Name = "تراکنش دستی توسط ادمین", IsIncrease = true }, Codes()));
    }

    [Fact]
    public void PackageAndOrderLedgers()
    {
        var package = WalletDescriptionRules.Describe(new WalletDescribeInput { Name = "CompanionPackageRefund:3", IsIncrease = true }, Codes());
        Assert.Contains("حمام و اصلاح", package);
        Assert.Contains("RSV-14050720-0900-5555", package);

        var order = WalletDescriptionRules.Describe(new WalletDescribeInput { Name = "OrderCancelRefund:14050720123", IsIncrease = true }, Codes());
        Assert.Contains("ORD-14050720-1200-7777", order);

        var debt = WalletDescriptionRules.Describe(new WalletDescribeInput { Name = WalletDescriptionRules.CompanionDebtName(9), CompanionReserveId = 9 }, Codes());
        Assert.Contains("RSV-14050720-0900-5555", debt);
    }

    [Fact]
    public void NothingKnown_StillNeverEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(WalletDescriptionRules.Describe(new WalletDescribeInput { IsIncrease = true }, null)));
        Assert.False(string.IsNullOrWhiteSpace(WalletDescriptionRules.Describe(new WalletDescribeInput { IsIncrease = false, TripId = 5 }, null)));
    }
}
