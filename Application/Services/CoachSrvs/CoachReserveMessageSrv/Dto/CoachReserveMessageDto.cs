using Entities.Entities.CoachField;
using Application.Common.Dto.Field;

namespace Application.Services.CoachSrvs.CoachReserveMessageSrv.Dto
{
    public class CoachReserveMessageDto : Id_FieldDto
    {
        public long CoachReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long CoachReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public string Content { get; set; }
    }
}
