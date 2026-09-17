using Application.Common.Dto.Input;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Iface;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto
{
    public class CompanionReserveMessageInputDto : BaseInputDto, ICompanionReserveMessageSearchFields
    {
        public long? CompanionReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long? CompanionReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public bool? IsRead { get; set; }
        public long? BeforeMessageId { get; set; }
        public long? AfterMessageId { get; set; }
    }
}
