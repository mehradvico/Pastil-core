using Application.Common.Dto.Input;
using Entities.Entities.PastilClubField;

namespace Application.Services.PastilClubSrvs.PointSrv.Dto
{
    public class ClubPointRuleInputDto : BaseInputDto
    {
        public ClubPointEventTypeEnum? EventType { get; set; }
    }
}
