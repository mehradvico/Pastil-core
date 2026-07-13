using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Dto;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Iface
{
    public interface IPastilMatchBlockService : ICommonSrv<PastilMatchBlock, PastilMatchBlockDto>
    {
        PastilMatchBlockSearchDto Search(PastilMatchBlockInputDto dto);
        Task<BaseResultDto<PastilMatchBlockVDto>> FindAsyncVDto(long id);
    }
}
