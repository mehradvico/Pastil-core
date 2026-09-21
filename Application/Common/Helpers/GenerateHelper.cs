using System;
using System.Security.Cryptography;
using System.Text;

namespace Application.Common.Helpers
{
    public static class GenerateHelper
    {
        /// <summary>
        /// کد عددی تصادفی با مولد امن رمزنگاری (برای OTP). رقم اول غیرصفر است تا طول ثابت بماند.
        /// </summary>
        /// <summary>
        /// طول کد OTP. پیش‌فرض ۴ (سازگار با اپ/وب‌اپ‌های قدیمی)؛ با Security:OtpLength (env PASTIL_OTP_LENGTH) تا ۸ قابل افزایش است.
        /// کلاینت‌ها طول را از پاسخ ارسال OTP (CodeLength) می‌خوانند، پس بعد از به‌روزرسانی همه‌ی کلاینت‌ها می‌شود روی ۶ گذاشت.
        /// </summary>
        public static int OtpLength { get; private set; } = 4;

        public static void ConfigureOtpLength(string value)
        {
            OtpLength = int.TryParse(value, out var length) && length >= 4 && length <= 8 ? length : 4;
        }

        public static string RandomDigit(int length = 4)
        {
            var random = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                random.Append(i == 0
                    ? RandomNumberGenerator.GetInt32(1, 10)
                    : RandomNumberGenerator.GetInt32(0, 10));
            }
            return random.ToString();
        }
    }
}
