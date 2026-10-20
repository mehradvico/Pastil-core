using Entities.Entities.CoachField;
using Application.Common.Dto.Field;

namespace Application.Services.CoachSrvs.CoachReserveMessageSrv.Dto
{
    public class CoachReserveMessageReactionItemVDto : Id_FieldDto
    {
        public long ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
