using System;
using System.Linq;

namespace Application.Common.Security
{
    /// <summary>
    /// آدرس فایل ضمیمه‌ی چت را کلاینت می‌فرستد و بعداً روی مرورگر/اپ طرف مقابل و پنل ادمین رندر می‌شود.
    /// فقط مسیر نسبیِ سرور فایل یا آدرس https روی دامنه‌ی خودمان مجاز است؛ در غیر این صورت هر کاربر می‌توانست
    /// آدرس بیرونی (ردیابی IP بیننده/فیشینگ)، `javascript:` یا مسیر پیمایشی (`..`) ذخیره کند.
    /// </summary>
    public static class AttachmentUrlPolicy
    {
        private const string TrustedRootDomain = "pastil.pet";

        public static bool IsAllowed(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            var value = url.Trim();
            if (value.Length > 2048 || value.Any(char.IsControl) || value.Contains('\\') || value.Contains(".."))
                return false;

            if (value.StartsWith('/'))
                return !value.StartsWith("//");

            if (value.Contains(':'))
            {
                return Uri.TryCreate(value, UriKind.Absolute, out var uri)
                       && uri.Scheme == Uri.UriSchemeHttps
                       && string.IsNullOrEmpty(uri.UserInfo)
                       && (uri.Host.Equals(TrustedRootDomain, StringComparison.OrdinalIgnoreCase)
                           || uri.Host.EndsWith("." + TrustedRootDomain, StringComparison.OrdinalIgnoreCase));
            }

            // مسیر نسبی بدون اسلش ابتدایی، مثل Media/2026/9/20/file.webp
            return true;
        }

        public static bool IsAllowedOptional(string url) => string.IsNullOrWhiteSpace(url) || IsAllowed(url);
    }
}
