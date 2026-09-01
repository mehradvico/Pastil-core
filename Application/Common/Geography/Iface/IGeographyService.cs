using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Result;
using Application.Common.Geography.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Common.Geography.Iface
{
    public interface IGeographyService
    {
        Task<double> GetDrivingDistanceAsync(PointDto start, PointDto end, bool kmResult = true, bool roundResult = true);
        Task<BaseResultDto<List<MapIrResultDto>>> SearchAsync(string q);
        Task<List<PointDto>> GetDrivingRouteAsync(PointDto start, PointDto end);
    }
}
