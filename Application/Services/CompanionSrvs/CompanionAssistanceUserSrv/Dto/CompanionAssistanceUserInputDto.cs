using Application.Common.Dto.Input;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Iface;

namespace Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Dto
{
    public class CompanionAssistanceUserInputDto : BaseInputDto, ICompanionAssistanceUserSearchFields
    {
        public long? CompanionId { get; set; }
        public long? CompanionAssistanceId { get; set; }
        public long? UserId { get; set; }
    }
}
