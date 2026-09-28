using Entities.Entities.CommonField;

namespace Entities.Entities.PetResanServiceField
{
    /// <summary>
    /// یک روز هفته + ساعت مشخص در یک PetResanService. فرمت Time/ReturnTime دقیقاً مثل CompanionTime.StartTime، "HH:mm".
    /// - ReturnTime = null: رفتار قدیمی — هر هفته همین روز/ساعت یک سفر «رفت‌وبرگشت» با قیمت رفت‌وبرگشت ساخته می‌شود
    ///   (PetResanService.RoundTrip، بدون سفر برگشت واقعی جداگانه).
    /// - ReturnTime پر باشد: دو سفر یک‌طرفه‌ی جدا ساخته می‌شود — رفت در Time (مبدا→مقصد) و برگشت در ReturnTime
    ///   (مقصد→مبدا، Trip.IsReturnLeg=true)، هرکدام با نرخ ساعت خودشان. رفع باگ: قبلاً «رفت‌وبرگشت» فقط ضریب
    ///   قیمت بود و برگشت هیچ سفر/ساعت واقعی نداشت.
    /// </summary>
    public class PetResanServiceSchedule : Id_Field
    {
        public long PetResanServiceId { get; set; }
        public long WeekDayId { get; set; }
        public string Time { get; set; }
        public string ReturnTime { get; set; }
        public bool Active { get; set; }

        public PetResanService PetResanService { get; set; }
        public WeekDay WeekDay { get; set; }
    }
}
