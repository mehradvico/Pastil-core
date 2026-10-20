using Entities.Entities.CommonField;

namespace Entities.Entities.ProviderField
{
    /// <summary>یک نقش (ProviderRole) که به یک ارائه‌دهنده داده شده؛ تأیید هر نقش جداست.</summary>
    public class ProviderRoleAssignment : Id_Field
    {
        public long ProviderId { get; set; }
        public long ProviderRoleId { get; set; }
        public bool Approved { get; set; }
        public bool Deleted { get; set; }

        public Provider Provider { get; set; }
        public ProviderRole ProviderRole { get; set; }
    }
}
