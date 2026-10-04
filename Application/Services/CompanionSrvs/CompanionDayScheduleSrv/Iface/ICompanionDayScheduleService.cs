using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionDayScheduleSrv.Dto;
using System;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionDayScheduleSrv.Iface
{
    public interface ICompanionDayScheduleService
    {
        // برنامه‌ی یک روز نماینده/کلینیک: رزروهای خدمت + مشاوره‌های ساعت‌دار، به ترتیب ساعت، با مشتری و پت.
        // companionId خالی ⇒ کلینیک خود کاربر (مالک) یا تنها کلینیکی که عضو آن است. مالک: همه‌ی کلینیک؛ عضو تیم: فقط رزروهای تخصیص‌یافته به خودش (مشاوره‌ی بی‌نماینده برای همه‌ی اعضا دیده می‌شود).
        Task<BaseResultDto<CompanionDayScheduleVDto>> GetAsync(long userId, long? companionId, DateTime date, bool includeCancelled, bool mineOnly);
    }
}
