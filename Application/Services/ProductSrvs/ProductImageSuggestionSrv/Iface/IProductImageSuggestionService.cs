using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductImageSuggestionSrv.Dto;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.ProductImageSuggestionSrv.Iface
{
    public interface IProductImageSuggestionService
    {
        // فروشنده (StoreId و UserId همیشه از توکن می‌آیند، نه از بدنه)
        Task<BaseResultDto<ProductImageSuggestionDto>> SubmitAsync(
            long storeId, long? actorUserId, long productId, IFormFile image, string authorizationHeaderValue, CancellationToken cancellationToken);
        Task<BaseResultDto<ProductImageSuggestionListDto>> ListAsync(long storeId, long? productId, int pageSize);
        Task<BaseResultDto> DeleteAsync(long storeId, long id);

        // ادمین
        Task<BaseResultDto<ProductImageSuggestionAdminListDto>> SearchAsync(ProductImageSuggestionAdminSearchDto dto);
        Task<BaseResultDto<ProductImageSuggestionApproveResultDto>> ApproveAsync(long adminUserId, ProductImageSuggestionApproveDto dto);
        Task<BaseResultDto<ProductImageSuggestionAdminDto>> RejectAsync(long adminUserId, ProductImageSuggestionRejectDto dto);
    }
}
