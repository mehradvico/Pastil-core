using Application.Common.Dto.Input;
using Application.Services.SchoolSrvs.SchoolPictureSrv.Iface;

namespace Application.Services.SchoolSrvs.SchoolPictureSrv.Dto
{
    public class SchoolPictureInputDto : BaseInputDto, ISchoolPictureSearchFields
    {
        public long? SchoolId { get; set; }
        public long? CompanionId { get; set; }
        // فقط پنل نماینده: همه‌ی مدرسه‌های نمایندگی‌های «این کاربر» (Companion.OwnerId)؛ سمت کنترلر از توکن/کاربر پر می‌شود
        public long? OwnerUserId { get; set; }
    }
}
