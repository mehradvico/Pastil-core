using Application.Common.Dto.LocationPoint;
using Application.Services.TripSrv.TripOptionSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.TripSrv.PetResanServiceSrv.Dto
{
    public class PetResanServiceVDto
    {
        public long Id { get; set; }
        public long UserPetId { get; set; }
        public string UserPetName { get; set; }
        public PointDto Origin { get; set; }
        public PointDto Destination { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public DateTime StartDate { get; set; }
        public int? TotalWeeks { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Active { get; set; }
        public double PricePerOccurrence { get; set; }
        public List<TripOptionVDto> TripOptions { get; set; } = new List<TripOptionVDto>();
        public List<PetResanServiceScheduleVDto> Schedules { get; set; } = new List<PetResanServiceScheduleVDto>();
    }
}
