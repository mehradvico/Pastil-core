using Application.Services.Order.ShippingSrv;
using Xunit;

namespace Application.Tests;

public class MiareAmountRulesTests
{
    [Theory]
    [InlineData("Rial", true)]
    [InlineData("rial", true)]
    [InlineData(" IRR ", true)]
    [InlineData("ریال", true)]
    [InlineData("Toman", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("anything else", false)]   // ناشناخته = تومان (رفتار قبلی؛ تبدیلی نمی‌شود)
    public void Unit_is_rial_only_for_explicit_rial_spellings(string unit, bool expected) =>
        Assert.Equal(expected, MiareAmountRules.IsRial(unit));

    [Theory]
    [InlineData(320000, "Rial", 320000)] // تنظیم قدیمی نباید مبلغ مشتری را تقسیم کند
    [InlineData(320000, "Toman", 320000)]
    [InlineData(32000, null, 32000)]
    [InlineData(1005, "Rial", 1005)]
    public void Quote_is_converted_to_toman(double raw, string unit, double expected) =>
        Assert.Equal(expected, MiareAmountRules.ToToman(raw, unit));

    [Theory]
    [InlineData(90100, "Toman", 901000)]
    [InlineData(320000, "Rial", 3200000)] // حتی با تنظیم قدیمی، عدد خام تومان است
    [InlineData(5000, null, 50000)]
    public void Provider_cost_is_stored_in_rial(double raw, string unit, double expected) =>
        Assert.Equal(expected, MiareAmountRules.ToRial(raw, unit));

    [Fact]
    public void Express_total_adds_the_toman_surcharge_to_the_raw_estimate()
    {
        var estimateToman = MiareAmountRules.ToToman(320000, "Rial");
        Assert.Equal(420000, ShippingExpressRules.ExpressPrice(estimateToman, 100000));
    }

    [Fact]
    public void Boost_amounts_keep_their_toman_value()
    {
        var amountToman = MiareAmountRules.ToToman(150000, "Rial");
        Assert.Equal(150000, amountToman);
        Assert.Equal(150000, ShippingExpressRules.Surcharge((3, amountToman), 100000));
    }
}
