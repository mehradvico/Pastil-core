using Application.Services.PastilClubSrvs.BenefitSrv;
using Entities.Entities.PastilClubField;
using System;
using Xunit;

namespace Application.Tests.PastilClub
{
    public class ClubBenefitPolicyTests
    {
        [Theory]
        [InlineData(100000d, 20d, false, 0d, 20000d)]
        [InlineData(100000d, 20d, false, 15000d, 15000d)]
        [InlineData(100000d, 120000d, true, 0d, 100000d)]
        public void CalculateDiscount_AppliesTypeMaximumAndOrderCap(
            double basePrice,
            double value,
            bool fixedDiscount,
            double maximum,
            double expected)
        {
            Assert.Equal(
                Convert.ToDecimal(expected),
                ClubBenefitPolicy.CalculateDiscount(
                    Convert.ToDecimal(basePrice),
                    Convert.ToDecimal(value),
                    fixedDiscount,
                    maximum > 0 ? Convert.ToDecimal(maximum) : null));
        }

        [Fact]
        public void IsScopeEligible_GlobalCredit_IsAvailableForEveryMethod()
        {
            Assert.True(ClubBenefitPolicy.IsScopeEligible(
                ClubRewardTargetTypeEnum.Global,
                null,
                ClubRewardTargetTypeEnum.Pansion,
                17));
        }

        [Fact]
        public void IsScopeEligible_DifferentBusiness_IsRejected()
        {
            Assert.False(ClubBenefitPolicy.IsScopeEligible(
                ClubRewardTargetTypeEnum.Store,
                10,
                ClubRewardTargetTypeEnum.Store,
                11));
        }

        [Fact]
        public void ResolvePastilAIEnd_DoesNotExtendOfferExpiration()
        {
            var start = new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc);
            var offerExpiration = new DateTimeOffset(start.AddDays(3));

            var end = ClubBenefitPolicy.ResolvePastilAIEnd(start, 30, offerExpiration);

            Assert.Equal(offerExpiration.UtcDateTime, end);
        }
    }
}
