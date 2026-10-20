using Entities.Entities.CommonField;

namespace Entities.Entities.CoachField
{
    /// <summary>ساعت کاری هفتگی مربی و ظرفیت هر بازه (معادل CompanionTime).</summary>
    public class CoachTime : Id_Field
    {
        public long CoachId { get; set; }
        public long WeekDayId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Capacity { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public Coach Coach { get; set; }
        public WeekDay WeekDay { get; set; }
    }
}
