using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.SchoolField
{
    // ویژگی‌های تکمیلی دوره‌ی مدرسه (۲۰۲۶-۱۰-۲۸): لیست انتظار، حضور و غیاب، کارنامه‌ی مهارت، گواهی، مدرس‌ها.

    /// <summary>لیست انتظار دوره‌ی پر: وقتی جایی خالی شد، به‌ترتیب ثبت به نفرات پیام داده می‌شود.</summary>
    public class SchoolCourseWaitlist : Id_Field
    {
        public long SchoolCourseId { get; set; }
        public long UserId { get; set; }
        public long UserPetId { get; set; }
        public DateTime CreateDate { get; set; }
        // آخرین باری که «جا خالی شد» برای این نفر فرستاده شد (null = هنوز نه)
        public DateTime? NotifiedDate { get; set; }
        public bool Deleted { get; set; }

        public SchoolCourse SchoolCourse { get; set; }
        public User User { get; set; }
        public UserPet UserPet { get; set; }
    }

    /// <summary>حضور و غیاب یک ثبت‌نام در یک جلسه (یکتا روی جلسه+ثبت‌نام).</summary>
    public class SchoolAttendance : Id_Field
    {
        public long SchoolCourseSessionId { get; set; }
        public long SchoolReserveId { get; set; }
        public bool Present { get; set; }
        public string Note { get; set; }
        public long RecordedByUserId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        // پیام «جلسه را از دست دادی» (یک‌بار)
        public DateTime? MissedPushSentDate { get; set; }

        public SchoolCourseSession SchoolCourseSession { get; set; }
        public SchoolReserve SchoolReserve { get; set; }
    }

    /// <summary>مهارتی که در کارنامه‌ی پیشرفت پت‌های این دوره نمره می‌گیرد (مثلاً «نشستن با فرمان»).</summary>
    public class SchoolCourseSkill : Id_Field
    {
        public long SchoolCourseId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public bool Deleted { get; set; }

        public SchoolCourse SchoolCourse { get; set; }
    }

    /// <summary>نمره‌ی یک مهارت برای یک ثبت‌نام (اختیاری: در یک جلسه‌ی مشخص). Score: ۱ تا ۵.</summary>
    public class SchoolProgress : Id_Field
    {
        public long SchoolReserveId { get; set; }
        public long SchoolCourseSkillId { get; set; }
        public long? SchoolCourseSessionId { get; set; }
        public int Score { get; set; }
        public string Note { get; set; }
        public long RecordedByUserId { get; set; }
        public DateTime CreateDate { get; set; }

        public SchoolReserve SchoolReserve { get; set; }
        public SchoolCourseSkill SchoolCourseSkill { get; set; }
        public SchoolCourseSession SchoolCourseSession { get; set; }
    }

    /// <summary>گواهی پایان دوره برای یک ثبت‌نام (یکتا)؛ Code برای استعلام عمومی.</summary>
    public class SchoolCertificate : Id_Field
    {
        public long SchoolReserveId { get; set; }
        public string Code { get; set; }
        public DateTime IssueDate { get; set; }
        public long IssuedByUserId { get; set; }

        public SchoolReserve SchoolReserve { get; set; }
    }

    /// <summary>مدرس دوره (مدرس اصلی یا کمک‌مدرس). مدرس حضور و غیاب، کارنامه و کلاس زنده‌ی همان دوره را مدیریت می‌کند.</summary>
    public class SchoolCourseInstructor : Id_Field
    {
        public long SchoolCourseId { get; set; }
        public long UserId { get; set; }
        public bool IsLead { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public SchoolCourse SchoolCourse { get; set; }
        public User User { get; set; }
    }
}
