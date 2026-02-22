using Entities.Entities.CommonField;
using Entities.Entities.CompanionField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class CompanionAssistance : Id_Field
    {
        public long CompanionId { get; set; }
        public long AssistanceId { get; set; }
        public bool IsSinglePackage { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public long CompanionTypeId { get; set; }
        public bool Approved { get; set; }
        public bool Deleted { get; set; }
        public decimal CommissionPercent { get; set; }
        public Code CompanionType { get; set; }
        public Companion Companion { get; set; }
        public Assistance Assistance { get; set; }
        public ICollection<CompanionAssistanceTime> CompanionAssistanceTimes { get; set; }
        public ICollection<CompanionAssistancePackage> CompanionAssistancePackages { get; set; }
        public ICollection<CompanionAssistanceUser> CompanionAssistanceUsers { get; set; }
        public ICollection<CompanionAssistanceReport> CompanionAssistanceReports { get; set; }
        public ICollection<Code> Codes { get; set; }
    }
}
