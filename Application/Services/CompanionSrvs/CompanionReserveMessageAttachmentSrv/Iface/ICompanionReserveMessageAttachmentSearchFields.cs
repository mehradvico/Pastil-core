namespace Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Iface
{
    public interface ICompanionReserveMessageAttachmentSearchFields
    {
        public long? CompanionReserveMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
