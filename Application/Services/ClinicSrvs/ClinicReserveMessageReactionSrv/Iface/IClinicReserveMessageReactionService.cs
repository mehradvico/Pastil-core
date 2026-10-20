using Entities.Entities.ClinicField;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Iface
{
    public interface IClinicReserveMessageReactionService : ICommonSrv<ClinicReserveMessageReaction, ClinicReserveMessageReactionDto>
    {
        ClinicReserveMessageReactionSearchDto Search(ClinicReserveMessageReactionInputDto dto);
        Task<BaseResultDto<ClinicReserveMessageReactionVDto>> FindAsyncVDto(long id);
    }
}
