using Application.Common.Dto.Result;
using Application.Services.LocationFields.NeighborhoodSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.LocationFields.NeighborhoodSrv.Dto
{
    public class NeighborhoodSearchDto : BaseSearchDto<Neighborhood, NeighborhoodVDto>, INeighborhoodSearchFields
    {
        public NeighborhoodSearchDto(NeighborhoodInputDto dto, IQueryable<Neighborhood> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.CityId = dto.CityId;
            this.StateId = dto.StateId;
        }

        public long? CityId { get; set; }
        public long? StateId { get; set; }

    }
}
