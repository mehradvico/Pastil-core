using Entities.Entities.ClinicField;
namespace Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Iface
{
    public interface IClinicReserveMessageSearchFields
    {
        public long? ClinicReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long? ClinicReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public bool? IsRead { get; set; }
        public long? BeforeMessageId { get; set; }
        public long? AfterMessageId { get; set; }
    }
}
