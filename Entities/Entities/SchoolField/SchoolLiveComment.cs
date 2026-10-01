using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.SchoolField
{
    // یک کامنتِ زنده‌ی کاربر حین پخش. تاریخچه هم بعد از پایان لایو نگه داشته می‌شود (برای مرور).
    public class SchoolLiveComment : Id_Field
    {
        public long SchoolCourseLiveSessionId { get; set; }
        public long UserId { get; set; }
        public string Message { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Deleted { get; set; }

        public SchoolCourseLiveSession SchoolCourseLiveSession { get; set; }
        public User User { get; set; }
    }
}
