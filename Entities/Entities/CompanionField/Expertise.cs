using Entities.Entities.CommonField;
using Entities.Entities.ProviderField;
using System.Collections.Generic;

namespace Entities.Entities.CompanionField
{
    public class Expertise : Name_Field
    {
        public int Priority { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public long? ProviderRoleId { get; set; }
        public ProviderRole ProviderRole { get; set; }

        public ICollection<CompanionUser> CompanionUsers { get; set; }
    }
}
