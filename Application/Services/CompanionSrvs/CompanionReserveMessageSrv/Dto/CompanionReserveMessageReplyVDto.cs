using Application.Common.Dto.Field;
using System;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto
{
    public class CompanionReserveMessageReplyVDto : Id_FieldDto
    {
        public long? SenderUserId { get; set; }
        public long CompanionReserveMessageTypeId { get; set; }
        public string Content { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
