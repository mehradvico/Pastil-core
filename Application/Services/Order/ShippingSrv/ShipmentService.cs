using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
using Application.Services.Order.ShippingSrv.Provider;
using Entities.Entities;
using Entities.Entities.ShippingField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv
{
    public class ShipmentService : IShipmentService
    {
        // متن آدرس گیرنده برای سرویس ارسال: آدرس + طبقه + واحد (اگر ثبت شده باشند)
        public static string ComposeRecipientAddress(Address address)
        {
            if (address == null)
                return null;

            var parts = new List<string> { address.AddressValue };
            if (!string.IsNullOrWhiteSpace(address.Floor))
                parts.Add($"{Resource.Field.Floor} {address.Floor.Trim()}");
            if (!string.IsNullOrWhiteSpace(address.Unit))
                parts.Add($"{Resource.Field.Unit} {address.Unit.Trim()}");

            return string.Join("، ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private readonly IDataBaseContext _context;
        private readonly IReadOnlyDictionary<ShippingProviderEnum, IShippingProvider> _providers;
        private readonly ILogger<ShipmentService> _logger;
        private readonly IPushNotificationService _pushService;
        private readonly ShippingOptions _options;
        private readonly IMessageSenderService _messageSender;

        public ShipmentService(
            IDataBaseContext context,
            IEnumerable<IShippingProvider> providers,
            ILogger<ShipmentService> logger,
            IPushNotificationService pushService,
            IOptions<ShippingOptions> options,
            IMessageSenderService messageSender)
        {
            _context = context;
            _providers = providers.ToDictionary(item => item.Provider);
            _logger = logger;
            _pushService = pushService;
            _options = options.Value;
            _messageSender = messageSender;
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
                if (orderStore.ShippingProvider == ShippingProviderEnum.Miare
                    && orderStore.ShippingSlotId.HasValue && orderStore.ShippingSlotDate.HasValue)
                {
                    await CreateAwaitingSellerShipmentAsync(productOrder, orderStore, quote, cancellationToken);
                    continue;
                }

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
                        RecipientAddress = ComposeRecipientAddress(productOrder.Address),
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

        private static string NewDeliveryCode() => RandomNumberGenerator.GetInt32(10000, 100000).ToString();

        private static string TehranHm(DateTime utc) => ShippingSlotRules.FormatTime(ShippingSlotRules.ToTehran(utc).TimeOfDay);

        // پرداخت انجام شد ← مرسوله «منتظر تأیید فروشنده» ساخته و به فروشنده پوش داده می‌شود. سفر میاره بعد از تأیید ساخته می‌شود.
        private async Task CreateAwaitingSellerShipmentAsync(
            ProductOrder productOrder,
            ProductOrderStore orderStore,
            ShippingQuote quote,
            CancellationToken cancellationToken)
        {
            var slot = await _context.ShippingSlots.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == orderStore.ShippingSlotId.Value, cancellationToken);
            var date = orderStore.ShippingSlotDate.Value.Date;
            DateTime? start = slot == null ? null : ShippingSlotRules.ToUtc(date, slot.StartTime);
            DateTime? end = slot == null ? null : ShippingSlotRules.ToUtc(date, slot.EndTime);
            var nowUtc = DateTime.UtcNow;
            var deadline = end.HasValue
                ? ShippingSlotRules.SellerConfirmDeadline(nowUtc, end.Value, _options.SellerConfirmMinutes, _options.MinDeliveryMinutes)
                : nowUtc.AddMinutes(_options.SellerConfirmMinutes);

            var shipment = new Shipment
            {
                ProductOrderStoreId = orderStore.Id,
                ShippingQuoteId = orderStore.ShippingQuoteId,
                Provider = ShippingProviderEnum.Miare,
                PaymentMode = orderStore.ShippingPaymentMode ?? ShippingPaymentModeEnum.Prepaid,
                Status = ShipmentStatusEnum.AwaitingSellerConfirm,
                QuotedPrice = orderStore.ShippingQuotedPrice,
                ChargedPrice = orderStore.DeliveryPrice,
                CreatedAtUtc = nowUtc,
                SlotStartUtc = start,
                SlotEndUtc = end,
                SellerConfirmDeadlineUtc = deadline,
                DeliveryCode = NewDeliveryCode()
            };
            await _context.Shipments.AddAsync(shipment, cancellationToken);
            if (quote != null)
            {
                quote.Status = ShippingQuoteStatusEnum.Used;
                quote.UsedAtUtc = nowUtc;
            }
            await _context.SaveChangesAsync(cancellationToken);

            var slotText = start.HasValue && end.HasValue ? $"{TehranHm(start.Value)}-{TehranHm(end.Value)}" : string.Empty;
            await PushToStoreUsersAsync(orderStore.StoreId, PushTypeEnum.PushShipmentAwaitingSeller,
                productOrder.OrderCode, TehranHm(deadline), slotText);
        }

        private async Task PushToStoreUsersAsync(long storeId, PushTypeEnum type, string token1, string token2, string token3)
        {
            try
            {
                var userIds = await _context.Stores.AsNoTracking()
                    .Where(store => store.Id == storeId)
                    .SelectMany(store => store.Users)
                    .Where(user => !user.Deleted && !user.Locked)
                    .Select(user => user.Id)
                    .Distinct()
                    .ToListAsync();
                foreach (var userId in userIds)
                {
                    try { await _pushService.SendPushAsync(type, userId, token1: token1, token2: token2, token3: token3); }
                    catch (Exception exception) { _logger.LogWarning(exception, "Shipment push {Type} failed for user {UserId}.", type, userId); }
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Shipment push {Type} failed for store {StoreId}.", type, storeId);
            }
        }

        public async Task<BaseResultDto<List<SellerShipmentVDto>>> GetAwaitingForStoreAsync(
            long storeId,
            CancellationToken cancellationToken = default)
        {
            var nowUtc = DateTime.UtcNow;
            var rows = await _context.Shipments.AsNoTracking()
                .Where(item => (item.Status == ShipmentStatusEnum.AwaitingSellerConfirm || item.Status == ShipmentStatusEnum.Preparing)
                               && item.ProductOrderStore.StoreId == storeId)
                .OrderBy(item => item.Status == ShipmentStatusEnum.AwaitingSellerConfirm ? item.SellerConfirmDeadlineUtc : item.ReadyDeadlineUtc)
                .Select(item => new
                {
                    item.ProductOrderStoreId,
                    item.ProductOrderStore.ProductOrderId,
                    item.ProductOrderStore.ProductOrder.OrderCode,
                    item.Status,
                    item.SlotStartUtc,
                    item.SlotEndUtc,
                    item.SellerConfirmDeadlineUtc,
                    item.ReadyDeadlineUtc
                })
                .ToListAsync(cancellationToken);

            var list = rows.Select(row => new SellerShipmentVDto
            {
                ProductOrderStoreId = row.ProductOrderStoreId,
                ProductOrderId = row.ProductOrderId,
                OrderCode = row.OrderCode,
                Status = row.Status,
                Step = row.Status == ShipmentStatusEnum.AwaitingSellerConfirm ? 1 : 2,
                SlotStartUtc = row.SlotStartUtc,
                SlotEndUtc = row.SlotEndUtc,
                SellerConfirmDeadlineUtc = row.Status == ShipmentStatusEnum.AwaitingSellerConfirm ? row.SellerConfirmDeadlineUtc : null,
                ReadyDeadlineUtc = row.ReadyDeadlineUtc ?? (row.SlotEndUtc.HasValue ? ShippingSlotRules.ReadyDeadline(row.SlotEndUtc.Value, _options.MinDeliveryMinutes) : null),
                PlannedPickupUtc = ShippingSlotRules.DefaultPickup(row.SlotStartUtc ?? nowUtc, nowUtc, _options.PickupLeadMinutes)
            }).ToList();
            return new BaseResultDto<List<SellerShipmentVDto>>(true, list);
        }

        // مرحله‌ی ۱: فروشنده سفارش را می‌پذیرد. تا اینجا هیچ سفری ساخته نمی‌شود؛ فقط وضعیت «در حال آماده‌سازی» و مهلت «آماده تحویل به پیک».
        public async Task<BaseResultDto> ConfirmBySellerAsync(
            long storeId,
            long productOrderStoreId,
            CancellationToken cancellationToken = default)
        {
            var shipment = await _context.Shipments
                .AsTracking()
                .FirstOrDefaultAsync(item => item.ProductOrderStoreId == productOrderStoreId
                                             && item.ProductOrderStore.StoreId == storeId, cancellationToken);
            if (shipment == null || shipment.Status != ShipmentStatusEnum.AwaitingSellerConfirm)
                return new BaseResultDto(false, Resource.Notification.ShipmentConfirmNotAllowed);

            var nowUtc = DateTime.UtcNow;
            if (shipment.SellerConfirmDeadlineUtc.HasValue && nowUtc > shipment.SellerConfirmDeadlineUtc.Value)
                return new BaseResultDto(false, Resource.Notification.ShipmentConfirmExpired);

            shipment.Status = ShipmentStatusEnum.Preparing;
            shipment.SellerConfirmedAtUtc = nowUtc;
            shipment.ReadyDeadlineUtc = shipment.SlotEndUtc.HasValue
                ? ShippingSlotRules.ReadyDeadline(shipment.SlotEndUtc.Value, _options.MinDeliveryMinutes)
                : nowUtc.AddHours(2);
            shipment.ReadyReminderSentAtUtc = null;
            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto(true, Resource.Notification.ShipmentAcceptedSuccessfully);
        }

        // مرحله‌ی ۲: «آماده تحویل به پیک». همین لحظه سفر میاره ساخته می‌شود؛ pickup.deadline = max(شروع بازه − PickupLead, الان + ۱۵ دقیقه)
        // تا پیک نه زودتر از موعد (کالا بیرون از بازه‌ی مشتری برسد) و نه دیرتر از «آخرین ساعت مفید» بیاید. این وضعیت برای مشتری نمایش داده نمی‌شود.
        public async Task<BaseResultDto> MarkReadyBySellerAsync(
            long storeId,
            long productOrderStoreId,
            CancellationToken cancellationToken = default)
        {
            var shipment = await _context.Shipments
                .Include(item => item.ProductOrderStore).ThenInclude(item => item.ProductOrder).ThenInclude(item => item.Address)
                .Include(item => item.ShippingQuote)
                .AsTracking()
                .FirstOrDefaultAsync(item => item.ProductOrderStoreId == productOrderStoreId
                                             && item.ProductOrderStore.StoreId == storeId, cancellationToken);
            if (shipment == null || shipment.Status != ShipmentStatusEnum.Preparing)
                return new BaseResultDto(false, Resource.Notification.ShipmentReadyNotAllowed);

            var nowUtc = DateTime.UtcNow;
            var slotEnd = shipment.SlotEndUtc ?? nowUtc.AddHours(4);
            var slotStart = shipment.SlotStartUtc ?? nowUtc;
            var readyDeadline = shipment.ReadyDeadlineUtc ?? ShippingSlotRules.ReadyDeadline(slotEnd, _options.MinDeliveryMinutes);
            if (nowUtc > readyDeadline)
                return new BaseResultDto(false, Resource.Notification.ShipmentReadyExpired);

            var pickup = ShippingSlotRules.DefaultPickup(slotStart, nowUtc, _options.PickupLeadMinutes);
            if (pickup > readyDeadline)
                return new BaseResultDto(false, Resource.Notification.ShipmentPickupTimeInvalid);

            if (!_providers.TryGetValue(shipment.Provider, out var provider))
                return new BaseResultDto(false, Resource.Notification.ShipmentCourierRequestFailed);

            var orderStore = shipment.ProductOrderStore;
            var productOrder = orderStore.ProductOrder;
            var store = await _context.Stores.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == orderStore.StoreId, cancellationToken);
            try
            {
                var result = await provider.CreateShipmentAsync(new ShippingProviderShipmentRequest
                {
                    OrderId = productOrder.Id,
                    BillNumber = $"{productOrder.Id}-{orderStore.Id}",
                    StoreId = orderStore.StoreId,
                    Provider = shipment.Provider,
                    PaymentMode = shipment.PaymentMode,
                    ExternalQuoteId = shipment.ShippingQuote?.ExternalQuoteId,
                    RecipientName = $"{productOrder.Address?.FirstName} {productOrder.Address?.LastName}".Trim(),
                    RecipientMobile = productOrder.Address?.Mobile,
                    RecipientAddress = ComposeRecipientAddress(productOrder.Address),
                    PickupName = store?.Name,
                    PickupPhone = store?.Phone,
                    PickupAddress = store?.Address,
                    OriginLatitude = store?.Location?.Y,
                    OriginLongitude = store?.Location?.X,
                    DestinationLatitude = productOrder.Address?.Location?.Y,
                    DestinationLongitude = productOrder.Address?.Location?.X,
                    PickupDeadlineUtc = pickup,
                    DeliveryCode = shipment.DeliveryCode
                }, cancellationToken);

                if (!result.IsSuccess)
                {
                    // فروشنده تا پایان مهلت می‌تواند دوباره امتحان کند؛ مرسوله هنوز «در حال آماده‌سازی» می‌ماند
                    shipment.FailureReason = Truncate(result.ErrorMessage);
                    await _context.SaveChangesAsync(cancellationToken);
                    return new BaseResultDto(false, Resource.Notification.ShipmentCourierRequestFailed);
                }

                shipment.Status = ShipmentStatusEnum.Requested;
                shipment.ExternalShipmentId = result.ExternalShipmentId;
                shipment.TrackingCode = result.TrackingCode;
                shipment.FailureReason = null;
                shipment.RequestedAtUtc = nowUtc;
                shipment.ReadyAtUtc = nowUtc;
                shipment.PickupDeadlineUtc = pickup;
                await _context.SaveChangesAsync(cancellationToken);
                return new BaseResultDto(true, Resource.Notification.ShipmentConfirmedSuccessfully);
            }
            catch (Exception exception)
            {
                shipment.FailureReason = Truncate(exception.Message);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogWarning(exception, "Miare trip creation failed for ProductOrderStore {Id}.", productOrderStoreId);
                return new BaseResultDto(false, Resource.Notification.ShipmentCourierRequestFailed);
            }
        }

        private static string Truncate(string value) =>
            string.IsNullOrEmpty(value) || value.Length <= 1000 ? value : value[..1000];

        // job: ۱۵ دقیقه مانده به مهلت یک‌بار یادآوری به فروشنده؛ بعد از مهلت، مرسوله Failed می‌شود و سفارش با «درخواست لغو»
        // وارد صف ادمین می‌شود (استرداد پول همان جریان فعلی لغو سفارش است) و به مشتری پوش می‌رسد.
        public async Task<int> ProcessUnconfirmedAsync(CancellationToken cancellationToken = default)
        {
            var nowUtc = DateTime.UtcNow;
            var shipments = await _context.Shipments
                .Include(item => item.ProductOrderStore).ThenInclude(item => item.ProductOrder)
                .Where(item => item.Status == ShipmentStatusEnum.AwaitingSellerConfirm
                               && item.SellerConfirmDeadlineUtc != null)
                .AsTracking()
                .ToListAsync(cancellationToken);

            var expired = 0;
            foreach (var shipment in shipments)
            {
                var deadline = shipment.SellerConfirmDeadlineUtc.Value;
                var orderStore = shipment.ProductOrderStore;
                var productOrder = orderStore.ProductOrder;
                try
                {
                    if (nowUtc >= deadline)
                    {
                        shipment.Status = ShipmentStatusEnum.Failed;
                        shipment.FailureReason = "SellerConfirmExpired";
                        if (productOrder.CancelRequestDate == null)
                        {
                            productOrder.CancelRequestDate = DateTime.Now;
                            if (string.IsNullOrWhiteSpace(productOrder.AdminDescription))
                                productOrder.AdminDescription = Resource.Notification.ShipmentSellerConfirmExpiredNote;
                        }
                        await _context.SaveChangesAsync(cancellationToken);
                        await _pushService.SendPushAsync(PushTypeEnum.PushShipmentSellerExpiredUser, productOrder.UserId,
                            token1: productOrder.OrderCode);
                        expired++;
                    }
                    else if (shipment.SellerReminderSentAtUtc == null && deadline - nowUtc <= TimeSpan.FromMinutes(15))
                    {
                        shipment.SellerReminderSentAtUtc = nowUtc;
                        await _context.SaveChangesAsync(cancellationToken);
                        await PushToStoreUsersAsync(orderStore.StoreId, PushTypeEnum.PushShipmentSellerReminder,
                            productOrder.OrderCode, TehranHm(deadline), string.Empty);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Processing unconfirmed shipment {ShipmentId} failed.", shipment.Id);
                }
            }
            expired += await ProcessPreparingAsync(nowUtc, cancellationToken);
            await NotifyLateShipmentsAsync(nowUtc, cancellationToken);
            await AskReceivedAsync(nowUtc, cancellationToken);
            return expired;
        }

        // «در حال آماده‌سازی» که «آماده تحویل به پیک» نخورده: ۳۰ دقیقه مانده به مهلت یک‌بار یادآوری؛ بعد از مهلت شکست و صف بررسی ادمین
        private async Task<int> ProcessPreparingAsync(DateTime nowUtc, CancellationToken cancellationToken)
        {
            var preparing = await _context.Shipments
                .Include(item => item.ProductOrderStore).ThenInclude(item => item.ProductOrder)
                .Where(item => item.Status == ShipmentStatusEnum.Preparing && item.ReadyDeadlineUtc != null)
                .AsTracking()
                .ToListAsync(cancellationToken);
            var expired = 0;
            foreach (var shipment in preparing)
            {
                var deadline = shipment.ReadyDeadlineUtc.Value;
                var orderStore = shipment.ProductOrderStore;
                var order = orderStore.ProductOrder;
                try
                {
                    if (nowUtc >= deadline)
                    {
                        shipment.Status = ShipmentStatusEnum.Failed;
                        shipment.FailureReason = "SellerNotReady";
                        await _context.SaveChangesAsync(cancellationToken);
                        await RequestOrderCancelReviewAsync(order, Resource.Notification.ShipmentNotReadyExpiredNote, cancellationToken);
                        await PushToUserAsync(PushTypeEnum.PushShipmentDelayedUser, order.UserId, order.OrderCode);
                        expired++;
                    }
                    else if (shipment.ReadyReminderSentAtUtc == null && deadline - nowUtc <= TimeSpan.FromMinutes(30))
                    {
                        shipment.ReadyReminderSentAtUtc = nowUtc;
                        await _context.SaveChangesAsync(cancellationToken);
                        await PushToStoreUsersAsync(orderStore.StoreId, PushTypeEnum.PushShipmentReadyReminder,
                            order.OrderCode, TehranHm(deadline), string.Empty);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Processing preparing shipment {ShipmentId} failed.", shipment.Id);
                }
            }
            return expired;
        }

        // سفارشی که پایان بازه‌ی تحویلش گذشته و هنوز تحویل نشده: یک‌بار پوش تأخیر به ادمین (به مشتری جداگانه «آیا تحویل گرفتید؟» می‌رود)
        private async Task NotifyLateShipmentsAsync(DateTime nowUtc, CancellationToken cancellationToken)
        {
            var activeStatuses = new[] { ShipmentStatusEnum.Requested, ShipmentStatusEnum.Accepted, ShipmentStatusEnum.PickedUp };
            var late = await _context.Shipments
                .Include(item => item.ProductOrderStore).ThenInclude(item => item.ProductOrder)
                .Where(item => activeStatuses.Contains(item.Status)
                               && item.SlotEndUtc != null && item.SlotEndUtc < nowUtc
                               && item.LateNotifiedAtUtc == null)
                .OrderBy(item => item.SlotEndUtc)
                .Take(100)
                .AsTracking()
                .ToListAsync(cancellationToken);
            if (late.Count == 0)
                return;
            var adminIds = await GetAdminUserIdsAsync(cancellationToken);
            foreach (var shipment in late)
            {
                try
                {
                    shipment.LateNotifiedAtUtc = nowUtc;
                    await _context.SaveChangesAsync(cancellationToken);
                    var order = shipment.ProductOrderStore.ProductOrder;
                    foreach (var adminId in adminIds)
                        await PushToUserAsync(PushTypeEnum.PushShipmentLateAdmin, adminId, order.OrderCode);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Late shipment notification failed for shipment {ShipmentId}.", shipment.Id);
                }
            }
        }

        // راس پایان بازه‌ی تحویل (مثلاً ساعت ۱۳ برای بازه‌ی ۹ تا ۱۳): پوش «آیا سفارش ... را تحویل گرفتید؟» با دکمه‌های بله/خیر.
        // یک‌بار برای هر سفارش (claim اتمی)، فقط اگر مشتری هنوز پاسخ نداده و سفارش لغو نشده. پاسخ مشتری تنها راه نهایی‌شدن تحویل است.
        private async Task AskReceivedAsync(DateTime nowUtc, CancellationToken cancellationToken)
        {
            var counted = new[]
            {
                ShipmentStatusEnum.AwaitingSellerConfirm, ShipmentStatusEnum.Preparing, ShipmentStatusEnum.Requested,
                ShipmentStatusEnum.Accepted, ShipmentStatusEnum.PickedUp, ShipmentStatusEnum.Delivered
            };
            var ids = await GetStatusIdsAsync(cancellationToken);
            var orderIds = await _context.Shipments.AsNoTracking()
                .Where(item => item.Provider == ShippingProviderEnum.Miare
                               && counted.Contains(item.Status)
                               && item.SlotEndUtc != null && item.SlotEndUtc <= nowUtc
                               && item.ProductOrderStore.ProductOrder.IsPaid
                               && item.ProductOrderStore.ProductOrder.UserReceived == null
                               && item.ProductOrderStore.ProductOrder.ReceiptAskedAtUtc == null
                               && item.ProductOrderStore.ProductOrder.ProductOrderStateId != ids.Canceled)
                .Select(item => item.ProductOrderStore.ProductOrderId)
                .Distinct()
                .Take(100)
                .ToListAsync(cancellationToken);

            foreach (var orderId in orderIds)
            {
                try
                {
                    var ends = await _context.Shipments.AsNoTracking()
                        .Where(item => item.ProductOrderStore.ProductOrderId == orderId && counted.Contains(item.Status))
                        .Select(item => item.SlotEndUtc)
                        .ToListAsync(cancellationToken);
                    if (!ShipmentLifecycleRules.ShouldAskReceived(ends, nowUtc))
                        continue;

                    var claimed = await _context.ProductOrders
                        .Where(order => order.Id == orderId && order.ReceiptAskedAtUtc == null && order.UserReceived == null)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(order => order.ReceiptAskedAtUtc, (DateTime?)nowUtc), cancellationToken);
                    if (claimed == 0)
                        continue;

                    var order = await _context.ProductOrders.AsNoTracking()
                        .Where(item => item.Id == orderId)
                        .Select(item => new { item.UserId, item.OrderCode })
                        .FirstOrDefaultAsync(cancellationToken);
                    if (order != null)
                        await PushToUserAsync(PushTypeEnum.PushOrderAskReceived, order.UserId, order.OrderCode);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Asking received for order {OrderId} failed.", orderId);
                }
            }
        }

        private Task<List<long>> GetAdminUserIdsAsync(CancellationToken cancellationToken) =>
            _context.Users.AsNoTracking()
                .Where(user => user.RoleId == (long)RoleEnum.Admin && !user.Deleted && !user.Locked)
                .Select(user => user.Id)
                .ToListAsync(cancellationToken);

        private async Task PushToUserAsync(PushTypeEnum type, long userId, string token1, string token2 = null)
        {
            try { await _pushService.SendPushAsync(type, userId, token1: token1, token2: token2); }
            catch (Exception exception) { _logger.LogWarning(exception, "Shipment push {Type} failed for user {UserId}.", type, userId); }
        }

        // مشتری «تحویل نگرفتم» زد: فقط علامت می‌خورد که گزارش شده (پوش و پیامک ادمین را ProductOrderService می‌فرستد)
        public async Task ReportNotReceivedAsync(string productOrderId, CancellationToken cancellationToken = default)
        {
            var nowUtc = DateTime.UtcNow;
            var shipments = await _context.Shipments
                .Where(item => item.ProductOrderStore.ProductOrderId == productOrderId
                               && item.Provider == ShippingProviderEnum.Miare
                               && item.DisputeReportedAtUtc == null)
                .AsTracking()
                .ToListAsync(cancellationToken);
            foreach (var shipment in shipments)
                shipment.DisputeReportedAtUtc = nowUtc;
            if (shipments.Count > 0)
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

                var shipment = await _context.Shipments
                    .Include(item => item.ProductOrderStore).ThenInclude(item => item.ProductOrder).ThenInclude(item => item.User)
                    .Include(item => item.ProductOrderStore).ThenInclude(item => item.Store)
                    .AsTracking()
                    .FirstOrDefaultAsync(
                        item => item.ExternalShipmentId == tripId && item.Provider == ShippingProviderEnum.Miare,
                        cancellationToken);
                if (shipment == null)
                {
                    _logger.LogWarning("Miare webhook received for unknown trip {TripId}.", tripId);
                    return;
                }

                var state = root.TryGetProperty("state", out var stateProp) ? stateProp.GetString() : null;
                if (string.IsNullOrWhiteSpace(state))
                    return;
                await ApplyMiareStateAsync(shipment, state.Trim().ToLowerInvariant(), root, cancellationToken);
            }
        }

        // یک وضعیت میاره را فقط رو به جلو اعمال می‌کند (وب‌هوک تکراری/جابه‌جا بی‌اثر است) و اثرهای جانبی را یک‌بار انجام می‌دهد.
        private async Task ApplyMiareStateAsync(Shipment shipment, string state, JsonElement root, CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;
            switch (state)
            {
                case "assign_queue":
                    await AdvanceAsync(shipment, ShipmentStatusEnum.Requested, cancellationToken);
                    break;
                case "pickup":
                    await AdvanceAsync(shipment, ShipmentStatusEnum.Accepted, cancellationToken);
                    break;
                case "dropoff": // پیک کالا را از فروشگاه گرفته و حرکت کرده
                    if (await AdvanceAsync(shipment, ShipmentStatusEnum.PickedUp, cancellationToken))
                        await OnPickedUpAsync(shipment, cancellationToken);
                    break;
                case "delivered":
                    if (!ShipmentLifecycleRules.CanAdvance(shipment.Status, ShipmentStatusEnum.Delivered))
                        break;
                    shipment.Status = ShipmentStatusEnum.Delivered;
                    shipment.DeliveredAtUtc = nowUtc;
                    // میاره «delivery_cost» را به تومان می‌فرستد؛ هزینه‌ی واقعی سفر نزد میاره (نه مبلغ دریافتی از مشتری)،
                    // برای ریال‌سازی مثل GetQuoteAsync ضرب‌در‌ده می‌شود.
                    if (root.TryGetProperty("delivery_cost", out var costProp) && costProp.ValueKind == JsonValueKind.Number)
                        shipment.ProviderCost = costProp.GetDouble() * 10;
                    shipment.ShippedNotifiedAtUtc ??= nowUtc; // «ارسال» را از دست داده بودیم؛ دیگر پیام ارسال نمی‌خواهد
                    await _context.SaveChangesAsync(cancellationToken);
                    await OnDeliveredAsync(shipment, cancellationToken);
                    break;
                case "returning": // مشتری تحویل نگرفت/پیک نتوانست تحویل بدهد و کالا برمی‌گردد
                    if (!ShipmentLifecycleRules.CanAdvance(shipment.Status, ShipmentStatusEnum.Failed))
                        break;
                    shipment.Status = ShipmentStatusEnum.Failed;
                    shipment.FailureReason = "Returning";
                    await _context.SaveChangesAsync(cancellationToken);
                    await RequestOrderCancelReviewAsync(shipment.ProductOrderStore.ProductOrder, Resource.Notification.ShipmentReturningNote, cancellationToken);
                    await PushToStoreUsersAsync(shipment.ProductOrderStore.StoreId, PushTypeEnum.PushShipmentReturning,
                        shipment.ProductOrderStore.ProductOrder.OrderCode, string.Empty, string.Empty);
                    break;
                case "canceled_by_delay":
                case "canceled_by_miare":
                    await OnCourierCanceledAsync(shipment, state, cancellationToken);
                    break;
                case "canceled_by_client":
                    if (ShipmentLifecycleRules.CanAdvance(shipment.Status, ShipmentStatusEnum.Cancelled))
                    {
                        shipment.Status = ShipmentStatusEnum.Cancelled;
                        shipment.FailureReason = "Miare state: " + state;
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    break;
            }
        }

        private async Task<bool> AdvanceAsync(Shipment shipment, ShipmentStatusEnum next, CancellationToken cancellationToken)
        {
            if (!ShipmentLifecycleRules.CanAdvance(shipment.Status, next))
                return false;
            shipment.Status = next;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        // ارسال با میاره = لحظه‌ای که پیک واقعاً کالا را گرفته. یک‌بار: وضعیت سفارش «ارسال»، پیامک و پوش کد تحویل به مشتری.
        private async Task OnPickedUpAsync(Shipment shipment, CancellationToken cancellationToken)
        {
            if (shipment.ShippedNotifiedAtUtc != null)
                return;
            shipment.ShippedNotifiedAtUtc = DateTime.UtcNow; // اول «claim» می‌شود تا وب‌هوک تکراری دوبار پیام نفرستد
            await _context.SaveChangesAsync(cancellationToken);

            var orderStore = shipment.ProductOrderStore;
            var order = orderStore.ProductOrder;
            try
            {
                await MarkOrderSentAsync(order.Id, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Marking order {OrderId} as sent failed.", order.Id);
            }

            var slotText = string.Empty;
            if (shipment.SlotStartUtc.HasValue && shipment.SlotEndUtc.HasValue)
                slotText = string.Format(Resource.Notification.ShipmentSlotRangeFormat,
                    ShipmentLifecycleRules.FormatHour(ShippingSlotRules.ToTehran(shipment.SlotStartUtc.Value).TimeOfDay),
                    ShipmentLifecycleRules.FormatHour(ShippingSlotRules.ToTehran(shipment.SlotEndUtc.Value).TimeOfDay));
            try
            {
                // قالب کاوه‌نگار: %token = نام، %token2 = شماره سفارش (متن و لینک)، %token3 = کد تحویل، %token10 = فروشگاه، %token20 = بازه
                await _messageSender.SendMessageAsync(
                    messageType: MessageTypeEnum.ProductOrderShippedMiare,
                    mobileReceptor: order.User?.Mobile,
                    emailReceptor: null,
                    token1: Application.Services.Order.CartSrv.AbandonedCartReminderRules.FirstNameOnly(order.User?.FirstName) ?? Resource.Notification.AbandonedCartDefaultCustomerName,
                    token2: order.OrderCode,
                    token3: shipment.DeliveryCode,
                    token4: ShipmentLifecycleRules.ForSmsToken(orderStore.Store?.Name),
                    token5: ShipmentLifecycleRules.ForSmsToken(slotText));
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Shipped SMS failed for order {OrderId}.", order.Id);
            }
            await PushToUserAsync(PushTypeEnum.PushShipmentShippedUser, order.UserId, shipment.DeliveryCode, order.OrderCode);
        }

        private async Task MarkOrderSentAsync(string orderId, CancellationToken cancellationToken)
        {
            var ids = await GetStatusIdsAsync(cancellationToken);
            var now = DateTime.Now;
            // فقط از «ثبت‌شده/در حال پردازش»؛ سفارش «تحویل شده» به عقب برنمی‌گردد
            await _context.ProductOrders
                .Where(order => order.Id == orderId
                                && (order.ProductOrderStatusId == ids.Insert || order.ProductOrderStatusId == ids.Process))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(order => order.ProductOrderStatusId, ids.Send)
                    .SetProperty(order => order.SentDate, (DateTime?)now), cancellationToken);
        }

        private async Task<(long Insert, long Process, long Send, long Delivered, long Canceled)> GetStatusIdsAsync(CancellationToken cancellationToken)
        {
            var labels = new[]
            {
                ProductOrderStatusEnum.ProductOrderStatus_Insert.ToString(),
                ProductOrderStatusEnum.ProductOrderStatus_Proccess.ToString(),
                ProductOrderStatusEnum.ProductOrderStatus_Send.ToString(),
                ProductOrderStatusEnum.ProductOrderStatus_Delivered.ToString(),
                ProductOrderStateEnum.ProductOrderState_Canceled.ToString()
            };
            var map = await _context.Codes.AsNoTracking()
                .Where(code => labels.Contains(code.Label))
                .ToDictionaryAsync(code => code.Label, code => code.Id, cancellationToken);
            return (map.GetValueOrDefault(labels[0]), map.GetValueOrDefault(labels[1]), map.GetValueOrDefault(labels[2]),
                map.GetValueOrDefault(labels[3]), map.GetValueOrDefault(labels[4]));
        }

        // پیک کد تحویل را از مشتری گرفته و میاره تحویل را ثبت کرده: مرسوله «تحویل شد» است، ولی سفارش را فقط خود مشتری نهایی می‌کند
        // (پوش راس پایان بازه: «آیا تحویل گرفتید؟»؛ AskReceivedAsync). اگر پایان بازه گذشته باشد، job بعدی (≤ ۵ دقیقه) همان پوش را می‌فرستد.
        private Task OnDeliveredAsync(Shipment shipment, CancellationToken cancellationToken) => Task.CompletedTask;

        // پیک بعد از ۲۰ دقیقه تأخیر (یا توسط پشتیبانی میاره) لغو شد: اگر هنوز وقت مفید مانده، یک‌بار دیگر از فروشنده تأیید
        // می‌گیریم؛ وگرنه شکست و سفارش وارد صف بررسی/استرداد ادمین می‌شود.
        private async Task OnCourierCanceledAsync(Shipment shipment, string state, CancellationToken cancellationToken)
        {
            if (!ShipmentLifecycleRules.CanAdvance(shipment.Status, ShipmentStatusEnum.Cancelled))
                return;
            var nowUtc = DateTime.UtcNow;
            var orderStore = shipment.ProductOrderStore;
            var order = orderStore.ProductOrder;
            var preParcel = shipment.Status is ShipmentStatusEnum.Requested or ShipmentStatusEnum.Accepted;

            if (preParcel && ShipmentLifecycleRules.MayRetryCourier(shipment.CourierRetryCount, shipment.SlotEndUtc, nowUtc,
                    _options.MinDeliveryMinutes, _options.SellerConfirmMinutes))
            {
                var deadline = ShippingSlotRules.ReadyDeadline(shipment.SlotEndUtc.Value, _options.MinDeliveryMinutes);
                shipment.Status = ShipmentStatusEnum.Preparing;
                shipment.CourierRetryCount++;
                shipment.FailureReason = Truncate($"{Resource.Notification.ShipmentCourierCanceledNote} ({state}, trip {shipment.ExternalShipmentId})");
                shipment.ExternalShipmentId = null;
                shipment.TrackingCode = null;
                shipment.RequestedAtUtc = null;
                shipment.ReadyAtUtc = null;
                shipment.PickupDeadlineUtc = null;
                shipment.ReadyReminderSentAtUtc = null;
                shipment.ReadyDeadlineUtc = deadline;
                await _context.SaveChangesAsync(cancellationToken);
                await PushToStoreUsersAsync(orderStore.StoreId, PushTypeEnum.PushShipmentCourierCanceled,
                    order.OrderCode, TehranHm(deadline), string.Empty);
                return;
            }

            shipment.Status = ShipmentStatusEnum.Failed;
            shipment.FailureReason = "Miare state: " + state;
            await _context.SaveChangesAsync(cancellationToken);
            await RequestOrderCancelReviewAsync(order, Resource.Notification.ShipmentCourierFailedNote, cancellationToken);
            await PushToUserAsync(PushTypeEnum.PushShipmentDelayedUser, order.UserId, order.OrderCode);
        }

        // سفارش را در صف «درخواست لغو» ادمین می‌گذارد (استرداد پول همان جریان فعلی لغو سفارش است)
        private async Task RequestOrderCancelReviewAsync(ProductOrder order, string note, CancellationToken cancellationToken)
        {
            var tracked = await _context.ProductOrders.AsTracking().FirstOrDefaultAsync(item => item.Id == order.Id, cancellationToken);
            if (tracked == null || tracked.CancelRequestDate != null)
                return;
            tracked.CancelRequestDate = DateTime.Now;
            if (string.IsNullOrWhiteSpace(tracked.AdminDescription))
                tracked.AdminDescription = note;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
