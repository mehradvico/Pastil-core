using Entities.Entities.CommonField;

namespace Entities.Entities.CoachField
{
    // تخصص‌ها/نقش‌های یک عضو تیم Coach (چندتایی، حداکثر ۱۰)؛ مثل CompanionUserExpertise
    public class CoachUserExpertise : Id_Field
    {
        public long CoachUserId { get; set; }
        public long ExpertiseId { get; set; }
        public CoachUser CoachUser { get; set; }
        public CompanionField.Expertise Expertise { get; set; }
    }
}
