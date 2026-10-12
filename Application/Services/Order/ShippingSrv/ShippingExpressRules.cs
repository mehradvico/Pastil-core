using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Order.ShippingSrv
{
    // قواعد خالص «ارسال اکسپرس» (میاره + Boost)؛ بدون شبکه/دیتابیس. طراحی: backend/Docs/MIARE_EXPRESS_SHIPPING_FA.md
    //
    // مستند میاره (Boost): سفر با boost.level ساخته می‌شود؛ مبلغ همان سطح (تومان) به هزینه‌ی سفر اضافه و سفر در صف تخصیص پیک اولویت می‌گیرد.
    // برآورد /estimate/price/ «Boost را شامل نمی‌شود»؛ پس قیمت اکسپرس = برآورد + مبلغ سطح boost. اگر Boost برای حساب فعال نباشد یا سطحی
    // برنگردد، یک مبلغ ثابت پاستیل (پیش‌فرض ۱۰۰٬۰۰۰ تومان) اضافه می‌شود.
    public static class ShippingExpressRules
    {
        private static readonly TimeZoneInfo Tehran = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

        // ساعت کاری اکسپرس (به وقت تهران): بازه‌ی [open, close). مقدار نامعتبر = بازه‌ی پیش‌فرض ۰۹:۰۰ تا ۲۰:۰۰
        public static bool IsOpen(DateTime nowUtc, string open, string close)
        {
            if (!ShippingSlotRules.TryParseTime(open, out var from)) from = TimeSpan.FromHours(9);
            if (!ShippingSlotRules.TryParseTime(close, out var to)) to = TimeSpan.FromHours(20);
            var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc), Tehran).TimeOfDay;
            return to > from ? local >= from && local < to : local >= from || local < to;
        }

        // سطح boost: همان سطح تنظیم‌شده؛ اگر نبود بالاترین سطح کمتر از آن؛ اگر آن هم نبود کمترین سطح موجود؛ بدون سطح = null
        public static (int Level, double Amount)? ChooseBoost(IReadOnlyList<(int Level, double Amount)> levels, int configuredLevel)
        {
            var usable = (levels ?? Array.Empty<(int Level, double Amount)>()).Where(item => item.Level > 0 && item.Amount > 0).OrderBy(item => item.Level).ToList();
            if (usable.Count == 0 || configuredLevel <= 0)
                return null;
            var exact = usable.FirstOrDefault(item => item.Level == configuredLevel);
            if (exact.Level > 0) return exact;
            var lower = usable.LastOrDefault(item => item.Level < configuredLevel);
            return lower.Level > 0 ? lower : usable[0];
        }

        // اضافه‌ی اکسپرس: مبلغ boost میاره اگر بود، وگرنه مبلغ ثابت پاستیل
        public static double Surcharge((int Level, double Amount)? boost, double fallbackToman) =>
            boost.HasValue && boost.Value.Amount > 0 ? boost.Value.Amount : Math.Max(0, fallbackToman);

        public static double ExpressPrice(double estimateToman, double surchargeToman) =>
            Math.Max(0, estimateToman) + Math.Max(0, surchargeToman);

        // قول تحویل به مشتری: از لحظه‌ی پرداخت تا N ساعت بعد (برای هشدار تأخیر و «آیا تحویل گرفتید؟»)
        public static DateTime PromiseEnd(DateTime paidAtUtc, int maxHours) => paidAtUtc.AddHours(Math.Max(1, maxHours));

        // مهلت فروشنده برای «درخواست پیک»
        public static DateTime SellerDeadline(DateTime paidAtUtc, int confirmMinutes) => paidAtUtc.AddMinutes(Math.Max(5, confirmMinutes));

        // فاصله‌ی مجاز اکسپرس؛ صفر/منفی = بدون محدودیت
        public static bool IsWithinDistance(double distanceKm, double maxKm) => ShippingGeoRules.IsWithinRange(distanceKm, maxKm);

        // «۲ تا ۳ ساعت کاری»
        public static string DeliveryText(int minHours, int maxHours)
        {
            var min = Math.Max(1, minHours);
            var max = Math.Max(min, maxHours);
            return RulesText.ToPersianDigits($"{min} تا {max} ساعت کاری");
        }

        private static class RulesText
        {
            public static string ToPersianDigits(string value)
            {
                var chars = value.ToCharArray();
                for (var i = 0; i < chars.Length; i++)
                    if (chars[i] >= '0' && chars[i] <= '9')
                        chars[i] = (char)('۰' + (chars[i] - '0'));
                return new string(chars);
            }
        }
    }
}
