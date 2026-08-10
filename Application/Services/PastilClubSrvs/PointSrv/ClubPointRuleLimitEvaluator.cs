namespace Application.Services.PastilClubSrvs.PointSrv
{
    public static class ClubPointRuleLimitEvaluator
    {
        public static bool CanAward(
            int dailyCount,
            int monthlyCount,
            int lifetimeCount,
            int? dailyLimit,
            int? monthlyLimit,
            int? lifetimeLimit)
        {
            return (!dailyLimit.HasValue || dailyCount < dailyLimit.Value) &&
                   (!monthlyLimit.HasValue || monthlyCount < monthlyLimit.Value) &&
                   (!lifetimeLimit.HasValue || lifetimeCount < lifetimeLimit.Value);
        }
    }
}
