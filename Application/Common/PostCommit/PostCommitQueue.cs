using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Common.PostCommit
{
    // کارهای جانبیِ best-effort (پیامک، پوش، اعلان ادمین) که نباید داخل تراکنش دیتابیس و پشت سر کاربر اجرا شوند: کاربر در صفحه‌ی
    // نتیجه‌ی پرداخت منتظر می‌ماند و تراکنش Serializable تا پایان تماس‌های شبکه قفل نگه داشته می‌شد (پوش از سرور ایران ممکن است
    // ده‌ها ثانیه معلق شود). سرویس‌ها کار را اینجا ثبت می‌کنند؛ فقط بعد از Commit موفق (PaymentService) در پس‌زمینه و با
    // scope تازه‌ی DI اجرا می‌شود. اگر تراکنش برگردد، Clear/عدم Flush یعنی هیچ‌کدام اجرا نمی‌شوند.
    public interface IPostCommitQueue
    {
        // فقط وقتی «فعال» است سرویس‌هایی مثل Push کار کندشان را به بعد از Commit می‌اندازند (PaymentService داخل
        // ApplyAndMapPaymentAsync فعال می‌کند و Flush می‌کند)؛ در بقیه‌ی تراکنش‌ها رفتار قبلی (ارسال هم‌زمان) می‌ماند.
        bool IsActive { get; }
        void Activate();
        void Deactivate();
        void Add(string name, Func<IServiceProvider, Task> action);
        void Clear();
        void Flush();
    }

    public sealed class PostCommitQueue : IPostCommitQueue
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PostCommitQueue> _logger;
        private readonly List<(string Name, Func<IServiceProvider, Task> Action)> _items = new();
        private readonly object _gate = new();

        public PostCommitQueue(IServiceScopeFactory scopeFactory, ILogger<PostCommitQueue> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        private volatile bool _active;

        public bool IsActive => _active;
        public void Activate() => _active = true;
        public void Deactivate() => _active = false;

        public void Add(string name, Func<IServiceProvider, Task> action)
        {
            lock (_gate)
                _items.Add((name, action));
        }

        public void Clear()
        {
            lock (_gate)
                _items.Clear();
        }

        public void Flush()
        {
            List<(string Name, Func<IServiceProvider, Task> Action)> batch;
            lock (_gate)
            {
                if (_items.Count == 0)
                    return;
                batch = new(_items);
                _items.Clear();
            }

            _ = Task.Run(async () =>
            {
                foreach (var (name, action) in batch)
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        await action(scope.ServiceProvider);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Post-commit action {ActionName} failed.", name);
                    }
                }
            });
        }
    }
}
