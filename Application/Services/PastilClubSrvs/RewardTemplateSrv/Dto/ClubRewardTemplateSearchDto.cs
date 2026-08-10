using Application.Common.Dto.Result;
using Entities.Entities.PastilClubField;

namespace Application.Services.PastilClubSrvs.RewardTemplateSrv.Dto
{
    public class ClubRewardTemplateSearchDto : BaseSearchDto<ClubRewardTemplateDto>
    {
        public ClubRewardTemplateSearchDto(ClubRewardTemplateInputDto dto) : base(dto)
        {
            RewardType = dto.RewardType;
            TargetType = dto.TargetType;
            PetTypeId = dto.PetTypeId;
            IsManualAllowed = dto.IsManualAllowed;
            IsAutomationAllowed = dto.IsAutomationAllowed;
        }

        public ClubRewardTypeEnum? RewardType { get; set; }
        public ClubRewardTargetTypeEnum? TargetType { get; set; }
        public long? PetTypeId { get; set; }
        public bool? IsManualAllowed { get; set; }
        public bool? IsAutomationAllowed { get; set; }
    }
}
