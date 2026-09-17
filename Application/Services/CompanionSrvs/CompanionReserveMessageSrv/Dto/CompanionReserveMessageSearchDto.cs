using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto
{
    public class CompanionReserveMessageSearchDto : BaseSearchDto<CompanionReserveMessage, CompanionReserveMessageVDto>, ICompanionReserveMessageSearchFields
    {
        public CompanionReserveMessageSearchDto(CompanionReserveMessageInputDto dto, IQueryable<CompanionReserveMessage> list, IMapper mapper) : base(dto, list, mapper)
        {
            CompanionReserveId = dto.CompanionReserveId;
            SenderUserId = dto.SenderUserId;
            CompanionReserveMessageTypeId = dto.CompanionReserveMessageTypeId;
            ReplyToMessageId = dto.ReplyToMessageId;
            IsRead = dto.IsRead;
            BeforeMessageId = dto.BeforeMessageId;
            AfterMessageId = dto.AfterMessageId;
        }

        public long? CompanionReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long? CompanionReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public bool? IsRead { get; set; }
        public long? BeforeMessageId { get; set; }
        public long? AfterMessageId { get; set; }
    }
}
