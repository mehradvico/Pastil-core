namespace Application.Services.Accounting.TicketMessageSrv.Dto
{
    public class TicketMessageInputDto
    {
        public TicketMessageInputDto()
        {
            PageSize = 30;
        }

        public long? BeforeId { get; set; }
        public int PageSize { get; set; }
    }
}