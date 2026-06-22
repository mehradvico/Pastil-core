using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Iface
{
    public interface ICompanionAssistanceUserService : ICommonSrv<CompanionAssistanceUser, CompanionAssistanceUserDto>
    {
        CompanionAssistanceUserSearchDto Search(CompanionAssistanceUserInputDto baseSearchDto);
        Task<BaseResultDto<CompanionAssistanceUserVDto>> FindAsyncVDto(long id);
        BaseResultDto ActivationDto(CompanionAssistanceUserActivationDto dto);
    }
}
