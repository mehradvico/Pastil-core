using Entities.Entities.ClinicField;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Iface
{
    public interface IClinicReserveMessageService : ICommonSrv<ClinicReserveMessage, ClinicReserveMessageDto>
    {
        ClinicReserveMessageSearchDto Search(ClinicReserveMessageInputDto dto);
        Task<BaseResultDto<ClinicReserveMessageVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateDeliveredDto(ClinicReserveMessageDeliveredDto dto);
        Task<BaseResultDto> UpdateReadDto(ClinicReserveMessageReadDto dto);
        Task<BaseResultDto<ClinicReserveMessageDto>> InsertSystemMessageAsync(long companionReserveId, string content);
    }
}
