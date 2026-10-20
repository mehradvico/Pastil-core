using Entities.Entities.ClinicField;
using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto
{
    public class ClinicReserveMessageSearchDto : BaseSearchDto<ClinicReserveMessage, ClinicReserveMessageVDto>, IClinicReserveMessageSearchFields
    {
        public ClinicReserveMessageSearchDto(ClinicReserveMessageInputDto dto, IQueryable<ClinicReserveMessage> list, IMapper mapper) : base(dto, list, mapper)
        {
            ClinicReserveId = dto.ClinicReserveId;
            SenderUserId = dto.SenderUserId;
            ClinicReserveMessageTypeId = dto.ClinicReserveMessageTypeId;
            ReplyToMessageId = dto.ReplyToMessageId;
            IsRead = dto.IsRead;
            BeforeMessageId = dto.BeforeMessageId;
            AfterMessageId = dto.AfterMessageId;
        }

        public long? ClinicReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long? ClinicReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public bool? IsRead { get; set; }
        public long? BeforeMessageId { get; set; }
        public long? AfterMessageId { get; set; }
    }
}
