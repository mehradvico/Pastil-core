using Application.Services.PastilClubSrvs.PointSrv;
using Xunit;

namespace Application.Tests.PastilClub
{
    public class ClubPointBalanceCalculatorTests
    {
        [Fact]
        public void Earn_WithoutDebt_IncreasesAvailablePoint()
        {
            var result = ClubPointBalanceCalculator.Earn(20, 0, 100);

            Assert.Equal(120, result.AvailablePoint);
            Assert.Equal(0, result.DebtPoint);
        }

        [Fact]
        public void ReverseEarn_WithInsufficientAvailablePoint_CreatesDebt()
        {
            var result = ClubPointBalanceCalculator.ReverseEarn(20, 0, 100);

            Assert.Equal(0, result.AvailablePoint);
            Assert.Equal(80, result.DebtPoint);
            Assert.Equal(80, result.DebtCreatedPoint);
        }

        [Fact]
        public void Earn_WithDebt_PaysDebtBeforeIncreasingAvailablePoint()
        {
            var result = ClubPointBalanceCalculator.Earn(0, 80, 50);

            Assert.Equal(0, result.AvailablePoint);
            Assert.Equal(30, result.DebtPoint);
            Assert.Equal(50, result.DebtPaidPoint);
        }

        [Fact]
        public void Earn_MoreThanDebt_PutsRemainderInAvailablePoint()
        {
            var result = ClubPointBalanceCalculator.Earn(0, 30, 100);

            Assert.Equal(70, result.AvailablePoint);
            Assert.Equal(0, result.DebtPoint);
            Assert.Equal(30, result.DebtPaidPoint);
        }

        [Fact]
        public void Spend_WithDebtOrInsufficientPoint_IsRejected()
        {
            Assert.Throws<InvalidOperationException>(() =>
                ClubPointBalanceCalculator.Spend(100, 1, 10));
            Assert.Throws<InvalidOperationException>(() =>
                ClubPointBalanceCalculator.Spend(9, 0, 10));
        }

        [Fact]
        public void Spend_WithEnoughPoint_DecreasesAvailablePoint()
        {
            var result = ClubPointBalanceCalculator.Spend(100, 0, 40);

            Assert.Equal(60, result.AvailablePoint);
            Assert.Equal(0, result.DebtPoint);
        }
    }
}
