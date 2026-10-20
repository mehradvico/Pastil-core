using Entities.Entities.ClinicField;
using Application.Common.Dto.Field;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto
{
    public class ClinicReserveMessageReactionItemVDto : Id_FieldDto
    {
        public long ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
