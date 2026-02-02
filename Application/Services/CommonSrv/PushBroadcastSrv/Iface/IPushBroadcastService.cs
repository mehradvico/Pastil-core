using Application.Common.Dto.Result;
using Application.Services.CommonSrv.PushBroadcastSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushBroadcastSrv.Iface
{
    public interface IPushBroadcastService
    {
        Task<BaseResultDto> BroadcastAsync(PushBroadcastDto req);
    }

}
