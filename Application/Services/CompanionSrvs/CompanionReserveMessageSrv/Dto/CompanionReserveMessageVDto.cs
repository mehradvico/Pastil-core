using Application.Common.Dto.Field;
using Application.Services.Setting.CodeSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto
{
    public class CompanionReserveMessageVDto : Id_FieldDto
    {
        public long CompanionReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long CompanionReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }

        public string Content { get; set; }

        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReadDate { get; set; }

        public DateTime CreateDate { get; set; }

        public CodeVDto CompanionReserveMessageType { get; set; }
        public CompanionReserveMessageReplyVDto ReplyToMessage { get; set; }

        public List<CompanionReserveMessageAttachmentItemVDto> Attachments { get; set; }
        public List<CompanionReserveMessageReactionItemVDto> Reactions { get; set; }
    }
}
