using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Ticket : Name_Field
    {
        public long UserId { get; set; }
        public long? AdminId { get; set; }

        public long StatusId { get; set; }
        public long ImportanceId { get; set; }
        public long TicketCategoryId { get; set; }

        public long? ProductId { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime? CloseDate { get; set; }

        public bool Deleted { get; set; }

        public User User { get; set; }
        public User Admin { get; set; }

        public Code Status { get; set; }
        public Code Importance { get; set; }
        public Code TicketCategory { get; set; }

        public Product Product { get; set; }

        public ICollection<TicketItem> TicketItems { get; set; } = new List<TicketItem>();
    }
}
