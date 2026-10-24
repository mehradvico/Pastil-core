using Application.Common.Dto.Result;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationOwnerSrv;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationPackageSrv.Iface
{
    public interface IConsultationPackageService
    {
        // همه‌ی پکیج‌های نام‌دار یک کلینیک (فعال و غیرفعال) برای صفحه‌ی مدیریت پکیج نماینده
        Task<BaseResultDto<List<ConsultationPackageItemDto>>> GetListAsync(ConsultationOwner owner);

        // ساخت یک پکیج جدید؛ فقط برای مالک همان کلینیک از کنترلر صدا زده می‌شود
        Task<BaseResultDto<ConsultationPackageItemDto>> CreateAsync(ConsultationOwner owner, ConsultationPackageItemDto dto);

        // ویرایش یک پکیج (بر اساس dto.Id)
        Task<BaseResultDto<ConsultationPackageItemDto>> UpdateAsync(ConsultationOwner owner, ConsultationPackageItemDto dto);

        // حذف نرم؛ خریدهای قبلی نام و قیمت خودشان را در لحظه‌ی خرید نگه داشته‌اند و اثری نمی‌پذیرند
        Task<BaseResultDto> DeleteAsync(ConsultationOwner owner, long id);

        // پکیج‌های قابل خرید یک کلینیک برای کاربر
        Task<BaseResultDto<List<ConsultationPackagePublicVDto>>> GetPublicAsync(ConsultationOwner owner);

        // جستجوی سراسری در پکیج‌های فعال همه‌ی کلینیک‌ها
        Task<List<SearchConsultationPackageDto>> SearchMinAsync(SearchRequestDto request);
    }
}
