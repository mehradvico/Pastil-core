using Entities.Entities.CommonField;

namespace Entities.Entities.PastilClubField
{
    public class ClubRewardPetType : Id_Field
    {
        public long RewardTemplateId { get; set; }
        public long PetTypeId { get; set; }

        public ClubRewardTemplate RewardTemplate { get; set; }
        public Pet PetType { get; set; }
    }
}
