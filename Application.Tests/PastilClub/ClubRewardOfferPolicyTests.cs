using Application.Services.PastilClubSrvs.RewardOfferSrv;
using Entities.Entities.PastilClubField;
using System;
using Xunit;

namespace Application.Tests.PastilClub
{
    public class ClubRewardOfferPolicyTests
    {
        private static readonly DateTimeOffset Now = new(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);

        [Fact]
        public void IsVisible_PendingOffer_ReturnsFalse()
        {
            var result = ClubRewardOfferPolicy.IsVisible(
                ClubRewardOfferStatusEnum.PendingApproval,
                Now.AddDays(1),
                true,
                null,
                null,
                Now);

            Assert.False(result);
        }

        [Fact]
        public void IsVisible_ApprovedActiveOffer_ReturnsTrue()
        {
            var result = ClubRewardOfferPolicy.IsVisible(
                ClubRewardOfferStatusEnum.Approved,
                Now.AddDays(1),
                true,
                Now.AddDays(-1),
                Now.AddDays(2),
                Now);

            Assert.True(result);
        }

        [Fact]
        public void IsVisible_ExpiredOffer_ReturnsFalse()
        {
            var result = ClubRewardOfferPolicy.IsVisible(
                ClubRewardOfferStatusEnum.Approved,
                Now,
                true,
                null,
                null,
                Now);

            Assert.False(result);
        }

        [Theory]
        [InlineData(300, 0, 300, true, true)]
        [InlineData(299, 0, 300, true, false)]
        [InlineData(500, 1, 300, true, false)]
        [InlineData(500, 0, 300, false, false)]
        public void CanRedeem_ValidatesBalanceDebtAndPet(
            long available,
            long debt,
            long cost,
            bool petEligible,
            bool expected)
        {
            Assert.Equal(expected, ClubRewardOfferPolicy.CanRedeem(available, debt, cost, petEligible));
        }
    }
}
