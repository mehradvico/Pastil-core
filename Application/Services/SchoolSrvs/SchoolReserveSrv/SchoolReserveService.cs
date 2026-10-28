using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Order.PaymentSrv;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.SchoolSrvs.SchoolCourseSrv;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using AutoMapper;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolReserveSrv
{
    public class SchoolReserveService : CommonSrv<SchoolReserve, SchoolReserveDto>, ISchoolReserveService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IRebateService _rebateService;
        private readonly IWalletService _walletService;
        private readonly ILogger<SchoolReserveService> _logger;

        public SchoolReserveService(
            IDataBaseContext _context,
            IMapper mapper,
            ICurrentUserHelper currentUser,
            IPushNotificationService pushNotificationService,
            IRebateService rebateService,
            IWalletService walletService,
            ILogger<SchoolReserveService> logger,
            Application.Services.TripSrv.TripSrv.Iface.ITripService tripService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUser = currentUser;
            this._pushNotificationService = pushNotificationService;
            this._rebateService = rebateService;
            this._walletService = walletService;
            this._logger = logger;
            this._tripService = tripService;
        }
        private readonly Application.Services.TripSrv.TripSrv.Iface.ITripService _tripService;

        // null یعنی ظرفیت نامحدود
        public async Task<int?> GetRemainingCapacityAsync(long schoolCourseId)
        {
            var course = await _context.SchoolCourses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == schoolCourseId);
            if (course == null) return 0;
            if (!course.Capacity.HasValue) return null;
            var taken = await SchoolCourseRules.SeatTakers(_context.SchoolReserves.Where(r => r.SchoolCourseId == schoolCourseId), DateTime.Now).CountAsync();
            return Math.Max(course.Capacity.Value - taken, 0);
        }

        public async Task<BaseResultDto<SchoolReserveVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.SchoolReserves
                .Include(r => r.SchoolCourse).ThenInclude(c => c.Pet)
                .Include(r => r.SchoolCourse).ThenInclude(c => c.PetBreed)
                .Include(r => r.SchoolCourse).ThenInclude(c => c.SchoolCourseSessions.Where(s => !s.Deleted).OrderBy(s => s.SessionDate).ThenBy(s => s.StartTime))
                .Include(r => r.SchoolCourse).ThenInclude(c => c.School).ThenInclude(s => s.Companion)
                .Include(r => r.UserPet)
                .Include(r => r.Booker)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (item != null)
            {
                var vdto = mapper.Map<SchoolReserveVDto>(item);
                var trips = await Application.Services.TripSrv.TripSrv.LinkedPetResanTripHelper.ForSchoolReservesAsync(_context, new[] { vdto.Id });
                vdto.PetResanTrip = trips.TryGetValue(vdto.Id, out var trip) ? trip : null;
                vdto.TotalPrice = vdto.PaymentPrice + (vdto.PetResanTrip?.PaymentPrice > 0 ? vdto.PetResanTrip.PaymentPrice : vdto.PetResanTrip?.Price ?? 0);
                return new BaseResultDto<SchoolReserveVDto>(true, vdto);
            }
            return new BaseResultDto<SchoolReserveVDto>(false, mapper.Map<SchoolReserveVDto>(item));
        }

        public SchoolReserveSearchDto Search(SchoolReserveInputDto baseSearchDto)
        {
            var model = _context.SchoolReserves
                .Include(r => r.SchoolCourse).ThenInclude(c => c.School).ThenInclude(s => s.Companion)
                .Include(r => r.Booker)
                .Include(r => r.UserPet)
                .AsQueryable();

            if (baseSearchDto.BookerId.HasValue)
                model = model.Where(r => r.BookerId == baseSearchDto.BookerId.Value);
            if (baseSearchDto.SchoolCourseId.HasValue)
                model = model.Where(r => r.SchoolCourseId == baseSearchDto.SchoolCourseId.Value);
            if (baseSearchDto.SchoolId.HasValue)
                model = model.Where(r => r.SchoolCourse.SchoolId == baseSearchDto.SchoolId.Value);
            if (baseSearchDto.CompanionId.HasValue)
                model = model.Where(r => r.SchoolCourse.School.CompanionId == baseSearchDto.CompanionId.Value);
            if (baseSearchDto.OwnerUserId.HasValue)
                model = model.Where(r => ((r.SchoolCourse.School.Provider != null && r.SchoolCourse.School.Provider.UserId == baseSearchDto.OwnerUserId.Value && !r.SchoolCourse.School.Provider.Deleted) || (r.SchoolCourse.School.Companion != null && r.SchoolCourse.School.Companion.OwnerId == baseSearchDto.OwnerUserId.Value && !r.SchoolCourse.School.Companion.Deleted)));
            if (baseSearchDto.StatusId.HasValue)
                model = model.Where(r => r.StatusId == baseSearchDto.StatusId.Value);

            model = baseSearchDto.SortBy switch
            {
                Common.Enumerable.SortEnum.Old => model.OrderBy(r => r.Id),
                _ => model.OrderByDescending(r => r.Id),
            };

            return new SchoolReserveSearchDto(baseSearchDto, model, mapper);
        }

        // ثبت‌نام پت در یک دوره - ظرفیت داخل یک تراکنش Serializable چک می‌شود تا در صورت
        // درخواست هم‌زمان چند کاربر برای آخرین جای خالی، رزرو اضافه بر ظرفیت ثبت نشود.
        public override async Task<BaseResultDto<SchoolReserveDto>> InsertAsyncDto(SchoolReserveDto dto)
        {
            if (dto == null)
                return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.InvalidData, dto);

            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                if (await Application.Services.CompanionSrvs.CompanionReserveDebtSrv.CompanionReserveDebtRules.IsLockedAsync(_context, dto.BookerId, DateTime.Now))
                    return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.UnpaidDebtLocked, dto);

                var course = await _context.SchoolCourses.Include(c => c.School)
                    .FirstOrDefaultAsync(c => c.Id == dto.SchoolCourseId && !c.Deleted && c.Active && !c.School.Deleted);
                if (course == null)
                {
                    return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.NothingFound, dto);
                }

                var ownedPet = await _context.UserPets.AnyAsync(p => p.Id == dto.UserPetId && p.UserId == dto.BookerId);
                if (!ownedPet)
                {
                    return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.InvalidData, dto);
                }

                // پیش‌نیاز: همین پت باید دوره‌ی پیش‌نیاز را «کامل‌شده» داشته باشد
                if (course.PrerequisiteCourseId.HasValue)
                {
                    var complete = (int)SchoolReserveStatusEnum.Complete;
                    var passed = await _context.SchoolReserves.AsNoTracking().AnyAsync(r =>
                        r.SchoolCourseId == course.PrerequisiteCourseId.Value && r.UserPetId == dto.UserPetId
                        && r.IsReserved && !r.IsCancel && r.StatusId == complete);
                    if (!passed)
                        return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.SchoolCoursePrerequisiteRequired, dto);
                }

                var enrollment = await SchoolCourseRules.EnrollmentAsync(_context, course.Id, DateTime.Now);

                var existing = await _context.SchoolReserves.AsTracking().FirstOrDefaultAsync(r =>
                    r.SchoolCourseId == dto.SchoolCourseId && r.UserPetId == dto.UserPetId && !r.IsCancel);
                if (existing != null)
                {
                    if (existing.IsReserved || existing.BookerId != dto.BookerId)
                    {
                        return new BaseResultDto<SchoolReserveDto>(
                            false, Resource.Notification.PaymentReserveAlreadyPaid, mapper.Map<SchoolReserveDto>(existing),
                            (int)ResultCodeEnum.PaymentAlreadyPaid);
                    }

                    if (!enrollment.Open)
                        return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.SchoolCourseRegistrationClosed, dto);

                    // اگر نگه‌داشتنِ جای این ثبت‌نام تمام شده، تلاش دوباره فقط وقتی ممکن است که هنوز جایی خالی باشد
                    if (course.Capacity.HasValue)
                    {
                        var othersTaking = await SchoolCourseRules.SeatTakers(
                            _context.SchoolReserves.Where(r => r.SchoolCourseId == dto.SchoolCourseId && r.Id != existing.Id), DateTime.Now).CountAsync();
                        if (othersTaking >= course.Capacity.Value)
                            return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.SchoolCourseCapacityFull, dto);
                    }

                    // ثبت‌نامِ پرداخت‌نشده‌ی قبلی (کاربر از درگاه برگشت یا اصلاً به درگاه نرسید): به‌جای «قبلاً رزرو شده»
                    // همان ثبت‌نام برای پرداخت دوباره برمی‌گردد. تلاش‌های درگاهِ رهاشده کنار گذاشته می‌شوند و مبلغ مثل یک
                    // ثبت‌نام تازه از قیمت فعلی دوره شروع می‌شود (صفحه‌ی نهایی‌سازی کیف پول/تخفیف را دوباره اعمال می‌کند).
                    var retryState = await PaymentAttemptRules.ReleaseForRetryAsync(
                        _context, PaymentCallbackTypeEnum.SchoolReserve.ToString(), existing.Id.ToString());
                    if (retryState == PaymentAttemptRules.TargetState.Paid)
                    {
                        return new BaseResultDto<SchoolReserveDto>(
                            false, Resource.Notification.PaymentReserveAlreadyPaid, mapper.Map<SchoolReserveDto>(existing),
                            (int)ResultCodeEnum.PaymentAlreadyPaid);
                    }
                    if (retryState == PaymentAttemptRules.TargetState.Verifying)
                    {
                        return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.PaymentPreviousAttemptVerifying, dto);
                    }

                    existing.Price = enrollment.Price;
                    existing.PaymentPrice = enrollment.Price;
                    existing.RebateId = null;
                    existing.RebatePrice = 0;
                    existing.FromWallet = false;
                    existing.WalletPrice = 0;
                    existing.CreateDate = DateTime.Now; // جای دوره دوباره برای UnpaidSeatHold نگه داشته می‌شود
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new BaseResultDto<SchoolReserveDto>(true, mapper.Map<SchoolReserveDto>(existing));
                }

                // بعد از شروع جلسه‌ی اول فقط طبق LateEnrollmentMode (بسته / قیمت کامل / قیمت نسبی)
                if (!enrollment.Open)
                    return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.SchoolCourseRegistrationClosed, dto);

                if (course.Capacity.HasValue)
                {
                    var takenCount = await SchoolCourseRules.SeatTakers(_context.SchoolReserves.Where(r => r.SchoolCourseId == dto.SchoolCourseId), DateTime.Now).CountAsync();
                    if (takenCount >= course.Capacity.Value)
                    {
                        return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.SchoolCourseCapacityFull, dto);
                    }
                }

                var item = mapper.Map<SchoolReserve>(dto);
                // تخفیف از بدنه پذیرفته نمی‌شود (RebatePrice منفی + حذف کد، مبلغ را پایین می‌آورد)؛ فقط SetRebateCode
                item.RebateId = null;
                item.RebatePrice = 0;
                item.Price = enrollment.Price;
                item.PaymentPrice = enrollment.Price;
                item.FromWallet = false;
                item.WalletPrice = 0;
                item.IsCancel = false;
                item.IsReserved = false;
                item.CreateDate = DateTime.Now;
                item.StatusId = (int)SchoolReserveStatusEnum.Registered;
                item.ReserveCode = PaymentCodeGenerator.Create(
                    PaymentCallbackTypeEnum.SchoolReserve,
                    item.CreateDate,
                    await _context.GetNextBusinessCodeNumberAsync());

                await _context.SchoolReserves.AddAsync(item);
                // ثبت‌نام کرد: از لیست انتظار همین دوره بیرون می‌آید
                await _context.SchoolCourseWaitlists
                    .Where(w => w.SchoolCourseId == dto.SchoolCourseId && w.UserPetId == dto.UserPetId && !w.Deleted)
                    .ExecuteUpdateAsync(x => x.SetProperty(w => w.Deleted, true));
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResultDto<SchoolReserveDto>(true, mapper.Map<SchoolReserveDto>(item));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Enrolling pet {UserPetId} in school course {SchoolCourseId} failed.", dto.UserPetId, dto.SchoolCourseId);
                return new BaseResultDto<SchoolReserveDto>(false, Resource.Notification.Unsuccess, dto);
            }
        }

        // این متد همیشه از داخل PaymentService.ApplyAndMapPaymentAsync صدا زده می‌شود که خودش از
        // قبل یک تراکنش Serializable باز کرده - باز کردن یک تراکنش تو در تو روی همان DbContext
        // در حالت تست (پرداخت آنی درون همان درخواست) خطا می‌داد، پس فقط وقتی تراکنش فعالی از
        // بیرون نیامده، خودمان یکی باز می‌کنیم.
        public async Task<BaseResultDto> SchoolReservePaymentCallback(long? reserveId, bool fromWallet = false)
        {
            if (_context.CurrentTransaction != null)
                return await SchoolReservePaymentCallbackCoreAsync(reserveId, fromWallet);

            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
            var result = await SchoolReservePaymentCallbackCoreAsync(reserveId, fromWallet);
            if (result.IsSuccess)
                await transaction.CommitAsync();
            else
                await transaction.RollbackAsync();
            return result;
        }

        private async Task<BaseResultDto> SchoolReservePaymentCallbackCoreAsync(long? reserveId, bool fromWallet)
        {
            try
            {
                var reserve = await _context.SchoolReserves.Include(r => r.SchoolCourse).Include(r => r.Booker).Include(r => r.Rebate)
                    .AsTracking().FirstOrDefaultAsync(r => r.Id == reserveId);
                if (reserve == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (reserve.IsReserved)
                    return new BaseResultDto(true);
                if (reserve.IsCancel)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                if (fromWallet && reserve.FromWallet && reserve.WalletPrice > 0)
                {
                    var walletItem = new WalletDto()
                    {
                        Painding = false,
                        Amount = reserve.WalletPrice,
                        UserId = reserve.Booker.Id,
                        SchoolReserveId = reserve.Id
                    };
                    var walletResult = await _walletService.InsertUpdateSchoolReserveAsync(walletItem, true);
                    if (!walletResult.IsSuccess)
                        return new BaseResultDto(false);
                }
                if (reserve.Rebate != null)
                {
                    _rebateService.IncreaseUseCount(reserve.Rebate, reserve.BookerId, reserve.RebatePrice);
                }

                reserve.IsReserved = true;
                reserve.StatusId = (int)SchoolReserveStatusEnum.Paid;

                if (reserve.SchoolCourse != null)
                {
                    var commissionPercent = reserve.SchoolCourse.CommissionPercent;
                    var siteShare = (double)((decimal)reserve.PaymentPrice * commissionPercent / 100m);
                    reserve.SiteShare = siteShare;
                    reserve.CompanionShare = reserve.PaymentPrice - siteShare;
                }

                await _context.SaveChangesAsync();

                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "School reserve payment callback failed for reserve {ReserveId}.", reserveId);
                return new BaseResultDto(false);
            }
        }

        public async Task<BaseResultDto> SetRebateCodeAsyncDto(SchoolReserveRebateCodeDto dto)
        {
            var item = await _context.SchoolReserves.AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id &&
                s.BookerId == _currentUser.CurrentUser.UserId &&
                s.StatusId == (int)SchoolReserveStatusEnum.Registered);

            if (item == null)
            {
                return new BaseResultDto<SchoolReserveRebateCodeDto>(false, Resource.Notification.NothingFound, dto);
            }
            if (string.IsNullOrEmpty(dto.RebateCode))
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
            var financialLock = await GuardFinancialChangeAsync(item.Id);
            if (financialLock != null)
                return financialLock;

            // پایه‌ی تخفیف همان Price سرور (قیمت دوره) است، نه PaymentPrice + RebatePrice
            var originalPrice = item.Price;
            var rebate = _rebateService.GetRebateByCodeAsync(originalPrice, item.BookerId, RebateTypeLabels.SchoolReserve, dto.RebateCode);
            if (rebate.IsSuccess)
            {
                item.RebateId = rebate.Data.Id;
                item.RebatePrice = rebate.Data.FinalPrice;
                item.PaymentPrice = originalPrice - item.RebatePrice;
                if (item.PaymentPrice < 0)
                {
                    item.PaymentPrice = 0;
                }
                if (item.WalletPrice != 0)
                {
                    item.WalletPrice = item.PaymentPrice;
                }
                _context.SchoolReserves.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
            }
            else
            {
                return new BaseResultDto(isSuccess: false, messages: rebate.Messages);
            }
        }

        public async Task<BaseResultDto> ClearRebateCodeAsync(long id)
        {
            var item = await _context.SchoolReserves.AsTracking().FirstOrDefaultAsync(s =>
                s.Id == id && s.BookerId == _currentUser.CurrentUser.UserId && !s.IsReserved);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var financialLock = await GuardFinancialChangeAsync(item.Id);
            if (financialLock != null)
                return financialLock;
            if (!item.RebateId.HasValue)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }
            item.RebateId = null;
            item.PaymentPrice = item.Price;
            item.RebatePrice = 0;
            if (item.WalletPrice != 0)
            {
                item.WalletPrice = item.PaymentPrice;
            }
            _context.SchoolReserves.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetWalletAsyncDto(SchoolReserveWalletDto dto)
        {
            var item = await _context.SchoolReserves.Include(s => s.Booker).AsTracking().FirstOrDefaultAsync(s =>
                s.Id == dto.Id && s.BookerId == _currentUser.CurrentUser.UserId);
            if (item == null)
            {
                return new BaseResultDto<SchoolReserveWalletDto>(false, Resource.Notification.NothingFound, dto);
            }
            var financialLock = await GuardFinancialChangeAsync(item.Id);
            if (financialLock != null)
                return financialLock;
            if (item.IsReserved)
            {
                return new BaseResultDto<SchoolReserveWalletDto>(false, Resource.Notification.ThisReserveIsPaid, dto);
            }
            if (dto.FromWallet)
            {
                item.FromWallet = true;
                item.WalletPrice = item.PaymentPrice;
            }
            else
            {
                item.FromWallet = false;
                item.WalletPrice = 0;
            }
            _context.SchoolReserves.Update(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto(isSuccess: true, val: Resource.Notification.Success);
        }

        // تغییر کیف پول/تخفیف توسط خود کاربر: تلاش درگاهِ رهاشده (برگشت بدون پرداخت / نرسیدن به درگاه) دیگر قفل نمی‌کند و
        // کنار گذاشته می‌شود؛ فقط پرداخت موفق («قبلاً پرداخت شده») یا callbackِ در حال verify جلوی تغییر مبلغ را می‌گیرد.
        private Task<BaseResultDto> GuardFinancialChangeAsync(long reserveId)
        {
            return Application.Services.Order.PaymentSrv.PaymentAttemptRules.GuardUserRetryAsync(
                _context, PaymentCallbackTypeEnum.SchoolReserve.ToString(), reserveId.ToString(),
                Resource.Notification.PaymentReserveAlreadyPaid);
        }

        public async Task<BaseResultDto> UpdateCancelDto(SchoolReserveCancelDto dto)
        {
            var model = await _context.SchoolReserves.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.IsReserved);
            if (model == null)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString() ||
                await _context.Roles
                    .Where(r => r.Id == _currentUser.CurrentUser.RoleId)
                    .SelectMany(r => r.Permissions)
                    .AnyAsync(p => !p.Deleted && p.Area == "Admin");
            var isOwner = model.BookerId == _currentUser.CurrentUser.UserId;

            if (!isAdmin && !isOwner)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            if (!dto.IsCancel)
            {
                await _context.SaveChangesAsync();
                return new BaseResultDto(true);
            }

            if (string.IsNullOrWhiteSpace(dto.CancelDetail))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterCancelDetail);
            if (model.IsCancel)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            // کاربر فقط تا قبل از شروع دوره و تا وقتی «کامل‌شده» نشده لغو می‌کند؛ بعد از آن فقط ادمین
            if (!isAdmin && (model.StatusId != (int)SchoolReserveStatusEnum.Paid ||
                             await SchoolCourseRules.HasStartedAsync(_context, model.SchoolCourseId, DateTime.Now)))
                return new BaseResultDto(false, Resource.Notification.YouCanNotCancelThisReservation);

            // سیاست لغو دوره: کاربر طبق فاصله تا جلسه‌ی اول درصدی پس می‌گیرد؛ لغو ادمین همیشه ۱۰۰٪
            var refundPercent = 100;
            if (!isAdmin)
            {
                var policy = await _context.SchoolCourses.AsNoTracking().Where(c => c.Id == model.SchoolCourseId)
                    .Select(c => new { c.FullRefundHoursBeforeStart, c.LateCancelRefundPercent }).FirstAsync();
                var starts = await SchoolCourseRules.SessionStartsAsync(_context, model.SchoolCourseId);
                refundPercent = SchoolCourseRules.RefundPercent(starts.Count == 0 ? null : starts.Min(), DateTime.Now,
                    policy.FullRefundHoursBeforeStart, policy.LateCancelRefundPercent);
            }
            var refundAmount = Math.Round(model.PaymentPrice * refundPercent / 100.0);

            // لغو + برگشت کل مبلغ پرداخت‌شده به کیف پول در یک تراکنش؛ اگر برگشت وجه نشد ثبت‌نام لغو نمی‌شود
            await using var transaction = _context.CurrentTransaction == null
                ? await _context.BeginTransactionAsync(IsolationLevel.Serializable)
                : null;

            model.IsCancel = true;
            model.CancelDetail = dto.CancelDetail;
            model.CancelDate = DateTime.Now;
            model.StatusId = (int)SchoolReserveStatusEnum.Cancelled;
            // سهم‌ها فقط وقتی تغییر می‌کنند که هنوز در تسویه‌ای پرداخت نشده باشند؛ برگشت جزئی ⇒ سهم‌ها به نسبت مبلغ نگه‌داشته‌شده
            // (ثبت‌نام لغوشده با سهم مثبت همچنان در تسویه‌ی مدرسه می‌آید — SettlementSources)
            if (!model.Permitted)
            {
                var keptRatio = model.PaymentPrice > 0 ? Math.Max(0, model.PaymentPrice - refundAmount) / model.PaymentPrice : 0;
                model.SiteShare = Math.Round(model.SiteShare * keptRatio);
                model.CompanionShare = Math.Round(model.CompanionShare * keptRatio);
            }
            await _context.SaveChangesAsync();

            if (!await RefundAsync(model, refundAmount))
                return new BaseResultDto(false, Resource.Notification.SchoolReserveRefundFailed);

            if (transaction != null)
                await transaction.CommitAsync();

            await _tripService.CancelLinkedTripForSchoolReserveAsync(model.Id);
            // یک جا خالی شد: به لیست انتظار خبر می‌دهیم (خطا لغو را برنمی‌گرداند)
            try { await Application.Services.SchoolSrvs.SchoolWaitlistSrv.SchoolWaitlistNotifier.NotifySeatOpenAsync(_context, _pushNotificationService, model.SchoolCourseId, DateTime.Now); }
            catch (Exception ex) { _logger.LogError(ex, "Waitlist notify failed for school course {CourseId}.", model.SchoolCourseId); }
            return new BaseResultDto(true);
        }

        // برگشت مبلغ (کل یا بخشی طبق سیاست لغو) به کیف پول؛ کلید دفتر (Name) یکتاست پس تکرار پول را دوباره برنمی‌گرداند.
        private async Task<bool> RefundAsync(SchoolReserve reserve, double amount)
        {
            if (amount <= 0)
                return true;
            var ledgerName = SchoolReserveRules.RefundLedgerName(reserve.Id);
            var exists = await _context.Wallets.AsNoTracking()
                .AnyAsync(w => w.Name == ledgerName && w.UserId == reserve.BookerId && !w.Deleted);
            if (exists)
                return true;
            var result = await _walletService.InsertAsyncDto(new WalletDto
            {
                Name = ledgerName,
                Amount = amount,
                IsIncrease = true,
                UserId = reserve.BookerId,
                Painding = false,
            });
            return result.IsSuccess;
        }

        // مدرسه‌ی صاحب دوره می‌تواند ثبت‌نام «پرداخت‌شده» و لغو‌نشده‌ی خودش را «کامل‌شده» کند؛ تغییرات دیگر فقط ادمین
        public async Task<BaseResultDto> CompleteByCompanionAsync(long id, long ownedSchoolId)
        {
            var paid = (int)SchoolReserveStatusEnum.Paid;
            var done = await _context.SchoolReserves
                .Where(s => s.Id == id && s.IsReserved && !s.IsCancel && s.StatusId == paid && s.SchoolCourse.SchoolId == ownedSchoolId)
                .ExecuteUpdateAsync(x => x.SetProperty(r => r.StatusId, (int)SchoolReserveStatusEnum.Complete));
            return done == 0 ? new BaseResultDto(false, Resource.Notification.NothingFound) : new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateStatusDto(SchoolReserveStatusDto dto)
        {
            var model = await _context.SchoolReserves.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.IsReserved);
            if (model == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            model.StatusId = dto.StatusId;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task SendClassStartingPushesAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var windowEnd = now.AddMinutes(10);

            // فقط جلسات دوره‌های زنده (Live) که در ۱۰ دقیقه‌ی آینده شروع می‌شوند و هنوز پوششان نرفته.
            var dueSessions = await _context.SchoolCourseSessions
                .Include(s => s.SchoolCourse)
                .AsTracking()
                .Where(s =>
                    !s.Deleted &&
                    s.Active &&
                    s.StartingPushSentDate == null &&
                    s.SchoolCourse.CourseTypeId != (int)SchoolCourseTypeEnum.Video &&
                    s.SessionDate.Date == now.Date)
                .ToListAsync(cancellationToken);

            foreach (var session in dueSessions)
            {
                if (!TimeSpan.TryParse(session.StartTime, out var startTime))
                    continue;

                var sessionStart = session.SessionDate.Date + startTime;
                if (sessionStart < now || sessionStart > windowEnd)
                    continue;

                var enrolledBookerIds = await _context.SchoolReserves
                    .Where(r => r.SchoolCourseId == session.SchoolCourseId && r.IsReserved && !r.IsCancel)
                    .Select(r => r.BookerId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                foreach (var bookerId in enrolledBookerIds)
                {
                    try
                    {
                        await _pushNotificationService.SendPushAsync(
                            PushTypeEnum.PushSchoolClassStarting,
                            bookerId,
                            token1: session.SchoolCourse.Name,
                            token2: session.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send class-starting push for session {SessionId} to booker {BookerId}.", session.Id, bookerId);
                    }
                }

                session.StartingPushSentDate = now;
            }

            if (dueSessions.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
