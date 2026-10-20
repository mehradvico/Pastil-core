using Entities.Entities.CommonField;

namespace Entities.Entities.ClinicField
{
    /// <summary>ساعت کاری هفتگی کلینیک و ظرفیت هر بازه (معادل CompanionTime).</summary>
    public class ClinicTime : Id_Field
    {
        public long ClinicId { get; set; }
        public long WeekDayId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Capacity { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public Clinic Clinic { get; set; }
        public WeekDay WeekDay { get; set; }
    }
}
