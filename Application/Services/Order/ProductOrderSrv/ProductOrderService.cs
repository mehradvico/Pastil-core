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
using Application.Services.Order.RebateSrv.Iface;
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

        public ProductOrderService(IDataBaseContext _context, IPushNotificationService pushNotificationService, IUserProductService userProductService,
            INoticeService notificationService, IMapper mapper, ICodeService codeService, IAdminSettingHelper adminSettingHelper, IWalletService walletService,
            IUserService userService, IProductService productService, IRebateService rebateService, IScoreTransactionService scoreService,
            IMessageSenderService messageSenderService) : base(_context, mapper)
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
        }
        public async Task<BaseResultDto> FindAsyncVDto(string id)
        {
            var item = await _context.ProductOrders.Include(s => s.User).Include(s => s.Address).Include(s => s.ProductOrderState)
                .Include(s => s.ProductOrderStatus).Include(s => s.PaymentType).Include(s => s.ProductOrderStores).ThenInclude(s => s.ProductOrderItems)
                .Include(s => s.ProductOrderStores).ThenInclude(s => s.Delivery).FirstOrDefaultAsync(s => s.Id == id);
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

                    await _context.ProductOrders.AddAsync(item);

                    await _context.SaveChangesAsync();
                    return new BaseResultDto<ProductOrderDto>(true, mapper.Map<ProductOrderDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<ProductOrderDto>(isSuccess: false, val: ex.Message, data: dto);
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
                query = query.Where(s => s.User.FirstName.Contains(baseSearchDto.Q) || s.User.LastName.Contains(baseSearchDto.Q) || s.User.Mobile.Contains(baseSearchDto.Q));
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
            var productOrder = await _context.ProductOrders.AsTracking().Include(s => s.User).Include(s => s.Rebate).Include(s => s.ProductOrderStores).ThenInclude(s => s.Store).Include(s => s.ProductOrderStores).ThenInclude(s => s.ProductOrderItems).ThenInclude(s => s.ProductItem).ThenInclude(s => s.Product).FirstOrDefaultAsync(s => s.Id == productOrderId);
            if (productOrder.Wallet != null)
            {
                var amount = await _walletService.GetAmountValueAsync(productOrder.UserId);
                if (amount >= productOrder.WalletPrice)
                {
                    var walletItem = new WalletDto() { Painding = false, Amount = productOrder.WalletPrice, UserId = productOrder.UserId, ProductOrderId = productOrder.Id };
                    await _walletService.InsertUpdateProductOrderAsync(walletItem, true);
                }
                else
                {
                    return new BaseResultDto(false);

                }
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
            var cart = await _context.Carts.AsTracking().Include(s => s.CartStores.Where(a => a.Active)).ThenInclude(s => s.CartItems).FirstOrDefaultAsync(s => s.UserId == productOrder.UserId);
            cart.DeliveryId = null;
            foreach (var item in cart.CartStores.ToList())
            {
                _context.CartItems.RemoveRange(item.CartItems);
                _context.CartStores.Remove(item);
            }
            await _context.SaveChangesAsync();
            await _userProductService.InsertOrderItemAsyncDto(productOrder);
            await _productService.IncreaseSellCountAsync(productOrder);
            _rebateService.IncreaseUseCount(productOrder);
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
            item.ProductOrderStatus = null;
            item.ProductOrderStatusId = dto.ProductOrderStatusId;
            var statusProccess = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Proccess.ToString());
            var statusSend = await _codeService.GetIdByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Send.ToString());
            if (dto.ProductOrderStatusId == statusProccess)
            {
                await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushProccessOrderUser, userId: item.UserId, token1: item.User.FirstName, token2: item.Id);
            }
            if (dto.ProductOrderStatusId == statusSend)
            {
                await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushSentOrderUser, userId: item.UserId, token1: item.User.FirstName, token2: item.Id);
            }
            _context.ProductOrders.Update(item);
            _context.SaveChanges();
            await _messageSenderService.SendMessageAsync(messageType: MessageTypeEnum.ProductOrderChangeStatus, mobileReceptor: item.User.Mobile, emailReceptor: item.User.Email, token1: item.User.FirstName, token2: item.Id);
            return new BaseResultDto(true);
        }
        public async Task<BaseResultDto> ChangeStateAsync(ProductOrderDto dto)
        {
            var item = await _context.ProductOrders.FirstOrDefaultAsync(s => s.Id == dto.Id);
            item.ProductOrderState = null;
            item.ProductOrderStateId = dto.ProductOrderStateId;

            _context.ProductOrders.Update(item);
            _context.SaveChanges();
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

            var existingWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.ProductOrderId == productOrder.Id && w.UserId == productOrder.UserId && !w.Deleted);
            if (existingWallet != null)
            {
                return new BaseResultDto(false, Resource.Notification.BonusHasAlreadyBeenAddedToTheWalletForThisProductOrder);
            }

            var bonusAmount = productOrder.Price * _adminSettingHelperService.BaseAdminSetting.BonusPercent;
            int bonus = (int)bonusAmount;

            var userWallet = await _context.Wallets.FirstOrDefaultAsync(s => s.UserId == user.Id);
            var wallet = new WalletDto
            {
                Amount = userWallet.Amount + bonus,
                IsIncrease = true,
                UserId = user.Id,
                Painding = false,
                ProductOrderId = productOrder.Id
            };

            await _walletService.InsertAsyncDto(wallet);

            return new BaseResultDto(true, Resource.Notification.BonusAmountAddedToWalletSuccessfully);
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
                _context.SaveChanges();
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
