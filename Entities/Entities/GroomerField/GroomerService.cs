using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities.GroomerField
{
    /// <summary>خدمتی که یک آرایشگاه ارائه می‌دهد (از کاتالوگ Assistance). جایگزین CompanionAssistance برای آرایشگاه.</summary>
    public class GroomerService : Id_Field
    {
        public long GroomerId { get; set; }
        public long AssistanceId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public bool Deleted { get; set; }
        public decimal CommissionPercent { get; set; }
        /// <summary>مدت‌زمان استاندارد انجام خدمت (دقیقه)؛ ۰ = تعریف‌نشده.</summary>
        public int DurationMinutes { get; set; }
        public string Description { get; set; }

        public Groomer Groomer { get; set; }
        public Assistance Assistance { get; set; }
        public ICollection<GroomerServicePackage> Packages { get; set; }
        public ICollection<GroomerServiceUser> ServiceUsers { get; set; }
    }
}
