using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachCatalogSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachCatalogSrv.Iface
{
    /// <summary>
    /// خدمات و بسته‌های قیمتی مربی (جایگزین CompanionAssistance / CompanionAssistancePackage).
    /// ownerUserId = null یعنی ادمین؛ در غیر این صورت فقط مالک همان مربی.
    /// </summary>
    public interface ICoachCatalogService
    {
        /// <summary>لیست مدیریتی (همه‌ی وضعیت‌ها) برای مالک/ادمین.</summary>
        Task<BaseResultDto<List<CoachServiceVDto>>> ListManageAsync(long coachId, long? ownerUserId);
        /// <summary>لیست عمومی: فقط خدمات فعال و تأییدشده‌ی مربی فعال؛ petSize (اختیاری) بسته‌ها را فیلتر می‌کند.</summary>
        Task<BaseResultDto<List<CoachServiceVDto>>> ListPublicAsync(long coachId, string petSize);

        Task<BaseResultDto<CoachServiceVDto>> AddServiceAsync(CoachServiceInputDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdateServiceAsync(CoachServiceInputDto dto, long? ownerUserId);
        Task<BaseResultDto> SetServiceActiveAsync(long serviceId, bool active, long? ownerUserId);
        Task<BaseResultDto> ApproveServiceAsync(CoachServiceApproveDto dto);
        Task<BaseResultDto> DeleteServiceAsync(long serviceId, long? ownerUserId);
        Task<BaseResultDto> SetServiceUsersAsync(long serviceId, IReadOnlyCollection<long> userIds, long? ownerUserId);

        Task<BaseResultDto<CoachServicePackageVDto>> AddPackageAsync(CoachServicePackageInputDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdatePackageAsync(CoachServicePackageInputDto dto, long? ownerUserId);
        Task<BaseResultDto> SetPackageActiveAsync(long packageId, bool active, long? ownerUserId);
        Task<BaseResultDto> DeletePackageAsync(long packageId, long? ownerUserId);
    }
}
