using Application.Common.Dto.Input;
using Entities.Entities.PastilClubField;
using System;

namespace Application.Services.PastilClubSrvs.RewardOfferSrv.Dto
{
    public class ClubRewardOfferInputDto : BaseInputDto
    {
        public long? UserId { get; set; }
        public long? RewardTemplateId { get; set; }
        public ClubRewardOfferStatusEnum? Status { get; set; }
        public ClubRewardOfferSourceEnum? SourceType { get; set; }
        public ClubRewardTypeEnum? RewardType { get; set; }
        public long? PetTypeId { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}
