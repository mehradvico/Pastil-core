using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.ConsultationOnlineSrv;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs
{
    /// <summary>
    /// «همین الان آنلاینم» برای مربی. سامانه‌ی مشاوره‌ی آنلاین (بسته‌ها، خرید، جلسه) روی Companion ساخته شده؛
    /// مربیِ منتقل‌شده از Companion (Coach.LegacyCompanionId) با همان Companion به آن سامانه وصل است. این سرویس فقط
    /// شناسه‌ی مربی را به Companion نگاشت می‌کند و مالکیت مربی را می‌سنجد؛ منطق وضعیت آنلاین همان ConsultationOnlineService است.
    /// مربیِ تازه‌ساخته (بدون Companion قدیمی) فعلاً مشاوره‌ی آنلاین ندارد.
    /// </summary>
    public interface ICoachConsultationOnlineService
    {
        Task<BaseResultDto<ConsultationOnlineStatusDto>> GetMineAsync(long userId, long coachId);
        Task<BaseResultDto<ConsultationOnlineStatusDto>> SetMineAsync(long userId, long coachId, bool online);
    }

    public class CoachConsultationOnlineService : ICoachConsultationOnlineService
    {
        private readonly IDataBaseContext _context;
        private readonly IConsultationOnlineService _online;

        public CoachConsultationOnlineService(IDataBaseContext context, IConsultationOnlineService online)
        {
            _context = context;
            _online = online;
        }

        public async Task<BaseResultDto<ConsultationOnlineStatusDto>> GetMineAsync(long userId, long coachId)
        {
            var (companionId, error) = await ResolveAsync(userId, coachId);
            return error != null
                ? new BaseResultDto<ConsultationOnlineStatusDto>(false, error, null)
                : await _online.GetMineAsync(userId, companionId);
        }

        public async Task<BaseResultDto<ConsultationOnlineStatusDto>> SetMineAsync(long userId, long coachId, bool online)
        {
            var (companionId, error) = await ResolveAsync(userId, coachId);
            return error != null
                ? new BaseResultDto<ConsultationOnlineStatusDto>(false, error, null)
                : await _online.SetMineAsync(userId, companionId, online);
        }

        private async Task<(long? CompanionId, string Error)> ResolveAsync(long userId, long coachId)
        {
            var coach = await _context.Coaches.AsNoTracking()
                .Where(c => c.Id == coachId && !c.Deleted && (c.OwnerId == userId || c.CoachUsers.Any(u => u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true)))
                .Select(c => new { c.LegacyCompanionId })
                .FirstOrDefaultAsync();
            if (coach == null)
                return (null, Resource.Notification.AccessDenied);
            if (!coach.LegacyCompanionId.HasValue)
                return (null, Resource.Notification.CoachConsultationUnavailable);
            return (coach.LegacyCompanionId, null);
        }
    }
}
