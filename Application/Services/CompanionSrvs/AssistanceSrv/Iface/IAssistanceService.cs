using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.AssistanceSrv.Iface
{
    public interface IAssistanceService : ICommonSrv<Assistance, AssistanceDto>
    {
        AssistanceSearchDto Search(AssistanceInputDto baseSearchDto);
        Task<BaseResultDto<AssistanceVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite);
        Task<List<SearchAssistanceDto>> SearchMinAsync(SearchRequestDto request);

    }
}
