namespace Application.Services.TripSrv.PetResanServiceSrv.Dto
{
    public class PetResanServiceScheduleDto
    {
        public long WeekDayId { get; set; }

        /// <summary>قالب "HH:mm" — دقیقاً مثل CompanionTime.StartTime. ساعت رفت.</summary>
        public string Time { get; set; }

        /// <summary>
        /// اختیاری، قالب "HH:mm". اگر پر باشد، برای همین روز یک سفر برگشتِ واقعی و جدا (مقصد→مبدا) هم در همین
        /// ساعت ساخته می‌شود؛ باید بعد از Time باشد. خالی/نال = رفتار قدیمی (RoundTrip فقط روی قیمت اثر دارد).
        /// </summary>
        public string ReturnTime { get; set; }
    }
}
