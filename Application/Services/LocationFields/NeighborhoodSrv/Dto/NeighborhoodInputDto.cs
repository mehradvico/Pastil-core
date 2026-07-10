using Application.Common.Dto.Input;
using Application.Services.LocationFields.NeighborhoodSrv.Iface;

namespace Application.Services.LocationFields.NeighborhoodSrv.Dto
{
    public class NeighborhoodInputDto : BaseInputDto, INeighborhoodSearchFields
    {
        public long? CityId { get; set; }
        public long? StateId { get; set; }
    }
}
