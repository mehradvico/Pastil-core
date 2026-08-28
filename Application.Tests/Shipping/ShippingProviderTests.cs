using Application.Services.Order.ShippingSrv;
using Application.Services.Order.ShippingSrv.Provider;
using Entities.Entities.ShippingField;
using Microsoft.Extensions.Options;
using Xunit;

namespace Application.Tests.Shipping;

public class ShippingProviderTests
{
    [Fact]
    public async Task AloPeyk_TestMode_ReturnsDeterministicPositiveQuote()
    {
        var provider = new AloPeykShippingProvider(Options.Create(new ShippingOptions
        {
            TestMode = true,
            AloPeyk = new ShippingProviderOptions { Enabled = true }
        }));

        var result = await provider.GetQuoteAsync(CreateRequest(ShippingProviderEnum.AloPeyk));

        Assert.True(result.IsSuccess);
        Assert.True(result.Price > 0);
        Assert.StartsWith("TEST-AloPeyk-", result.ExternalQuoteId);
        Assert.Equal("IRR", result.Currency);
    }

    [Fact]
    public async Task Provider_ProductionWithoutCredentials_FailsClosed()
    {
        var provider = new SnappBoxShippingProvider(Options.Create(new ShippingOptions
        {
            TestMode = false,
            SnappBox = new ShippingProviderOptions { Enabled = true }
        }));

        var result = await provider.GetQuoteAsync(CreateRequest(ShippingProviderEnum.SnappBox));

        Assert.False(result.IsSuccess);
        Assert.Equal(0, result.Price);
        Assert.Contains("تکمیل نشده", result.ErrorMessage);
    }

    [Fact]
    public async Task Tipax_Quote_IncreasesForHeavierPackage()
    {
        var provider = new TipaxShippingProvider(Options.Create(new ShippingOptions
        {
            TestMode = true,
            Tipax = new ShippingProviderOptions { Enabled = true }
        }));
        var light = CreateRequest(ShippingProviderEnum.Tipax);
        var heavy = CreateRequest(ShippingProviderEnum.Tipax);
        heavy.WeightGrams = 5000;

        var lightResult = await provider.GetQuoteAsync(light);
        var heavyResult = await provider.GetQuoteAsync(heavy);

        Assert.True(lightResult.IsSuccess);
        Assert.True(heavyResult.Price > lightResult.Price);
    }

    [Fact]
    public async Task Miare_CreateShipment_WithoutCredentials_FailsClosed()
    {
        var provider = new MiareShippingProvider(Options.Create(new ShippingOptions
        {
            TestMode = false,
            Miare = new ShippingProviderOptions { Enabled = true }
        }));

        var result = await provider.CreateShipmentAsync(new ShippingProviderShipmentRequest
        {
            OrderId = "ORD-1",
            StoreId = 1,
            Provider = ShippingProviderEnum.Miare,
            RecipientName = "علی علوی",
            RecipientMobile = "09123456789",
            RecipientAddress = "تهران",
            PickupName = "فروشگاه تست",
            PickupPhone = "09120000000",
            OriginLatitude = 35.7219,
            OriginLongitude = 51.3347,
            DestinationLatitude = 35.6892,
            DestinationLongitude = 51.3890
        });

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task Miare_Disabled_FailsClosed()
    {
        var provider = new MiareShippingProvider(Options.Create(new ShippingOptions
        {
            TestMode = true,
            Miare = new ShippingProviderOptions { Enabled = false }
        }));

        var result = await provider.CreateShipmentAsync(new ShippingProviderShipmentRequest
        {
            OrderId = "ORD-1",
            StoreId = 1,
            Provider = ShippingProviderEnum.Miare
        });

        Assert.False(result.IsSuccess);
    }

    private static ShippingProviderQuoteRequest CreateRequest(ShippingProviderEnum provider) => new()
    {
        Provider = provider,
        StoreId = 1,
        OriginLatitude = 35.7219,
        OriginLongitude = 51.3347,
        DestinationLatitude = 35.6892,
        DestinationLongitude = 51.3890,
        WeightGrams = 1000,
        LengthCm = 20,
        WidthCm = 20,
        HeightCm = 20,
        DeclaredValue = 1000000
    };
}
