using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Dto;
using Entities.Entities;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Iface
{
    public interface IPastilMatchProfileGoalService : ICommonSrv<PastilMatchProfileGoal, PastilMatchProfileGoalDto>
    {
        PastilMatchProfileGoalSearchDto Search(PastilMatchProfileGoalInputDto baseSearchDto);
        Task<BaseResultDto<PastilMatchProfileGoalVDto>> FindAsyncVDto(long id);
    }
}
