using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Order.ProductOrderSrv
{
    /// <summary>
    /// حسابداری سهم فروشگاه/پاستیل در سفارش کالا با تخفیفِ دو‌لایه.
    /// <para>
    /// تخفیف محصول دو لایه دارد و کاملاً مستقل‌اند: اول تخفیف فروشگاه (هر درصدی؛ هزینه‌اش با فروشگاه) روی قیمت پایه،
    /// بعد تخفیف سهم پاستیل (≤ درصد کمیسیون؛ هزینه‌اش با پاستیل) روی «قیمتِ بعد از تخفیف فروشگاه».
    /// </para>
    /// <para>
    /// payment = آنچه مشتری برای این فروشگاه پرداخته (بعد از هر دو تخفیف، با هزینه‌ی ارسال)؛ F = جمع تخفیفِ سهم پاستیل.
    /// سهم فروشگاه = (payment + F) × (۱ − کمیسیون٪) — یعنی تخفیفِ پاستیل روی مبنای کمیسیون فروشگاه اثر نمی‌گذارد.
    /// سهم پاستیل = payment − سهم فروشگاه = کمیسیون٪ × (payment + F) − F؛ چون تخفیفِ پاستیل ≤ کمیسیون و روی همان مبنا اعمال می‌شود هیچ‌وقت منفی نمی‌شود.
    /// بدون تخفیفِ پاستیل (F = ۰) دقیقاً همان رفتار قبلی است. اگر F برابر کمیسیون باشد سهم خالص پاستیل صفر می‌شود («تخفیف به‌جای کمیسیون»).
    /// کد تخفیف (Rebate) و تخفیف ارسال باشگاه کاملاً هزینه‌ی پاستیل‌اند و از قبل روی مبنای فروشگاه اثر نمی‌گذارند.
    /// </para>
    /// </summary>
    public static class ProductOrderCommissionRules
    {
        /// <summary>جمع تخفیفِ سهم پاستیل آیتم‌های باقی‌مانده‌ی یک فروشگاهِ سفارش (حذف‌شده‌ها حساب نمی‌شوند).</summary>
        public static decimal PastilFunded(IEnumerable<ProductOrderItem> items) =>
            (items ?? Enumerable.Empty<ProductOrderItem>())
                .Where(item => !item.Deleted && item.Count > 0 && item.PastilDiscountUnit > 0)
                .Sum(item => (decimal)item.PastilDiscountUnit * item.Count);

        public static (decimal Store, decimal Site) Split(decimal payment, decimal pastilFunded, decimal commissionPercent)
        {
            var baseAmount = payment + pastilFunded;
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
                if (store.Store == null)
                    continue;
                var percent = store.Store.CommissionPercent;
                if (percent < 0 || percent > 100)
                    continue;
                var funded = PastilFunded(store.ProductOrderItems);
                var payment = (decimal)store.PaymentPrice;
                if (payment <= 0 && funded <= 0)
                    continue;
                var (storeShare, siteShare) = Split(payment, funded, percent);
                totalStore += storeShare;
                totalSite += siteShare;
                totalFunded += funded;
            }
            return (totalStore, totalSite, totalFunded);
        }
    }
}
