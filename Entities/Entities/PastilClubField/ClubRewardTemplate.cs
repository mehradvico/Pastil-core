using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;

namespace Entities.Entities.PastilClubField
{
    public class ClubRewardTemplate : Id_Field
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public ClubRewardTypeEnum RewardType { get; set; }
        public ClubRewardApplicationMethodEnum ApplicationMethod { get; set; }
        public long PointCost { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public ClubRewardExpirationTypeEnum ExpirationType { get; set; }
        public int? ExpirationValue { get; set; }
        public DateTimeOffset? FixedExpirationDate { get; set; }
        public decimal? BenefitValue { get; set; }
        public decimal? MaximumBenefitValue { get; set; }
        public ClubRewardFundingTypeEnum FundingType { get; set; }
        public bool IsAutomationAllowed { get; set; }
        public bool IsManualAllowed { get; set; }
        public bool Active { get; set; }
        public ClubRewardNotificationLevelEnum NotificationLevel { get; set; }
        public long? PictureId { get; set; }
        public string Terms { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public Picture Picture { get; set; }
        public ICollection<ClubRewardTarget> Targets { get; set; }
        public ICollection<ClubRewardPetType> PetTypes { get; set; }
        public ICollection<ClubRewardOffer> Offers { get; set; }
        public ClubRewardPastilAITarget PastilAITarget { get; set; }
    }
}
