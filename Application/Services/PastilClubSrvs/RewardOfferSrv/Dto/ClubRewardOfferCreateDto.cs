using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.PastilClubSrvs.RewardOfferSrv.Dto
{
    public class ClubRewardOfferCreateDto
    {
        [Range(1, long.MaxValue)]
        public long UserId { get; set; }

        [Range(1, long.MaxValue)]
        public long RewardTemplateId { get; set; }

        public DateTimeOffset? CustomExpiresAt { get; set; }
        public bool ApproveImmediately { get; set; }
    }
}
