using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities.ClinicField
{
    /// <summary>خدمتی که یک کلینیک ارائه می‌دهد (از کاتالوگ Assistance). جایگزین CompanionAssistance برای کلینیک.</summary>
    public class ClinicService : Id_Field
    {
        public long ClinicId { get; set; }
        public long AssistanceId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public bool Deleted { get; set; }
        public decimal CommissionPercent { get; set; }
        /// <summary>مدت‌زمان استاندارد انجام خدمت (دقیقه)؛ ۰ = تعریف‌نشده.</summary>
        public int DurationMinutes { get; set; }
        public string Description { get; set; }

        public Clinic Clinic { get; set; }
        public Assistance Assistance { get; set; }
        public ICollection<ClinicServicePackage> Packages { get; set; }
        public ICollection<ClinicServiceUser> ServiceUsers { get; set; }
    }
}
