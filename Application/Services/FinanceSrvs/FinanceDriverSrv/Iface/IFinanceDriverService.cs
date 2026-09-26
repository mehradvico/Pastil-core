using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceDriverSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceDriverSrv.Iface
{
    public interface IFinanceDriverService
    {
        Task<FinanceDriverListDto> SearchAsync(FinanceDriverInputDto dto);
        Task<BaseResultDto<FinanceDriverDetailVDto>> DetailAsync(long driverId);
        Task<BaseResultDto> UpdateCommissionAsync(FinanceDriverCommissionDto dto);
    }
}
