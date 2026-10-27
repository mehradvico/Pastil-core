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
        // پیام‌های چت جلسه‌ی آنلاین یک خرید (فقط خواندن، حذف‌شده‌ها هم با علامت)
        Task<BaseResultDto<List<ConsultationAdminMessageVDto>>> GetPurchaseMessagesAsync(long id);
        // پروفایل و سابقه‌ی پزشکی پت(های) یک خرید (همان داده‌ای که نماینده می‌بیند)
        Task<BaseResultDto<List<ConsultationSessionSrv.Dto.ConsultationPetProfileVDto>>> GetPurchasePetsAsync(long id);
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

        // کلینیک/مربی مستقل (kind = clinic | coach): فهرست مراکز با خلاصه‌ی بسته‌ها، بسته‌های یک مرکز با درصد سهم سایت، و حذف بسته
        Task<BaseResultDto<List<ConsultationVenueAdminVDto>>> GetVenuesAsync(string kind, string q);
        Task<BaseResultDto<List<ConsultationVenuePackageAdminVDto>>> GetVenuePackagesAsync(string kind, long venueId);
        Task<BaseResultDto> DeleteVenuePackageAsync(string kind, long venueId, long id);
        Task<BaseResultDto> ApproveVenuePackageAsync(string kind, long venueId, long id, bool approved);
    }
}
