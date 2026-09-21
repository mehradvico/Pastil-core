using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationAdminSrv.Iface
{
    // نمای فقط‌خواندنی ادمین روی خریدها و بسته‌های مشاوره‌ی آنلاین (بدون تغییر وضعیت/مالی)
    public interface IConsultationAdminService
    {
        Task<BaseResultDto<ConsultationPurchaseAdminSearchDto>> SearchPurchasesAsync(ConsultationPurchaseAdminInputDto dto);
        Task<BaseResultDto<ConsultationPurchaseAdminVDto>> GetPurchaseAsync(long id);
        Task<BaseResultDto<List<ConsultationClinicAdminVDto>>> GetClinicsAsync();
        Task<BaseResultDto<List<ConsultationPackageAdminVDto>>> GetClinicPackagesAsync(long companionId);
    }
}
