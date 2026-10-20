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
    [InlineData(901000, "Rial", 90100)]    // لاگ Production، ۲۵ مهر ۱۴۰۵: ≈۱۱ کیلومتر
    [InlineData(853000, "Rial", 85300)]
    [InlineData(32000, "Toman", 32000)]    // Staging: ≈۲٫۸ کیلومتر، بدون تبدیل
    [InlineData(32000, null, 32000)]       // واحد تنظیم‌نشده = تومان (پیش‌فرض امن برای Staging)
    [InlineData(1005, "Rial", 101)]        // رُند به تومان کامل (نیمه به بالا)
    public void Quote_is_converted_to_toman(double raw, string unit, double expected) =>
        Assert.Equal(expected, MiareAmountRules.ToToman(raw, unit));

    [Theory]
    [InlineData(90100, "Toman", 901000)]   // delivery_cost به تومان ← ریال ذخیره
    [InlineData(901000, "Rial", 901000)]   // delivery_cost به ریال ← همان
    [InlineData(5000, null, 50000)]
    public void Provider_cost_is_stored_in_rial(double raw, string unit, double expected) =>
        Assert.Equal(expected, MiareAmountRules.ToRial(raw, unit));

    [Fact]
    public void Express_total_adds_the_toman_surcharge_to_the_converted_estimate()
    {
        // ۹۰٬۱۰۰ تومان (برآورد ریالیِ تبدیل‌شده) + ۱۰۰٬۰۰۰ تومان اضافه‌ی ثابت = ۱۹۰٬۱۰۰ تومان، نه ۱٫۰۰۱٬۰۰۰
        var estimateToman = MiareAmountRules.ToToman(901000, "Rial");
        Assert.Equal(190100, ShippingExpressRules.ExpressPrice(estimateToman, 100000));
    }

    [Fact]
    public void Boost_amounts_are_converted_before_they_are_added_to_the_express_price()
    {
        // سطح ۳ در Staging = ۱۵٬۰۰۰ تومان؛ اگر Production ریال بدهد، ۱۵۰٬۰۰۰ خام باید ۱۵٬۰۰۰ تومان شود
        var amountToman = MiareAmountRules.ToToman(150000, "Rial");
        Assert.Equal(15000, amountToman);
        Assert.Equal(15000, ShippingExpressRules.Surcharge((3, amountToman), 100000));
    }
}
