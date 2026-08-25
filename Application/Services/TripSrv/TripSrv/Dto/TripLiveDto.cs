using Application.Common.Dto.LocationPoint;
using System;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    /// <summary>
    /// خروجی سبک برای Polling لوکیشن زنده در طول یک سفر فعال — فقط وضعیت فعلی و
    /// آخرین موقعیت شناخته‌شده‌ی طرف مقابل، بدون کل جزئیات Trip.
    /// </summary>
    public class TripLiveDto
    {
        public long TripStatusId { get; set; }
        public int ProgressStageId { get; set; }
        public bool IsReturnLeg { get; set; }
        public PointDto CounterpartLocation { get; set; }
        public DateTime? CounterpartLocationUpdatedAt { get; set; }
    }
}
