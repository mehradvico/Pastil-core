using System;
using System.Globalization;

namespace Application.Common.Helpers
{
    /// <summary>
    /// قاعده‌ی پیش‌پرداخت پکیج ارائه‌دهنده: حداقل = سهم پاستیل (درصد کمیسیون مؤثر × مبلغ کل)، حداکثر = کل مبلغ.
    /// درصد مؤثر = <see cref="VenueCommission.Resolve"/> (درصد خود خدمت اگر &gt; ۰ وگرنه کمیسیون اصلی مرکز).
    /// </summary>
    public static class PackagePrepayment
    {
        /// <summary>حداقل پیش‌پرداخت (تومان، رو به بالا). درصد ۰ ⇒ ۰.</summary>
        public static double Minimum(double total, decimal percent)
        {
            if (percent <= 0 || total <= 0 || double.IsNaN(total) || double.IsInfinity(total)) return 0;
            return Math.Ceiling(total * (double)percent / 100d - 1e-6);
        }

        /// <summary>null = معتبر؛ وگرنه پیام خطای فارسی (از Resource) همراه با مبلغ‌ها.</summary>
        public static string Check(double total, double prepayment, decimal minPercent)
        {
            if (prepayment > total + 1e-6)
                return string.Format(CultureInfo.InvariantCulture, Resource.Notification.PackagePrepaymentAboveTotal, Money(total));
            var min = Minimum(total, minPercent);
            if (prepayment + 1e-6 < min)
                return string.Format(CultureInfo.InvariantCulture, Resource.Notification.PackagePrepaymentBelowMinimum,
                    Money(min), minPercent.ToString("0.##", CultureInfo.InvariantCulture));
            return null;
        }

        private static string Money(double value) => ((long)Math.Ceiling(value)).ToString("N0", CultureInfo.InvariantCulture);
    }
}
