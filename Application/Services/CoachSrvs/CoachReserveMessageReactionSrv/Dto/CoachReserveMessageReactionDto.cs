using Entities.Entities.CoachField;
using Application.Common.Dto.Field;

namespace Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Dto
{
    public class CoachReserveMessageReactionDto : Id_FieldDto
    {
        public long CoachReserveMessageId { get; set; }
        public string Reaction { get; set; }
    }
}
