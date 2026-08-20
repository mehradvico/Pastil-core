using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Entities.LocationField;

namespace Entities.Entities.PastilMatchField
{
    public class PastilMatchMessage : Id_Field
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

        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public PastilMatch PastilMatch { get; set; }
        public PastilMatchProfile SenderProfile { get; set; }
        public Code PastilMatchMessageType { get; set; }
        public PastilMatchMessage ReplyToMessage { get; set; }
        public Park Park { get; set; }

        public ICollection<PastilMatchMessageAttachment> Attachments { get; set; }
        public ICollection<PastilMatchMessageReaction> Reactions { get; set; }
    }
}
