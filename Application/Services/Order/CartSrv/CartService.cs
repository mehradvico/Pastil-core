using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Interface;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.Dto;
using Application.Services.Order.AddressSrv.iface;
using Application.Services.Order.CartSrv.Dto;
using Application.Services.Order.CartSrv.Iface;
using Application.Services.Order.DeliverySrv.iface;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.Order.ProductOrderItemSrv.Iface;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.ProductItemSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.CodeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZarinPal.Class;

namespace Application.Services.Order.CartSrv
{
    public class CartService : ICartService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IProductItemService _productItemService;
        private readonly IRebateService _rebateService;
        private readonly IProductOrderService _productOrderService;
        private readonly IProductOrderItemService _productOrderItemService;
        private readonly ICodeService _codeService;
        private readonly IPaymentService _paymentService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IDeliveryService _deliveryService;
        private readonly IWalletService _walletService;
        private readonly IUserService _userService;
        private readonly IAddressService _addressService;


        public CartService(IDataBaseContext _context, IMapper mapper, IUserService userService, IAddressService addressService, IProductItemService productItemService, IRebateService rebateService, IProductOrderService productOrderService, IProductOrderItemService productOrderItemService, ICodeService codeService, IPaymentService paymentService, ICurrentUserHelper currentUser, IDeliveryService deliveryService, IWalletService walletService)
        {
            this._context = _context;
            this.mapper = mapper;
            _productItemService = productItemService;
            _rebateService = rebateService;
            _productOrderService = productOrderService;
            _productOrderItemService = productOrderItemService;
            _codeService = codeService;
            _paymentService = paymentService;
            _currentUser = currentUser;
            _deliveryService = deliveryService;
            _walletService = walletService;
            _userService = userService;
            _addressService = addressService;
        }
        public async Task<BaseResultDto> CartUpdateAsync(CartUpdateDto cartUpdate)
            {
            if (cartUpdate.CartUpdateType == CartUpdateEnum.GetCount)
            {
                return await CartGetCountAsync(cartUpdate);
            }
            var cart = await CartGetAsync(cartUpdate);
            var result = new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);

