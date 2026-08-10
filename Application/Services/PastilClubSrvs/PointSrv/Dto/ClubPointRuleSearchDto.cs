using Application.Common.Dto.Result;

namespace Application.Services.PastilClubSrvs.PointSrv.Dto
{
    public class ClubPointRuleSearchDto : BaseSearchDto<ClubPointRuleDto>
    {
        public ClubPointRuleSearchDto(ClubPointRuleInputDto dto)
            : base(dto)
        {
            EventType = dto.EventType;
        }

        public Entities.Entities.PastilClubField.ClubPointEventTypeEnum? EventType { get; set; }
    }
}
