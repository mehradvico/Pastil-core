using Entities.Entities.CommonField;

namespace Entities.Entities.SchoolField
{
    // یک ویدیوی آموزشی متعلق به یک دوره‌ی ضبط‌شده (Video) - فایل واقعی روی سرویس File آپلود
    // و فقط FileId اینجا نگه داشته می‌شود، دقیقاً مثل الگوی PostFile.
    public class SchoolCourseVideo : Name_Field
    {
        public long SchoolCourseId { get; set; }
        public long FileId { get; set; }
        public int SortOrder { get; set; }
        public int? DurationSeconds { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public SchoolCourse SchoolCourse { get; set; }
        public File File { get; set; }
    }
}
