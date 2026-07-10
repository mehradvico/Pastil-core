using Application.Common.Dto.Result;
using Application.Services.LocationFields.CitySrv.Iface;
using AutoMapper;
using Entities.Entities.LocationField;
using System.Linq;

namespace Application.Services.LocationFields.CitySrv.Dto
{
    public class CitySearchDto : BaseSearchDto<City, CityVDto>, ICitySearchFields
    {
        public CitySearchDto(CityInputDto dto, IQueryable<City> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.StateId = dto.StateId;
        }


        public long StateId { get; set; }
    }
}
