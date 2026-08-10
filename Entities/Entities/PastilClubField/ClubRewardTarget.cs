using Entities.Entities.CommonField;

namespace Entities.Entities.PastilClubField
{
    public class ClubRewardTarget : Id_Field
    {
        public long RewardTemplateId { get; set; }
        public ClubRewardTargetTypeEnum TargetType { get; set; }
        public long? TargetId { get; set; }
        public bool IncludeChildren { get; set; }

        public ClubRewardTemplate RewardTemplate { get; set; }
    }
}
