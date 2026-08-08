using Application.Common.Dto.Result;
using Application.Services.PastilAISrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrvs.PastilAIPlan.Iface
{
    public interface IPastilAiPlanService
    {
        Task<BaseResultDto<List<PastilAiPlanVDto>>> GetPlansAsync(bool admin, CancellationToken cancellationToken);
        Task<BaseResultDto<PastilAiPlanVDto>> UpdateAsync(PastilAiPlanUpdateDto dto, CancellationToken cancellationToken);
        Task<BaseResultDto<PastilAiQuotaDto>> GetQuotaAsync(long userId, CancellationToken cancellationToken);
    }
}
