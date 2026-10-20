using Application.Common.Dto.Result;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicSrv.Iface
{
    /// <summary>
    /// کلینیک (Clinic). همه‌ی متدهای «مالک» با <c>ownerUserId</c> فقط روی کلینیک خود آن کاربر کار می‌کنند؛
    /// ownerUserId = null یعنی ادمین.
    /// </summary>
    public interface IClinicCoreService
    {
        ClinicSearchDto Search(ClinicInputDto input, bool onlyDeleted = false);
        Task<BaseResultDto<ClinicVDto>> FindAsyncVDto(long id, bool includeDeleted = false);
        /// <summary>جستجوی عمومی: فقط کلینیک تأییدشده و فعال، بدون شناسه‌ی مالک/ارائه‌دهنده.</summary>
        ClinicSearchDto SearchPublic(ClinicInputDto input);
        Task<BaseResultDto<ClinicVDto>> FindPublicAsync(long id);
        Task<BaseResultDto<ClinicVDto>> CreateAsync(ClinicDto dto, bool byAdmin, long actorUserId);
        Task<BaseResultDto> UpdateAsync(ClinicDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdateActiveAsync(ClinicActiveDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdateApproveAsync(ClinicApproveDto dto);
        Task<BaseResultDto> UpdateAdminSettingsAsync(ClinicAdminSettingsDto dto);
        Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite);
        Task<BaseResultDto> SoftDeleteAsync(long id, long? ownerUserId, long actorUserId);
        Task<BaseResultDto> RestoreAsync(long id);
        Task<BaseResultDto> SetPetsAsync(long clinicId, IReadOnlyCollection<long> petIds, long? ownerUserId);
        Task UpdateCommentCountAsync(long clinicId);
        Task<List<SearchClinicDto>> SearchMinAsync(SearchRequestDto request);
        BaseResultDto GetSiteMap();
    }
}
