using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.ExpertiseSrv.Dto;
using Application.Services.Dto;
using System.Runtime.Serialization;

namespace Application.Services.CompanionSrvs.CompanionUserSrv.Dto
{
    public class CompanionUserDto : Id_FieldDto
    {
        public bool? UserAccept { get; set; }

        public long CompanionId { get; set; }
        public string Phone { get; set; }
        public long UserId { get; set; }
        public bool Active { get; set; }
        // تخصص اصلی (اولین مورد ExpertiseIds)؛ برای سازگاری با اپ‌های قدیمی نگه داشته شده
        public long? ExpertiseId { get; set; }
        public ExpertiseVDto Expertise { get; set; }
        // چند تخصص/عنوان شغلی؛ در ورودی اگر خالی باشد ExpertiseId تکی استفاده می‌شود
        public System.Collections.Generic.List<long> ExpertiseIds { get; set; } = new();
        public System.Collections.Generic.List<ExpertiseVDto> Expertises { get; set; } = new();
        public CompanionVDto Companion { get; set; }
        public UserMinVDto User { get; set; }


    }
}
