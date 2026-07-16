using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class TicketItem : Id_Field
    {
        public string Body { get; set; }
        public long UserId { get; set; }
        public long TicketId { get; set; }
        public long? FileId { get; set; }
        public long? ReplyToTicketItemId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsSeen { get; set; }
        public DateTime? SeenDate { get; set; }
        public bool Deleted { get; set; }

        public User User { get; set; }
        public File File { get; set; }
        public Ticket Ticket { get; set; }
        public TicketItem ReplyToTicketItem { get; set; }

        public ICollection<TicketItem> Replies { get; set; }
    }
}