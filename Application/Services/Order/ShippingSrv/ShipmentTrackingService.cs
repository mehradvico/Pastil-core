using Application.Common.Dto.Result;
using Application.Services.Order.CartSrv;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
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
    public class ShipmentTrackingService : IShipmentTrackingService
    {
        private readonly IDataBaseContext _context;

        public ShipmentTrackingService(IDataBaseContext context)
        {
            _context = context;
        }

        private sealed class TrackingRow
        {
            public Shipment Shipment { get; set; }
            public long StoreId { get; set; }
            public string StoreName { get; set; }
            public double? StoreLatitude { get; set; }
            public double? StoreLongitude { get; set; }
            public double? DestinationLatitude { get; set; }
            public double? DestinationLongitude { get; set; }
        }

        private static IQueryable<TrackingRow> Project(IQueryable<Shipment> query) =>
            query.Select(item => new TrackingRow
            {
                Shipment = item,
                StoreId = item.ProductOrderStore.StoreId,
                StoreName = item.ProductOrderStore.Store.Name,
                StoreLatitude = (double?)item.ProductOrderStore.Store.Location.Y,
                StoreLongitude = (double?)item.ProductOrderStore.Store.Location.X,
                DestinationLatitude = (double?)item.ProductOrderStore.ProductOrder.Address.Location.Y,
                DestinationLongitude = (double?)item.ProductOrderStore.ProductOrder.Address.Location.X
            });

        public async Task<BaseResultDto<List<ShipmentTrackingVDto>>> GetForCustomerAsync(
            long userId,
            string orderIdOrCode,
            CancellationToken cancellationToken = default)
        {
            var key = orderIdOrCode?.Trim();
            if (userId <= 0 || string.IsNullOrEmpty(key))
                return new BaseResultDto<List<ShipmentTrackingVDto>>(true, new List<ShipmentTrackingVDto>());

            var rows = await Project(_context.Shipments.AsNoTracking()
                    .Where(item => item.Provider == ShippingProviderEnum.Miare
                                   && item.ProductOrderStore.ProductOrder.UserId == userId
                                   && !item.ProductOrderStore.ProductOrder.Deleted
                                   && (item.ProductOrderStore.ProductOrderId == key || item.ProductOrderStore.ProductOrder.OrderCode == key)))
                .OrderBy(row => row.Shipment.Id)
                .ToListAsync(cancellationToken);

            var times = await LoadStateTimesAsync(rows.Select(row => row.Shipment.Id).ToList(), cancellationToken);
            var nowUtc = DateTime.UtcNow;
            var list = rows
                .Select(row => Build(row, TrackingAudience.Customer, times.GetValueOrDefault(row.Shipment.Id), nowUtc))
                .ToList();
            return new BaseResultDto<List<ShipmentTrackingVDto>>(true, list);
        }

        public async Task<BaseResultDto<ShipmentTrackingVDto>> GetForStoreAsync(
            long storeId,
            long productOrderStoreId,
            CancellationToken cancellationToken = default)
        {
            if (storeId <= 0 || productOrderStoreId <= 0)
                return new BaseResultDto<ShipmentTrackingVDto>(false, Resource.Notification.AccessDenied, null);

            var row = await Project(_context.Shipments.AsNoTracking()
                    .Where(item => item.Provider == ShippingProviderEnum.Miare
                                   && item.ProductOrderStoreId == productOrderStoreId
                                   && item.ProductOrderStore.StoreId == storeId
                                   && !item.ProductOrderStore.ProductOrder.Deleted))
                .FirstOrDefaultAsync(cancellationToken);
            if (row == null)
                return new BaseResultDto<ShipmentTrackingVDto>(false, Resource.Notification.ResourceNotFind, null);

            var times = await LoadStateTimesAsync(new List<long> { row.Shipment.Id }, cancellationToken);
            return new BaseResultDto<ShipmentTrackingVDto>(true,
                Build(row, TrackingAudience.Seller, times.GetValueOrDefault(row.Shipment.Id), DateTime.UtcNow));
        }

        // اولین زمان رسیدن هر state میاره برای هر مرسوله (برای زمان مراحل)
        private async Task<Dictionary<long, Dictionary<string, DateTime>>> LoadStateTimesAsync(
            List<long> shipmentIds,
            CancellationToken cancellationToken)
        {
            if (shipmentIds.Count == 0)
                return new Dictionary<long, Dictionary<string, DateTime>>();

            var rows = await _context.ShipmentEvents.AsNoTracking()
                .Where(item => shipmentIds.Contains(item.ShipmentId))
                .GroupBy(item => new { item.ShipmentId, item.State })
                .Select(group => new { group.Key.ShipmentId, group.Key.State, At = group.Min(item => item.ReceivedAtUtc) })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(item => item.ShipmentId)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToDictionary(item => item.State, item => item.At, StringComparer.OrdinalIgnoreCase));
        }

        private static DateTime? Utc(DateTime? value) =>
            value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

        private static ShipmentTrackingVDto Build(
            TrackingRow row,
            TrackingAudience audience,
            Dictionary<string, DateTime> stateTimes,
            DateTime nowUtc)
        {
            var shipment = row.Shipment;
            stateTimes ??= new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

            var steps = ShipmentCourierRules.BuildSteps(shipment.Status, shipment.FailureReason, stateTimes.Keys, shipment.RequestedAtUtc.HasValue);
            var currentKey = ShipmentCourierRules.CurrentKey(steps);

            DateTime? StepTime(string key) => key switch
            {
                "preparing" => shipment.CreatedAtUtc,
                "courier_requested" => shipment.RequestedAtUtc,
                "courier_assigned" => stateTimes.TryGetValue("pickup", out var assigned) ? assigned : null,
                "picked_up" => stateTimes.TryGetValue("dropoff", out var pickedUp) ? pickedUp : null,
                "delivered" => shipment.DeliveredAtUtc ?? (stateTimes.TryGetValue("delivered", out var delivered) ? delivered : null),
                _ => shipment.LastMiareEventAtUtc
            };

            var dto = new ShipmentTrackingVDto
            {
                ShipmentId = shipment.Id,
                ProductOrderStoreId = shipment.ProductOrderStoreId,
                StoreId = row.StoreId,
                StoreName = row.StoreName,
                Provider = shipment.Provider.ToString(),
                Status = (int)shipment.Status,
                StatusKey = shipment.Status.ToString(),
                IsExpress = shipment.IsExpress,
                IsFinal = ShipmentLifecycleRules.IsTerminal(shipment.Status),
                IsLive = shipment.Status is ShipmentStatusEnum.Accepted or ShipmentStatusEnum.PickedUp,
                CurrentStepKey = currentKey,
                Headline = StepTitle(currentKey),
                Steps = steps.Select(step => new ShipmentTrackStepVDto
                {
                    Key = step.Key,
                    Title = StepTitle(step.Key),
                    Done = step.Done,
                    Current = step.Current,
                    AtUtc = step.Done || step.Current ? Utc(StepTime(step.Key)) : null
                }).ToList(),
                PlannedPickupUtc = Utc(shipment.PickupDeadlineUtc),
                DeliveredAtUtc = Utc(shipment.DeliveredAtUtc),
                LastUpdateUtc = Utc(shipment.LastMiareEventAtUtc),
                PollSeconds = ShipmentCourierRules.SuggestedPollSeconds
            };

            if (row.StoreLatitude.HasValue && row.StoreLongitude.HasValue
                && ShipmentCourierRules.IsUsable(row.StoreLatitude.Value, row.StoreLongitude.Value))
                dto.Store = new ShipmentPointVDto { Latitude = row.StoreLatitude.Value, Longitude = row.StoreLongitude.Value };

            // مختصات مشتری فقط در پاسخ خودِ مشتری
            ShipmentPointVDto destination = null;
            if (audience == TrackingAudience.Customer && row.DestinationLatitude.HasValue && row.DestinationLongitude.HasValue
                && ShipmentCourierRules.IsUsable(row.DestinationLatitude.Value, row.DestinationLongitude.Value))
            {
                destination = new ShipmentPointVDto { Latitude = row.DestinationLatitude.Value, Longitude = row.DestinationLongitude.Value };
                dto.Destination = destination;
            }

            dto.Courier = BuildCourier(shipment, audience, dto.Store, destination, nowUtc);
            return dto;
        }

        private static ShipmentCourierVDto BuildCourier(
            Shipment shipment,
            TrackingAudience audience,
            ShipmentPointVDto store,
            ShipmentPointVDto destination,
            DateTime nowUtc)
        {
            var showName = ShipmentCourierRules.ShowName(audience, shipment.Status);
            var showPhone = ShipmentCourierRules.ShowPhone(audience, shipment.Status);
            var showLocation = ShipmentCourierRules.ShowLocation(audience, shipment.Status);

            var courier = new ShipmentCourierVDto();
            if (showName && !string.IsNullOrEmpty(shipment.CourierName))
            {
                // مشتری فقط نام کوچک؛ فروشنده برای شناسایی در تحویل نام کامل
                courier.Name = audience == TrackingAudience.Customer
                    ? AbandonedCartReminderRules.FirstNameOnly(shipment.CourierName) ?? shipment.CourierName
                    : shipment.CourierName;
            }
            if (showPhone && !string.IsNullOrEmpty(shipment.CourierPhone))
            {
                courier.Phone = shipment.CourierPhone;
                courier.CanCall = true;
            }
            if (showLocation && shipment.CourierLatitude.HasValue && shipment.CourierLongitude.HasValue)
            {
                courier.HasLocation = true;
                courier.Latitude = shipment.CourierLatitude;
                courier.Longitude = shipment.CourierLongitude;
                courier.LocationUpdatedAtUtc = Utc(shipment.CourierLocationAtUtc);
                courier.LocationAgeSeconds = shipment.CourierLocationAtUtc.HasValue
                    ? Math.Max(0, (int)(nowUtc - shipment.CourierLocationAtUtc.Value).TotalSeconds)
                    : null;
                courier.LocationStale = ShipmentCourierRules.IsLocationStale(shipment.CourierLocationAtUtc, nowUtc);

                // پیک بعد از برداشتن کالا به‌سمت مشتری می‌رود؛ قبلش به‌سمت فروشگاه
                var towardDestination = audience == TrackingAudience.Customer && shipment.Status == ShipmentStatusEnum.PickedUp;
                var target = towardDestination ? destination : store;
                if (target != null)
                {
                    courier.DistanceKm = Math.Round(
                        ShipmentCourierRules.DistanceKm(shipment.CourierLatitude.Value, shipment.CourierLongitude.Value, target.Latitude, target.Longitude), 1);
                    courier.DistanceTarget = towardDestination ? "destination" : "store";
                }
            }

            return courier.Name == null && courier.Phone == null && !courier.HasLocation ? null : courier;
        }

        public static string StepTitle(string key) => key switch
        {
            "preparing" => Resource.Notification.ShipmentTrackStepPreparing,
            "courier_requested" => Resource.Notification.ShipmentTrackStepCourierRequested,
            "courier_assigned" => Resource.Notification.ShipmentTrackStepCourierAssigned,
            "picked_up" => Resource.Notification.ShipmentTrackStepPickedUp,
            "delivered" => Resource.Notification.ShipmentTrackStepDelivered,
            "cancelled" => Resource.Notification.ShipmentTrackStepCancelled,
            "returning" => Resource.Notification.ShipmentTrackStepReturning,
            "failed" => Resource.Notification.ShipmentTrackStepFailed,
            _ => string.Empty
        };
    }
}
