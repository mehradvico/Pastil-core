using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Entities.Entities;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionSrv.Iface
{
    public interface IPansionService : ICommonSrv<Pansion, PansionDto>
    {
        PansionSearchDto Search(PansionInputDto baseSearchDto);
        Task<BaseResultDto<PansionVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite);
        BaseResultDto UpdatePansionActiveDto(PansionActiveDto dto, long? companionId = null);
        Task<BaseResultDto> UpdatePansionApproveAsyncDto(PansionApproveDto dto);
        Task<BaseResultDto> ResubmitAsyncDto(PansionDto dto, long companionId, long ownerId);
        void UpdatePansionCommentCount(long pansionId);
        Task<List<SearchPansionDto>> SearchMinAsync(SearchRequestDto request);
        BaseResultDto GetSiteMap();

    }
}
