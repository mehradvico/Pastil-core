using System.ComponentModel.DataAnnotations;

namespace Application.Services.PastilClubSrvs.RewardRedemptionSrv.Dto
{
    public class ClubRewardRedeemDto
    {
        [Range(1, long.MaxValue)]
        public long RewardOfferId { get; set; }
    }
}
