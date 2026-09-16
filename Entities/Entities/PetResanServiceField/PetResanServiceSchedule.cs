using Entities.Entities.CommonField;

namespace Entities.Entities.PetResanServiceField
{
    /// <summary>
    /// یک روز هفته + ساعت مشخص در یک PetResanService — هر ردیف یعنی «هر هفته، این روز، این ساعت
    /// یک سفر رفت‌وبرگشت ساخته شود». فرمت Time دقیقاً مثل CompanionTime.StartTime، "HH:mm".
    /// </summary>
    public class PetResanServiceSchedule : Id_Field
    {
        public long PetResanServiceId { get; set; }
        public long WeekDayId { get; set; }
        public string Time { get; set; }
        public bool Active { get; set; }

        public PetResanService PetResanService { get; set; }
        public WeekDay WeekDay { get; set; }
    }
}
