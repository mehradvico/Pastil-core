using Application.Common.Dto.LocationPoint;
using System.Collections.Generic;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    /// <summary>
    /// درخواست ساخت سفر پت‌رسانِ متصل به یک رزرو (حالت دو — سفر لحظه‌ای نیست).
    /// راننده در لحظه‌ی ثبت انتخاب نمی‌شود؛ Job زمان‌بندی‌شده در لحظه‌ی مقرر آن را فعال می‌کند.
    /// </summary>
    public class TripReservationCreateDto
    {
        public long CompanionReserveId { get; set; }
        public PointDto Origin { get; set; }
        public PointDto Destination { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        /// <summary>فاصله‌ی زمانی حرکت راننده قبل از موعد رزرو — فقط ۶۰ یا ۱۲۰ دقیقه.</summary>
        public int ScheduledLeadMinutes { get; set; }

        public bool OwnerRidesAlong { get; set; }
        public List<long> UserPetIds { get; set; } = new List<long>();
    }
}
