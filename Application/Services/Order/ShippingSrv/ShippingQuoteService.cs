using Application.Common.Dto.Result;
using Application.Services.Order.DeliverySrv.iface;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
using Application.Services.Order.ShippingSrv.Provider;
using Entities.Entities;
using Entities.Entities.ShippingField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv
{
    public class ShippingQuoteService : IShippingQuoteService
    {
        private readonly IDataBaseContext _context;
        private readonly IDeliveryService _deliveryService;
        private readonly IReadOnlyDictionary<ShippingProviderEnum, IShippingProvider> _providers;
        private readonly ShippingOptions _options;

        public ShippingQuoteService(
            IDataBaseContext context,
            IDeliveryService deliveryService,
            IEnumerable<IShippingProvider> providers,
            IOptions<ShippingOptions> options)
        {
            _context = context;
            _deliveryService = deliveryService;
            _providers = providers.ToDictionary(item => item.Provider);
            _options = options.Value;
        }

        public async Task<BaseResultDto<List<ShippingQuoteVDto>>> CreateQuotesAsync(
            long userId,
            long storeId,
            CancellationToken cancellationToken = default)
        {
            var cart = await _context.Carts
                .Include(item => item.Address)
                .Include(item => item.CartStores)
                    .ThenInclude(item => item.Store)
                .Include(item => item.CartStores)
                    .ThenInclude(item => item.CartItems)
                        .ThenInclude(item => item.ProductItem)
                            .ThenInclude(item => item.Product)
                .AsTracking()
                .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
            var cartStore = cart?.CartStores.FirstOrDefault(item => item.StoreId == storeId && item.Active);
            if (cartStore == null || cartStore.CartItems == null || !cartStore.CartItems.Any())
                return Failed("سبد فعالی برای این فروشگاه یافت نشد.");
            if (!cart.AddressId.HasValue || cart.Address == null)
                return Failed("ابتدا آدرس دریافت سفارش را انتخاب کنید.");

            var deliveries = await _context.Deliveries
                .Include(item => item.DeliveryType)
                .Where(item => item.StoreId == storeId && item.Active && !item.Deleted)
                .OrderBy(item => item.Id)
                .ToListAsync(cancellationToken);
            if (!deliveries.Any())
                return Failed("روش ارسال فعالی برای این فروشگاه تعریف نشده است.");

            var now = DateTime.UtcNow;
            var results = new List<ShippingQuoteVDto>();
            foreach (var delivery in deliveries)
            {
                if (!delivery.LivePricing || delivery.ShippingProvider == ShippingProviderEnum.None)
                {
                    var staticDelivery = _deliveryService.GetDelivery(cart, delivery, storeId);
                    if (staticDelivery == null)
                        continue;

                    if (delivery.AllowPrepaid && !delivery.AfterRent)
                        results.Add(await SaveQuoteAsync(cart, cartStore, delivery,
                            ShippingPaymentModeEnum.Prepaid, staticDelivery.DeliveryPrice, null, now, cancellationToken));
                    if (delivery.AllowReceiverPay || delivery.AfterRent)
                        results.Add(await SaveQuoteAsync(cart, cartStore, delivery,
                            ShippingPaymentModeEnum.ReceiverPays, staticDelivery.DeliveryPrice, null, now, cancellationToken));
                    continue;
                }

                if (cartStore.Store?.Location == null || cart.Address.Location == null)
                    continue;
                if (!_providers.TryGetValue(delivery.ShippingProvider, out var provider))
                    continue;

                var request = CreateProviderRequest(cart, cartStore, delivery.ShippingProvider);
                var providerResult = await provider.GetQuoteAsync(request, cancellationToken);
                if (!providerResult.IsSuccess)
                    continue;

                if (delivery.AllowPrepaid)
                    results.Add(await SaveQuoteAsync(cart, cartStore, delivery,
                        ShippingPaymentModeEnum.Prepaid, providerResult.Price,
                        providerResult.ExternalQuoteId, now, cancellationToken));
                if (delivery.AllowReceiverPay)
                    results.Add(await SaveQuoteAsync(cart, cartStore, delivery,
                        ShippingPaymentModeEnum.ReceiverPays, providerResult.Price,
                        providerResult.ExternalQuoteId, now, cancellationToken));
            }

            await _context.SaveChangesAsync(cancellationToken);
            return results.Any()
                ? new BaseResultDto<List<ShippingQuoteVDto>>(true, results)
                : Failed("در حال حاضر امکان دریافت قیمت ارسال وجود ندارد.");
        }

        public async Task<BaseResultDto> SelectQuoteAsync(
            long userId,
            Guid quoteToken,
            CancellationToken cancellationToken = default)
        {
            var quote = await _context.ShippingQuotes
                .Include(item => item.CartStore)
                    .ThenInclude(item => item.Cart)
                .AsTracking()
                .FirstOrDefaultAsync(item => item.Token == quoteToken && item.UserId == userId, cancellationToken);
            if (quote == null)
                return new BaseResultDto(false, "قیمت ارسال انتخاب‌شده معتبر نیست.");
            if (quote.Status != ShippingQuoteStatusEnum.Active || quote.ExpiresAtUtc <= DateTime.UtcNow)
                return new BaseResultDto(false, "اعتبار قیمت ارسال تمام شده است؛ قیمت را مجدداً دریافت کنید.");
            if (quote.CartStore?.Cart?.AddressId != quote.AddressId)
                return new BaseResultDto(false, "آدرس سبد پس از دریافت قیمت تغییر کرده است؛ قیمت را مجدداً دریافت کنید.");

            var previousQuotes = await _context.ShippingQuotes
                .Where(item => item.CartStoreId == quote.CartStoreId &&
                    item.Status == ShippingQuoteStatusEnum.Selected)
                .ToListAsync(cancellationToken);
            foreach (var previousQuote in previousQuotes)
                previousQuote.Status = ShippingQuoteStatusEnum.Cancelled;

            quote.Status = ShippingQuoteStatusEnum.Selected;
            quote.SelectedAtUtc = DateTime.UtcNow;
            quote.CartStore.DeliveryId = quote.DeliveryId;
            quote.CartStore.ShippingQuoteId = quote.Id;
            quote.CartStore.ShippingProvider = quote.Provider;
            quote.CartStore.ShippingPaymentMode = quote.PaymentMode;
            quote.CartStore.ShippingQuotedPrice = quote.Price;
            quote.CartStore.DeliveryPrice = quote.PaymentMode == ShippingPaymentModeEnum.Prepaid
                ? quote.Price
                : 0;

            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto(true, "روش ارسال با موفقیت انتخاب شد.");
        }

        public async Task<BaseResultDto> ValidateSelectionAsync(
            CartStore cartStore,
            long userId,
            long? addressId,
            CancellationToken cancellationToken = default)
        {
            if (!cartStore.DeliveryId.HasValue)
                return new BaseResultDto(false, "روش ارسال انتخاب نشده است.");

            var delivery = await _context.Deliveries.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == cartStore.DeliveryId.Value &&
                    item.StoreId == cartStore.StoreId && item.Active && !item.Deleted, cancellationToken);
            if (delivery == null)
                return new BaseResultDto(false, "روش ارسال انتخاب‌شده دیگر فعال نیست.");
            if (!delivery.LivePricing || delivery.ShippingProvider == ShippingProviderEnum.None)
                return new BaseResultDto(true);
            if (!cartStore.ShippingQuoteId.HasValue || !addressId.HasValue)
                return new BaseResultDto(false, "برای این روش ارسال باید قیمت لحظه‌ای دریافت شود.");

            var quote = await _context.ShippingQuotes.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == cartStore.ShippingQuoteId.Value &&
                    item.UserId == userId && item.CartStoreId == cartStore.Id &&
                    item.AddressId == addressId.Value && item.DeliveryId == delivery.Id &&
                    item.Status == ShippingQuoteStatusEnum.Selected, cancellationToken);
            if (quote == null || quote.ExpiresAtUtc <= DateTime.UtcNow)
                return new BaseResultDto(false, "قیمت ارسال منقضی شده است؛ لطفاً قیمت جدید دریافت کنید.");
            if (cartStore.ShippingQuotedPrice != quote.Price ||
                cartStore.ShippingProvider != quote.Provider ||
                cartStore.ShippingPaymentMode != quote.PaymentMode)
                return new BaseResultDto(false, "اطلاعات قیمت ارسال معتبر نیست.");
            if (!string.Equals(
                    quote.RequestFingerprint,
                    CreateFingerprint(cartStore, addressId.Value, delivery.Id),
                    StringComparison.Ordinal))
                return new BaseResultDto(false, "محتویات سبد پس از دریافت قیمت ارسال تغییر کرده است؛ قیمت جدید دریافت کنید.");

            var expectedPayablePrice = quote.PaymentMode == ShippingPaymentModeEnum.Prepaid ? quote.Price : 0;
            return cartStore.DeliveryPrice == expectedPayablePrice
                ? new BaseResultDto(true)
                : new BaseResultDto(false, "مبلغ ارسال سبد معتبر نیست.");
        }

        private ShippingProviderQuoteRequest CreateProviderRequest(
            Cart cart,
            CartStore cartStore,
            ShippingProviderEnum provider)
        {
            var products = cartStore.CartItems.Select(item => new
            {
                Count = Math.Max(1, item.Count),
                Product = item.ProductItem.Product
            }).ToList();
            var weight = products.Sum(item =>
                (item.Product.ShippingWeightGrams ?? _options.DefaultWeightGrams) * item.Count);

            return new ShippingProviderQuoteRequest
            {
                StoreId = cartStore.StoreId,
                Provider = provider,
                OriginLatitude = cartStore.Store.Location.Y,
                OriginLongitude = cartStore.Store.Location.X,
                DestinationLatitude = cart.Address.Location.Y,
                DestinationLongitude = cart.Address.Location.X,
                WeightGrams = Math.Max(weight, _options.DefaultWeightGrams),
                LengthCm = products.Max(item => item.Product.ShippingLengthCm ?? _options.DefaultLengthCm),
                WidthCm = products.Max(item => item.Product.ShippingWidthCm ?? _options.DefaultWidthCm),
                HeightCm = products.Sum(item =>
                    (item.Product.ShippingHeightCm ?? _options.DefaultHeightCm) * item.Count),
                DeclaredValue = cartStore.Price
            };
        }

        private async Task<ShippingQuoteVDto> SaveQuoteAsync(
            Cart cart,
            CartStore cartStore,
            Delivery delivery,
            ShippingPaymentModeEnum paymentMode,
            double price,
            string externalQuoteId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var quote = new ShippingQuote
            {
                Token = Guid.NewGuid(),
                UserId = cart.UserId.Value,
                CartStoreId = cartStore.Id,
                AddressId = cart.AddressId.Value,
                DeliveryId = delivery.Id,
                Provider = delivery.ShippingProvider,
                PaymentMode = paymentMode,
                Status = ShippingQuoteStatusEnum.Active,
                Price = Math.Max(0, price),
                Currency = "IRR",
                ExternalQuoteId = externalQuoteId,
                RequestFingerprint = CreateFingerprint(cartStore, cart.AddressId.Value, delivery.Id),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(Math.Clamp(_options.QuoteTtlMinutes, 1, 30))
            };
            await _context.ShippingQuotes.AddAsync(quote, cancellationToken);

            return new ShippingQuoteVDto
            {
                QuoteToken = quote.Token,
                DeliveryId = delivery.Id,
                DeliveryName = delivery.DeliveryType?.Name,
                Provider = delivery.ShippingProvider,
                PaymentMode = paymentMode,
                QuotedPrice = quote.Price,
                PayableDeliveryPrice = paymentMode == ShippingPaymentModeEnum.Prepaid ? quote.Price : 0,
                PayAtDestination = paymentMode == ShippingPaymentModeEnum.ReceiverPays,
                Currency = quote.Currency,
                ExpiresAtUtc = quote.ExpiresAtUtc
            };
        }

        private static string CreateFingerprint(CartStore cartStore, long addressId, long deliveryId)
        {
            var value = $"{cartStore.Id}:{addressId}:{deliveryId}:{cartStore.ItemCount}:{cartStore.Price}";
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
        }

        private static BaseResultDto<List<ShippingQuoteVDto>> Failed(string message) =>
            new(false, message, new List<ShippingQuoteVDto>());
    }
}
