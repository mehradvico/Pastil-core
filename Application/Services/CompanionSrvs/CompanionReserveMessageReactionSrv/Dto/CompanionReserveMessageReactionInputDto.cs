using Application.Common.Dto.Input;
using Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Iface;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Dto
{
    public class CompanionReserveMessageReactionInputDto : BaseInputDto, ICompanionReserveMessageReactionSearchFields
    {
        public long? CompanionReserveMessageId { get; set; }
        public long? ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
