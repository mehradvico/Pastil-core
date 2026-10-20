using Entities.Entities.ClinicField;
using Application.Common.Dto.Field;
using Application.Services.Setting.CodeSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto
{
    public class ClinicReserveMessageVDto : Id_FieldDto
    {
        public long ClinicReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long ClinicReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }

        public string Content { get; set; }

        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReadDate { get; set; }

        public DateTime CreateDate { get; set; }

        public CodeVDto ClinicReserveMessageType { get; set; }
        public ClinicReserveMessageReplyVDto ReplyToMessage { get; set; }

        public List<ClinicReserveMessageAttachmentItemVDto> Attachments { get; set; }
        public List<ClinicReserveMessageReactionItemVDto> Reactions { get; set; }
    }
}
