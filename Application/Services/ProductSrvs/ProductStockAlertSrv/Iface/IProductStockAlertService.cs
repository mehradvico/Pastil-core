using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductStockAlertSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.ProductStockAlertSrv.Iface
{
    public interface IProductStockAlertService
    {
        ProductStockAlertSearchDto SearchDto(ProductStockAlertInputDto dto);
        Task<BaseResultDto<ProductStockAlertDto>> SubscribeAsync(ProductStockAlertDto dto);
        Task<BaseResultDto> UnsubscribeAsync(ProductStockAlertDto dto);
        Task NotifyRestockedAsync(long productId, long storeId);
    }
}
