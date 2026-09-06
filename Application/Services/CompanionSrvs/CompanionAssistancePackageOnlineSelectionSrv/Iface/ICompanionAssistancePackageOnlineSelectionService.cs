using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Iface
{
    public interface ICompanionAssistancePackageOnlineSelectionService : ICommonSrv<CompanionAssistancePackageOnlineSelection, CompanionAssistancePackageOnlineSelectionDto>
    {
        CompanionAssistancePackageOnlineSelectionSearchDto Search(CompanionAssistancePackageOnlineSelectionInputDto baseSearchDto);
        Task<BaseResultDto<CompanionAssistancePackageOnlineSelectionVDto>> FindAsyncVDto(long id);
        BaseResultDto ActivationDto(CompanionAssistancePackageOnlineSelectionActivationDto dto);
    }
}
