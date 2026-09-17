using Application.Common.Dto.Field;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto
{
    public class CompanionReserveMessageReactionItemVDto : Id_FieldDto
    {
        public long ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
