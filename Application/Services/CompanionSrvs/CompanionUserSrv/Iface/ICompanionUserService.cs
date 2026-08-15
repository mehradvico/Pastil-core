using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using Entities.Entities;
using Entities.Entities.CompanionField;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionUserSrv.Iface
{
    public interface ICompanionUserService : ICommonSrv<CompanionUser, CompanionUserDto>
    {
        CompanionUserSearchDto SearchDto(CompanionUserInputDto dto);
        CompanionUserSearchDto SearchPublic(CompanionUserInputDto dto);
        void InsertOrUpdate(CompanionUserDto CompanionUser);
        void InsertOrUpdate(Companion companion, List<CompanionUserDto> CompanionUsersDto);
        Task<BaseResultDto<CompanionUserDto>> UpdateAsyncDto(CompanionUserDto dto);
        Task<BaseResultDto<List<CompanionUserDto>>> GetAvailableCompanionUsersAsync(long companionAssistanceId);
        Task<BaseResultDto> Active(CompanionUserDto user);
        Task<BaseResultDto> UserAccept(CompanionUserDto user);
        Task<BaseResultDto> UserAccept(long id, long userId, bool userAccept);
    }
}
