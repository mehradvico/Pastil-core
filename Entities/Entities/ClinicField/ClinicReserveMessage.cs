using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities.ClinicField
{
    public class ClinicReserveMessage : Id_Field
    {
        public long ClinicReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long ClinicReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }

        public string Content { get; set; }

        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReadDate { get; set; }

        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public ClinicReserve ClinicReserve { get; set; }
        public User SenderUser { get; set; }
        public Code ClinicReserveMessageType { get; set; }
        public ClinicReserveMessage ReplyToMessage { get; set; }

        public ICollection<ClinicReserveMessageAttachment> Attachments { get; set; }
        public ICollection<ClinicReserveMessageReaction> Reactions { get; set; }
    }
}
