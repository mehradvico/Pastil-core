using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities.CoachField
{
    /// <summary>خدمتی که یک مربی ارائه می‌دهد (از کاتالوگ Assistance). جایگزین CompanionAssistance برای مربی.</summary>
    public class CoachService : Id_Field
    {
        public long CoachId { get; set; }
        public long AssistanceId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public bool Deleted { get; set; }
        public decimal CommissionPercent { get; set; }
        /// <summary>مدت‌زمان استاندارد انجام خدمت (دقیقه)؛ ۰ = تعریف‌نشده.</summary>
        public int DurationMinutes { get; set; }
        public string Description { get; set; }

        public Coach Coach { get; set; }
        public Assistance Assistance { get; set; }
        public ICollection<CoachServicePackage> Packages { get; set; }
        public ICollection<CoachServiceUser> ServiceUsers { get; set; }
    }
}
