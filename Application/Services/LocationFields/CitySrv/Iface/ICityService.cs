using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.LocationFields.CitySrv.Dto;
using Entities.Entities.LocationField;

namespace Application.Services.LocationFields.CitySrv.Iface
{
    public interface ICityService : ICommonSrv<City, CityDto>
    {
        BaseSearchDto<CityVDto> Search(CityInputDto baseSearchDto);
        BaseResultDto GetAll();
    }
}
