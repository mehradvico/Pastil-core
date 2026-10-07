using Entities.Entities.CommonField;
using Entities.Entities.CompanionField;
using Entities.Entities.LocationField;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace Entities.Entities.SchoolField
{
    // مدرسه‌ی تربیت پت - همیشه توسط یک «مربی» (Companion) اداره می‌شود؛ کاتالوگ دوره‌ها
    // (SchoolCourse) روی همین رکورد تعریف می‌شوند، نه یک قیمت تخت مثل Pansion.
    public class School : Name_Field
    {
        public long CompanionId { get; set; }
        public bool Active { get; set; }
        public bool ShowToSite { get; set; }
        public bool Approve { get; set; }
        public string ApprovalValue { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public string Discription { get; set; }
        public string AddressValue { get; set; }
        // موقعیت دقیق خود مدرسه روی نقشه (نه موقعیت مربی)
        public Point Location { get; set; }
        public int CommentCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public long? PictureId { get; set; }
        public bool Suggested { get; set; }
        // حذف نرم: ردیف پاک نمی‌شود (رزروها و گزارش مالی می‌مانند) ولی از همه‌ی فهرست‌ها، جستجو و رزرو جدید حذف می‌شود.
        // حذف توسط مالک کلینیک یا ادمین؛ فقط ادمین می‌تواند بازگرداند.
        public bool Deleted { get; set; }
        public System.DateTime? DeleteDate { get; set; }
        public long? DeletedByUserId { get; set; }
        public string Regulations { get; set; }

        public Companion Companion { get; set; }
        public State State { get; set; }
        public City City { get; set; }
        public Picture Picture { get; set; }
        public ICollection<SchoolCourse> SchoolCourses { get; set; }
        public ICollection<SchoolComment> SchoolComments { get; set; }
        public ICollection<SchoolPicture> SchoolPictures { get; set; }
    }
}
