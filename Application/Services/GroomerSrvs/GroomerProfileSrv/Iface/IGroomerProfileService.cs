using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.GroomerSrvs.GroomerProfileSrv.Iface
{
    /// <summary>
    /// راه‌اندازی آرایشگاه: پرسنل، ساعت کاری هفتگی، محدوده‌ی پوشش خدمات در محل و گالری.
    /// ownerUserId = null یعنی ادمین؛ در غیر این صورت فقط مالک همان آرایشگاه.
    /// </summary>
    public interface IGroomerProfileService
    {
        Task<BaseResultDto<List<GroomerStaffVDto>>> ListStaffAsync(long groomerId, long? ownerUserId);
        Task<BaseResultDto<GroomerStaffVDto>> AddStaffAsync(GroomerStaffAddDto dto, long? ownerUserId);
        Task<BaseResultDto> SetStaffActiveAsync(long staffId, bool active, long? ownerUserId);
        Task<BaseResultDto> RemoveStaffAsync(long staffId, long? ownerUserId);

        /// <summary>دعوت‌نامه‌های منتظر پاسخِ کاربر جاری.</summary>
        Task<List<GroomerStaffInviteVDto>> ListInvitesAsync(long userId);
        /// <summary>کاربر دعوت را می‌پذیرد/رد می‌کند؛ فقط پرسنل پذیرفته‌شده می‌توانند گرومر یک خدمت یا نوبت باشند.</summary>
        Task<BaseResultDto> RespondInviteAsync(long staffId, long userId, bool accept);

        Task<BaseResultDto<List<GroomerTimeVDto>>> GetTimesAsync(long groomerId);
        /// <summary>کل ساعت‌های کاری را با لیست جدید جایگزین می‌کند (اعتبارسنجی فرمت، ترتیب و هم‌پوشانی).</summary>
        Task<BaseResultDto<List<GroomerTimeVDto>>> SetTimesAsync(long groomerId, IReadOnlyList<GroomerTimeInputDto> items, long? ownerUserId);

        Task<BaseResultDto<List<GroomerZoneVDto>>> GetZonesAsync(long groomerId);
        Task<BaseResultDto<List<GroomerZoneVDto>>> SetZonesAsync(long groomerId, IReadOnlyList<GroomerZoneInputDto> items, long? ownerUserId);

        Task<BaseResultDto<List<GroomerPictureVDto>>> GetPicturesAsync(long groomerId);
        Task<BaseResultDto<GroomerPictureVDto>> AddPictureAsync(GroomerPictureAddDto dto, long? ownerUserId);
        Task<BaseResultDto> RemovePictureAsync(long groomerPictureId, long? ownerUserId);
    }
}
