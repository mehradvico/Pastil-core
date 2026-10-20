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

namespace Application.Services.ClinicSrvs
{
    /// <summary>
    /// همه‌ی پوش و پیامک‌های کلینیک یک‌جا. هیچ متدی خطا پرت نمی‌کند (شکست اعلان نباید رزرو/پرداخت را برگرداند).
    /// پیامک بدون ردیف SmsSetting بی‌صدا ارسال نمی‌شود؛ ساخت قالب‌ها: scripts/SeedClinicSms.sql
    /// </summary>
    public interface IClinicNotifier
    {
        Task ClinicDecidedAsync(long clinicId, bool approved, string reason);
        Task ReservePaidAsync(long reserveId);
        Task ReserveApprovedAsync(long reserveId);
        Task ReserveRejectedAsync(long reserveId, string reason);
        Task ReserveExpiredAsync(long reserveId);
        Task ReserveCancelledByBookerAsync(long reserveId);
        Task ReserveCancelledByAdminAsync(long reserveId, string reason);
        Task ReserveRescheduledAsync(long reserveId, bool byOwner, DateTime oldDoDate);
        Task StaffInvitedAsync(long clinicId, long userId);
    }

    public class ClinicNotifier : IClinicNotifier
    {
        private readonly IDataBaseContext _context;
        private readonly IPushNotificationService _push;
        private readonly IMessageSenderService _sms;
        private readonly ILogger<ClinicNotifier> _logger;

        public ClinicNotifier(IDataBaseContext context, IPushNotificationService push, IMessageSenderService sms, ILogger<ClinicNotifier> logger)
        {
            _context = context;
            _push = push;
            _sms = sms;
            _logger = logger;
        }

        private sealed record ReserveInfo(
            long Id, long ClinicId, string ClinicName, long OwnerId, string OwnerMobile, long BookerId, string BookerFirstName,
            string BookerMobile, string PetName, DateTime DoDate, string ServiceName);

        public async Task ClinicDecidedAsync(long clinicId, bool approved, string reason)
        {
            await SafeAsync("clinic decision", async () =>
            {
                var clinic = await _context.Clinics.AsNoTracking().Where(g => g.Id == clinicId)
                    .Select(g => new { g.Name, g.OwnerId, g.Owner.Mobile }).FirstOrDefaultAsync();
                if (clinic == null)
                    return;
                var name = Rules.SmsText(clinic.Name);
                await _push.SendPushAsync(approved ? PushTypeEnum.PushClinicApproved : PushTypeEnum.PushClinicRejected,
                    clinic.OwnerId, token1: clinic.Name, token2: reason);
            });
        }

        public async Task ReservePaidAsync(long reserveId)
        {
            await SafeAsync("reserve paid", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushClinicReserveApprovalRequired, r.OwnerId,
                    token1: r.ClinicName, token2: r.PetName, token3: r.Id.ToString());
                // پیامک: همان قالب‌های رزرو Companion (قالب جدید نداریم).
                await _sms.SendMessageAsync(MessageTypeEnum.CompanionReserveForUser, r.BookerMobile, null,
                    token1: r.ServiceName);
                await _sms.SendMessageAsync(MessageTypeEnum.CompanionReserveForCompanion, r.OwnerMobile, null,
                    token1: r.ServiceName, token2: r.BookerFirstName);
            });
        }

        public async Task ReserveApprovedAsync(long reserveId)
        {
            await SafeAsync("reserve approved", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushClinicReserveApproved, r.BookerId,
                    token1: r.ClinicName, token2: r.PetName, token3: r.Id.ToString());
            });
        }

        public async Task ReserveRejectedAsync(long reserveId, string reason)
        {
            await SafeAsync("reserve rejected", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushClinicReserveRejected, r.BookerId,
                    token1: r.ClinicName, token2: reason, token3: r.Id.ToString());
            });
        }

        public async Task ReserveExpiredAsync(long reserveId)
        {
            await SafeAsync("reserve expired", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushClinicReserveExpired, r.BookerId, token1: r.ClinicName, token3: r.Id.ToString());
            });
        }

        public async Task ReserveCancelledByBookerAsync(long reserveId)
        {
            await SafeAsync("reserve cancelled by booker", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushClinicReserveCancelledByBooker, r.OwnerId,
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
                await _push.SendPushAsync(PushTypeEnum.PushClinicReserveCancelledByAdmin, r.BookerId,
                    token1: r.ClinicName, token2: reason, token3: r.Id.ToString());
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
                await _push.SendPushAsync(PushTypeEnum.PushClinicReserveRescheduled, r.BookerId,
                    token1: r.ClinicName, token2: date, token3: Rules.Time(r.DoDate));
                // پیامک: همان قالب تغییر زمان Companion (مرکز، تاریخ قبلی، تاریخ جدید، ساعت جدید، علت).
                await _sms.SendMessageAsync(MessageTypeEnum.CompanionReserveRescheduledUser, r.BookerMobile, null,
                    token1: Rules.SmsText(r.ClinicName), token2: Rules.JalaliDate(oldDoDate), token3: date,
                    token4: Rules.SmsTime(r.DoDate),
                    token5: Rules.SmsText(byOwner ? Resource.Notification.ClinicRescheduleReasonByOwner : Resource.Notification.ClinicRescheduleReasonBySupport));
                // وقتی خود کلینیک زمان را عوض کرده، به خودش خبر نمی‌دهیم؛ فقط اگر ادمین عوض کرده.
                if (!byOwner)
                {
                    await _push.SendPushAsync(PushTypeEnum.PushClinicReserveRescheduledForClinic, r.OwnerId,
                        token1: r.PetName, token2: date, token3: Rules.Time(r.DoDate));
                }
            });
        }

        public async Task StaffInvitedAsync(long clinicId, long userId)
        {
            await SafeAsync("staff invite", async () =>
            {
                var clinicName = await _context.Clinics.AsNoTracking().Where(g => g.Id == clinicId).Select(g => g.Name).FirstOrDefaultAsync();
                var user = await _context.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => new { u.FirstName, u.Mobile }).FirstOrDefaultAsync();
                if (clinicName == null || user == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushClinicStaffInvite, userId, token1: clinicName);
            });
        }

        private Task<ReserveInfo> LoadAsync(long reserveId) =>
            _context.ClinicReserves.AsNoTracking().Where(r => r.Id == reserveId)
                .Select(r => new ReserveInfo(
                    r.Id, r.ClinicId, r.Clinic.Name, r.Clinic.OwnerId, r.Clinic.Owner.Mobile, r.BookerId,
                    r.Booker.FirstName, r.Booker.Mobile, r.UserPet.Name, r.DoDate, r.ClinicService.Assistance.Name))
                .FirstOrDefaultAsync();

        private async Task SafeAsync(string name, Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Clinic notification {Name} failed.", name);
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
