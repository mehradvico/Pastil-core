using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Order.CartSrv;
using Application.Services.Setting.SmsSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.MarketingSrv
{
    public interface IPaymentReminderService
    {
        /// <summary>job هر ۵ دقیقه: پوش و پیامک «پرداخت ناموفق». تعداد پیام‌های فرستاده‌شده را برمی‌گرداند.</summary>
        Task<int> SendFailedPaymentRemindersAsync(CancellationToken cancellationToken = default);

        /// <summary>job هر ۵ دقیقه: پوش و پیامک «رزرو پرداخت‌نشده». تعداد پیام‌های فرستاده‌شده را برمی‌گرداند.</summary>
        Task<int> SendUnpaidReserveRemindersAsync(CancellationToken cancellationToken = default);
    }

    // سفر ۲ سند اتوماسیون پیام: پرداخت ناموفق و رزرو پرداخت‌نشده. قواعد زمانی در PaymentReminderRules.
    // ضدتکرار از روی همان ردیف‌های PushNotifications و Smses انجام می‌شود (بدون جدول جدید).
    public class PaymentReminderService : IPaymentReminderService
    {
        private readonly IDataBaseContext _context;
        private readonly IPushNotificationService _pushService;
        private readonly ISmsService _smsService;
        private readonly ILogger<PaymentReminderService> _logger;

        public PaymentReminderService(IDataBaseContext context, IPushNotificationService pushService, ISmsService smsService, ILogger<PaymentReminderService> logger)
        {
            _context = context;
            _pushService = pushService;
            _smsService = smsService;
            _logger = logger;
        }

        private sealed record Sent(long UserId, DateTime CreateDate, string Key);

        // ---------------------------------------------------------------- پرداخت ناموفق

        public async Task<int> SendFailedPaymentRemindersAsync(CancellationToken cancellationToken = default)
        {
            var tehranNow = MarketingQuietHours.TehranNow();
            var pushAllowed = MarketingQuietHours.IsPushAllowed(tehranNow);
            var smsAllowed = MarketingQuietHours.IsSmsAllowed(tehranNow);
            if (!pushAllowed && !smsAllowed)
                return 0;

            var now = DateTime.Now;
            var from = now - PaymentReminderRules.FailedMaxAge;
            var attempts = await _context.Payments.AsNoTracking()
                .Where(p => p.IsOnline && p.CreateDate >= from && p.IsSuccess != true &&
                            (p.GatewayStatus == null || (!p.GatewayStatus.StartsWith("TEST_") && p.GatewayStatus != "SUPERSEDED")))
                .Select(p => new { p.UserId, p.CreateDate, p.IsSuccess, p.CallBackTypeLabel, p.CallBackId })
                .ToListAsync(cancellationToken);
            if (attempts.Count == 0)
                return 0;

            // آخرین تلاش ناموفق هر کاربر
            var latest = attempts.GroupBy(a => a.UserId).Select(g => g.OrderByDescending(a => a.CreateDate).First()).ToList();
            var userIds = latest.Select(a => a.UserId).ToList();
            var paidAfter = await _context.Payments.AsNoTracking()
                .Where(p => userIds.Contains(p.UserId) && p.IsSuccess == true && p.CreateDate >= from)
                .Select(p => new { p.UserId, p.CreateDate })
                .ToListAsync(cancellationToken);
            var users = await LoadUsersAsync(userIds, cancellationToken);
            var pushes = await LoadPushesAsync(userIds, PushTypeEnum.PushPaymentFailed, from, cancellationToken);
            var smses = await LoadSmsesAsync(users.Values.Select(u => u.Mobile), MessageTypeEnum.PaymentFailedReminder, now - PaymentReminderRules.SmsCooldown - PaymentReminderRules.FailedMaxAge, cancellationToken);

            var count = 0;
            foreach (var attempt in latest)
            {
                if (!users.TryGetValue(attempt.UserId, out var user))
                    continue;
                if (paidAfter.Any(p => p.UserId == attempt.UserId && p.CreateDate >= attempt.CreateDate))
                    continue;

                var push = pushes.Where(p => p.UserId == attempt.UserId && p.CreateDate >= attempt.CreateDate).OrderBy(p => p.CreateDate).FirstOrDefault();
                if (push == null)
                {
                    if (pushAllowed && PaymentReminderRules.IsFailedPushDue(attempt.IsSuccess, attempt.CreateDate, now) &&
                        await TryPushAsync(PushTypeEnum.PushPaymentFailed, attempt.UserId, PaymentReminderRules.RetryPath(attempt.CallBackTypeLabel, attempt.CallBackId)))
                        count++;
                    continue;
                }

                if (!smsAllowed || !PaymentReminderRules.IsFailedSmsDue(attempt.CreateDate, push.CreateDate, now))
                    continue;
                if (smses.Any(s => s.Receptor == user.Mobile && (s.CreateDate >= attempt.CreateDate || now - s.CreateDate < PaymentReminderRules.SmsCooldown)))
                    continue;
                if (await TrySmsAsync(MessageTypeEnum.PaymentFailedReminder, user))
                    count++;
            }
            return count;
        }

        // ---------------------------------------------------------------- رزرو پرداخت‌نشده

        private sealed record Unpaid(string Kind, long Id, long UserId, DateTime CreateDate, string Name, string Path)
        {
            public string Key => Kind + ":" + Id;
        }

        public async Task<int> SendUnpaidReserveRemindersAsync(CancellationToken cancellationToken = default)
        {
            var tehranNow = MarketingQuietHours.TehranNow();
            var pushAllowed = MarketingQuietHours.IsPushAllowed(tehranNow);
            var smsAllowed = MarketingQuietHours.IsSmsAllowed(tehranNow);
            if (!pushAllowed && !smsAllowed)
                return 0;

            var now = DateTime.Now;
            var from = now - PaymentReminderRules.UnpaidMaxAge;
            var items = await LoadUnpaidAsync(now, from, cancellationToken);
            if (items.Count == 0)
                return 0;

            var userIds = items.Select(i => i.UserId).Distinct().ToList();
            var users = await LoadUsersAsync(userIds, cancellationToken);
            var reservePushes = await LoadPushesAsync(userIds, PushTypeEnum.PushUnpaidReserve, from, cancellationToken);
            var failedPushes = await LoadPushesAsync(userIds, PushTypeEnum.PushPaymentFailed, from, cancellationToken);
            var smsFrom = now - PaymentReminderRules.SmsCooldown - PaymentReminderRules.UnpaidMaxAge;
            var mobiles = users.Values.Select(u => u.Mobile).ToList();
            var reserveSmses = await LoadSmsesAsync(mobiles, MessageTypeEnum.UnpaidReserveReminder, smsFrom, cancellationToken);
            var failedSmses = await LoadSmsesAsync(mobiles, MessageTypeEnum.PaymentFailedReminder, smsFrom, cancellationToken);

            var count = 0;
            var smsSentNow = new HashSet<long>();
            foreach (var item in items.OrderBy(i => i.CreateDate))
            {
                if (!users.TryGetValue(item.UserId, out var user))
                    continue;
                // کاربر بعد از ثبت این رزرو پیام «پرداخت ناموفق» گرفته: همان کافی است
                if (failedPushes.Any(p => p.UserId == item.UserId && p.CreateDate >= item.CreateDate))
                    continue;

                var push = reservePushes.FirstOrDefault(p => p.UserId == item.UserId && p.Key == item.Key);
                if (push == null)
                {
                    if (pushAllowed && PaymentReminderRules.IsUnpaidPushDue(item.CreateDate, now) &&
                        await TryPushAsync(PushTypeEnum.PushUnpaidReserve, item.UserId, item.Name, item.Path, item.Key))
                        count++;
                    continue;
                }

                if (!smsAllowed || smsSentNow.Contains(item.UserId) || !PaymentReminderRules.IsUnpaidSmsDue(item.CreateDate, push.CreateDate, now))
                    continue;
                if (reserveSmses.Any(s => s.Receptor == user.Mobile && now - s.CreateDate < PaymentReminderRules.SmsCooldown) ||
                    failedSmses.Any(s => s.Receptor == user.Mobile && s.CreateDate >= item.CreateDate))
                    continue;
                if (await TrySmsAsync(MessageTypeEnum.UnpaidReserveReminder, user))
                {
                    smsSentNow.Add(item.UserId);
                    count++;
                }
            }
            return count;
        }

        // رزروهایی که کاربر ثبت کرده، لغو نشده، موعدش نگذشته و هنوز مبلغ آنلاین‌شان پرداخت نشده
        private async Task<List<Unpaid>> LoadUnpaidAsync(DateTime now, DateTime from, CancellationToken ct)
        {
            var list = new List<Unpaid>();
            const string reservePath = "/reserve";

            list.AddRange((await _context.ClinicReserves.AsNoTracking()
                    .Where(r => r.Status == 0 && !r.IsCancel && r.PaymentPrice > 0 && r.CreateDate >= from && r.DoDate > now)
                    .Select(r => new { r.Id, r.BookerId, r.CreateDate, r.Clinic.Name }).ToListAsync(ct))
                .Select(r => new Unpaid("clinic", r.Id, r.BookerId, r.CreateDate, r.Name, reservePath)));
            list.AddRange((await _context.GroomerReserves.AsNoTracking()
                    .Where(r => r.Status == 0 && !r.IsCancel && r.PaymentPrice > 0 && r.CreateDate >= from && r.DoDate > now)
                    .Select(r => new { r.Id, r.BookerId, r.CreateDate, r.Groomer.Name }).ToListAsync(ct))
                .Select(r => new Unpaid("groomer", r.Id, r.BookerId, r.CreateDate, r.Name, reservePath)));
            list.AddRange((await _context.CoachReserves.AsNoTracking()
                    .Where(r => r.Status == 0 && !r.IsCancel && r.PaymentPrice > 0 && r.CreateDate >= from && r.DoDate > now)
                    .Select(r => new { r.Id, r.BookerId, r.CreateDate, r.Coach.Name }).ToListAsync(ct))
                .Select(r => new Unpaid("coach", r.Id, r.BookerId, r.CreateDate, r.Name, reservePath)));

            var pansionRegistered = (long)PansionReserveStatusEnum.PansionReserveState_Registered;
            list.AddRange((await _context.PansionReserves.AsNoTracking()
                    .Where(r => r.StatusId == pansionRegistered && !r.IsCancel && r.PaymentPrice > 0 && r.CreateDate >= from &&
                                (r.FromDate == null || r.FromDate > now))
                    .Select(r => new { r.Id, r.BookerId, r.CreateDate, r.Pansion.Name }).ToListAsync(ct))
                .Select(r => new Unpaid("pansion", r.Id, r.BookerId, r.CreateDate, r.Name, reservePath)));

            var schoolRegistered = (int)SchoolReserveStatusEnum.Registered;
            list.AddRange((await _context.SchoolReserves.AsNoTracking()
                    .Where(r => r.StatusId == schoolRegistered && !r.IsCancel && r.PaymentPrice > 0 && r.CreateDate >= from)
                    .Select(r => new { r.Id, r.BookerId, r.CreateDate, r.SchoolCourse.Name }).ToListAsync(ct))
                .Select(r => new Unpaid("school", r.Id, r.BookerId, r.CreateDate, r.Name, reservePath)));

            var consultationPending = (int)ConsultationPurchaseStatusEnum.PendingPayment;
            list.AddRange((await _context.ConsultationPurchases.AsNoTracking()
                    .Where(r => r.Status == consultationPending && r.CancelDate == null && r.PaymentPrice > 0 && r.CreateDate >= from)
                    .Select(r => new { r.Id, r.UserId, r.CreateDate, r.PackageName }).ToListAsync(ct))
                .Select(r => new Unpaid("consultation", r.Id, r.UserId, r.CreateDate, r.PackageName, "/consultations")));

            var tripRequested = (long)TripStatusEnum.TripStatus_Requested;
            list.AddRange((await _context.Trips.AsNoTracking()
                    .Where(t => !t.IsPaid && !t.PaidWithReserve && t.TripStatusId == tripRequested && t.PaymentPrice > 0 && t.CreateDate >= from)
                    .Select(t => new { t.Id, t.UserId, t.CreateDate }).ToListAsync(ct))
                .Select(t => new Unpaid("trip", t.Id, t.UserId, t.CreateDate, Resource.Notification.UnpaidReserveTripName, "/trip/" + t.Id)));

            return list
                .Select(i => i with { Name = ShortName(i.Name) })
                .ToList();
        }

        private static string ShortName(string name)
        {
            var shortName = AbandonedCartReminderRules.ShortProductName(name);
            return string.IsNullOrWhiteSpace(shortName) ? Resource.Notification.UnpaidReserveDefaultName : shortName;
        }

        // ---------------------------------------------------------------- کمکی‌ها

        private async Task<Dictionary<long, (string Mobile, string FirstName)>> LoadUsersAsync(List<long> userIds, CancellationToken ct)
        {
            var rows = await _context.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id) && !u.Deleted && !u.Locked && u.Mobile != null && u.Mobile != "")
                .Select(u => new { u.Id, u.Mobile, u.FirstName })
                .ToListAsync(ct);
            return rows.ToDictionary(u => u.Id, u => (u.Mobile, u.FirstName));
        }

        private async Task<List<Sent>> LoadPushesAsync(List<long> userIds, PushTypeEnum type, DateTime from, CancellationToken ct)
        {
            var typeId = (long)type;
            var rows = await _context.PushNotifications.AsNoTracking()
                .Where(n => userIds.Contains(n.UserId) && n.PushPattern.PushTypeId == typeId && n.CreateDate >= from)
                .Select(n => new { n.UserId, n.CreateDate, n.Token3 })
                .ToListAsync(ct);
            return rows.Select(r => new Sent(r.UserId, r.CreateDate, r.Token3)).ToList();
        }

        private async Task<List<(string Receptor, DateTime CreateDate)>> LoadSmsesAsync(IEnumerable<string> mobiles, MessageTypeEnum type, DateTime from, CancellationToken ct)
        {
            var list = mobiles.Distinct().ToList();
            if (list.Count == 0)
                return new();
            var label = type.ToString();
            var rows = await _context.Smses.AsNoTracking()
                .Where(s => list.Contains(s.Receptor) && s.SmsType.Label == label && s.CreateDate >= from)
                .Select(s => new { s.Receptor, s.CreateDate })
                .ToListAsync(ct);
            return rows.Select(r => (r.Receptor, r.CreateDate)).ToList();
        }

        private async Task<bool> TryPushAsync(PushTypeEnum type, long userId, string token1, string token2 = null, string token3 = null)
        {
            try
            {
                await _pushService.SendPushAsync(type, userId, token1: token1, token2: token2, token3: token3);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{PushType} reminder push failed for user {UserId}.", type, userId);
                return false;
            }
        }

        private async Task<bool> TrySmsAsync(MessageTypeEnum type, (string Mobile, string FirstName) user)
        {
            try
            {
                var name = AbandonedCartReminderRules.FirstNameOnly(user.FirstName) ?? Resource.Notification.AbandonedCartDefaultCustomerName;
                await _smsService.SendSmsAsync(type, user.Mobile, token1: name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{SmsType} reminder SMS failed for {Mobile}.", type, user.Mobile);
                return false;
            }
        }
    }
}
