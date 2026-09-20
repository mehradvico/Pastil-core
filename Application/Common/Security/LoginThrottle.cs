using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Application.Common.Security
{
    /// <summary>
    /// سقف تلاش ناموفق رمز برای هر حساب. محدودکننده‌ی IP به‌تنهایی جلوی حمله‌ی توزیع‌شده روی یک حساب را نمی‌گیرد؛
    /// این کلاس پس از N خطا در بازه، ورود با رمز آن حساب را برای مدتی می‌بندد (کد OTP مسیر جدا و قفل خودش را دارد).
    /// حافظه‌ی داخل پروسه است (schema/دیتابیس تغییر نمی‌کند)؛ با restart ریست می‌شود که برای throttle قابل قبول است.
    /// </summary>
    public interface ILoginThrottle
    {
        bool IsBlocked(string key, out TimeSpan retryAfter);
        void RegisterFailure(string key);
        void Reset(string key);
    }

    public sealed class LoginThrottle : ILoginThrottle
    {
        private sealed class Entry
        {
            public int Failures;
            public DateTime WindowStart;
            public DateTime? BlockedUntil;
        }

        private readonly ConcurrentDictionary<string, Entry> _entries = new ConcurrentDictionary<string, Entry>();
        private readonly int _maxFailures;
        private readonly TimeSpan _window;
        private readonly TimeSpan _blockDuration;
        private readonly Func<DateTime> _now;

        public LoginThrottle() : this(5, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15), () => DateTime.UtcNow) { }

        public LoginThrottle(int maxFailures, TimeSpan window, TimeSpan blockDuration, Func<DateTime> now)
        {
            _maxFailures = maxFailures;
            _window = window;
            _blockDuration = blockDuration;
            _now = now;
        }

        public bool IsBlocked(string key, out TimeSpan retryAfter)
        {
            retryAfter = TimeSpan.Zero;
            if (string.IsNullOrEmpty(key) || !_entries.TryGetValue(key, out var entry))
                return false;

            lock (entry)
            {
                if (entry.BlockedUntil.HasValue && entry.BlockedUntil.Value > _now())
                {
                    retryAfter = entry.BlockedUntil.Value - _now();
                    return true;
                }
            }
            return false;
        }

        public void RegisterFailure(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            var now = _now();
            var entry = _entries.GetOrAdd(key, _ => new Entry { WindowStart = now });
            lock (entry)
            {
                if (now - entry.WindowStart > _window || (entry.BlockedUntil.HasValue && entry.BlockedUntil.Value <= now))
                {
                    entry.Failures = 0;
                    entry.WindowStart = now;
                    entry.BlockedUntil = null;
                }

                entry.Failures++;
                if (entry.Failures >= _maxFailures)
                    entry.BlockedUntil = now + _blockDuration;
            }

            if (_entries.Count > 20_000)
                Prune(now);
        }

        public void Reset(string key)
        {
            if (!string.IsNullOrEmpty(key))
                _entries.TryRemove(key, out _);
        }

        private void Prune(DateTime now)
        {
            foreach (var pair in _entries.ToArray())
            {
                var entry = pair.Value;
                lock (entry)
                {
                    var expired = now - entry.WindowStart > _window && (!entry.BlockedUntil.HasValue || entry.BlockedUntil.Value <= now);
                    if (expired)
                        _entries.TryRemove(pair.Key, out _);
                }
            }
        }
    }
}
