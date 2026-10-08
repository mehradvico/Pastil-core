using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.AiProductMatchSrv.Dto;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.AiProductMatchSrv.Iface
{
    public interface IAiProductMatchService
    {
        Task<BaseResultDto<AiProductMatchStatusDto>> GetStatusAsync();
        Task<BaseResultDto<AiProductMatchAnalyzeResultDto>> AnalyzeAsync(
            long storeId,
            AiProductMatchAnalyzeInputDto dto,
            CancellationToken cancellationToken,
            Action<int, int> onBatchProgress = null);
    }
}
