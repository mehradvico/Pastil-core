using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.AssistanceExpertiseSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.AssistanceExpertiseSrv.Iface
{
    public interface IAssistanceExpertiseService
    {
        Task<BaseResultDto<AssistanceExpertiseVDto>> GetAsync(long assistanceId);
        Task<BaseResultDto<AssistanceExpertiseVDto>> SaveAsync(AssistanceExpertiseDto dto);
    }
}
