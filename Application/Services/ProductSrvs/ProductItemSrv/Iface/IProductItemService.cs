using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.ProductItemSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.ProductItemSrv.Iface
{
    public interface IProductItemService : ICommonSrv<ProductItem, ProductItemDto>
    {
        ProductItemSearchDto SearchDto(ProductItemInputDto dto);
        void InsertOrUpdate(ProductItemDto productItem);
        void InsertOrUpdate(List<ProductItemDto> productItemListUpdate);
        Task<BaseResultDto> InsertOrUpdateAsync(ProductItemListUpdateDto productItemListUpdate);
        Task<BaseResultDto<ProductItemVDto>> IsSalable(long productItemId, int count);
        Task<BaseResultDto> GetInsertOrUpdateListAsync(ProductItemListRequestDto productItemListrequest);
        Task<BaseResultDto> GetVarietyAsync(long productId);
        Task<BaseResultDto> GetProductVarietyStructureAsync(long productId);
        Task<BaseResultDto<ProductItemForProductDto>> GetForProductVDto(long productId);
        Task<BaseResultDto> GetVariety2Async(long productId, long? varietyItem1Id, long? varietyItem2Id);
    }
}
