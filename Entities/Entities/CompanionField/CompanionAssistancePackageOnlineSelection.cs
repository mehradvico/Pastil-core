using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class CompanionAssistancePackageOnlineSelection : Id_Field
    {
        public long CompanionAssistancePackageId { get; set; }
        public long CompanionAssistancePackageOnlineId { get; set; }
        public double Price { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public bool Deleted { get; set; }
        public CompanionAssistancePackage CompanionAssistancePackage { get; set; }
        public CompanionAssistancePackageOnline CompanionAssistancePackageOnline { get; set; }
    }
}
