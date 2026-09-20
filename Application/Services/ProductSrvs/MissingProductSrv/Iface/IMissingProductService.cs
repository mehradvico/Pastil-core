using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.MissingProductSrv.Dto;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.MissingProductSrv.Iface
{
    public interface IMissingProductService
    {
        // فروشنده (StoreId همیشه از توکن می‌آید، نه از بدنه)
        Task<BaseResultDto<MissingProductBatchResultDto>> CreateBatchAsync(long storeId, MissingProductBatchInputDto dto);
        Task<BaseResultDto<MissingProductListDto>> ListAsync(long storeId, int pageSize);
        Task<BaseResultDto<MissingProductDto>> UpdateAsync(long storeId, MissingProductUpdateDto dto);
        Task<BaseResultDto<MissingProductDto>> AddPictureAsync(long storeId, long id, IFormFile image, string authorizationHeaderValue, CancellationToken cancellationToken);
        Task<BaseResultDto<MissingProductDto>> RemovePictureAsync(long storeId, long id, long pictureId);
        Task<BaseResultDto<MissingProductDto>> SubmitAsync(long storeId, long id);
        Task<BaseResultDto> DeleteAsync(long storeId, long id);

        // ادمین
        Task<BaseResultDto<MissingProductAdminListDto>> SearchAsync(MissingProductAdminSearchDto dto);
        Task<BaseResultDto<MissingProductAdminDto>> GetAsync(long id, CancellationToken cancellationToken);
        Task<BaseResultDto<MissingProductApproveResultDto>> ApproveAsync(long adminUserId, MissingProductApproveDto dto);
        Task<BaseResultDto<MissingProductAdminDto>> RejectAsync(long adminUserId, MissingProductRejectDto dto);
    }
}
