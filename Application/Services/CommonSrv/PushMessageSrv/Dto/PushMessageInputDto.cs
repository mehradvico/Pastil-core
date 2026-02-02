using Application.Common.Dto.Input;
using Application.Services.CommonSrv.PushBroadcastSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushBroadcastSrv.Dto
{
    public class PushMessageInputDto : BaseInputDto, IPushMessageSearchFields
    {
        public long? PushMessageTypeId { get; set; }
    }
}
