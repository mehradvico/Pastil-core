using Application.Common.Dto.LocationPoint;
using System.Collections.Generic;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    /// <summary>درخواست ساخت سفر پت‌رسان متصل به رزرو پرداخت‌شده‌ی آرایشگاه.</summary>
    public class TripGroomerReservationCreateDto
    {
        public long GroomerReserveId { get; set; }
        public PointDto Origin { get; set; }
        public PointDto Destination { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        /// <summary>فاصله‌ی حرکت راننده تا زمان نوبت؛ فقط ۶۰ یا ۱۲۰ دقیقه.</summary>
        public int ScheduledLeadMinutes { get; set; }
        public bool OwnerRidesAlong { get; set; }
        public List<long> UserPetIds { get; set; } = new List<long>();
        public List<long> TripOptionIds { get; set; } = new List<long>();
    }
}
