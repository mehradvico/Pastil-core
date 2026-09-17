using Application.Common.Dto.Field;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Dto
{
    public class CompanionReserveMessageReactionVDto : Id_FieldDto
    {
        public long CompanionReserveMessageId { get; set; }
        public long ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
