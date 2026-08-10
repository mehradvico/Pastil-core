using System;

namespace Application.Services.PastilClubSrvs.PointSrv
{
    public readonly record struct ClubPointBalanceChange(
        long AvailablePoint,
        long DebtPoint,
        long DebtPaidPoint,
        long DebtCreatedPoint);

    public static class ClubPointBalanceCalculator
    {
        public static ClubPointBalanceChange Earn(long availablePoint, long debtPoint, long amount)
        {
            ValidateState(availablePoint, debtPoint, amount);
            var debtPaid = Math.Min(debtPoint, amount);
            return new ClubPointBalanceChange(
                availablePoint + amount - debtPaid,
                debtPoint - debtPaid,
                debtPaid,
                0);
        }

        public static ClubPointBalanceChange Spend(long availablePoint, long debtPoint, long amount)
        {
            ValidateState(availablePoint, debtPoint, amount);
            if (debtPoint > 0 || availablePoint < amount)
                throw new InvalidOperationException("CLUB_POINT_NOT_ENOUGH");

            return new ClubPointBalanceChange(
                availablePoint - amount,
                debtPoint,
                0,
                0);
        }

        public static ClubPointBalanceChange ReverseEarn(long availablePoint, long debtPoint, long amount)
        {
            ValidateState(availablePoint, debtPoint, amount);
            var removed = Math.Min(availablePoint, amount);
            var debtCreated = amount - removed;
            return new ClubPointBalanceChange(
                availablePoint - removed,
                checked(debtPoint + debtCreated),
                0,
                debtCreated);
        }

        private static void ValidateState(long availablePoint, long debtPoint, long amount)
        {
            if (availablePoint < 0 || debtPoint < 0)
                throw new ArgumentOutOfRangeException(nameof(availablePoint));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
        }
    }
}
