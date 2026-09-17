using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class CompanionReserveMessage : Id_Field
    {
        public long CompanionReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long CompanionReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }

        public string Content { get; set; }

        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReadDate { get; set; }

        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public CompanionReserve CompanionReserve { get; set; }
        public User SenderUser { get; set; }
        public Code CompanionReserveMessageType { get; set; }
        public CompanionReserveMessage ReplyToMessage { get; set; }

        public ICollection<CompanionReserveMessageAttachment> Attachments { get; set; }
        public ICollection<CompanionReserveMessageReaction> Reactions { get; set; }
    }
}
