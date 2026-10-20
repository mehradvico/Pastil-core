using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Iface
{
    public interface IProviderSubscriptionService
    {
        ProviderSubscriptionSearchDto Search(ProviderSubscriptionInputDto input);
        /// <summary>اشتراک جاری ارائه‌دهنده؛ اگر نبود Data = null.</summary>
        Task<BaseResultDto<ProviderSubscriptionVDto>> GetCurrentAsync(long providerId);
        /// <summary>اختصاص یک پلن؛ اشتراک جاری که با شروع جدید هم‌پوشانی دارد کوتاه می‌شود تا تاریخچه تمیز بماند.</summary>
        Task<BaseResultDto<ProviderSubscriptionVDto>> AssignAsync(ProviderSubscriptionDto dto, long? adminUserId);
        /// <summary>پایان‌دادن فوری به یک اشتراک (EndDate = اکنون).</summary>
        Task<BaseResultDto> CancelAsync(long id);
        Task<BaseResultDto> DeleteAsync(long id);
    }
}
