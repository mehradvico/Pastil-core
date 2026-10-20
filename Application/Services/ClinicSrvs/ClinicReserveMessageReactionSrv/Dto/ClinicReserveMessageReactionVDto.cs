using Entities.Entities.ClinicField;
using Application.Common.Dto.Field;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Dto
{
    public class ClinicReserveMessageReactionVDto : Id_FieldDto
    {
        public long ClinicReserveMessageId { get; set; }
        public long ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
