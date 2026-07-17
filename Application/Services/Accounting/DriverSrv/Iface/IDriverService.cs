using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.DriverSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.Accounting.DriverSrv.Iface
{
    public interface IDriverService : ICommonSrv<Driver, DriverDto>
    {
        DriverSearchDto Search(DriverInputDto baseSearchDto);
        Task<BaseResultDto<DriverVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateAsyncDto(DriverDto dto);
        BaseResultDto DriverUpdateStatusDto(DriverUpdateStatusDto dto);
    }
}
