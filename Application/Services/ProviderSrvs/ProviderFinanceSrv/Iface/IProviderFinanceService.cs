using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderFinanceSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderFinanceSrv.Iface
{
    /// <summary>حسابداری ارائه‌دهنده‌محور (فقط خواندن). ثبت تسویه در ISettlementService است.</summary>
    public interface IProviderFinanceService
    {
        /// <summary>لیست ارائه‌دهندگان با مانده‌ی هر استخر و هشدار نمایندگی/فروشگاه‌های بدون ارائه‌دهنده.</summary>
        Task<ProviderFinanceSearchVDto> SearchAsync(ProviderFinanceInputDto input);

        /// <summary>جمع هر استخر + تفکیک هر نمایندگی/فروشگاه + کارت‌های بانکی ارائه‌دهنده.</summary>
        Task<BaseResultDto<ProviderFinanceDetailVDto>> GetDetailAsync(long providerId);

        /// <summary>ریز ردیف‌های تسویه‌نشده‌ی یک استخر؛ دقیقاً همان‌هایی که ثبت تسویه می‌پردازد.</summary>
        Task<BaseResultDto<ProviderPendingVDto>> GetPendingAsync(long providerId, SettlementKindEnum kind);
    }
}
