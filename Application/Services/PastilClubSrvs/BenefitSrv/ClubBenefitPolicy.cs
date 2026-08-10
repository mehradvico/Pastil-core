using Entities.Entities.PastilClubField;
using System;

namespace Application.Services.PastilClubSrvs.BenefitSrv
{
    public static class ClubBenefitPolicy
    {
        public static decimal CalculateDiscount(decimal basePrice, decimal value, bool fixedDiscount, decimal? maximum)
        {
            if (basePrice <= 0 || value <= 0)
                return 0;
            var calculated = fixedDiscount ? value : Math.Round(basePrice * value / 100, 0);
            if (maximum.HasValue)
                calculated = Math.Min(calculated, maximum.Value);
            return Math.Min(basePrice, calculated);
        }

        public static bool IsScopeEligible(
            ClubRewardTargetTypeEnum benefitScopeType,
            long? benefitScopeId,
            ClubRewardTargetTypeEnum requestedScopeType,
            long? requestedScopeId) =>
            benefitScopeType == ClubRewardTargetTypeEnum.Global ||
            benefitScopeType == requestedScopeType && benefitScopeId == requestedScopeId;

        public static DateTime ResolvePastilAIEnd(DateTime startUtc, int durationDays, DateTimeOffset offerExpiresAt)
        {
            var calculated = startUtc.AddDays(durationDays);
            return calculated < offerExpiresAt.UtcDateTime ? calculated : offerExpiresAt.UtcDateTime;
        }
    }
}
