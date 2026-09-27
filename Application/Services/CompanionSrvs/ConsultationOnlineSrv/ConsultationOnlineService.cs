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
        // فقط در پاسخ عمومی (نمای کاربر): چند نفر از تیم همین کلینیک الان آنلاین‌اند
        public int OnlineStaffCount { get; set; }
    }

    public interface IConsultationOnlineService
    {
        // وضعیت من (کاربر جاری) در یک کلینیک؛ اگر عضو فعال چند کلینیک باشد companionId الزامی است
        Task<BaseResultDto<ConsultationOnlineStatusDto>> GetMineAsync(long userId, long? companionId);
        Task<BaseResultDto<ConsultationOnlineStatusDto>> SetMineAsync(long userId, long? companionId, bool online);
        // وضعیت عمومی یک کلینیک برای صفحه‌ی خرید مشاوره
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

        public async Task<BaseResultDto<ConsultationOnlineStatusDto>> GetMineAsync(long userId, long? companionId)
        {
            try
            {
                var now = DateTime.Now;
                var rows = await MyMemberships(userId, companionId).AsNoTracking()
                    .Select(s => new { s.ConsultationOnline, s.ConsultationOnlineExpiresAt })
                    .ToListAsync();
                if (rows.Count == 0)
                    return new BaseResultDto<ConsultationOnlineStatusDto>(false, Resource.Notification.AccessDenied, null);

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
                    return new BaseResultDto<ConsultationOnlineStatusDto>(false, Resource.Notification.AccessDenied, null);

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
