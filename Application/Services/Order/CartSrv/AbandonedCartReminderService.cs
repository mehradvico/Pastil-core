using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Order.CartSrv.Iface;
using Application.Services.Setting.SmsSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.CartSrv
{
    public class AbandonedCartReminderService : IAbandonedCartReminderService
    {
        private const int QueryChunkSize = 500;

        private readonly IDataBaseContext _context;
        private readonly IPushNotificationService _pushService;
        private readonly ISmsService _smsService;
        private readonly ILogger<AbandonedCartReminderService> _logger;

        public AbandonedCartReminderService(
            IDataBaseContext context,
            IPushNotificationService pushService,
            ISmsService smsService,
            ILogger<AbandonedCartReminderService> logger)
        {
            _context = context;
            _pushService = pushService;
            _smsService = smsService;
            _logger = logger;
        }

        private sealed class Candidate
        {
            public long UserId { get; init; }
            public DateTime LastActivity { get; init; }
            public string ProductName { get; init; }
        }

        // کاربران لاگین‌کرده‌ای که الان واقعاً کالایی (موجود و فعال) در سبدشان دارند. سبد خالی‌شده (کاربر حذف کرده یا سفارش داده)
        // هیچ ردیف CartItem ندارد، پس خودبه‌خود کنار می‌رود؛ این چک در لحظه‌ی ارسال از روی داده‌ی زنده انجام می‌شود.
        private async Task<List<Candidate>> LoadCandidatesAsync(DateTime now, CancellationToken cancellationToken)
        {
            var from = now - AbandonedCartReminderRules.MaxAge;
            var rows = await _context.CartItems.AsNoTracking()
                .Where(item => item.Count > 0
                               && item.CreateDate >= from
                               && item.CartStore.Cart.UserId != null
                               && item.ProductItem.Active && item.ProductItem.SystemActive && !item.ProductItem.Deleted
                               && item.ProductItem.Quantity > 0
                               && item.ProductItem.Product.Active && !item.ProductItem.Product.Deleted)
                .Select(item => new
                {
                    UserId = item.CartStore.Cart.UserId.Value,
                    item.CreateDate,
                    item.ProductItem.Product.Name
                })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(row => row.UserId)
                .Select(group =>
                {
                    var latest = group.OrderByDescending(row => row.CreateDate).First();
                    return new Candidate { UserId = group.Key, LastActivity = latest.CreateDate, ProductName = latest.Name };
                })
                .Where(candidate => now - candidate.LastActivity >= AbandonedCartReminderRules.MinIdle)
                .ToList();
        }

        public async Task<int> SendPushRemindersAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var candidates = await LoadCandidatesAsync(now, cancellationToken);
            if (candidates.Count == 0)
                return 0;

            var typeId = (long)PushTypeEnum.PushAbandonedCart;
            var minActivity = candidates.Min(c => c.LastActivity);
            var sent = new List<(long UserId, DateTime CreateDate)>();
            foreach (var chunk in candidates.Select(c => c.UserId).Chunk(QueryChunkSize))
            {
                var ids = chunk.ToList();
                var rows = await _context.PushNotifications.AsNoTracking()
                    .Where(n => ids.Contains(n.UserId) && n.PushPattern.PushTypeId == typeId && n.CreateDate >= minActivity)
                    .Select(n => new { n.UserId, n.CreateDate })
                    .ToListAsync(cancellationToken);
                sent.AddRange(rows.Select(r => (r.UserId, r.CreateDate)));
            }

            var count = 0;
            foreach (var candidate in candidates)
            {
                var already = sent.Count(s => s.UserId == candidate.UserId && s.CreateDate >= candidate.LastActivity);
                if (!AbandonedCartReminderRules.IsEligible(candidate.LastActivity, now, already))
                    continue;
                try
                {
                    var name = AbandonedCartReminderRules.ShortProductName(candidate.ProductName);
                    if (string.IsNullOrWhiteSpace(name))
                        name = Resource.Notification.AbandonedCartDefaultProductName;
                    await _pushService.SendPushAsync(PushTypeEnum.PushAbandonedCart, candidate.UserId, token1: name);
                    count++;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Abandoned cart push failed for user {UserId}.", candidate.UserId);
                }
            }
            return count;
        }

        public async Task<int> SendSmsRemindersAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var candidates = await LoadCandidatesAsync(now, cancellationToken);
            if (candidates.Count == 0)
                return 0;

            var users = new Dictionary<long, (string Mobile, string FirstName)>();
            foreach (var chunk in candidates.Select(c => c.UserId).Chunk(QueryChunkSize))
            {
                var ids = chunk.ToList();
                var rows = await _context.Users.AsNoTracking()
                    .Where(u => ids.Contains(u.Id) && !u.Deleted && !u.Locked && u.Mobile != null && u.Mobile != "")
                    .Select(u => new { u.Id, u.Mobile, u.FirstName })
                    .ToListAsync(cancellationToken);
                foreach (var row in rows)
                    users[row.Id] = (row.Mobile, row.FirstName);
            }

            var label = MessageTypeEnum.UserAbandonedCart.ToString();
            var mobiles = users.Values.Select(u => u.Mobile).Distinct().ToList();
            var minActivity = candidates.Min(c => c.LastActivity);
            var sent = new List<(string Receptor, DateTime CreateDate)>();
            foreach (var chunk in mobiles.Chunk(QueryChunkSize))
            {
                var batch = chunk.ToList();
                var rows = await _context.Smses.AsNoTracking()
                    .Where(s => batch.Contains(s.Receptor) && s.SmsType.Label == label && s.CreateDate >= minActivity)
                    .Select(s => new { s.Receptor, s.CreateDate })
                    .ToListAsync(cancellationToken);
                sent.AddRange(rows.Select(r => (r.Receptor, r.CreateDate)));
            }

            var count = 0;
            foreach (var candidate in candidates)
            {
                if (!users.TryGetValue(candidate.UserId, out var user))
                    continue;
                var already = sent.Count(s => s.Receptor == user.Mobile && s.CreateDate >= candidate.LastActivity);
                if (!AbandonedCartReminderRules.IsEligible(candidate.LastActivity, now, already))
                    continue;
                try
                {
                    var customer = AbandonedCartReminderRules.FirstNameOnly(user.FirstName) ?? Resource.Notification.AbandonedCartDefaultCustomerName;
                    var product = AbandonedCartReminderRules.ShortProductName(candidate.ProductName);
                    if (string.IsNullOrWhiteSpace(product))
                        product = Resource.Notification.AbandonedCartDefaultProductName;
                    // قالب کاوه‌نگار: %token = نام کاربر، %token2 = نام کالا. توکن‌های ۱ تا ۳ فاصله نمی‌پذیرند و SmsService فاصله را
                    // به «_» تبدیل می‌کند؛ برای متن خواناتر، فاصله‌ی نام کالا از قبل با نیم‌فاصله (ZWNJ) عوض می‌شود.
                    product = product.Replace(' ', '‌');
                    await _smsService.SendSmsAsync(MessageTypeEnum.UserAbandonedCart, user.Mobile, token1: customer, token2: product);
                    count++;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Abandoned cart SMS failed for user {UserId}.", candidate.UserId);
                }
            }
            return count;
        }
    }
}
