using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Iface
{
    public interface ICompanionReserveMessageService : ICommonSrv<CompanionReserveMessage, CompanionReserveMessageDto>
    {
        CompanionReserveMessageSearchDto Search(CompanionReserveMessageInputDto dto);
        Task<BaseResultDto<CompanionReserveMessageVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateDeliveredDto(CompanionReserveMessageDeliveredDto dto);
        Task<BaseResultDto> UpdateReadDto(CompanionReserveMessageReadDto dto);
        Task<BaseResultDto<CompanionReserveMessageDto>> InsertSystemMessageAsync(long companionReserveId, string content);
    }
}