            switch (cartUpdate.CartUpdateType)
            {
                case Common.Enumerable.CartUpdateEnum.None:
                    break;
                case Common.Enumerable.CartUpdateEnum.Add:
                    {
                        return await CartAddAsync(cartUpdate, cart);
                    }
                case Common.Enumerable.CartUpdateEnum.Delete:
                    {

                        result = await CartDeleteAsync(cartUpdate, cart);
                        break;
                    }
                case Common.Enumerable.CartUpdateEnum.SetAddress:
                    {
                        result = await CartSetAddressAsync(cartUpdate, cart);
                        break;
                    }


                case Common.Enumerable.CartUpdateEnum.DeleteAddress:
                    {
                        result = await CartDeleteAddressAsync(cartUpdate, cart);
                        break;

                    }
                case Common.Enumerable.CartUpdateEnum.SetDelivery:
                    {
                        result = CartSetDelivery(cartUpdate, cart);
                        break;

                    }
                case Common.Enumerable.CartUpdateEnum.GetDelivery:
                    {
                        result = CartGetDelivery(cartUpdate, cart);
                        break;

                    }
                case Common.Enumerable.CartUpdateEnum.SetReferralCode:
                    {
                        result = await CartSetReferralCodeAsync(cartUpdate, cart);
                        break;

                    }
                case Common.Enumerable.CartUpdateEnum.RemoveReferralCode:
                    {
                        result = await CartRemoveReferralCodeAsync(cartUpdate, cart);
                        break;

                    }
                case Common.Enumerable.CartUpdateEnum.SetRebate:
                    {
                        result = await CartSetRebateAsync(cartUpdate, cart);
                        break;

                    }
                case Common.Enumerable.CartUpdateEnum.RemoveRebate:
                    {
                        result = await CartRemoveRebateAsync(cartUpdate, cart);
                        break;

                    }
                case Common.Enumerable.CartUpdateEnum.SetMerchant:
                    {
                        return await CartSetMerchantAsync(cartUpdate, cart);
                    }
                case Common.Enumerable.CartUpdateEnum.SetOrder:
                    {
                        await UpdateCartAsync(cartUpdate, cart);
                        return await CartOrderAsync(cartUpdate, cart);
                    }
                case Common.Enumerable.CartUpdateEnum.SetActiveCart:
                    {
                        await CartSetActiveAsync(cartUpdate, cart);
                        break;
                    }
                case Common.Enumerable.CartUpdateEnum.Clear:
                    {
                        await CartClearAsync(cart);
                        break;
                    }
                case Common.Enumerable.CartUpdateEnum.GetCart:
                    {
                        await UpdateCartAsync(cartUpdate, cart);
                        return CartGetDto(cart);
                    }
                default:
                    break;
            }
            await UpdateCartAsync(cartUpdate, cart);
            return result;
        }
        private async Task<Cart> CartGetAsync(CartUpdateDto cartUpdate)
        {
            var currentUser = _currentUser?.CurrentUser;
            cartUpdate.UserId = currentUser?.UserId;
            var cart = await _context.Carts.Include(s => s.CartStores).ThenInclude(s => s.Store).Include(s => s.CartStores).ThenInclude(s => s.CartItems).ThenInclude(s => s.ProductItem).ThenInclude(s => s.VarietyItem).ThenInclude(s => s.Variety).Include(s => s.CartStores).ThenInclude(s => s.CartItems).ThenInclude(s => s.ProductItem).ThenInclude(s => s.VarietyItem2).ThenInclude(s => s.Variety).Include(s => s.CartStores).ThenInclude(s => s.CartItems).ThenInclude(s => s.ProductItem).ThenInclude(s => s.Product).ThenInclude(s => s.Category).Include(s => s.CartStores).ThenInclude(s => s.CartItems).ThenInclude(s => s.ProductItem).ThenInclude(s => s.Store).Include(s => s.Delivery).Include(s => s.Rebate).Include(s => s.User).Include(s => s.Address).AsTracking().FirstOrDefaultAsync(s => (cartUpdate.UserId.HasValue ? s.UserId == cartUpdate.UserId : s.UniqueId == cartUpdate.UniqueId));
            if (cart == null)
            {
                DateTime justNow = DateTime.Now;
                string uniqueId = Guid.NewGuid().ToString();
                cart = new Cart() { CreateDate = justNow, UpdateDate = justNow, UserId = cartUpdate.UserId, UniqueId = uniqueId, CartStores = new List<CartStore>() };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }
        private async Task<BaseResultDto<int>> CartGetCountAsync(CartUpdateDto cartUpdate)
        {
            var currentUser = _currentUser?.CurrentUser;
            cartUpdate.UserId = currentUser?.UserId;
            var cart = await _context.Carts.FirstOrDefaultAsync(s => (cartUpdate.UserId.HasValue ? s.UserId == cartUpdate.UserId : s.UniqueId == cartUpdate.UniqueId));
            if (cart != null)
            {
                return new BaseResultDto<int>(true, cart.ItemCount);
            }
            return new BaseResultDto<int>(true, 0);
        }
        private async Task CartClearAsync(Cart cart)
        {
            await CartRemoveRebateAsync(cart);
            await CartRemoveDeliveryAsync(cart);
            await CartRemoveReferralCodeAsync(cart);
        }
        private BaseResultDto<CartVDto> CartGetDto(Cart cart)
        {
            return new BaseResultDto<CartVDto>(true, mapper.Map<CartVDto>(cart));
        }
        private async Task UpdateCartAsync(CartUpdateDto cartUpdate, Cart cart, bool force = true)
        {
            if (cart.Changed || force == true)
            {
                var cartStore = cart.CartStores.FirstOrDefault(s => s.Active);
                if (cartStore != null)
                {
                    foreach (var removedItem in cartStore.CartItems.Where(s => s.ProductItem.SystemActive == false))
                    {
                        _context.CartItems.Remove(removedItem);
                    }
                    foreach (var item in cartStore.CartItems.Where(c => c.Count > c.ProductItem.Quantity))
                    {
                        item.Count = item.ProductItem.Quantity;
                        _context.CartItems.Update(item);
                    }
                    cartStore.BasePrice = cartStore.CartItems.Sum(s => s.ProductItem.BasePrice * s.Count);
                    cartStore.Price = cartStore.CartItems.Sum(s => s.ProductItem.Price * s.Count);
                    cartStore.ItemCount = cartStore.CartItems.Sum(s => s.Count);
                    cartStore.Discount = cartStore.CartItems.Sum(s => (s.ProductItem.BasePrice - s.ProductItem.Price) * s.Count);
                    cartStore.PaymentPrice = cartStore.Price + cartStore.DeliveryPrice;


                    cart.BasePrice = cartStore.BasePrice;
                    cart.Price = cartStore.Price;
                    cart.Discount = cartStore.Discount;
                    cart.DeliveryPrice = cartStore.DeliveryPrice;
                    cart.PaymentPrice = (cart.Price - cart.RebatePrice) + cart.DeliveryPrice;
                    cart.ItemCount = cart.CartStores.Count();
                    _context.Carts.Update(cart);
                    await _context.SaveChangesAsync();
                }
                foreach (var removedstore in cart.CartStores.Where(s => s.CartItems.Any() == false))
                {
                    _context.CartStores.Remove(removedstore);
                }
                await _context.SaveChangesAsync();

            }

        }
        private async Task<BaseResultDto> CartAddAsync(CartUpdateDto cartUpdate, Cart cart)
        {
            if (cartUpdate.ProductItemId.HasValue)
            {
                if (cartUpdate.Count < 1)
                {
                    cartUpdate.Count = 1;
                }

                var productItemIsSalable = await _productItemService.IsSalable(cartUpdate.ProductItemId.Value, cartUpdate.Count);
                if (!productItemIsSalable.IsSuccess)
                {
                    return productItemIsSalable;
                }

                var productItem = productItemIsSalable.Data;

                if (productItem.Product.SellLimitCount.HasValue && productItem.Product.SellLimitCount > 0)
                {
                    var existItems = cart.CartStores
                        .SelectMany(s => s.CartItems.Where(a => a.ProductItem.ProductId == productItem.ProductId));

                    if (existItems.Sum(s => s.Count) > productItem.Product.SellLimitCount)
                    {
                        _context.CartItems.RemoveRange(existItems);
                        await _context.SaveChangesAsync();
                        return new BaseResultDto(isSuccess: false,
                            val: string.Format(Resource.Pattern.YouCanBuyUpToT1Product, productItem.Product.SellLimitCount));
                    }

                    if (existItems.Where(s => s.ProductItemId != cartUpdate.ProductItemId)
                        .Sum(s => s.Count) + cartUpdate.Count > productItem.Product.SellLimitCount)
                    {
                        return new BaseResultDto(isSuccess: false,
                            val: string.Format(Resource.Pattern.YouCanBuyUpToT1Product, productItem.Product.SellLimitCount));
                    }
                }

                var cartStore = cart.CartStores.FirstOrDefault(s => s.StoreId == productItem.StoreId);
                if (cartStore != null)
                {
                    var cartItem = cartStore.CartItems.FirstOrDefault(s => s.ProductItemId == productItem.Id);
                    if (cartItem != null)
                    {
                        cartItem.Count = cartUpdate.Count;
                        _context.CartItems.Update(cartItem);
                    }
                    else
                    {
                        cartStore.CartItems.Add(new CartItem
                        {
                            Count = cartUpdate.Count,
                            CreateDate = DateTime.Now,
                            ProductItemId = productItem.Id,
                        });
                    }

                    await _context.SaveChangesAsync();
                    return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
                }
                else
                {
                    cartStore = new CartStore
                    {
                        StoreId = productItem.StoreId,
                    };
                    cart.CartStores.Add(cartStore);
                    cartStore.CartItems = new List<CartItem>();

                    cartStore.CartItems.Add(new CartItem
                    {
                        Count = cartUpdate.Count,
                        CreateDate = DateTime.Now,
                        ProductItemId = productItem.Id,
                    });

                    await _context.SaveChangesAsync();
                }
                cartUpdate.StoreId = cartStore.StoreId;
            }

            await CartRemoveRebateAsync(cart);
            await CartRemoveDeliveryAsync(cart);
            await CartSetActiveAsync(cartUpdate, cart);
            return new BaseResultDto<string>(isSuccess: true, val: Resource.Notification.SuccessfullyAddedToCart, cart.UniqueId);
        }


        private async Task<BaseResultDto> CartDeleteAsync(CartUpdateDto cartUpdate, Cart cart)
        {
            if (!cartUpdate.ProductItemId.HasValue)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);

            }
            var cartItem = _context.CartItems.AsTracking().FirstOrDefault(s => s.ProductItemId == cartUpdate.ProductItemId && s.CartStore.CartId == cart.Id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                await CartRemoveRebateAsync(cart);
                await CartRemoveDeliveryAsync(cart);

                return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
            }
            else
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);

        }
        private async Task<BaseResultDto> CartSetAddressAsync(CartUpdateDto cartUpdate, Cart cart)
        {
            if (!cartUpdate.AddressId.HasValue)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            cart.AddressId = cartUpdate.AddressId;
            cart.Address = null;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }
        private async Task<BaseResultDto> CartSetMerchantAsync(CartUpdateDto cartUpdate, Cart cart)
        {
            if (!cartUpdate.MerchantId.HasValue)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            cart.Merchant = null;
            cart.MerchantId = cartUpdate.MerchantId;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }
        private async Task<BaseResultDto> CartSetActiveAsync(CartUpdateDto cartUpdate, Cart cart)
        {
            if (cartUpdate.StoreId == 0)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            foreach (var cStore in cart.CartStores)
            {
                if (cStore.StoreId == cartUpdate.StoreId)
                    cStore.Active = true;
                else
                    cStore.Active = false;
                _context.CartStores.Update(cStore);
            }
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }
        private async Task<BaseResultDto> CartSetRebateAsync(CartUpdateDto cartUpdate, Cart cart)
        {
            if (string.IsNullOrEmpty(cartUpdate.RebateCode))
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            var rebate = _rebateService.GetRebateByCodeAsync(cart, cartUpdate.RebateCode);
            if (rebate.IsSuccess)
            {
                cart.Rebate = null;
                cart.RebateId = rebate.Data.Id;
                cart.RebatePrice = rebate.Data.FinalPrice;
                _context.Carts.Update(cart);
                await _context.SaveChangesAsync();
                return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
            }
            else
            {
                return new BaseResultDto(isSuccess: false, messages: rebate.Messages);
            }

        }
        private async Task<BaseResultDto> CartSetReferralCodeAsync(CartUpdateDto cartUpdate, Cart cart)
        {
            if (string.IsNullOrEmpty(cartUpdate.ReferralCode))
            {
                cart.ReferralCode = null;
                _context.Carts.Update(cart);
                await _context.SaveChangesAsync();

                return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
            }

            var refferalUser = await _userService.GetUserByReferralCodeAsync(cartUpdate.ReferralCode);

            if (refferalUser == null || (refferalUser != null && refferalUser.Id == cart.UserId))
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidBonusCode);
            }
            cart.ReferralCode = refferalUser.ReferralCode;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();

            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        private BaseResultDto CartSetDelivery(CartUpdateDto cartUpdate, Cart cart)
        {
            if (cartUpdate.DeliveryId == null)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            var delivery = _deliveryService.GetDelivery(cart, cartUpdate.DeliveryId.Value, cartUpdate.StoreId);
            if (delivery != null)
            {
                var cartStore = cart.CartStores.FirstOrDefault(s => s.StoreId == cartUpdate.StoreId);
                cartStore.DeliveryId = delivery.Id;
                cartStore.Delivery = null;
                cart.Delivery = null;
                cartStore.DeliveryPrice = delivery.DeliveryPrice;
                cart.DeliveryId = delivery.Id;

                _context.CartStores.Update(cartStore);
                _context.SaveChanges();
                return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
            }
            else
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }

        }

        private async Task<BaseResultDto> CartDeleteAddressAsync(CartUpdateDto cartUpdate, Cart cart)
        {
            if (!cartUpdate.AddressId.HasValue)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            _addressService.DeleteDto(cartUpdate.AddressId.Value);
            if (cartUpdate.AddressId == cart.AddressId)
            {
                cart.AddressId = null;
                cart.DeliveryId = null;
                cart.DeliveryPrice = 0;
                _context.Carts.Update(cart);
                foreach (var s in cart.CartStores)
                {
                    s.Delivery = null;
                    s.DeliveryPrice = 0;
                    _context.CartStores.Update(s);
                }
                await _context.SaveChangesAsync();
            }

            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }
        private async Task<BaseResultDto> CartRemoveRebateAsync(CartUpdateDto cartUpdate, Cart cart)
        {
            await CartRemoveRebateAsync(cart);
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }
        private async Task CartRemoveRebateAsync(Cart cart)
        {
            cart.RebateId = null;
            cart.RebatePrice = 0;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }
        private async Task CartRemoveDeliveryAsync(Cart cart)
        {
            cart.Delivery = null;
            cart.DeliveryId = null;
            cart.DeliveryPrice = 0;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }
        private async Task<BaseResultDto> CartRemoveReferralCodeAsync(CartUpdateDto cartUpdate, Cart cart)
        {
            await CartRemoveReferralCodeAsync(cart);
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }
        private async Task CartRemoveReferralCodeAsync(Cart cart)
        {
            cart.ReferralCode = null;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }
        private async Task<BaseResultDto> CartOrderAsync(CartUpdateDto cartUpdate, Cart cart)
        {
            var cartStore = cart.CartStores.FirstOrDefault(s => s.Active == true);
            if (cartStore != null)
            {
                if (cartStore.ItemCount < 1)
                {
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.CartIsEmpty);
                }
                if (cartStore.Store.TypeId != (long)StoreTypeEnum.StoreType_PackageShop)
                {
                    if (cart.AddressId == null)
                    {
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.PleaseSelectTheAddress);
                    }
                    if (cartStore.DeliveryId == null)
                    {
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.PleaseSelectTheDeliveryType);
                    }
                }
                var productOrderDto = mapper.Map<ProductOrderDto>(cart);


                var insertStatus = await _codeService.GetByLabelAsync(ProductOrderStatusEnum.ProductOrderStatus_Insert.ToString());
                var normalState = await _codeService.GetByLabelAsync(ProductOrderStateEnum.ProductOrderState_Normal.ToString());
                var orderPaymentTypeOnline = await _codeService.GetByLabelAsync(OrderPaymentTypeEnum.OrderPaymentType_Online.ToString());
                var orderPaymentTypeWallet = await _codeService.GetByLabelAsync(OrderPaymentTypeEnum.OrderPaymentType_Wallet.ToString());
                var orderPaymentTypeCombinatorial = await _codeService.GetByLabelAsync(OrderPaymentTypeEnum.OrderPaymentType_Combinatorial.ToString());
                var orderPaymentTypeNot = await _codeService.GetByLabelAsync(OrderPaymentTypeEnum.OrderPaymentType_Not.ToString());
                var paymentTypeProductOrder = await _codeService.GetByLabelAsync(PaymentTypeEnum.PaymentType_ProductOrder.ToString());

                productOrderDto.ProductOrderStatusId = insertStatus.Id;
                productOrderDto.ProductOrderStateId = normalState.Id;
                if (productOrderDto.PaymentPrice < 1)
                {
                    productOrderDto.PaymentTypeId = orderPaymentTypeNot.Id;

                }
                else
                {
                    productOrderDto.PaymentTypeId = orderPaymentTypeOnline.Id;

                    if (cartUpdate.FromWallet)
                    {
                        var walletAmount = await _walletService.GetAmountValueAsync(cart.UserId.Value);
                        if (walletAmount < 1)
                        {
                            return new BaseResultDto(isSuccess: false);
                        }
                        if (walletAmount < productOrderDto.PaymentPrice && cart.MerchantId == null)
                        {
                            return new BaseResultDto(isSuccess: false, val: Resource.Notification.PleaseSelectTheMerchant);

                        }
                        productOrderDto.WalletPrice = Application.Services.Order.PaymentSrv.PaymentAmountHelper.GetWalletContribution(walletAmount, productOrderDto.PaymentPrice);
                        productOrderDto.PaymentTypeId = orderPaymentTypeWallet.Id;
                    }
                    else if (productOrderDto.PaymentPrice > 0 && cart.MerchantId == null)
                    {
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.PleaseSelectTheMerchant);
                    }
                }

                var insertedProductOrder = await _productOrderService.InsertAsyncDto(productOrderDto);


                if (!insertedProductOrder.IsSuccess)
                {
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
                }

                var paymentDto = new PaymentStartDto()
                {
                    Amount = productOrderDto.PaymentPrice,
                    IsOnline = productOrderDto.PaymentTypeId == orderPaymentTypeOnline.Id,
                    MerchantId = cart.MerchantId,
                    ProductOrderId = insertedProductOrder.Data.Id,
                    User = mapper.Map<UserMinVDto>(cart.User),
                    TypeId = paymentTypeProductOrder.Id,
                    UserId = _currentUser.CurrentUser.UserId,
                    CallBackTypeLabel = PaymentCallbackTypeEnum.ProductOrder.ToString(),
                    CallBackId = insertedProductOrder.Data.Id,
                    CallbackUrl = null
                };
                if (cartUpdate.FromWallet)
                {
                    var walletAmount = await _walletService.GetAmountValueAsync(cart.UserId.Value);

                    if (walletAmount >= productOrderDto.PaymentPrice)
                    {
                        var callback = await _productOrderService.ProductPaymentCallback(insertedProductOrder.Data.Id, true);
                        if (!callback.IsSuccess)
                            return callback;
                        return new BaseResultDto<PaymentStartDto>(true, paymentDto);
                    }
                    else
                    {
                        paymentDto.Amount = productOrderDto.PaymentPrice - productOrderDto.WalletPrice;
                        paymentDto.ProductOrderId = null;
                        paymentDto.CallBackTypeLabel = PaymentCallbackTypeEnum.ProductOrder.ToString();
                        paymentDto.CallBackId = insertedProductOrder.Data.Id;
                        return await _paymentService.StartPayment(paymentDto);

                    }


                }

                return await _paymentService.StartPayment(paymentDto);
            }
            else
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.CartIsEmpty);

            }
        }
        private BaseResultDto CartGetDelivery(CartUpdateDto cartUpdate, Cart cart)
        {
            return _deliveryService.GetDeliveries(cart: cart, cartUpdate.StoreId);
        }
    }
}
