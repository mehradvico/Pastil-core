using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.DeliverySrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.Order.DeliverySrv.iface
{
    public interface IDeliveryService : ICommonSrv<Delivery, DeliveryDto>
    {
        DeliverySearchDto Search(DeliveryInputDto baseSearchDto);
        Task<BaseResultDto<DeliveryVDto>> FindAsyncVDto(long id);
        BaseResultDto GetDeliveries(Cart cart, long? storeId);
        DeliveryResultVDto GetDelivery(Cart cart, long deliveryId, long? storeId);
        DeliveryResultVDto GetDelivery(Cart cart, Delivery delivery, long? storeId);
    }
}
