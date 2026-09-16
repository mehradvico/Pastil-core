using Application.Common.Dto.LocationPoint;
using System.Collections.Generic;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    /// <summary>
    /// درخواست ساخت سفر پت‌رسانِ متصل به یک رزرو پانسیون (تحویل پت هنگام ورود به هتل/مهد).
    /// راننده در لحظه‌ی ثبت انتخاب نمی‌شود؛ دقیقاً مثل TripReservationCreateDto (حالت دو).
    /// </summary>
    public class TripPansionReservationCreateDto
    {
        public long PansionReserveId { get; set; }
        public PointDto Origin { get; set; }
        public PointDto Destination { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        /// <summary>فاصله‌ی زمانی حرکت راننده قبل از لحظه‌ی تحویل — فقط ۶۰ یا ۱۲۰ دقیقه.</summary>
        public int ScheduledLeadMinutes { get; set; }

        public bool OwnerRidesAlong { get; set; }
        public List<long> UserPetIds { get; set; } = new List<long>();

        /// <summary>
        /// فقط برای پانسیون‌های شبانه/اقامتی (Pansion.IsDaycare == false) لازم است، چون FromDate
        /// آن‌ها فقط تاریخ دارد (بدون ساعت). قالب "HH:mm". برای پانسیون‌های روزانه/مهدی نادیده گرفته
        /// می‌شود چون StartTime واقعی رزرو خودش این نقش را دارد.
        /// </summary>
        public string DropOffTime { get; set; }
    }
}
