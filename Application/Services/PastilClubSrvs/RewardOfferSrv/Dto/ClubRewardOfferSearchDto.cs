using Application.Common.Dto.Result;
using Entities.Entities.PastilClubField;
using System;

namespace Application.Services.PastilClubSrvs.RewardOfferSrv.Dto
{
    public class ClubRewardOfferSearchDto : BaseSearchDto<ClubRewardOfferVDto>
    {
        public ClubRewardOfferSearchDto(ClubRewardOfferInputDto dto) : base(dto)
        {
            UserId = dto.UserId;
            RewardTemplateId = dto.RewardTemplateId;
            Status = dto.Status;
            SourceType = dto.SourceType;
            RewardType = dto.RewardType;
            PetTypeId = dto.PetTypeId;
            FromDate = dto.FromDate;
            ToDate = dto.ToDate;
        }

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
