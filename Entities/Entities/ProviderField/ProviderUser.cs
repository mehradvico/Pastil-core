using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.ProviderField
{
    /// <summary>
    /// کارمند پنل ارائه‌دهنده (۲۰۲۶-۱۱-۰۳): کاربری که برای یک ارائه‌دهنده کار می‌کند و با نقشی که فقط مالک ارائه‌دهنده می‌سازد
    /// (<see cref="ProviderUserRole"/>) به بخش‌هایی از پنل همان ارائه‌دهنده دسترسی دارد. با پرسنل/اپراتور مرکز
    /// (ClinicUser / GroomerUser / CoachUser) فرق دارد: آن‌ها پنل اپراتور خودشان را دارند و به پنل ارائه‌دهنده راه ندارند.
    /// </summary>
    public class ProviderUser : Id_Field
    {
        public long ProviderId { get; set; }
        public long UserId { get; set; }
        public long ProviderUserRoleId { get; set; }
        /// <summary>دعوت: null = منتظر پاسخ کاربر، true = پذیرفته، false = رد شده. فقط دعوتِ پذیرفته‌شده دسترسی می‌دهد.</summary>
        public bool? UserAccept { get; set; }
        public DateTime? InviteDate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public Provider Provider { get; set; }
        public User User { get; set; }
        public ProviderUserRole ProviderUserRole { get; set; }
    }
}
