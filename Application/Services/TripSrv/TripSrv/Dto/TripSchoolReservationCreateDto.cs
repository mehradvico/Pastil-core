using Application.Common.Dto.LocationPoint;
using System.Collections.Generic;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    /// <summary>پت رسان متصل به ثبت نام یک دوره حضوری؛ زمان حرکت از اولین جلسه دوره به دست می آید.</summary>
    public class TripSchoolReservationCreateDto
    {
        public long SchoolReserveId { get; set; }
        public PointDto Origin { get; set; }
        public PointDto Destination { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public int ScheduledLeadMinutes { get; set; }
        public bool OwnerRidesAlong { get; set; }
        public List<long> UserPetIds { get; set; } = new List<long>();
        public List<long> TripOptionIds { get; set; } = new List<long>();
    }
}
