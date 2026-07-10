using Application.Common.Dto.Input;
using Application.Services.LocationFields.CitySrv.Iface;

namespace Application.Services.LocationFields.CitySrv.Dto
{
    public class CityInputDto : BaseInputDto, ICitySearchFields
    {
        public long StateId { get; set; }
    }
}
