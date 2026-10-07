using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Common.Service;
using Application.Services.Accounting.ScoreTransactionSrv.Iface;
using Application.Services.Accounting.UserProductSrv.Iface;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Order.ProductOrderOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Application.Services.Order.PaymentSrv;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.Order.ShippingSrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.CodeSrv;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.ShippingField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersianDate.Standard;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Application.Services.Order.ProductOrderSrv
{
    public class ProductOrderService : CommonSrv<ProductOrder, ProductOrderDto>, IProductOrderService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IRebateService _rebateService;
        private readonly IMessageSenderService _messageSenderService;
        private readonly IWalletService _walletService;
        private readonly IAdminSettingHelper _adminSettingHelperService;
        private readonly IUserProductService _userProductService;
        private readonly INoticeService _notificationService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ICodeService _codeService;
        private readonly IScoreTransactionService _scoreService;
        private readonly IClubPointIntegrationService _clubPointIntegrationService;
        private readonly IShipmentService _shipmentService;
        private readonly ILogger<ProductOrderService> _logger;

        public ProductOrderService(IDataBaseContext _context, IPushNotificationService pushNotificationService, IUserProductService userProductService,
            INoticeService notificationService, IMapper mapper, ICodeService codeService, IAdminSettingHelper adminSettingHelper, IWalletService walletService,
            IUserService userService, IProductService productService, IRebateService rebateService, IScoreTransactionService scoreService,
            IMessageSenderService messageSenderService,
            IClubPointIntegrationService clubPointIntegrationService,
            IShipmentService shipmentService,
            ILogger<ProductOrderService> logger) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._userService = userService;
            this._productService = productService;
            this._rebateService = rebateService;
            this._messageSenderService = messageSenderService;
            this._walletService = walletService;
            this._adminSettingHelperService = adminSettingHelper;
            this._userProductService = userProductService;
            this._notificationService = notificationService;
            this._pushNotificationService = pushNotificationService;
            this._codeService = codeService;
            this._scoreService = scoreService;
            this._clubPointIntegrationService = clubPointIntegrationService;
            this._shipmentService = shipmentService;
            this._logger = logger;
        }
        public async Task<BaseResultDto> FindAsyncVDto(string id, long? userId = null)
        {
            var query = _context.ProductOrders.Include(s => s.User).Include(s => s.Address).Include(s => s.ProductOrderState)
                .Include(s => s.ProductOrderStatus).Include(s => s.PaymentType).Include(s => s.ProductOrderStores).ThenInclude(s => s.ProductOrderItems)
                .Include(s => s.ProductOrderStores).ThenInclude(s => s.Delivery)
                .Include(s => s.ProductOrderStores).ThenInclude(s => s.Shipment)
                // لینک پیامک با شماره‌ی سفارش ساخته می‌شود (محدودیت توکن‌های کاوه‌نگار)؛ جست‌وجوی با کد فقط برای خودِ مشتری
                .Where(s => !s.Deleted && (s.Id == id || (userId != null && s.OrderCode == id)));
            if (userId.HasValue)
                query = query.Where(s => s.UserId == userId.Value);
            var item = await query.FirstOrDefaultAsync();
            if (item != null)
            {
                var vdto = mapper.Map<ProductOrderVDto>(item);
                // کد تحویل فقط به خودِ مشتری نشان داده می‌شود؛ فروشنده/ادمین (userId == null) نباید آن را ببینند
                // و حتی برای مشتری، فقط بعد از تحویل کالا به پیک (ShipmentLifecycleRules.IsCodeVisible)
                if (vdto.ProductOrderStores != null)
                    foreach (var orderStore in vdto.ProductOrderStores)
                        if (!userId.HasValue || !Application.Services.Order.ShippingSrv.ShipmentLifecycleRules.IsCodeVisible(orderStore.ShipmentStatus))
                            orderStore.ShipmentDeliveryCode = null;
                return new BaseResultDto<ProductOrderVDto>(true, data: vdto);
            }
            return new BaseResultDto(false, val: Resource.Notification.ResourceNotFind);
        }

        public override async Task<BaseResultDto<ProductOrderDto>> InsertAsyncDto(ProductOrderDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<ProductOrderDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<ProductOrder>(dto);
                    DateTime justNow = DateTime.UtcNow;
                    item.CreateDate = DateTime.Now;
                    item.Id = justNow.ToFa("yyyy") + justNow.ToFa("MM") + justNow.ToFa("dd") + justNow.ToString("HHmmssff");
                    item.OrderCode = PaymentCodeGenerator.Create(
                        PaymentCallbackTypeEnum.ProductOrder,
                        item.CreateDate,
                        await _context.GetNextBusinessCodeNumberAsync());

                    // رزرو موجودی: بررسی «موجودی در دسترس» و ثبت سفارش زیر قفل‌های هم‌نام روی هر آیتم کالا انجام
                    // می‌شود تا دو خریدار همزمان هر دو آخرین واحد را نگیرند (جزئیات: StockReservation).
                    var requested = (item.ProductOrderStores ?? new List<ProductOrderStore>())
                        .SelectMany(store => store.ProductOrderItems ?? new List<ProductOrderItem>())
                        .GroupBy(orderItem => orderItem.ProductItemId)
                        .ToDictionary(group => group.Key, group => group.Sum(orderItem => orderItem.Count));

                    await using var transaction = await _context.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
                    foreach (var productItemId in requested.Keys.OrderBy(id => id))
                    {
                        await _context.AcquireTransactionLockAsync($"stock-product-item:{productItemId}");
                    }

                    var itemIds = requested.Keys.ToList();
                    var quantities = await _context.ProductItems.AsNoTracking()
                        .Where(productItem => itemIds.Contains(productItem.Id))
                        .ToDictionaryAsync(productItem => productItem.Id, productItem => productItem.Quantity);

                    var holdCutoff = DateTime.Now.AddMinutes(-StockReservation.HoldMinutes);
                    var heldRows = await _context.ProductOrderItems.AsNoTracking()
                        .Where(orderItem => itemIds.Contains(orderItem.ProductItemId)
                            && !orderItem.Deleted
                            && !orderItem.ProductOrderStore.ProductOrder.IsPaid
                            && !orderItem.ProductOrderStore.ProductOrder.Deleted
                            && orderItem.ProductOrderStore.ProductOrder.CreateDate > holdCutoff
                            && orderItem.ProductOrderStore.ProductOrder.UserId != dto.UserId
                            && !(_context.Payments.Any(payment =>
                                    (payment.ProductOrderId == orderItem.ProductOrderStore.ProductOrderId
                                        || payment.CallBackId == orderItem.ProductOrderStore.ProductOrderId)
                                    && payment.IsSuccess == false)
                                && !_context.Payments.Any(payment =>
                                    (payment.ProductOrderId == orderItem.ProductOrderStore.ProductOrderId
                                        || payment.CallBackId == orderItem.ProductOrderStore.ProductOrderId)
                                    && payment.IsSuccess == null)))
                        .GroupBy(orderItem => orderItem.ProductItemId)
                        .Select(group => new { ProductItemId = group.Key, Held = group.Sum(orderItem => orderItem.Count) })
                        .ToListAsync();
                    var heldByOthers = heldRows.ToDictionary(row => row.ProductItemId, row => row.Held);

                    var shortages = StockReservation.FindShortages(requested, quantities, heldByOthers);
                    if (shortages.Count > 0)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogInformation("ProductOrder rejected for user {UserId}: insufficient available stock for product items {ProductItemIds}", dto.UserId, string.Join(",", shortages));
                        return new BaseResultDto<ProductOrderDto>(false, Resource.Notification.TheNumberIsMoreThanStock, dto);
                    }

                    await _context.ProductOrders.AddAsync(item);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new BaseResultDto<ProductOrderDto>(true, mapper.Map<ProductOrderDto>(item));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProductOrder insert failed for userId {UserId}, paymentPrice {PaymentPrice}, storeCount {StoreCount}", dto.UserId, dto.PaymentPrice, dto.ProductOrderStores?.Count ?? 0);
                // 👈 موقت جهت عیب‌یابی: پیام واقعی Exception (val2) رو هم برمی‌گردونیم تا بدون
                // نیاز به لاگ سرور علت واقعی شکست insert سفارش رو ببینیم. فرانت‌اند فقط
                // item1 (پیام عمومی فعلی) رو توی toast نشون می‌ده، پس رفتار کاربر عوض نمی‌شه.
                var detail = ex.InnerException?.Message ?? ex.Message;
                return new BaseResultDto<ProductOrderDto>(isSuccess: false, val1: Resource.Notification.Unsuccess, val2: detail, data: dto);
            }
        }

        public ProductOrderSearchDto Search(ProductOrderInputDto baseSearchDto)
        {
            var query = _context.ProductOrders.Include(s => s.Rebate).Include(s => s.Address).Include(s => s.DeliveryType).Include(s => s.PaymentType)
                .Include(s => s.User).Include(s => s.ProductOrderState).Include(s => s.ProductOrderStatus).Include(s => s.PaymentType)
                .Include(s => s.ProductOrderStores).ThenInclude(s => s.ProductOrderItems)
                .Include(s => s.ProductOrderStores).ThenInclude(s => s.Delivery).Where(s => s.Deleted == false).AsQueryable();

            if (baseSearchDto.UserId.HasValue)
            {
                query = query.Where(s => s.UserId.Equals(baseSearchDto.UserId));
            }
            if (baseSearchDto.StoreId.HasValue)
            {
                query = query.Where(s => s.ProductOrderStores.Any(m => m.StoreId == baseSearchDto.StoreId.Value));
            }
            if (baseSearchDto.ProductOrderStateEnum.HasValue)
            {
                query = query.Where(s => s.ProductOrderState.Label.Equals(baseSearchDto.ProductOrderStateEnum.ToString()));
            }
            if (baseSearchDto.ProductOrderStatusEnum.HasValue)
            {
                query = query.Where(s => s.ProductOrderStatus.Label.Equals(baseSearchDto.ProductOrderStatusEnum.ToString()));
            }
            if (baseSearchDto.UserDelivery.HasValue)
            {
                // 0 = منتظر پاسخ کاربر (ارسال‌شده، پرداخت‌شده، لغو نشده و هنوز پاسخ نداده)، 1 = تحویل گرفته، 2 = تحویل نگرفته
                var sendLabel = ProductOrderStatusEnum.ProductOrderStatus_Send.ToString();
                var canceledLabel = ProductOrderStateEnum.ProductOrderState_Canceled.ToString();
                query = baseSearchDto.UserDelivery.Value switch
                {
                    0 => query.Where(s => s.IsPaid && s.UserReceived == null && s.ProductOrderStatus.Label == sendLabel && s.ProductOrderState.Label != canceledLabel),
                    1 => query.Where(s => s.UserReceived == true),
                    2 => query.Where(s => s.UserReceived == false),
                    _ => query
                };
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                var queryText = baseSearchDto.Q.Trim();
                query = query.Where(s => s.OrderCode == queryText || s.Id == queryText ||
                    s.User.FirstName.Contains(queryText) || s.User.LastName.Contains(queryText) || s.User.Mobile.Contains(queryText));
            }
            if (!string.IsNullOrEmpty(baseSearchDto.TrackingCode))
            {
                query = query.Where(s => s.TrackingCode.Contains(baseSearchDto.TrackingCode));
            }
            if (baseSearchDto.DateFrom.HasValue)
            {
                query = query.Where(s => s.CreateDate >= baseSearchDto.DateFrom);
            }
            if (baseSearchDto.DateTo.HasValue)
            {
                query = query.Where(s => s.CreateDate <= baseSearchDto.DateTo);
            }
            if (baseSearchDto.HasCancelRequestDate.HasValue)
            {
                query = query.Where(s => s.CancelRequestDate.HasValue == baseSearchDto.HasCancelRequestDate);
            }
            if (baseSearchDto.HasReserveDate.HasValue)
            {
                query = query.Where(s => s.ReserveDate.HasValue == baseSearchDto.HasReserveDate);
            }
            if (baseSearchDto.HasParentOrderId.HasValue)
            {
                query = query.Where(s => (!string.IsNullOrEmpty(s.ParentOrderId)) == baseSearchDto.HasParentOrderId);
            }
            if (baseSearchDto.HasChildOrderId.HasValue)
            {
                query = query.Where(s => (!string.IsNullOrEmpty(s.ChildOrderId)) == baseSearchDto.HasChildOrderId);
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.Default:
                    {
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.New:
                    {
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        query = query.OrderBy(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Expensive:
                    {
                        query = query.OrderByDescending(s => s.Price);
                        break;
                    }
                case Common.Enumerable.SortEnum.Inexpensive:
                    {
                        query = query.OrderBy(s => s.Price);
                        break;
                    }
                default:
                    break;
            }

            return new ProductOrderSearchDto(baseSearchDto, query, mapper);
        }

        public async Task<BaseResultDto> ProductPaymentCallback(string productOrderId, bool fromWallet = false)
        {
            var productOrder = await _context.ProductOrders.AsTracking().Include(s => s.User).Include(s => s.Address).Include(s => s.Rebate).Include(s => s.ProductOrderStores).ThenInclude(s => s.Store).Include(s => s.ProductOrderStores).ThenInclude(s => s.ProductOrderItems).ThenInclude(s => s.ProductItem).ThenInclude(s => s.Product).FirstOrDefaultAsync(s => s.Id == productOrderId);
            if (productOrder == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (productOrder.IsPaid)
                return new BaseResultDto(true);

            if (productOrder.ClubFreeDeliveryBenefitId.HasValue && productOrder.ClubDeliveryDiscount > 0)
            {
                var now = DateTimeOffset.UtcNow;
                var benefit = await _context.ClubFreeDeliveryBenefits.AsTracking()
                    .Include(item => item.RewardRedemption)
                        .ThenInclude(item => item.RewardTemplate)
                    .FirstOrDefaultAsync(item =>
                        item.Id == productOrder.ClubFreeDeliveryBenefitId.Value &&
                        item.UserId == productOrder.UserId &&
                        item.RemainingUsageCount > 0 &&
                        item.ExpiresAt > now);
                if (benefit == null)
                    return new BaseResultDto(false, "CLUB_FREE_DELIVERY_NOT_AVAILABLE");

                benefit.RemainingUsageCount--;
                _context.ClubRewardCostTransactions.Add(new Entities.Entities.PastilClubField.ClubRewardCostTransaction
                {
                    RewardRedemptionId = benefit.RewardRedemptionId,
                    UserId = productOrder.UserId,
                    BusinessType = Entities.Entities.PastilClubField.ClubRewardTargetTypeEnum.Store,
                    BusinessId = benefit.StoreId,
                    RewardType = Entities.Entities.PastilClubField.ClubRewardTypeEnum.FreeDelivery,
                    GrossValue = Convert.ToDecimal(productOrder.ClubDeliveryDiscount),
                    PastilFundedValue = Convert.ToDecimal(productOrder.ClubDeliveryDiscount),
                    OrderId = productOrder.Id,
                    CreateDate = DateTime.UtcNow
                });
            }

            if (fromWallet && productOrder.WalletPrice > 0)
            {
                var walletItem = new WalletDto() { Painding = false, Amount = productOrder.WalletPrice, UserId = productOrder.UserId, ProductOrderId = productOrder.Id };
                var walletResult = await _walletService.InsertUpdateProductOrderAsync(walletItem, true);
                if (!walletResult.IsSuccess)
                    return new BaseResultDto(false);
            }
            if (productOrder.Rebate != null)
            {
                _rebateService.IncreaseUseCount(productOrder);
            }
            productOrder.IsPaid = true;
            await UpdateProductOrderCommissionDto(productOrder);
            double scoreRatio = 10000;
            double earnedScore = Math.Floor(productOrder.PaymentPrice / scoreRatio);

            if (earnedScore > 0)
            {
                await _scoreService.AddScoreAsync(
                    userId: productOrder.UserId,
                    amount: earnedScore,
                    type: ScoreTransactionType.ScoreTransactionType_ProductOrder,
                    referenceId: productOrder.Id.ToString()
                );
            }
            await _context.SaveChangesAsync();
            try
            {
                await _shipmentService.CreateForPaidOrderAsync(productOrder);
            }
            catch
            {
                // پرداخت موفق نباید به دلیل اختلال سرویس حمل‌ونقل ناموفق اعلام شود.
            }
            var cart = await _context.Carts.AsTracking().Include(s => s.CartStores.Where(a => a.Active)).ThenInclude(s => s.CartItems).FirstOrDefaultAsync(s => s.UserId == productOrder.UserId);
            if (cart != null)
            {
                cart.DeliveryId = null;
                foreach (var item in cart.CartStores.ToList())
                {
                    _context.CartItems.RemoveRange(item.CartItems);
                    _context.CartStores.Remove(item);
                }
                await _context.SaveChangesAsync();
            }
            await _userProductService.InsertOrderItemAsyncDto(productOrder);
            await _productService.IncreaseSellCountAsync(productOrder);
            string nameText = string.Format("{0}_{1}", productOrder.User.FirstName, productOrder.User.LastName).Replace(" ", "_");

            string bonusCode = productOrder.ReferralCode;

            if (!string.IsNullOrEmpty(bonusCode))
            {
                await AddBonusAmountToWalletAsync(productOrder);
            }
            var orderUrl = productOrder.Id;

            // ثبت سفارش: ایمیل نمی‌رود (فقط پیامک). نام فروشگاه(ها) در token4 (= %token10 کاوه‌نگار) برای کاربر و ادمین؛
            // پیامک فروشگاه فقط نام کاربر و شماره سفارش دارد و فقط یک‌بار به هر شماره‌ی موبایل می‌رود.
            var storeNamesText = RegisterOrderStoreNames(productOrder);
            await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.UserRegisterOrder, mobileReceptor: productOrder.User.Mobile, emailReceptor: null, token1: nameText, token2: productOrder.OrderCode, token3: orderUrl, token4: storeNamesText);
            await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.AdminRegisterOrder, mobileReceptor: _adminSettingHelperService.BaseAdminSetting.AdminMobiles, emailReceptor: null, token1: nameText, token2: productOrder.OrderCode, token4: storeNamesText);
            await SendRegisterOrderPushesAsync(productOrder, nameText);
            var orderId = long.Parse(productOrder.Id);
            await _notificationService.CreateAsync(new NoticeCreateDto
            {
                Label = NoticeTypeLabels.ProductOrderRegistered,
                ActorUserId = productOrder.UserId,
                ReferenceType = "ProductOrder",
                ReferenceId = orderId,
                DeduplicationKey = $"{NoticeTypeLabels.ProductOrderRegistered}:{productOrder.Id}",
                Metadata = new Dictionary<string, string> { { "userName", $"{productOrder.User.FirstName} {productOrder.User.LastName}".Trim() }, { "orderId", productOrder.Id }, { "mobile", productOrder.User.Mobile } }
            });

            // پیامک فقط به شماره‌ی خودِ فروشگاه (مالک) و یک‌بار برای هر شماره، حتی اگر چند فروشگاه سفارش یک شماره دارند
            var storeMobiles = productOrder.ProductOrderStores
                .Select(productOrderStore => productOrderStore.Store?.Mobile?.Trim())
                .Where(mobile => !string.IsNullOrEmpty(mobile))
                .Distinct();
            foreach (var mobile in storeMobiles)
            {
                await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.StoreRegisterOrder, mobileReceptor: mobile, emailReceptor: null, token1: nameText, token2: productOrder.OrderCode);
            }
            return new BaseResultDto(true);
        }

        // نام فروشگاه(های) سفارش برای توکن پیامک: توکن‌های کاوه‌نگار فاصله نمی‌پذیرند و SmsService فاصله را «_» می‌کند؛
        // پس فاصله‌ها از قبل با نیم‌فاصله (ZWNJ) عوض می‌شوند. چند فروشگاه با «،» جدا می‌شوند.
        private static string RegisterOrderStoreNames(ProductOrder productOrder)
        {
            var names = productOrder.ProductOrderStores
                .Select(productOrderStore => productOrderStore.Store?.Name?.Trim())
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct();
            var text = string.Join("،", names).Replace(' ', '‌');
            return text.Length > 100 ? text.Substring(0, 100) : text;
        }

        // پوش ثبت سفارش فقط برای فروشگاه (کاربران همان فروشگاه‌ها) و ادمین؛ برای مشتری پوش نمی‌رود.
        private async Task SendRegisterOrderPushesAsync(ProductOrder productOrder, string nameText)
        {
            try
            {
                var storeIds = productOrder.ProductOrderStores.Select(productOrderStore => productOrderStore.StoreId).Distinct().ToList();
                var storeUserIds = await _context.Stores.AsNoTracking()
                    .Where(store => storeIds.Contains(store.Id))
                    .SelectMany(store => store.Users)
                    .Where(user => !user.Deleted && !user.Locked)
                    .Select(user => user.Id)
                    .Distinct()
                    .ToListAsync();
                var adminUserIds = await _context.Users.AsNoTracking()
                    .Where(user => user.RoleId == (long)RoleEnum.Admin && !user.Deleted && !user.Locked)
                    .Select(user => user.Id)
                    .ToListAsync();

                foreach (var userId in storeUserIds)
                    await SendOrderPushSafeAsync(PushTypeEnum.PushRegisterOrderStore, userId, nameText, productOrder.OrderCode);
                foreach (var userId in adminUserIds.Except(storeUserIds))
                    await SendOrderPushSafeAsync(PushTypeEnum.PushRegisterOrderAdmin, userId, nameText, productOrder.OrderCode);
            }
            catch
            {
                // پوش فروشگاه/ادمین best-effort است؛ ثبت سفارش نباید به‌خاطرش خراب شود.
            }
        }

        private async Task SendOrderPushSafeAsync(PushTypeEnum pushType, long userId, string nameText, string orderCode)
        {
            try
            {
                await _pushNotificationService.SendPushAsync(pushType, userId, token1: nameText, token2: orderCode);
            }
            catch
            {
                // یک گیرنده‌ی خراب بقیه را متوقف نکند.
            }
        }

        public Task UpdateProductOrderCommissionDto(ProductOrder order)
        {
            if (order == null || order.StoreShare > 0 || order.SiteShare > 0)
                return Task.CompletedTask;

            decimal totalStoreShare = 0m;
            decimal totalSiteShare = 0m;

            foreach (var s in order.ProductOrderStores ?? Enumerable.Empty<ProductOrderStore>())
            {
                if (s.Store == null || s.PaymentPrice <= 0)
                    continue;

                decimal percent = s.Store.CommissionPercent;
                if (percent < 0 || percent > 100)
                    continue;

                decimal payment = (decimal)s.PaymentPrice;
                decimal siteShare = (payment * percent) / 100m;
                decimal storeShare = payment - siteShare;

                totalStoreShare += storeShare;
                totalSiteShare += siteShare;
            }

            if (totalStoreShare == 0 && totalSiteShare == 0)
                return Task.CompletedTask;

            order.StoreShare = (double)totalStoreShare;
            order.SiteShare = (double)totalSiteShare;

            return Task.CompletedTask;
        }
        public async Task<BaseResultDto> ChangeStatusAsync(ProductOrderDto dto, bool allowMiareOverride = false)
        {
            var item = await _context.ProductOrders.AsTracking().Include(s => s.User).FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            // مقدار باید یکی از وضعیت‌های تعریف‌شده باشد؛ قبلاً هر شناسه‌ی دلخواهی (حتی کدهای نامرتبط) ذخیره می‌شد.
            var statusLabels = Enum.GetNames(typeof(ProductOrderStatusEnum));
            if (!await _context.Codes.AnyAsync(c => c.Id == dto.ProductOrderStatusId && statusLabels.Contains(c.Label)))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            // «تحویل داده شد» (نهایی‌شدن سفارش) فقط با تأیید خود کاربر ثبت می‌شود؛ فروشگاه/ادمین نمی‌توانند آن را بزنند
            // (ConfirmDeliveryAsync / PUT api/EndUser/ProductOrderDelivery). سفارشی که کاربر «تحویل گرفتم» زده هم دیگر عوض نمی‌شود.
            var deliveredId = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Delivered.ToString());
            if (dto.ProductOrderStatusId == deliveredId && item.ProductOrderStatusId != deliveredId)
                return new BaseResultDto(false, Resource.Notification.ProductOrderDeliveredOnlyByUser);
            if (!ProductOrderDeliveryRules.StaffMayChangeStatus(item.ProductOrderStatusId, dto.ProductOrderStatusId, deliveredId, item.UserReceived))
                return new BaseResultDto(false, Resource.Notification.ProductOrderStatusLockedByUserConfirmation);

            // «ارسال» برای سفارش میاره خودکار و با تحویل واقعی کالا به پیک (وب‌هوک) ثبت می‌شود؛ فروشنده نمی‌تواند زودتر دستی بزند
            // (وگرنه مشتری پیامک «تحویل میاره شد» می‌گیرد در حالی که پیک هنوز نیامده).
            var sendStatusId = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Send.ToString());
            if (!allowMiareOverride && dto.ProductOrderStatusId == sendStatusId && item.ProductOrderStatusId != sendStatusId &&
                await _context.ProductOrderStores.AnyAsync(os => os.ProductOrderId == item.Id && !os.Deleted && os.ShippingProvider == ShippingProviderEnum.Miare))
                return new BaseResultDto(false, Resource.Notification.ShipmentMiareManualShipBlocked);

            var previousStatusId = item.ProductOrderStatusId;
            item.ProductOrderStatus = null;
            item.ProductOrderStatusId = dto.ProductOrderStatusId;
            var statusProccess = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Proccess.ToString());
            var statusSend = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Send.ToString());
            var statusDelivered = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Delivered.ToString());
            var canceledState = await _codeService.GetIdByLabelAsync(ProductOrderStateEnum.ProductOrderState_Canceled.ToString());
            // مبنای تأیید خودکار ۷ روزه: لحظه‌ی ورود به «ارسال شده» (ثبت دوباره‌ی همان وضعیت ساعت را عوض نمی‌کند؛ خروج از آن پاکش می‌کند)
            if (dto.ProductOrderStatusId == statusSend)
            {
                if (previousStatusId != statusSend || item.SentDate == null)
                    item.SentDate = DateTime.Now;
            }
            else
                item.SentDate = null;
            if (dto.ProductOrderStatusId == statusProccess)
            {
                await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushProccessOrderUser, userId: item.UserId, token1: item.User.FirstName, token2: item.Id);
            }
            // پوش «ارسال شد» عمومی حذف شد: میاره پوش کد تحویل دارد (ShipmentService) و پست پوش ندارد.
            _context.ProductOrders.Update(item);
            await _context.SaveChangesAsync();

            if (dto.ProductOrderStatusId == statusDelivered &&
                item.ProductOrderStateId != canceledState &&
                item.IsPaid)
                await _clubPointIntegrationService.ProductOrderCompletedAsync(item.UserId, item.Id);

            // «ارسال» با پست: پیامک اختصاصی (فقط یک‌بار، با شماره‌ی پیگیری) جای پیامک عمومی تغییر وضعیت می‌نشیند
            var dedicatedShippedSms = dto.ProductOrderStatusId == statusSend && previousStatusId != statusSend
                && await TrySendPostShippedAsync(item.Id);
            if (!dedicatedShippedSms)
            await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.ProductOrderChangeStatus, mobileReceptor: item.User.Mobile, emailReceptor: null, token1: item.User.FirstName, token2: item.OrderCode);
            return new BaseResultDto(true);
        }
        // تحویل‌گیری توسط خود کاربر (طراحی: backend/Docs/PRODUCT_ORDER_USER_DELIVERY_FA.md)
        public async Task<BaseResultDto> ConfirmDeliveryAsync(string orderId, long userId, bool received, string note)
        {
            try
            {
                note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
                if (note != null && note.Length > 500)
                    note = note.Substring(0, 500);

                var canceledLabel = ProductOrderStateEnum.ProductOrderState_Canceled.ToString();
                var sendLabel = ProductOrderStatusEnum.ProductOrderStatus_Send.ToString();
                var order = await _context.ProductOrders.AsNoTracking()
                    .Where(o => o.Id == orderId && o.UserId == userId && !o.Deleted)
                    .Select(o => new
                    {
                        o.IsPaid,
                        o.UserReceived,
                        ReceiptAsked = o.ReceiptAskedAtUtc != null,
                        o.ProductOrderStatusId,
                        StatusLabel = o.ProductOrderStatus.Label,
                        StateLabel = o.ProductOrderState.Label
                    })
                    .FirstOrDefaultAsync();
                if (order == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                // سفارش میاره با اعلام کد خودکار «تحویل شد» می‌شود؛ ولی پیک گاهی بدون گرفتن کد تحویل صوری می‌زند. تا ۳ ساعت بعد از
                // تحویل (مهلت گزارش مشکل در میاره) مشتری هنوز می‌تواند «تحویل نگرفتم» بزند.
                if (!received && order.UserReceived == true &&
                    order.StatusLabel == ProductOrderStatusEnum.ProductOrderStatus_Delivered.ToString() &&
                    order.StateLabel != canceledLabel)
                {
                    var deliveredAt = await _context.Shipments.AsNoTracking()
                        .Where(sh => sh.ProductOrderStore.ProductOrderId == orderId && sh.Provider == ShippingProviderEnum.Miare && sh.Status == ShipmentStatusEnum.Delivered)
                        .OrderByDescending(sh => sh.DeliveredAtUtc)
                        .Select(sh => sh.DeliveredAtUtc)
                        .FirstOrDefaultAsync();
                    if (Application.Services.Order.ShippingSrv.ShipmentLifecycleRules.InDisputeWindow(deliveredAt, DateTime.UtcNow))
                        return await DisputeCourierDeliveryAsync(orderId, userId, note);
                }
                switch (ProductOrderDeliveryRules.Decide(order.IsPaid, order.StatusLabel, order.StateLabel, order.UserReceived, order.ReceiptAsked))
                {
                    case ProductOrderDeliveryRules.Decision.AlreadyConfirmed:
                        return new BaseResultDto(false, Resource.Notification.ProductOrderDeliveryAlreadyConfirmed);
                    case ProductOrderDeliveryRules.Decision.NotAllowed:
                        return new BaseResultDto(false, Resource.Notification.ProductOrderDeliveryNotAllowed);
                }

                var now = DateTime.Now;
                int affected;
                if (received)
                {
                    var deliveredId = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Delivered.ToString());
                    // گذار اتمی: فقط از «ارسال‌شده» و تا وقتی نهایی نشده؛ دوبار کلیک یا هم‌زمانی دوباره چیزی عوض نمی‌کند
                    affected = await _context.ProductOrders
                        .Where(o => o.Id == orderId && o.UserId == userId && o.UserReceived != true && o.ProductOrderStatusId == order.ProductOrderStatusId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(o => o.UserReceived, (bool?)true)
                            .SetProperty(o => o.UserReceivedDate, (DateTime?)now)
                            .SetProperty(o => o.UserReceiveNote, (string)null)
                            .SetProperty(o => o.ProductOrderStatusId, deliveredId));
                    if (affected == 0)
                        return new BaseResultDto(false, Resource.Notification.ProductOrderDeliveryNotAllowed);

                    // همان اثر جانبی «تحویل داده شد» که قبلاً ChangeStatusAsync داشت (امتیاز باشگاه)؛ خطا نباید تأیید کاربر را خراب کند
                    await NotifyOrderReceivedAsync(orderId); // پیامک/پوش به ادمین و فروشنده
                    try { await _clubPointIntegrationService.ProductOrderCompletedAsync(userId, orderId); }
                    catch { /* در اجرای بعدی/پشتیبانی جبران می‌شود */ }
                }
                else
                {
                    affected = await _context.ProductOrders
                        .Where(o => o.Id == orderId && o.UserId == userId && o.UserReceived != true && o.ProductOrderStatusId == order.ProductOrderStatusId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(o => o.UserReceived, (bool?)false)
                            .SetProperty(o => o.UserReceivedDate, (DateTime?)now)
                            .SetProperty(o => o.UserReceiveNote, note));
                    if (affected == 0)
                        return new BaseResultDto(false, Resource.Notification.ProductOrderDeliveryNotAllowed);
                    await NotifyStoresNotReceivedAsync(orderId);
                    if (order.UserReceived != false)
                        await NotifyAdminsNotReceivedAsync(orderId);
                }
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        // پوش به همه‌ی کاربران فروشگاه(های) سفارش وقتی مشتری «تحویل نگرفتم» می‌زند؛ یک‌بار برای هر سفارش و هر گیرنده.
        // خطا هرگز ثبت پاسخ مشتری را خراب نمی‌کند.
        private async Task NotifyStoresNotReceivedAsync(string orderId)
        {
            try
            {
                var info = await _context.ProductOrders.AsNoTracking()
                    .Where(o => o.Id == orderId)
                    .Select(o => new
                    {
                        o.OrderCode,
                        CustomerName = (o.User.FirstName + " " + o.User.LastName).Trim(),
                        StoreIds = o.ProductOrderStores.Where(s => !s.Deleted).Select(s => s.StoreId).ToList()
                    })
                    .FirstOrDefaultAsync();
                if (info == null || info.StoreIds.Count == 0)
                    return;

                var recipients = await _context.Stores.AsNoTracking()
                    .Where(s => info.StoreIds.Contains(s.Id))
                    .SelectMany(s => s.Users)
                    .Select(u => u.Id)
                    .Distinct()
                    .ToListAsync();

                var key = ProductOrderDeliveryRules.NotReceivedKey(orderId);
                var typeId = (long)PushTypeEnum.PushProductOrderNotReceivedStore;
                foreach (var userId in recipients)
                {
                    try
                    {
                        var already = await _context.PushNotifications.AsNoTracking()
                            .AnyAsync(n => n.UserId == userId && n.Token2 == key && n.PushPattern.PushTypeId == typeId);
                        if (already)
                            continue;
                        await _pushNotificationService.SendPushAsync(PushTypeEnum.PushProductOrderNotReceivedStore, userId,
                            token1: info.CustomerName, token2: key, token3: info.OrderCode);
                    }
                    catch { /* یک گیرنده‌ی خراب بقیه را متوقف نکند */ }
                }
            }
            catch { /* اعلان فروشگاه best-effort است */ }
        }

        // job (هر ساعت): تأیید خودکار تحویل بعد از ۷ روز بدون پاسخ کاربر + هشدار ۲ روز قبل. طراحی: backend/Docs/PRODUCT_ORDER_USER_DELIVERY_FA.md
        // «تحویل نگرفتم» هرگز خودکار تأیید نمی‌شود. تعداد سفارش‌های خودکار تأییدشده را برمی‌گرداند.
        public async Task<int> AutoConfirmDeliveriesAsync()
        {
            var now = DateTime.Now;
            var sendId = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Send.ToString());
            var deliveredId = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Delivered.ToString());
            var canceledId = await _codeService.GetIdByLabelAsync(ProductOrderStateEnum.ProductOrderState_Canceled.ToString());

            // ۱) سفارش‌های قدیمیِ «ارسال شده» که زمان ارسالشان ثبت نشده: ساعت از همین لحظه شروع می‌شود تا روز اول دیپلوی هیچ سفارشی یک‌جا خودکار تأیید نشود
            await _context.ProductOrders
                .Where(o => !o.Deleted && o.ProductOrderStatusId == sendId && o.SentDate == null)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.SentDate, (DateTime?)now));

            var dueBefore = now.AddDays(-ProductOrderDeliveryRules.AutoConfirmAfterDays);
            var warnBefore = now.AddDays(-ProductOrderDeliveryRules.WarnAfterDays);

            // ۲) هشدار (فقط یک‌بار برای هر سفارش): از روز پنجم تا قبل از تأیید خودکار، برای کسی که هنوز پاسخ نداده
            var toWarn = await _context.ProductOrders.AsNoTracking()
                .Where(o => !o.Deleted && o.IsPaid && o.UserReceived == null && o.ProductOrderStatusId == sendId && o.ProductOrderStateId != canceledId &&
                            o.SentDate != null && o.SentDate > dueBefore && o.SentDate <= warnBefore)
                .OrderBy(o => o.SentDate)
                .Select(o => new { o.Id, o.UserId, o.OrderCode, SentDate = o.SentDate.Value })
                .Take(200)
                .ToListAsync();
            foreach (var order in toWarn)
            {
                try
                {
                    var key = ProductOrderDeliveryRules.WarnKey(order.Id);
                    var typeId = (long)PushTypeEnum.PushProductOrderAutoDeliveryWarning;
                    var already = await _context.PushNotifications.AsNoTracking()
                        .AnyAsync(n => n.UserId == order.UserId && n.Token2 == key && n.PushPattern.PushTypeId == typeId);
                    if (already)
                        continue;
                    var calendar = new System.Globalization.PersianCalendar();
                    var at = ProductOrderDeliveryRules.AutoConfirmDate(order.SentDate);
                    var atText = string.Create(System.Globalization.CultureInfo.InvariantCulture,
                        $"{calendar.GetYear(at)}/{calendar.GetMonth(at):D2}/{calendar.GetDayOfMonth(at):D2}");
                    await _pushNotificationService.SendPushAsync(PushTypeEnum.PushProductOrderAutoDeliveryWarning, order.UserId,
                        token1: null, token2: key, token3: order.OrderCode, token4: atText);
                }
                catch { /* اجرای بعدی دوباره تلاش می‌کند */ }
            }

            // ۳) تأیید خودکار: گذار اتمی فقط برای «بدون پاسخ» و «ارسال شده»؛ «تحویل نگرفتم» (UserReceived=false) را لمس نمی‌کند
            var due = await _context.ProductOrders.AsNoTracking()
                .Where(o => !o.Deleted && o.IsPaid && o.UserReceived == null && o.ProductOrderStatusId == sendId && o.ProductOrderStateId != canceledId &&
                            o.SentDate != null && o.SentDate <= dueBefore)
                .OrderBy(o => o.SentDate)
                .Select(o => new { o.Id, o.UserId, o.OrderCode })
                .Take(100)
                .ToListAsync();

            var confirmed = 0;
            foreach (var order in due)
            {
                try
                {
                    var affected = await _context.ProductOrders
                        .Where(o => o.Id == order.Id && o.UserReceived == null && o.ProductOrderStatusId == sendId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(o => o.UserReceived, (bool?)true)
                            .SetProperty(o => o.UserReceivedDate, (DateTime?)now)
                            .SetProperty(o => o.UserReceivedAuto, true)
                            .SetProperty(o => o.UserReceiveNote, (string)null)
                            .SetProperty(o => o.ProductOrderStatusId, deliveredId));
                    if (affected == 0)
                        continue;
                    confirmed++;

                    try { await _clubPointIntegrationService.ProductOrderCompletedAsync(order.UserId, order.Id); } catch { }
                    try
                    {
                        var key = ProductOrderDeliveryRules.AutoKey(order.Id);
                        await _pushNotificationService.SendPushAsync(PushTypeEnum.PushProductOrderAutoDelivered, order.UserId,
                            token1: null, token2: key, token3: order.OrderCode);
                    }
                    catch { }
                }
                catch { /* اجرای بعدی دوباره تلاش می‌کند */ }
            }
            return confirmed;
        }

        public async Task<BaseResultDto> ChangeStateAsync(ProductOrderDto dto)
        {
            var item = await _context.ProductOrders.FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            var stateLabels = Enum.GetNames(typeof(ProductOrderStateEnum));
            if (!await _context.Codes.AnyAsync(c => c.Id == dto.ProductOrderStateId && stateLabels.Contains(c.Label)))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            item.ProductOrderState = null;
            item.ProductOrderStateId = dto.ProductOrderStateId;

            _context.ProductOrders.Update(item);
            await _context.SaveChangesAsync();

            var canceledState = await _codeService.GetIdByLabelAsync(ProductOrderStateEnum.ProductOrderState_Canceled.ToString());
            if (dto.ProductOrderStateId == canceledState)
                await _clubPointIntegrationService.ProductOrderReversedAsync(item.UserId, item.Id);

            return new BaseResultDto(true);
        }
        public async Task<BaseResultDto> ChangeTrackingCode(ProductOrderDto order)
        {
            var productOrder = await _context.ProductOrders.Include(s => s.DeliveryType).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == order.Id);
            if (productOrder != null)
            {
                productOrder.TrackingCode = order.TrackingCode;
                _context.ProductOrders.Update(productOrder);
                _context.SaveChanges();
                // پیامک رهگیری جدا حذف شد: «تحویل پست داده شد» (ProductOrderShippedPost) فقط یک‌بار و فقط وقتی وضعیت «ارسال» است
                // می‌رود؛ اگر کد رهگیری بعد از زدن «ارسال» ثبت شود، همین‌جا ارسال می‌شود.
                var sendId = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Send.ToString());
                if (!string.IsNullOrEmpty(productOrder.TrackingCode) && productOrder.ProductOrderStatusId == sendId)
                    await TrySendPostShippedAsync(productOrder.Id);
            }
            return new BaseResultDto(true);
        }
        public async Task<BaseResultDto> ChangeDescriptions(ProductOrderDto order)
        {
            var productOrder = await _context.ProductOrders.FindAsync(order.Id);
            if (productOrder != null)
            {
                productOrder.AdminDescription = order.AdminDescription;
                productOrder.UserDescription = order.UserDescription;
                _context.ProductOrders.Update(productOrder);
                _context.SaveChanges();

            }
            return new BaseResultDto(true);
        }
        public async Task UpdateWalletAsync(string productOrderId, bool complete)
        {
            await _walletService.InsertUpdateProductOrderAsync(new WalletDto { ProductOrderId = productOrderId }, complete: complete);
        }

        public async Task<BaseResultDto> AddBonusAmountToWalletAsync(ProductOrder productOrder)
        {
            var user = await _userService.GetUserByReferralCodeAsync(productOrder.ReferralCode);
            if (user == null)
            {
                return new BaseResultDto(false, Resource.Notification.UserWithTheProvidedBonusCodeNotFound);
            }

            var bonusReference = $"ReferralBonus:{productOrder.Id}";
            var existingWallet = await _context.Wallets.AsNoTracking()
                .FirstOrDefaultAsync(w => w.Name == bonusReference && w.UserId == user.Id && !w.Deleted);
            if (existingWallet != null)
            {
                return new BaseResultDto(true, Resource.Notification.BonusHasAlreadyBeenAddedToTheWalletForThisProductOrder);
            }

            var bonusAmount = productOrder.Price * _adminSettingHelperService.BaseAdminSetting.BonusPercent;
            int bonus = (int)bonusAmount;
            if (bonus <= 0)
                return new BaseResultDto(true);

            var wallet = new WalletDto
            {
                Name = bonusReference,
                Amount = bonus,
                IsIncrease = true,
                UserId = user.Id,
                Painding = false
            };

            var result = await _walletService.InsertAsyncDto(wallet);
            return new BaseResultDto(result.IsSuccess,
                result.IsSuccess ? Resource.Notification.BonusAmountAddedToWalletSuccessfully : Resource.Notification.Unsuccess);
        }
        public BaseResultDto<List<ProductOrderVDto>> GetReserved(long userId, long addressId)
        {
            var items = _context.ProductOrders.Where(s => s.UserId == userId && s.AddressId == addressId && string.IsNullOrEmpty(s.ChildOrderId) && s.ReserveDate.HasValue && s.ReserveDate.Value > DateTime.Now && s.ProductOrderState.Label == ProductOrderStateEnum.ProductOrderState_Normal.ToString() && s.ProductOrderStatus.Label == ProductOrderStatusEnum.ProductOrderStatus_Insert.ToString() && s.IsPaid);
            return new BaseResultDto<List<ProductOrderVDto>>(items.Any(), mapper.Map<List<ProductOrderVDto>>(items));
        }

        public async Task<BaseResultDto> SetCancelRequestAsync(ProductOrderDto productOrder)
        {
            var item = await _context.ProductOrders.FirstOrDefaultAsync(s => s.Id == productOrder.Id && s.UserId == productOrder.UserId);
            if (item != null)
            {
                if (item.IsPaid && item.CancelRequestDate == null)
                {
                    item.CancelRequestDate = DateTime.Now;
                    item.UserDescription = productOrder.UserDescription;
                    _context.ProductOrders.Update(item);
                    _context.SaveChanges();
                    await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.ProductOrderCancelRequest, mobileReceptor: _adminSettingHelperService.BaseAdminSetting.AdminMobiles, emailReceptor: item.User.Email, token1: item.OrderCode);

                    return new BaseResultDto(true);
                }

            }
            return new BaseResultDto(false, val: Resource.Notification.InvalidData);

        }
        public async Task<BaseResultDto> AnswerCancelRequestAsync(ProductOrderDto productOrder)
        {
            var item = await _context.ProductOrders.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == productOrder.Id);
            if (item != null)
            {
                item.ProductOrderStateId = productOrder.ProductOrderStateId;
                item.AdminDescription = productOrder.AdminDescription;
                _context.ProductOrders.Update(item);
                await _context.SaveChangesAsync();

                var canceledState = await _codeService.GetIdByLabelAsync(ProductOrderStateEnum.ProductOrderState_Canceled.ToString());
                if (productOrder.ProductOrderStateId == canceledState)
                {
                    await _clubPointIntegrationService.ProductOrderReversedAsync(item.UserId, item.Id);
                    await _shipmentService.CancelForOrderAsync(item.Id);
                }

                await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.ProductOrderCancelAnswer, mobileReceptor: item.User.Mobile, emailReceptor: item.User.Email, token1: item.OrderCode);
                return new BaseResultDto(true);
            }
            return new BaseResultDto(false, val: Resource.Notification.InvalidData);

        }

        public async Task<BaseResultDto> UpdatePermittedAsyncDto(string id)
        {
            var item = await _context.ProductOrders.AsTracking().FirstOrDefaultAsync(s => s.Id == id);
            item.Permitted = true;
            _context.ProductOrders.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        // پیامک «تحویل پست داده شد»: فقط یک‌بار برای هر سفارش، فقط وقتی کد رهگیری ثبت شده و فروشگاهی با ارسال غیرمیاره دارد.
        // (فروشگاه‌های میاره پیام ارسال خودشان را از ShipmentService می‌گیرند.) true یعنی پیامک اختصاصی فرستاده شد.
        private async Task<bool> TrySendPostShippedAsync(string orderId)
        {
            try
            {
                var order = await _context.ProductOrders.AsNoTracking()
                    .Where(o => o.Id == orderId)
                    .Select(o => new { o.OrderCode, o.TrackingCode, o.PostShippedNotifiedAtUtc, o.User.FirstName, o.User.Mobile })
                    .FirstOrDefaultAsync();
                if (order == null || order.PostShippedNotifiedAtUtc != null || string.IsNullOrWhiteSpace(order.TrackingCode) || string.IsNullOrEmpty(order.Mobile))
                    return false;

                var storeNames = await _context.ProductOrderStores.AsNoTracking()
                    .Where(os => os.ProductOrderId == orderId && !os.Deleted && os.ShippingProvider != ShippingProviderEnum.Miare)
                    .Select(os => os.Store.Name)
                    .ToListAsync();
                if (storeNames.Count == 0)
                    return false;

                // اول claim (گذار اتمی) تا دوبار کلیک/دوبار ثبت کد رهگیری دو پیامک نسازد
                var now = DateTime.UtcNow;
                var claimed = await _context.ProductOrders
                    .Where(o => o.Id == orderId && o.PostShippedNotifiedAtUtc == null)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.PostShippedNotifiedAtUtc, (DateTime?)now));
                if (claimed == 0)
                    return false;

                // قالب کاوه‌نگار: %token = نام، %token2 = شماره سفارش (متن و لینک)، %token3 = شماره پیگیری، %token10 = فروشگاه
                await _messageSenderService.SendMessageAsync(
                    messageType: MessageTypeEnum.ProductOrderShippedPost,
                    mobileReceptor: order.Mobile,
                    emailReceptor: null,
                    token1: Application.Services.Order.CartSrv.AbandonedCartReminderRules.FirstNameOnly(order.FirstName) ?? Resource.Notification.AbandonedCartDefaultCustomerName,
                    token2: order.OrderCode,
                    token3: order.TrackingCode.Trim(),
                    token4: Application.Services.Order.ShippingSrv.ShipmentLifecycleRules.ForSmsToken(string.Join("،", storeNames.Distinct())));
                return true;
            }
            catch
            {
                return false; // پیامک best-effort است؛ تغییر وضعیت نباید به‌خاطرش خراب شود
            }
        }

        // «تحویل نگرفتم» بعد از تحویل میاره (داخل ۳ ساعت): سفارش به «ارسال» برمی‌گردد، امتیاز باشگاه برگشت می‌خورد، فروشگاه و ادمین خبر می‌شوند.
        private async Task<BaseResultDto> DisputeCourierDeliveryAsync(string orderId, long userId, string note)
        {
            var sendLabel = ProductOrderStatusEnum.ProductOrderStatus_Send.ToString();
            var deliveredLabel = ProductOrderStatusEnum.ProductOrderStatus_Delivered.ToString();
            var ids = await _context.Codes.AsNoTracking()
                .Where(c => c.Label == sendLabel || c.Label == deliveredLabel)
                .ToDictionaryAsync(c => c.Label, c => c.Id);
            var sendId = ids.GetValueOrDefault(sendLabel);
            var deliveredId = ids.GetValueOrDefault(deliveredLabel);
            var now = DateTime.Now;
            var affected = await _context.ProductOrders
                .Where(o => o.Id == orderId && o.UserId == userId && o.UserReceived == true && o.ProductOrderStatusId == deliveredId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(o => o.UserReceived, (bool?)false)
                    .SetProperty(o => o.UserReceivedDate, (DateTime?)now)
                    .SetProperty(o => o.UserReceiveNote, note)
                    .SetProperty(o => o.ProductOrderStatusId, sendId));
            if (affected == 0)
                return new BaseResultDto(false, Resource.Notification.ProductOrderDeliveryNotAllowed);

            try { await _clubPointIntegrationService.ProductOrderReversedAsync(userId, orderId); }
            catch { /* در پیگیری پشتیبانی جبران می‌شود */ }
            await NotifyStoresNotReceivedAsync(orderId);
            await NotifyAdminsNotReceivedAsync(orderId);
            try { await _shipmentService.ReportNotReceivedAsync(orderId); }
            catch { /* اعلان ادمین best-effort است */ }
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        private sealed class OrderPartyInfo
        {
            public string OrderCode { get; init; }
            public string CustomerName { get; init; }
            public List<(long Id, string Name, string Mobile)> Stores { get; init; }
        }

        private async Task<OrderPartyInfo> LoadOrderPartyInfoAsync(string orderId)
        {
            var info = await _context.ProductOrders.AsNoTracking()
                .Where(o => o.Id == orderId)
                .Select(o => new
                {
                    o.OrderCode,
                    First = o.User.FirstName,
                    Last = o.User.LastName,
                    Stores = o.ProductOrderStores.Where(s => !s.Deleted).Select(s => new { s.Store.Id, s.Store.Name, s.Store.Mobile }).ToList()
                })
                .FirstOrDefaultAsync();
            if (info == null)
                return null;
            return new OrderPartyInfo
            {
                OrderCode = info.OrderCode,
                CustomerName = Application.Services.Order.ShippingSrv.ShipmentLifecycleRules.ForSmsToken($"{info.First} {info.Last}"),
                Stores = info.Stores.GroupBy(s => s.Id).Select(g => (g.Key, g.First().Name, g.First().Mobile)).ToList()
            };
        }

        private async Task PushToOrderStaffAsync(OrderPartyInfo info, string orderId, PushTypeEnum storeType, PushTypeEnum adminType)
        {
            var storeIds = info.Stores.Select(s => s.Id).ToList();
            var storeUserIds = await _context.Stores.AsNoTracking()
                .Where(store => storeIds.Contains(store.Id))
                .SelectMany(store => store.Users)
                .Where(user => !user.Deleted && !user.Locked)
                .Select(user => user.Id)
                .Distinct()
                .ToListAsync();
            var adminUserIds = await _context.Users.AsNoTracking()
                .Where(user => user.RoleId == (long)RoleEnum.Admin && !user.Deleted && !user.Locked)
                .Select(user => user.Id)
                .ToListAsync();
            foreach (var userId in storeUserIds)
                await SendStaffPushSafeAsync(storeType, userId, info.OrderCode);
            foreach (var userId in adminUserIds.Except(storeUserIds))
                await SendStaffPushSafeAsync(adminType, userId, info.OrderCode);
        }

        private async Task SendStaffPushSafeAsync(PushTypeEnum type, long userId, string orderCode)
        {
            try { await _pushNotificationService.SendPushAsync(type, userId, token1: orderCode); }
            catch { /* یک گیرنده‌ی خراب بقیه را متوقف نکند */ }
        }

        // مشتری «تحویل گرفتم» زد (وضعیت سفارش همان لحظه «تحویل شد/تکمیل‌شده» شده): پیامک به ادمین (برای هر فروشگاه) و به مالک فروشگاه،
        // و پوش «سفارش ... با موفقیت به کاربر تحویل داده شد» به کاربران فروشگاه و ادمین‌ها. best-effort: خطا پاسخ مشتری را خراب نمی‌کند.
        private async Task NotifyOrderReceivedAsync(string orderId)
        {
            try
            {
                var info = await LoadOrderPartyInfoAsync(orderId);
                if (info == null || info.Stores.Count == 0)
                    return;
                var adminMobiles = _adminSettingHelperService.BaseAdminSetting?.AdminMobiles;
                foreach (var store in info.Stores)
                {
                    var storeName = Application.Services.Order.ShippingSrv.ShipmentLifecycleRules.ForSmsToken(store.Name);
                    // قالب کاوه‌نگار ProductOrderReceivedAdmin: %token = شماره سفارش، %token10 = نام فروشگاه
                    if (!string.IsNullOrWhiteSpace(adminMobiles))
                        await _messageSenderService.SendMessageAsync(MessageTypeEnum.ProductOrderReceivedAdmin, adminMobiles, null,
                            token1: info.OrderCode, token4: storeName);
                    // قالب ProductOrderReceivedStore: %token = نام کاربر، %token2 = شماره سفارش
                    if (!string.IsNullOrWhiteSpace(store.Mobile))
                        await _messageSenderService.SendMessageAsync(MessageTypeEnum.ProductOrderReceivedStore, store.Mobile.Trim(), null,
                            token1: info.CustomerName, token2: info.OrderCode);
                }
                await PushToOrderStaffAsync(info, orderId, PushTypeEnum.PushOrderReceivedStore, PushTypeEnum.PushOrderReceivedAdmin);
            }
            catch { /* اعلان‌ها best-effort است */ }
        }

        // مشتری «تحویل نگرفتم» زد: پیامک به ادمین (برای هر فروشگاه) و پوش «سفارش ... در ساعت مقرر به کاربر نرسیده است» به ادمین‌ها.
        private async Task NotifyAdminsNotReceivedAsync(string orderId)
        {
            try
            {
                var info = await LoadOrderPartyInfoAsync(orderId);
                if (info == null)
                    return;
                var adminMobiles = _adminSettingHelperService.BaseAdminSetting?.AdminMobiles;
                if (!string.IsNullOrWhiteSpace(adminMobiles))
                    foreach (var store in info.Stores)
                        // قالب ProductOrderNotReceivedAdmin: %token = نام کاربر، %token2 = شماره سفارش، %token10 = نام فروشگاه
                        await _messageSenderService.SendMessageAsync(MessageTypeEnum.ProductOrderNotReceivedAdmin, adminMobiles, null,
                            token1: info.CustomerName, token2: info.OrderCode,
                            token4: Application.Services.Order.ShippingSrv.ShipmentLifecycleRules.ForSmsToken(store.Name));
                var adminUserIds = await _context.Users.AsNoTracking()
                    .Where(user => user.RoleId == (long)RoleEnum.Admin && !user.Deleted && !user.Locked)
                    .Select(user => user.Id)
                    .ToListAsync();
                foreach (var userId in adminUserIds)
                    await SendStaffPushSafeAsync(PushTypeEnum.PushShipmentNotReceivedAdmin, userId, info.OrderCode);
            }
            catch { /* اعلان‌ها best-effort است */ }
        }
    }
}
