using Application.Common.Dto.LocationPoint;
using System.Collections.Generic;

namespace Application.Services.TripSrv.PetResanServiceSrv.Dto
{
    public class PetResanServiceCreateDto
    {
        // فهرست همه‌ی پت‌های سرویس (چند پت مجاز). خالی/نال ⇒ به UserPetId (قدیمی، یک پت) برمی‌گردیم؛
        // اگر هر دو پر باشند UserPetIds اولویت دارد. همان قرارداد Trip.UserPetIds/UserPetId.
        public List<long> UserPetIds { get; set; } = new List<long>();
        public long UserPetId { get; set; }
        public PointDto Origin { get; set; }
        public PointDto Destination { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        /// <summary>null یا خالی یعنی بی‌نهایت/باز، تا خودِ کاربر لغو کند.</summary>
        public int? TotalWeeks { get; set; }

        /// <summary>رفت‌وبرگشت؟ پیش‌فرض true (رفتار قبلی سرویس هفتگی)؛ false = فقط رفت. فرمول رفت‌وبرگشت همان PriceCalculationService (+۵۰٪ کل هزینه).</summary>
        public bool RoundTrip { get; set; } = true;

        public List<long> TripOptionIds { get; set; } = new List<long>();
        public List<PetResanServiceScheduleDto> Schedules { get; set; } = new List<PetResanServiceScheduleDto>();
    }
}
