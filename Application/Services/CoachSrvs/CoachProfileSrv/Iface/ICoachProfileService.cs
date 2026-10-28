using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachProfileSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachProfileSrv.Iface
{
    /// <summary>
    /// راه‌اندازی مربی: پرسنل، ساعت کاری هفتگی، محدوده‌ی پوشش خدمات در محل و گالری.
    /// ownerUserId = null یعنی ادمین؛ در غیر این صورت فقط مالک همان مربی.
    /// </summary>
    public interface ICoachProfileService
    {
        Task<BaseResultDto<List<CoachStaffVDto>>> ListStaffAsync(long coachId, long? ownerUserId);
        Task<BaseResultDto<CoachStaffVDto>> AddStaffAsync(CoachStaffAddDto dto, long? ownerUserId);
        Task<BaseResultDto> SetStaffActiveAsync(long staffId, bool active, long? ownerUserId);
        Task<BaseResultDto> RemoveStaffAsync(long staffId, long? ownerUserId);
        Task<BaseResultDto<CoachStaffVDto>> SetStaffExpertiseAsync(CoachStaffExpertiseDto dto, long? ownerUserId);

        /// <summary>دعوت‌نامه‌های منتظر پاسخِ کاربر جاری.</summary>
        Task<List<CoachStaffInviteVDto>> ListInvitesAsync(long userId);
        /// <summary>کاربر دعوت را می‌پذیرد/رد می‌کند؛ فقط پرسنل پذیرفته‌شده می‌توانند گرومر یک خدمت یا نوبت باشند.</summary>
        Task<BaseResultDto> RespondInviteAsync(long staffId, long userId, bool accept);

        Task<BaseResultDto<List<CoachTimeVDto>>> GetTimesAsync(long coachId);
        /// <summary>کل ساعت‌های کاری را با لیست جدید جایگزین می‌کند (اعتبارسنجی فرمت، ترتیب و هم‌پوشانی).</summary>
        Task<BaseResultDto<List<CoachTimeVDto>>> SetTimesAsync(long coachId, IReadOnlyList<CoachTimeInputDto> items, long? ownerUserId);

        Task<BaseResultDto<List<CoachZoneVDto>>> GetZonesAsync(long coachId);
        Task<BaseResultDto<List<CoachZoneVDto>>> SetZonesAsync(long coachId, IReadOnlyList<CoachZoneInputDto> items, long? ownerUserId);

        Task<BaseResultDto<List<CoachPictureVDto>>> GetPicturesAsync(long coachId);
        Task<BaseResultDto<CoachPictureVDto>> AddPictureAsync(CoachPictureAddDto dto, long? ownerUserId);
        Task<BaseResultDto> RemovePictureAsync(long coachPictureId, long? ownerUserId);
    }
}
