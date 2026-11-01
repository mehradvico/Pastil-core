using Application.Common.Dto.Result;
using Application.Services.Order.ShippingSrv.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv.Iface
{
    public interface IStoreWorkingHourService
    {
        Task<BaseResultDto<List<StoreWorkingHourDto>>> GetAsync(long storeId, CancellationToken cancellationToken = default);

        /// <summary>کل هفته را جایگزین می‌کند؛ لیست خالی = حذف محدودیت ساعت کاری.</summary>
        Task<BaseResultDto> SaveAsync(long storeId, List<StoreWorkingHourDto> hours, CancellationToken cancellationToken = default);

        /// <summary>آیا فروشگاه در این لحظه (طبق ساعت کاری خودش) باز است؟ بدون ساعت ثبت‌شده = همیشه باز.</summary>
        Task<bool> IsOpenAsync(long storeId, DateTime nowUtc, CancellationToken cancellationToken = default);
    }
}
