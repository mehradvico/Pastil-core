using Application.Services.CompanionSrv.CompanionAssistancePackageSrv;
using Entities.Entities;
using Xunit;

namespace Application.Tests;

public class CompanionPackagePricingTests
{
    private const long Online = 37, InPerson = 38, InPlace = 39;

    private static CompanionAssistancePackageType Row(long type, double price, double prepay, bool deleted = false) =>
        new() { CompanionAssistanceTypeId = type, Price = price, PrePaymentPrice = prepay, Deleted = deleted };

    [Fact]
    public void Package_without_mode_rows_is_a_legacy_package_offered_in_every_mode_at_its_own_price()
    {
        foreach (var mode in new[] { Online, InPerson, InPlace })
        {
            var quote = CompanionPackagePricing.Resolve(null!, 200_000, 50_000, mode);

            Assert.True(quote.Offered);
            Assert.False(quote.FromModeRow);
            Assert.Equal(200_000, quote.Price);
            Assert.Equal(50_000, quote.PrePaymentPrice);
        }
    }

    [Fact]
    public void Each_mode_uses_its_own_price_and_prepayment()
    {
        var rows = new[] { Row(InPerson, 200_000, 40_000), Row(InPlace, 350_000, 70_000), Row(Online, 120_000, 120_000) };

        var inPlace = CompanionPackagePricing.Resolve(rows, 120_000, 120_000, InPlace);
        var inPerson = CompanionPackagePricing.Resolve(rows, 120_000, 120_000, InPerson);
        var online = CompanionPackagePricing.Resolve(rows, 120_000, 120_000, Online);

        Assert.Equal((350_000d, 70_000d), (inPlace.Price, inPlace.PrePaymentPrice));
        Assert.Equal((200_000d, 40_000d), (inPerson.Price, inPerson.PrePaymentPrice));
        Assert.Equal((120_000d, 120_000d), (online.Price, online.PrePaymentPrice));
        Assert.All(new[] { inPlace, inPerson, online }, q => Assert.True(q.Offered && q.FromModeRow));
    }

    [Fact]
    public void A_mode_the_package_does_not_offer_is_not_bookable()
    {
        var rows = new[] { Row(InPerson, 200_000, 40_000) };

        var quote = CompanionPackagePricing.Resolve(rows, 200_000, 40_000, InPlace);

        Assert.False(quote.Offered);
        Assert.Equal(0, quote.Price);
    }

    [Fact]
    public void Deleted_rows_are_ignored_and_a_package_with_only_deleted_rows_is_legacy_again()
    {
        var rows = new[] { Row(InPlace, 350_000, 70_000, deleted: true) };

        var quote = CompanionPackagePricing.Resolve(rows, 200_000, 50_000, InPerson);

        Assert.True(quote.Offered);
        Assert.False(quote.FromModeRow);
        Assert.Equal(200_000, quote.Price);
    }
}
