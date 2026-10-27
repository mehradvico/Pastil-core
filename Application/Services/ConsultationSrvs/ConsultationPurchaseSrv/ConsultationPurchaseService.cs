using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.ConsultationSrvs.ConsultationNotificationSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationOwnerSrv;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Iface;
using Application.Services.Order.PaymentSrv;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationPurchaseSrv
{
    // خرید پکیج مشاوره آنلاین (کاربر): ساخت خرید، شروع پرداخت (درگاه/کیف پول/تخفیف)، لغو و بازپرداخت.
    // الگو: PastilAiPlanService.PurchaseAsync. طراحی: backend/Docs/ONLINE_CONSULTATION_PACKAGES_FA.md
    public class ConsultationPurchaseService : IConsultationPurchaseService
    {
        private const string CancelReasonUser = "UserCancelled";
        private const string CancelReasonAdmin = "AdminCancelled";
        private const string CancelReasonPaymentFailed = "PaymentFailed";
        private const string CancelReasonStartDeadline = "StartDeadlineExpired";
        private const string CancelReasonSuperseded = "Superseded";
        private const int ExpireBatchSize = 100;

        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly IRebateService _rebateService;
        private readonly IPaymentService _paymentService;
        private readonly Application.Services.CompanionSrvs.ConsultationOnlineSrv.IConsultationOnlineService _onlineService;
        private readonly IConsultationNotificationService _notifications;
        private readonly Application.Services.ConsultationSrvs.ConsultationBookingSrv.Iface.IConsultationBookingService _booking;

        public ConsultationPurchaseService(
            IDataBaseContext context,
            IWalletService walletService,
            IRebateService rebateService,
            IPaymentService paymentService,
            IConsultationNotificationService notifications,
            Application.Services.CompanionSrvs.ConsultationOnlineSrv.IConsultationOnlineService onlineService,
            Application.Services.ConsultationSrvs.ConsultationBookingSrv.Iface.IConsultationBookingService booking)
        {
            _notifications = notifications;
            _context = context;
            _walletService = walletService;
            _rebateService = rebateService;
            _paymentService = paymentService;
            _onlineService = onlineService;
            _booking = booking;
        }

        public async Task<BaseResultDto> PurchaseAsync(long userId, ConsultationPurchaseCreateDto dto)
        {
            try
            {
                if (dto == null || dto.PackageId <= 0)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                if (await Application.Services.CompanionSrvs.CompanionReserveDebtSrv.CompanionReserveDebtRules.IsLockedAsync(_context, userId, DateTime.Now))
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtLocked);

                var package = await _context.ConsultationPackages.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == dto.PackageId && !s.Deleted && s.Active && s.Approved && s.Price > 0);
                if (package == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);

                var packageOwner = ConsultationOwner.Of(package);
                var clinicOpen = packageOwner.HasValue && await ConsultationOwners.IsOpenAsync(_context, packageOwner.Value);
                if (!clinicOpen)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);

                // پکیج «قابل رزرو»: ساعت الزامی است و آنلاین‌بودن نماینده «همین الان» شرط نیست (مشاوره در ساعت رزرو انجام می‌شود)
                var bookable = package.Bookable;
                DateTime? scheduledStart = null;
                if (bookable)
                {
                    if (!dto.ScheduledStart.HasValue)
                        return new BaseResultDto(false, Resource.Notification.ConsultationBookingTimeRequired);
                    // ساعت دیواری سرور (ایران). اگر کلاینت با Z/UTC فرستاده باشد به ساعت محلی سرور تبدیل می‌شود؛ بدون آفست همان ساعت دیواری است.
                    var requested = dto.ScheduledStart.Value;
                    scheduledStart = DateTime.SpecifyKind(requested.Kind == DateTimeKind.Utc ? requested.ToLocalTime() : requested, DateTimeKind.Unspecified);
                }
                else
                {
                    // الان هیچ نماینده/مربی‌ای آنلاین نیست: نمی‌توان پکیج آنلاین این کلینیک/مدرسه را خرید
                    // (اجباری سمت سرور؛ فرانت هم باید صفحه‌ی پکیج را وقتی آفلاین است پیش‌تر مسدود کند)
                    var companionOnline = await _onlineService.GetPublicAsync(packageOwner.Value);
                    if (!companionOnline.Online)
                        return new BaseResultDto(false, Resource.Notification.ConsultationCompanionOffline);
                }

                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(s => s.Id == userId && !s.Deleted && !s.Locked);
                if (user == null)
                    return new BaseResultDto(false, Resource.Notification.UserNotFound);

                // مشاوره برای پت(های) خود کاربر خریده می‌شود؛ نماینده پروفایل و سابقه‌ی همین پت‌ها را می‌بیند
                var petIds = ConsultationPurchaseRules.NormalizePetIds(dto.UserPetIds);
                if (petIds == null)
                    return new BaseResultDto(false, Resource.Notification.ConsultationPetRequired);
                var ownedPetCount = await _context.UserPets.AsNoTracking()
                    .CountAsync(p => petIds.Contains(p.Id) && p.UserId == userId && !p.Deleted);
                if (ownedPetCount != petIds.Count)
                    return new BaseResultDto(false, Resource.Notification.ConsultationPetInvalid);

                var paid = (int)ConsultationPurchaseStatusEnum.Paid;
                var active = (int)ConsultationPurchaseStatusEnum.Active;
                var purchaseLabel = PaymentCallbackTypeEnum.ConsultationPurchase.ToString();
                // هر مورد «قبلاً خریداری‌شده» با کد ۲۰ و شناسه‌ی همان خرید برمی‌گردد تا فرانت به‌جای خطا جزئیاتش را نشان دهد
                if (bookable)
                {
                    // همین پکیج و همین ساعت قبلاً پرداخت شده (مثلاً کاربر بعد از پرداخت موفق دوباره «خرید» را زد)
                    var samePurchaseId = await _context.ConsultationPurchases.AsNoTracking()
                        .Where(s => s.UserId == userId && s.ConsultationPackageId == package.Id &&
                                    s.ScheduledStart == scheduledStart && (s.Status == paid || s.Status == active))
                        .Select(s => (long?)s.Id)
                        .FirstOrDefaultAsync();
                    if (samePurchaseId.HasValue)
                        return PaymentAttemptRules.AlreadyPaid(Resource.Notification.ConsultationAlreadyPurchased, purchaseLabel, samePurchaseId.Value.ToString());
                }
                else
                {
                    // خرید تکراری اشتباهی: همان کلینیک و همان کانال، در حالی که یکی پرداخت‌شده/در جریان دارد
                    // (رزروهای ساعت‌دار آینده مانع خرید فوری نیستند؛ فقط خرید فوریِ باز حساب می‌شود)
                    var sameChannelId = await _context.ConsultationPurchases.AsNoTracking()
                        .ForOwner(packageOwner.Value).Where(s => s.UserId == userId && s.ChannelId == package.ChannelId &&
                                    s.ScheduledStart == null && (s.Status == paid || s.Status == active))
                        .Select(s => (long?)s.Id)
                        .FirstOrDefaultAsync();
                    if (sameChannelId.HasValue)
                        return PaymentAttemptRules.AlreadyPaid(Resource.Notification.ConsultationAlreadyInProgress, purchaseLabel, sameChannelId.Value.ToString());

                    // تا وقتی زمانِ یک مشاوره‌ی فوریِ خریداری‌شده تمام نشده (منتظر شروع و در مهلت، یا در جریان و پنجره‌اش باز)،
                    // کاربر مشاوره‌ی فوریِ دیگری — با هر کلینیک یا روشی — نمی‌تواند بخرد.
                    var nowForBlock = DateTime.Now;
                    var liveConsultationId = await _context.ConsultationPurchases.AsNoTracking()
                        .Where(s => s.UserId == userId && s.ScheduledStart == null &&
                                    ((s.Status == paid && (s.StartDeadline == null || s.StartDeadline > nowForBlock)) ||
                                     (s.Status == active && (s.ExpireDate == null || s.ExpireDate > nowForBlock))))
                        .Select(s => (long?)s.Id)
                        .FirstOrDefaultAsync();
                    if (liveConsultationId.HasValue)
                        return PaymentAttemptRules.AlreadyPaid(Resource.Notification.ConsultationAlreadyInProgress, purchaseLabel, liveConsultationId.Value.ToString());
                }

                // خریدهای «در انتظار پرداخت» قبلی همین کاربر (برگشت از درگاه بدون پرداخت، نرسیدن به درگاه، قطع شبکه) دیگر قفل نمی‌کنند:
                // تلاش درگاهشان کنار گذاشته و خودشان لغو می‌شوند (ظرفیت ساعتی که نگه داشته بودند هم آزاد می‌شود) و خرید تازه ساخته می‌شود.
                // فقط اگر پرداخت یکی از آن‌ها واقعاً موفق شده یا callbackش همین الان در حال verify است، کاربر متوقف می‌شود.
                var pending = (int)ConsultationPurchaseStatusEnum.PendingPayment;
                var stalePendingIds = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.UserId == userId && s.Status == pending)
                    .Select(s => s.Id)
                    .ToListAsync();
                if (stalePendingIds.Count > 0)
                {
                    await PaymentAttemptRules.SupersedeUserPendingAsync(_context, userId, purchaseLabel);
                    var staleKeys = stalePendingIds.Select(id => id.ToString()).ToList();
                    var unfinished = await _context.Payments.AsNoTracking()
                        .Where(s => s.UserId == userId && s.CallBackTypeLabel == purchaseLabel && staleKeys.Contains(s.CallBackId) &&
                                    (s.IsSuccess == true || s.IsSuccess == null && s.AppliedDate == null))
                        .Select(s => new { s.CallBackId, s.IsSuccess })
                        .ToListAsync();
                    var paidAttempt = unfinished.FirstOrDefault(s => s.IsSuccess == true);
                    if (paidAttempt != null)
                        return PaymentAttemptRules.AlreadyPaid(Resource.Notification.ConsultationAlreadyPurchased, purchaseLabel, paidAttempt.CallBackId);
                    if (unfinished.Count > 0)
                        return new BaseResultDto(false, Resource.Notification.PaymentPreviousAttemptVerifying);

                    await _context.ConsultationPurchases
                        .Where(s => stalePendingIds.Contains(s.Id) && s.Status == pending)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Cancelled)
                            .SetProperty(s => s.CancelDate, (DateTime?)DateTime.Now)
                            .SetProperty(s => s.CancelReason, CancelReasonSuperseded));
                }

                double rebateDiscount = 0;
                long? rebateId = null;
                if (!string.IsNullOrWhiteSpace(dto.RebateCode))
                {
                    var rebateResult = _rebateService.GetRebateByCodeAsync(
                        package.Price, userId, RebateTypeLabels.ConsultationPurchase, dto.RebateCode, package.Id);
                    if (!rebateResult.IsSuccess)
                        return new BaseResultDto(false, messages: rebateResult.Messages);

                    rebateId = rebateResult.Data.Id;
                    rebateDiscount = rebateResult.Data.FinalPrice;
                }

                // فقط موجودی نقد (اعتبار تبلیغاتی باشگاه برای مشاوره تعریف نشده و در برداشت نهایی اعمال نمی‌شود)
                var walletBalance = dto.FromWallet ? await _walletService.GetAmountValueAsync(userId) : 0;
                var amounts = ConsultationPurchaseRules.ComputeAmounts(package.Price, rebateDiscount, dto.FromWallet, walletBalance);
                if (amounts.Gateway > 0 && (!dto.MerchantId.HasValue || dto.MerchantId.Value <= 0))
                    return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

                var typeId = await _context.Codes.AsNoTracking()
                    .Where(s => s.Label == PaymentTypeEnum.PaymentType_ConsultationPurchase.ToString() && s.Active)
                    .Select(s => (long?)s.Id)
                    .FirstOrDefaultAsync();
                if (!typeId.HasValue)
                    return new BaseResultDto(false, Resource.Notification.PaymentTypeNotConfiguredInSystem);

                // رزرو ساعت‌دار: بررسی ظرفیت اسلات و ساخت خرید در «یک» تراکنش Serializable تا دو کاربر هم‌زمان یک اسلات آخر را نگیرند.
                // تراکنش قبل از StartPayment commit می‌شود (ظرفیت با وضعیت PendingPayment تا BookingPendingHold نگه داشته می‌شود).
                await using var bookingTransaction = bookable && _context.CurrentTransaction == null
                    ? await _context.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                    : null;
                if (bookable)
                {
                    var now = DateTime.Now;
                    var end = scheduledStart.Value.AddMinutes(package.DurationMinutes);
                    var problem = await _booking.CheckSlotAsync(packageOwner.Value, package.DurationMinutes, scheduledStart.Value, now);
                    switch (problem)
                    {
                        case ConsultationBookingSrv.ConsultationBookingRules.SlotProblem.None:
                            break;
                        case ConsultationBookingSrv.ConsultationBookingRules.SlotProblem.TooSoon:
                            return new BaseResultDto(false, Resource.Notification.ConsultationSlotTooSoon);
                        case ConsultationBookingSrv.ConsultationBookingRules.SlotProblem.Full:
                            return new BaseResultDto(false, Resource.Notification.ConsultationSlotFull);
                        default:
                            return new BaseResultDto(false, Resource.Notification.ConsultationSlotInvalid);
                    }
                    if (await _booking.UserHasOverlapAsync(userId, scheduledStart.Value, end, now))
                        return new BaseResultDto(false, Resource.Notification.ConsultationBookingUserBusy);
                }

                var purchase = new ConsultationPurchase
                {
                    ScheduledStart = scheduledStart,
                    ScheduledEnd = scheduledStart?.AddMinutes(package.DurationMinutes),
                    UserId = userId,
                    ConsultationPackageId = package.Id,
                    CompanionId = package.CompanionId,
                    ClinicId = package.ClinicId,
                    CoachId = package.CoachId,
                    ChannelId = package.ChannelId,
                    DurationMinutes = package.DurationMinutes,
                    PackageName = package.Name,
                    Price = package.Price,
                    CommissionPercent = package.CommissionPercent,
                    Status = pending,
                    FromWallet = dto.FromWallet,
                    WalletPrice = amounts.Wallet,
                    PaymentPrice = amounts.Payable,
                    RebateId = rebateId,
                    RebatePrice = amounts.RebatePrice,
                    Discount = amounts.RebatePrice,
                    CreateDate = DateTime.Now,
                    Pets = petIds.Select(id => new ConsultationPurchasePet { UserPetId = id, CreateDate = DateTime.Now }).ToList()
                };
                if (!await TryInsertWithUniqueCodeAsync(purchase))
                    return new BaseResultDto(false, Resource.Notification.Unsuccess);
                if (bookingTransaction != null)
                    await bookingTransaction.CommitAsync();

                var paymentDto = new PaymentStartDto
                {
                    MerchantId = dto.MerchantId,
                    Amount = amounts.Gateway,
                    GrossAmount = amounts.Payable + amounts.RebatePrice,
                    RebateAmount = amounts.RebatePrice,
                    WalletAmount = amounts.Wallet,
                    RebateId = rebateId,
                    UserId = userId,
                    User = new Application.Services.Dto.UserMinVDto { Id = user.Id, Mobile = user.Mobile, Email = user.Email },
                    TypeId = typeId.Value,
                    CallBackTypeLabel = PaymentCallbackTypeEnum.ConsultationPurchase.ToString(),
                    CallBackId = purchase.Id.ToString()
                };

                var result = await _paymentService.StartPayment(paymentDto);
                if (!result.IsSuccess)
                {
                    await _context.ConsultationPurchases
                        .Where(s => s.Id == purchase.Id && s.Status == pending)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Cancelled)
                            .SetProperty(s => s.CancelDate, (DateTime?)DateTime.Now)
                            .SetProperty(s => s.CancelReason, CancelReasonPaymentFailed));
                    return result;
                }

                var callbackId = purchase.Id.ToString();
                var paymentId = await _context.Payments.AsNoTracking()
                    .Where(s => s.UserId == userId && s.CallBackTypeLabel == PaymentCallbackTypeEnum.ConsultationPurchase.ToString() && s.CallBackId == callbackId)
                    .OrderByDescending(s => s.Id)
                    .Select(s => (long?)s.Id)
                    .FirstOrDefaultAsync();
                if (paymentId.HasValue)
                {
                    // فقط اگر هنوز خرید بدون پرداخت است؛ اگر پرداخت با کیف پول همان لحظه تکمیل شده (Paid) دست نمی‌خورد
                    await _context.ConsultationPurchases
                        .Where(s => s.Id == purchase.Id && s.PaymentId == null)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.PaymentId, paymentId));
                }
                return result;
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto<List<ConsultationPurchaseVDto>>> GetMineAsync(long userId)
        {
            try
            {
                var now = DateTime.Now;
                var rows = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.Id)
                    .Take(50)
                    .Select(s => new { Purchase = s, CompanionName = s.Companion != null ? s.Companion.Name : s.Clinic != null ? s.Clinic.Name : s.Coach.Name })
                    .ToListAsync();
                var pets = await LoadPetsAsync(rows.Select(r => r.Purchase.Id).ToList());
                return new BaseResultDto<List<ConsultationPurchaseVDto>>(true, rows.Select(r => WithPets(ToVDto(r.Purchase, r.CompanionName, now), pets)).ToList());
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPurchaseVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationPurchaseVDto>> GetAsync(long userId, long id)
        {
            try
            {
                var row = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.Id == id && s.UserId == userId)
                    .Select(s => new { Purchase = s, CompanionName = s.Companion != null ? s.Companion.Name : s.Clinic != null ? s.Clinic.Name : s.Coach.Name })
                    .FirstOrDefaultAsync();
                if (row == null)
                    return new BaseResultDto<ConsultationPurchaseVDto>(false, Resource.Notification.NothingFound, null);
                var pets = await LoadPetsAsync(new List<long> { row.Purchase.Id });
                return new BaseResultDto<ConsultationPurchaseVDto>(true, WithPets(ToVDto(row.Purchase, row.CompanionName, DateTime.Now), pets));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationPurchaseVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto> CancelAsync(long userId, long id)
        {
            try
            {
                // رزرو ساعت‌دار: لغو آزاد فقط تا ۲ ساعت قبل از ساعت رزرو (خرید فوری: scheduledStart = null ⇒ محدودیتی نیست)
                var scheduledStart = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.Id == id && s.UserId == userId)
                    .Select(s => s.ScheduledStart)
                    .FirstOrDefaultAsync();
                if (!ConsultationBookingSrv.ConsultationBookingRules.IsUserCancelAllowed(scheduledStart, DateTime.Now))
                    return new BaseResultDto(false, Resource.Notification.ConsultationBookingCancelTooLate);

                await using var transaction = _context.CurrentTransaction == null
                    ? await _context.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                    : null;

                // گذار اتمی: فقط Paid → Cancelled؛ اگر همزمان نماینده «شروع» را زده باشد (Active) هیچ ردیفی تغییر نمی‌کند
                var affected = await _context.ConsultationPurchases
                    .Where(s => s.Id == id && s.UserId == userId && s.Status == (int)ConsultationPurchaseStatusEnum.Paid)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Cancelled)
                        .SetProperty(s => s.CancelDate, (DateTime?)DateTime.Now)
                        .SetProperty(s => s.CancelReason, CancelReasonUser));
                if (affected == 0)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                if (!await RefundAsync(id))
                    return new BaseResultDto(false, Resource.Notification.Unsuccess);

                if (transaction != null)
                    await transaction.CommitAsync();
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> AdminCancelAsync(long id, string reason)
        {
            try
            {
                await using var transaction = _context.CurrentTransaction == null
                    ? await _context.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                    : null;

                var detail = string.IsNullOrWhiteSpace(reason) ? CancelReasonAdmin : reason.Trim();
                if (detail.Length > 500)
                    detail = detail.Substring(0, 500);
                var paid = (int)ConsultationPurchaseStatusEnum.Paid;
                var active = (int)ConsultationPurchaseStatusEnum.Active;

                var purchase = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.Id == id && (s.Status == paid || s.Status == active))
                    .Select(s => new { s.Id, s.OnlineSessionId })
                    .FirstOrDefaultAsync();
                if (purchase == null)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                // گذار اتمی؛ اگر همزمان وضعیت عوض شده باشد هیچ ردیفی تغییر نمی‌کند
                var affected = await _context.ConsultationPurchases
                    .Where(s => s.Id == id && (s.Status == paid || s.Status == active))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Cancelled)
                        .SetProperty(s => s.CancelDate, (DateTime?)DateTime.Now)
                        .SetProperty(s => s.CancelReason, detail));
                if (affected == 0)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                if (purchase.OnlineSessionId.HasValue)
                {
                    await _context.OnlineSessions
                        .Where(s => s.Id == purchase.OnlineSessionId.Value && s.EndDate == null)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.EndDate, (DateTime?)DateTime.Now));
                }

                if (!await RefundAsync(id))
                    return new BaseResultDto(false, Resource.Notification.Unsuccess);

                if (transaction != null)
                    await transaction.CommitAsync();
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> AdminCompleteAsync(long id)
        {
            try
            {
                var active = (int)ConsultationPurchaseStatusEnum.Active;
                var purchase = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.Id == id && s.Status == active)
                    .Select(s => new { s.Id, s.OnlineSessionId })
                    .FirstOrDefaultAsync();
                if (purchase == null)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                var now = DateTime.Now;
                var affected = await _context.ConsultationPurchases
                    .Where(s => s.Id == id && s.Status == active)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Completed));
                if (affected == 0)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                if (purchase.OnlineSessionId.HasValue)
                {
                    await _context.OnlineSessions
                        .Where(s => s.Id == purchase.OnlineSessionId.Value && s.EndDate == null)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.EndDate, (DateTime?)now));
                }
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> ReviewAsync(long userId, long id, Dto.ConsultationPurchaseReviewDto dto)
        {
            try
            {
                if (dto == null || dto.Rate < 1 || dto.Rate > 5)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                // گذار اتمی: فقط برای مشاوره‌ی «تکمیل‌شده»‌ای که هنوز نظری برایش ثبت نشده - هر کاربر فقط یک‌بار نظر می‌دهد
                var affected = await _context.ConsultationPurchases
                    .Where(s => s.Id == id && s.UserId == userId &&
                                s.Status == (int)ConsultationPurchaseStatusEnum.Completed &&
                                s.ReviewedAt == null)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Rate, dto.Rate)
                        .SetProperty(s => s.ReviewText, (dto.Text ?? string.Empty).Trim())
                        .SetProperty(s => s.ReviewedAt, (DateTime?)DateTime.Now));

                if (affected == 0)
                    return new BaseResultDto(false, Resource.Notification.CompanionReserveCommentOnlyForOwnCompletedReserve);

                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<int> ExpireOverdueAsync()
        {
            var now = DateTime.Now;
            var paid = (int)ConsultationPurchaseStatusEnum.Paid;
            var ids = await _context.ConsultationPurchases.AsNoTracking()
                .Where(s => s.Status == paid && s.StartDeadline != null && s.StartDeadline < now)
                .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .Take(ExpireBatchSize)
                .ToListAsync();

            var expired = 0;
            foreach (var id in ids)
            {
                try
                {
                    await using var transaction = _context.CurrentTransaction == null
                        ? await _context.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                        : null;
                    var affected = await _context.ConsultationPurchases
                        .Where(s => s.Id == id && s.Status == paid)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Expired)
                            .SetProperty(s => s.CancelDate, (DateTime?)now)
                            .SetProperty(s => s.CancelReason, CancelReasonStartDeadline));
                    if (affected == 0)
                        continue;

                    if (!await RefundAsync(id))
                        continue; // rollback با dispose؛ در اجرای بعدی دوباره تلاش می‌شود

                    if (transaction != null)
                        await transaction.CommitAsync();
                    expired++;
                    try { await _notifications.NotifyExpiredRefundAsync(id); } catch { /* اعلان بازپرداخت نباید لغو را خراب کند */ }
                }
                catch
                {
                    // یک خرید خراب نباید بقیه را متوقف کند؛ اجرای بعدی job دوباره تلاش می‌کند
                }
            }
            return expired;
        }

        // بازپرداخت کامل مبلغ پرداخت‌شده به کیف پول؛ idempotent (نام ردیف دفتر + RefundDate). باید داخل تراکنش صدا زده شود.
        private async Task<bool> RefundAsync(long purchaseId)
        {
            var purchase = await _context.ConsultationPurchases.AsNoTracking().FirstOrDefaultAsync(s => s.Id == purchaseId);
            if (purchase == null)
                return false;
            if (purchase.RefundDate.HasValue)
                return true;

            var amount = ConsultationPurchaseRules.RefundAmount(purchase.PaymentPrice);
            if (amount > 0)
            {
                var ledgerName = ConsultationPurchaseRules.RefundLedgerName(purchaseId);
                var alreadyCredited = await _context.Wallets.AsNoTracking().AnyAsync(s => s.Name == ledgerName && s.UserId == purchase.UserId && !s.Deleted);
                if (!alreadyCredited)
                {
                    var credit = await _walletService.InsertAsyncDto(new WalletDto
                    {
                        Name = ledgerName,
                        Amount = amount,
                        IsIncrease = true,
                        UserId = purchase.UserId,
                        Painding = false
                    });
                    if (!credit.IsSuccess)
                        return false;
                }
            }

            await _context.ConsultationPurchases
                .Where(s => s.Id == purchaseId && s.RefundDate == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Refunded)
                    // خرید بازپرداخت‌شده درآمدی برای کلینیک/پاستیل ندارد؛ مبلغ پرداختی (PaymentPrice) برای سابقه می‌ماند
                    .SetProperty(s => s.CompanionShare, 0d)
                    .SetProperty(s => s.SiteShare, 0d)
                    .SetProperty(s => s.RefundDate, (DateTime?)DateTime.Now));
            return true;
        }

        // پت‌های هر خرید (برای «مشاوره‌های من» کاربر)
        private async Task<Dictionary<long, List<Dto.ConsultationPurchasePetVDto>>> LoadPetsAsync(List<long> purchaseIds)
        {
            if (purchaseIds.Count == 0)
                return new Dictionary<long, List<Dto.ConsultationPurchasePetVDto>>();
            var rows = await _context.ConsultationPurchasePets.AsNoTracking()
                .Where(p => purchaseIds.Contains(p.ConsultationPurchaseId))
                .Select(p => new
                {
                    p.ConsultationPurchaseId,
                    Pet = new Dto.ConsultationPurchasePetVDto
                    {
                        UserPetId = p.UserPetId,
                        Name = p.UserPet.Name,
                        PetName = p.UserPet.Pet.Name,
                        PictureUrl = p.UserPet.Picture == null ? null : p.UserPet.Picture.Url + "/" + p.UserPet.Picture.Name
                    }
                })
                .ToListAsync();
            return rows.GroupBy(r => r.ConsultationPurchaseId).ToDictionary(g => g.Key, g => g.Select(r => r.Pet).ToList());
        }

        private static ConsultationPurchaseVDto WithPets(ConsultationPurchaseVDto vdto, Dictionary<long, List<Dto.ConsultationPurchasePetVDto>> pets)
        {
            if (pets.TryGetValue(vdto.Id, out var list))
                vdto.Pets = list;
            return vdto;
        }

        private async Task<bool> TryInsertWithUniqueCodeAsync(ConsultationPurchase purchase)
        {
            for (var attempt = 0; attempt < 3; attempt++)
            {
                purchase.PurchaseCode = ConsultationPurchaseRules.NewPurchaseCode(DateTime.Now, RandomNumberGenerator.GetInt32(10000));
                try
                {
                    await _context.ConsultationPurchases.AddAsync(purchase);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateException)
                {
                    // برخورد نادر با ایندکس یکتای PurchaseCode: موجودیت ناموفق را رها و کد جدید امتحان می‌شود
                    _context.Entry(purchase).State = EntityState.Detached;
                    purchase.Id = 0;
                    foreach (var pet in purchase.Pets ?? new List<ConsultationPurchasePet>())
                    {
                        _context.Entry(pet).State = EntityState.Detached;
                        pet.Id = 0;
                        pet.ConsultationPurchaseId = 0;
                    }
                }
            }
            return false;
        }

        private static ConsultationPurchaseVDto ToVDto(ConsultationPurchase s, string companionName, DateTime now) => new()
        {
            Id = s.Id,
            PurchaseCode = s.PurchaseCode,
            CompanionId = s.CompanionId,
            ClinicId = s.ClinicId,
            CoachId = s.CoachId,
            CompanionName = companionName,
            PackageName = s.PackageName,
            ChannelId = s.ChannelId,
            DurationMinutes = s.DurationMinutes,
            Price = s.Price,
            RebatePrice = s.RebatePrice,
            PaymentPrice = s.PaymentPrice,
            WalletPrice = s.WalletPrice,
            Status = s.Status,
            CreateDate = s.CreateDate,
            PaidDate = s.PaidDate,
            StartDeadline = s.StartDeadline,
            ScheduledStart = s.ScheduledStart,
            ScheduledEnd = s.ScheduledEnd,
            CancelAllowedUntil = s.ScheduledStart?.Subtract(ConsultationRules.BookingFreeCancelBefore),
            StartDate = s.StartDate,
            ExpireDate = s.ExpireDate,
            OnlineSessionId = s.OnlineSessionId,
            CancelDate = s.CancelDate,
            RefundDate = s.RefundDate,
            ServerNow = now,
            Rate = s.Rate,
            ReviewText = s.ReviewText,
            ReviewedAt = s.ReviewedAt
        };
    }
}
