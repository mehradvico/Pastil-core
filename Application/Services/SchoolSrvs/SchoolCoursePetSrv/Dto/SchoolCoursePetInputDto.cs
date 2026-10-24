using Application.Common.Dto.Input;
using Application.Services.SchoolSrvs.SchoolCoursePetSrv.Iface;

namespace Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto
{
    public class SchoolCoursePetInputDto : BaseInputDto, ISchoolCoursePetSearchFields
    {
        public long? SchoolCourseId { get; set; }
        public long? CompanionId { get; set; }
        // فقط پنل نماینده: همه‌ی مدرسه‌های نمایندگی‌های «این کاربر» (Companion.OwnerId)؛ سمت کنترلر از توکن/کاربر پر می‌شود
        public long? OwnerUserId { get; set; }
    }
}
