using Application.Services.Order.ProductOrderSrv;
using Entities.Entities.ShippingField;
using Xunit;
using static Application.Services.Order.ProductOrderSrv.ProductOrderAdjustmentRules;

namespace Application.Tests;

public class ProductOrderAdjustmentRulesTests
{
    private const string Insert = "ProductOrderStatus_Insert";
    private const string Process = "ProductOrderStatus_Proccess";
    private const string Send = "ProductOrderStatus_Send";
    private const string Delivered = "ProductOrderStatus_Delivered";
    private const string Normal = "ProductOrderState_Normal";
    private const string Canceled = "ProductOrderState_Canceled";

    private static bool Editable(bool paid = true, bool deleted = false, string status = Insert, string state = Normal, bool? received = null)
        => IsBeforeShipment(paid, deleted, status, state, received, Insert, Process, Canceled);

    [Theory]
    [InlineData(Insert, true)]
    [InlineData(Process, true)]
    [InlineData(Send, false)]        // ارسال‌شده: دیگر نه
    [InlineData(Delivered, false)]
    public void Only_orders_before_shipment_can_be_cancelled_or_edited(string status, bool expected)
        => Assert.Equal(expected, Editable(status: status));

    [Fact]
    public void Unpaid_deleted_cancelled_or_answered_orders_are_not_editable()
    {
        Assert.False(Editable(paid: false));
        Assert.False(Editable(deleted: true));
        Assert.False(Editable(state: Canceled));
        Assert.False(Editable(received: true));
        Assert.False(Editable(received: false));
    }

    [Theory]
    [InlineData(ShipmentStatusEnum.AwaitingSellerConfirm, true, ShipmentGate.Allowed)]
    [InlineData(ShipmentStatusEnum.Preparing, true, ShipmentGate.Allowed)]
    [InlineData(ShipmentStatusEnum.Requested, true, ShipmentGate.AdminOnly)]   // لغو کل سفارش باید سفر را هم لغو کند
    [InlineData(ShipmentStatusEnum.Accepted, true, ShipmentGate.AdminOnly)]
    [InlineData(ShipmentStatusEnum.Requested, false, ShipmentGate.Allowed)]    // کم‌کردن آیتم به سفر کاری ندارد
    [InlineData(ShipmentStatusEnum.PickedUp, true, ShipmentGate.Blocked)]      // پیک کالا را گرفته: هیچ‌کس
    [InlineData(ShipmentStatusEnum.PickedUp, false, ShipmentGate.Blocked)]
    [InlineData(ShipmentStatusEnum.Delivered, false, ShipmentGate.Blocked)]
    public void Courier_state_decides_who_may_change_the_order(ShipmentStatusEnum status, bool cancelWhole, ShipmentGate expected)
        => Assert.Equal(expected, GateForShipment(status, cancelWhole));

    [Fact]
    public void Removing_an_item_refunds_its_price_when_there_is_no_coupon()
    {
        // سفارش ۱٬۰۰۰٬۰۰۰ + ارسال ۵۰٬۰۰۰؛ یک کالای ۳۰۰٬۰۰۰ حذف می‌شود
        var result = RecalculateOrder(oldPrice: 1_000_000, oldRebatePrice: 0, oldPaymentPrice: 1_050_000, deliveryPrice: 50_000,
            clubDeliveryDiscount: 0, newPrice: 700_000, newBasePrice: 900_000);
        Assert.Equal(700_000, result.Price);
        Assert.Equal(750_000, result.PaymentPrice);
        Assert.Equal(300_000, result.Refund);
        Assert.Equal(200_000, result.DiscountPrice);
    }

    [Fact]
    public void A_coupon_shrinks_with_the_order_so_the_refund_is_never_more_than_was_paid()
    {
        // ۱٬۰۰۰ با کد تخفیف ۱۰۰ (۱۰٪) و ارسال ۵۰ ⇒ پرداختی ۹۵۰؛ نصف سفارش حذف می‌شود
        var result = RecalculateOrder(1_000, 100, 950, 50, 0, 500, 500);
        Assert.Equal(50, result.RebatePrice);
        Assert.Equal(500, result.PaymentPrice); // ۵۰۰ − ۵۰ + ۵۰
        Assert.Equal(450, result.Refund);        // نه ۵۰۰
    }

    [Fact]
    public void The_club_delivery_discount_stays_and_the_refund_never_goes_negative()
    {
        var result = RecalculateOrder(1_000, 0, 960, 100, 140, 1_000, 1_000); // چیزی تغییر نکرده
        Assert.Equal(960, result.PaymentPrice);
        Assert.Equal(0, result.Refund);
    }

    [Fact]
    public void Store_totals_ignore_deleted_and_zero_lines()
    {
        var totals = StoreTotals(new[]
        {
            new ItemLine { UnitPrice = 100, UnitBasePrice = 150, Count = 2 },
            new ItemLine { UnitPrice = 40, UnitBasePrice = 40, Count = 1, Deleted = true },
            new ItemLine { UnitPrice = 10, UnitBasePrice = 10, Count = 0 }
        });
        Assert.Equal(200, totals.StorePrice);
        Assert.Equal(300, totals.StoreBasePrice);
        Assert.Equal(100, totals.StoreDiscountPrice);
    }

    [Theory]
    [InlineData(3, 2, true)]
    [InlineData(3, 0, true)]
    [InlineData(3, 3, false)]   // تغییری نیست
    [InlineData(3, 4, false)]   // افزایش مجاز نیست
    [InlineData(3, -1, false)]
    public void An_item_can_only_be_reduced(int current, int next, bool expected)
        => Assert.Equal(expected, IsValidReducedCount(current, next));

    [Fact]
    public void Refund_ledger_names_are_unique_per_cancel_or_reduction()
    {
        Assert.NotEqual(CancelRefundLedgerName("a"), CancelRefundLedgerName("b"));
        Assert.NotEqual(ItemRefundLedgerName(5, 1), ItemRefundLedgerName(5, 0));
        Assert.Equal(ItemRefundLedgerName(5, 1), ItemRefundLedgerName(5, 1)); // تکرار همان درخواست = همان دفتر = دوبار پول برنمی‌گردد
    }
}
