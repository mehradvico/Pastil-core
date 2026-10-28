using Entities.Entities.CommonField;

namespace Entities.Entities.ClinicField
{
    // تخصص‌ها/نقش‌های یک عضو تیم Clinic (چندتایی، حداکثر ۱۰)؛ مثل CompanionUserExpertise
    public class ClinicUserExpertise : Id_Field
    {
        public long ClinicUserId { get; set; }
        public long ExpertiseId { get; set; }
        public ClinicUser ClinicUser { get; set; }
        public CompanionField.Expertise Expertise { get; set; }
    }
}
