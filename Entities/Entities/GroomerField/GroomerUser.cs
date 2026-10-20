using Entities.Entities.Security;
using Entities.Entities.CommonField;

namespace Entities.Entities.GroomerField
{
    /// <summary>پرسنل آرایشگاه (گرومر/اپراتور).</summary>
    public class GroomerUser : Id_Field
    {
        public long GroomerId { get; set; }
        public long UserId { get; set; }
        /// <summary>دعوت: null = منتظر پاسخ کاربر، true = پذیرفته، false = رد شده. فقط پرسنل پذیرفته‌شده کار می‌کنند.</summary>
        public bool? UserAccept { get; set; }
        public System.DateTime? InviteDate { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public bool Deleted { get; set; }

        public Groomer Groomer { get; set; }
        public User User { get; set; }
    }
}
