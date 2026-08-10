using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.PastilClubSrvs.RewardOfferSrv.Dto
{
    public class ClubRewardOfferDecisionDto
    {
        [Range(1, long.MaxValue)]
        public long RewardOfferId { get; set; }

        [MaxLength(1000)]
        public string Reason { get; set; }
    }

    public class ClubRewardOfferBulkDecisionDto
    {
        [MinLength(1)]
        public List<long> RewardOfferIds { get; set; } = [];

        [MaxLength(1000)]
        public string Reason { get; set; }
    }
}
