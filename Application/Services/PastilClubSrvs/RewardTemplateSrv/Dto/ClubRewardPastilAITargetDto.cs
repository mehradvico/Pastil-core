namespace Application.Services.PastilClubSrvs.RewardTemplateSrv.Dto
{
    public class ClubRewardPastilAITargetDto
    {
        public long PlanId { get; set; }
        public long? TargetPlanId { get; set; }
        public int? FreeDays { get; set; }
        public bool IsUpgrade { get; set; }
    }
}
