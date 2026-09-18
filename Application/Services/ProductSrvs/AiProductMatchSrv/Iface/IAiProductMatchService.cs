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

        // onBatchProgress اختیاری است (پیش‌فرض null) — فقط مسیر Job-based ازش استفاده می‌کنه تا وضعیت
        // پیشرفت رو در AiProductMatchJobStore گزارش بده؛ endpoint همزمان (/analyze) بدون تغییر رفتار قبلی می‌مونه.
        Task<BaseResultDto<AiProductMatchAnalyzeResultDto>> AnalyzeAsync(
            long storeId,
            AiProductMatchAnalyzeInputDto dto,
            string authorizationHeaderValue,
            CancellationToken cancellationToken,
            Action<int, int> onBatchProgress = null);
    }
}
