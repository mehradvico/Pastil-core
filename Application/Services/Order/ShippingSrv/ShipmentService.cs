using Application.Services.Order.ShippingSrv.Iface;
using Application.Services.Order.ShippingSrv.Provider;
using Entities.Entities;
using Entities.Entities.ShippingField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv
{
    public class ShipmentService : IShipmentService
    {
        private readonly IDataBaseContext _context;
        private readonly IReadOnlyDictionary<ShippingProviderEnum, IShippingProvider> _providers;

        public ShipmentService(IDataBaseContext context, IEnumerable<IShippingProvider> providers)
        {
            _context = context;
            _providers = providers.ToDictionary(item => item.Provider);
        }

        public async Task CreateForPaidOrderAsync(
            ProductOrder productOrder,
            CancellationToken cancellationToken = default)
        {
            foreach (var orderStore in productOrder.ProductOrderStores ?? Enumerable.Empty<ProductOrderStore>())
            {
                if (!orderStore.ShippingProvider.HasValue ||
                    orderStore.ShippingProvider == ShippingProviderEnum.None ||
                    !_providers.TryGetValue(orderStore.ShippingProvider.Value, out var provider))
                    continue;
                if (await _context.Shipments.AnyAsync(
                    item => item.ProductOrderStoreId == orderStore.Id,
                    cancellationToken))
                    continue;

                var quote = orderStore.ShippingQuoteId.HasValue
                    ? await _context.ShippingQuotes.AsTracking().FirstOrDefaultAsync(
                        item => item.Id == orderStore.ShippingQuoteId.Value,
                        cancellationToken)
                    : null;
                var shipment = new Shipment
                {
                    ProductOrderStoreId = orderStore.Id,
                    ShippingQuoteId = orderStore.ShippingQuoteId,
                    Provider = orderStore.ShippingProvider.Value,
                    PaymentMode = orderStore.ShippingPaymentMode ?? ShippingPaymentModeEnum.Prepaid,
                    Status = ShipmentStatusEnum.Pending,
                    QuotedPrice = orderStore.ShippingQuotedPrice,
                    ChargedPrice = orderStore.DeliveryPrice,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _context.Shipments.AddAsync(shipment, cancellationToken);

                var store = await _context.Stores.AsNoTracking().FirstOrDefaultAsync(
                    item => item.Id == orderStore.StoreId,
                    cancellationToken);

                try
                {
                    var result = await provider.CreateShipmentAsync(new ShippingProviderShipmentRequest
                    {
                        OrderId = productOrder.Id,
                        StoreId = orderStore.StoreId,
                        Provider = shipment.Provider,
                        PaymentMode = shipment.PaymentMode,
                        ExternalQuoteId = quote?.ExternalQuoteId,
                        RecipientName = $"{productOrder.Address?.FirstName} {productOrder.Address?.LastName}".Trim(),
                        RecipientMobile = productOrder.Address?.Mobile,
                        RecipientAddress = productOrder.Address?.AddressValue,
                        PickupName = store?.Name,
                        PickupPhone = store?.Phone,
                        OriginLatitude = store?.Location?.Y,
                        OriginLongitude = store?.Location?.X,
                        DestinationLatitude = productOrder.Address?.Location?.Y,
                        DestinationLongitude = productOrder.Address?.Location?.X
                    }, cancellationToken);
                    shipment.Status = result.IsSuccess ? ShipmentStatusEnum.Requested : ShipmentStatusEnum.Failed;
                    shipment.ExternalShipmentId = result.ExternalShipmentId;
                    shipment.TrackingCode = result.TrackingCode;
                    shipment.FailureReason = result.ErrorMessage;
                    shipment.RequestedAtUtc = result.IsSuccess ? DateTime.UtcNow : null;
                }
                catch (Exception exception)
                {
                    shipment.Status = ShipmentStatusEnum.Failed;
                    shipment.FailureReason = exception.Message.Length > 1000
                        ? exception.Message[..1000]
                        : exception.Message;
                }

                if (quote != null)
                {
                    quote.Status = ShippingQuoteStatusEnum.Used;
                    quote.UsedAtUtc = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task CancelForOrderAsync(
            string productOrderId,
            CancellationToken cancellationToken = default)
        {
            var terminalStatuses = new[]
            {
                ShipmentStatusEnum.Delivered,
                ShipmentStatusEnum.Cancelled,
                ShipmentStatusEnum.Failed
            };

            var shipments = await _context.Shipments
                .Include(item => item.ProductOrderStore)
                .Where(item => item.ProductOrderStore.ProductOrderId == productOrderId &&
                               !terminalStatuses.Contains(item.Status))
                .AsTracking()
                .ToListAsync(cancellationToken);

            foreach (var shipment in shipments)
            {
                if (!_providers.TryGetValue(shipment.Provider, out var provider) ||
                    string.IsNullOrWhiteSpace(shipment.ExternalShipmentId))
                {
                    shipment.Status = ShipmentStatusEnum.Cancelled;
                    continue;
                }

                try
                {
                    var result = await provider.CancelShipmentAsync(new ShippingProviderCancelRequest
                    {
                        Provider = shipment.Provider,
                        ExternalShipmentId = shipment.ExternalShipmentId
                    }, cancellationToken);

                    if (result.IsSuccess)
                        shipment.Status = ShipmentStatusEnum.Cancelled;
                    else
                        shipment.FailureReason = result.ErrorMessage;
                }
                catch (Exception exception)
                {
                    shipment.FailureReason = exception.Message.Length > 1000
                        ? exception.Message[..1000]
                        : exception.Message;
                }
            }

            if (shipments.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
