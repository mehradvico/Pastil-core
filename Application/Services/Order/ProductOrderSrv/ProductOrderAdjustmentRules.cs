using Entities.Entities.ShippingField;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Order.ProductOrderSrv
{
    public enum OrderAdjustActor
    {
        Admin = 1,
        Store = 2
    }

    // قواعد خالص «لغو کامل سفارش» و «کم/حذف کردن آیتم» توسط فروشنده/ادمین. طراحی: backend/Docs/PRODUCT_ORDER_CANCEL_ADJUST_FA.md
    public static class ProductOrderAdjustmentRules
    {
        // فقط قبل از ارسال: پرداخت‌شده، حذف/لغو نشده، هنوز «ارسال/تحویل» نشده و مشتری هم پاسخ تحویل نداده.
        public static bool IsBeforeShipment(bool isPaid, bool deleted, string statusLabel, string stateLabel, bool? userReceived,
            string insertLabel, string processLabel, string canceledLabel)
        {
            if (!isPaid || deleted || userReceived != null || stateLabel == canceledLabel)
                return false;
            return statusLabel == insertLabel || statusLabel == processLabel;
        }

        public enum ShipmentGate
        {
            Allowed,
            /// <summary>پیک درخواست/پذیرفته شده؛ فروشنده نمی‌تواند، ادمین می‌تواند (با هشدار لغو سفر).</summary>
            AdminOnly,
            /// <summary>پیک کالا را گرفته یا تحویل داده؛ هیچ‌کس نمی‌تواند.</summary>
            Blocked
        }

        public static ShipmentGate GateForShipment(ShipmentStatusEnum status, bool cancelWholeOrder)
        {
            switch (status)
            {
                case ShipmentStatusEnum.PickedUp:
                case ShipmentStatusEnum.Delivered:
                    return ShipmentGate.Blocked;
                case ShipmentStatusEnum.Requested:
                case ShipmentStatusEnum.Accepted:
                    // کم/حذف کردن آیتم به سفر کاری ندارد؛ فقط لغو کل سفارش باید سفر میاره را هم لغو کند
                    return cancelWholeOrder ? ShipmentGate.AdminOnly : ShipmentGate.Allowed;
                default:
                    return ShipmentGate.Allowed;
            }
        }

        public sealed class ItemLine
        {
            public double UnitPrice { get; init; }
            public double UnitBasePrice { get; init; }
            public int Count { get; init; }
            public bool Deleted { get; init; }
        }

        public sealed class Totals
        {
            public double StorePrice { get; init; }
            public double StoreBasePrice { get; init; }
            public double StoreDiscountPrice => StoreBasePrice - StorePrice;
        }

        // جمع‌های یک فروشگاه از روی آیتم‌های باقی‌مانده
        public static Totals StoreTotals(IEnumerable<ItemLine> lines)
        {
            var live = lines.Where(line => !line.Deleted && line.Count > 0).ToList();
            return new Totals
            {
                StorePrice = live.Sum(line => line.UnitPrice * line.Count),
                StoreBasePrice = live.Sum(line => line.UnitBasePrice * line.Count)
            };
        }

        public sealed class OrderRecalc
        {
            public double Price { get; init; }
            public double BasePrice { get; init; }
            public double DiscountPrice { get; init; }
            public double RebatePrice { get; init; }
            public double PaymentPrice { get; init; }
            /// <summary>مبلغی که باید به کیف پول مشتری برگردد (تفاوت مبلغ پرداختی قبلی و جدید).</summary>
            public double Refund { get; init; }
        }

        // جمع‌های سفارش بعد از کم‌شدن آیتم: کد تخفیف به نسبت کاهش قیمت کاهش می‌یابد (تا برگشت پول بیشتر از واقعیت نشود)،
        // هزینه‌ی ارسال و تخفیف ارسال باشگاه ثابت می‌ماند. فرمول PaymentPrice مثل سبد: (Price - Rebate) + Delivery - ClubDeliveryDiscount.
        public static OrderRecalc RecalculateOrder(double oldPrice, double oldRebatePrice, double oldPaymentPrice, double deliveryPrice,
            double clubDeliveryDiscount, double newPrice, double newBasePrice)
        {
            newPrice = Math.Max(0, newPrice);
            var newRebate = oldPrice > 0 ? oldRebatePrice * (newPrice / oldPrice) : 0;
            newRebate = Math.Round(Math.Min(newRebate, newPrice), 0);
            var newPayment = Math.Max(0, (newPrice - newRebate) + deliveryPrice - clubDeliveryDiscount);
            newPayment = Math.Round(newPayment, 0);
            return new OrderRecalc
            {
                Price = newPrice,
                BasePrice = Math.Max(0, newBasePrice),
                DiscountPrice = Math.Max(0, newBasePrice - newPrice),
                RebatePrice = newRebate,
                PaymentPrice = newPayment,
                Refund = Math.Max(0, Math.Round(oldPaymentPrice - newPayment, 0))
            };
        }

        public static bool IsValidReducedCount(int currentCount, int newCount) => newCount >= 0 && newCount < currentCount;

        // نام دفتر کیف پول: یکتا برای هر لغو/کاهش؛ ثبت دوباره‌ی همان درخواست هرگز دوبار پول برنمی‌گرداند.
        public static string CancelRefundLedgerName(string orderId) => $"OrderCancelRefund:{orderId}";
        public static string ItemRefundLedgerName(long itemId, int newCount) => $"OrderItemRefund:{itemId}:{newCount}";
    }
}
