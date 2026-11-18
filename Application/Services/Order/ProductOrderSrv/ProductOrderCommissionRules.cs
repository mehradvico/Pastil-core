using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Order.ProductOrderSrv
{
    /// <summary>
    /// کمیسیون فقط روی کالا، پس از بازگرداندن تخفیف تأمین‌شده توسط پاستیل محاسبه می‌شود.
    /// کل DeliveryPrice متعلق به پاستیل است؛ ارسال پس‌کرایه در DeliveryPrice صفر است.
    /// payment مبلغ ProductOrderStore پیش از کد تخفیف/مزیت ارسال باشگاه است.
    /// سهم فروشگاه = (payment − delivery + funded) × (۱ − کمیسیون٪).
    /// سهم پاستیل = payment − سهم فروشگاه = کمیسیون کالا + ارسال − funded.
    /// </summary>
    public static class ProductOrderCommissionRules
    {
        /// <summary>جمع تخفیفِ سهم پاستیل آیتم‌های باقی‌مانده‌ی یک فروشگاهِ سفارش (حذف‌شده‌ها حساب نمی‌شوند).</summary>
        public static decimal PastilFunded(IEnumerable<ProductOrderItem> items) =>
            (items ?? Enumerable.Empty<ProductOrderItem>())
                .Where(item => !item.Deleted && item.Count > 0 && item.PastilDiscountUnit > 0)
                .Sum(item => (decimal)item.PastilDiscountUnit * item.Count);

        public static (decimal Store, decimal Site) Split(decimal payment, decimal pastilFunded, decimal commissionPercent, decimal deliveryPrice = 0m)
        {
            var baseAmount = payment - deliveryPrice + pastilFunded;
            var store = baseAmount * (100m - commissionPercent) / 100m;
            return (store, payment - store);
        }

        /// <summary>
        /// سهم‌های کل سفارش. فروشگاهِ بدون Store (null)، پرداخت صفر و بدون تخفیف پاستیل، یا کمیسیونِ خارج از ۰..۱۰۰ نادیده گرفته می‌شود (مثل رفتار قبلی).
        /// </summary>
        public static (decimal Store, decimal Site, decimal PastilFunded) ForOrder(IEnumerable<ProductOrderStore> stores)
        {
            decimal totalStore = 0m, totalSite = 0m, totalFunded = 0m;
            foreach (var store in stores ?? Enumerable.Empty<ProductOrderStore>())
            {
                if (store.Deleted || store.Store == null)
                    continue;
                var percent = store.Store.CommissionPercent;
                if (percent < 0 || percent > 100)
                    continue;
                var funded = PastilFunded(store.ProductOrderItems);
                var payment = (decimal)store.PaymentPrice;
                if (payment <= 0 && funded <= 0)
                    continue;
                var (storeShare, siteShare) = Split(payment, funded, percent, (decimal)store.DeliveryPrice);
                totalStore += storeShare;
                totalSite += siteShare;
                totalFunded += funded;
            }
            return (totalStore, totalSite, totalFunded);
        }
    }
}
