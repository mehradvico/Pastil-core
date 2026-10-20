using Entities.Entities.Security;
using Entities.Entities.CommonField;

namespace Entities.Entities.CoachField
{
    /// <summary>پرسنل مربی (گرومر/اپراتور).</summary>
    public class CoachUser : Id_Field
    {
        public long CoachId { get; set; }
        public long UserId { get; set; }
        /// <summary>دعوت: null = منتظر پاسخ کاربر، true = پذیرفته، false = رد شده. فقط پرسنل پذیرفته‌شده کار می‌کنند.</summary>
        public bool? UserAccept { get; set; }
        public System.DateTime? InviteDate { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public bool Deleted { get; set; }

        public Coach Coach { get; set; }
        public User User { get; set; }
    }
}
