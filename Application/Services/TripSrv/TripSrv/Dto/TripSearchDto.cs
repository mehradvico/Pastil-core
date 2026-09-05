using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Result;
using Application.Services.TripSrv.TripSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Linq;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    public class TripSearchDto : BaseSearchDto<Trip, TripVDto>, ITripSearchFields
    {
        public TripSearchDto(TripInputDto dto, IQueryable<Trip> list, IMapper mapper) : base(dto, list, mapper)
        {
            FromCityId = dto.FromCityId;
            PassengerId = dto.PassengerId;
            DriverId = dto.DriverId;
            DriverStatusId = dto.DriverStatusId;
            TripStatusId = dto.TripStatusId;
            VehicleTypeId = dto.VehicleTypeId;
            Point = dto.Point;
            FromDate = dto.FromDate;
            ToDate = dto.ToDate;
            ToMinute = dto.ToMinute;
            IsPaid = dto.IsPaid;
            ManualPay = dto.ManualPay;
        }

        public long? FromCityId { get; set; }
        public long? PassengerId { get; set; }
        public long? DriverId { get; set; }
        public long? DriverStatusId { get; set; }
        public long? TripStatusId { get; set; }
        public long? VehicleTypeId { get; set; }
        public bool? ManualPay { get; set; }
        public bool? IsPaid { get; set; }
        public PointDto Point { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double? ToMinute { get; set; }

    }
}
