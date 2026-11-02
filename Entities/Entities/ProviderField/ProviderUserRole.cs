using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;

namespace Entities.Entities.ProviderField
{
    /// <summary>
    /// نقشی که مالک ارائه‌دهنده برای کارمندانش می‌سازد (مثلاً «مسئول کلینیک»، «حسابدار کلینیک»). هر نقش فقط مال همان ارائه‌دهنده است
    /// و دسترسی‌هایش کلیدهای کاتالوگ ثابت کد هستند (ProviderPermissionCatalog)، نه ردیف‌های آزاد.
    /// </summary>
    public class ProviderUserRole : Id_Field
    {
        public long ProviderId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public Provider Provider { get; set; }
        public ICollection<ProviderUserRolePermission> Permissions { get; set; }
        public ICollection<ProviderUser> Users { get; set; }
    }
}
