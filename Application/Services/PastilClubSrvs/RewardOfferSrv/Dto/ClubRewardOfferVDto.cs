using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.PastilClubSrvs.RewardTemplateSrv.Dto;
using Entities.Entities.PastilClubField;
using System;
using System.Collections.Generic;

namespace Application.Services.PastilClubSrvs.RewardOfferSrv.Dto
{
    public class ClubRewardOfferVDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
        public long RewardTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Terms { get; set; }
        public ClubRewardTypeEnum RewardType { get; set; }
        public ClubRewardOfferSourceEnum SourceType { get; set; }
        public ClubRewardOfferStatusEnum Status { get; set; }
        public long PointCost { get; set; }
        public decimal? BenefitValue { get; set; }
        public decimal? MaximumBenefitValue { get; set; }
        public DateTimeOffset GeneratedDate { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }
        public DateTimeOffset? RejectedDate { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? RedeemedDate { get; set; }
        public string RejectReason { get; set; }
        public PictureVDto Picture { get; set; }
        public List<ClubRewardTargetDto> Targets { get; set; } = [];
        public List<long> PetTypeIds { get; set; } = [];
        public bool CanRedeem { get; set; }
    }
}
