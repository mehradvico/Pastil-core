using Entities.Entities.CommonField;

namespace Entities.Entities.GroomerField
{
    /// <summary>ساعت کاری هفتگی آرایشگاه و ظرفیت هر بازه (معادل CompanionTime).</summary>
    public class GroomerTime : Id_Field
    {
        public long GroomerId { get; set; }
        public long WeekDayId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Capacity { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public Groomer Groomer { get; set; }
        public WeekDay WeekDay { get; set; }
    }
}
