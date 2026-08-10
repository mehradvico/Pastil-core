using Entities.Entities.PastilClubField;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.PastilClubSrvs.RewardTemplateSrv.Dto
{
    public class ClubRewardTemplateDto
    {
        public long Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [Required, MaxLength(250)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string ShortDescription { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }

        public ClubRewardTypeEnum RewardType { get; set; }
        public ClubRewardApplicationMethodEnum ApplicationMethod { get; set; }

        [Range(1, long.MaxValue)]
        public long PointCost { get; set; }

        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public ClubRewardExpirationTypeEnum ExpirationType { get; set; }

        [Range(1, int.MaxValue)]
        public int? ExpirationValue { get; set; }

        public DateTimeOffset? FixedExpirationDate { get; set; }
        public decimal? BenefitValue { get; set; }
        public decimal? MaximumBenefitValue { get; set; }
        public ClubRewardFundingTypeEnum FundingType { get; set; } = ClubRewardFundingTypeEnum.Pastil;
        public bool IsAutomationAllowed { get; set; }
        public bool IsManualAllowed { get; set; }
        public bool Active { get; set; }
        public ClubRewardNotificationLevelEnum NotificationLevel { get; set; }
        public long? PictureId { get; set; }

        [MaxLength(4000)]
        public string Terms { get; set; }

        public List<ClubRewardTargetDto> Targets { get; set; } = [];
        public List<long> PetTypeIds { get; set; } = [];
        public ClubRewardPastilAITargetDto PastilAITarget { get; set; }
    }
}
