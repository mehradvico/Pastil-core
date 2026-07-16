namespace Application.Services.Accounting.TicketMessageSrv.Dto
{
    public class SendTicketMessageDto
    {
        public string Body { get; set; }
        public long? FileId { get; set; }
        public long? ReplyToTicketItemId { get; set; }
    }
}