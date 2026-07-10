using Application.Common.Dto.Field;
using Application.Services.LocationFields.CitySrv.Dto;
using Entities.Entities.CommonField;

namespace Application.Services.LocationFields.NeighborhoodSrv.Dto
{
    public class NeighborhoodVDto : Name_FieldDto
    {
        public int RegionNumber { get; set; }
        public long CityId { get; set; }
        public CityVDto City { get; set; }
    }
}
