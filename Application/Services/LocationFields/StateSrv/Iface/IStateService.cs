using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.LocationFields.LocationSrv.Dto;
using Application.Services.LocationFields.StateSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.StateSrv.Iface
{
    public interface IStateService : ICommonSrv<State, StateDto>
    {
        BaseSearchDto<StateVDto> Search(StateInputDto baseSearchDto);
        Task<LocationBoundaryVDto> FindBoundaryAsync(long id);
    }
}
