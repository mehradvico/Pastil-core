using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.DiscountSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.DiscountSrv.IFace
{
    public interface IDiscountService
    {
        /// <param name="byAdmin">فقط ادمین می‌تواند تخفیفِ «سهم پاستیل» (FundedByPastil) بسازد/تغییر دهد؛ برای فروشنده همیشه false.</param>
        Task<BaseResultDto> InsertAsync(DiscountDto discount, bool byAdmin = false);
        Task<BaseResultDto> ActiveAsync(DiscountDto discount, bool byAdmin = false);
        Task<BaseResultDto> DeleteAsync(DiscountDto discount, bool byAdmin = false);
        Task<BaseResultDto> FindAsyncDto(long id);
        DiscountSearchDto Search(DiscountInputDto searchDto);
        Task SyncExpiredAsync();

    }
}
