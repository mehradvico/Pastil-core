using System;
using System.Globalization;

namespace Persistence.Context
{
    // قالب مشترک کدهای عمومی (ORD-14050726-1237-3320): پیشوند-تاریخ شمسی-ساعت-پسوند چهاررقمی.
    // پسوند از شماره‌ی BusinessCodeSequence با یک جایگشت در بازه‌ی ۱۰۰۰..۹۹۹۹ ساخته می‌شود تا در هر دقیقه
    // تا ۹۰۰۰ کد یکتا بدهد و ترتیبی به نظر نرسد.
    public static class BusinessCodeFormatter
    {
        public static string Format(string prefix, DateTime createdAt, long sequenceNumber)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Prefix is required.", nameof(prefix));
            if (sequenceNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(sequenceNumber));

            var calendar = new PersianCalendar();
            var year = calendar.GetYear(createdAt);
            var month = calendar.GetMonth(createdAt);
            var day = calendar.GetDayOfMonth(createdAt);
            var jalaliDate = string.Concat(
                year.ToString("D4", CultureInfo.InvariantCulture),
                month.ToString("D2", CultureInfo.InvariantCulture),
                day.ToString("D2", CultureInfo.InvariantCulture));
            var time = createdAt.ToString("HHmm", CultureInfo.InvariantCulture);
            var minuteKey = year * 100000000L + month * 1000000L + day * 10000L +
                            createdAt.Hour * 100L + createdAt.Minute;
            var sequencePosition = (sequenceNumber - 1) % 9000;
            var minuteOffset = (minuteKey * 3571 + 1877) % 9000;
            var permutationExponent = (sequencePosition + minuteOffset) % 9000;
            var randomLookingSuffix = ModPow(7, permutationExponent, 9001) + 999;

            return $"{prefix}-{jalaliDate}-{time}-{randomLookingSuffix.ToString(CultureInfo.InvariantCulture)}";
        }

        private static int ModPow(int value, long exponent, int modulus)
        {
            long result = 1;
            long factor = value % modulus;
            while (exponent > 0)
            {
                if ((exponent & 1) == 1)
                    result = result * factor % modulus;

                factor = factor * factor % modulus;
                exponent >>= 1;
            }

            return (int)result;
        }
    }
}
