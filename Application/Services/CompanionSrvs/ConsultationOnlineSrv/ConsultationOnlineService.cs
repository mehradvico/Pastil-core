using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
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
        // وضعیت من (کاربر جاری) در یک کلینیک/مدرسه؛ اگر عضو فعال چند کلینیک باشد companionId الزامی است
        Task<BaseResultDto<ConsultationOnlineStatusDto>> GetMineAsync(long userId, long? companionId);
        Task<BaseResultDto<ConsultationOnlineStatusDto>> SetMineAsync(long userId, long? companionId, bool online);
        // وضعیت عمومی یک کلینیک/مدرسه برای صفحه‌ی خرید مشاوره
        Task<ConsultationOnlineStatusDto> GetPublicAsync(long companionId);
    }

    public class ConsultationOnlineService : IConsultationOnlineService
    {
        private readonly IDataBaseContext _context;

        public ConsultationOnlineService(IDataBaseContext context)
        {
            _context = context;
        }

        private IQueryable<Entities.Entities.CompanionField.CompanionUser> MyMemberships(long userId, long? companionId) =>
            _context.CompanionUsers.Where(s =>
                s.UserId == userId && !s.Deleted && s.Active && s.UserAccept == true &&
                (companionId == null || s.CompanionId == companionId.Value));

        // مالک کلینیک/مدرسه («مربی»)؛ اگر عضویتی در CompanionUsers نداشته باشد هم باید بتواند روشن/خاموش کند
        private Task<bool> IsOwnerAsync(long userId, long companionId) =>
            _context.Companions.AsNoTracking().AnyAsync(s => s.Id == companionId && s.OwnerId == userId && !s.Deleted);

        public async Task<BaseResultDto<ConsultationOnlineStatusDto>> GetMineAsync(long userId, long? companionId)
        {
            try
            {
                var now = DateTime.Now;
                var rows = await MyMemberships(userId, companionId).AsNoTracking()
                    .Select(s => new { s.ConsultationOnline, s.ConsultationOnlineExpiresAt })
                    .ToListAsync();

                if (rows.Count == 0)
                {
                    // مالک بدون عضویتِ صریح در CompanionUsers («مربی» تازه‌کار بدون همکار): پیش‌فرض آفلاین، نه خطا
                    if (companionId.HasValue && await IsOwnerAsync(userId, companionId.Value))
                        return new BaseResultDto<ConsultationOnlineStatusDto>(true, new ConsultationOnlineStatusDto { Online = false, ExpiresAt = null });

                    return new BaseResultDto<ConsultationOnlineStatusDto>(false, Resource.Notification.AccessDenied, null);
                }

                var online = rows.Any(r => ConsultationOnlineRules.IsOnline(r.ConsultationOnline, r.ConsultationOnlineExpiresAt, now));
                var expiresAt = rows.Where(r => ConsultationOnlineRules.IsOnline(r.ConsultationOnline, r.ConsultationOnlineExpiresAt, now))
                    .Select(r => r.ConsultationOnlineExpiresAt).Max();
                return new BaseResultDto<ConsultationOnlineStatusDto>(true, new ConsultationOnlineStatusDto { Online = online, ExpiresAt = expiresAt });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationOnlineStatusDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationOnlineStatusDto>> SetMineAsync(long userId, long? companionId, bool online)
        {
            try
            {
                var now = DateTime.Now;
                var expiresAt = online ? ConsultationOnlineRules.ExpiresAt(now) : (DateTime?)null;

                var affected = await MyMemberships(userId, companionId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.ConsultationOnline, online)
                        .SetProperty(s => s.ConsultationOnlineExpiresAt, expiresAt));

                if (affected == 0)
                {
                    // مالک بدون عضویتِ صریح («مربی» مدرسه معمولاً همین حالت است): بدون تغییر schema، یک ردیفِ
                    // خودمالکی در CompanionUsers می‌سازیم تا این و دفعات بعد از همان مسیر معمولی آپدیت شود.
                    if (!companionId.HasValue || !await IsOwnerAsync(userId, companionId.Value))
                        return new BaseResultDto<ConsultationOnlineStatusDto>(false, Resource.Notification.AccessDenied, null);

                    var hasInactiveRow = await _context.CompanionUsers.AnyAsync(s =>
                        s.UserId == userId && s.CompanionId == companionId.Value && !s.Deleted);
                    if (hasInactiveRow)
                        // ردیفی هست ولی Active/UserAccept نیست؛ عمداً غیرفعال شده - دخالت نکن
                        return new BaseResultDto<ConsultationOnlineStatusDto>(false, Resource.Notification.AccessDenied, null);

                    await _context.CompanionUsers.AddAsync(new Entities.Entities.CompanionField.CompanionUser
                    {
                        CompanionId = companionId.Value,
                        UserId = userId,
                        Active = true,
                        UserAccept = true,
                        ConsultationOnline = online,
                        ConsultationOnlineExpiresAt = expiresAt
                    });
                    await _context.SaveChangesAsync();
                }

                return new BaseResultDto<ConsultationOnlineStatusDto>(true, new ConsultationOnlineStatusDto { Online = online, ExpiresAt = expiresAt });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationOnlineStatusDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<ConsultationOnlineStatusDto> GetPublicAsync(long companionId)
        {
            var now = DateTime.Now;
            var rows = await _context.CompanionUsers.AsNoTracking()
                .Where(s => s.CompanionId == companionId && !s.Deleted && s.Active && s.UserAccept == true)
                .Select(s => new { s.ConsultationOnline, s.ConsultationOnlineExpiresAt })
                .ToListAsync();
            var onlineCount = rows.Count(r => ConsultationOnlineRules.IsOnline(r.ConsultationOnline, r.ConsultationOnlineExpiresAt, now));
            return new ConsultationOnlineStatusDto { Online = onlineCount > 0, OnlineStaffCount = onlineCount };
        }
    }
}
