using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities
{
    public class CompanionInstantCallRequest : Id_Field
    {
        public long CompanionAssistancePackageOnlineSelectionId { get; set; }
        public long BookerId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Deleted { get; set; }
        public CompanionAssistancePackageOnlineSelection CompanionAssistancePackageOnlineSelection { get; set; }
        public User Booker { get; set; }
    }
}
