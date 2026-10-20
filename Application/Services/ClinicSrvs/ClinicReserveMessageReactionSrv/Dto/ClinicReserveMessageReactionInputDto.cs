using Entities.Entities.ClinicField;
using Application.Common.Dto.Input;
using Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Iface;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Dto
{
    public class ClinicReserveMessageReactionInputDto : BaseInputDto, IClinicReserveMessageReactionSearchFields
    {
        public long? ClinicReserveMessageId { get; set; }
        public long? ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
