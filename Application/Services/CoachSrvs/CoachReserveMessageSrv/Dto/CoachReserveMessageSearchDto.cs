using Entities.Entities.CoachField;
using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachReserveMessageSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CoachSrvs.CoachReserveMessageSrv.Dto
{
    public class CoachReserveMessageSearchDto : BaseSearchDto<CoachReserveMessage, CoachReserveMessageVDto>, ICoachReserveMessageSearchFields
    {
        public CoachReserveMessageSearchDto(CoachReserveMessageInputDto dto, IQueryable<CoachReserveMessage> list, IMapper mapper) : base(dto, list, mapper)
        {
            CoachReserveId = dto.CoachReserveId;
            SenderUserId = dto.SenderUserId;
            CoachReserveMessageTypeId = dto.CoachReserveMessageTypeId;
            ReplyToMessageId = dto.ReplyToMessageId;
            IsRead = dto.IsRead;
            BeforeMessageId = dto.BeforeMessageId;
            AfterMessageId = dto.AfterMessageId;
        }

        public long? CoachReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long? CoachReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public bool? IsRead { get; set; }
        public long? BeforeMessageId { get; set; }
        public long? AfterMessageId { get; set; }
    }
}
