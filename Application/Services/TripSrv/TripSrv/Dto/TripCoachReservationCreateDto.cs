using Application.Common.Dto.LocationPoint;
using System.Collections.Generic;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    /// <summary>درخواست سفر پت‌رسان متصل به رزرو مربی (فقط «پانسیون آموزشی در مرکز»: بردن پت تا مرکز مربی).</summary>
    public class TripCoachReservationCreateDto
    {
        public long CoachReserveId { get; set; }
        public PointDto Origin { get; set; }
        public PointDto Destination { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        /// <summary>فاصله‌ی حرکت راننده تا زمان تحویل پت؛ فقط ۶۰ یا ۱۲۰ دقیقه.</summary>
        public int ScheduledLeadMinutes { get; set; }
        public bool OwnerRidesAlong { get; set; }
        /// <summary>ساعت تحویل پت به مرکز ("HH:mm")؛ فقط وقتی رزرو ساعت ندارد (فقط تاریخ شروع) لازم است.</summary>
        public string DropOffTime { get; set; }
        public List<long> TripOptionIds { get; set; } = new List<long>();
    }
}
