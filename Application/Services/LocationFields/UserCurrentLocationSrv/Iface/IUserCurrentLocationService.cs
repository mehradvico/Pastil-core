using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.LocationFields.UserCurrentLocationSrv.Dto;
using Entities.Entities.Security;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.UserCurrentLocationSrv.Iface
{
    public interface IUserCurrentLocationService : ICommonSrv<UserCurrentLocation, UserCurrentLocationDto>
    {
        UserCurrentLocationSearchDto Search(UserCurrentLocationInputDto inputDto);
        Task<BaseResultDto<UserCurrentLocationDto>> SetAsyncDto(long userId, SetUserCurrentLocationDto dto);
    }
}
