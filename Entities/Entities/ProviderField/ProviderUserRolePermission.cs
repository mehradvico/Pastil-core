using Entities.Entities.CommonField;

namespace Entities.Entities.ProviderField
{
    /// <summary>یک کلید دسترسی (مثل «Clinic.Finance.View») که به نقش کارمندان ارائه‌دهنده داده شده.</summary>
    public class ProviderUserRolePermission : Id_Field
    {
        public long ProviderUserRoleId { get; set; }
        public string PermissionKey { get; set; }

        public ProviderUserRole ProviderUserRole { get; set; }
    }
}
