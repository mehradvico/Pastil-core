using Application.Common.Dto.Field;
using Application.Services.LocationFields.CountrySrv.Dto;

namespace Application.Services.LocationFields.StateSrv.Dto
{
    public class StateVDto : Name_FieldDto
    {
        public string EnName { get; set; }
        public long CountryId { get; set; }
        public CountryVDto Country { get; set; }
    }
}
