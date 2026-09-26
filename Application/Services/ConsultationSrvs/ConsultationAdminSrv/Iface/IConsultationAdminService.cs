using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationAdminSrv.Iface
{
    // ادمین: خریدها فقط‌خواندنی‌اند (گزارش مدت/هزینه/سهم‌ها)؛ پکیج‌های نام‌دار هر کلینیک را می‌تواند تعریف/ویرایش/حذف کند
    public interface IConsultationAdminService
    {
        Task<BaseResultDto<ConsultationPurchaseAdminSearchDto>> SearchPurchasesAsync(ConsultationPurchaseAdminInputDto dto);
        Task<BaseResultDto<ConsultationPurchaseAdminVDto>> GetPurchaseAsync(long id);
        // خلاصه‌ی مالی و مدت برای همان فیلترهای جستجوی خریدها
        Task<BaseResultDto<ConsultationPurchaseAdminSummaryDto>> GetPurchaseSummaryAsync(ConsultationPurchaseAdminInputDto dto);
        // همه‌ی کلینیک‌های فعال (با q برای جستجو)؛ کلینیک بدون بسته هم دیده می‌شود تا ادمین برایش تعریف کند
        Task<BaseResultDto<List<ConsultationClinicAdminVDto>>> GetClinicsAsync(string q);
        // همه‌ی پکیج‌های نام‌دار یک کلینیک (فعال و غیرفعال)
        Task<BaseResultDto<List<ConsultationPackageAdminVDto>>> GetClinicPackagesAsync(long companionId);

        // همان قوانین ذخیره‌ی نماینده (نام، مدت از فهرست ثابت، قیمت > ۰ برای فعال)
        Task<BaseResultDto<ConsultationPackageAdminVDto>> CreateClinicPackageAsync(long companionId, ConsultationPackageItemDto dto);
        Task<BaseResultDto<ConsultationPackageAdminVDto>> UpdateClinicPackageAsync(long companionId, ConsultationPackageItemDto dto);
        Task<BaseResultDto> DeleteClinicPackageAsync(long companionId, long id);
    }
}
