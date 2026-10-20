using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicCatalogSrv.Iface
{
    /// <summary>
    /// خدمات و بسته‌های قیمتی کلینیک (جایگزین CompanionAssistance / CompanionAssistancePackage).
    /// ownerUserId = null یعنی ادمین؛ در غیر این صورت فقط مالک همان کلینیک.
    /// </summary>
    public interface IClinicCatalogService
    {
        /// <summary>لیست مدیریتی (همه‌ی وضعیت‌ها) برای مالک/ادمین.</summary>
        Task<BaseResultDto<List<ClinicServiceVDto>>> ListManageAsync(long clinicId, long? ownerUserId);
        /// <summary>لیست عمومی: فقط خدمات فعال و تأییدشده‌ی کلینیک فعال؛ petSize (اختیاری) بسته‌ها را فیلتر می‌کند.</summary>
        Task<BaseResultDto<List<ClinicServiceVDto>>> ListPublicAsync(long clinicId, string petSize);

        Task<BaseResultDto<ClinicServiceVDto>> AddServiceAsync(ClinicServiceInputDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdateServiceAsync(ClinicServiceInputDto dto, long? ownerUserId);
        Task<BaseResultDto> SetServiceActiveAsync(long serviceId, bool active, long? ownerUserId);
        Task<BaseResultDto> ApproveServiceAsync(ClinicServiceApproveDto dto);
        Task<BaseResultDto> DeleteServiceAsync(long serviceId, long? ownerUserId);
        Task<BaseResultDto> SetServiceUsersAsync(long serviceId, IReadOnlyCollection<long> userIds, long? ownerUserId);

        Task<BaseResultDto<ClinicServicePackageVDto>> AddPackageAsync(ClinicServicePackageInputDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdatePackageAsync(ClinicServicePackageInputDto dto, long? ownerUserId);
        Task<BaseResultDto> SetPackageActiveAsync(long packageId, bool active, long? ownerUserId);
        Task<BaseResultDto> DeletePackageAsync(long packageId, long? ownerUserId);
    }
}
