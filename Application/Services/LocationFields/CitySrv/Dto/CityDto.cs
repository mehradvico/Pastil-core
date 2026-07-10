using Application.Common.Dto.Field;
using Application.Services.LocationFields.StateSrv.Dto;
using System.Runtime.Serialization;

namespace Application.Services.LocationFields.CitySrv.Dto
{
    public class CityDto : Name_FieldDto
    {
        public int StateId { get; set; }
        [IgnoreDataMember]
        public StateDto State { get; set; }
    }
}
