namespace Application.Services.PastilClubSrvs.PointSrv.Dto
{
    public class ClubPointBalanceVDto
    {
        public long UserId { get; set; }
        public long AvailablePoint { get; set; }
        public long DebtPoint { get; set; }
        public long LifetimeEarnedPoint { get; set; }
        public long LifetimeSpentPoint { get; set; }
        public long LifetimeReversedPoint { get; set; }
    }
}
