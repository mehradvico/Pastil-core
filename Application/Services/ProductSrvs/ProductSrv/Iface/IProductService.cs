using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Application.Services.ProductSrvs.ProductSrv.Iface
{
    public interface IProductService
    {
        Task<BaseResultDto<ProductDto>> FindAsyncDto(long id, long? storeId = null);
        Task<BaseResultDto<ProductVDto>> FindAsyncVDto(long id, bool visit);
        Task<BaseResultDto<ProductDto>> InsertAsyncDto(ProductDto dto);
        ProductSearchDto Search(ProductInputDto baseSearchDto);
        Task<List<SearchProductDto>> SearchMinAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
        Task<BaseResultDto> UpdateDtoAsync(ProductDto dto, long? storeId = null);
        BaseResultDto DeleteDto(long id);
        Task<Product> GetByIdAsync(long id);
        Task IncreaseSellCountAsync(ProductOrder order);
        BaseResultDto GetSiteMap();
        Task UpdateProductPriceAsync(ProductUpdateTypeEnum productUpdateType, string Id);
        Task<BaseResultDto<ProductDto>> DuplicateAsyncDto(ProductDuplicateDto productDuplicateDto);
        Task<BaseResultDto> ChangeProductVarietiesAsync(ProductDto product);
    }
}
