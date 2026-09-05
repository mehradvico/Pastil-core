using Application.Common.Dto.Input;
using Application.Common.Dto.LocationPoint;
using Application.Services.TripSrv.TripSrv.Iface;
using System;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    public class TripInputDto : BaseInputDto, ITripSearchFields
    {
        public long? FromCityId { get; set; }
        public long? PassengerId { get; set; }
        public long? DriverId { get; set; }
        public long? DriverStatusId { get; set; }
        public long? TripStatusId { get; set; }
        public long? VehicleTypeId { get; set; }
        public bool? ManualPay { get; set; }
        public bool? IsPaid { get; set; }
        public PointDto Point { get; set; } = null;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double? ToMinute { get; set; }

    }
}
