using Application.Common.Dto.Result;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushSubscriptionSrv.Iface
{
    public interface IPushSubscriptionService
    {
        Task<BaseResultDto> SubscribeAsync(long? userId, PushSubscribeDto dto);
        Task<BaseResultDto> AttachAsync(long userId, Guid deviceKey);
    }
}
