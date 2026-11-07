using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;

namespace Entities.Entities.ProviderField
{
    /// <summary>
    /// نقش کارمندان پنل ارائه‌دهنده. دو نوع دارد:
    /// - نقش خودِ ارائه‌دهنده: ProviderId پر، Scope خالی؛ فقط مالک همان ارائه‌دهنده می‌سازد/ویرایش می‌کند.
    /// - نقش آماده (قالب سراسری، ۲۰۲۶-۱۱-۰۷): ProviderId خالی، Scope = یکی از <see cref="ProviderRoleLabels"/> (مثلاً Clinic)؛
    ///   فقط ادمین اصلی می‌سازد و فقط به ارائه‌دهنده‌ای نمایش/اعتبار دارد که همان نمایندگی را دارد. دسترسی‌هایش فقط کلیدهای همان Scope است.
    /// دسترسی‌ها کلیدهای کاتالوگ ثابت کد هستند (ProviderPermissionCatalog)، نه ردیف‌های آزاد.
    /// </summary>
    public class ProviderUserRole : Id_Field
    {
        public long? ProviderId { get; set; }
        /// <summary>فقط برای نقش آماده: نمایندگیِ مادر (Store/Clinic/Groomer/Coach/Pansion/School).</summary>
        public string Scope { get; set; }
        /// <summary>ترتیب نمایش نقش‌های آماده (کمتر = بالاتر).</summary>
        public int Priority { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public bool IsTemplate => ProviderId == null;

        public Provider Provider { get; set; }
        public ICollection<ProviderUserRolePermission> Permissions { get; set; }
        public ICollection<ProviderUser> Users { get; set; }
    }
}
