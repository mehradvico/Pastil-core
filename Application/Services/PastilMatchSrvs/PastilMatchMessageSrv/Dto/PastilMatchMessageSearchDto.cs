using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto
{
    public class PastilMatchMessageSearchDto : BaseSearchDto<PastilMatchMessage, PastilMatchMessageVDto>, IPastilMatchMessageSearchFields
    {
        public PastilMatchMessageSearchDto(PastilMatchMessageInputDto dto, IQueryable<PastilMatchMessage> list, IMapper mapper) : base(dto, list, mapper)
        {
            PastilMatchId = dto.PastilMatchId;
            SenderProfileId = dto.SenderProfileId;
            PastilMatchMessageTypeId = dto.PastilMatchMessageTypeId;
            ReplyToMessageId = dto.ReplyToMessageId;
            ParkId = dto.ParkId;
            IsPinned = dto.IsPinned;
            IsRead = dto.IsRead;
            BeforeMessageId = dto.BeforeMessageId;
            AfterMessageId = dto.AfterMessageId;
        }

        public long? PastilMatchId { get; set; }
        public long? SenderProfileId { get; set; }
        public long? PastilMatchMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public long? ParkId { get; set; }
        public bool? IsPinned { get; set; }
        public bool? IsRead { get; set; }
        public long? BeforeMessageId { get; set; }
        public long? AfterMessageId { get; set; }
    }
}
