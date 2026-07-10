using Application.Common.Dto.Result;
using Application.Services.LocationFields.ParkSrv.Dto;
using Application.Services.LocationFields.ParkSrv.Iface;
using AutoMapper;
using Entities.Entities.LocationField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkSrv.Dto
{
    public class ParkSearchDto : BaseSearchDto<Park, ParkVDto>, IParkSearchFields
    {
        public ParkSearchDto(ParkInputDto dto, IQueryable<Park> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.NeighborhoodId = dto.NeighborhoodId;
            this.CityId = dto.CityId;
            this.StateId = dto.StateId;
        }
        public long? NeighborhoodId { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }
    }
}
