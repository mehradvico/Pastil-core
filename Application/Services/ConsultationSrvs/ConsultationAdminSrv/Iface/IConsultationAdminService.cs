using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationAdminSrv.Iface
{
    // ادمین: خریدها فقط‌خواندنی‌اند؛ بسته‌های هر کلینیک را می‌تواند تعریف/ویرایش/فعال‌وغیرفعال کند (همان ماتریس ۸ خانه‌ای نماینده)
    public interface IConsultationAdminService
    {
        Task<BaseResultDto<ConsultationPurchaseAdminSearchDto>> SearchPurchasesAsync(ConsultationPurchaseAdminInputDto dto);
        Task<BaseResultDto<ConsultationPurchaseAdminVDto>> GetPurchaseAsync(long id);
        // همه‌ی کلینیک‌های فعال (با q برای جستجو)؛ کلینیک بدون بسته هم دیده می‌شود تا ادمین برایش تعریف کند
        Task<BaseResultDto<List<ConsultationClinicAdminVDto>>> GetClinicsAsync(string q);
        // ماتریس کامل ۸ خانه‌ای (خانه‌ی تعریف‌نشده با Id=0، قیمت ۰ و غیرفعال)
        Task<BaseResultDto<List<ConsultationPackageAdminVDto>>> GetClinicPackagesAsync(long companionId);

        // ذخیره‌ی ماتریس یک کلینیک (همان قوانین ذخیره‌ی نماینده: قیمت > ۰ برای فعال)
        Task<BaseResultDto<List<ConsultationPackageAdminVDto>>> SaveClinicPackagesAsync(long companionId, ConsultationPackageSaveDto dto);
    }
}
