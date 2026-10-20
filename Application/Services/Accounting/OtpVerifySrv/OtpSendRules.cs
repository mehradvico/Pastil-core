using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Accounting.OtpVerifySrv
{
    // محدودیت ارسال کد برای هر شماره/ایمیل (جدا از محدودیت per-IP در Program.cs و قفل ۳ کد در ۱۰ دقیقه‌ی OtpVerify):
    // حداکثر یک کد در ۲ دقیقه و ۵ کد در هر ساعت؛ جلوی SMS bombing روی شماره‌ی دیگران از IPهای مختلف را می‌گیرد.
    // حافظه‌ی همین نمونه‌ی API نگه داشته می‌شود (یک کانتینر)؛ با ری‌استارت صفر می‌شود که برای این محدودیت قابل قبول است.
    public static class OtpSendRules
    {
        public static readonly TimeSpan MinInterval = TimeSpan.FromMinutes(2);
        public static readonly TimeSpan Window = TimeSpan.FromHours(1);
        public const int MaxPerWindow = 5;

        public enum BlockReason { None, TooSoon, HourlyLimit }

        public readonly record struct Decision(BlockReason Reason, TimeSpan RetryAfter)
        {
            public bool Allowed => Reason == BlockReason.None;
        }

        public static Decision Evaluate(IEnumerable<DateTime> previousSends, DateTime now)
        {
            var recent = (previousSends ?? Enumerable.Empty<DateTime>())
                .Where(sentAt => now - sentAt < Window)
                .OrderBy(sentAt => sentAt)
                .ToList();
            if (recent.Count > 0 && now - recent[^1] < MinInterval)
                return new Decision(BlockReason.TooSoon, MinInterval - (now - recent[^1]));
            if (recent.Count >= MaxPerWindow)
                return new Decision(BlockReason.HourlyLimit, Window - (now - recent[0]));
            return new Decision(BlockReason.None, TimeSpan.Zero);
        }

        private static readonly object Gate = new();

        // بررسی و ثبت در یک قفل تا دو درخواست هم‌زمان هر دو از محدودیت رد نشوند
        public static Decision TryRegisterSend(IMemoryCache cache, string recipient, DateTime now)
        {
            if (cache == null || string.IsNullOrWhiteSpace(recipient))
                return new Decision(BlockReason.None, TimeSpan.Zero);

            var key = "otp-send:" + recipient.Trim().ToLowerInvariant();
            lock (Gate)
            {
                var sends = cache.TryGetValue(key, out List<DateTime> stored) && stored != null
                    ? stored.Where(sentAt => now - sentAt < Window).ToList()
                    : new List<DateTime>();
                var decision = Evaluate(sends, now);
                if (!decision.Allowed)
                    return decision;

                sends.Add(now);
                cache.Set(key, sends, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = Window,
                    Size = 1
                });
                return decision;
            }
        }
    }
}
