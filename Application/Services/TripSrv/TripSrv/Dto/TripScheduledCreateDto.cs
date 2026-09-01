using Application.Common.Dto.LocationPoint;
using System;
using System.Collections.Generic;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    /// <summary>
    /// درخواست ساخت سفر پت‌رسانِ «تاریخ‌دار» (حالت سه — نه لحظه‌ای، نه متصل به رزرو کلینیک).
    /// کاربر خودش یک تاریخ/ساعت دلخواه انتخاب می‌کند؛ درست مثل سفر لحظه‌ای به همه‌ی راننده‌ها
    /// Broadcast می‌شود، فقط راننده‌ی پذیرنده باید در همون زمانِ مشخص‌شده در مبدا حاضر بشه.
    /// </summary>
    public class TripScheduledCreateDto
    {
        public PointDto Origin { get; set; }
        public PointDto Destination { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        /// <summary>تاریخ/ساعتِ دقیقی که راننده باید در مبدا حاضر باشه — باید از الان به بعد باشه.</summary>
        public DateTime ScheduledDepartureAt { get; set; }

        public bool RoundTrip { get; set; }
        public bool OwnerRidesAlong { get; set; }
        public List<long> UserPetIds { get; set; } = new List<long>();
        public List<long> TripOptionIds { get; set; } = new List<long>();
    }
}
