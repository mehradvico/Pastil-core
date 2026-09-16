using Application.Common.Dto.LocationPoint;
using System.Collections.Generic;

namespace Application.Services.TripSrv.PetResanServiceSrv.Dto
{
    public class PetResanServiceCreateDto
    {
        public long UserPetId { get; set; }
        public PointDto Origin { get; set; }
        public PointDto Destination { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        /// <summary>null یا خالی یعنی بی‌نهایت/باز، تا خودِ کاربر لغو کند.</summary>
        public int? TotalWeeks { get; set; }

        public List<PetResanServiceScheduleDto> Schedules { get; set; } = new List<PetResanServiceScheduleDto>();
    }
}
