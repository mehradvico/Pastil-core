using Application.Services.Order.ShippingSrv.Iface;
using Application.Services.Order.ShippingSrv.Provider;
using Entities.Entities;
using Entities.Entities.ShippingField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv
{
    public class ShipmentService : IShipmentService
    {
        private readonly IDataBaseContext _context;
        private readonly IReadOnlyDictionary<ShippingProviderEnum, IShippingProvider> _providers;
        private readonly ILogger<ShipmentService> _logger;

        public ShipmentService(
            IDataBaseContext context,
            IEnumerable<IShippingProvider> providers,
            ILogger<ShipmentService> logger)
        {
            _context = context;
            _providers = providers.ToDictionary(item => item.Provider);
            _logger = logger;
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

        public async Task HandleMiareWebhookAsync(
            string payload,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return;

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(payload);
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Miare webhook payload was not valid JSON.");
                return;
            }

            using (doc)
            {
                var root = doc.RootElement;
                var tripId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                if (string.IsNullOrWhiteSpace(tripId))
                    return;

                var shipment = await _context.Shipments.AsTracking().FirstOrDefaultAsync(
                    item => item.ExternalShipmentId == tripId && item.Provider == ShippingProviderEnum.Miare,
                    cancellationToken);
                if (shipment == null)
                {
                    _logger.LogWarning("Miare webhook received for unknown trip {TripId}.", tripId);
                    return;
                }

                var state = root.TryGetProperty("state", out var stateProp) ? stateProp.GetString() : null;
                var newStatus = MapMiareState(state, shipment.Status);
                shipment.Status = newStatus;

                if (newStatus is ShipmentStatusEnum.Cancelled or ShipmentStatusEnum.Failed)
                    shipment.FailureReason = $"Miare state: {state}";

                if (string.Equals(state, "delivered", StringComparison.OrdinalIgnoreCase))
                {
                    shipment.DeliveredAtUtc = DateTime.UtcNow;

                    // میاره «delivery_cost» را به تومان می‌فرستد؛ این هزینه‌ی واقعی سفر نزد میاره است
                    // (نه مبلغِ دریافتی از مشتری که در ChargedPrice/QuotedPrice است)، برای ریال‌سازی
                    // مثل GetQuoteAsync ضرب‌در‌ده می‌شود.
                    if (root.TryGetProperty("delivery_cost", out var costProp) && costProp.ValueKind == JsonValueKind.Number)
                        shipment.ProviderCost = costProp.GetDouble() * 10;
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        private static ShipmentStatusEnum MapMiareState(string state, ShipmentStatusEnum current) => state switch
        {
            "assign_queue" => ShipmentStatusEnum.Requested,
            "pickup" => ShipmentStatusEnum.Accepted,
            "dropoff" => ShipmentStatusEnum.PickedUp,
            "delivered" => ShipmentStatusEnum.Delivered,
            "canceled_by_miare" or "canceled_by_delay" or "canceled_by_client" => ShipmentStatusEnum.Cancelled,
            "returning" => ShipmentStatusEnum.Failed,
            _ => current
        };
    }
}
