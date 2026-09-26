using Application.Services.CompanionSrvs.CompanionReserveUnifiedSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveUnifiedSrv.Iface
{
    public interface IUnifiedReserveService
    {
        Task<UnifiedReserveListDto> SearchAsync(UnifiedReserveInputDto dto);
    }
}
