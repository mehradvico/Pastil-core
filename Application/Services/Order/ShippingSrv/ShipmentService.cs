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
                        RecipientAddress = productOrder.Address?.AddressValue
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
    }
}
