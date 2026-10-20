using Entities.Entities.CoachField;
using Application.Common.Dto.Field;

namespace Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Dto
{
    public class CoachReserveMessageReactionVDto : Id_FieldDto
    {
        public long CoachReserveMessageId { get; set; }
        public long ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
