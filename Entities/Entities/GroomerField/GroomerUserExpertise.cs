using Entities.Entities.CommonField;

namespace Entities.Entities.GroomerField
{
    // تخصص‌ها/نقش‌های یک عضو تیم Groomer (چندتایی، حداکثر ۱۰)؛ مثل CompanionUserExpertise
    public class GroomerUserExpertise : Id_Field
    {
        public long GroomerUserId { get; set; }
        public long ExpertiseId { get; set; }
        public GroomerUser GroomerUser { get; set; }
        public CompanionField.Expertise Expertise { get; set; }
    }
}
