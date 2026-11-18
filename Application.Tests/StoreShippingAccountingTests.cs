using Application.Services.Order.ProductOrderSrv;
using Entities.Entities;
using Xunit;

namespace Application.Tests;

public class StoreShippingAccountingTests
{
    [Theory]
    [InlineData(10, 0, 900_000, 150_000)]
    [InlineData(0, 0, 1_000_000, 50_000)]
    [InlineData(100, 0, 0, 1_050_000)]
    [InlineData(10, 50_000, 945_000, 105_000)]
    public void All_prepaid_delivery_belongs_to_pastil(decimal percent, decimal funded, decimal expectedStore, decimal expectedSite)
    {
        var (store, site) = ProductOrderCommissionRules.Split(1_050_000, funded, percent, 50_000);
        Assert.Equal(expectedStore, store);
        Assert.Equal(expectedSite, site);
        Assert.Equal(1_050_000, store + site);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(100)]
    public void Adding_delivery_never_changes_store_receivable(decimal percent)
    {
        var without = ProductOrderCommissionRules.Split(850_000, 40_000, percent);
        var with = ProductOrderCommissionRules.Split(937_123.45m, 40_000, percent, 87_123.45m);
        Assert.Equal(without.Store, with.Store);
        Assert.Equal(without.Site + 87_123.45m, with.Site);
    }

    [Fact]
    public void Callback_and_item_adjustment_share_the_same_shipping_rule()
    {
        var active = new ProductOrderStore
        {
            PaymentPrice = 970_000, DeliveryPrice = 50_000,
            Store = new Store { CommissionPercent = 8 },
            ProductOrderItems = new List<ProductOrderItem> { new() { Count = 1, PastilDiscountUnit = 80_000 } }
        };
        var deleted = new ProductOrderStore
        {
            Deleted = true, PaymentPrice = 999_999, DeliveryPrice = 50_000,
            Store = new Store { CommissionPercent = 10 }
        };
        var paid = ProductOrderCommissionRules.ForOrder(new[] { active, deleted });
        Assert.Equal(920_000, paid.Store);
        Assert.Equal(50_000, paid.Site);
        Assert.Equal(80_000, paid.PastilFunded);

        // بعد از کاهش تعداد، ارسال ثابت است و فقط کمیسیون کالا دوباره محاسبه می‌شود.
        active.PaymentPrice = 510_000;
        active.ProductOrderItems.Single().PastilDiscountUnit = 40_000;
        var adjusted = ProductOrderCommissionRules.ForOrder(new[] { active });
        Assert.Equal(460_000, adjusted.Store);
        Assert.Equal(50_000, adjusted.Site);
    }

    [Fact]
    public void Postpaid_quote_is_not_collected_shipping_revenue()
    {
        var row = new ProductOrderStore
        {
            PaymentPrice = 1_000_000, DeliveryPrice = 0, ShippingQuotedPrice = 120_000,
            ShippingPaymentMode = Entities.Entities.ShippingField.ShippingPaymentModeEnum.ReceiverPays,
            Store = new Store { CommissionPercent = 10 }
        };
        var result = ProductOrderCommissionRules.ForOrder(new[] { row });
        Assert.Equal(900_000, result.Store);
        Assert.Equal(100_000, result.Site);
    }
}
