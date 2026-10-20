using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerCatalogSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.GroomerSrvs.GroomerCatalogSrv.Iface
{
    /// <summary>
    /// خدمات و بسته‌های قیمتی آرایشگاه (جایگزین CompanionAssistance / CompanionAssistancePackage).
    /// ownerUserId = null یعنی ادمین؛ در غیر این صورت فقط مالک همان آرایشگاه.
    /// </summary>
    public interface IGroomerCatalogService
    {
        /// <summary>لیست مدیریتی (همه‌ی وضعیت‌ها) برای مالک/ادمین.</summary>
        Task<BaseResultDto<List<GroomerServiceVDto>>> ListManageAsync(long groomerId, long? ownerUserId);
        /// <summary>لیست عمومی: فقط خدمات فعال و تأییدشده‌ی آرایشگاه فعال؛ petSize (اختیاری) بسته‌ها را فیلتر می‌کند.</summary>
        Task<BaseResultDto<List<GroomerServiceVDto>>> ListPublicAsync(long groomerId, string petSize);

        Task<BaseResultDto<GroomerServiceVDto>> AddServiceAsync(GroomerServiceInputDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdateServiceAsync(GroomerServiceInputDto dto, long? ownerUserId);
        Task<BaseResultDto> SetServiceActiveAsync(long serviceId, bool active, long? ownerUserId);
        Task<BaseResultDto> ApproveServiceAsync(GroomerServiceApproveDto dto);
        Task<BaseResultDto> DeleteServiceAsync(long serviceId, long? ownerUserId);
        Task<BaseResultDto> SetServiceUsersAsync(long serviceId, IReadOnlyCollection<long> userIds, long? ownerUserId);

        Task<BaseResultDto<GroomerServicePackageVDto>> AddPackageAsync(GroomerServicePackageInputDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdatePackageAsync(GroomerServicePackageInputDto dto, long? ownerUserId);
        Task<BaseResultDto> SetPackageActiveAsync(long packageId, bool active, long? ownerUserId);
        Task<BaseResultDto> DeletePackageAsync(long packageId, long? ownerUserId);
    }
}
