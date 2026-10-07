using Application.Common.Dto.Result;
using Application.Services.Order.ShippingSrv.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv.Iface
{
    public interface IShippingSlotService
    {
        // مشتری
        Task<BaseResultDto<List<ShippingSlotDayVDto>>> GetAvailableAsync(long storeId, CancellationToken cancellationToken = default);

        // انتخاب روز + بازه برای یک فروشگاه باید معتبر باشد: بازه فعال، روز هفته مطابق، داخل افق، متناسب با زمان آماده‌سازی همان فروشگاه، و ظرفیت باقی باشد.
        Task<BaseResultDto> ValidateSelectionAsync(long storeId, long slotId, DateTime date, CancellationToken cancellationToken = default);

        // ادمین
        Task<BaseResultDto<List<ShippingSlotDto>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<BaseResultDto> SaveAsync(ShippingSlotDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
