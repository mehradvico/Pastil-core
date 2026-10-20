using Entities.Entities.ClinicField;
using Application.Common.Dto.Field;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Dto
{
    public class ClinicReserveMessageReactionDto : Id_FieldDto
    {
        public long ClinicReserveMessageId { get; set; }
        public string Reaction { get; set; }
    }
}
