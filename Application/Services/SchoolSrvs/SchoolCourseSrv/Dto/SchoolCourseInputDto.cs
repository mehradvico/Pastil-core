using Application.Common.Dto.Input;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv.Dto
{
    public class SchoolCourseInputDto : BaseInputDto
    {
        public long? SchoolId { get; set; }
        // فقط پنل نماینده: دوره‌های مدرسه‌های همین نماینده (سمت کنترلر از توکن پر می‌شود)
        public long? CompanionId { get; set; }
        // فقط پنل نماینده: همه‌ی مدرسه‌های نمایندگی‌های «این کاربر» (Companion.OwnerId)؛ سمت کنترلر از توکن/کاربر پر می‌شود
        public long? OwnerUserId { get; set; }
        public int? CourseTypeId { get; set; }
        public long? PetId { get; set; }
    }
}
