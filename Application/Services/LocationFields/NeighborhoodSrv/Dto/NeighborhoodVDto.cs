using Application.Services.LocationFields.CitySrv.Dto;
using Entities.Entities.CommonField;

namespace Application.Services.LocationFields.NeighborhoodSrv.Dto
{
    public class NeighborhoodVDto : Name_Field
    {
        public long CityId { get; set; }

        public CityVDto City { get; set; }

    }
}
