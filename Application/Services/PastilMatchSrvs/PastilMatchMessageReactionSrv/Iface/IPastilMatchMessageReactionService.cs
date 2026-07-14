using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Dto;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Iface
{
    public interface IPastilMatchMessageReactionService : ICommonSrv<PastilMatchMessageReaction, PastilMatchMessageReactionDto>
    {
        PastilMatchMessageReactionSearchDto Search(PastilMatchMessageReactionInputDto dto);
        Task<BaseResultDto<PastilMatchMessageReactionVDto>> FindAsyncVDto(long id);
    }
}
