using Application.Common.Dto.Field;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Dto
{
    public class CompanionReserveMessageReactionDto : Id_FieldDto
    {
        public long CompanionReserveMessageId { get; set; }
        public string Reaction { get; set; }
    }
}
