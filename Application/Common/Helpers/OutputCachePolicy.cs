using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Application.Common.Helpers
{
    /// <summary>
    /// تصمیم‌های کش خروجی (CustomOutputCacheAttribute) بیرون از فیلتر تا بدون Host قابل‌تست باشد.
    /// <c>CacheSettings:DisableAll=true</c> همه‌ی کش‌ها را خاموش می‌کند، مگر کلیدی که در
    /// <c>CacheSettings:EnabledKeys</c> آمده باشد (روشن‌کردن فقط یک نوع، مثلاً ProductSearch، با مدت کوتاه).
    /// </summary>
    public static class OutputCachePolicy
    {
        /// <summary>پاسخ بزرگ‌تر از این (تعداد نویسه‌ی JSON) هرگز کش نمی‌شود تا حافظه‌ی سرور پر نشود.</summary>
        public const int MaxCachedChars = 200_000;

        /// <summary>هر ۱۰ هزار نویسه یک واحد از SizeLimit (۱۰٬۰۰۰ واحد) مصرف می‌کند؛ حداقل ۱.</summary>
        public const int CharsPerSizeUnit = 10_000;

        public static bool IsEnabled(IConfiguration configuration, string key)
        {
            if (!configuration.GetValue<bool>("CacheSettings:DisableAll")) return true;
            var enabled = configuration.GetSection("CacheSettings:EnabledKeys").Get<string[]>();
            return enabled != null && enabled.Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        public static int EntrySize(int chars) => Math.Max(1, chars / CharsPerSizeUnit);

        public static bool ShouldStore(int chars) => chars <= MaxCachedChars;
    }
}
