using Application.Common.Interface;
using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using Entities.Entities;

namespace Application.Services.LocationFields.NeighborhoodSrv.Iface
{
    public interface INeighborhoodService : ICommonSrv<Neighborhood, NeighborhoodDto>
    {
        NeighborhoodSearchDto Search(NeighborhoodInputDto inputdto);
    }
}
