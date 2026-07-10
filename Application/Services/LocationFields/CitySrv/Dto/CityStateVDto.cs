using Application.Common.Dto.Field;

namespace Application.Services.LocationFields.CitySrv.Dto
{
    public class CityStateVDto : Name_FieldDto
    {
        public long StateId { get; set; }
        public string StateName { get; set; }
        public string FullName { get; set; }
    }
}
