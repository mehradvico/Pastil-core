using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities.ProviderField
{
    /// <summary>
    /// کاتالوگ نقش‌های ارائه‌دهنده (فروشگاه، کلینیک، مربی، آرایشگاه، پانسیون، مدرسه)؛ هم‌شکل جدول Role.
    /// هر Provider چند نقش دارد (ProviderRoleAssignment). Label کلید ثابت کد است (ProviderRoleLabels) و تغییر نمی‌کند؛ Name عنوان فارسی قابل ویرایش.
    /// </summary>
    public class ProviderRole : Id_Field
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public ICollection<ProviderRoleAssignment> Assignments { get; set; }
    }
}
