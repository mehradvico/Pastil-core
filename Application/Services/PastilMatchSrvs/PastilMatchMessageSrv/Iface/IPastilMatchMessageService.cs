using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Iface
{
    public interface IPastilMatchMessageService : ICommonSrv<PastilMatchMessage, PastilMatchMessageDto>
    {
        PastilMatchMessageSearchDto Search(PastilMatchMessageInputDto dto);
        Task<BaseResultDto<PastilMatchMessageVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateEditDto(PastilMatchMessageEditDto dto);
        Task<BaseResultDto> UpdatePinDto(PastilMatchMessagePinDto dto);
        Task<BaseResultDto> UpdateDeliveredDto(PastilMatchMessageDeliveredDto dto);
        Task<BaseResultDto> UpdateReadDto(PastilMatchMessageReadDto dto);
        Task<BaseResultDto<PastilMatchMessageDto>> InsertSystemMessageAsync(long pastilMatchId, string content);
    }
}
