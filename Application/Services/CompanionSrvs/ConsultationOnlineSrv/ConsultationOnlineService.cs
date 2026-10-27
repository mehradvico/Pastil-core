using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.ConsultationSrvs.ConsultationOwnerSrv;
using Entities.Entities.ClinicField;
using Entities.Entities.CoachField;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.ConsultationOnlineSrv
{
    public class ConsultationOnlineStatusDto
    {
        public bool Online { get; set; }
        public DateTime? ExpiresAt { get; set; }
        // فقط در پاسخ عمومی (نمای کاربر): چند نفر از تیم همین کلینیک/مدرسه الان آنلاین‌اند
        public int OnlineStaffCount { get; set; }
    }

    public interface IConsultationOnlineService
    {
        // وضعیت من (کاربر جاری) در یک مرکز (نمایندگی قدیمی / کلینیک / مربی)؛ owner خالی فقط برای نمایندگی قدیمی: همه‌ی عضویت‌های من
        Task<BaseResultDto<ConsultationOnlineStatusDto>> GetMineAsync(long userId, ConsultationOwner? owner);
        Task<BaseResultDto<ConsultationOnlineStatusDto>> SetMineAsync(long userId, ConsultationOwner? owner, bool online);
        // وضعیت عمومی یک مرکز برای صفحه‌ی خرید مشاوره
        Task<ConsultationOnlineStatusDto> GetPublicAsync(ConsultationOwner owner);
    }

    public class ConsultationOnlineService : IConsultationOnlineService
    {
        private readonly IDataBaseContext _context;

        public ConsultationOnlineService(IDataBaseContext context)
        {
            _context = context;
        }

        private sealed record Flag(bool Online, DateTime? ExpiresAt);

        // owner خالی = همه‌ی نمایندگی‌های قدیمی که کاربر عضو فعالشان است
        private static ConsultationOwnerKind KindOf(ConsultationOwner? owner) => owner?.Kind ?? ConsultationOwnerKind.Companion;

        private async Task<List<Flag>> MyFlagsAsync(long userId, ConsultationOwner? owner)
        {
            switch (KindOf(owner))
            {
                case ConsultationOwnerKind.Clinic:
                {
                    var id = owner.Value.Id;
                    return await _context.ClinicUsers.AsNoTracking()
                        .Where(s => s.UserId == userId && !s.Deleted && s.Active && s.UserAccept == true && s.ClinicId == id)
                        .Select(s => new Flag(s.ConsultationOnline, s.ConsultationOnlineExpiresAt)).ToListAsync();
                }
                case ConsultationOwnerKind.Coach:
                {
                    var id = owner.Value.Id;
                    return await _context.CoachUsers.AsNoTracking()
                        .Where(s => s.UserId == userId && !s.Deleted && s.Active && s.UserAccept == true && s.CoachId == id)
                        .Select(s => new Flag(s.ConsultationOnline, s.ConsultationOnlineExpiresAt)).ToListAsync();
                }
                default:
                {
                    var id = owner?.Id;
                    return await _context.CompanionUsers.AsNoTracking()
                        .Where(s => s.UserId == userId && !s.Deleted && s.Active && s.UserAccept == true && (id == null || s.CompanionId == id.Value))
                        .Select(s => new Flag(s.ConsultationOnline, s.ConsultationOnlineExpiresAt)).ToListAsync();
                }
            }
        }

        private async Task<int> UpdateMyFlagsAsync(long userId, ConsultationOwner? owner, bool online, DateTime? expiresAt)
        {
            switch (KindOf(owner))
            {
                case ConsultationOwnerKind.Clinic:
                {
                    var id = owner.Value.Id;
                    return await _context.ClinicUsers
                        .Where(s => s.UserId == userId && !s.Deleted && s.Active && s.UserAccept == true && s.ClinicId == id)
                        .ExecuteUpdateAsync(x => x.SetProperty(s => s.ConsultationOnline, online).SetProperty(s => s.ConsultationOnlineExpiresAt, expiresAt));
                }
                case ConsultationOwnerKind.Coach:
                {
                    var id = owner.Value.Id;
                    return await _context.CoachUsers
                        .Where(s => s.UserId == userId && !s.Deleted && s.Active && s.UserAccept == true && s.CoachId == id)
                        .ExecuteUpdateAsync(x => x.SetProperty(s => s.ConsultationOnline, online).SetProperty(s => s.ConsultationOnlineExpiresAt, expiresAt));
                }
                default:
                {
                    var id = owner?.Id;
                    return await _context.CompanionUsers
                        .Where(s => s.UserId == userId && !s.Deleted && s.Active && s.UserAccept == true && (id == null || s.CompanionId == id.Value))
                        .ExecuteUpdateAsync(x => x.SetProperty(s => s.ConsultationOnline, online).SetProperty(s => s.ConsultationOnlineExpiresAt, expiresAt));
                }
            }
        }

        // ردیفی (فعال یا غیرفعال) برای کاربر در جدول عضویت مرکز هست؟ تا ردیف عمداً غیرفعال‌شده را دوباره نسازیم
        private Task<bool> HasAnyMembershipRowAsync(long userId, ConsultationOwner owner)
        {
            var id = owner.Id;
            return owner.Kind switch
            {
                ConsultationOwnerKind.Clinic => _context.ClinicUsers.AnyAsync(s => s.UserId == userId && s.ClinicId == id && !s.Deleted),
                ConsultationOwnerKind.Coach => _context.CoachUsers.AnyAsync(s => s.UserId == userId && s.CoachId == id && !s.Deleted),
                _ => _context.CompanionUsers.AnyAsync(s => s.UserId == userId && s.CompanionId == id && !s.Deleted),
            };
        }

        private async Task AddSelfMembershipAsync(long userId, ConsultationOwner owner, bool online, DateTime? expiresAt)
        {
            switch (owner.Kind)
            {
                case ConsultationOwnerKind.Clinic:
                    await _context.ClinicUsers.AddAsync(new ClinicUser { ClinicId = owner.Id, UserId = userId, Active = true, UserAccept = true, ConsultationOnline = online, ConsultationOnlineExpiresAt = expiresAt });
                    break;
                case ConsultationOwnerKind.Coach:
                    await _context.CoachUsers.AddAsync(new CoachUser { CoachId = owner.Id, UserId = userId, Active = true, UserAccept = true, ConsultationOnline = online, ConsultationOnlineExpiresAt = expiresAt });
                    break;
                default:
                    await _context.CompanionUsers.AddAsync(new CompanionUser { CompanionId = owner.Id, UserId = userId, Active = true, UserAccept = true, ConsultationOnline = online, ConsultationOnlineExpiresAt = expiresAt });
                    break;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<BaseResultDto<ConsultationOnlineStatusDto>> GetMineAsync(long userId, ConsultationOwner? owner)
        {
            try
            {
                var now = DateTime.Now;
                var rows = await MyFlagsAsync(userId, owner);

                if (rows.Count == 0)
                {
                    // مالک بدون عضویتِ صریح (مرکز تازه‌کار بدون همکار): پیش‌فرض آفلاین، نه خطا
                    if (owner.HasValue && await ConsultationOwners.IsOwnerAsync(_context, userId, owner.Value))
                        return new BaseResultDto<ConsultationOnlineStatusDto>(true, new ConsultationOnlineStatusDto { Online = false, ExpiresAt = null });

                    return new BaseResultDto<ConsultationOnlineStatusDto>(false, Resource.Notification.AccessDenied, null);
                }

                var online = rows.Any(r => ConsultationOnlineRules.IsOnline(r.Online, r.ExpiresAt, now));
                var expiresAt = rows.Where(r => ConsultationOnlineRules.IsOnline(r.Online, r.ExpiresAt, now))
                    .Select(r => r.ExpiresAt).Max();
                return new BaseResultDto<ConsultationOnlineStatusDto>(true, new ConsultationOnlineStatusDto { Online = online, ExpiresAt = expiresAt });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationOnlineStatusDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationOnlineStatusDto>> SetMineAsync(long userId, ConsultationOwner? owner, bool online)
        {
            try
            {
                var now = DateTime.Now;
                var expiresAt = online ? ConsultationOnlineRules.ExpiresAt(now) : (DateTime?)null;

                var affected = await UpdateMyFlagsAsync(userId, owner, online, expiresAt);

                if (affected == 0)
                {
                    // مالک بدون عضویتِ صریح: بدون تغییر schema، یک ردیفِ خودمالکی در جدول عضویت می‌سازیم
                    // تا این و دفعات بعد از همان مسیر معمولی آپدیت شود.
                    if (!owner.HasValue || !await ConsultationOwners.IsOwnerAsync(_context, userId, owner.Value))
                        return new BaseResultDto<ConsultationOnlineStatusDto>(false, Resource.Notification.AccessDenied, null);

                    if (await HasAnyMembershipRowAsync(userId, owner.Value))
                        // ردیفی هست ولی Active/UserAccept نیست؛ عمداً غیرفعال شده - دخالت نکن
                        return new BaseResultDto<ConsultationOnlineStatusDto>(false, Resource.Notification.AccessDenied, null);

                    await AddSelfMembershipAsync(userId, owner.Value, online, expiresAt);
                }

                return new BaseResultDto<ConsultationOnlineStatusDto>(true, new ConsultationOnlineStatusDto { Online = online, ExpiresAt = expiresAt });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationOnlineStatusDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<ConsultationOnlineStatusDto> GetPublicAsync(ConsultationOwner owner)
        {
            owner = await ConsultationOwners.ResolvePublicAsync(_context, owner);
            var now = DateTime.Now;
            var id = owner.Id;
            List<Flag> rows;
            switch (owner.Kind)
            {
                case ConsultationOwnerKind.Clinic:
                    rows = await _context.ClinicUsers.AsNoTracking()
                        .Where(s => s.ClinicId == id && !s.Deleted && s.Active && s.UserAccept == true)
                        .Select(s => new Flag(s.ConsultationOnline, s.ConsultationOnlineExpiresAt)).ToListAsync();
                    break;
                case ConsultationOwnerKind.Coach:
                    rows = await _context.CoachUsers.AsNoTracking()
                        .Where(s => s.CoachId == id && !s.Deleted && s.Active && s.UserAccept == true)
                        .Select(s => new Flag(s.ConsultationOnline, s.ConsultationOnlineExpiresAt)).ToListAsync();
                    break;
                default:
                    rows = await _context.CompanionUsers.AsNoTracking()
                        .Where(s => s.CompanionId == id && !s.Deleted && s.Active && s.UserAccept == true)
                        .Select(s => new Flag(s.ConsultationOnline, s.ConsultationOnlineExpiresAt)).ToListAsync();
                    break;
            }
            var onlineCount = rows.Count(r => ConsultationOnlineRules.IsOnline(r.Online, r.ExpiresAt, now));
            return new ConsultationOnlineStatusDto { Online = onlineCount > 0, OnlineStaffCount = onlineCount };
        }
    }
}
