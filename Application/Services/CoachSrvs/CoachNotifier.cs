using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs
{
    /// <summary>
    /// همه‌ی پوش و پیامک‌های مربی یک‌جا. هیچ متدی خطا پرت نمی‌کند (شکست اعلان نباید رزرو/پرداخت را برگرداند).
    /// پیامک بدون ردیف SmsSetting بی‌صدا ارسال نمی‌شود؛ ساخت قالب‌ها: scripts/SeedCoachSms.sql
    /// </summary>
    public interface ICoachNotifier
    {
        Task CoachDecidedAsync(long coachId, bool approved, string reason);
        Task ReservePaidAsync(long reserveId);
        Task ReserveApprovedAsync(long reserveId);
        Task ReserveRejectedAsync(long reserveId, string reason);
        Task ReserveExpiredAsync(long reserveId);
        Task ReserveCancelledByBookerAsync(long reserveId);
        Task ReserveCancelledByAdminAsync(long reserveId, string reason);
        Task ReserveRescheduledAsync(long reserveId, bool byOwner, DateTime oldDoDate);
        /// <summary>سفر پت‌رسانِ متصل با رزرو جابه‌جا شد: به راننده‌ی پذیرنده (اگر بود) خبر می‌دهد (همان قالب پیامک/پوش تغییر زمان Companion).</summary>
        Task StaffInvitedAsync(long coachId, long userId);
    }

    public class CoachNotifier : ICoachNotifier
    {
        private readonly IDataBaseContext _context;
        private readonly IPushNotificationService _push;
        private readonly IMessageSenderService _sms;
        private readonly ILogger<CoachNotifier> _logger;

        public CoachNotifier(IDataBaseContext context, IPushNotificationService push, IMessageSenderService sms, ILogger<CoachNotifier> logger)
        {
            _context = context;
            _push = push;
            _sms = sms;
            _logger = logger;
        }

        private sealed record ReserveInfo(
            long Id, long CoachId, string CoachName, long OwnerId, string OwnerMobile, long BookerId, string BookerFirstName,
            string BookerMobile, string PetName, DateTime DoDate, string ServiceName, int Mode, long? AssigneeUserId, string BookerName);

        public async Task CoachDecidedAsync(long coachId, bool approved, string reason)
        {
            await SafeAsync("coach decision", async () =>
            {
                var coach = await _context.Coaches.AsNoTracking().Where(g => g.Id == coachId)
                    .Select(g => new { g.Name, g.OwnerId, g.Owner.Mobile }).FirstOrDefaultAsync();
                if (coach == null)
                    return;
                var name = Rules.SmsText(coach.Name);
                await _push.SendPushAsync(approved ? PushTypeEnum.PushCoachApproved : PushTypeEnum.PushCoachRejected,
                    coach.OwnerId, token1: coach.Name, token2: reason);
            });
        }

        public async Task ReservePaidAsync(long reserveId)
        {
            await SafeAsync("reserve paid", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushCoachReserveApprovalRequired, r.OwnerId,
                    token1: r.CoachName, token2: r.PetName, token3: r.Id.ToString());
                // پیامک: همان قالب‌های رزرو Companion (قالب جدید نداریم).
                await _sms.SendMessageAsync(MessageTypeEnum.CompanionReserveForUser, r.BookerMobile, null,
                    token1: r.ServiceName);
                await _sms.SendMessageAsync(MessageTypeEnum.CompanionReserveForCompanion, r.OwnerMobile, null,
                    token1: r.ServiceName, token2: r.BookerFirstName);

                // رزرو آنلاین: دو یادآور زمان‌بندی‌شده (۱۰ دقیقه قبل و سر ساعت) برای مربی
                if (r.Mode == (int)Entities.Entities.CoachField.CoachServiceModeEnum.Online)
                {
                    var targetUserId = r.AssigneeUserId ?? r.OwnerId;
                    await _push.SendPushAsync(PushTypeEnum.PushCoachOnlineReserveReminderBefore, targetUserId,
                        token1: r.BookerName, token2: r.ServiceName, token3: r.CoachName, token4: r.Id.ToString(),
                        sendDate: CoachReserveReminders.BeforeAt(r.DoDate));
                    await _push.SendPushAsync(PushTypeEnum.PushCoachOnlineReserveReminderAtTime, targetUserId,
                        token1: r.BookerName, token2: r.ServiceName, token3: r.CoachName, token4: r.Id.ToString(),
                        sendDate: r.DoDate);
                }

            });
        }

        public async Task ReserveApprovedAsync(long reserveId)
        {
            await SafeAsync("reserve approved", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushCoachReserveApproved, r.BookerId,
                    token1: r.CoachName, token2: r.PetName, token3: r.Id.ToString());
            });
        }

        public async Task ReserveRejectedAsync(long reserveId, string reason)
        {
            await SafeAsync("reserve rejected", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushCoachReserveRejected, r.BookerId,
                    token1: r.CoachName, token2: reason, token3: r.Id.ToString());
            });
        }

        public async Task ReserveExpiredAsync(long reserveId)
        {
            await SafeAsync("reserve expired", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushCoachReserveExpired, r.BookerId, token1: r.CoachName, token3: r.Id.ToString());
            });
        }

        public async Task ReserveCancelledByBookerAsync(long reserveId)
        {
            await SafeAsync("reserve cancelled by booker", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushCoachReserveCancelledByBooker, r.OwnerId,
                    token1: r.PetName, token2: r.DoDate.ToString("yyyy/MM/dd HH:mm"), token3: r.Id.ToString());
            });
        }

        public async Task ReserveCancelledByAdminAsync(long reserveId, string reason)
        {
            await SafeAsync("reserve cancelled by admin", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushCoachReserveCancelledByAdmin, r.BookerId,
                    token1: r.CoachName, token2: reason, token3: r.Id.ToString());
            });
        }

        public async Task ReserveRescheduledAsync(long reserveId, bool byOwner, DateTime oldDoDate)
        {
            await SafeAsync("reserve rescheduled", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                var date = Rules.JalaliDate(r.DoDate);
                await _push.SendPushAsync(PushTypeEnum.PushCoachReserveRescheduled, r.BookerId,
                    token1: r.CoachName, token2: date, token3: Rules.Time(r.DoDate));
                // پیامک: همان قالب تغییر زمان Companion (مرکز، تاریخ قبلی، تاریخ جدید، ساعت جدید، علت).
                await _sms.SendMessageAsync(MessageTypeEnum.CompanionReserveRescheduledUser, r.BookerMobile, null,
                    token1: Rules.SmsText(r.CoachName), token2: Rules.JalaliDate(oldDoDate), token3: date,
                    token4: Rules.SmsTime(r.DoDate),
                    token5: Rules.SmsText(byOwner ? Resource.Notification.CoachRescheduleReasonByOwner : Resource.Notification.CoachRescheduleReasonBySupport));
                // وقتی خود مربی زمان را عوض کرده، به خودش خبر نمی‌دهیم؛ فقط اگر ادمین عوض کرده.
                if (!byOwner)
                {
                    await _push.SendPushAsync(PushTypeEnum.PushCoachReserveRescheduledForCoach, r.OwnerId,
                        token1: r.PetName, token2: date, token3: Rules.Time(r.DoDate));
                }
            });
        }

        public async Task StaffInvitedAsync(long coachId, long userId)
        {
            await SafeAsync("staff invite", async () =>
            {
                var coachName = await _context.Coaches.AsNoTracking().Where(g => g.Id == coachId).Select(g => g.Name).FirstOrDefaultAsync();
                var user = await _context.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => new { u.FirstName, u.Mobile }).FirstOrDefaultAsync();
                if (coachName == null || user == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushCoachStaffInvite, userId, token1: coachName);
            });
        }

        private Task<ReserveInfo> LoadAsync(long reserveId) =>
            _context.CoachReserves.AsNoTracking().Where(r => r.Id == reserveId)
                .Select(r => new ReserveInfo(
                    r.Id, r.CoachId, r.Coach.Name, r.Coach.OwnerId, r.Coach.Owner.Mobile, r.BookerId,
                    r.Booker.FirstName, r.Booker.Mobile, r.UserPet.Name, r.DoDate, r.CoachService.Assistance.Name,
                    r.Mode, r.AssigneeUserId, (r.Booker.FirstName + " " + r.Booker.LastName).Trim()))
                .FirstOrDefaultAsync();

        private async Task SafeAsync(string name, Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Coach notification {Name} failed.", name);
            }
        }

        private static class Rules
        {
            public static string SmsText(string value) => CompanionReserveRescheduleRules.SmsText(value);
            public static string JalaliDate(DateTime date) => CompanionReserveRescheduleRules.JalaliDate(date);
            public static string SmsTime(DateTime date) => CompanionReserveRescheduleRules.SmsTime(date);
            public static string Time(DateTime date) => CompanionReserveRescheduleRules.Time(date);
        }
    }
}
