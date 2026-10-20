using Entities.Entities.CoachField;
using Application.Common.Dto.Field;
using System;

namespace Application.Services.CoachSrvs.CoachReserveMessageSrv.Dto
{
    public class CoachReserveMessageReplyVDto : Id_FieldDto
    {
        public long? SenderUserId { get; set; }
        public long CoachReserveMessageTypeId { get; set; }
        public string Content { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
