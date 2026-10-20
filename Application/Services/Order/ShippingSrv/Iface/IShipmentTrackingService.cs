using Application.Common.Dto.Result;
using Application.Services.Order.ShippingSrv.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv.Iface
{
    // ردیابی پیک میاره برای مشتری و فروشنده (ادمین از IShipmentService.GetForOrderAsync می‌گیرد).
    // قرارداد: backend/Docs/MIARE_COURIER_TRACKING_FA.md
    public interface IShipmentTrackingService
    {
        // مرسوله‌های پیک یک سفارش مشتری؛ شناسه یا شماره‌ی سفارش. سفارش دیگران = لیست خالی (بدون افشای وجود سفارش).
        Task<BaseResultDto<List<ShipmentTrackingVDto>>> GetForCustomerAsync(
            long userId,
            string orderIdOrCode,
            CancellationToken cancellationToken = default);

        // مرسوله‌ی یک سفارش این فروشگاه
        Task<BaseResultDto<ShipmentTrackingVDto>> GetForStoreAsync(
            long storeId,
            long productOrderStoreId,
            CancellationToken cancellationToken = default);
    }
}
