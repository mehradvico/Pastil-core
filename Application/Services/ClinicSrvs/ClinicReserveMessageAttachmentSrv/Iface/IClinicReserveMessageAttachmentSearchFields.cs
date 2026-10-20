using Entities.Entities.ClinicField;
namespace Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Iface
{
    public interface IClinicReserveMessageAttachmentSearchFields
    {
        public long? ClinicReserveMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
