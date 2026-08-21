using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Service;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Iface;
using Application.Services.Content.CargoSrv.Iface;
using Application.Services.Order.MerchantSrv.Iface;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Application.Services.PastilAISrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.TripSrv.TripSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.PastilAIField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentSrv
{
    public class PaymentService : CommonSrv<Payment, PaymentDto>, IPaymentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IMerchantService _merchantService;
        private readonly IProductOrderService _productOrderService;
        private readonly ICompanionReserveService _companionReserveService;
        private readonly IWalletService _walletService;
        private readonly ICodeService _codeService;
        private readonly ITripService _tripService;
        private readonly ICargoService _cargoService;
        private readonly ICompanionInsurancePackageSaleService _companionInsurance;
        private readonly IPansionReserveService _pansionReserve;
        private readonly IPastilAiSubscriptionActivator _pastilAiSubscriptionActivator;
        private readonly IRebateService _rebateService;
        private readonly IConfiguration _configuration;
        private readonly IPaymentTestModeService _paymentTestModeService;
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IDataBaseContext context,
            IMapper mapper,
            ICodeService codeService,
            IWalletService walletService,
            IMerchantService merchantService,
            IPansionReserveService pansionReserve,
            IProductOrderService productOrderService,
            ICompanionReserveService companionReserveService,
            ITripService tripService,
            ICargoService cargoService,
            ICompanionInsurancePackageSaleService companionInsurance,
            IPastilAiSubscriptionActivator pastilAiSubscriptionActivator,
            IRebateService rebateService,
            IPaymentTestModeService paymentTestModeService,
            ICurrentUserHelper currentUserHelper,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<PaymentService> logger)
            : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _merchantService = merchantService;
            _productOrderService = productOrderService;
            _walletService = walletService;
            _codeService = codeService;
            _companionReserveService = companionReserveService;
            _tripService = tripService;
            _cargoService = cargoService;
            _companionInsurance = companionInsurance;
            _pansionReserve = pansionReserve;
            _pastilAiSubscriptionActivator = pastilAiSubscriptionActivator;
            _rebateService = rebateService;
            _paymentTestModeService = paymentTestModeService;
            _currentUserHelper = currentUserHelper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<BaseResultDto> InsertWalletPaymentAsyncDto(PaymentStartDto dto)
        {
            if (dto.Amount < 10000)
            {
                dto.Amount = 10000;
            }

            if (dto.MerchantId == null)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);
            }

            var paymentTypeWallet = await _codeService.GetByLabelAsync(
                PaymentTypeEnum.PaymentType_Wallet.ToString());

            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = paymentTypeWallet.Id;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Wallet.ToString();
            dto.CallBackId = null;
            dto.GrossAmount = dto.Amount;
            dto.RebateAmount = 0;
            dto.WalletAmount = 0;

            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> StartPayment(PaymentStartDto dto)
        {
            Payment item = null;

            try
            {
                if (!TryGetIdempotencyKey(out var idempotencyKey, out var idempotencyError))
                    return new BaseResultDto(false, idempotencyError);

                dto.GrossAmount = dto.GrossAmount > 0
                    ? dto.GrossAmount
                    : dto.Amount + dto.RebateAmount + dto.WalletAmount;
                var isInternalSettlement = dto.Amount <= 0 &&
                    (dto.WalletAmount > 0 || dto.RebateAmount > 0) &&
                    Math.Abs(dto.GrossAmount - dto.RebateAmount - dto.WalletAmount) <= 0.01;

                if (!isInternalSettlement && dto.Amount < 10000 && !_paymentTestModeService.IsEnabled)
                {
                    return new BaseResultDto(
                        false,
                        string.Format(Resource.Pattern.AmountsLessT1CannotPaid, 10000));
                }

                if (dto.UserId == null || dto.UserId <= 0 ||
                    !await _context.Users.AsNoTracking().AnyAsync(s => s.Id == dto.UserId.Value))
                {
                    return new BaseResultDto(false, Resource.Notification.UserNotFound);
                }
                if (!isInternalSettlement && (!dto.MerchantId.HasValue || dto.MerchantId.Value <= 0))
                    return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

                await using var createTransaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                if (idempotencyKey != null)
                {
                    var existingPayment = await _context.Payments.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.IdempotencyKey == idempotencyKey);
                    if (existingPayment != null)
                    {
                        await createTransaction.RollbackAsync();
                        if (!IsSameCheckout(existingPayment, dto))
                            return new BaseResultDto(false, "این Idempotency-Key قبلاً برای Checkout دیگری استفاده شده است.");

                        return CreateIdempotentReplayResult(existingPayment);
                    }
                }

                if (dto.RebateId.HasValue)
                {
                    var now = DateTime.Now;
                    var holdFrom = now.AddMinutes(-30);
                    var rebate = await _context.Rebate.AsTracking().FirstOrDefaultAsync(s =>
                        s.Id == dto.RebateId.Value && s.Active && !s.Deleted &&
                        s.StartDatetime <= now && s.EndDatetime >= now);
                    if (rebate == null)
                    {
                        await createTransaction.RollbackAsync();
                        return new BaseResultDto(false, Resource.Notification.ThisDiscountCodeExpired);
                    }

                    var globalHolds = await _context.Payments.AsNoTracking().CountAsync(s =>
                        s.RebateId == rebate.Id && s.AppliedDate == null &&
                        (s.IsSuccess == true || s.IsSuccess == null && s.CreateDate >= holdFrom));
                    var userHolds = await _context.Payments.AsNoTracking().CountAsync(s =>
                        s.RebateId == rebate.Id && s.UserId == dto.UserId.Value && s.AppliedDate == null &&
                        (s.IsSuccess == true || s.IsSuccess == null && s.CreateDate >= holdFrom));
                    var userUsage = await _context.UserRebates.AsNoTracking()
                        .Where(s => s.RebateId == rebate.Id && s.UserId == dto.UserId.Value)
                        .Select(s => s.UsageCount)
                        .FirstOrDefaultAsync();
                    if (rebate.UsedCount + globalHolds >= rebate.UseCount ||
                        userUsage + userHolds >= rebate.MaxUsePerUser)
                    {
                        await createTransaction.RollbackAsync();
                        return new BaseResultDto(false, Resource.Notification.TheLimitUsesDiscountCodeReached);
                    }
                }
                if (!string.IsNullOrWhiteSpace(dto.CallBackTypeLabel) &&
                    !string.IsNullOrWhiteSpace(dto.CallBackId) &&
                    await _context.Payments.AsNoTracking().AnyAsync(s =>
                        s.UserId == dto.UserId.Value &&
                        s.CallBackTypeLabel == dto.CallBackTypeLabel &&
                        s.CallBackId == dto.CallBackId &&
                        (s.IsSuccess == null || s.IsSuccess == true)))
                {
                    await createTransaction.RollbackAsync();
                    return new BaseResultDto(false, "برای این مورد یک پرداخت فعال یا موفق وجود دارد.");
                }

                item = mapper.Map<Payment>(dto);
                item.CreateDate = DateTime.Now;
                item.IsOnline = true;
                item.IdempotencyKey = idempotencyKey;
                item.PaymentCode = await CreateUniquePaymentCodeAsync();
                item.CallbackToken = isInternalSettlement
                    ? null
                    : Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
                if (isInternalSettlement)
                {
                    item.IsSuccess = true;
                    item.GatewayStatus = "WALLET_APPROVED";
                }

                await _context.Payments.AddAsync(item);
                await _context.SaveChangesAsync();
                await createTransaction.CommitAsync();

                dto.PaymentId = item.Id;
                dto.PaymentCode = item.PaymentCode;
                if (isInternalSettlement)
                {
                    var applyResult = await ApplyAndMapPaymentAsync(item);
                    return new BaseResultDto<PaymentStartDto>(
                        applyResult.IsSuccess,
                        applyResult.Messages,
                        dto);
                }

                var paymentBaseUrl = _configuration["Urls:PaymentBaseUrl"]?.TrimEnd('/');

                if (string.IsNullOrWhiteSpace(paymentBaseUrl))
                {
                    item.IsSuccess = false;
                    item.Description = "Payment callback base URL is not configured.";
                    await _context.SaveChangesAsync();
                    return new BaseResultDto(false, item.Description);
                }

                dto.CallbackUrl = $"{paymentBaseUrl}/callback/{item.Id}?callbackToken={item.CallbackToken}";
                var startResult = await _merchantService.StartAsync(dto);

                item.PaymentUrl = dto.PaymentUrl;
                item.PaymentIsLink = dto.PaymentIsLink;
                _context.Payments.Update(item);
                await _context.SaveChangesAsync();

                if (!startResult.IsSuccess)
                {
                    item.IsSuccess = false;
                    item.Description = $"{Resource.Notification.ErrorOnStartPayment}:" +
                        string.Join(" | ", startResult.Messages.Select(message => message.Item1));
                    _context.Payments.Update(item);
                    await _context.SaveChangesAsync(true);
                }

                if (startResult.IsSuccess && _paymentTestModeService.IsEnabled)
                {
                    var testCallbackResult = await _merchantService.CallbackAsync(item);
                    if (!testCallbackResult.IsSuccess)
                    {
                        await HandleFailedPaymentAsync(item);
                        return new BaseResultDto<PaymentStartDto>(
                            false,
                            testCallbackResult.Messages,
                            dto);
                    }

                    var applyResult = await ApplyAndMapPaymentAsync(item);
                    return new BaseResultDto<PaymentStartDto>(
                        applyResult.IsSuccess,
                        applyResult.Messages,
                        dto);
                }

                return startResult;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Payment start failed for payment {PaymentId} and callback target {CallbackType}/{CallbackId}.",
                    item?.Id,
                    dto.CallBackTypeLabel,
                    dto.CallBackId);

                if (item != null && item.IsSuccess == null)
                {
                    item.IsSuccess = false;
                    item.Description = "START_PAYMENT_FAILED";
                    await _context.SaveChangesAsync();
                }

                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            }
        }

        public async Task<BaseResultDto<ManualPaymentVDto>> InsertManualPaymentAsync(ManualPaymentDto dto)
        {
            try
            {
                return await InsertManualPaymentInternalAsync(dto);
            }
            catch (Exception)
            {
                return new BaseResultDto<ManualPaymentVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        private async Task<BaseResultDto<ManualPaymentVDto>> InsertManualPaymentInternalAsync(ManualPaymentDto dto)
        {
            if (dto == null || !dto.TargetType.HasValue)
            {
                return new BaseResultDto<ManualPaymentVDto>(false, Resource.Notification.InvalidData, null);
            }
            if (string.IsNullOrWhiteSpace(dto.RefNumber) || string.IsNullOrWhiteSpace(dto.Description))
                return new BaseResultDto<ManualPaymentVDto>(false, Resource.Notification.InvalidData, null);

            if (dto.FileId.HasValue && !await _context.Files.AnyAsync(s => s.Id == dto.FileId.Value))
            {
                return new BaseResultDto<ManualPaymentVDto>(false, Resource.Notification.FileCanNotBeFound, null);
            }

            var refNumber = dto.RefNumber?.Trim();
            if (!string.IsNullOrWhiteSpace(refNumber) &&
                await _context.Payments.AnyAsync(s => !s.IsOnline && s.IsSuccess == true && s.RefNumber == refNumber))
            {
                return new BaseResultDto<ManualPaymentVDto>(false, "شماره پیگیری قبلاً برای یک پرداخت موفق ثبت شده است.", null);
            }

            var targetResult = await ResolveManualPaymentTargetAsync(dto);
            if (!targetResult.IsSuccess)
            {
                return new BaseResultDto<ManualPaymentVDto>(false, targetResult.Error, null);
            }

            if (targetResult.Amount <= 0)
            {
                return new BaseResultDto<ManualPaymentVDto>(false, Resource.Notification.AmountNotCorrect, null);
            }

            var typeLabel = GetPaymentTypeLabel(dto.TargetType.Value);
            var typeId = await _context.Codes.AsNoTracking()
                .Where(s => s.Label == typeLabel && s.Active)
                .Select(s => (long?)s.Id)
                .FirstOrDefaultAsync();

            if (!typeId.HasValue)
            {
                return new BaseResultDto<ManualPaymentVDto>(false, "نوع پرداخت در تنظیمات سیستم ثبت نشده است.", null);
            }

            PastilAiSubscription subscription = null;
            if (dto.TargetType == PaymentCallbackTypeEnum.PastilAI)
            {
                subscription = new PastilAiSubscription
                {
                    UserId = targetResult.UserId,
                    PlanId = targetResult.PlanId.Value,
                    Status = PastilAiSubscriptionStatus.PendingPayment,
                    PriceSnapshot = (decimal)targetResult.Amount,
                    CreateDateUtc = DateTime.UtcNow
                };
                await _context.PastilAiSubscriptions.AddAsync(subscription);
                await _context.SaveChangesAsync();
                targetResult.ReferenceId = subscription.Id.ToString();
            }

            var paymentCreateDate = DateTime.Now;
            var payment = new Entities.Entities.Payment
            {
                PaymentCode = await CreateUniquePaymentCodeAsync(),
                MerchantId = null,
                ProductOrderId = dto.TargetType == PaymentCallbackTypeEnum.ProductOrder ? targetResult.ReferenceId : null,
                CompanionReserveId = dto.TargetType == PaymentCallbackTypeEnum.CompanionReserve ? ParseNullableLong(targetResult.ReferenceId) : null,
                TripId = dto.TargetType == PaymentCallbackTypeEnum.Trip ? ParseNullableLong(targetResult.ReferenceId) : null,
                CargoId = dto.TargetType == PaymentCallbackTypeEnum.Cargo ? ParseNullableLong(targetResult.ReferenceId) : null,
                CompanionInsurancePackageSaleId = dto.TargetType == PaymentCallbackTypeEnum.Insurance ? ParseNullableLong(targetResult.ReferenceId) : null,
                RefNumber = refNumber,
                Amount = targetResult.Amount,
                GrossAmount = targetResult.Amount,
                RebateAmount = 0,
                WalletAmount = 0,
                CreateDate = paymentCreateDate,
                Description = dto.Description?.Trim(),
                IsSuccess = true,
                IsOnline = false,
                FileId = dto.FileId,
                UserId = targetResult.UserId,
                ApprovedByUserId = _currentUserHelper.CurrentUser.UserId,
                ApprovedIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                TypeId = typeId.Value,
                CallBackTypeLabel = dto.TargetType.Value.ToString(),
                CallBackId = targetResult.ReferenceId,
                GatewayStatus = "ManualApproved"
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            if (subscription != null)
            {
                subscription.PaymentId = payment.Id;
                await _context.SaveChangesAsync();
            }

            var callbackResult = await ApplyAndMapPaymentAsync(payment);
            if (!callbackResult.IsSuccess)
            {
                if (dto.TargetType == PaymentCallbackTypeEnum.PastilAI &&
                    long.TryParse(payment.CallBackId, out var subscriptionId))
                {
                    await _pastilAiSubscriptionActivator.MarkPaymentFailedAsync(subscriptionId, payment.Id);
                }

                payment.IsSuccess = false;
                payment.GatewayStatus = "ManualCallbackFailed";
                payment.Description = AppendDescription(payment.Description, "اعمال نتیجه پرداخت روی آیتم ناموفق بود.");
                await _context.SaveChangesAsync();
                return new BaseResultDto<ManualPaymentVDto>(false, Resource.Notification.Unsuccess, MapManualPayment(payment, dto.TargetType.Value));
            }

            await MarkManualPaymentDateAsync(dto.TargetType.Value, payment.CallBackId);
            var result = MapManualPayment(payment, dto.TargetType.Value);
            result.TargetReferenceId = dto.ReferenceId;
            return new BaseResultDto<ManualPaymentVDto>(true, result);
        }

        public async Task<BaseResultDto<PaymentDto>> CallbackPayment(long paymentId, string callbackToken)
        {
            try
            {
                var payment = await FindAsync(paymentId);
                if (payment == null || !IsValidCallbackToken(payment.CallbackToken, callbackToken))
                {
                    return new BaseResultDto<PaymentDto>(
                        false,
                        Resource.Notification.Unsuccess,
                        null);
                }

                if (payment.AppliedDate.HasValue)
                {
                    return new BaseResultDto<PaymentDto>(true, await MapPaymentForDisplayAsync(payment));
                }

                if (payment.IsSuccess == null && payment.CreateDate < DateTime.Now.AddMinutes(-30))
                {
                    payment.IsSuccess = false;
                    payment.GatewayStatus = "EXPIRED";
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<PaymentDto>(false, Resource.Notification.Unsuccess, await MapPaymentForDisplayAsync(payment));
                }

                if (payment.IsSuccess == false)
                {
                    return new BaseResultDto<PaymentDto>(
                        false,
                        Resource.Notification.Unsuccess,
                        await MapPaymentForDisplayAsync(payment));
                }

                if (payment.IsSuccess != true)
                {
                    var callbackResult = await _merchantService.CallbackAsync(payment);
                    if (!callbackResult.IsSuccess)
                    {
                        await HandleFailedPaymentAsync(payment);
                        return new BaseResultDto<PaymentDto>(
                            false,
                            callbackResult.Messages,
                            await MapPaymentForDisplayAsync(payment));
                    }
                }

                return await ApplyAndMapPaymentAsync(payment);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Payment callback failed while applying payment {PaymentId} to its target.",
                    paymentId);
                return new BaseResultDto<PaymentDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        private async Task<BaseResultDto<PaymentDto>> ApplyAndMapPaymentAsync(
            Entities.Entities.Payment payment)
        {
            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
            var lockedPayment = await FindAsync(payment.Id);
            if (lockedPayment == null || lockedPayment.IsSuccess != true)
            {
                await transaction.RollbackAsync();
                return new BaseResultDto<PaymentDto>(false, Resource.Notification.Unsuccess, null);
            }

            if (lockedPayment.AppliedDate.HasValue)
            {
                await transaction.CommitAsync();
                return new BaseResultDto<PaymentDto>(true, await MapPaymentForDisplayAsync(lockedPayment));
            }

            var snapshotValidation = await ValidatePaymentSnapshotAsync(lockedPayment);
            if (!snapshotValidation.IsSuccess)
            {
                await transaction.RollbackAsync();
                return new BaseResultDto<PaymentDto>(false, snapshotValidation.Messages, await MapPaymentForDisplayAsync(lockedPayment));
            }

            var applyResult = await ApplySuccessfulPaymentAsync(lockedPayment);
            if (!applyResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                if (lockedPayment.GatewayStatus == "WALLET_APPROVED")
                {
                    await _context.Payments.Where(s => s.Id == lockedPayment.Id && s.AppliedDate == null)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(s => s.IsSuccess, false)
                            .SetProperty(s => s.GatewayStatus, "WALLET_TARGET_FAILED"));
                }
                return new BaseResultDto<PaymentDto>(false, applyResult.Messages, await MapPaymentForDisplayAsync(lockedPayment));
            }

            lockedPayment.AppliedDate = DateTime.UtcNow;
            lockedPayment.GatewayStatus = lockedPayment.GatewayStatus?.StartsWith("TEST_") == true
                ? "TEST_APPLIED"
                : "APPLIED";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new BaseResultDto<PaymentDto>(
                true,
                applyResult.Messages,
                await MapPaymentForDisplayAsync(lockedPayment));
        }

        private async Task<PaymentDto> MapPaymentForDisplayAsync(Entities.Entities.Payment payment)
        {
            var dto = mapper.Map<PaymentDto>(payment);
            dto.ReferenceCode = payment.CallBackTypeLabel switch
            {
                nameof(PaymentCallbackTypeEnum.ProductOrder) =>
                    payment.ProductOrder?.OrderCode ?? await _context.ProductOrders.AsNoTracking()
                        .Where(item => item.Id == (payment.CallBackId ?? payment.ProductOrderId))
                        .Select(item => item.OrderCode)
                        .FirstOrDefaultAsync(),
                nameof(PaymentCallbackTypeEnum.CompanionReserve) when long.TryParse(payment.CallBackId, out var companionReserveId) =>
                    await _context.CompanionReserves.AsNoTracking()
                        .Where(item => item.Id == companionReserveId)
                        .Select(item => item.ReserveCode)
                        .FirstOrDefaultAsync(),
                nameof(PaymentCallbackTypeEnum.PansionReserve) when long.TryParse(payment.CallBackId, out var pansionReserveId) =>
                    await _context.PansionReserves.AsNoTracking()
                        .Where(item => item.Id == pansionReserveId)
                        .Select(item => item.ReserveCode)
                        .FirstOrDefaultAsync(),
                _ => null
            };
            return dto;
        }

        private static bool IsValidCallbackToken(string expectedToken, string callbackToken)
        {
            if (string.IsNullOrWhiteSpace(expectedToken) || string.IsNullOrWhiteSpace(callbackToken))
                return false;

            var expectedBytes = Encoding.UTF8.GetBytes(expectedToken);
            var actualBytes = Encoding.UTF8.GetBytes(callbackToken.Trim());
            return expectedBytes.Length == actualBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
        }

        private async Task HandleFailedPaymentAsync(Entities.Entities.Payment payment)
        {
            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.PastilAI.ToString() &&
                long.TryParse(payment.CallBackId, out var subscriptionId))
            {
                await _pastilAiSubscriptionActivator.MarkPaymentFailedAsync(subscriptionId, payment.Id);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.ProductOrder.ToString())
            {
                await _productOrderService.UpdateWalletAsync(
                    payment.CallBackId ?? payment.ProductOrderId,
                    false);
            }
        }

        public BaseSearchDto<PaymentVDto> Search(PaymentInputDto baseSearchDto)
        {
            var query = _context.Payments.Include(s => s.Merchant).ThenInclude(s => s.Bank).Include(s => s.File).AsQueryable();
            if (!string.IsNullOrEmpty(baseSearchDto.ProductOrderId))
            {
                query = query.Where(s => s.ProductOrderId == baseSearchDto.ProductOrderId);
            }
            if (!string.IsNullOrWhiteSpace(baseSearchDto.PaymentCode))
            {
                var paymentCode = baseSearchDto.PaymentCode.Trim();
                query = query.Where(s => s.PaymentCode == paymentCode);
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
                        query = query.OrderByDescending(s => s.Amount);
                        break;
                    }
                case Common.Enumerable.SortEnum.Inexpensive:
                    {
                        query = query.OrderBy(s => s.Amount);
                        break;
                    }
                default:
                    break;
            }

            return new BaseSearchDto<Payment, PaymentVDto>(baseSearchDto, query, mapper);
        }
        public async Task<BaseResultDto> InsertReservePaymentAsyncDto(PaymentStartDto dto)
        {
            var reservedetail = await _context.CompanionReserves.Include(s => s.Rebate).FirstOrDefaultAsync(s => s.Id == dto.CompanionReserveId);
            if (reservedetail == null || reservedetail.BookerId != dto.UserId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (reservedetail.IsReserved || reservedetail.IsCancel)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            var reserveRebateValidation = ValidateAppliedRebate(
                reservedetail.Rebate,
                reservedetail.PrePaymentPrice + reservedetail.RebatePrice,
                reservedetail.BookerId,
                RebateTypeLabels.CompanionReserve,
                reservedetail.RebatePrice);
            if (!reserveRebateValidation.IsSuccess)
                return reserveRebateValidation;

            dto.Amount = reservedetail.PrePaymentPrice;
            dto.GrossAmount = reservedetail.PrePaymentPrice + reservedetail.RebatePrice;
            dto.RebateAmount = reservedetail.RebatePrice;
            dto.RebateId = reservedetail.RebateId;
            dto.WalletAmount = 0;

            if (dto.Amount < 0)
            {
                return new BaseResultDto(false, Resource.Notification.AmountNotCorrect);
            }
            else if (dto.Amount > 0 && dto.MerchantId == null && !reservedetail.FromWallet)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

            }
            if (reservedetail.FromWallet)
            {
                var assistanceId = await _context.CompanionAssistances.AsNoTracking()
                    .Where(item => item.Id == reservedetail.CompanionAssistanceId)
                    .Select(item => (long?)item.AssistanceId)
                    .FirstOrDefaultAsync();
                var walletAmount = await _walletService.GetSpendableAmountValueAsync(
                    reservedetail.BookerId,
                    Entities.Entities.PastilClubField.ClubRewardTargetTypeEnum.Assistance,
                    assistanceId);
                reservedetail.WalletPrice = PaymentAmountHelper.GetWalletContribution(walletAmount, reservedetail.PrePaymentPrice);
                dto.WalletAmount = reservedetail.WalletPrice;
                await _context.SaveChangesAsync();

                if (walletAmount >= reservedetail.PrePaymentPrice)
                {
                    dto.Amount = 0;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.CompanionReserve.ToString();
                    dto.CallBackId = reservedetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_CompanionReserve.ToString());
                    return await StartPayment(dto);
                }
                else
                {
                    dto.Amount = reservedetail.PrePaymentPrice - reservedetail.WalletPrice;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.CompanionReserve.ToString();
                    dto.CallBackId = reservedetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_CompanionReserve.ToString());
                    return await StartPayment(dto);

                }
            }
            var PaymentType_AgencyReserve = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_CompanionReserve.ToString());
            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = PaymentType_AgencyReserve;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.CompanionReserve.ToString();
            dto.CallBackId = reservedetail.Id.ToString();
            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> InsertPansionReservePaymentAsyncDto(PaymentStartDto dto)
        {
            var reservedetail = await _context.PansionReserves.Include(s => s.Rebate).FirstOrDefaultAsync(s => s.Id == dto.PansionReserveId);
            if (reservedetail == null || reservedetail.BookerId != dto.UserId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (reservedetail.IsReserved || reservedetail.IsCancel)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            var pansionRebateValidation = ValidateAppliedRebate(
                reservedetail.Rebate,
                reservedetail.PaymentPrice + reservedetail.RebatePrice,
                reservedetail.BookerId,
                RebateTypeLabels.PansionReserve,
                reservedetail.RebatePrice);
            if (!pansionRebateValidation.IsSuccess)
                return pansionRebateValidation;

            dto.Amount = reservedetail.PaymentPrice;
            dto.GrossAmount = reservedetail.PaymentPrice + reservedetail.RebatePrice;
            dto.RebateAmount = reservedetail.RebatePrice;
            dto.RebateId = reservedetail.RebateId;
            dto.WalletAmount = 0;

            if (dto.Amount < 0)
            {
                return new BaseResultDto(false, Resource.Notification.AmountNotCorrect);
            }
            else if (dto.Amount > 0 && dto.MerchantId == null && !reservedetail.FromWallet)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

            }
            if (reservedetail.FromWallet)
            {
                var walletAmount = await _walletService.GetSpendableAmountValueAsync(
                    reservedetail.BookerId,
                    Entities.Entities.PastilClubField.ClubRewardTargetTypeEnum.Pansion,
                    reservedetail.PansionId);
                reservedetail.WalletPrice = PaymentAmountHelper.GetWalletContribution(walletAmount, reservedetail.PaymentPrice);
                dto.WalletAmount = reservedetail.WalletPrice;
                await _context.SaveChangesAsync();

                if (walletAmount >= reservedetail.PaymentPrice)
                {
                    dto.Amount = 0;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.PansionReserve.ToString();
                    dto.CallBackId = reservedetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_PansionReserve.ToString());
                    return await StartPayment(dto);
                }
                else
                {
                    dto.Amount = reservedetail.PaymentPrice - reservedetail.WalletPrice;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.PansionReserve.ToString();
                    dto.CallBackId = reservedetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_PansionReserve.ToString());
                    return await StartPayment(dto);

                }
            }
            var PaymentType_AgencyReserve = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_PansionReserve.ToString());
            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = PaymentType_AgencyReserve;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.PansionReserve.ToString();
            dto.CallBackId = reservedetail.Id.ToString();
            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> InsertTripPaymentAsyncDto(PaymentStartDto dto)
        {
            var tripdetail = await _context.Trips.Include(s => s.UserPet).Include(s => s.Rebate).FirstOrDefaultAsync(s => s.Id == dto.TripId);
            if (tripdetail == null || tripdetail.UserPet?.UserId != dto.UserId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (tripdetail.IsPaid)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            var tripRebateValidation = ValidateAppliedRebate(
                tripdetail.Rebate, tripdetail.Price, tripdetail.UserPet.UserId, RebateTypeLabels.Trip, tripdetail.RebatePrice);
            if (!tripRebateValidation.IsSuccess)
                return tripRebateValidation;
            dto.Amount = tripdetail.PaymentPrice;
            dto.GrossAmount = tripdetail.PaymentPrice + tripdetail.RebatePrice;
            dto.RebateAmount = tripdetail.RebatePrice;
            dto.RebateId = tripdetail.RebateId;
            dto.WalletAmount = 0;

            if (tripdetail.TripStatusId != (long)TripStatusEnum.TripStatus_Accepted)
            {
                return new BaseResultDto(false, Resource.Notification.YourCargoRequestIsNotAccepted);
            }

            if (dto.Amount < 0)
            {
                return new BaseResultDto(false, Resource.Notification.AmountNotCorrect);
            }
            else if (dto.Amount > 0 && dto.MerchantId == null && !tripdetail.FromWallet)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

            }
            if (tripdetail.FromWallet)
            {
                var walletAmount = await _walletService.GetAmountValueAsync(tripdetail.UserPet.UserId);
                tripdetail.WalletPrice = PaymentAmountHelper.GetWalletContribution(walletAmount, tripdetail.PaymentPrice);
                dto.WalletAmount = tripdetail.WalletPrice;
                await _context.SaveChangesAsync();

                if (walletAmount >= tripdetail.PaymentPrice)
                {
                    dto.Amount = 0;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Trip.ToString();
                    dto.CallBackId = tripdetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Trip.ToString());
                    return await StartPayment(dto);
                }
                else
                {
                    dto.Amount = tripdetail.PaymentPrice - tripdetail.WalletPrice;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Trip.ToString();
                    dto.CallBackId = tripdetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Trip.ToString());
                    return await StartPayment(dto);

                }
            }
            var PaymentType_trip = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Trip.ToString());
            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = PaymentType_trip;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Trip.ToString();
            dto.CallBackId = tripdetail.Id.ToString();
            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> InsertCargoPaymentAsyncDto(PaymentStartDto dto)
        {
            var cargodetail = await _context.Cargoes.Include(s => s.UserPet).Include(s => s.Rebate).FirstOrDefaultAsync(s => s.Id == dto.CargoId);
            if (cargodetail == null || cargodetail.UserPet?.UserId != dto.UserId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (cargodetail.IsPaid)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            var cargoRebateValidation = ValidateAppliedRebate(
                cargodetail.Rebate, cargodetail.Price, cargodetail.UserPet.UserId, RebateTypeLabels.Cargo, cargodetail.RebatePrice);
            if (!cargoRebateValidation.IsSuccess)
                return cargoRebateValidation;
            dto.Amount = cargodetail.PaymentPrice;
            dto.GrossAmount = cargodetail.PaymentPrice + cargodetail.RebatePrice;
            dto.RebateAmount = cargodetail.RebatePrice;
            dto.RebateId = cargodetail.RebateId;
            dto.WalletAmount = 0;

            if (cargodetail.StatusId != (long)CargoStatusEnum.CargoStatus_Accepted)
            {
                return new BaseResultDto(false, Resource.Notification.YourCargoRequestIsNotAccepted);
            }
            if (dto.Amount < 0)
            {
                return new BaseResultDto(false, Resource.Notification.AmountNotCorrect);
            }
            else if (dto.Amount > 0 && dto.MerchantId == null && !cargodetail.FromWallet)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

            }
            if (cargodetail.FromWallet)
            {
                var walletAmount = await _walletService.GetAmountValueAsync(cargodetail.UserPet.UserId);
                cargodetail.WalletPrice = PaymentAmountHelper.GetWalletContribution(walletAmount, cargodetail.PaymentPrice);
                dto.WalletAmount = cargodetail.WalletPrice;
                await _context.SaveChangesAsync();

                if (walletAmount >= cargodetail.PaymentPrice)
                {
                    dto.Amount = 0;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Cargo.ToString();
                    dto.CallBackId = cargodetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Cargo.ToString());
                    return await StartPayment(dto);
                }
                else
                {
                    dto.Amount = cargodetail.PaymentPrice - cargodetail.WalletPrice;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Cargo.ToString();
                    dto.CallBackId = cargodetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Cargo.ToString());
                    return await StartPayment(dto);

                }
            }
            var PaymentType_cargo = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Cargo.ToString());
            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = PaymentType_cargo;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Cargo.ToString();
            dto.CallBackId = cargodetail.Id.ToString();
            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> InsertCompanionInsurancePackageSalePaymentAsyncDto(PaymentStartDto dto)
        {
            var insuranceDetail = await _context.CompanionInsurancePackageSales.Include(s => s.UserPet).Include(s => s.Rebate).FirstOrDefaultAsync(s => s.Id == dto.CompanionInsurancePackageSaleId);
            if (insuranceDetail == null || insuranceDetail.UserPet?.UserId != dto.UserId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (insuranceDetail.IsPaid)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            var insuranceRebateValidation = ValidateAppliedRebate(
                insuranceDetail.Rebate,
                insuranceDetail.Price,
                insuranceDetail.UserPet.UserId,
                RebateTypeLabels.InsurancePackageSale,
                insuranceDetail.RebatePrice);
            if (!insuranceRebateValidation.IsSuccess)
                return insuranceRebateValidation;
            dto.Amount = insuranceDetail.PaymentPrice;

            insuranceDetail.PaymentPrice = insuranceDetail.Price - insuranceDetail.RebatePrice;
            if (insuranceDetail.PaymentPrice < 0)
            {
                insuranceDetail.PaymentPrice = 0;
            }

            dto.Amount = insuranceDetail.PaymentPrice;
            dto.GrossAmount = insuranceDetail.PaymentPrice + insuranceDetail.RebatePrice;
            dto.RebateAmount = insuranceDetail.RebatePrice;
            dto.RebateId = insuranceDetail.RebateId;
            dto.WalletAmount = 0;

            if (dto.Amount < 0)
            {
                return new BaseResultDto(false, Resource.Notification.AmountNotCorrect);
            }
            else if (dto.Amount > 0 && dto.MerchantId == null && !insuranceDetail.FromWallet)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

            }
            if (insuranceDetail.FromWallet)
            {
                var walletAmount = await _walletService.GetAmountValueAsync(insuranceDetail.UserPet.UserId);
                insuranceDetail.WalletPrice = PaymentAmountHelper.GetWalletContribution(walletAmount, insuranceDetail.PaymentPrice);
                dto.WalletAmount = insuranceDetail.WalletPrice;
                await _context.SaveChangesAsync();

                if (walletAmount >= insuranceDetail.PaymentPrice)
                {
                    dto.Amount = 0;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Insurance.ToString();
                    dto.CallBackId = insuranceDetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Insurance.ToString());
                    return await StartPayment(dto);
                }
                else
                {
                    dto.Amount = insuranceDetail.PaymentPrice - insuranceDetail.WalletPrice;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Insurance.ToString();
                    dto.CallBackId = insuranceDetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Insurance.ToString());
                    return await StartPayment(dto);

                }
            }
            var PaymentType_insurance = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Insurance.ToString());
            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = PaymentType_insurance;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Insurance.ToString();
            dto.CallBackId = insuranceDetail.Id.ToString();
            return await StartPayment(dto);
        }

        public async Task<Payment> FindAsync(long id)
        {
            return await _context.Payments.Include(s => s.User).Include(s => s.Type).Include(s => s.ProductOrder).Include(s => s.Merchant).ThenInclude(s => s.Bank).AsTracking().SingleOrDefaultAsync(s => s.Id == id);
        }

        private BaseResultDto ValidateAppliedRebate(
            Rebate rebate,
            double basePrice,
            long userId,
            string typeLabel,
            double appliedRebatePrice)
        {
            if (rebate == null)
                return new BaseResultDto(true);

            var validation = _rebateService.GetRebateByCodeAsync(basePrice, userId, typeLabel, rebate.CodeValue);
            if (!validation.IsSuccess || validation.Data?.Id != rebate.Id)
                return new BaseResultDto(false, messages: validation.Messages);
            if (Math.Abs(validation.Data.FinalPrice - appliedRebatePrice) > 0.01)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            return new BaseResultDto(true);
        }

        private async Task<BaseResultDto> ValidatePaymentSnapshotAsync(Entities.Entities.Payment payment)
        {
            if (payment.Amount < 0 || payment.GrossAmount < 0 ||
                payment.RebateAmount < 0 || payment.WalletAmount < 0)
            {
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.Wallet.ToString())
            {
                return SnapshotMatches(payment, payment.Amount, 0, 0, payment.Amount)
                    ? new BaseResultDto(true)
                    : new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.ProductOrder.ToString())
            {
                var id = payment.CallBackId ?? payment.ProductOrderId;
                var item = await _context.ProductOrders.AsNoTracking()
                    .Where(s => s.Id == id && s.UserId == payment.UserId)
                    .Select(s => new { s.PaymentPrice, s.RebatePrice, s.WalletPrice })
                    .FirstOrDefaultAsync();
                return item != null && SnapshotMatches(
                    payment,
                    item.PaymentPrice + item.RebatePrice,
                    item.RebatePrice,
                    item.WalletPrice,
                    item.PaymentPrice - item.WalletPrice)
                    ? new BaseResultDto(true)
                    : new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            if (!long.TryParse(payment.CallBackId, out var referenceId))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.CompanionReserve.ToString())
            {
                var item = await _context.CompanionReserves.AsNoTracking()
                    .Where(s => s.Id == referenceId && s.BookerId == payment.UserId && !s.IsCancel)
                    .Select(s => new { s.PrePaymentPrice, s.RebatePrice, s.WalletPrice })
                    .FirstOrDefaultAsync();
                return item != null && SnapshotMatches(
                    payment,
                    item.PrePaymentPrice + item.RebatePrice,
                    item.RebatePrice,
                    item.WalletPrice,
                    item.PrePaymentPrice - item.WalletPrice)
                    ? new BaseResultDto(true)
                    : new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.PansionReserve.ToString())
            {
                var item = await _context.PansionReserves.AsNoTracking()
                    .Where(s => s.Id == referenceId && s.BookerId == payment.UserId && !s.IsCancel)
                    .Select(s => new { s.PaymentPrice, s.RebatePrice, s.WalletPrice })
                    .FirstOrDefaultAsync();
                return item != null && SnapshotMatches(
                    payment,
                    item.PaymentPrice + item.RebatePrice,
                    item.RebatePrice,
                    item.WalletPrice,
                    item.PaymentPrice - item.WalletPrice)
                    ? new BaseResultDto(true)
                    : new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.PastilAI.ToString())
            {
                var item = await _context.PastilAiSubscriptions.AsNoTracking()
                    .Where(s => s.Id == referenceId && s.UserId == payment.UserId)
                    .Select(s => new { s.PriceSnapshot, s.RebatePrice, s.WalletPrice })
                    .FirstOrDefaultAsync();
                return item != null && SnapshotMatches(
                    payment,
                    (double)(item.PriceSnapshot + item.RebatePrice),
                    (double)item.RebatePrice,
                    (double)item.WalletPrice,
                    (double)(item.PriceSnapshot - item.WalletPrice))
                    ? new BaseResultDto(true)
                    : new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.Trip.ToString())
            {
                var item = await _context.Trips.AsNoTracking()
                    .Where(s => s.Id == referenceId && s.UserPet.UserId == payment.UserId)
                    .Select(s => new { s.PaymentPrice, s.RebatePrice, s.WalletPrice })
                    .FirstOrDefaultAsync();
                return item != null && SnapshotMatches(payment, item.PaymentPrice + item.RebatePrice,
                    item.RebatePrice, item.WalletPrice, item.PaymentPrice - item.WalletPrice)
                    ? new BaseResultDto(true)
                    : new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.Cargo.ToString())
            {
                var item = await _context.Cargoes.AsNoTracking()
                    .Where(s => s.Id == referenceId && s.UserPet.UserId == payment.UserId)
                    .Select(s => new { s.PaymentPrice, s.RebatePrice, s.WalletPrice })
                    .FirstOrDefaultAsync();
                return item != null && SnapshotMatches(payment, item.PaymentPrice + item.RebatePrice,
                    item.RebatePrice, item.WalletPrice, item.PaymentPrice - item.WalletPrice)
                    ? new BaseResultDto(true)
                    : new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.Insurance.ToString())
            {
                var item = await _context.CompanionInsurancePackageSales.AsNoTracking()
                    .Where(s => s.Id == referenceId && s.UserPet.UserId == payment.UserId)
                    .Select(s => new { s.PaymentPrice, s.RebatePrice, s.WalletPrice })
                    .FirstOrDefaultAsync();
                return item != null && SnapshotMatches(payment, item.PaymentPrice + item.RebatePrice,
                    item.RebatePrice, item.WalletPrice, item.PaymentPrice - item.WalletPrice)
                    ? new BaseResultDto(true)
                    : new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            return new BaseResultDto(false, Resource.Notification.InvalidData);
        }

        private static bool SnapshotMatches(
            Entities.Entities.Payment payment,
            double grossAmount,
            double rebateAmount,
            double walletAmount,
            double gatewayAmount)
        {
            const double tolerance = 0.01;
            if (!payment.IsOnline)
            {
                return Math.Abs(payment.Amount - Math.Max(0, gatewayAmount + walletAmount)) <= tolerance;
            }
            return Math.Abs(payment.GrossAmount - grossAmount) <= tolerance &&
                   Math.Abs(payment.RebateAmount - rebateAmount) <= tolerance &&
                   Math.Abs(payment.WalletAmount - walletAmount) <= tolerance &&
                   Math.Abs(payment.Amount - Math.Max(0, gatewayAmount)) <= tolerance;
        }

        private async Task<BaseResultDto> ApplySuccessfulPaymentAsync(Entities.Entities.Payment payment)
        {
            var useWallet = payment.IsOnline;

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.PastilAI.ToString())
            {
                if (!long.TryParse(payment.CallBackId, out var subscriptionId))
                {
                    return new BaseResultDto(false, Resource.Notification.InvalidData);
                }

                return await _pastilAiSubscriptionActivator.ActivateAfterPaymentAsync(subscriptionId, payment.Id);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.ProductOrder.ToString())
            {
                return await _productOrderService.ProductPaymentCallback(payment.CallBackId ?? payment.ProductOrderId, useWallet);
            }

            if (!long.TryParse(payment.CallBackId, out var referenceId))
            {
                if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.Wallet.ToString() ||
                    payment.Type?.Label == PaymentTypeEnum.PaymentType_Wallet.ToString())
                {
                    return await _walletService.WalletPaymentCallback(payment);
                }

                return new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.CompanionReserve.ToString())
            {
                return await _companionReserveService.CompanionReservePaymentCallback(referenceId, useWallet);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.PansionReserve.ToString())
            {
                return await _pansionReserve.PansionReservePaymentCallback(referenceId, useWallet);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.Trip.ToString())
            {
                return await _tripService.TripPaymentCallback(referenceId, useWallet);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.Cargo.ToString())
            {
                return await _cargoService.CargoPaymentCallback(referenceId, useWallet);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.Insurance.ToString())
            {
                return await _companionInsurance.CompanionInsurancePackageSalePaymentCallback(referenceId, useWallet);
            }

            return new BaseResultDto(false, Resource.Notification.InvalidData);
        }

        private async Task<ManualPaymentTargetResult> ResolveManualPaymentTargetAsync(ManualPaymentDto dto)
        {
            if (dto.TargetType == PaymentCallbackTypeEnum.Wallet)
            {
                if (!dto.UserId.HasValue || !dto.Amount.HasValue || dto.Amount.Value <= 0 ||
                    !await _context.Users.AnyAsync(s => s.Id == dto.UserId.Value))
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.InvalidData);
                }

                return ManualPaymentTargetResult.Success(dto.UserId.Value, dto.Amount.Value, null);
            }

            if (string.IsNullOrWhiteSpace(dto.ReferenceId))
            {
                return ManualPaymentTargetResult.Fail(Resource.Notification.InvalidData);
            }

            if (dto.TargetType == PaymentCallbackTypeEnum.ProductOrder)
            {
                var item = await _context.ProductOrders.AsNoTracking()
                    .Where(s => s.Id == dto.ReferenceId)
                    .Select(s => new { s.UserId, s.PaymentPrice, s.IsPaid })
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.NothingFound);
                }
                if (item.IsPaid)
                {
                    return ManualPaymentTargetResult.Fail("این سفارش قبلاً پرداخت شده است.");
                }
                return ManualPaymentTargetResult.Success(item.UserId, item.PaymentPrice, dto.ReferenceId);
            }

            if (!long.TryParse(dto.ReferenceId, out var referenceId))
            {
                return ManualPaymentTargetResult.Fail(Resource.Notification.InvalidData);
            }

            if (dto.TargetType == PaymentCallbackTypeEnum.CompanionReserve)
            {
                var item = await _context.CompanionReserves.AsNoTracking()
                    .Where(s => s.Id == referenceId)
                    .Select(s => new { s.BookerId, s.PrePaymentPrice, s.IsReserved })
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.NothingFound);
                }
                if (item.IsReserved)
                {
                    return ManualPaymentTargetResult.Fail("این رزرو قبلاً پرداخت شده است.");
                }
                return ManualPaymentTargetResult.Success(item.BookerId, item.PrePaymentPrice, dto.ReferenceId);
            }

            if (dto.TargetType == PaymentCallbackTypeEnum.PansionReserve)
            {
                var item = await _context.PansionReserves.AsNoTracking()
                    .Where(s => s.Id == referenceId)
                    .Select(s => new { s.BookerId, s.PaymentPrice, s.IsReserved })
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.NothingFound);
                }
                if (item.IsReserved)
                {
                    return ManualPaymentTargetResult.Fail("این رزرو قبلاً پرداخت شده است.");
                }
                return ManualPaymentTargetResult.Success(item.BookerId, item.PaymentPrice, dto.ReferenceId);
            }

            if (dto.TargetType == PaymentCallbackTypeEnum.Trip)
            {
                var item = await _context.Trips.AsNoTracking()
                    .Where(s => s.Id == referenceId)
                    .Select(s => new { s.UserId, s.PaymentPrice, s.IsPaid })
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.NothingFound);
                }
                if (item.IsPaid)
                {
                    return ManualPaymentTargetResult.Fail("این سفر قبلاً پرداخت شده است.");
                }
                return ManualPaymentTargetResult.Success(item.UserId, item.PaymentPrice, dto.ReferenceId);
            }

            if (dto.TargetType == PaymentCallbackTypeEnum.Cargo)
            {
                var item = await _context.Cargoes.AsNoTracking()
                    .Where(s => s.Id == referenceId)
                    .Select(s => new { UserId = s.UserPet.UserId, s.PaymentPrice, s.IsPaid })
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.NothingFound);
                }
                if (item.IsPaid)
                {
                    return ManualPaymentTargetResult.Fail("این درخواست کارگو قبلاً پرداخت شده است.");
                }
                return ManualPaymentTargetResult.Success(item.UserId, item.PaymentPrice, dto.ReferenceId);
            }

            if (dto.TargetType == PaymentCallbackTypeEnum.Insurance)
            {
                var item = await _context.CompanionInsurancePackageSales.AsNoTracking()
                    .Where(s => s.Id == referenceId)
                    .Select(s => new { UserId = s.UserPet.UserId, s.PaymentPrice, s.IsPaid })
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.NothingFound);
                }
                if (item.IsPaid)
                {
                    return ManualPaymentTargetResult.Fail("این بیمه قبلاً پرداخت شده است.");
                }
                return ManualPaymentTargetResult.Success(item.UserId, item.PaymentPrice, dto.ReferenceId);
            }

            if (dto.TargetType == PaymentCallbackTypeEnum.PastilAI)
            {
                if (!dto.UserId.HasValue || !await _context.Users.AnyAsync(s => s.Id == dto.UserId.Value))
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.UserNotFound);
                }

                var plan = await _context.PastilAiPlans.AsNoTracking().FirstOrDefaultAsync(s =>
                    s.Id == referenceId && s.Active && s.PurchaseEnabled &&
                    s.Code != PastilAiPlanCode.Free.ToString());
                if (plan == null)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.NothingFound);
                }

                return ManualPaymentTargetResult.Success(dto.UserId.Value, (double)plan.Price, null, plan.Id);
            }

            return ManualPaymentTargetResult.Fail(Resource.Notification.InvalidData);
        }

        private static string GetPaymentTypeLabel(PaymentCallbackTypeEnum targetType)
        {
            return targetType switch
            {
                PaymentCallbackTypeEnum.ProductOrder => PaymentTypeEnum.PaymentType_ProductOrder.ToString(),
                PaymentCallbackTypeEnum.CompanionReserve => PaymentTypeEnum.PaymentType_CompanionReserve.ToString(),
                PaymentCallbackTypeEnum.PansionReserve => PaymentTypeEnum.PaymentType_PansionReserve.ToString(),
                PaymentCallbackTypeEnum.Trip => PaymentTypeEnum.PaymentType_Trip.ToString(),
                PaymentCallbackTypeEnum.Cargo => PaymentTypeEnum.PaymentType_Cargo.ToString(),
                PaymentCallbackTypeEnum.Insurance => PaymentTypeEnum.PaymentType_Insurance.ToString(),
                PaymentCallbackTypeEnum.PastilAI => PaymentTypeEnum.PaymentType_PastilAI.ToString(),
                PaymentCallbackTypeEnum.Wallet => PaymentTypeEnum.PaymentType_Wallet.ToString(),
                _ => null
            };
        }

        private static long? ParseNullableLong(string value)
        {
            return long.TryParse(value, out var result) ? result : null;
        }

        private static string AppendDescription(string description, string message)
        {
            return string.IsNullOrWhiteSpace(description) ? message : $"{description} | {message}";
        }

        private static ManualPaymentVDto MapManualPayment(Entities.Entities.Payment payment, PaymentCallbackTypeEnum targetType)
        {
            return new ManualPaymentVDto
            {
                PaymentId = payment.Id,
                PaymentCode = payment.PaymentCode,
                TargetType = targetType,
                CallbackId = payment.CallBackId,
                UserId = payment.UserId,
                Amount = payment.Amount,
                FileId = payment.FileId,
                RefNumber = payment.RefNumber,
                Description = payment.Description,
                IsSuccess = payment.IsSuccess == true
            };
        }

        private async Task<string> CreateUniquePaymentCodeAsync()
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var token = PaymentCodeGenerator.CreatePaymentToken();
                if (!await _context.Payments.AsNoTracking().AnyAsync(payment => payment.PaymentCode == token))
                    return token;
            }

            throw new InvalidOperationException("امکان تولید توکن یکتای پرداخت وجود ندارد.");
        }

        private bool TryGetIdempotencyKey(out string idempotencyKey, out string error)
        {
            idempotencyKey = null;
            error = null;
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null || !request.Headers.TryGetValue("Idempotency-Key", out var values))
                return true;

            var rawValue = values.Count == 1 ? values[0]?.Trim() : null;
            if (!Guid.TryParseExact(rawValue, "D", out var parsed) || parsed == Guid.Empty)
            {
                error = "هدر Idempotency-Key باید یک UUID معتبر باشد.";
                return false;
            }

            idempotencyKey = parsed.ToString("D");
            return true;
        }

        private static bool IsSameCheckout(Payment payment, PaymentStartDto dto)
        {
            const double tolerance = 0.01;
            return payment.UserId == dto.UserId &&
                   string.Equals(payment.CallBackTypeLabel, dto.CallBackTypeLabel, StringComparison.Ordinal) &&
                   string.Equals(payment.CallBackId, dto.CallBackId, StringComparison.Ordinal) &&
                   payment.MerchantId == dto.MerchantId &&
                   Math.Abs(payment.Amount - dto.Amount) <= tolerance &&
                   Math.Abs(payment.GrossAmount - dto.GrossAmount) <= tolerance &&
                   Math.Abs(payment.RebateAmount - dto.RebateAmount) <= tolerance &&
                   Math.Abs(payment.WalletAmount - dto.WalletAmount) <= tolerance;
        }

        private static BaseResultDto CreateIdempotentReplayResult(Payment payment)
        {
            var dto = new PaymentStartDto
            {
                PaymentId = payment.Id,
                PaymentCode = payment.PaymentCode,
                IsOnline = payment.IsOnline,
                MerchantId = payment.MerchantId,
                RebateId = payment.RebateId,
                ProductOrderId = payment.ProductOrderId,
                CompanionReserveId = payment.CompanionReserveId,
                TripId = payment.TripId,
                CargoId = payment.CargoId,
                CompanionInsurancePackageSaleId = payment.CompanionInsurancePackageSaleId,
                Amount = payment.Amount,
                GrossAmount = payment.GrossAmount,
                RebateAmount = payment.RebateAmount,
                WalletAmount = payment.WalletAmount,
                UserId = payment.UserId,
                TypeId = payment.TypeId,
                CallBackTypeLabel = payment.CallBackTypeLabel,
                CallBackId = payment.CallBackId,
                PaymentUrl = payment.PaymentUrl,
                PaymentIsLink = payment.PaymentIsLink
            };

            if (payment.IsSuccess == false && string.IsNullOrWhiteSpace(payment.PaymentUrl))
                return new BaseResultDto<PaymentStartDto>(false, "تلاش قبلی این Checkout ناموفق بوده است.", dto);

            if (payment.IsSuccess == null && string.IsNullOrWhiteSpace(payment.PaymentUrl))
                return new BaseResultDto<PaymentStartDto>(false, "درخواست پرداخت در حال پردازش است؛ همین درخواست را دوباره ارسال کنید.", dto);

            return new BaseResultDto<PaymentStartDto>(true, dto);
        }

        private async Task MarkManualPaymentDateAsync(PaymentCallbackTypeEnum targetType, string callbackId)
        {
            if (!long.TryParse(callbackId, out var referenceId))
            {
                return;
            }

            var now = DateTime.Now;
            if (targetType == PaymentCallbackTypeEnum.Trip)
            {
                await _context.Trips.Where(s => s.Id == referenceId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.ManualPayDate, now));
            }
            else if (targetType == PaymentCallbackTypeEnum.Insurance)
            {
                await _context.CompanionInsurancePackageSales.Where(s => s.Id == referenceId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.ManualPayDate, now));
            }
        }

        private sealed class ManualPaymentTargetResult
        {
            public bool IsSuccess { get; private set; }
            public string Error { get; private set; }
            public long UserId { get; private set; }
            public double Amount { get; private set; }
            public string ReferenceId { get; set; }
            public long? PlanId { get; private set; }

            public static ManualPaymentTargetResult Success(long userId, double amount, string referenceId, long? planId = null)
            {
                return new ManualPaymentTargetResult
                {
                    IsSuccess = true,
                    UserId = userId,
                    Amount = amount,
                    ReferenceId = referenceId,
                    PlanId = planId
                };
            }

            public static ManualPaymentTargetResult Fail(string error)
            {
                return new ManualPaymentTargetResult
                {
                    IsSuccess = false,
                    Error = error
                };
            }
        }
    }
}
