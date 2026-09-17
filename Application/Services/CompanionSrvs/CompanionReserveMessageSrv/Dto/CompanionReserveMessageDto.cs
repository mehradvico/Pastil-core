using Application.Common.Dto.Field;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto
{
    public class CompanionReserveMessageDto : Id_FieldDto
    {
        public long CompanionReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long CompanionReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public string Content { get; set; }
    }
}
