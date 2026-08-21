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
using Microsoft.EntityFrameworkCore;
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

        public ProductOrderService(IDataBaseContext _context, IPushNotificationService pushNotificationService, IUserProductService userProductService,
            INoticeService notificationService, IMapper mapper, ICodeService codeService, IAdminSettingHelper adminSettingHelper, IWalletService walletService,
            IUserService userService, IProductService productService, IRebateService rebateService, IScoreTransactionService scoreService,
            IMessageSenderService messageSenderService,
            IClubPointIntegrationService clubPointIntegrationService,
            IShipmentService shipmentService) : base(_context, mapper)
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
        }
        public async Task<BaseResultDto> FindAsyncVDto(string id, long? userId = null)
        {
            var query = _context.ProductOrders.Include(s => s.User).Include(s => s.Address).Include(s => s.ProductOrderState)
                .Include(s => s.ProductOrderStatus).Include(s => s.PaymentType).Include(s => s.ProductOrderStores).ThenInclude(s => s.ProductOrderItems)
                .Include(s => s.ProductOrderStores).ThenInclude(s => s.Delivery).Where(s => s.Id == id && !s.Deleted);
            if (userId.HasValue)
                query = query.Where(s => s.UserId == userId.Value);
            var item = await query.FirstOrDefaultAsync();
            if (item != null)
            {
                return new BaseResultDto<ProductOrderVDto>(true, data: mapper.Map<ProductOrderVDto>(item));
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

                    await _context.ProductOrders.AddAsync(item);

                    await _context.SaveChangesAsync();
                    return new BaseResultDto<ProductOrderDto>(true, mapper.Map<ProductOrderDto>(item));
                }

            }
            catch (Exception)
            {
                return new BaseResultDto<ProductOrderDto>(isSuccess: false, val: Resource.Notification.Unsuccess, data: dto);
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

            await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.UserRegisterOrder, mobileReceptor: productOrder.User.Mobile, emailReceptor: productOrder.User.Email, token1: nameText, token2: productOrder.Id, token3: orderUrl);
            await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.AdminRegisterOrder, mobileReceptor: _adminSettingHelperService.BaseAdminSetting.AdminMobiles, emailReceptor: productOrder.User.Email, token1: nameText, token2: productOrder.Id);
            await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushRegisterOrderUser, userId: productOrder.UserId, token1: nameText, token2: productOrder.Id.ToString());
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

            foreach (var productOrderStore in productOrder.ProductOrderStores)
            {
                var store = productOrderStore.Store;
                if (store != null)
                {
                    await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.StoreRegisterOrder, mobileReceptor: store.Mobile, emailReceptor: store.Email, token1: nameText, token2: productOrder.Id);
                }
            }
            return new BaseResultDto(true);
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
        public async Task<BaseResultDto> ChangeStatusAsync(ProductOrderDto dto)
        {
            var item = await _context.ProductOrders.AsTracking().Include(s => s.User).FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            item.ProductOrderStatus = null;
            item.ProductOrderStatusId = dto.ProductOrderStatusId;
            var statusProccess = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Proccess.ToString());
            var statusSend = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Send.ToString());
            var statusDelivered = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Delivered.ToString());
            var canceledState = await _codeService.GetIdByLabelAsync(ProductOrderStateEnum.ProductOrderState_Canceled.ToString());
            if (dto.ProductOrderStatusId == statusProccess)
            {
                await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushProccessOrderUser, userId: item.UserId, token1: item.User.FirstName, token2: item.Id);
            }
            if (dto.ProductOrderStatusId == statusSend)
            {
                await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushSentOrderUser, userId: item.UserId, token1: item.User.FirstName, token2: item.Id);
            }
            _context.ProductOrders.Update(item);
            await _context.SaveChangesAsync();

            if (dto.ProductOrderStatusId == statusDelivered &&
                item.ProductOrderStateId != canceledState &&
                item.IsPaid)
                await _clubPointIntegrationService.ProductOrderCompletedAsync(item.UserId, item.Id);

            await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.ProductOrderChangeStatus, mobileReceptor: item.User.Mobile, emailReceptor: item.User.Email, token1: item.User.FirstName, token2: item.Id);
            return new BaseResultDto(true);
        }
        public async Task<BaseResultDto> ChangeStateAsync(ProductOrderDto dto)
        {
            var item = await _context.ProductOrders.FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

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
                if (!string.IsNullOrEmpty(productOrder.TrackingCode) && productOrder.DeliveryTypeId.HasValue)
                {
                    {
                        string nameText = string.Format("{0}_{1}", productOrder.User.FirstName, productOrder.User.LastName).Replace(" ", "_");
                        string trackingText = string.Format(Resource.Pattern.ProductOrderTrakingCode, productOrder.DeliveryType.Name, productOrder.TrackingCode);
                        await _messageSenderService.SendMessageAsync(messageType: Common.Enumerable.Message.MessageTypeEnum.TrackingCode, mobileReceptor: productOrder.User.Mobile, emailReceptor: productOrder.User.Email, token1: nameText, token2: productOrder.Id, token5: trackingText, sendDate: DateTime.Now);
                    }
                }
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
                    await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.ProductOrderCancelRequest, mobileReceptor: _adminSettingHelperService.BaseAdminSetting.AdminMobiles, emailReceptor: item.User.Email, token1: productOrder.Id);

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
                    await _clubPointIntegrationService.ProductOrderReversedAsync(item.UserId, item.Id);

                await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.ProductOrderCancelAnswer, mobileReceptor: item.User.Mobile, emailReceptor: item.User.Email, token1: productOrder.Id);
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
    }
}
