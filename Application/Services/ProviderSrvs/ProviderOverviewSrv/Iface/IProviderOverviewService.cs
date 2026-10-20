using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderOverviewSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderOverviewSrv.Iface
{
    public interface IProviderOverviewService
    {
        Task<BaseResultDto<ProviderOverviewVDto>> GetAsync(long providerId);
    }
}
