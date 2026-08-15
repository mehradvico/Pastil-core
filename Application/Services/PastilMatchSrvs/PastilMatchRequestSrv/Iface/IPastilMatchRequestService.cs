using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Dto;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Iface
{
    public interface IPastilMatchRequestService : ICommonSrv<PastilMatchRequest, PastilMatchRequestDto>
    {
        PastilMatchRequestSearchDto Search(PastilMatchRequestInputDto dto);
        Task<BaseResultDto<PastilMatchRequestVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateResponseDto(PastilMatchRequestResponseDto dto);
        Task<BaseResultDto> DeleteAsyncDto(long id);
    }
}
