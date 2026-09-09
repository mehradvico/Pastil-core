using Entities.Entities.CommonField;
using System;

namespace Entities.Entities.SchoolField
{
    // یک جلسه‌ی زمان‌بندی‌شده‌ی واقعی از یک دوره‌ی زنده (Live) - تاریخ مشخص، نه یک الگوی هفتگی
    // انتزاعی، چون باید بشود دقیقاً سرِ همین زمان پوش «کلاس شروع شد» فرستاد (نگاه کنید به
    // StartingPushSentDate که جلوی ارسال تکراری آن را می‌گیرد).
    public class SchoolCourseSession : Id_Field
    {
        public long SchoolCourseId { get; set; }
        public DateTime SessionDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string MeetingUrl { get; set; }
        public DateTime? StartingPushSentDate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public SchoolCourse SchoolCourse { get; set; }
    }
}
