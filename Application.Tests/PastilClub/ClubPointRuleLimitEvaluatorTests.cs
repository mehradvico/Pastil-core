using Application.Services.PastilClubSrvs.PointSrv;
using Xunit;

namespace Application.Tests.PastilClub
{
    public class ClubPointRuleLimitEvaluatorTests
    {
        [Fact]
        public void CanAward_WithoutLimits_ReturnsTrue()
        {
            var result = ClubPointRuleLimitEvaluator.CanAward(10, 20, 30, null, null, null);

            Assert.True(result);
        }

        [Theory]
        [InlineData(1, 0, 0, 1, null, null)]
        [InlineData(0, 5, 0, null, 5, null)]
        [InlineData(0, 0, 10, null, null, 10)]
        public void CanAward_WhenAConfiguredLimitIsReached_ReturnsFalse(
            int dailyCount,
            int monthlyCount,
            int lifetimeCount,
            int? dailyLimit,
            int? monthlyLimit,
            int? lifetimeLimit)
        {
            var result = ClubPointRuleLimitEvaluator.CanAward(
                dailyCount,
                monthlyCount,
                lifetimeCount,
                dailyLimit,
                monthlyLimit,
                lifetimeLimit);

            Assert.False(result);
        }

        [Fact]
        public void CanAward_WhenCountsAreBelowAllLimits_ReturnsTrue()
        {
            var result = ClubPointRuleLimitEvaluator.CanAward(1, 3, 9, 2, 4, 10);

            Assert.True(result);
        }
    }
}
