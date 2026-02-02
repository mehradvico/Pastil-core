using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CommonSrv.NeighborhoodSrv.Dto;
using Application.Services.CommonSrv.PushBroadcastSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushBroadcastSrv.Iface
{
    public interface IPushMessageService : ICommonSrv<PushMessage, PushMessageDto>
    {
        PushMessageSearchDto Search(PushMessageInputDto dto);
        Task<BaseResultDto<PushMessageVDto>> FindAsyncVDto(long id);
    }
}
