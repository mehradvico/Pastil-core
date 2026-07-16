using System;

namespace Application.Services.Accounting.TicketMessageSrv.Dto
{
    public class TicketSeenVDto
    {
        public long TicketId { get; set; }
        public int SeenCount { get; set; }
        public DateTime SeenDate { get; set; }
    }
}