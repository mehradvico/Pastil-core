using Entities.Entities.ClinicField;
namespace Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Iface
{
    public interface IClinicReserveMessageReactionSearchFields
    {
        public long? ClinicReserveMessageId { get; set; }
        public long? ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
