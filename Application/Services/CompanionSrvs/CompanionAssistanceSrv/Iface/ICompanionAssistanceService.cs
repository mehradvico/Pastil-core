using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface
{
    public interface ICompanionAssistanceService : ICommonSrv<CompanionAssistance, CompanionAssistanceDto>
    {
        CompanionAssistanceSearchDto Search(CompanionAssistanceInputDto baseSearchDto);
        Task<BaseResultDto<CompanionAssistanceVDto>> FindAsyncVDto(long Id);
        Task<BaseResultDto> UpdateAsyncDto(CompanionAssistanceDto dto, long? companionId = null);
        BaseResultDto ActivationDto(CompanionAssistanceActivationDto dto);
    }
}
