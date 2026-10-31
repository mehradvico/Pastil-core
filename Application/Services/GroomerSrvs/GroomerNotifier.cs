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

namespace Application.Services.GroomerSrvs
{
    /// <summary>
    /// همه‌ی پوش و پیامک‌های آرایشگاه یک‌جا. هیچ متدی خطا پرت نمی‌کند (شکست اعلان نباید رزرو/پرداخت را برگرداند).
    /// پیامک بدون ردیف SmsSetting بی‌صدا ارسال نمی‌شود؛ ساخت قالب‌ها: scripts/SeedGroomerSms.sql
    /// </summary>
    public interface IGroomerNotifier
    {
        Task GroomerDecidedAsync(long groomerId, bool approved, string reason);
        Task ReservePaidAsync(long reserveId);
        Task ReserveApprovedAsync(long reserveId);
        Task ReserveRejectedAsync(long reserveId, string reason);
        Task ReserveExpiredAsync(long reserveId);
        Task ReserveCancelledByBookerAsync(long reserveId);
        Task ReserveCancelledByAdminAsync(long reserveId, string reason);
        Task ReserveRescheduledAsync(long reserveId, bool byOwner);
        Task StaffInvitedAsync(long groomerId, long userId);
        /// <summary>به پرسنلی که مالک رزرو را به او سپرد (همان قالب «رزرو به شما اختصاص داده شد»، مسیر /operator). Token1 = نام خدمت، Token2 = نام مشتری.</summary>
        Task ReserveAssignedAsync(long reserveId);
    }

    public class GroomerNotifier : IGroomerNotifier
    {
        private readonly IDataBaseContext _context;
        private readonly IPushNotificationService _push;
        private readonly IMessageSenderService _sms;
        private readonly ILogger<GroomerNotifier> _logger;

        public GroomerNotifier(IDataBaseContext context, IPushNotificationService push, IMessageSenderService sms, ILogger<GroomerNotifier> logger)
        {
            _context = context;
            _push = push;
            _sms = sms;
            _logger = logger;
        }

        private sealed record ReserveInfo(
            long Id, long GroomerId, string GroomerName, long OwnerId, string OwnerMobile, long BookerId, string BookerFirstName,
            string BookerMobile, string PetName, DateTime DoDate);

        public async Task GroomerDecidedAsync(long groomerId, bool approved, string reason)
        {
            await SafeAsync("groomer decision", async () =>
            {
                var groomer = await _context.Groomers.AsNoTracking().Where(g => g.Id == groomerId)
                    .Select(g => new { g.Name, g.OwnerId, g.Owner.Mobile }).FirstOrDefaultAsync();
                if (groomer == null)
                    return;
                var name = Rules.SmsText(groomer.Name);
                await _push.SendPushAsync(approved ? PushTypeEnum.PushGroomerApproved : PushTypeEnum.PushGroomerRejected,
                    groomer.OwnerId, token1: groomer.Name, token2: reason);
                await _sms.SendMessageAsync(approved ? MessageTypeEnum.GroomerApprovedForOwner : MessageTypeEnum.GroomerRejectedForOwner,
                    groomer.Mobile, null, token1: name, token2: approved ? null : Rules.SmsText(reason));
            });
        }

