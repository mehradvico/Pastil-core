using Entities.Entities.CommonField;

namespace Entities.Entities.CompanionField
{
    // تخصص/عنوان شغلی‌های یک عضو تیم کلینیک (چندتایی). CompanionUser.ExpertiseId «تخصص اصلی» (اولین مورد) را
    // برای سازگاری با کدهای قدیمی نگه می‌دارد؛ منبع کامل فهرست همین جدول است.
    public class CompanionUserExpertise : Id_Field
    {
        public long CompanionUserId { get; set; }
        public long ExpertiseId { get; set; }
        public CompanionUser CompanionUser { get; set; }
        public Expertise Expertise { get; set; }
    }
}
