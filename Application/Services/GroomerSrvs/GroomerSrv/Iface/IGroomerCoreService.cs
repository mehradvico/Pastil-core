using Application.Common.Dto.Result;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.GroomerSrvs.GroomerSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.GroomerSrvs.GroomerSrv.Iface
{
    /// <summary>
    /// آرایشگاه (Groomer). همه‌ی متدهای «مالک» با <c>ownerUserId</c> فقط روی آرایشگاه خود آن کاربر کار می‌کنند؛
    /// ownerUserId = null یعنی ادمین.
    /// </summary>
    public interface IGroomerCoreService
    {
        GroomerSearchDto Search(GroomerInputDto input, bool onlyDeleted = false);
        Task<BaseResultDto<GroomerVDto>> FindAsyncVDto(long id, bool includeDeleted = false);
        /// <summary>جستجوی عمومی: فقط آرایشگاه تأییدشده و فعال، بدون شناسه‌ی مالک/ارائه‌دهنده.</summary>
        GroomerSearchDto SearchPublic(GroomerInputDto input);
        Task<BaseResultDto<GroomerVDto>> FindPublicAsync(long id);
        Task<BaseResultDto<GroomerVDto>> CreateAsync(GroomerDto dto, bool byAdmin, long actorUserId);
        Task<BaseResultDto> UpdateAsync(GroomerDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdateActiveAsync(GroomerActiveDto dto, long? ownerUserId);
        Task<BaseResultDto> UpdateApproveAsync(GroomerApproveDto dto);
        Task<BaseResultDto> UpdateAdminSettingsAsync(GroomerAdminSettingsDto dto);
        Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite);
        Task<BaseResultDto> SoftDeleteAsync(long id, long? ownerUserId, long actorUserId);
        Task<BaseResultDto> RestoreAsync(long id);
        Task<BaseResultDto> SetPetsAsync(long groomerId, IReadOnlyCollection<long> petIds, long? ownerUserId);
        Task UpdateCommentCountAsync(long groomerId);
        Task<List<SearchGroomerDto>> SearchMinAsync(SearchRequestDto request);
        BaseResultDto GetSiteMap();
    }
}
