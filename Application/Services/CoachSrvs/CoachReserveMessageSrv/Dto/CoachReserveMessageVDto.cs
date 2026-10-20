using Entities.Entities.CoachField;
using Application.Common.Dto.Field;
using Application.Services.Setting.CodeSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.CoachSrvs.CoachReserveMessageSrv.Dto
{
    public class CoachReserveMessageVDto : Id_FieldDto
    {
        public long CoachReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long CoachReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }

        public string Content { get; set; }

        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReadDate { get; set; }

        public DateTime CreateDate { get; set; }

        public CodeVDto CoachReserveMessageType { get; set; }
        public CoachReserveMessageReplyVDto ReplyToMessage { get; set; }

        public List<CoachReserveMessageAttachmentItemVDto> Attachments { get; set; }
        public List<CoachReserveMessageReactionItemVDto> Reactions { get; set; }
    }
}
