using Entities.Entities.CoachField;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Iface
{
    public interface ICoachReserveMessageReactionService : ICommonSrv<CoachReserveMessageReaction, CoachReserveMessageReactionDto>
    {
        CoachReserveMessageReactionSearchDto Search(CoachReserveMessageReactionInputDto dto);
        Task<BaseResultDto<CoachReserveMessageReactionVDto>> FindAsyncVDto(long id);
    }
}
