using Application.Common.Interface;
using Application.Services.LocationFields.LocationSrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.NeighborhoodSrv.Iface
{
    public interface INeighborhoodService : ICommonSrv<Neighborhood, NeighborhoodDto>
    {
        NeighborhoodSearchDto Search(NeighborhoodInputDto inputdto);
        Task<LocationBoundaryVDto> FindBoundaryAsync(long id);
    }
}
