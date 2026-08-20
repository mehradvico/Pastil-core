using Application.Common.Dto.Field;
using Application.Services.Setting.CodeSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.LocationFields.ParkSrv.Dto;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto
{
    public class PastilMatchMessageVDto : Id_FieldDto
    {
        public long PastilMatchId { get; set; }
        public long? SenderProfileId { get; set; }
        public long PastilMatchMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public long? ParkId { get; set; }

        public string Content { get; set; }

        public bool IsEdited { get; set; }
        public DateTime? EditDate { get; set; }

        public bool IsPinned { get; set; }
        public DateTime? PinDate { get; set; }

        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReadDate { get; set; }

        public DateTime CreateDate { get; set; }

        public CodeVDto PastilMatchMessageType { get; set; }
        public PastilMatchMessageReplyVDto ReplyToMessage { get; set; }
        public ParkVDto Park { get; set; }

        public List<PastilMatchMessageAttachmentItemVDto> Attachments { get; set; }
        public List<PastilMatchMessageReactionItemVDto> Reactions { get; set; }
    }
}
