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
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using Application.Services.GroomerSrvs.GroomerReserveSrv.Iface;
using Application.Services.ClinicSrvs.ClinicReserveSrv.Iface;
using Application.Services.CoachSrvs.CoachReserveSrv.Iface;
using Application.Services.PastilAISrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Iface;
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
        private readonly ISchoolReserveService _schoolReserve;
        private readonly IGroomerReserveService _groomerReserve;
        private readonly IClinicReserveService _clinicReserve;
        private readonly ICoachReserveService _coachReserve;
        private readonly IPastilAiSubscriptionActivator _pastilAiSubscriptionActivator;
        private readonly IConsultationPurchaseActivator _consultationPurchaseActivator;
        private readonly IRebateService _rebateService;
        private readonly IConfiguration _configuration;
        private readonly IPaymentTestModeService _paymentTestModeService;
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentService> _logger;
        private readonly Application.Common.PostCommit.IPostCommitQueue _postCommit;

        public PaymentService(
            IDataBaseContext context,
            IMapper mapper,
            ICodeService codeService,
            IWalletService walletService,
            IMerchantService merchantService,
            IPansionReserveService pansionReserve,
            ISchoolReserveService schoolReserve,
            IGroomerReserveService groomerReserve,
            IClinicReserveService clinicReserve,
            ICoachReserveService coachReserve,
            IProductOrderService productOrderService,
            ICompanionReserveService companionReserveService,
            ITripService tripService,
            ICargoService cargoService,
            ICompanionInsurancePackageSaleService companionInsurance,
            IPastilAiSubscriptionActivator pastilAiSubscriptionActivator,
            IConsultationPurchaseActivator consultationPurchaseActivator,
            IRebateService rebateService,
            IPaymentTestModeService paymentTestModeService,
            ICurrentUserHelper currentUserHelper,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<PaymentService> logger,
            Application.Common.PostCommit.IPostCommitQueue postCommit)
            : base(context, mapper)
        {
            _postCommit = postCommit;
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
            _schoolReserve = schoolReserve;
            _groomerReserve = groomerReserve;
            _clinicReserve = clinicReserve;
            _coachReserve = coachReserve;
            _pastilAiSubscriptionActivator = pastilAiSubscriptionActivator;
            _consultationPurchaseActivator = consultationPurchaseActivator;
            _rebateService = rebateService;
            _paymentTestModeService = paymentTestModeService;
            _currentUserHelper = currentUserHelper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<BaseResultDto> InsertWalletPaymentAsyncDto(PaymentStartDto dto)
        {
            if (!Application.Services.ProductSrvs.WalletSrv.WalletChargeRules.IsValid(dto.Amount))
            {
                return new BaseResultDto(false, string.Format(
                    Resource.Pattern.WalletChargeAmountOutOfRange,
                    Application.Services.ProductSrvs.WalletSrv.WalletChargeRules.MinAmount.ToString("N0", System.Globalization.CultureInfo.InvariantCulture),
                    Application.Services.ProductSrvs.WalletSrv.WalletChargeRules.MaxAmount.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)));
            }

            if (dto.MerchantId == null)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);
            }

            var paymentTypeWallet = await _codeService.GetByLabelAsync(
                PaymentTypeEnum.PaymentType_Wallet.ToString());

            dto.IsOnline = true;
            ScopePaymentTarget(dto, PaymentCallbackTypeEnum.Wallet, null);
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

                // این فیلدها را فقط سرور و درگاه پر می‌کنند؛ مقداری که کلاینت در بدنه‌ی PaymentStartDto فرستاده دور ریخته می‌شود
                dto.PaymentCode = null;
                dto.PaymentUrl = null;
                dto.PaymentIsLink = false;
                dto.IsTestMode = false;
                dto.TestSuccessUrl = null;
                dto.TestFailureUrl = null;
                dto.CallbackUrl = null;
                dto.GatewayToken = null;

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

                // همان قاعده‌ی لیست درگاه‌ها: همه زرین‌پال، مدیر اصلی علاوه بر آن درگاه تستی (merchantId قدیمیِ ذخیره‌شده در سبد یا
                // دست‌ساز پذیرفته نمی‌شود و کاربر باید از لیست دوباره انتخاب کند)
                if (!isInternalSettlement)
                {
                    var payer = _currentUserHelper.CurrentUser;
                    if (payer != null && payer.UserId != dto.UserId.Value)
                        payer = null;
                    var allowedBankIds = Application.Services.Order.MerchantSrv.MerchantGatewayRules.AllowedBankIds(payer);
                    var merchantAllowed = await _context.Merchants.AsNoTracking()
                        .AnyAsync(s => s.Id == dto.MerchantId.Value && s.Active && allowedBankIds.Contains(s.BankId));
                    if (!merchantAllowed)
                        return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);
                }

                await using var createTransaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                if (idempotencyKey != null)
                {
                    var existingPayment = await _context.Payments.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.IdempotencyKey == idempotencyKey);
                    if (existingPayment != null)
                    {
                        await createTransaction.RollbackAsync();
                        if (!IsSameCheckout(existingPayment, dto))
                            return new BaseResultDto(false, Resource.Notification.PaymentIdempotencyKeyAlreadyUsedForDifferentCheckout);

                        return CreateIdempotentReplayResult(existingPayment);
                    }
                }

                if (!string.IsNullOrWhiteSpace(dto.CallBackTypeLabel) &&
                    !string.IsNullOrWhiteSpace(dto.CallBackId))
                {
                    // تلاش نیمه‌کاره‌ی قبلی همین هدف (برگشت از درگاه بدون پرداخت / نرسیدن به درگاه) کاربر را قفل نمی‌کند:
                    // کنار گذاشته می‌شود و پرداخت تازه با لینک درگاه تازه ساخته می‌شود. (تکرار همان درخواست با همان
                    // Idempotency-Key بالاتر همان لینک قبلی را برمی‌گرداند و به این‌جا نمی‌رسد.)
                    // فقط پرداخت موفق («قبلاً پرداخت شده») یا callbackِ در حال verify (چند ثانیه) جلوی تلاش تازه را می‌گیرد.
                    var targetState = await PaymentAttemptRules.ReleaseForRetryAsync(_context, dto.CallBackTypeLabel, dto.CallBackId);
                    if (targetState != PaymentAttemptRules.TargetState.Free)
                    {
                        await createTransaction.RollbackAsync();
                        return targetState == PaymentAttemptRules.TargetState.Paid
                            ? PaymentAttemptRules.AlreadyPaid(Resource.Notification.PaymentTargetAlreadyPaid, dto.CallBackTypeLabel, dto.CallBackId)
                            : new BaseResultDto(false, Resource.Notification.PaymentPreviousAttemptVerifying);
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

                // توکن داخل مسیر (نه query): هر درگاه پارامترهای خودش را بی‌دردسر به انتهای آدرس اضافه می‌کند.
                // شکل قدیمی ?callbackToken= هم در CallbackController پذیرفته می‌شود (پرداخت‌های در جریانِ قبل از انتشار).
                dto.CallbackUrl = $"{paymentBaseUrl}/callback/{item.Id}/{item.CallbackToken}";
                var startResult = await _merchantService.StartAsync(dto);

                item.PaymentUrl = dto.PaymentUrl;
                item.PaymentIsLink = dto.PaymentIsLink;
                item.Token = dto.GatewayToken;
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

                // جزئیات exception فقط در لاگ بالا؛ به کاربر/کلاینت پیام عمومی
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
                return new BaseResultDto<ManualPaymentVDto>(false, Resource.Notification.PaymentRefNumberAlreadyUsedForSuccessfulPayment, null);
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
                return new BaseResultDto<ManualPaymentVDto>(false, Resource.Notification.PaymentTypeNotConfiguredInSystem, null);
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
                payment.Description = AppendDescription(payment.Description, Resource.Notification.PaymentApplyResultToTargetFailed);
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

                if (payment.IsSuccess == null && payment.CreateDate < DateTime.Now - PaymentAttemptRules.PendingAttemptLifetime)
                {
                    payment.IsSuccess = false;
                    payment.GatewayStatus = "EXPIRED";
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<PaymentDto>(false, Resource.Notification.Unsuccess, await MapPaymentForDisplayAsync(payment));
                }

                if (payment.IsSuccess == false)
                {
                    // درگاهِ یک تلاش کنارگذاشته‌شده پرداخت شد: verify نمی‌شود تا بانک مبلغ را برگرداند؛ کاربر باید دلیلش را بداند
                    var failMessage = payment.GatewayStatus == PaymentAttemptRules.SupersededGatewayStatus
                        ? Resource.Notification.PaymentAttemptSuperseded
                        : payment.GatewayStatus == RefundedToWalletStatus
                            ? Resource.Notification.PaymentRefundedToWallet
                            : Resource.Notification.Unsuccess;
                    return new BaseResultDto<PaymentDto>(
                        false,
                        failMessage,
                        await MapPaymentForDisplayAsync(payment));
                }

                if (payment.IsSuccess != true)
                {
                    // قبل از verify با بانک «در حال تأیید» علامت می‌خورد تا تلاش تازه‌ی هم‌زمانِ کاربر این پرداخت را کنار نگذارد
                    // (وگرنه بانک پول را قطعی می‌کرد و پرداختِ کنارگذاشته روی رزرو/سفارش اعمال نمی‌شد).
                    if (!await PaymentAttemptRules.TryMarkVerifyingAsync(_context, payment.Id))
                    {
                        // بین خواندن و علامت‌زدن، تلاش تازه‌ی کاربر این پرداخت را کنار گذاشت یا callback دیگری نتیجه را ثبت کرد
                        await _context.Entry(payment).ReloadAsync();
                        if (payment.AppliedDate.HasValue)
                            return new BaseResultDto<PaymentDto>(true, await MapPaymentForDisplayAsync(payment));
                        if (payment.IsSuccess != true)
                        {
                            return new BaseResultDto<PaymentDto>(
                                false,
                                payment.GatewayStatus == PaymentAttemptRules.SupersededGatewayStatus
                                    ? Resource.Notification.PaymentAttemptSuperseded
                                    : Resource.Notification.Unsuccess,
                                await MapPaymentForDisplayAsync(payment));
                        }
                    }
                    else
                    {
                        BaseResultDto callbackResult;
                        try
                        {
                            callbackResult = await _merchantService.CallbackAsync(payment);
                        }
                        finally
                        {
                            // اگر نتیجه‌ای ثبت نشد (خطای تنظیمات/exception)، علامت برداشته می‌شود تا تلاش بعدی کاربر قفل نماند
                            await PaymentAttemptRules.ClearVerifyingAsync(_context, payment.Id);
                        }
                        if (!callbackResult.IsSuccess && callbackResult.Code == (int)ResultCodeEnum.PaymentVerificationPending)
                        {
                            // جواب بانک نرسید: پرداخت ناموفق نیست (پول ممکن است کسر شده باشد)؛ سفارش/کیف پول دست نمی‌خورد و
                            // job تطبیق بعد از چند دقیقه verify را دوباره می‌زند.
                            _logger.LogWarning("Payment {PaymentId} verification is pending (no definitive answer from the gateway).", payment.Id);
                            return new BaseResultDto<PaymentDto>(
                                false,
                                callbackResult.Messages,
                                await MapPaymentForDisplayAsync(payment),
                                (int)ResultCodeEnum.PaymentVerificationPending);
                        }
                        if (!callbackResult.IsSuccess)
                        {
                            await HandleFailedPaymentAsync(payment);
                            return new BaseResultDto<PaymentDto>(
                                false,
                                callbackResult.Messages,
                                await MapPaymentForDisplayAsync(payment));
                        }
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

                // استثنا ممکن است بعد از ثبت واقعی رخ داده باشد (مثلاً موقع ساخت پاسخ/اعلان‌ها): وضعیت ماندگار را از دیتابیس
                // می‌خوانیم و اگر پرداخت اعمال شده به کاربر «ناموفق» نشان نمی‌دهیم.
                try
                {
                    (_context as DbContext)?.ChangeTracker.Clear();
                    var durable = await _context.Payments.AsNoTracking().FirstOrDefaultAsync(s => s.Id == paymentId);
                    if (durable?.AppliedDate != null && IsValidCallbackToken(durable.CallbackToken, callbackToken))
                        return new BaseResultDto<PaymentDto>(true, await MapPaymentForDisplayAsync(durable));
                }
                catch (Exception recheckException)
                {
                    _logger.LogError(recheckException, "Re-checking payment {PaymentId} after a callback error failed.", paymentId);
                }
                return new BaseResultDto<PaymentDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        private async Task<BaseResultDto<PaymentDto>> ApplyAndMapPaymentAsync(
            Entities.Entities.Payment payment)
        {
            // کارهای جانبیِ ثبت‌شده‌ی تلاشِ قبلیِ ناموفق (مثلاً در job تطبیق که چند پرداخت را با یک scope می‌گذراند) نباید اجرا شوند
            _postCommit.Clear();
            // تا پایان اعمال، پوش‌هایی که داخل تراکنش فرستاده می‌شوند (مثلاً ثبت سفارش اکسپرس) به بعد از Commit می‌روند
            _postCommit.Activate();
            try
            {
                return await ApplyAndMapPaymentCoreAsync(payment);
            }
            finally
            {
                _postCommit.Deactivate();
            }
        }

        private async Task<BaseResultDto<PaymentDto>> ApplyAndMapPaymentCoreAsync(
            Entities.Entities.Payment payment)
        {
            var applyWatch = System.Diagnostics.Stopwatch.StartNew();
            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
            // The callback path already tracked this entity before entering the
            // transaction. Reload it under Serializable isolation so a duplicate
            // callback cannot apply a stale, already-applied payment a second time.
            await _context.Entry(payment).ReloadAsync();
            var lockedPayment = payment;
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

            // هر هدف فقط یک پرداخت اعمال‌شده دارد. پرداخت تأییدشده‌ی دوم برای همان هدف (پرداخت تکراری) قبلاً بی‌صدا «موفق» می‌شد
            // چون callback هدف برای موردِ پرداخت‌شده true برمی‌گرداند؛ حالا اعمال نمی‌شود و مبلغش به کیف پول برمی‌گردد.
            if (!string.IsNullOrWhiteSpace(lockedPayment.CallBackId) &&
                await _context.Payments.AsNoTracking().AnyAsync(s =>
                    s.Id != lockedPayment.Id &&
                    s.CallBackTypeLabel == lockedPayment.CallBackTypeLabel &&
                    s.CallBackId == lockedPayment.CallBackId &&
                    s.AppliedDate != null))
            {
                await transaction.RollbackAsync();
                if (await RefundUnappliedToWalletAsync(lockedPayment))
                    return new BaseResultDto<PaymentDto>(false, Resource.Notification.PaymentDuplicateRefundedToWallet, await MapPaymentForDisplayAsync(lockedPayment));
                return new BaseResultDto<PaymentDto>(false, Resource.Notification.PaymentTargetAlreadyPaid, await MapPaymentForDisplayAsync(lockedPayment));
            }

            var snapshotValidation = await ValidatePaymentSnapshotAsync(lockedPayment);
            if (!snapshotValidation.IsSuccess)
            {
                await transaction.RollbackAsync();
                if (await RefundUnappliedToWalletAsync(lockedPayment))
                    return new BaseResultDto<PaymentDto>(false, Resource.Notification.PaymentRefundedToWallet, await MapPaymentForDisplayAsync(lockedPayment));
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
                else if (await RefundUnappliedToWalletAsync(lockedPayment))
                {
                    return new BaseResultDto<PaymentDto>(false, Resource.Notification.PaymentRefundedToWallet, await MapPaymentForDisplayAsync(lockedPayment));
                }
                return new BaseResultDto<PaymentDto>(false, applyResult.Messages, await MapPaymentForDisplayAsync(lockedPayment));
            }

            var isTestPayment = lockedPayment.GatewayStatus?.StartsWith("TEST_") == true;
            if (isTestPayment)
                await MarkTestTargetSettledAsync(lockedPayment);

            lockedPayment.AppliedDate = DateTime.UtcNow;
            lockedPayment.GatewayStatus = isTestPayment
                ? "TEST_APPLIED"
                : "APPLIED";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // پیامک/پوش/اعلان‌های این پرداخت فقط بعد از Commit موفق و در پس‌زمینه می‌روند؛ کاربر منتظرشان نمی‌ماند
            _postCommit.Flush();
            if (applyWatch.ElapsedMilliseconds > 5000)
                _logger.LogWarning("Applying payment {PaymentId} ({Target}) took {Ms} ms.", lockedPayment.Id, lockedPayment.CallBackTypeLabel, applyWatch.ElapsedMilliseconds);

            return new BaseResultDto<PaymentDto>(
                true,
                applyResult.Messages,
                await MapPaymentForDisplayAsync(lockedPayment));
        }

        private const string GatewaySuccessStatus = "SUCCESS";
        private const string RefundedToWalletStatus = "REFUNDED_TO_WALLET";
        private const int ReconcileBatchSize = 50;
        // کاربر حداقل ۱۰ دقیقه فرصت دارد خودش از درگاه برگردد؛ بعد از عمر درگاه (۳۰ دقیقه) callback پرداخت را EXPIRED می‌کند
        // verify ناموفقِ job ذخیره نمی‌شود (کاربر ممکن است هنوز در درگاه باشد)، پس ۳ دقیقه امن است؛ job هر ۵ دقیقه اجرا می‌شود
        private static readonly TimeSpan ReconcileAfter = TimeSpan.FromMinutes(3);

        public async Task<int> ReconcileUnconfirmedAsync()
        {
            var now = DateTime.Now;
            var olderThan = now - ReconcileAfter;
            var newerThan = now - PaymentAttemptRules.PendingAttemptLifetime + TimeSpan.FromMinutes(1);
            var zarinpal = (long)Common.Enumerable.MerchantEnum.zarinpal;
            var ids = await _context.Payments.AsNoTracking()
                .Where(s => s.IsSuccess == null && s.AppliedDate == null && s.GatewayStatus == null &&
                            s.IsOnline && s.Amount > 0 && s.Token != null && s.CallbackToken != null &&
                            s.CreateDate <= olderThan && s.CreateDate >= newerThan &&
                            s.Merchant.BankId == zarinpal)
                .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .Take(ReconcileBatchSize)
                .ToListAsync();

            var applied = 0;
            foreach (var id in ids)
            {
                try
                {
                    var payment = await FindAsync(id);
                    if (payment == null || payment.IsSuccess != null)
                        continue;
                    // همان قفل callback: اگر کاربر همین لحظه برگشت، فقط یکی verify می‌کند و تلاش تازه کنارش نمی‌گذارد
                    if (!await PaymentAttemptRules.TryMarkVerifyingAsync(_context, payment.Id))
                        continue;

                    BaseResultDto verified;
                    try
                    {
                        verified = await _merchantService.ReconcileUnconfirmedAsync(payment);
                    }
                    finally
                    {
                        await PaymentAttemptRules.ClearVerifyingAsync(_context, payment.Id);
                    }
                    if (!verified.IsSuccess)
                        continue;

                    await _context.Entry(payment).ReloadAsync();
                    var result = await ApplyAndMapPaymentAsync(payment);
                    if (result.IsSuccess)
                        applied++;
                    _logger.LogWarning(
                        "Payment {PaymentId} was paid at the gateway but its callback never arrived; reconciled (applied: {Applied}).",
                        id, result.IsSuccess);
                }
                catch (Exception exception)
                {
                    // یک پرداخت خراب بقیه را متوقف نمی‌کند؛ اجرای بعدی (تا پایان عمر درگاه) دوباره تلاش می‌کند
                    _logger.LogError(exception, "Reconciling unconfirmed payment {PaymentId} failed.", id);
                }
                finally
                {
                    // هر پرداخت با context تمیز؛ موجودیت‌های tracked پرداخت قبلی در ذخیره‌ی بعدی نمی‌آیند
                    (_context as DbContext)?.ChangeTracker.Clear();
                }
            }
            return applied;
        }

        // بانک پول را قطعی کرده ولی اعمال روی هدف شکست خورد (مبلغ هدف عوض شده، کیف پول کافی نبود، هدف لغو شد، ...):
        // به‌جای رها کردن پولِ گرفته‌شده، کل مبلغ درگاه یک‌بار (ردیف دفتر PaymentRefund:{id}) به کیف پول کاربر برمی‌گردد و پرداخت
        // IsSuccess=false می‌شود تا هدف دوباره قابل پرداخت باشد و این پرداخت هرگز بعداً اعمال نشود. فقط درگاه واقعی (نه TEST_ / کیف پول).
        // exception (خطای گذرا) به این‌جا نمی‌رسد: بالا می‌رود تا رفرش صفحه‌ی callback دوباره اعمال را امتحان کند.
        private async Task<bool> RefundUnappliedToWalletAsync(Entities.Entities.Payment payment)
        {
            if (payment.GatewayStatus != GatewaySuccessStatus || !payment.IsOnline || payment.Amount <= 0 || payment.AppliedDate.HasValue)
                return false;

            // اعمالِ rollback‌شده ممکن است موجودیت‌های tracked نیمه‌کاره (رزرو/سفارش) گذاشته باشد؛ SaveChanges بعدی نباید آن‌ها را ذخیره کند
            _context.Entry(payment).Context.ChangeTracker.Clear();

            await using var refundTransaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
            var claimed = await _context.Payments
                .Where(s => s.Id == payment.Id && s.IsSuccess == true && s.AppliedDate == null && s.GatewayStatus == GatewaySuccessStatus)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.IsSuccess, false)
                    .SetProperty(s => s.GatewayStatus, RefundedToWalletStatus));
            if (claimed == 0)
            {
                await refundTransaction.RollbackAsync();
                return false;
            }

            var ledgerName = $"PaymentRefund:{payment.Id}";
            if (!await _context.Wallets.AsNoTracking().AnyAsync(s => s.Name == ledgerName && s.UserId == payment.UserId && !s.Deleted))
            {
                var credit = await _walletService.InsertAsyncDto(new WalletDto
                {
                    Name = ledgerName,
                    Amount = payment.Amount,
                    IsIncrease = true,
                    UserId = payment.UserId,
                    PaymentId = payment.Id,
                    Painding = false
                });
                if (!credit.IsSuccess)
                {
                    await refundTransaction.RollbackAsync();
                    return false;
                }
            }

            await refundTransaction.CommitAsync();
            payment.IsSuccess = false;
            payment.GatewayStatus = RefundedToWalletStatus;
            _logger.LogWarning(
                "Verified payment {PaymentId} ({CallbackType}/{CallbackId}) could not be applied; {Amount} refunded to wallet of user {UserId}.",
                payment.Id, payment.CallBackTypeLabel, payment.CallBackId, payment.Amount, payment.UserId);
            return true;
        }

        // پرداخت تستی (درگاه تستی پاستیل / حالت تست) پول واقعی ندارد: هدفش «تسویه‌شده» علامت می‌خورد تا در تسویه‌ی
        // نماینده/فروشگاه سهمی بابتش پرداخت نشود. داخل تراکنش اعمال صدا زده می‌شود.
        private async Task MarkTestTargetSettledAsync(Entities.Entities.Payment payment)
        {
            var label = payment.CallBackTypeLabel;
            if (label == nameof(PaymentCallbackTypeEnum.ProductOrder))
            {
                var orderId = payment.CallBackId ?? payment.ProductOrderId;
                await _context.ProductOrders.Where(s => s.Id == orderId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Permitted, true));
                return;
            }
            if (!long.TryParse(payment.CallBackId, out var targetId))
                return;
            switch (label)
            {
                case nameof(PaymentCallbackTypeEnum.CompanionReserve):
                    await _context.CompanionReserves.Where(s => s.Id == targetId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Permitted, true));
                    break;
                case nameof(PaymentCallbackTypeEnum.CompanionReserveBatch):
                    await _context.CompanionReserves.Where(s => s.BatchId == targetId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Permitted, true));
                    break;
                case nameof(PaymentCallbackTypeEnum.PansionReserve):
                    await _context.PansionReserves.Where(s => s.Id == targetId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Permitted, true));
                    break;
                case nameof(PaymentCallbackTypeEnum.SchoolReserve):
                    await _context.SchoolReserves.Where(s => s.Id == targetId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Permitted, true));
                    break;
                case nameof(PaymentCallbackTypeEnum.GroomerReserve):
                    await _context.GroomerReserves.Where(s => s.Id == targetId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Permitted, true));
                    break;

                case nameof(PaymentCallbackTypeEnum.ClinicReserve):
                    await _context.ClinicReserves.Where(s => s.Id == targetId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Permitted, true));
                    break;
                case nameof(PaymentCallbackTypeEnum.CoachReserve):
                    await _context.CoachReserves.Where(s => s.Id == targetId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Permitted, true));
                    break;
                case nameof(PaymentCallbackTypeEnum.ConsultationPurchase):
                    await _context.ConsultationPurchases.Where(s => s.Id == targetId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Permitted, true));
                    break;
            }
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
                nameof(PaymentCallbackTypeEnum.ConsultationPurchase) when long.TryParse(payment.CallBackId, out var consultationPurchaseRefId) =>
                    await _context.ConsultationPurchases.AsNoTracking()
                        .Where(item => item.Id == consultationPurchaseRefId)
                        .Select(item => item.PurchaseCode)
                        .FirstOrDefaultAsync(),
                nameof(PaymentCallbackTypeEnum.GroomerReserve) when long.TryParse(payment.CallBackId, out var groomerReserveId) =>
                    await _context.GroomerReserves.AsNoTracking()
                        .Where(item => item.Id == groomerReserveId)
                        .Select(item => item.ReserveCode)
                        .FirstOrDefaultAsync(),

                nameof(PaymentCallbackTypeEnum.ClinicReserve) when long.TryParse(payment.CallBackId, out var clinicReserveId) =>
                    await _context.ClinicReserves.AsNoTracking()
                        .Where(item => item.Id == clinicReserveId)
                        .Select(item => item.ReserveCode)
                        .FirstOrDefaultAsync(),
                nameof(PaymentCallbackTypeEnum.CoachReserve) when long.TryParse(payment.CallBackId, out var coachReserveId) =>
                    await _context.CoachReserves.AsNoTracking()
                        .Where(item => item.Id == coachReserveId)
                        .Select(item => item.ReserveCode)
                        .FirstOrDefaultAsync(),
                nameof(PaymentCallbackTypeEnum.SchoolReserve) when long.TryParse(payment.CallBackId, out var schoolReserveId) =>
                    await _context.SchoolReserves.AsNoTracking()
                        .Where(item => item.Id == schoolReserveId)
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

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.ConsultationPurchase.ToString() &&
                long.TryParse(payment.CallBackId, out var consultationPurchaseId))
            {
                await _consultationPurchaseActivator.MarkPaymentFailedAsync(consultationPurchaseId, payment.Id);
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
            var reserveBlocked = await GuardRetryAsync(
                reservedetail.IsCancel, reservedetail.IsReserved, Resource.Notification.PaymentReserveAlreadyPaid,
                PaymentCallbackTypeEnum.CompanionReserve, reservedetail.Id.ToString());
            // عضو یک سبد: پرداخت سبدیِ رهاشده هم کنار گذاشته می‌شود تا دو پرداخت هم‌زمان برای یک رزرو باز نماند
            if (reserveBlocked == null && reservedetail.BatchId.HasValue)
                reserveBlocked = await PaymentAttemptRules.GuardUserRetryAsync(
                    _context, PaymentCallbackTypeEnum.CompanionReserveBatch.ToString(), reservedetail.BatchId.Value.ToString(),
                    Resource.Notification.PaymentReserveAlreadyPaid);
            if (reserveBlocked != null)
                return reserveBlocked;
            ScopePaymentTarget(dto, PaymentCallbackTypeEnum.CompanionReserve, reservedetail.Id);
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
            var reservedetail = await _context.PansionReserves.Include(s => s.Rebate).Include(s => s.Pansion).AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.PansionReserveId);
            if (reservedetail == null || reservedetail.BookerId != dto.UserId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var pansionBlocked = await GuardRetryAsync(
                reservedetail.IsCancel, reservedetail.IsReserved, Resource.Notification.PaymentReserveAlreadyPaid,
                PaymentCallbackTypeEnum.PansionReserve, reservedetail.Id.ToString());
            if (pansionBlocked != null)
                return pansionBlocked;
            var reserveDay = reservedetail.FromDate ?? reservedetail.SchoolCreateDate;
            if (reserveDay.HasValue && reserveDay.Value.Date < DateTime.Today)
                return new BaseResultDto(false, Resource.Notification.PansionReserveDatePassed);
            switch (Application.Services.PansionSrvs.PansionReserveSrv.PansionReserveRules.Reprice(reservedetail, reservedetail.Pansion))
            {
                case Application.Services.PansionSrvs.PansionReserveSrv.PansionReserveRules.RepriceResult.Invalid:
                    return new BaseResultDto(false, Resource.Notification.PansionCheckoutMustBeAfterCheckin);
                case Application.Services.PansionSrvs.PansionReserveSrv.PansionReserveRules.RepriceResult.Changed:
                    await _context.SaveChangesAsync();
                    break;
            }
            ScopePaymentTarget(dto, PaymentCallbackTypeEnum.PansionReserve, null);
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

        public async Task<BaseResultDto> InsertGroomerReservePaymentAsyncDto(PaymentStartDto dto)
        {
            var reservedetail = await _context.GroomerReserves.Include(s => s.Rebate).Include(s => s.Groomer).AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.GroomerReserveId);
            if (reservedetail == null || reservedetail.BookerId != dto.UserId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var groomerBlocked = await GuardRetryAsync(
                reservedetail.IsCancel, reservedetail.IsReserved, Resource.Notification.PaymentReserveAlreadyPaid,
                PaymentCallbackTypeEnum.GroomerReserve, reservedetail.Id.ToString());
            if (groomerBlocked != null)
                return groomerBlocked;
            if (reservedetail.DoDate <= DateTime.Now)
                return new BaseResultDto(false, Resource.Notification.GroomerReserveDatePassed);
            if (reservedetail.Groomer == null || !reservedetail.Groomer.Active || !reservedetail.Groomer.Approved || reservedetail.Groomer.Deleted)
                return new BaseResultDto(false, Resource.Notification.GroomerReserveGroomerUnavailable);
            ScopePaymentTarget(dto, PaymentCallbackTypeEnum.GroomerReserve, null);
            var groomerRebateValidation = ValidateAppliedRebate(
                reservedetail.Rebate,
                reservedetail.PaymentPrice + reservedetail.RebatePrice,
                reservedetail.BookerId,
                RebateTypeLabels.GroomerReserve,
                reservedetail.RebatePrice);
            if (!groomerRebateValidation.IsSuccess)
                return groomerRebateValidation;

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
            var groomerTypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_GroomerReserve.ToString());
            if (reservedetail.FromWallet)
            {
                var walletAmount = await _walletService.GetSpendableAmountValueAsync(
                    reservedetail.BookerId,
                    Entities.Entities.PastilClubField.ClubRewardTargetTypeEnum.Groomer,
                    reservedetail.GroomerId);
                reservedetail.WalletPrice = PaymentAmountHelper.GetWalletContribution(walletAmount, reservedetail.PaymentPrice);
                dto.WalletAmount = reservedetail.WalletPrice;
                await _context.SaveChangesAsync();

                dto.Amount = walletAmount >= reservedetail.PaymentPrice ? 0 : reservedetail.PaymentPrice - reservedetail.WalletPrice;
                dto.ProductOrderId = null;
                dto.CallBackTypeLabel = PaymentCallbackTypeEnum.GroomerReserve.ToString();
                dto.CallBackId = reservedetail.Id.ToString();
                dto.TypeId = groomerTypeId;
                return await StartPayment(dto);
            }

            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = groomerTypeId;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.GroomerReserve.ToString();
            dto.CallBackId = reservedetail.Id.ToString();
            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> InsertClinicReservePaymentAsyncDto(PaymentStartDto dto)
        {
            var reservedetail = await _context.ClinicReserves.Include(s => s.Rebate).Include(s => s.Clinic).AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.ClinicReserveId);
            if (reservedetail == null || reservedetail.BookerId != dto.UserId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var clinicBlocked = await GuardRetryAsync(
                reservedetail.IsCancel, reservedetail.IsReserved, Resource.Notification.PaymentReserveAlreadyPaid,
                PaymentCallbackTypeEnum.ClinicReserve, reservedetail.Id.ToString());
            if (clinicBlocked != null)
                return clinicBlocked;
            if (reservedetail.DoDate <= DateTime.Now)
                return new BaseResultDto(false, Resource.Notification.ClinicReserveDatePassed);
            if (reservedetail.Clinic == null || !reservedetail.Clinic.Active || !reservedetail.Clinic.Approved || reservedetail.Clinic.Deleted)
                return new BaseResultDto(false, Resource.Notification.ClinicReserveClinicUnavailable);
            ScopePaymentTarget(dto, PaymentCallbackTypeEnum.ClinicReserve, null);
            var clinicRebateValidation = ValidateAppliedRebate(
                reservedetail.Rebate,
                reservedetail.PaymentPrice + reservedetail.RebatePrice,
                reservedetail.BookerId,
                RebateTypeLabels.ClinicReserve,
                reservedetail.RebatePrice);
            if (!clinicRebateValidation.IsSuccess)
                return clinicRebateValidation;

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
            var clinicTypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_ClinicReserve.ToString());
            if (reservedetail.FromWallet)
            {
                var walletAmount = await _walletService.GetSpendableAmountValueAsync(
                    reservedetail.BookerId,
                    Entities.Entities.PastilClubField.ClubRewardTargetTypeEnum.Clinic,
                    reservedetail.ClinicId);
                reservedetail.WalletPrice = PaymentAmountHelper.GetWalletContribution(walletAmount, reservedetail.PaymentPrice);
                dto.WalletAmount = reservedetail.WalletPrice;
                await _context.SaveChangesAsync();

                dto.Amount = walletAmount >= reservedetail.PaymentPrice ? 0 : reservedetail.PaymentPrice - reservedetail.WalletPrice;
                dto.ProductOrderId = null;
                dto.CallBackTypeLabel = PaymentCallbackTypeEnum.ClinicReserve.ToString();
                dto.CallBackId = reservedetail.Id.ToString();
                dto.TypeId = clinicTypeId;
                return await StartPayment(dto);
            }

            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = clinicTypeId;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.ClinicReserve.ToString();
            dto.CallBackId = reservedetail.Id.ToString();
            return await StartPayment(dto);
        }
        public async Task<BaseResultDto> InsertCoachReservePaymentAsyncDto(PaymentStartDto dto)
        {
            var reservedetail = await _context.CoachReserves.Include(s => s.Rebate).Include(s => s.Coach).AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.CoachReserveId);
            if (reservedetail == null || reservedetail.BookerId != dto.UserId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var coachBlocked = await GuardRetryAsync(
                reservedetail.IsCancel, reservedetail.IsReserved, Resource.Notification.PaymentReserveAlreadyPaid,
                PaymentCallbackTypeEnum.CoachReserve, reservedetail.Id.ToString());
            if (coachBlocked != null)
                return coachBlocked;
            if (reservedetail.DoDate <= DateTime.Now)
                return new BaseResultDto(false, Resource.Notification.CoachReserveDatePassed);
            if (reservedetail.Coach == null || !reservedetail.Coach.Active || !reservedetail.Coach.Approved || reservedetail.Coach.Deleted)
                return new BaseResultDto(false, Resource.Notification.CoachReserveCoachUnavailable);
            ScopePaymentTarget(dto, PaymentCallbackTypeEnum.CoachReserve, null);
            var coachRebateValidation = ValidateAppliedRebate(
                reservedetail.Rebate,
                reservedetail.PaymentPrice + reservedetail.RebatePrice,
                reservedetail.BookerId,
                RebateTypeLabels.CoachReserve,
                reservedetail.RebatePrice);
            if (!coachRebateValidation.IsSuccess)
                return coachRebateValidation;

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
            var coachTypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_CoachReserve.ToString());
            if (reservedetail.FromWallet)
            {
                var walletAmount = await _walletService.GetSpendableAmountValueAsync(
                    reservedetail.BookerId,
                    Entities.Entities.PastilClubField.ClubRewardTargetTypeEnum.Coach,
                    reservedetail.CoachId);
                reservedetail.WalletPrice = PaymentAmountHelper.GetWalletContribution(walletAmount, reservedetail.PaymentPrice);
                dto.WalletAmount = reservedetail.WalletPrice;
                await _context.SaveChangesAsync();

                dto.Amount = walletAmount >= reservedetail.PaymentPrice ? 0 : reservedetail.PaymentPrice - reservedetail.WalletPrice;
                dto.ProductOrderId = null;
                dto.CallBackTypeLabel = PaymentCallbackTypeEnum.CoachReserve.ToString();
                dto.CallBackId = reservedetail.Id.ToString();
                dto.TypeId = coachTypeId;
                return await StartPayment(dto);
            }

            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = coachTypeId;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.CoachReserve.ToString();
            dto.CallBackId = reservedetail.Id.ToString();
            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> InsertSchoolReservePaymentAsyncDto(PaymentStartDto dto)
        {
            var reservedetail = await _context.SchoolReserves.Include(s => s.Rebate).AsTracking().FirstOrDefaultAsync(s => s.Id == dto.SchoolReserveId);
            if (reservedetail == null || reservedetail.BookerId != dto.UserId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var schoolBlocked = await GuardRetryAsync(
                reservedetail.IsCancel, reservedetail.IsReserved, Resource.Notification.PaymentReserveAlreadyPaid,
                PaymentCallbackTypeEnum.SchoolReserve, reservedetail.Id.ToString());
            if (schoolBlocked != null)
                return schoolBlocked;
            // ثبت‌نامِ پرداخت‌نشده‌ای که جلسه‌ی اول دوره‌اش شروع شده دیگر پرداخت نمی‌شود
            if (!await Application.Services.SchoolSrvs.SchoolCourseSrv.SchoolCourseRules.IsRegistrationOpenAsync(_context, reservedetail.SchoolCourseId, DateTime.Now))
                return new BaseResultDto(false, Resource.Notification.SchoolCourseRegistrationClosed);
            ScopePaymentTarget(dto, PaymentCallbackTypeEnum.SchoolReserve, null);
            var schoolRebateValidation = ValidateAppliedRebate(
                reservedetail.Rebate,
                reservedetail.PaymentPrice + reservedetail.RebatePrice,
                reservedetail.BookerId,
                RebateTypeLabels.SchoolReserve,
                reservedetail.RebatePrice);
            if (!schoolRebateValidation.IsSuccess)
                return schoolRebateValidation;

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
                    Entities.Entities.PastilClubField.ClubRewardTargetTypeEnum.School,
                    reservedetail.SchoolCourseId);
                reservedetail.WalletPrice = PaymentAmountHelper.GetWalletContribution(walletAmount, reservedetail.PaymentPrice);
                dto.WalletAmount = reservedetail.WalletPrice;
                await _context.SaveChangesAsync();

                if (walletAmount >= reservedetail.PaymentPrice)
                {
                    dto.Amount = 0;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.SchoolReserve.ToString();
                    dto.CallBackId = reservedetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_SchoolReserve.ToString());
                    return await StartPayment(dto);
                }
                else
                {
                    dto.Amount = reservedetail.PaymentPrice - reservedetail.WalletPrice;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.SchoolReserve.ToString();
                    dto.CallBackId = reservedetail.Id.ToString();
                    dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_SchoolReserve.ToString());
                    return await StartPayment(dto);
                }
            }

            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_SchoolReserve.ToString());
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.SchoolReserve.ToString();
            dto.CallBackId = reservedetail.Id.ToString();
            return await StartPayment(dto);
        }

        // پرداخت «سبد رزرو»: یک پرداخت واحد برای مجموع چند رزرو مستقل (هر کدام برای یک خدمت متفاوت اما
        // همگی متعلق به یک نمایندگی) که با هم در یک سبد ثبت شده‌اند. مبلغ کیف پول یک‌بار و روی مجموع کل
        // سبد محاسبه می‌شود (نه جداگانه برای هر عضو) تا از over-commitment جمع کیف‌پول جلوگیری شود، سپس
        // بین اعضا (بر اساس Id صعودی) توزیع می‌شود. نسخه اول: بدون پشتیبانی از کد تخفیف مشترک روی سبد.
        public async Task<BaseResultDto> InsertCompanionReserveBatchPaymentAsyncDto(PaymentStartDto dto)
        {
            var members = await _context.CompanionReserves
                .Include(s => s.CompanionAssistance)
                .Where(s => s.BatchId == dto.CompanionReserveBatchId)
                .OrderBy(s => s.Id)
                .AsTracking()
                .ToListAsync();

            if (!members.Any() || members.Any(s => s.BookerId != dto.UserId))
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            // اعضای سبد با یک پرداخت واحد پرداخت می‌شوند؛ پس پرداخت‌شدنِ هر عضو یعنی پرداخت‌شدنِ سبد
            var batchBlocked = await GuardRetryAsync(
                members.Any(s => s.IsCancel), members.Any(s => s.IsReserved), Resource.Notification.PaymentReserveAlreadyPaid,
                PaymentCallbackTypeEnum.CompanionReserveBatch, dto.CompanionReserveBatchId.ToString());
            // پرداخت تکیِ رهاشده‌ی هر عضو هم کنار گذاشته می‌شود
            foreach (var member in members)
            {
                if (batchBlocked != null)
                    break;
                batchBlocked = await PaymentAttemptRules.GuardUserRetryAsync(
                    _context, PaymentCallbackTypeEnum.CompanionReserve.ToString(), member.Id.ToString(),
                    Resource.Notification.PaymentReserveAlreadyPaid);
            }
            if (batchBlocked != null)
                return batchBlocked;
            ScopePaymentTarget(dto, PaymentCallbackTypeEnum.CompanionReserveBatch, null);

            var totalPrePayment = members.Sum(s => s.PrePaymentPrice);
            var totalRebate = members.Sum(s => s.RebatePrice);

            dto.Amount = totalPrePayment;
            dto.GrossAmount = totalPrePayment + totalRebate;
            dto.RebateAmount = totalRebate;
            dto.RebateId = null;
            dto.WalletAmount = 0;

            if (dto.Amount < 0)
            {
                return new BaseResultDto(false, Resource.Notification.AmountNotCorrect);
            }

            var useWallet = members.Any(s => s.FromWallet);
            if (dto.Amount > 0 && dto.MerchantId == null && !useWallet)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);
            }

            if (useWallet)
            {
                var companionId = members.First().CompanionAssistance.CompanionId;
                var walletAmount = await _walletService.GetSpendableAmountValueAsync(
                    dto.UserId.Value,
                    Entities.Entities.PastilClubField.ClubRewardTargetTypeEnum.Companion,
                    companionId);
                var batchWalletContribution = PaymentAmountHelper.GetWalletContribution(walletAmount, totalPrePayment);

                var remaining = batchWalletContribution;
                foreach (var member in members)
                {
                    var contribution = remaining > 0 ? Math.Min(remaining, member.PrePaymentPrice) : 0;
                    member.FromWallet = contribution > 0;
                    member.WalletPrice = contribution;
                    remaining -= contribution;
                }
                await _context.SaveChangesAsync();

                dto.WalletAmount = batchWalletContribution;
                dto.Amount = totalPrePayment - batchWalletContribution;
            }

            dto.ProductOrderId = null;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.CompanionReserveBatch.ToString();
            dto.CallBackId = dto.CompanionReserveBatchId.ToString();
            dto.TypeId = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_CompanionReserveBatch.ToString());

            if (dto.Amount <= 0)
            {
                return await StartPayment(dto);
            }

            dto.IsOnline = true;
            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> InsertTripPaymentAsyncDto(PaymentStartDto dto)
        {
            var tripdetail = await _context.Trips.Include(s => s.Rebate).FirstOrDefaultAsync(s => s.Id == dto.TripId);
            if (tripdetail == null || tripdetail.UserId != dto.UserId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var tripBlocked = await GuardRetryAsync(
                false, tripdetail.IsPaid, Resource.Notification.PaymentTripAlreadyPaid,
                PaymentCallbackTypeEnum.Trip, tripdetail.Id.ToString());
            if (tripBlocked != null)
                return tripBlocked;
            ScopePaymentTarget(dto, PaymentCallbackTypeEnum.Trip, tripdetail.Id);
            var tripRebateValidation = ValidateAppliedRebate(
                tripdetail.Rebate, tripdetail.Price, tripdetail.UserId, RebateTypeLabels.Trip, tripdetail.RebatePrice);
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
                var walletAmount = await _walletService.GetAmountValueAsync(tripdetail.UserId);
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
            var cargoBlocked = await GuardRetryAsync(
                false, cargodetail.IsPaid, Resource.Notification.PaymentCargoRequestAlreadyPaid,
                PaymentCallbackTypeEnum.Cargo, cargodetail.Id.ToString());
            if (cargoBlocked != null)
                return cargoBlocked;
            ScopePaymentTarget(dto, PaymentCallbackTypeEnum.Cargo, cargodetail.Id);
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
            var insuranceBlocked = await GuardRetryAsync(
                false, insuranceDetail.IsPaid, Resource.Notification.PaymentInsuranceAlreadyPaid,
                PaymentCallbackTypeEnum.Insurance, insuranceDetail.Id.ToString());
            if (insuranceBlocked != null)
                return insuranceBlocked;
            ScopePaymentTarget(dto, PaymentCallbackTypeEnum.Insurance, insuranceDetail.Id);
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

        // ستون‌های هدفِ Payment (CompanionReserveId/TripId/CargoId/...) فقط از هدفِ واقعیِ بررسی‌شده پر می‌شوند؛ مقداری که کلاینت
        // در بدنه‌ی PaymentStartDto فرستاده دور ریخته می‌شود. (Pansion/School/Batch روی Payment ستون جدا ندارند.)
        private static void ScopePaymentTarget(PaymentStartDto dto, PaymentCallbackTypeEnum target, long? targetId)
        {
            dto.ProductOrderId = null;
            dto.CompanionReserveId = target == PaymentCallbackTypeEnum.CompanionReserve ? targetId : null;
            dto.TripId = target == PaymentCallbackTypeEnum.Trip ? targetId : null;
            dto.CargoId = target == PaymentCallbackTypeEnum.Cargo ? targetId : null;
            dto.CompanionInsurancePackageSaleId = target == PaymentCallbackTypeEnum.Insurance ? targetId : null;
        }

        // ورودی مشترک پرداخت هر هدف: لغوشده ⇒ پیام لغو؛ پرداخت‌شده ⇒ «قبلاً پرداخت شده» (کد ۲۰)؛ وگرنه تلاش‌های رهاشده‌ی قبلی
        // همین‌جا (قبل از بازمحاسبه‌ی سهم کیف پول روی خود هدف) کنار گذاشته می‌شوند. null یعنی پرداخت تازه می‌تواند شروع شود.
        private async Task<BaseResultDto> GuardRetryAsync(
            bool isCancelled, bool isPaid, string alreadyPaidMessage, PaymentCallbackTypeEnum target, string callBackId)
        {
            if (isCancelled)
                return new BaseResultDto(false, Resource.Notification.PaymentTargetCancelled);
            var label = target.ToString();
            if (isPaid)
                return PaymentAttemptRules.AlreadyPaid(alreadyPaidMessage, label, callBackId);
            return await PaymentAttemptRules.GuardUserRetryAsync(_context, label, callBackId, alreadyPaidMessage);
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

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.GroomerReserve.ToString())
            {
                var item = await _context.GroomerReserves.AsNoTracking()
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

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.ClinicReserve.ToString())
            {
                var item = await _context.ClinicReserves.AsNoTracking()
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
            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.CoachReserve.ToString())
            {
                var item = await _context.CoachReserves.AsNoTracking()
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

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.CompanionReserveBatch.ToString())
            {
                var members = await _context.CompanionReserves.AsNoTracking()
                    .Where(s => s.BatchId == referenceId && s.BookerId == payment.UserId && !s.IsCancel)
                    .Select(s => new { s.PrePaymentPrice, s.RebatePrice, s.WalletPrice })
                    .ToListAsync();
                if (!members.Any())
                    return new BaseResultDto(false, Resource.Notification.InvalidData);
                var totalPrePayment = members.Sum(s => s.PrePaymentPrice);
                var totalRebate = members.Sum(s => s.RebatePrice);
                var totalWallet = members.Sum(s => s.WalletPrice);
                return SnapshotMatches(
                    payment,
                    totalPrePayment + totalRebate,
                    totalRebate,
                    totalWallet,
                    totalPrePayment - totalWallet)
                    ? new BaseResultDto(true)
                    : new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.SchoolReserve.ToString())
            {
                var item = await _context.SchoolReserves.AsNoTracking()
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

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.ConsultationPurchase.ToString())
            {
                var item = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.Id == referenceId && s.UserId == payment.UserId)
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

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.GroomerReserve.ToString())
            {
                return await _groomerReserve.GroomerReservePaymentCallback(referenceId, useWallet);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.ClinicReserve.ToString())
            {
                return await _clinicReserve.ClinicReservePaymentCallback(referenceId, useWallet);
            }
            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.CoachReserve.ToString())
            {
                return await _coachReserve.CoachReservePaymentCallback(referenceId, useWallet);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.CompanionReserveBatch.ToString())
            {
                return await _companionReserveService.CompanionReserveBatchPaymentCallback(referenceId, useWallet);
            }

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.SchoolReserve.ToString())
            {
                return await _schoolReserve.SchoolReservePaymentCallback(referenceId, useWallet);
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

            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.ConsultationPurchase.ToString())
            {
                return await _consultationPurchaseActivator.ActivateAfterPaymentAsync(referenceId, payment.Id);
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
                    return ManualPaymentTargetResult.Fail(Resource.Notification.PaymentOrderAlreadyPaid);
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
                    return ManualPaymentTargetResult.Fail(Resource.Notification.PaymentReserveAlreadyPaid);
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
                    return ManualPaymentTargetResult.Fail(Resource.Notification.PaymentReserveAlreadyPaid);
                }
                return ManualPaymentTargetResult.Success(item.BookerId, item.PaymentPrice, dto.ReferenceId);
            }

            if (dto.TargetType == PaymentCallbackTypeEnum.GroomerReserve)
            {
                var item = await _context.GroomerReserves.AsNoTracking()
                    .Where(s => s.Id == referenceId)
                    .Select(s => new { s.BookerId, s.PaymentPrice, s.IsReserved, s.IsCancel })
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.NothingFound);
                }
                if (item.IsReserved)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.PaymentReserveAlreadyPaid);
                }
                if (item.IsCancel)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.GroomerReserveCannotCancel);
                }
                return ManualPaymentTargetResult.Success(item.BookerId, item.PaymentPrice, dto.ReferenceId);
            }

            if (dto.TargetType == PaymentCallbackTypeEnum.ClinicReserve)
            {
                var item = await _context.ClinicReserves.AsNoTracking()
                    .Where(s => s.Id == referenceId)
                    .Select(s => new { s.BookerId, s.PaymentPrice, s.IsReserved, s.IsCancel })
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.NothingFound);
                }
                if (item.IsReserved)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.PaymentReserveAlreadyPaid);
                }
                if (item.IsCancel)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.ClinicReserveCannotCancel);
                }
                return ManualPaymentTargetResult.Success(item.BookerId, item.PaymentPrice, dto.ReferenceId);
            }
            if (dto.TargetType == PaymentCallbackTypeEnum.CoachReserve)
            {
                var item = await _context.CoachReserves.AsNoTracking()
                    .Where(s => s.Id == referenceId)
                    .Select(s => new { s.BookerId, s.PaymentPrice, s.IsReserved, s.IsCancel })
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.NothingFound);
                }
                if (item.IsReserved)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.PaymentReserveAlreadyPaid);
                }
                if (item.IsCancel)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.CoachReserveCannotCancel);
                }
                return ManualPaymentTargetResult.Success(item.BookerId, item.PaymentPrice, dto.ReferenceId);
            }

            if (dto.TargetType == PaymentCallbackTypeEnum.SchoolReserve)
            {
                var item = await _context.SchoolReserves.AsNoTracking()
                    .Where(s => s.Id == referenceId)
                    .Select(s => new { s.BookerId, s.PaymentPrice, s.IsReserved })
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.NothingFound);
                }
                if (item.IsReserved)
                {
                    return ManualPaymentTargetResult.Fail(Resource.Notification.PaymentReserveAlreadyPaid);
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
                    return ManualPaymentTargetResult.Fail(Resource.Notification.PaymentTripAlreadyPaid);
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
                    return ManualPaymentTargetResult.Fail(Resource.Notification.PaymentCargoRequestAlreadyPaid);
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
                    return ManualPaymentTargetResult.Fail(Resource.Notification.PaymentInsuranceAlreadyPaid);
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
                PaymentCallbackTypeEnum.SchoolReserve => PaymentTypeEnum.PaymentType_SchoolReserve.ToString(),
                PaymentCallbackTypeEnum.GroomerReserve => PaymentTypeEnum.PaymentType_GroomerReserve.ToString(),
                PaymentCallbackTypeEnum.ClinicReserve => PaymentTypeEnum.PaymentType_ClinicReserve.ToString(),
                PaymentCallbackTypeEnum.CoachReserve => PaymentTypeEnum.PaymentType_CoachReserve.ToString(),
                PaymentCallbackTypeEnum.Trip => PaymentTypeEnum.PaymentType_Trip.ToString(),
                PaymentCallbackTypeEnum.Cargo => PaymentTypeEnum.PaymentType_Cargo.ToString(),
                PaymentCallbackTypeEnum.Insurance => PaymentTypeEnum.PaymentType_Insurance.ToString(),
                PaymentCallbackTypeEnum.PastilAI => PaymentTypeEnum.PaymentType_PastilAI.ToString(),
                PaymentCallbackTypeEnum.ConsultationPurchase => PaymentTypeEnum.PaymentType_ConsultationPurchase.ToString(),
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
                error = Resource.Notification.PaymentIdempotencyKeyHeaderMustBeValidUuid;
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
                return new BaseResultDto<PaymentStartDto>(false, Resource.Notification.PaymentPreviousCheckoutAttemptFailed, dto);

            if (payment.IsSuccess == null && string.IsNullOrWhiteSpace(payment.PaymentUrl))
                return new BaseResultDto<PaymentStartDto>(false, Resource.Notification.PaymentRequestBeingProcessedResubmitSame, dto);

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
