using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Iface
{
    public interface ICompanionReserveMessageReactionService : ICommonSrv<CompanionReserveMessageReaction, CompanionReserveMessageReactionDto>
    {
        CompanionReserveMessageReactionSearchDto Search(CompanionReserveMessageReactionInputDto dto);
        Task<BaseResultDto<CompanionReserveMessageReactionVDto>> FindAsyncVDto(long id);
    }
}