        public async Task ReservePaidAsync(long reserveId)
        {
            await SafeAsync("reserve paid", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushGroomerReserveApprovalRequired, r.OwnerId,
                    token1: r.GroomerName, token2: r.PetName, token3: r.Id.ToString());
                await _sms.SendMessageAsync(MessageTypeEnum.GroomerReserveForUser, r.BookerMobile, null,
                    token1: Rules.SmsText(r.BookerFirstName), token2: Rules.SmsText(r.GroomerName),
                    token3: Rules.JalaliDate(r.DoDate), token4: Rules.SmsTime(r.DoDate));
                await _sms.SendMessageAsync(MessageTypeEnum.GroomerReserveForGroomer, r.OwnerMobile, null,
                    token1: Rules.SmsText(r.GroomerName), token2: Rules.SmsText(r.PetName),
                    token3: Rules.JalaliDate(r.DoDate), token4: Rules.SmsTime(r.DoDate));
            });
        }

        public async Task ReserveApprovedAsync(long reserveId)
        {
            await SafeAsync("reserve approved", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushGroomerReserveApproved, r.BookerId,
                    token1: r.GroomerName, token2: r.PetName, token3: r.Id.ToString());
                await _sms.SendMessageAsync(MessageTypeEnum.GroomerReserveApprovedForUser, r.BookerMobile, null,
                    token1: Rules.SmsText(r.BookerFirstName), token2: Rules.SmsText(r.GroomerName),
                    token3: Rules.JalaliDate(r.DoDate), token4: Rules.SmsTime(r.DoDate));
            });
        }

        public async Task ReserveRejectedAsync(long reserveId, string reason)
        {
            await SafeAsync("reserve rejected", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushGroomerReserveRejected, r.BookerId,
                    token1: r.GroomerName, token2: reason, token3: r.Id.ToString());
                await _sms.SendMessageAsync(MessageTypeEnum.GroomerReserveRejectedForUser, r.BookerMobile, null,
                    token1: Rules.SmsText(r.BookerFirstName), token2: Rules.SmsText(r.GroomerName), token3: Rules.SmsText(reason));
            });
        }

        public async Task ReserveExpiredAsync(long reserveId)
        {
            await SafeAsync("reserve expired", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushGroomerReserveExpired, r.BookerId, token1: r.GroomerName, token3: r.Id.ToString());
                await _sms.SendMessageAsync(MessageTypeEnum.GroomerReserveExpiredForUser, r.BookerMobile, null,
                    token1: Rules.SmsText(r.BookerFirstName), token2: Rules.SmsText(r.GroomerName));
            });
        }

        public async Task ReserveCancelledByBookerAsync(long reserveId)
        {
            await SafeAsync("reserve cancelled by booker", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushGroomerReserveCancelledByBooker, r.OwnerId,
                    token1: r.PetName, token2: r.DoDate.ToString("yyyy/MM/dd HH:mm"), token3: r.Id.ToString());
                await _sms.SendMessageAsync(MessageTypeEnum.GroomerReserveCancelledForGroomer, r.OwnerMobile, null,
                    token1: Rules.SmsText(r.PetName), token2: Rules.JalaliDate(r.DoDate), token3: Rules.SmsTime(r.DoDate));
            });
        }

        public async Task ReserveCancelledByAdminAsync(long reserveId, string reason)
        {
            await SafeAsync("reserve cancelled by admin", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushGroomerReserveCancelledByAdmin, r.BookerId,
                    token1: r.GroomerName, token2: reason, token3: r.Id.ToString());
                await _sms.SendMessageAsync(MessageTypeEnum.GroomerReserveCancelledByAdminForUser, r.BookerMobile, null,
                    token1: Rules.SmsText(r.BookerFirstName), token2: Rules.SmsText(r.GroomerName), token3: Rules.SmsText(reason));
            });
        }

        public async Task ReserveRescheduledAsync(long reserveId, bool byOwner)
        {
            await SafeAsync("reserve rescheduled", async () =>
            {
                var r = await LoadAsync(reserveId);
                if (r == null)
                    return;
                var date = Rules.JalaliDate(r.DoDate);
                await _push.SendPushAsync(PushTypeEnum.PushGroomerReserveRescheduled, r.BookerId,
                    token1: r.GroomerName, token2: date, token3: Rules.Time(r.DoDate));
                await _sms.SendMessageAsync(MessageTypeEnum.GroomerReserveRescheduledForUser, r.BookerMobile, null,
                    token1: Rules.SmsText(r.BookerFirstName), token2: Rules.SmsText(r.GroomerName), token3: date, token4: Rules.SmsTime(r.DoDate));
                // وقتی خود آرایشگاه زمان را عوض کرده، به خودش خبر نمی‌دهیم؛ فقط اگر ادمین عوض کرده.
                if (!byOwner)
                {
                    await _push.SendPushAsync(PushTypeEnum.PushGroomerReserveRescheduledForGroomer, r.OwnerId,
                        token1: r.PetName, token2: date, token3: Rules.Time(r.DoDate));
                    await _sms.SendMessageAsync(MessageTypeEnum.GroomerReserveRescheduledForGroomer, r.OwnerMobile, null,
                        token1: Rules.SmsText(r.PetName), token2: date, token3: Rules.SmsTime(r.DoDate));
                }
            });
        }

        public async Task StaffInvitedAsync(long groomerId, long userId)
        {
            await SafeAsync("staff invite", async () =>
            {
                var groomerName = await _context.Groomers.AsNoTracking().Where(g => g.Id == groomerId).Select(g => g.Name).FirstOrDefaultAsync();
                var user = await _context.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => new { u.FirstName, u.Mobile }).FirstOrDefaultAsync();
                if (groomerName == null || user == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushGroomerStaffInvite, userId, token1: groomerName);
                await _sms.SendMessageAsync(MessageTypeEnum.GroomerStaffInvite, user.Mobile, null,
                    token1: Rules.SmsText(user.FirstName), token2: Rules.SmsText(groomerName));
            });
        }

        public async Task ReserveAssignedAsync(long reserveId)
        {
            await SafeAsync(nameof(ReserveAssignedAsync), async () =>
            {
                var r = await _context.GroomerReserves.AsNoTracking().Where(x => x.Id == reserveId)
                    .Select(x => new { x.AssigneeUserId, ServiceName = x.GroomerService.Assistance.Name, BookerName = (x.Booker.FirstName + " " + x.Booker.LastName).Trim() })
                    .FirstOrDefaultAsync();
                if (r?.AssigneeUserId == null)
                    return;
                await _push.SendPushAsync(PushTypeEnum.PushCompanionReserveAssigned, r.AssigneeUserId.Value,
                    token1: r.ServiceName, token2: r.BookerName);
            });
        }

        private Task<ReserveInfo> LoadAsync(long reserveId) =>
            _context.GroomerReserves.AsNoTracking().Where(r => r.Id == reserveId)
                .Select(r => new ReserveInfo(
                    r.Id, r.GroomerId, r.Groomer.Name, r.Groomer.OwnerId, r.Groomer.Owner.Mobile, r.BookerId,
                    r.Booker.FirstName, r.Booker.Mobile, r.UserPet.Name, r.DoDate))
                .FirstOrDefaultAsync();

        private async Task SafeAsync(string name, Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groomer notification {Name} failed.", name);
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
