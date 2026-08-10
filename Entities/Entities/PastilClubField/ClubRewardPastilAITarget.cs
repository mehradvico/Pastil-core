using Entities.Entities.CommonField;
using Entities.Entities.PastilAIField;

namespace Entities.Entities.PastilClubField
{
    public class ClubRewardPastilAITarget : Id_Field
    {
        public long RewardTemplateId { get; set; }
        public long PlanId { get; set; }
        public long? TargetPlanId { get; set; }
        public int? FreeDays { get; set; }
        public bool IsUpgrade { get; set; }

        public ClubRewardTemplate RewardTemplate { get; set; }
        public PastilAiPlan Plan { get; set; }
        public PastilAiPlan TargetPlan { get; set; }
    }
}
