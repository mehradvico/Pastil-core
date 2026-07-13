using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchSrv.Dto;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchSrv.Iface
{
    public interface IPastilMatchService : ICommonSrv<PastilMatch, PastilMatchDto>
    {
        PastilMatchSearchDto Search(PastilMatchInputDto dto);
        Task<BaseResultDto<PastilMatchVDto>> FindAsyncVDto(long id);
    }
}
