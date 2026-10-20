using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementSrv.Iface
{
    public interface ISettlementService : ICommonSrv<Settlement, SettlementDto>
    {
        SettlementSearchDto Search(SettlementInputDto baseSearchDto);
        Task<BaseResultDto<SettlementVDto>> FindAsyncVDto(long id);

        /// <summary>ریز تسویه: همه‌ی ردیف‌هایی که در این تسویه پرداخت شده‌اند (نمایندگی/فروشگاه/آرایشگاه).</summary>
        Task<BaseResultDto<List<SettlementItemVDto>>> GetItemsAsync(long settlementId);

        /// <summary>
        /// تاریخچه‌ی تسویه برای اپ نماینده/فروشنده: تسویه‌ی ارائه‌دهنده‌ی همان نمایندگی/فروشگاه و همان استخر
        /// (ردیف‌های قدیمی که ارائه‌دهنده ندارند با شناسه‌ی خودِ مکان). venueId خالی = لیست خالی.
        /// </summary>
        Task<SettlementSearchDto> SearchForVenueAsync(SettlementInputDto input, Entities.Entities.SettlementKindEnum kind, long? venueId);

        /// <summary>تاریخچه‌ی تسویه‌ی ارائه‌دهنده‌ی خودِ کاربر در یک استخر (برای مالک آرایشگاه که چند آرایشگاه دارد). کاربر بدون ارائه‌دهنده = لیست خالی.</summary>
        Task<SettlementSearchDto> SearchForProviderUserAsync(SettlementInputDto input, Entities.Entities.SettlementKindEnum kind, long userId);

        Task<bool> BelongsToProviderUserAsync(long settlementId, Entities.Entities.SettlementKindEnum kind, long userId);

        /// <summary>آیا این تسویه مال ارائه‌دهنده‌ی (یا ردیف قدیمیِ) همین نمایندگی/فروشگاه است؟</summary>
        Task<bool> BelongsToVenueAsync(long settlementId, Entities.Entities.SettlementKindEnum kind, long? venueId);
    }
}
