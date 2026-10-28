using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicProfileSrv.Iface
{
    /// <summary>
    /// راه‌اندازی کلینیک: پرسنل، ساعت کاری هفتگی، محدوده‌ی پوشش خدمات در محل و گالری.
    /// ownerUserId = null یعنی ادمین؛ در غیر این صورت فقط مالک همان کلینیک.
    /// </summary>
    public interface IClinicProfileService
    {
        Task<BaseResultDto<List<ClinicStaffVDto>>> ListStaffAsync(long clinicId, long? ownerUserId);
        Task<BaseResultDto<ClinicStaffVDto>> AddStaffAsync(ClinicStaffAddDto dto, long? ownerUserId);
        Task<BaseResultDto> SetStaffActiveAsync(long staffId, bool active, long? ownerUserId);
        Task<BaseResultDto> RemoveStaffAsync(long staffId, long? ownerUserId);
        Task<BaseResultDto<ClinicStaffVDto>> SetStaffExpertiseAsync(ClinicStaffExpertiseDto dto, long? ownerUserId);

        /// <summary>دعوت‌نامه‌های منتظر پاسخِ کاربر جاری.</summary>
        Task<List<ClinicStaffInviteVDto>> ListInvitesAsync(long userId);
        /// <summary>کاربر دعوت را می‌پذیرد/رد می‌کند؛ فقط پرسنل پذیرفته‌شده می‌توانند گرومر یک خدمت یا نوبت باشند.</summary>
        Task<BaseResultDto> RespondInviteAsync(long staffId, long userId, bool accept);

        Task<BaseResultDto<List<ClinicTimeVDto>>> GetTimesAsync(long clinicId);
        /// <summary>کل ساعت‌های کاری را با لیست جدید جایگزین می‌کند (اعتبارسنجی فرمت، ترتیب و هم‌پوشانی).</summary>
        Task<BaseResultDto<List<ClinicTimeVDto>>> SetTimesAsync(long clinicId, IReadOnlyList<ClinicTimeInputDto> items, long? ownerUserId);

        Task<BaseResultDto<List<ClinicZoneVDto>>> GetZonesAsync(long clinicId);
        Task<BaseResultDto<List<ClinicZoneVDto>>> SetZonesAsync(long clinicId, IReadOnlyList<ClinicZoneInputDto> items, long? ownerUserId);

        Task<BaseResultDto<List<ClinicPictureVDto>>> GetPicturesAsync(long clinicId);
        Task<BaseResultDto<ClinicPictureVDto>> AddPictureAsync(ClinicPictureAddDto dto, long? ownerUserId);
        Task<BaseResultDto> RemovePictureAsync(long clinicPictureId, long? ownerUserId);
    }
}
