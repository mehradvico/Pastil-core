using Entities.Entities.PastilAIField;
using Entities.Entities.PastilClubField;
using System;
using System.Collections.Generic;

namespace Application.Services.PastilClubSrvs.BenefitSrv.Dto
{
    public class ClubBenefitWalletVDto
    {
        public List<ClubCouponVDto> Coupons { get; set; } = [];
        public List<ClubFreeDeliveryVDto> FreeDeliveries { get; set; } = [];
        public List<ClubPromotionalCreditVDto> PromotionalCredits { get; set; } = [];
        public List<ClubPastilAIBenefitVDto> PastilAIBenefits { get; set; } = [];
    }

    public class ClubCouponVDto
    {
        public long Id { get; set; }
        public string RewardTitle { get; set; }
        public string Code { get; set; }
        public ClubRewardApplicationMethodEnum ApplicationMethod { get; set; }
        public ClubRewardTypeEnum RewardType { get; set; }
        public decimal? BenefitValue { get; set; }
        public decimal? MaximumBenefitValue { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public bool Used { get; set; }
    }

    public class ClubFreeDeliveryVDto
    {
        public long Id { get; set; }
        public string RewardTitle { get; set; }
        public long? StoreId { get; set; }
        public long? CityId { get; set; }
        public decimal? MaximumDeliveryAmount { get; set; }
        public int RemainingUsageCount { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }

    public class ClubPromotionalCreditVDto
    {
        public long Id { get; set; }
        public string RewardTitle { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public ClubRewardTargetTypeEnum ServiceScopeType { get; set; }
        public long? ServiceScopeId { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public ClubPromotionalCreditStatusEnum Status { get; set; }
    }

    public class ClubPastilAIBenefitVDto
    {
        public long Id { get; set; }
        public string RewardTitle { get; set; }
        public long PlanId { get; set; }
        public string PlanName { get; set; }
        public PastilAiSubscriptionStatus Status { get; set; }
        public DateTime? StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
    }
}
