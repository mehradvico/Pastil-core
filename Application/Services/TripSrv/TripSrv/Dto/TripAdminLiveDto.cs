using Application.Common.Dto.LocationPoint;
using System;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    /// <summary>
    /// خلاصه‌ی زنده‌ی یک سفرِ فعال، برای نمایش روی نقشه‌ی ادمین (مثل اسنپ مپ).
    /// </summary>
    public class TripAdminLiveDto
    {
        public long Id { get; set; }
        public long TripStatusId { get; set; }
        public int ProgressStageId { get; set; }
        public bool IsReturnLeg { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public PointDto Origin { get; set; }
        public PointDto Destination { get; set; }

        public long? DriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverPhone { get; set; }
        public PointDto DriverLocation { get; set; }
        public DateTime? DriverLocationUpdatedAt { get; set; }

        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public PointDto UserLocation { get; set; }
        public DateTime? UserLocationUpdatedAt { get; set; }
    }
}
