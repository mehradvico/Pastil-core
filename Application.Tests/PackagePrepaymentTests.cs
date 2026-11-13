using Application.Common.Helpers;
using Xunit;

namespace Application.Tests;

// حداقل پیش‌پرداخت پکیج = سهم پاستیل (درصد مؤثر × مبلغ کل)، حداکثر = کل مبلغ.
public class PackagePrepaymentTests
{
    [Theory]
    [InlineData(1_000_000, 20, 200_000)]
    [InlineData(1_500_000, 12.5, 187_500)]
    [InlineData(333_333, 10, 33_334)]       // رو به بالا
    [InlineData(1_000_000, 0, 0)]           // کمیسیون صفر ⇒ حداقل صفر
    [InlineData(0, 20, 0)]
    public void Minimum_is_percent_of_total_rounded_up(double total, double percent, double expected) =>
        Assert.Equal(expected, PackagePrepayment.Minimum(total, (decimal)percent));

    [Fact]
    public void Check_accepts_between_minimum_and_total_inclusive()
    {
        Assert.Null(PackagePrepayment.Check(1_000_000, 200_000, 20));
        Assert.Null(PackagePrepayment.Check(1_000_000, 650_000, 20));
        Assert.Null(PackagePrepayment.Check(1_000_000, 1_000_000, 20));
        Assert.Null(PackagePrepayment.Check(1_000_000, 0, 0));
    }

    [Fact]
    public void Check_rejects_below_minimum_with_the_amount_in_the_message()
    {
        var error = PackagePrepayment.Check(1_000_000, 199_999, 20);
        Assert.NotNull(error);
        Assert.Contains("200,000", error);
        Assert.Contains("20", error);
    }

    [Fact]
    public void Check_rejects_above_total_with_the_total_in_the_message()
    {
        var error = PackagePrepayment.Check(1_000_000, 1_000_001, 20);
        Assert.NotNull(error);
        Assert.Contains("1,000,000", error);
        Assert.NotNull(PackagePrepayment.Check(1_000_000, 1_500_000, 0)); // حتی بدون کمیسیون
    }

    [Fact]
    public void Effective_percent_prefers_service_over_venue()
    {
        Assert.Equal(15m, VenueCommission.Resolve(15m, 20m));
        Assert.Equal(20m, VenueCommission.Resolve(0m, 20m));
    }
}
