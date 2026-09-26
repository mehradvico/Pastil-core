using Entities.Entities.CommonField;

namespace Entities.Entities.CompanionField
{
    // تخصص‌های مرتبط با یک «خدمت» (مثلاً زایمان ← متخصص زنان و زایمان).
    // فقط برای پیشنهاد همکار مناسب هنگام تخصیص رزرو استفاده می‌شود؛ اتصال واقعی همکار به خدمت همچنان CompanionAssistanceUser است.
    public class AssistanceExpertise : Id_Field
    {
        public long AssistanceId { get; set; }
        public long ExpertiseId { get; set; }
        public Assistance Assistance { get; set; }
        public Expertise Expertise { get; set; }
    }
}
