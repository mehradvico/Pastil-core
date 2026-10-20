using Entities.Entities.CoachField;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachReserveMessageSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachReserveMessageSrv.Iface
{
    public interface ICoachReserveMessageService : ICommonSrv<CoachReserveMessage, CoachReserveMessageDto>
    {
        CoachReserveMessageSearchDto Search(CoachReserveMessageInputDto dto);
        Task<BaseResultDto<CoachReserveMessageVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateDeliveredDto(CoachReserveMessageDeliveredDto dto);
        Task<BaseResultDto> UpdateReadDto(CoachReserveMessageReadDto dto);
        Task<BaseResultDto<CoachReserveMessageDto>> InsertSystemMessageAsync(long companionReserveId, string content);
    }
}
