using Application.Common.Dto.Result;
using Application.Services.CommonSrv.PushBroadcastSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushBroadcastSrv.Dto
{
    public class PushMessageSearchDto : BaseSearchDto<PushMessage, PushMessageVDto>, IPushMessageSearchFields
    {
        public PushMessageSearchDto(PushMessageInputDto dto, IQueryable<PushMessage> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.PushMessageTypeId = dto.PushMessageTypeId;
        }
        public long? PushMessageTypeId { get; set; }

    }
}
