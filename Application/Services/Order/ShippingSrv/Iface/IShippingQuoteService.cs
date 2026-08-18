using Application.Common.Dto.Result;
using Application.Services.Order.ShippingSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv.Iface
{
    public interface IShippingQuoteService
    {
        Task<BaseResultDto<List<ShippingQuoteVDto>>> CreateQuotesAsync(
            long userId,
            long storeId,
            CancellationToken cancellationToken = default);
        Task<BaseResultDto> SelectQuoteAsync(
            long userId,
            Guid quoteToken,
            CancellationToken cancellationToken = default);
        Task<BaseResultDto> ValidateSelectionAsync(
            CartStore cartStore,
            long userId,
            long? addressId,
            CancellationToken cancellationToken = default);
    }
}
