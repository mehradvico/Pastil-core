using Application.Common.Dto.Field;
using Application.Services.LocationFields.StateSrv.Dto;

namespace Application.Services.LocationFields.CitySrv.Dto
{
    public class CityVDto : Name_FieldDto
    {
        public long StateId { get; set; }
        public StateVDto State { get; set; }
    }
}
