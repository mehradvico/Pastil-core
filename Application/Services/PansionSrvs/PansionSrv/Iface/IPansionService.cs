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
        BaseResultDto UpdatePansionActiveDto(PansionActiveDto dto);
        BaseResultDto UpdatePansionApproveDto(PansionApproveDto dto);
        void UpdatePansionCommentCount(long pansionId);
        Task<List<SearchPansionDto>> SearchMinAsync(SearchRequestDto request);

    }
}
