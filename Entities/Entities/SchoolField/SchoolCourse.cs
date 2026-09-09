using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities.SchoolField
{
    // یک دوره‌ی چندجلسه‌ای مدرسه (مثلاً «تربیت پایه سگ»، ۸ جلسه‌ی ۹۰ دقیقه‌ای).
    // CourseTypeId: Application.Common.Enumerable.Code.SchoolCourseTypeEnum
    //   Video = محتوای ضبط‌شده (SchoolCourseVideo) که کاربر بعد از خرید هر زمان می‌بیند.
    //   Live  = کلاس زنده با جلسات از پیش زمان‌بندی‌شده (SchoolCourseSession) و لینک اتصال (Adobe Connect).
    public class SchoolCourse : Name_Field
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
        public bool Deleted { get; set; }

        public School School { get; set; }
        public Pet Pet { get; set; }
        public PetBreed PetBreed { get; set; }
        public ICollection<SchoolCourseSession> SchoolCourseSessions { get; set; }
        public ICollection<SchoolCourseVideo> SchoolCourseVideos { get; set; }
        public ICollection<SchoolReserve> SchoolReserves { get; set; }
    }
}
