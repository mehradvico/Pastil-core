using Entities.Entities.PastilClubField;

namespace Application.Services.PastilClubSrvs.RewardTemplateSrv.Dto
{
    public class ClubRewardTargetDto
    {
        public long Id { get; set; }
        public ClubRewardTargetTypeEnum TargetType { get; set; }
        public long? TargetId { get; set; }
        public bool IncludeChildren { get; set; }
    }
}
