using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Common.Interface;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Order.ProductOrderSrv.Iface;
using Application.Services.Order.ShippingSrv;
using Application.Services.Order.ShippingSrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Common.Helpers.Iface;
using Entities.Entities;
using Entities.Entities.ShippingField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ProductOrderSrv
{
    public class ProductOrderAdjustmentService : IProductOrderAdjustmentService
    {
        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly IShipmentService _shipmentService;
        private readonly IClubPointIntegrationService _clubPoints;
        private readonly IMessageSenderService _messageSender;
        private readonly IPushNotificationService _pushService;
        private readonly IAdminSettingHelper _adminSettings;
        private readonly ILogger<ProductOrderAdjustmentService> _logger;

        public ProductOrderAdjustmentService(
            IDataBaseContext context,
            IWalletService walletService,
            IShipmentService shipmentService,
            IClubPointIntegrationService clubPoints,
            IMessageSenderService messageSender,
            IPushNotificationService pushService,
            IAdminSettingHelper adminSettings,
            ILogger<ProductOrderAdjustmentService> logger)
        {
            _context = context;
            _walletService = walletService;
            _shipmentService = shipmentService;
            _clubPoints = clubPoints;
            _messageSender = messageSender;
            _pushService = pushService;
            _adminSettings = adminSettings;
            _logger = logger;
        }

        private static readonly string InsertLabel = ProductOrderStatusEnum.ProductOrderStatus_Insert.ToString();
        private static readonly string ProcessLabel = ProductOrderStatusEnum.ProductOrderStatus_Proccess.ToString();
        private static readonly string CanceledLabel = ProductOrderStateEnum.ProductOrderState_Canceled.ToString();

        private IQueryable<ProductOrder> OrderGraph() => _context.ProductOrders
            .Include(order => order.User)
            .Include(order => order.ProductOrderStatus)
            .Include(order => order.ProductOrderState)
            .Include(order => order.ProductOrderStores).ThenInclude(store => store.Store)
            .Include(order => order.ProductOrderStores).ThenInclude(store => store.Shipment)
            .Include(order => order.ProductOrderStores).ThenInclude(store => store.ProductOrderItems)
                .ThenInclude(item => item.ProductItem).ThenInclude(productItem => productItem.Product)
            .AsTracking();

        // ---------------------------------------------------------------- لغو کامل
        public async Task<BaseResultDto> CancelOrderAsync(string orderId, OrderAdjustActor actor, long storeId, string reason,
            CancellationToken cancellationToken = default)
        {
            reason = NormalizeReason(reason);
            if (actor == OrderAdjustActor.Store && string.IsNullOrWhiteSpace(reason))
                return new BaseResultDto(false, Resource.Notification.OrderAdjustReasonRequired);

            var order = await OrderGraph().FirstOrDefaultAsync(item => item.Id == orderId && !item.Deleted, cancellationToken);
            var check = Validate(order, actor, storeId, cancelWholeOrder: true);
            if (check != null)
                return check;

            var canceledId = await GetCodeIdAsync(CanceledLabel, cancellationToken);
            if (canceledId == 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            await using (var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken))
            {
                try
                {
                    foreach (var line in LiveItems(order))
                        await RestoreStockAsync(line.ProductItemId, line.ProductId, line.Count, cancellationToken);

                    var refund = Math.Round(order.PaymentPrice, 0);
                    if (refund > 0 && !await CreditAsync(order.UserId, ProductOrderAdjustmentRules.CancelRefundLedgerName(order.Id), refund, cancellationToken))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return new BaseResultDto(false, Resource.Notification.OrderAdjustRefundFailed);
                    }

                    order.ProductOrderState = null;
                    order.ProductOrderStateId = canceledId;
                    order.StateDescription = reason;
                    order.AdminDescription = string.IsNullOrWhiteSpace(order.AdminDescription) ? reason : $"{order.AdminDescription}\n{reason}";
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(exception, "Cancelling order {OrderId} failed.", orderId);
                    return new BaseResultDto(false, Resource.Notification.SomethingWentWrong);
                }
            }

            // بعد از ثبت: لغو سفر (میاره)، امتیاز باشگاه، پیام‌ها (هیچ‌کدام نباید لغوِ انجام‌شده را خراب کنند)
            var warning = string.Empty;
            try
            {
                await _shipmentService.CancelForOrderAsync(order.Id, cancellationToken);
                var stillActive = await _context.Shipments.AsNoTracking().AnyAsync(shipment =>
                    shipment.ProductOrderStore.ProductOrderId == order.Id &&
                    shipment.Status != ShipmentStatusEnum.Cancelled && shipment.Status != ShipmentStatusEnum.Failed &&
                    shipment.Status != ShipmentStatusEnum.Delivered, cancellationToken);
                if (stillActive)
                    warning = " " + Resource.Notification.OrderCancelMiareWarning;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Cancelling shipments of order {OrderId} failed.", order.Id);
                warning = " " + Resource.Notification.OrderCancelMiareWarning;
            }
            try { await _clubPoints.ProductOrderReversedAsync(order.UserId, order.Id); }
            catch (Exception exception) { _logger.LogWarning(exception, "Club reversal for cancelled order {OrderId} failed.", order.Id); }

            await NotifyCancelAsync(order, actor, reason, cancellationToken);
            return new BaseResultDto(true, Resource.Notification.OrderCancelledSuccessfully + warning);
        }

        // ---------------------------------------------------------------- کم / حذف آیتم
        public async Task<BaseResultDto> AdjustItemAsync(long productOrderItemId, int newCount, OrderAdjustActor actor, long storeId, string reason,
            CancellationToken cancellationToken = default)
        {
            reason = NormalizeReason(reason);
            var orderId = await _context.ProductOrderItems.AsNoTracking()
                .Where(item => item.Id == productOrderItemId)
                .Select(item => item.ProductOrderStore.ProductOrderId)
                .FirstOrDefaultAsync(cancellationToken);
            if (orderId == null)
                return new BaseResultDto(false, Resource.Notification.OrderAdjustNotFound);

            var order = await OrderGraph().FirstOrDefaultAsync(item => item.Id == orderId && !item.Deleted, cancellationToken);
            var check = Validate(order, actor, storeId, cancelWholeOrder: false);
            if (check != null)
                return check;

            var orderStore = order.ProductOrderStores.FirstOrDefault(store => store.ProductOrderItems.Any(line => line.Id == productOrderItemId));
            var line = orderStore?.ProductOrderItems.FirstOrDefault(item => item.Id == productOrderItemId);
            if (line == null || line.Deleted)
                return new BaseResultDto(false, Resource.Notification.OrderAdjustNotFound);
            if (actor == OrderAdjustActor.Store && orderStore.StoreId != storeId)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            if (!ProductOrderAdjustmentRules.IsValidReducedCount(line.Count, newCount))
                return new BaseResultDto(false, Resource.Notification.OrderAdjustInvalidCount);

            // آخرین کالای کل سفارش حذف می‌شود ← همان لغو کامل (مبلغ ارسال هم برمی‌گردد)
            var remainingAfter = order.ProductOrderStores.SelectMany(store => store.ProductOrderItems)
                .Where(item => !item.Deleted)
                .Sum(item => item.Id == line.Id ? newCount : item.Count);
            if (remainingAfter <= 0)
                return await CancelOrderAsync(order.Id, actor, storeId, reason, cancellationToken);
            // همه‌ی کالاهای یک فروشگاه از سفارش چندفروشگاهی (هزینه‌ی ارسال و مرسوله‌ی جدا) فقط با پشتیبانی
            var storeRemaining = orderStore.ProductOrderItems.Where(item => !item.Deleted).Sum(item => item.Id == line.Id ? newCount : item.Count);
            if (storeRemaining <= 0)
                return new BaseResultDto(false, Resource.Notification.OrderAdjustStoreWouldBeEmpty);

            var oldCount = line.Count;
            var delta = oldCount - newCount;
            var productName = line.ProductItem?.Product?.Name ?? string.Empty;
            double refund;

            await using (var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken))
            {
                try
                {
                    line.Count = newCount;
                    line.Deleted = newCount == 0;
                    line.Edited = true;
                    orderStore.Edited = true;
                    await RestoreStockAsync(line.ProductItemId, line.ProductItem?.ProductId ?? 0, delta, cancellationToken);

                    // جمع‌ها از روی آیتم‌های باقی‌مانده (نه جمع/تفریق تدریجی) تا هیچ‌وقت ناهماهنگ نشوند
                    foreach (var store in order.ProductOrderStores)
                    {
                        var totals = ProductOrderAdjustmentRules.StoreTotals(store.ProductOrderItems.Select(item => new ProductOrderAdjustmentRules.ItemLine
                        {
                            UnitPrice = item.Price, UnitBasePrice = item.BasePrice, Count = item.Count, Deleted = item.Deleted
                        }));
                        store.Price = totals.StorePrice;
                        store.BasePrice = totals.StoreBasePrice;
                        store.DiscountPrice = totals.StoreDiscountPrice;
                        store.PaymentPrice = store.Price + store.DeliveryPrice;
                    }
                    var recalculated = ProductOrderAdjustmentRules.RecalculateOrder(
                        order.Price, order.RebatePrice, order.PaymentPrice, order.DeliveryPrice, order.ClubDeliveryDiscount,
                        order.ProductOrderStores.Sum(store => store.Price), order.ProductOrderStores.Sum(store => store.BasePrice));
                    order.Price = recalculated.Price;
                    order.BasePrice = recalculated.BasePrice;
                    order.DiscountPrice = recalculated.DiscountPrice;
                    order.RebatePrice = recalculated.RebatePrice;
                    order.PaymentPrice = recalculated.PaymentPrice;
                    refund = recalculated.Refund;
                    // سهم فروشگاه/سایت از روی مبلغ جدید دوباره حساب می‌شود (UpdateProductOrderCommissionDto فقط وقتی هر دو صفر باشند حساب می‌کند)
                    RecalculateCommission(order);

                    if (refund > 0 && !await CreditAsync(order.UserId, ProductOrderAdjustmentRules.ItemRefundLedgerName(line.Id, newCount), refund, cancellationToken))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return new BaseResultDto(false, Resource.Notification.OrderAdjustRefundFailed);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(exception, "Adjusting item {ItemId} of order {OrderId} failed.", productOrderItemId, orderId);
                    return new BaseResultDto(false, Resource.Notification.SomethingWentWrong);
                }
            }

            await NotifyItemChangeAsync(order, orderStore, productName, oldCount, newCount, actor, cancellationToken);
            return new BaseResultDto(true, Resource.Notification.OrderItemAdjustedSuccessfully);
        }

        // ---------------------------------------------------------------- اعتبارسنجی مشترک
        private BaseResultDto Validate(ProductOrder order, OrderAdjustActor actor, long storeId, bool cancelWholeOrder)
        {
            if (order == null)
                return new BaseResultDto(false, Resource.Notification.OrderAdjustNotFound);

            if (!ProductOrderAdjustmentRules.IsBeforeShipment(order.IsPaid, order.Deleted, order.ProductOrderStatus?.Label,
                    order.ProductOrderState?.Label, order.UserReceived, InsertLabel, ProcessLabel, CanceledLabel))
                return new BaseResultDto(false, Resource.Notification.OrderAdjustNotAllowedShipped);

            if (actor == OrderAdjustActor.Store)
            {
                if (storeId <= 0 || order.ProductOrderStores.All(store => store.StoreId != storeId))
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                // لغو کل سفارشِ چندفروشگاهی، کالای فروشگاه‌های دیگر را هم لغو می‌کرد
                if (cancelWholeOrder && order.ProductOrderStores.Any(store => store.StoreId != storeId))
                    return new BaseResultDto(false, Resource.Notification.OrderAdjustMultiStoreSeller);
            }

            foreach (var shipment in order.ProductOrderStores.Select(store => store.Shipment).Where(shipment => shipment != null))
            {
                var gate = ProductOrderAdjustmentRules.GateForShipment(shipment.Status, cancelWholeOrder);
                if (gate == ProductOrderAdjustmentRules.ShipmentGate.Blocked)
                    return new BaseResultDto(false, Resource.Notification.OrderAdjustNotAllowedShipped);
                if (gate == ProductOrderAdjustmentRules.ShipmentGate.AdminOnly && actor == OrderAdjustActor.Store)
                    return new BaseResultDto(false, Resource.Notification.OrderAdjustSellerCourierRequested);
            }
            return null;
        }

        private static string NormalizeReason(string reason)
        {
            var clean = string.Join(' ', (reason ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
            return clean.Length > 300 ? clean.Substring(0, 300) : clean;
        }

        private async Task<long> GetCodeIdAsync(string label, CancellationToken cancellationToken) =>
            await _context.Codes.AsNoTracking().Where(code => code.Label == label).Select(code => code.Id).FirstOrDefaultAsync(cancellationToken);

        private sealed record LiveLine(long ProductItemId, long ProductId, int Count);

        private static List<LiveLine> LiveItems(ProductOrder order) => order.ProductOrderStores
            .SelectMany(store => store.ProductOrderItems)
            .Where(item => !item.Deleted && item.Count > 0)
            .Select(item => new LiveLine(item.ProductItemId, item.ProductItem?.ProductId ?? 0, item.Count))
            .ToList();

        // موجودی و فروش به‌صورت اتمی (همان الگوی IncreaseSellCountAsync، به‌صورت معکوس)
        private async Task RestoreStockAsync(long productItemId, long productId, int count, CancellationToken cancellationToken)
        {
            if (count <= 0)
                return;
            await _context.ProductItems
                .Where(item => item.Id == productItemId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Quantity, item => item.Quantity + count), cancellationToken);
            if (productId > 0)
                await _context.Products
                    .Where(product => product.Id == productId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(product => product.SellCount, product => product.SellCount >= count ? product.SellCount - count : 0), cancellationToken);
        }

        // برگشت پول به کیف پول؛ idempotent با نام یکتای دفتر
        private async Task<bool> CreditAsync(long userId, string ledgerName, double amount, CancellationToken cancellationToken)
        {
            var exists = await _context.Wallets.AsNoTracking()
                .AnyAsync(wallet => wallet.Name == ledgerName && wallet.UserId == userId && !wallet.Deleted, cancellationToken);
            if (exists)
                return true;
            var result = await _walletService.InsertAsyncDto(new WalletDto
            {
                Name = ledgerName,
                Amount = amount,
                IsIncrease = true,
                UserId = userId,
                Painding = false
            });
            return result.IsSuccess;
        }

        private static void RecalculateCommission(ProductOrder order)
        {
            decimal totalStoreShare = 0m, totalSiteShare = 0m;
            foreach (var store in order.ProductOrderStores)
            {
                if (store.Store == null || store.PaymentPrice <= 0)
                    continue;
                var percent = store.Store.CommissionPercent;
                if (percent < 0 || percent > 100)
                    continue;
                var payment = (decimal)store.PaymentPrice;
                var siteShare = payment * percent / 100m;
                totalSiteShare += siteShare;
                totalStoreShare += payment - siteShare;
            }
            order.SiteShare = (double)totalSiteShare;
            order.StoreShare = (double)totalStoreShare;
        }

        // ---------------------------------------------------------------- پیام‌ها
        private string OrderCode(ProductOrder order) => order.OrderCode;

        private async Task<List<long>> StoreUserIdsAsync(IEnumerable<long> storeIds, CancellationToken cancellationToken)
        {
            var ids = storeIds.Distinct().ToList();
            return await _context.Stores.AsNoTracking()
                .Where(store => ids.Contains(store.Id))
                .SelectMany(store => store.Users)
                .Where(user => !user.Deleted && !user.Locked)
                .Select(user => user.Id)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<long>> AdminUserIdsAsync(CancellationToken cancellationToken) =>
            _context.Users.AsNoTracking()
                .Where(user => user.RoleId == (long)RoleEnum.Admin && !user.Deleted && !user.Locked)
                .Select(user => user.Id)
                .ToListAsync(cancellationToken);

        private async Task PushAsync(PushTypeEnum type, IEnumerable<long> userIds, string token1, string token2 = null)
        {
            foreach (var userId in userIds.Distinct())
            {
                try { await _pushService.SendPushAsync(type, userId, token1: token1, token2: token2); }
                catch (Exception exception) { _logger.LogWarning(exception, "Order push {Type} failed for user {UserId}.", type, userId); }
            }
        }

        private async Task SmsAsync(MessageTypeEnum type, string mobile, string token1 = null, string token2 = null, string token3 = null, string token4 = null, string token5 = null)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return;
            try { await _messageSender.SendMessageAsync(type, mobile.Trim(), null, token1: token1, token2: token2, token3: token3, token4: token4, token5: token5); }
            catch (Exception exception) { _logger.LogWarning(exception, "Order SMS {Type} failed.", type); }
        }

        private static string FirstName(ProductOrder order) =>
            Application.Services.Order.CartSrv.AbandonedCartReminderRules.FirstNameOnly(order.User?.FirstName) ?? Resource.Notification.AbandonedCartDefaultCustomerName;

        private async Task NotifyCancelAsync(ProductOrder order, OrderAdjustActor actor, string reason, CancellationToken cancellationToken)
        {
            try
            {
                var code = OrderCode(order);
                var stores = order.ProductOrderStores.Select(store => store.Store).Where(store => store != null).GroupBy(store => store.Id).Select(group => group.First()).ToList();
                var storeUserIds = await StoreUserIdsAsync(stores.Select(store => store.Id), cancellationToken);
                var adminMobiles = _adminSettings.BaseAdminSetting?.AdminMobiles;
                var adminUserIds = await AdminUserIdsAsync(cancellationToken);

                if (actor == OrderAdjustActor.Admin)
                {
                    // فروشگاه: پیامک + پوش؛ کاربر: پیامک
                    foreach (var store in stores)
                        await SmsAsync(MessageTypeEnum.ProductOrderCancelledByAdminStore, store.Mobile, token1: code);
                    await SmsAsync(MessageTypeEnum.ProductOrderCancelledByAdminUser, order.User?.Mobile, token1: code);
                    await PushAsync(PushTypeEnum.PushOrderCancelledByAdminStore, storeUserIds, code);
                }
                else
                {
                    // ادمین: پیامک + پوش؛ کاربر: پیامک (عذرخواهی)
                    var storeName = ShipmentLifecycleRules.ForSmsToken(stores.FirstOrDefault()?.Name);
                    var shownReason = string.IsNullOrWhiteSpace(reason) ? Resource.Notification.OrderAdjustNoReason : reason;
                    if (!string.IsNullOrWhiteSpace(adminMobiles))
                        await SmsAsync(MessageTypeEnum.ProductOrderCancelledByStoreAdmin, adminMobiles, token1: code,
                            token2: ShipmentLifecycleRules.ForSmsToken($"{order.User?.FirstName} {order.User?.LastName}", 60),
                            token4: storeName, token5: ShipmentLifecycleRules.ForSmsToken(shownReason));
                    await SmsAsync(MessageTypeEnum.ProductOrderCancelledByStoreUser, order.User?.Mobile, token1: FirstName(order), token2: code, token4: storeName);
                    await PushAsync(PushTypeEnum.PushOrderCancelledByStoreAdmin, adminUserIds.Except(storeUserIds), code, stores.FirstOrDefault()?.Name);
                }
            }
            catch (Exception exception) { _logger.LogWarning(exception, "Cancel notifications for order {OrderId} failed.", order.Id); }
        }

        private async Task NotifyItemChangeAsync(ProductOrder order, ProductOrderStore orderStore, string productName, int oldCount, int newCount,
            OrderAdjustActor actor, CancellationToken cancellationToken)
        {
            try
            {
                var code = OrderCode(order);
                var removed = newCount == 0;
                var storeUserIds = await StoreUserIdsAsync(new[] { orderStore.StoreId }, cancellationToken);
                var product = ShipmentLifecycleRules.ForSmsToken(productName, 30);

                if (actor == OrderAdjustActor.Admin)
                {
                    // فروشگاه: توضیح کامل (چه چیزی تغییر کرد)
                    var detail = removed
                        ? Resource.Notification.OrderItemDetailRemoved
                        : string.Format(Resource.Notification.OrderItemDetailReduced, oldCount, newCount);
                    await SmsAsync(MessageTypeEnum.ProductOrderItemChangedByAdminStore, orderStore.Store?.Mobile, token1: code,
                        token4: product, token5: ShipmentLifecycleRules.ForSmsToken(detail));
                    // کاربر: «کاهش یافت / حذف شد» و برگشت هزینه به کیف پول
                    var action = removed ? Resource.Notification.OrderItemActionRemovedAdmin : Resource.Notification.OrderItemActionReducedAdmin;
                    await SmsAsync(MessageTypeEnum.ProductOrderItemChangedByAdminUser, order.User?.Mobile, token1: code,
                        token4: product, token5: ShipmentLifecycleRules.ForSmsToken(action));
                    await PushAsync(PushTypeEnum.PushOrderChangedByAdminStore, storeUserIds, code);
                }
                else
                {
                    // کاربر: عذرخواهی + «کاهش داد / حذف کرد»؛ ادمین: پوش
                    var action = removed ? Resource.Notification.OrderItemActionRemovedStore : Resource.Notification.OrderItemActionReducedStore;
                    await SmsAsync(MessageTypeEnum.ProductOrderItemChangedByStoreUser, order.User?.Mobile, token1: code,
                        token2: ShipmentLifecycleRules.ForSmsToken(action),
                        token4: ShipmentLifecycleRules.ForSmsToken(orderStore.Store?.Name), token5: product);
                    var adminUserIds = await AdminUserIdsAsync(cancellationToken);
                    await PushAsync(PushTypeEnum.PushOrderChangedByStoreAdmin, adminUserIds.Except(storeUserIds), code);
                }
            }
            catch (Exception exception) { _logger.LogWarning(exception, "Item change notifications for order {OrderId} failed.", order.Id); }
        }
    }
}
