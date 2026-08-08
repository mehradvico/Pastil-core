using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Iface
{
    public interface IPastilMatchProfileService : ICommonSrv<PastilMatchProfile, PastilMatchProfileDto>
    {
        PastilMatchProfileSearchDto Search(PastilMatchProfileInputDto baseSearchDto);
        Task<BaseResultDto<PastilMatchProfileVDto>> FindAsyncVDto(long id);
        BaseResultDto UpdateActiveDto(PastilMatchProfileActiveDto dto);
        Task<BaseResultDto> RequestVerificationDto(PastilMatchProfileVerificationRequestDto dto);
        BaseResultDto UpdateVerificationDto(PastilMatchProfileVerificationDto dto);
    }
}
