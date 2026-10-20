using Application.Common.Dto.Result;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachSrv.Iface
{
    /// <summary>
    /// مربی (Coach). همه‌ی متدهای «مالک» با <c>ownerUserId</c> فقط روی مربی خود آن کاربر کار می‌کنند؛
    /// ownerUserId = null یعنی ادمین.
    /// </summary>
    public interface ICoachCoreService
    {
        CoachSearchDto Search(CoachInputDto input, bool onlyDeleted = false);
        Task<BaseResultDto<CoachVDto>> FindAsyncVDto(long id, bool includeDeleted = false);
        /// <summary>جستجوی عمومی: فقط مربی تأییدشده و فعال، بدون شناسه‌ی مالک/ارائه‌دهنده.</summary>
        CoachSearchDto SearchPublic(CoachInputDto input);
        Task<BaseResultDto<CoachVDto>> FindPublicAsync(long id);
        Task<BaseResultDto<CoachVDto>> CreateAsync(CoachDto dto, bool byAdmin, long actorUserId);
        Task<BaseResultDto> UpdateAsync(CoachDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdateActiveAsync(CoachActiveDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdateApproveAsync(CoachApproveDto dto);
        Task<BaseResultDto> UpdateAdminSettingsAsync(CoachAdminSettingsDto dto);
        Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite);
        Task<BaseResultDto> SoftDeleteAsync(long id, long? ownerUserId, long actorUserId);
        Task<BaseResultDto> RestoreAsync(long id);
        Task<BaseResultDto> SetPetsAsync(long coachId, IReadOnlyCollection<long> petIds, long? ownerUserId);
        Task UpdateCommentCountAsync(long coachId);
        Task<List<SearchCoachDto>> SearchMinAsync(SearchRequestDto request);
        BaseResultDto GetSiteMap();
    }
}
