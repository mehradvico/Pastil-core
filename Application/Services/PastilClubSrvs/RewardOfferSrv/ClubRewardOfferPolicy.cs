using Entities.Entities.PastilClubField;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.PastilClubSrvs.RewardOfferSrv
{
    public static class ClubRewardOfferPolicy
    {
        public static bool IsVisible(
            ClubRewardOfferStatusEnum status,
            DateTimeOffset expiresAt,
            bool templateActive,
            DateTimeOffset? templateStartDate,
            DateTimeOffset? templateEndDate,
            DateTimeOffset now)
        {
            return status == ClubRewardOfferStatusEnum.Approved &&
                   expiresAt > now &&
                   templateActive &&
                   (!templateStartDate.HasValue || templateStartDate <= now) &&
                   (!templateEndDate.HasValue || templateEndDate >= now);
        }

        // جایزه‌ی رایگان (PointCost = 0، مثل هدیه‌ی تکمیل پروفایل پت) امتیاز نمی‌خواهد و بدهی امتیاز هم جلویش را نمی‌گیرد.
        public static bool CanRedeem(long availablePoint, long debtPoint, long pointCost, bool petEligible) =>
            petEligible && (pointCost == 0 || pointCost > 0 && debtPoint == 0 && availablePoint >= pointCost);
    }

    public static class ClubRewardPetEligibilityEvaluator
    {
        public static bool IsEligible(
            IEnumerable<long> userPetTypeIds,
            IEnumerable<long> rewardPetTypeIds)
        {
            var targets = rewardPetTypeIds?.ToHashSet() ?? [];
            return targets.Count == 0 || userPetTypeIds != null && userPetTypeIds.Any(targets.Contains);
        }
    }
}
