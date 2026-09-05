using Application.Common.Dto.Result;
using Application.Services.Accounting.DriverSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.Accounting.DriverSrv.Dto
{
    public class DriverSearchDto : BaseSearchDto<Driver, DriverVDto>, IDriverSearchFields
    {
        public DriverSearchDto(DriverInputDto dto, IQueryable<Driver> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.OwnerId = dto.OwnerId;
            this.Vehicle = dto.Vehicle;
            this.Rate = dto.Rate;
            this.CityId = dto.CityId;
            this.NeighborhoodId = dto.NeighborhoodId;
            this.StatusId = dto.StatusId;
            this.Approved = dto.Approved;
            this.VehicleTypeId = dto.VehicleTypeId;
        }
        public long? OwnerId { get; set; }
        public string Vehicle { get; set; }
        public int? Rate { get; set; }
        public long? CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public long? StatusId { get; set; }
        public bool? Approved { get; set; }
        public long? VehicleTypeId { get; set; }
    }
}
