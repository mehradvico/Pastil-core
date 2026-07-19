using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.LocationSrv.Dto;
using Entities.Entities.LocationField;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.CitySrv.Iface
{
    public interface ICityService : ICommonSrv<City, CityDto>
    {
        BaseSearchDto<CityVDto> Search(CityInputDto baseSearchDto);
        Task<LocationBoundaryVDto> FindBoundaryAsync(long id);
        BaseResultDto GetAll();
    }
}
