using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.DeliverySrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Order.DeliverySrv.iface
{
    public interface IDeliveryService : ICommonSrv<Delivery, DeliveryDto>
    {
        DeliverySearchDto Search(DeliveryInputDto baseSearchDto);
        Task<BaseResultDto<DeliveryVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto<DeliveryVDto>> FindForStoreAsync(long id, long storeId);
        Task<BaseResultDto<DeliveryDto>> InsertForStoreAsync(DeliveryDto dto, long storeId);
        Task<BaseResultDto> UpdateForStoreAsync(DeliveryDto dto, long storeId);
        Task<BaseResultDto> DeleteForStoreAsync(long id, long storeId);
        Task<BaseResultDto<List<DeliveryTypeOptionDto>>> GetDeliveryTypesAsync();
        BaseResultDto GetDeliveries(Cart cart, long? storeId);
        DeliveryResultVDto GetDelivery(Cart cart, long deliveryId, long? storeId);
        DeliveryResultVDto GetDelivery(Cart cart, Delivery delivery, long? storeId);
    }
}
