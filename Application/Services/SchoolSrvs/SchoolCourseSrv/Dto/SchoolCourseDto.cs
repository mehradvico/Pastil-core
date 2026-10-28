using Application.Common.Dto.Field;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv.Dto
{
    public class SchoolCourseDto : Name_FieldDto
    {
        public long SchoolId { get; set; }
        public string Discription { get; set; }
        public int CourseTypeId { get; set; }
        public double Price { get; set; }
        public int SessionCount { get; set; }
        public int SessionDurationMinutes { get; set; }
        // null = ظرفیت نامحدود
        public int? Capacity { get; set; }
        public long? PetId { get; set; }
        public long? PetBreedId { get; set; }
        public decimal CommissionPercent { get; set; }
        // ---- ویژگی‌های تکمیلی (۲۰۲۶-۱۰-۲۸)
        // SchoolCourseLevelEnum: ۰ نامشخص، ۱ مقدماتی، ۲ متوسط، ۳ پیشرفته
        public int Level { get; set; }
        // دوره‌ای که همین پت باید قبلاً «کامل‌شده» داشته باشد (از همین مدرسه)
        public long? PrerequisiteCourseId { get; set; }
        // SchoolLateEnrollmentEnum: ۰ بعد از شروع بسته، ۱ باز با قیمت کامل، ۲ باز با قیمت جلسه‌های باقی‌مانده
        public int LateEnrollmentMode { get; set; }
        // سیاست لغو (null در ساخت = پیش‌فرض ۲۴ ساعت / ۵۰٪)
        public int? FullRefundHoursBeforeStart { get; set; }
        public int? LateCancelRefundPercent { get; set; }
        public bool CertificateEnabled { get; set; }
        public bool Active { get; set; }
    }
}
