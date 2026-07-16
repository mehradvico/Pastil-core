using Application.Services.Accounting.TicketItemSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.Accounting.TicketMessageSrv.Dto
{
    public class TicketMessageSearchDto
    {
        public TicketMessageSearchDto()
        {
            List = new List<TicketItemVDto>();
        }

        public long TicketId { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
        public long? NextBeforeId { get; set; }
        public List<TicketItemVDto> List { get; set; }
    }
}