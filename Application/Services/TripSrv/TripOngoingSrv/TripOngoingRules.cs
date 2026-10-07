using System;
using System.Globalization;

namespace Application.Services.TripSrv.TripOngoingSrv
{
    // قواعد خالص اعلان ماندگار سفر پت‌رسان (بدون دیتابیس/شبکه) تا تست‌پذیر باشد.
    public static class TripOngoingRules
    {
        /// <summary>سرعت متوسط درون‌شهری برای تخمین زمان رسیدن از فاصله‌ی رانندگی (کیلومتر بر ساعت).</summary>
        public const double AverageSpeedKmh = 30;

        /// <summary>موقعیت راننده‌ای که قدیمی‌تر از این باشد برای تخمین زمان قابل اعتماد نیست.</summary>
        public static readonly TimeSpan LocationMaxAge = TimeSpan.FromMinutes(10);

        /// <summary>حداقل فاصله‌ی دو پوش به‌روزرسانی برای یک سفر.</summary>
        public static readonly TimeSpan MinInterval = TimeSpan.FromMinutes(2);

        /// <summary>اگر متن عوض نشده باشد هم حداکثر هر این مدت یک‌بار دوباره می‌فرستیم (اگر کاربر اعلان را بسته باشد برگردد).</summary>
        public static readonly TimeSpan MaxSilence = TimeSpan.FromMinutes(10);

        /// <summary>اعلانی که از شروعش این‌همه گذشته بسته می‌شود (سفر گیرکرده نباید اعلان را برای همیشه نگه دارد).</summary>
        public static readonly TimeSpan MaxLifetime = TimeSpan.FromHours(12);

        public static int EtaMinutes(double distanceKm)
        {
            if (double.IsNaN(distanceKm) || distanceKm < 0) return 1;
            return Math.Max(1, (int)Math.Ceiling(distanceKm / AverageSpeedKmh * 60));
        }

        // «۹ دقیقه»؛ بدون موقعیت قابل اعتماد: «نامشخص»
        public static string EtaText(int? minutes, DateTime now)
        {
            if (minutes == null) return "نامشخص";
            var m = Math.Max(1, minutes.Value);
            var text = m >= 60 ? $"{m / 60} ساعت و {m % 60} دقیقه" : $"{m} دقیقه";
            return ToPersianDigits(text);
        }

        public static bool ShouldSendUpdate(string lastText, DateTime? lastSentAt, string newText, DateTime now)
        {
            if (lastSentAt == null) return true;
            var age = now - lastSentAt.Value;
            if (age >= MaxSilence) return true;
            if (age < MinInterval) return false;
            return !string.Equals(lastText, newText, StringComparison.Ordinal);
        }

        public static string ToPersianDigits(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var chars = value.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
                if (chars[i] >= '0' && chars[i] <= '9')
                    chars[i] = (char)('۰' + (chars[i] - '0'));
            return new string(chars);
        }

        // کلید اعلان: شناسه‌ی سفر (مسیر رفت) یا شناسه + r (مسیر برگشتِ رفت‌وبرگشت) تا هر مسیر اعلان و پایان جدا داشته باشد
        public static string TripKey(long tripId, bool isReturnLeg = false)
            => tripId.ToString(CultureInfo.InvariantCulture) + (isReturnLeg ? "r" : string.Empty);

        public static bool TryParseTripId(string key, out long tripId)
        {
            tripId = 0;
            if (string.IsNullOrEmpty(key)) return false;
            return long.TryParse(key.TrimEnd('r'), NumberStyles.None, CultureInfo.InvariantCulture, out tripId) && tripId > 0;
        }
    }
}
