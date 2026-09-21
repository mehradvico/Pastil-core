using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationPackageSrv.Iface
{
    public interface IConsultationPackageService
    {
        // ماتریس کامل ۸ خانه‌ای پکیج‌های یک کلینیک (برای صفحه‌ی تعریف پکیج نماینده)
        Task<BaseResultDto<List<ConsultationPackageItemDto>>> GetMatrixAsync(long companionId);

        // ذخیره‌ی ماتریس (upsert)؛ فقط برای مالک همان کلینیک از کنترلر صدا زده می‌شود
        Task<BaseResultDto<List<ConsultationPackageItemDto>>> SaveMatrixAsync(long companionId, ConsultationPackageSaveDto dto);

        // پکیج‌های قابل خرید یک کلینیک برای کاربر
        Task<BaseResultDto<List<ConsultationPackagePublicVDto>>> GetPublicAsync(long companionId);
    }
}
