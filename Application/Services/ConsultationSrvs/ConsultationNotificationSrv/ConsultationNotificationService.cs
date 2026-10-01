using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationNotificationSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationNotificationSrv
{
    public class ConsultationNotificationService : IConsultationNotificationService
    {
        private const int BatchSize = 50;
        // بعد از این مدت از پرداخت، اگر هنوز کسی شروع نکرده یادآوری می‌رود
        public static readonly TimeSpan UnclaimedAfter = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan EndingSoonLead = TimeSpan.FromMinutes(5);
        private static readonly CultureInfo Persian = new("fa");

        private readonly IDataBaseContext _context;
        private readonly IPushNotificationService _push;

        public ConsultationNotificationService(IDataBaseContext context, IPushNotificationService push)
        {
            _context = context;
            _push = push;
        }

        // مقصد کلیک روی پوش (وب‌اپ): صفحه‌ی مشاوره‌های کاربر / مدیریت نماینده
        public const string UserPagePath = "/consultations";
        public const string AgentPagePath = "/consultations/manage";

        public async Task NotifyPurchasedAsync(long purchaseId)
        {
            var purchase = await LoadAsync(purchaseId);
            var paid = (int)ConsultationPurchaseStatusEnum.Paid;
            var active = (int)ConsultationPurchaseStatusEnum.Active;
            if (purchase == null || (purchase.Status != paid && purchase.Status != active))
                return;

            var buyerName = FullName(purchase.User);
            var channelLabel = ChannelLabel(purchase.ChannelId);

            // رزرو ساعت‌دار: پوش «برای ساعت X رزرو شد» (نه «بیا ارتباط را برقرار کن»)
            if (purchase.ScheduledStart.HasValue)
            {
                var when = FormatScheduled(purchase.ScheduledStart.Value);
                foreach (var agentId in await AgentUserIdsAsync(purchase.CompanionId))
                {
                    await SendOnceAsync(PushTypeEnum.PushConsultationBookedAgent, agentId, purchase.Id,
                        buyerName, purchase.Id.ToString(), channelLabel, when);
                }
                await SendOnceAsync(PushTypeEnum.PushConsultationBookedUser, purchase.UserId, purchase.Id,
                    purchase.Companion?.Name ?? string.Empty, purchase.Id.ToString(), when);
                return;
            }

            foreach (var agentId in await AgentUserIdsAsync(purchase.CompanionId))
            {
                await SendOnceAsync(PushTypeEnum.PushConsultationPurchasedAgent, agentId, purchase.Id,
                    buyerName, purchase.Id.ToString(), channelLabel, purchase.DurationMinutes.ToString(Persian));
            }

            await SendOnceAsync(PushTypeEnum.PushConsultationPurchasedUser, purchase.UserId, purchase.Id,
                purchase.Companion?.Name ?? string.Empty, purchase.Id.ToString());
        }

        public async Task<int> NotifyBookingRemindersAsync()
        {
            var now = DateTime.Now;
            var paid = (int)ConsultationPurchaseStatusEnum.Paid;
            var until = now + ConsultationRules.BookingReminderLead;
            var rows = await _context.ConsultationPurchases.AsNoTracking()
                .Where(s => s.Status == paid && s.ScheduledStart != null && s.ScheduledStart > now && s.ScheduledStart <= until)
                .Include(s => s.User)
                .Include(s => s.Companion)
                .OrderBy(s => s.ScheduledStart)
                .Take(BatchSize)
                .ToListAsync();

            var notified = 0;
            foreach (var purchase in rows)
            {
                try
                {
                    // کاربر: با نام کلینیک؛ نمایندگان: با نام کاربر. مقصد کلیک: صفحه‌ی مشاوره‌های هر طرف
                    await SendOnceAsync(PushTypeEnum.PushConsultationBookingReminder, purchase.UserId, purchase.Id,
                        purchase.Companion?.Name ?? string.Empty, purchase.Id.ToString(), UserPagePath);

                    // اگر مشاوره به نماینده‌ای تخصیص دارد: او و مالک؛ وگرنه همه‌ی نمایندگان مجاز
                    var owners = await _context.Companions.AsNoTracking()
                        .Where(c => c.Id == purchase.CompanionId && !c.Deleted).Select(c => c.OwnerId).ToListAsync();
                    var recipients = purchase.AgentUserId.HasValue
                        ? owners.Append(purchase.AgentUserId.Value).Distinct().ToList()
                        : await AgentUserIdsAsync(purchase.CompanionId);
                    foreach (var agentId in recipients)
                    {
                        await SendOnceAsync(PushTypeEnum.PushConsultationBookingReminder, agentId, purchase.Id,
                            FullName(purchase.User), purchase.Id.ToString(), AgentPagePath);
                    }
                    notified++;
                }
                catch { /* اجرای بعدی دوباره تلاش می‌کند */ }
            }
            return notified;
        }

        public async Task NotifyExpiredRefundAsync(long purchaseId)
        {
            var purchase = await LoadAsync(purchaseId);
            if (purchase == null)
                return;

            await SendOnceAsync(PushTypeEnum.PushConsultationExpiredRefund, purchase.UserId, purchase.Id,
                purchase.Companion?.Name ?? string.Empty, purchase.Id.ToString());
        }

        public async Task<int> NotifyPendingPurchasesAsync()
        {
            var now = DateTime.Now;
            var paid = (int)ConsultationPurchaseStatusEnum.Paid;
            var active = (int)ConsultationPurchaseStatusEnum.Active;
            var purchasedUserType = (long)PushTypeEnum.PushConsultationPurchasedUser;
            var bookedUserType = (long)PushTypeEnum.PushConsultationBookedUser;
            var since = now.AddDays(-1);
            var settle = now.AddSeconds(-10);

            // پرداخت‌شده‌ها که پوش «ثبت شد»ِ کاربرشان هنوز نرفته (اعلان لحظه‌ای هم به همین معیار dedupe می‌شود)
            var ids = await _context.ConsultationPurchases.AsNoTracking()
                .Where(s => (s.Status == paid || s.Status == active) && s.PaidDate != null && s.PaidDate > since && s.PaidDate < settle &&
                            !_context.PushNotifications.Any(n => n.UserId == s.UserId && n.Token2 == s.Id.ToString() &&
                                                                 n.PushPattern.PushTypeId == (s.ScheduledStart == null ? purchasedUserType : bookedUserType)))
                .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .Take(BatchSize)
                .ToListAsync();

            foreach (var id in ids)
            {
                try { await NotifyPurchasedAsync(id); } catch { /* اجرای بعدی دوباره تلاش می‌کند */ }
            }
            return ids.Count;
        }

        public async Task<int> NotifyEndingSoonAsync()
        {
            var now = DateTime.Now;
            var active = (int)ConsultationPurchaseStatusEnum.Active;
            var until = now + EndingSoonLead;
            var soon = await _context.ConsultationPurchases.AsNoTracking()
                .Where(s => s.Status == active && s.ExpireDate != null && s.ExpireDate > now && s.ExpireDate <= until)
                .Include(s => s.User)
                .Include(s => s.AgentUser)
                .OrderBy(s => s.ExpireDate)
                .Take(BatchSize)
                .ToListAsync();

            var notified = 0;
            foreach (var purchase in soon)
            {
                try
                {
                    // مقصد پوش کاربر: خودِ گفتگو/تماس؛ نماینده به صفحه‌ی مدیریت می‌رود
                    var userTarget = purchase.OnlineSessionId.HasValue
                        ? purchase.ChannelId == (int)OnlineSessionChannelEnum.Chat
                            ? $"/online-chat/{purchase.OnlineSessionId.Value}"
                            : purchase.ChannelId == (int)OnlineSessionChannelEnum.InAppCall || purchase.ChannelId == (int)OnlineSessionChannelEnum.VideoCall
                                ? $"/call/session/{purchase.OnlineSessionId.Value}"
                                : UserPagePath
                        : UserPagePath;

                    await SendOnceAsync(PushTypeEnum.PushConsultationEndingSoon, purchase.UserId, purchase.Id,
                        FullName(purchase.AgentUser), purchase.Id.ToString(), userTarget);
                    if (purchase.AgentUserId.HasValue)
                    {
                        await SendOnceAsync(PushTypeEnum.PushConsultationEndingSoon, purchase.AgentUserId.Value, purchase.Id,
                            FullName(purchase.User), purchase.Id.ToString(), AgentPagePath);
                    }
                    notified++;
                }
                catch { /* اجرای بعدی دوباره تلاش می‌کند */ }
            }
            return notified;
        }

        public async Task NotifyAssignedAsync(long purchaseId, long agentUserId)
        {
            var purchase = await LoadAsync(purchaseId);
            if (purchase == null || purchase.Status != (int)ConsultationPurchaseStatusEnum.Paid)
                return;

            await SendOnceAsync(PushTypeEnum.PushConsultationAssigned, agentUserId, purchase.Id,
                FullName(purchase.User), purchase.Id.ToString(), ChannelLabel(purchase.ChannelId));
        }

        public async Task NotifyTakenByColleagueAsync(long purchaseId, long takerUserId)
        {
            var purchase = await LoadAsync(purchaseId);
            if (purchase == null)
                return;

            var taker = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == takerUserId);
            var takerName = FullName(taker);
            var buyerName = FullName(purchase.User);
            foreach (var agentId in await AgentUserIdsAsync(purchase.CompanionId))
            {
                if (agentId == takerUserId)
                    continue;
                try
                {
                    await SendOnceAsync(PushTypeEnum.PushConsultationTakenByColleague, agentId, purchase.Id,
                        takerName, purchase.Id.ToString(), buyerName);
                }
                catch { /* پوش همکار نباید شروع مشاوره را خراب کند */ }
            }
        }

        public async Task<int> NotifyUnclaimedAsync()
        {
            var now = DateTime.Now;
            var paid = (int)ConsultationPurchaseStatusEnum.Paid;
            var before = now - UnclaimedAfter;
            var rows = await _context.ConsultationPurchases.AsNoTracking()
                .Where(s => s.Status == paid && s.PaidDate != null && s.PaidDate <= before &&
                            (s.StartDeadline == null || s.StartDeadline > now) &&
                            // رزرو ساعت‌دار: «هنوز کسی جواب نداده» فقط وقتی ساعت رزرو نزدیک/رسیده است، نه روزها قبل
                            (s.ScheduledStart == null || s.ScheduledStart <= now + ConsultationRules.BookingEarlyStart))
                .Include(s => s.User)
                .OrderBy(s => s.PaidDate)
                .Take(BatchSize)
                .ToListAsync();

            var notified = 0;
            foreach (var purchase in rows)
            {
                try
                {
                    var owners = await _context.Companions.AsNoTracking()
                        .Where(c => c.Id == purchase.CompanionId && !c.Deleted).Select(c => c.OwnerId).ToListAsync();
                    var recipients = purchase.AgentUserId.HasValue
                        ? owners.Append(purchase.AgentUserId.Value).Distinct().ToList()
                        : await AgentUserIdsAsync(purchase.CompanionId);

                    var minutes = ((int)UnclaimedAfter.TotalMinutes).ToString(Persian);
                    foreach (var userId in recipients)
                    {
                        await SendOnceAsync(PushTypeEnum.PushConsultationUnclaimed, userId, purchase.Id,
                            FullName(purchase.User), purchase.Id.ToString(), minutes);
                    }
                    notified++;
                }
                catch { /* اجرای بعدی دوباره تلاش می‌کند */ }
            }
            return notified;
        }

        // ---- کمکی‌ها
        private Task<ConsultationPurchase> LoadAsync(long purchaseId) =>
            _context.ConsultationPurchases.AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Companion)
                .FirstOrDefaultAsync(s => s.Id == purchaseId);

        // نمایندگان مجاز کلینیک: مالک + همه‌ی اعضای فعال و تأییدشده‌ی تیم (همان تعریف در ConsultationSessionService؛
        // قبلاً فقط کارکنان خدمت ۱۵ بودند، پس اپراتورها هیچ پوشی برای مشاوره‌ی جدید/بی‌صاحب نمی‌گرفتند)
        private async Task<List<long>> AgentUserIdsAsync(long companionId)
        {
            var owners = await _context.Companions.AsNoTracking()
                .Where(s => s.Id == companionId && !s.Deleted)
                .Select(s => s.OwnerId)
                .ToListAsync();
            var members = await _context.CompanionUsers.AsNoTracking()
                .Where(s => s.CompanionId == companionId && s.Active && s.UserAccept == true && !s.Deleted)
                .Select(s => s.UserId)
                .ToListAsync();
            return owners.Concat(members).Distinct().ToList();
        }

        // هر (کاربر، نوع پوش، خرید) فقط یک‌بار: token2 همیشه شناسه‌ی خرید است
        private async Task SendOnceAsync(PushTypeEnum type, long userId, long purchaseId,
            string token1, string token2, string token3 = null, string token4 = null)
        {
            var typeId = (long)type;
            var key = purchaseId.ToString();
            var already = await _context.PushNotifications.AsNoTracking()
                .AnyAsync(n => n.UserId == userId && n.Token2 == key && n.PushPattern.PushTypeId == typeId);
            if (already)
                return;

            await _push.SendPushAsync(type, userId, token1, token2, token3, token4);
        }

        // «۱۴۰۵/۰۷/۱۱ ساعت ۱۶:۰۰» (تاریخ شمسی)
        private static string FormatScheduled(DateTime when)
        {
            var calendar = new PersianCalendar();
            return string.Create(CultureInfo.InvariantCulture,
                $"{calendar.GetYear(when)}/{calendar.GetMonth(when):D2}/{calendar.GetDayOfMonth(when):D2} ساعت {when:HH:mm}");
        }

        private static string FullName(Entities.Entities.Security.User user) =>
            $"{user?.FirstName} {user?.LastName}".Trim();

        private static string ChannelLabel(int channelId) => channelId switch
        {
            (int)OnlineSessionChannelEnum.Chat => Resource.Lang.ResourceManager.GetString("ConsultationChannelChat", Persian),
            (int)OnlineSessionChannelEnum.InAppCall => Resource.Lang.ResourceManager.GetString("ConsultationChannelInAppCall", Persian),
            (int)OnlineSessionChannelEnum.VideoCall => Resource.Lang.ResourceManager.GetString("ConsultationChannelVideoCall", Persian),
            (int)OnlineSessionChannelEnum.Phone => Resource.Lang.ResourceManager.GetString("ConsultationChannelPhone", Persian),
            _ => string.Empty
        };
    }
}
