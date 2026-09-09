using Application.Common.Dto.Field;
using Application.Services.Accounting.PetBreedSrv.Dto;
using Application.Services.Accounting.PetSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv.Dto
{
    public class SchoolCourseVDto : Name_FieldDto
    {
        public long SchoolId { get; set; }
        public string Discription { get; set; }
        public int CourseTypeId { get; set; }
        public double Price { get; set; }
        public int SessionCount { get; set; }
        public int SessionDurationMinutes { get; set; }
        public int Capacity { get; set; }
        public long? PetId { get; set; }
        public long? PetBreedId { get; set; }
        public decimal CommissionPercent { get; set; }
        public bool Active { get; set; }

        public PetVDto Pet { get; set; }
        public PetBreedVDto PetBreed { get; set; }
        public SchoolVDto School { get; set; }
        public List<SchoolCourseSessionDto> SchoolCourseSessions { get; set; }
        public List<SchoolCourseVideoDto> SchoolCourseVideos { get; set; }

        // ظرفیت باقی‌مانده - در سرویس محاسبه می‌شود (Capacity منهای تعداد ثبت‌نام‌های غیرلغوشده).
        public int RemainingCapacity { get; set; }
    }
}
