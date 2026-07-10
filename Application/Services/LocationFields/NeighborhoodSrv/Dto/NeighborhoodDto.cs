using Application.Common.Dto.Field;
using Entities.Entities.CommonField;

namespace Application.Services.LocationFields.NeighborhoodSrv.Dto
{
    public class NeighborhoodDto : Name_FieldDto
    {
        public int RegionNumber { get; set; }
        public long CityId { get; set; }
    }
}
