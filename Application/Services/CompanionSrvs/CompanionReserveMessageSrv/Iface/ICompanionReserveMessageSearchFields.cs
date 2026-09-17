namespace Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Iface
{
    public interface ICompanionReserveMessageSearchFields
    {
        public long? CompanionReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long? CompanionReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public bool? IsRead { get; set; }
        public long? BeforeMessageId { get; set; }
        public long? AfterMessageId { get; set; }
    }
}
