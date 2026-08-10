using Application.Common.Dto.Input;
using Entities.Entities.PastilClubField;

namespace Application.Services.PastilClubSrvs.RewardTemplateSrv.Dto
{
    public class ClubRewardTemplateInputDto : BaseInputDto
    {
        public ClubRewardTypeEnum? RewardType { get; set; }
        public ClubRewardTargetTypeEnum? TargetType { get; set; }
        public long? PetTypeId { get; set; }
        public bool? IsManualAllowed { get; set; }
        public bool? IsAutomationAllowed { get; set; }
    }
}
