using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities.CoachField
{
    public class CoachReserveMessage : Id_Field
    {
        public long CoachReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long CoachReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }

        public string Content { get; set; }

        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReadDate { get; set; }

        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public CoachReserve CoachReserve { get; set; }
        public User SenderUser { get; set; }
        public Code CoachReserveMessageType { get; set; }
        public CoachReserveMessage ReplyToMessage { get; set; }

        public ICollection<CoachReserveMessageAttachment> Attachments { get; set; }
        public ICollection<CoachReserveMessageReaction> Reactions { get; set; }
    }
}
