using Entities.Entities.CoachField;
using Application.Common.Dto.Input;
using Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Iface;

namespace Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Dto
{
    public class CoachReserveMessageReactionInputDto : BaseInputDto, ICoachReserveMessageReactionSearchFields
    {
        public long? CoachReserveMessageId { get; set; }
        public long? ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
