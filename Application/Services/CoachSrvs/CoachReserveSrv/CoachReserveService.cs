using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.Accounting.ScoreTransactionSrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveDebtSrv;
using Application.Services.CoachSrvs.CoachReserveSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveSrv.Iface;
using Application.Services.Order.PaymentSrv;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachReserveSrv
{
    public class CoachReserveService : ICoachReserveService
    {
        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly IRebateService _rebateService;
        private readonly ICoachNotifier _notifier;
        private readonly IScoreTransactionService _scoreService;
        private readonly IClubPointIntegrationService _clubPoints;
        private readonly ILogger<CoachReserveService> _logger;

        public CoachReserveService(
            IDataBaseContext context,
            IWalletService walletService,
            IRebateService rebateService,
            ICoachNotifier notifier,
            IScoreTransactionService scoreService,
            IClubPointIntegrationService clubPoints,
            ILogger<CoachReserveService> logger)
        {
            _context = context;
            _walletService = walletService;
            _rebateService = rebateService;
            _notifier = notifier;
            _scoreService = scoreService;
            _clubPoints = clubPoints;
            _logger = logger;
        }

        // ------------------------------------------------------------------ قیمت و زمان‌های آزاد

        public Task<BaseResultDto<CoachReserveQuoteDto>> QuoteAsync(long packageId, int mode) =>
            QuoteAsync(new[] { packageId }, mode);

        public async Task<BaseResultDto<CoachReserveQuoteDto>> QuoteAsync(IReadOnlyList<long> packageIds, int mode)
        {
            var (set, error) = await BuildSetAsync(packageIds, null, mode);
            if (error != null)
                return new BaseResultDto<CoachReserveQuoteDto>(false, error, null);
            var modeError = ValidateOffered(set.Primary, mode);
            if (modeError != null)
                return new BaseResultDto<CoachReserveQuoteDto>(false, modeError, null);

            var (price, online) = CoachReserveRules.Amounts(set.Packages.Select(p => (p.Price, p.PrePaymentPrice)));
            return new BaseResultDto<CoachReserveQuoteDto>(true, new CoachReserveQuoteDto
            {
                CoachServicePackageId = set.Primary.Id,
                PackageIds = set.Packages.Select(p => p.Id).ToList(),
                Mode = mode,
                PackagePrice = set.Packages.Sum(p => p.Price),
                Price = price,
                OnlineAmount = online,
                PayAtVenue = price - online,
                DurationMinutes = set.DurationMinutes,
            });
        }

        public async Task<BaseResultDto<CoachAvailabilityDto>> GetAvailabilityAsync(
            long coachId, DateTime date, long packageId, long? assigneeUserId, IReadOnlyList<long> extraPackageIds = null)
        {
            var (set, error) = await BuildSetAsync(new[] { packageId }.Concat(extraPackageIds ?? Array.Empty<long>()).ToList(), coachId, null);
            if (error != null)
                return new BaseResultDto<CoachAvailabilityDto>(false, error, null);
            var package = set.Primary;

            var day = date.Date;
            var duration = set.DurationMinutes;
            var result = new CoachAvailabilityDto { Date = day, DurationMinutes = duration };

            var weekDay = CoachReserveRules.WeekDayNumber(day);
            var rows = await _context.CoachTimes.AsNoTracking()
                .Where(t => t.CoachId == coachId && !t.Deleted && t.Active && t.WeekDay.Number == weekDay)
                .Select(t => new { t.Id, t.StartTime, t.EndTime, t.Capacity })
                .ToListAsync();
            if (rows.Count == 0)
                return new BaseResultDto<CoachAvailabilityDto>(true, result);

            var now = DateTime.Now;
            var holdFrom = now - CoachReserveRules.UnpaidHold;
            var dayEnd = day.AddDays(1);
            var existing = await _context.CoachReserves.AsNoTracking()
                .Where(r => r.CoachId == coachId && !r.IsCancel && r.DoDate < dayEnd && r.EndDate > day
                            && (r.IsReserved || r.CreateDate > holdFrom))
                .Select(r => new { r.DoDate, r.EndDate, r.AssigneeUserId })
                .ToListAsync();

            foreach (var row in rows.OrderBy(r => r.StartTime, StringComparer.Ordinal))
            {
                if (!CoachReserveRules.TryParseTime(row.StartTime, out var rowStart) || !CoachReserveRules.TryParseTime(row.EndTime, out var rowEnd))
                    continue;
                var capacity = CoachReserveRules.EffectiveCapacity(row.Capacity, package.MaxConcurrentPets);
                for (var offset = rowStart; offset + TimeSpan.FromMinutes(duration) <= rowEnd; offset += TimeSpan.FromMinutes(CoachReserveRules.SlotStepMinutes))
                {
                    var start = day + offset;
                    if (start < now + CoachReserveRules.MinLead)
                        continue;
                    var end = start.AddMinutes(duration);
                    var overlapping = existing.Where(r => CoachReserveRules.Overlaps(r.DoDate, r.EndDate, start, end)).ToList();
                    var concurrent = CoachReserveRules.MaxConcurrency(overlapping.Select(r => (r.DoDate, r.EndDate)), start, end);
                    var available = Math.Max(0, capacity - concurrent);
                    if (assigneeUserId.HasValue && overlapping.Any(r => r.AssigneeUserId == assigneeUserId))
                        available = 0;
                    result.Slots.Add(new CoachAvailabilitySlotDto
                    {
                        Time = $"{(int)offset.TotalHours:00}:{offset.Minutes:00}",
                        StartDate = start,
                        Capacity = capacity,
                        Available = available,
                    });
                }
            }
            return new BaseResultDto<CoachAvailabilityDto>(true, result);
        }

        // ------------------------------------------------------------------ ساخت رزرو

        public async Task<BaseResultDto<CoachReserveVDto>> CreateAsync(CoachReserveInputDto dto, long bookerId)
        {
            try
            {
                if (dto == null || dto.CoachId <= 0 || dto.CoachServicePackageId <= 0 || dto.UserPetId <= 0 || bookerId <= 0)
                    return Fail(Resource.Notification.InvalidData);

                var now = DateTime.Now;
                if (await CompanionReserveDebtRules.IsLockedAsync(_context, bookerId, now))
                    return Fail(Resource.Notification.UnpaidDebtLocked);

                var (set, setError) = await BuildSetAsync(new[] { dto.CoachServicePackageId }.Concat(dto.ExtraPackageIds ?? new List<long>()).ToList(), dto.CoachId, dto.Mode);
                if (setError != null)
                    return Fail(setError);
                var package = set.Primary;

                var pet = await _context.UserPets.AsNoTracking()
                    .Where(p => p.Id == dto.UserPetId && p.UserId == bookerId && !p.Deleted)
                    .Select(p => new { p.Id, p.PetId, p.Size })
                    .FirstOrDefaultAsync();
                if (pet == null)
                    return Fail(Resource.Notification.CoachReservePetInvalid);

                var accepted = await _context.CoachPets.AsNoTracking()
                    .Where(p => p.CoachId == dto.CoachId && !p.Deleted).Select(p => p.PetId).ToListAsync();
                if (accepted.Count > 0 && !accepted.Contains(pet.PetId))
                    return Fail(Resource.Notification.CoachReservePetTypeNotAccepted);
                // هر بسته‌ی انتخابی باید با سایز حیوان بخواند.
                if (set.Packages.Any(p => !string.IsNullOrWhiteSpace(p.PetSize) &&
                                          !string.Equals(p.PetSize, pet.Size?.Trim(), StringComparison.OrdinalIgnoreCase)))
                    return Fail(Resource.Notification.CoachReservePetSizeMismatch);

                var modeError = await ValidateModeAsync(dto, package, bookerId);
                if (modeError != null)
                    return Fail(modeError);

                var doDate = new DateTime(dto.DoDate.Year, dto.DoDate.Month, dto.DoDate.Day, dto.DoDate.Hour, dto.DoDate.Minute, 0);
                if (doDate < now + CoachReserveRules.MinLead)
                    return Fail(Resource.Notification.CoachReserveTimeInPast);
                var duration = set.DurationMinutes;
                var endDate = doDate.AddMinutes(duration);
                if (endDate.Date != doDate.Date)
                    return Fail(Resource.Notification.CoachReserveOutsideWorkingHours);

                var assigneeError = await ValidateAssigneeAsync(dto.AssigneeUserId, package);
                if (assigneeError != null)
                    return Fail(assigneeError);

                var (price, online) = CoachReserveRules.Amounts(set.Packages.Select(p => (p.Price, p.PrePaymentPrice)));

                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);

                // نوبتِ پرداخت‌نشده‌ی قبلیِ همین کاربر برای همین پت در همین بازه، با نوبت جدید جایگزین می‌شود (کاربر پرداخت را رها کرده و دوباره می‌خواهد).
                var staleHolds = await _context.CoachReserves.AsTracking()
                    .Where(r => r.BookerId == bookerId && r.UserPetId == dto.UserPetId && !r.IsReserved && !r.IsCancel
                                && r.DoDate < endDate && r.EndDate > doDate)
                    .ToListAsync();
                foreach (var stale in staleHolds)
                {
                    stale.IsCancel = true;
                    stale.CancelDate = now;
                    stale.CancelDetail = "replaced";
                    stale.CancelledByBooker = true;
                }
                if (staleHolds.Count > 0)
                    await _context.SaveChangesAsync();

                var slot = await CheckSlotAsync(dto.CoachId, doDate, endDate, dto.AssigneeUserId, dto.UserPetId, includeHolds: true, excludeReserveId: null, package.MaxConcurrentPets);
                if (slot.Error != null)
                {
                    await transaction.RollbackAsync();
                    return Fail(slot.Error);
                }

                var reserve = new CoachReserve
                {
                    CoachId = dto.CoachId,
                    CoachServiceId = package.CoachServiceId,
                    CoachServicePackageId = package.Id,
                    BookerId = bookerId,
                    UserPetId = dto.UserPetId,
                    AssigneeUserId = dto.AssigneeUserId,
                    CoachTimeId = slot.CoachTimeId,
                    DoDate = doDate,
                    DurationMinutes = duration,
                    EndDate = endDate,
                    Mode = dto.Mode,
                    AddressId = dto.Mode == (int)CoachServiceModeEnum.InPlace ? dto.AddressId : null,
                    BookerNote = string.IsNullOrWhiteSpace(dto.BookerNote) ? null : dto.BookerNote.Trim(),
                    PackagePrice = set.Packages.Sum(p => p.Price),
                    Price = price,
                    OnlineAmount = online,
                    PaymentPrice = online,
                    Status = (int)CoachReserveStatusEnum.Registered,
                    OwnerDecision = (int)CoachReserveOwnerDecisionEnum.NotRequired,
                    CreateDate = now,
                    Items = set.Packages.Select(p => new CoachReserveItem
                    {
                        CoachServiceId = p.CoachServiceId,
                        CoachServicePackageId = p.Id,
                        PackagePrice = p.Price,
                        OnlineAmount = CoachReserveRules.Deposit(p.Price, p.PrePaymentPrice),
                        DurationMinutes = CoachReserveRules.EffectiveDuration(p.DurationMinutes),
                        PackageName = p.Name,
                        CreateDate = now,
                    }).ToList(),
                };
                reserve.ReserveCode = PaymentCodeGenerator.Create(
                    PaymentCallbackTypeEnum.CoachReserve, now, await _context.GetNextBusinessCodeNumberAsync());

                await _context.CoachReserves.AddAsync(reserve);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (await FindAsync(reserve.Id, bookerId, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating coach reserve failed for coach {CoachId}, booker {BookerId}.", dto?.CoachId, bookerId);
                return Fail(Resource.Notification.Unsuccess);
            }
        }

        // ------------------------------------------------------------------ تغییر زمان

        public async Task<BaseResultDto<CoachReserveVDto>> RescheduleAsync(long id, DateTime newDoDate, long? ownerUserId)
        {
            try
            {
                var now = DateTime.Now;
                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                var reserve = await _context.CoachReserves.AsTracking()
                    .Include(r => r.Coach)
                    .FirstOrDefaultAsync(r => r.Id == id);
                if (reserve == null)
                {
                    await transaction.RollbackAsync();
                    return Fail(Resource.Notification.NothingFound);
                }
                if (ownerUserId.HasValue && reserve.Coach.OwnerId != ownerUserId.Value)
                {
                    await transaction.RollbackAsync();
                    return Fail(Resource.Notification.AccessDenied);
                }
                if (!reserve.IsReserved || reserve.IsCancel || reserve.Status == (int)CoachReserveStatusEnum.Complete)
                {
                    await transaction.RollbackAsync();
                    return Fail(Resource.Notification.CoachReserveRescheduleNotAllowed);
                }

                var start = new DateTime(newDoDate.Year, newDoDate.Month, newDoDate.Day, newDoDate.Hour, newDoDate.Minute, 0);
                if (start == reserve.DoDate)
                {
                    await transaction.RollbackAsync();
                    return Fail(Resource.Notification.CoachReserveRescheduleSameTime);
                }
                if (start < now + CoachReserveRules.MinLead)
                {
                    await transaction.RollbackAsync();
                    return Fail(Resource.Notification.CoachReserveTimeInPast);
                }
                var end = start.AddMinutes(reserve.DurationMinutes);
                var slot = await CheckSlotAsync(reserve.CoachId, start, end, reserve.AssigneeUserId, reserve.UserPetId,
                    includeHolds: false, excludeReserveId: reserve.Id, reserve.Coach.MaxConcurrentPets);
                if (slot.Error != null)
                {
                    await transaction.RollbackAsync();
                    return Fail(slot.Error);
                }

                var oldDoDate = reserve.DoDate;
                reserve.OriginalDoDate ??= reserve.DoDate;
                reserve.DoDate = start;
                reserve.EndDate = end;
                reserve.CoachTimeId = slot.CoachTimeId;
                reserve.RescheduleCount++;
                reserve.LastRescheduleDate = now;
                // مهلت پاسخ مربی به نوبتِ جدید نسبت داده می‌شود.
                if (CoachReserveRules.IsPending(reserve.OwnerDecision))
                    reserve.OwnerApprovalDeadline = CoachReserveRules.Deadline(now, start);
                await CoachReserveReminders.ShiftAsync(_context, reserve.Id, start);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _notifier.ReserveRescheduledAsync(reserve.Id, byOwner: ownerUserId.HasValue, oldDoDate);
                return await FindAsync(reserve.Id, null, ownerUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rescheduling coach reserve {ReserveId} failed.", id);
                return Fail(Resource.Notification.Unsuccess);
            }
        }

        // ------------------------------------------------------------------ جستجو و نمایش

        public CoachReservePage Search(CoachReserveSearchInputDto input)
        {
            var query = _context.CoachReserves.AsNoTracking().AsQueryable();
            if (input.BookerId.HasValue)
                query = query.Where(r => r.BookerId == input.BookerId.Value);
            if (input.CoachId.HasValue)
                query = query.Where(r => r.CoachId == input.CoachId.Value);
            if (input.ProviderId.HasValue)
                query = query.Where(r => r.Coach.ProviderId == input.ProviderId.Value);
            if (input.OwnerUserId.HasValue)
                query = query.Where(r => r.Coach.OwnerId == input.OwnerUserId.Value);
            if (input.UserPetId.HasValue)
                query = query.Where(r => r.UserPetId == input.UserPetId.Value);
            if (input.Status.HasValue)
                query = query.Where(r => r.Status == input.Status.Value);
            if (input.OwnerDecision.HasValue)
                query = query.Where(r => r.OwnerDecision == input.OwnerDecision.Value);
            if (input.IsCancel.HasValue)
                query = query.Where(r => r.IsCancel == input.IsCancel.Value);
            if (input.OnlyPaid == true)
                query = query.Where(r => r.IsReserved);
            if (input.DateFrom.HasValue)
                query = query.Where(r => r.DoDate >= input.DateFrom.Value.Date);
            if (input.DateTo.HasValue)
            {
                var to = input.DateTo.Value.Date.AddDays(1);
                query = query.Where(r => r.DoDate < to);
            }
            if (!string.IsNullOrWhiteSpace(input.Q))
            {
                var q = input.Q.Trim();
                query = query.Where(r => r.ReserveCode == q || r.Booker.FirstName.Contains(q) || r.Booker.LastName.Contains(q) ||
                                         r.Booker.Mobile.Contains(q) || r.Coach.Name.Contains(q));
            }

            query = input.SortBy == SortEnum.Old ? query.OrderBy(r => r.Id) : query.OrderByDescending(r => r.Id);
            var total = query.Count();
            var rows = Project(query.Skip((input.PageIndex - 1) * input.PageSize).Take(input.PageSize)).ToList();
            var now = DateTime.Now;
            foreach (var row in rows)
                Finish(row, now);
            return new CoachReservePage { PageIndex = input.PageIndex, PageSize = input.PageSize, TotalCount = total, List = rows };
        }

        public async Task<BaseResultDto<CoachReserveVDto>> FindAsync(long id, long? bookerId, long? ownerUserId)
        {
            var query = _context.CoachReserves.AsNoTracking().Where(r => r.Id == id);
            if (bookerId.HasValue)
                query = query.Where(r => r.BookerId == bookerId.Value);
            if (ownerUserId.HasValue)
                query = query.Where(r => r.Coach.OwnerId == ownerUserId.Value);
            var item = await Project(query).FirstOrDefaultAsync();
            if (item == null)
                return Fail(Resource.Notification.NothingFound);
            Finish(item, DateTime.Now);
            if (bookerId.HasValue)
            {
                // کاربر سهم‌های حسابداری را نمی‌بیند.
                item.ProviderShare = 0;
                item.SiteShare = 0;
                item.CommissionPercent = 0;
                item.Permitted = false;
            }
            return new BaseResultDto<CoachReserveVDto>(true, item);
        }

        // ------------------------------------------------------------------ تخفیف و کیف پول

        public async Task<BaseResultDto> SetRebateCodeAsync(long id, string rebateCode, long bookerId)
        {
            if (string.IsNullOrWhiteSpace(rebateCode))
                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            var reserve = await _context.CoachReserves.AsTracking()
                .Include(r => r.CoachService)
                .FirstOrDefaultAsync(r => r.Id == id && r.BookerId == bookerId && !r.IsReserved && !r.IsCancel);
            if (reserve == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var locked = await GuardFinancialChangeAsync(reserve.Id);
            if (locked != null)
                return locked;

            var rebate = _rebateService.GetRebateByCodeAsync(reserve, reserve.CoachService.AssistanceId, rebateCode.Trim());
            if (!rebate.IsSuccess)
                return new BaseResultDto(false, rebate.Messages);

            // پایه‌ی تخفیف همان OnlineAmount سرور است؛ تخفیف از مبلغ آنلاین کم می‌شود نه از باقی‌مانده‌ی حضوری.
            reserve.RebateId = rebate.Data.Id;
            reserve.RebatePrice = Math.Min(rebate.Data.FinalPrice, reserve.OnlineAmount);
            reserve.PaymentPrice = CoachReserveRules.PaymentAfterRebate(reserve.OnlineAmount, reserve.RebatePrice);
            if (reserve.FromWallet)
                reserve.WalletPrice = reserve.PaymentPrice;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task<BaseResultDto> ClearRebateAsync(long id, long bookerId)
        {
            var reserve = await _context.CoachReserves.AsTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.BookerId == bookerId && !r.IsReserved && !r.IsCancel);
            if (reserve == null || !reserve.RebateId.HasValue)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var locked = await GuardFinancialChangeAsync(reserve.Id);
            if (locked != null)
                return locked;
            reserve.RebateId = null;
            reserve.RebatePrice = 0;
            reserve.PaymentPrice = reserve.OnlineAmount;
            if (reserve.FromWallet)
                reserve.WalletPrice = reserve.PaymentPrice;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetWalletAsync(long id, bool fromWallet, long bookerId)
        {
            var reserve = await _context.CoachReserves.AsTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.BookerId == bookerId);
            if (reserve == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (reserve.IsCancel)
                return new BaseResultDto(false, Resource.Notification.CoachReserveCannotCancel);
            if (reserve.IsReserved || reserve.Status != (int)CoachReserveStatusEnum.Registered)
                return new BaseResultDto(false, Resource.Notification.ThisReserveIsPaid);
            var locked = await GuardFinancialChangeAsync(reserve.Id);
            if (locked != null)
                return locked;
            reserve.FromWallet = fromWallet;
            reserve.WalletPrice = fromWallet ? reserve.PaymentPrice : 0;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        private Task<BaseResultDto> GuardFinancialChangeAsync(long reserveId) =>
            PaymentAttemptRules.GuardUserRetryAsync(
                _context, PaymentCallbackTypeEnum.CoachReserve.ToString(), reserveId.ToString(),
                Resource.Notification.PaymentReserveAlreadyPaid);

        // ------------------------------------------------------------------ پرداخت

        // از داخل PaymentService.ApplyAndMapPaymentAsync (که تراکنش Serializable باز کرده) صدا زده می‌شود؛ بیرون از آن خودمان تراکنش باز می‌کنیم.
        public async Task<BaseResultDto> CoachReservePaymentCallback(long? reserveId, bool fromWallet = false)
        {
            if (_context.CurrentTransaction != null)
                return await PaymentCallbackCoreAsync(reserveId, fromWallet);

            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
            var result = await PaymentCallbackCoreAsync(reserveId, fromWallet);
            if (result.IsSuccess)
                await transaction.CommitAsync();
            else
                await transaction.RollbackAsync();
            return result;
        }

        private async Task<BaseResultDto> PaymentCallbackCoreAsync(long? reserveId, bool fromWallet)
        {
            try
            {
                var reserve = await _context.CoachReserves
                    .Include(r => r.Coach).Include(r => r.CoachService).Include(r => r.Rebate)
                    .AsTracking().FirstOrDefaultAsync(r => r.Id == reserveId);
                if (reserve == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (reserve.IsReserved)
                    return new BaseResultDto(true);
                if (reserve.IsCancel)
                    return new BaseResultDto(false, Resource.Notification.CoachReserveCannotCancel);

                var now = DateTime.Now;
                // بین ساخت رزرو و برگشت از درگاه ممکن است زمان نوبت گذشته باشد؛ اعمال نمی‌شود و پول درگاه به کیف پول برمی‌گردد.
                if (reserve.DoDate <= now)
                    return new BaseResultDto(false, Resource.Notification.CoachReserveDatePassed);
                if (!reserve.Coach.Active || !reserve.Coach.Approved || reserve.Coach.Deleted)
                    return new BaseResultDto(false, Resource.Notification.CoachReserveCoachUnavailable);

                // ظرفیت دوباره بررسی می‌شود (فقط رزروهای پرداخت‌شده؛ نگه‌داشت‌های موقت دیگران جلوی پرداخت موفق را نمی‌گیرند).
                var slot = await CheckSlotAsync(reserve.CoachId, reserve.DoDate, reserve.EndDate, reserve.AssigneeUserId, reserve.UserPetId,
                    includeHolds: false, excludeReserveId: reserve.Id, reserve.Coach.MaxConcurrentPets);
                if (slot.Error != null)
                    return new BaseResultDto(false, slot.Error);

                if (fromWallet && reserve.FromWallet && reserve.WalletPrice > 0)
                {
                    var wallet = await _walletService.InsertUpdateCoachReserveAsync(new WalletDto
                    {
                        Painding = false,
                        Amount = reserve.WalletPrice,
                        UserId = reserve.BookerId,
                        CoachReserveId = reserve.Id,
                    }, true);
                    if (!wallet.IsSuccess)
                        return new BaseResultDto(false);
                }
                if (reserve.Rebate != null)
                    _rebateService.IncreaseUseCount(reserve);

                reserve.IsReserved = true;
                reserve.Status = (int)CoachReserveStatusEnum.Paid;
                reserve.PaidDate = now;
                reserve.OwnerDecision = (int)CoachReserveOwnerDecisionEnum.Pending;
                reserve.OwnerDecisionDate = null;
                reserve.OwnerDecisionReason = null;
                reserve.OwnerApprovalDeadline = CoachReserveRules.Deadline(now, reserve.DoDate);

                await ApplyCommissionAsync(reserve);

                // امتیاز خرید (مثل پانسیون: هر ۱۰٬۰۰۰ تومان = ۱ امتیاز). نبودن ردیف نوع امتیاز نباید پرداخت را خراب کند.
                var earnedScore = Math.Floor(reserve.PaymentPrice / 10000);
                if (earnedScore > 0)
                {
                    try
                    {
                        await _scoreService.AddScoreAsync(reserve.BookerId, earnedScore, ScoreTransactionType.ScoreTransactionType_CoachReserve, reserve.Id.ToString());
                    }
                    catch (Exception scoreEx)
                    {
                        _logger.LogError(scoreEx, "Adding score for coach reserve {ReserveId} failed.", reserve.Id);
                    }
                }
                await _context.SaveChangesAsync();

                await _notifier.ReservePaidAsync(reserve.Id);
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Coach reserve payment callback failed for reserve {ReserveId}.", reserveId);
                return new BaseResultDto(false);
            }
        }

        // ------------------------------------------------------------------ تکمیل

        public async Task<BaseResultDto> CompleteAsync(long id, long? ownerUserId)
        {
            var reserve = await _context.CoachReserves.AsTracking()
                .Include(r => r.Coach)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (reserve == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && reserve.Coach.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            if (reserve.Status == (int)CoachReserveStatusEnum.Complete)
                return new BaseResultDto(false, Resource.Notification.CoachReserveAlreadyCompleted);
            if (!reserve.IsReserved || reserve.IsCancel)
                return new BaseResultDto(false, Resource.Notification.CoachReserveNotPaid);
            if (!CoachReserveRules.CanComplete(reserve.OwnerDecision))
                return new BaseResultDto(false, Resource.Notification.CoachReserveAwaitingOwnerApproval);
            if (reserve.DoDate > DateTime.Now)
                return new BaseResultDto(false, Resource.Notification.CoachReserveNotStartedYet);

            reserve.Status = (int)CoachReserveStatusEnum.Complete;
            reserve.DoneDate = DateTime.Now;
            await _context.SaveChangesAsync();
            await SafeAsync(() => _clubPoints.CoachReserveCompletedAsync(reserve.BookerId, reserve.Id), reserve.Id, "club points");
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        // ------------------------------------------------------------------ کمکی

        private sealed record PackageSet(List<PackageInfo> Packages, PackageInfo Primary, int DurationMinutes);

        /// <summary>
        /// بسته‌ی اصلی + بسته‌های اضافه‌ی یک رزرو: همه رزروشدنی، از یک مربی، هر خدمت حداکثر یک‌بار، حداکثر ۵ بسته؛ مجموع مدت از سقف یک روز کمتر.
        /// expectedCoachId پر باشد باید با مربیِ همه‌ی بسته‌ها یکی باشد.
        /// </summary>
        private async Task<(PackageSet Set, string Error)> BuildSetAsync(IReadOnlyList<long> packageIds, long? expectedCoachId, int? mode)
        {
            var ids = (packageIds ?? Array.Empty<long>()).Where(id => id > 0).Distinct().ToList();
            if (ids.Count == 0)
                return (null, Resource.Notification.CoachReservePackageInvalid);
            if (ids.Count > CoachReserveRules.MaxPackages)
                return (null, Resource.Notification.CoachReserveTooManyPackages);

            var packages = new List<PackageInfo>();
            foreach (var id in ids)
            {
                var package = await LoadBookablePackageAsync(id, mode);
                if (package == null || (expectedCoachId.HasValue && package.CoachId != expectedCoachId.Value))
                    return (null, Resource.Notification.CoachReservePackageInvalid);
                if (mode.HasValue && !package.OfferedInMode)
                    return (null, Resource.Notification.CoachReservePackageNotOffered);
                packages.Add(package);
            }
            if (packages.Select(p => p.CoachId).Distinct().Count() > 1)
                return (null, Resource.Notification.CoachReservePackageInvalid);
            if (packages.Select(p => p.CoachServiceId).Distinct().Count() != packages.Count)
                return (null, Resource.Notification.CoachReserveDuplicateService);

            var duration = packages.Sum(p => CoachReserveRules.EffectiveDuration(p.DurationMinutes));
            if (duration > CoachReserveRules.MaxTotalMinutes)
                return (null, Resource.Notification.CoachReserveTooLong);
            return (new PackageSet(packages, packages[0], duration), null);
        }

        /// <summary>
        /// سهم هر بسته: مبلغ پرداخت‌شده بین بسته‌ها (و هزینه‌ی دریافت/تحویل) به نسبت سهمشان از مبلغ آنلاین تقسیم می‌شود و کمیسیون هر بخش
        /// با درصد خودش (خدمت، یا پیش‌فرض مربی) حساب می‌شود؛ مجموع سهم‌ها همیشه دقیقاً برابر مبلغ پرداخت است.
        /// </summary>
        private async Task ApplyCommissionAsync(CoachReserve reserve)
        {
            var items = await _context.CoachReserveItems.AsTracking()
                .Where(i => i.CoachReserveId == reserve.Id).OrderBy(i => i.Id).ToListAsync();
            var coachPercent = reserve.Coach.CommissionPercent;
            var servicePercents = await _context.CoachServices.AsNoTracking()
                .Where(s => items.Select(i => i.CoachServiceId).Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.CommissionPercent);

            if (items.Count == 0)
            {
                // رزرو قدیمیِ بدون آیتم (نباید پیش بیاید): همان قاعده‌ی تک‌خدمتی
                reserve.CommissionPercent = CoachReserveRules.ResolveCommission(reserve.CoachService.CommissionPercent, coachPercent);
                (reserve.ProviderShare, reserve.SiteShare) = CoachReserveRules.Split(reserve.PaymentPrice, reserve.CommissionPercent);
                return;
            }

            var parts = items.Select(i => (Weight: i.OnlineAmount, Percent: CoachReserveRules.ResolveCommission(servicePercents.GetValueOrDefault(i.CoachServiceId), coachPercent))).ToList();

            var shares = CoachReserveRules.AllocateCommission(reserve.PaymentPrice, parts);
            for (var index = 0; index < items.Count; index++)
            {
                items[index].CommissionPercent = parts[index].Percent;
                items[index].ProviderShare = shares[index].Provider;
                items[index].SiteShare = shares[index].Site;
            }
            reserve.ProviderShare = shares.Sum(s => s.Provider);
            reserve.SiteShare = shares.Sum(s => s.Site);
            // درصد مؤثر کل رزرو (برای نمایش)
            reserve.CommissionPercent = reserve.PaymentPrice > 0
                ? Math.Round((decimal)reserve.SiteShare * 100m / (decimal)reserve.PaymentPrice, 2)
                : parts[0].Percent;
        }

        /// <summary>Price/PrePaymentPrice همان قیمتِ نحوه‌ی ارائه‌ی انتخاب‌شده است (بدون mode = قیمت پایه). OfferedInMode = false یعنی بسته در این حالت ارائه نمی‌شود.</summary>
        private sealed record PackageInfo(
            long Id, long CoachServiceId, long CoachId, double Price, double PrePaymentPrice, string PetSize,
            int DurationMinutes, bool ServiceAtHome, bool ServiceInPerson, bool ServiceOnline,
            int MaxConcurrentPets, long AssistanceId, long CityId, bool OfferedInMode, string Name);

        private async Task<PackageInfo> LoadBookablePackageAsync(long packageId, int? mode)
        {
            var row = await _context.CoachServicePackages.AsNoTracking()
                .Where(p => p.Id == packageId && !p.Deleted && p.Active
                            && !p.CoachService.Deleted && p.CoachService.Active && p.CoachService.Approved
                            && !p.CoachService.Coach.Deleted && p.CoachService.Coach.Active && p.CoachService.Coach.Approved)
                .Select(p => new
                {
                    p.Id, p.Name, p.CoachServiceId, CoachId = p.CoachService.CoachId, p.Price, p.PrePaymentPrice, p.PetSize,
                    p.CoachService.DurationMinutes, p.CoachService.Coach.ServiceAtHome, p.CoachService.Coach.ServiceInPerson,
                    p.CoachService.Coach.ServiceOnline, p.CoachService.Coach.MaxConcurrentPets,
                    p.CoachService.AssistanceId, p.CoachService.Coach.CityId,
                    Types = p.Types.Where(x => !x.Deleted).Select(x => new { x.Mode, x.Price, x.PrePaymentPrice }).ToList(),
                })
                .FirstOrDefaultAsync();
            if (row == null)
                return null;

            var price = row.Price;
            var pre = row.PrePaymentPrice;
            var offered = true;
            if (mode.HasValue)
            {
                var types = row.Types.Select(x => (x.Mode, x.Price, x.PrePaymentPrice)).ToList();
                var resolved = CoachReserveRules.ResolveModePrice(row.Price, row.PrePaymentPrice, types, mode.Value);
                offered = resolved.HasValue;
                if (resolved.HasValue)
                    (price, pre) = resolved.Value;
            }
            return new PackageInfo(
                row.Id, row.CoachServiceId, row.CoachId, price, pre, row.PetSize, row.DurationMinutes,
                row.ServiceAtHome, row.ServiceInPerson, row.ServiceOnline, row.MaxConcurrentPets, row.AssistanceId, row.CityId, offered, row.Name);
        }

        /// <summary>نحوه‌ی ارائه باید یکی از سه حالت باشد و مربی آن را ارائه دهد. null = معتبر.</summary>
        private static string ValidateOffered(PackageInfo package, int mode)
        {
            if (!Enum.IsDefined(typeof(CoachServiceModeEnum), mode))
                return Resource.Notification.CoachReserveModeInvalid;
            return (CoachServiceModeEnum)mode switch
            {
                CoachServiceModeEnum.Online => package.ServiceOnline ? null : Resource.Notification.CoachReserveOnlineUnavailable,
                CoachServiceModeEnum.InPerson => package.ServiceInPerson ? null : Resource.Notification.CoachReserveSalonServiceUnavailable,
                _ => package.ServiceAtHome ? null : Resource.Notification.CoachReserveHomeServiceUnavailable,
            };
        }

        /// <summary>null = معتبر.</summary>
        private async Task<string> ValidateModeAsync(CoachReserveInputDto dto, PackageInfo package, long bookerId)
        {
            var offered = ValidateOffered(package, dto.Mode);
            if (offered != null)
                return offered;
            if (dto.Mode == (int)CoachServiceModeEnum.InPlace)
            {
                if (!dto.AddressId.HasValue)
                    return Resource.Notification.CoachReserveAddressRequired;
                var address = await _context.Addresses.AsNoTracking()
                    .Where(a => a.Id == dto.AddressId.Value && a.UserId == bookerId && !a.Deleted)
                    .Select(a => new { a.CityId }).FirstOrDefaultAsync();
                if (address == null)
                    return Resource.Notification.CoachReserveAddressRequired;
                var zones = await _context.CoachZones.AsNoTracking()
                    .Where(z => z.CoachId == package.CoachId && !z.Deleted).Select(z => z.CityId).ToListAsync();
                var inZone = zones.Count > 0 ? zones.Contains(address.CityId) : address.CityId == package.CityId;
                return inZone ? null : Resource.Notification.CoachReserveAddressOutOfZone;
            }
            return null;
        }

        private async Task<string> ValidateAssigneeAsync(long? assigneeUserId, PackageInfo package)
        {
            if (!assigneeUserId.HasValue)
                return null;
            var serviceUsers = await _context.CoachServiceUsers.AsNoTracking()
                .Where(u => u.CoachServiceId == package.CoachServiceId && !u.Deleted && u.Active).Select(u => u.UserId).ToListAsync();
            if (serviceUsers.Count > 0)
                return serviceUsers.Contains(assigneeUserId.Value) ? null : Resource.Notification.CoachReserveAssigneeInvalid;

            var allowed = await _context.CoachUsers.AsNoTracking()
                .Where(u => u.CoachId == package.CoachId && !u.Deleted && u.Active && u.UserAccept == true).Select(u => u.UserId).ToListAsync();
            allowed.Add(await _context.Coaches.AsNoTracking().Where(g => g.Id == package.CoachId).Select(g => g.OwnerId).FirstAsync());
            return allowed.Contains(assigneeUserId.Value) ? null : Resource.Notification.CoachReserveAssigneeInvalid;
        }

        private sealed record SlotCheck(string Error, long? CoachTimeId);

        /// <summary>
        /// ساعت کاری، ظرفیت هم‌زمان، تداخل گرومر انتخابی و تداخل خودِ پت را بررسی می‌کند.
        /// includeHolds = رزروهای پرداخت‌نشده‌ی تازه هم ظرفیت را نگه می‌دارند (فقط هنگام ساخت).
        /// </summary>
        private async Task<SlotCheck> CheckSlotAsync(
            long coachId, DateTime start, DateTime end, long? assigneeUserId, long userPetId,
            bool includeHolds, long? excludeReserveId, int coachMaxConcurrent)
        {
            var weekDay = CoachReserveRules.WeekDayNumber(start);
            var rows = await _context.CoachTimes.AsNoTracking()
                .Where(t => t.CoachId == coachId && !t.Deleted && t.Active && t.WeekDay.Number == weekDay)
                .Select(t => new { t.Id, t.StartTime, t.EndTime, t.Capacity }).ToListAsync();

            var from = start.TimeOfDay;
            var to = end.TimeOfDay;
            var row = rows.FirstOrDefault(r =>
                CoachReserveRules.TryParseTime(r.StartTime, out var s) && CoachReserveRules.TryParseTime(r.EndTime, out var e) &&
                from >= s && to <= e && end.Date == start.Date);
            if (row == null)
                return new SlotCheck(Resource.Notification.CoachReserveOutsideWorkingHours, null);

            var now = DateTime.Now;
            var holdFrom = now - CoachReserveRules.UnpaidHold;
            var overlapping = await _context.CoachReserves.AsNoTracking()
                .Where(r => r.CoachId == coachId && !r.IsCancel && r.DoDate < end && r.EndDate > start
                            && (excludeReserveId == null || r.Id != excludeReserveId)
                            && (r.IsReserved || (includeHolds && r.CreateDate > holdFrom)))
                .Select(r => new { r.DoDate, r.EndDate, r.AssigneeUserId })
                .ToListAsync();

            var capacity = CoachReserveRules.EffectiveCapacity(row.Capacity, coachMaxConcurrent);
            var concurrent = CoachReserveRules.MaxConcurrency(overlapping.Select(r => (r.DoDate, r.EndDate)), start, end);
            if (concurrent >= capacity)
                return new SlotCheck(Resource.Notification.CoachReserveSlotFull, null);
            if (assigneeUserId.HasValue && overlapping.Any(r => r.AssigneeUserId == assigneeUserId))
                return new SlotCheck(Resource.Notification.CoachReserveSlotFull, null);

            // یک پت هم‌زمان در دو جا نوبت نمی‌گیرد (در هر مربیی؛ فقط رزروهای پرداخت‌شده).
            var petBusy = await _context.CoachReserves.AsNoTracking().AnyAsync(r =>
                r.UserPetId == userPetId && r.IsReserved && !r.IsCancel && r.DoDate < end && r.EndDate > start &&
                (excludeReserveId == null || r.Id != excludeReserveId));
            if (petBusy)
                return new SlotCheck(Resource.Notification.CoachReservePetBusy, null);

            return new SlotCheck(null, row.Id);
        }

        private IQueryable<CoachReserveVDto> Project(IQueryable<CoachReserve> query) =>
            query.Select(r => new CoachReserveVDto
            {
                Id = r.Id,
                ReserveCode = r.ReserveCode,
                CoachId = r.CoachId,
                CoachName = r.Coach.Name,
                CoachPhone = r.Coach.Phone,
                CoachAddress = r.Coach.AddressValue,
                CoachServiceId = r.CoachServiceId,
                ServiceName = r.CoachService.Assistance.Name,
                CoachServicePackageId = r.CoachServicePackageId,
                PackageName = r.CoachServicePackage.Name,
                PackagePetSize = r.CoachServicePackage.PetSize,
                BookerId = r.BookerId,
                BookerName = (r.Booker.FirstName + " " + r.Booker.LastName).Trim(),
                BookerMobile = r.Booker.Mobile,
                UserPetId = r.UserPetId,
                PetName = r.UserPet.Name,
                AssigneeUserId = r.AssigneeUserId,
                AssigneeName = r.Assignee == null ? null : (r.Assignee.FirstName + " " + r.Assignee.LastName).Trim(),
                DoDate = r.DoDate,
                EndDate = r.EndDate,
                DurationMinutes = r.DurationMinutes,
                Mode = r.Mode,
                AddressId = r.AddressId,
                AddressValue = r.Address == null ? null : r.Address.AddressValue,
                BookerNote = r.BookerNote,
                PackagePrice = r.PackagePrice,
                Price = r.Price,
                OnlineAmount = r.OnlineAmount,
                PayAtVenue = r.Price - r.OnlineAmount,
                FromWallet = r.FromWallet,
                WalletPrice = r.WalletPrice,
                RebateId = r.RebateId,
                RebatePrice = r.RebatePrice,
                PaymentPrice = r.PaymentPrice,
                Status = r.Status,
                IsReserved = r.IsReserved,
                IsCancel = r.IsCancel,
                CancelDetail = r.CancelDetail,
                CancelDate = r.CancelDate,
                CancelledByBooker = r.CancelledByBooker,
                CreateDate = r.CreateDate,
                PaidDate = r.PaidDate,
                DoneDate = r.DoneDate,
                OwnerDecision = r.OwnerDecision,
                OwnerDecisionDate = r.OwnerDecisionDate,
                OwnerDecisionReason = r.OwnerDecisionReason,
                OwnerApprovalDeadline = r.OwnerApprovalDeadline,
                ProviderShare = r.ProviderShare,
                SiteShare = r.SiteShare,
                CommissionPercent = r.CommissionPercent,
                Permitted = r.Permitted,
                RescheduleCount = r.RescheduleCount,
                OriginalDoDate = r.OriginalDoDate,
                CallStartDate = r.CallStartDate,
                CallEndDate = r.CallEndDate,
                Items = r.Items.OrderBy(i => i.Id).Select(i => new CoachReserveItemVDto
                {
                    Id = i.Id,
                    CoachServiceId = i.CoachServiceId,
                    ServiceName = i.CoachService.Assistance.Name,
                    CoachServicePackageId = i.CoachServicePackageId,
                    PackageName = i.CoachServicePackage.Name,
                    PackagePrice = i.PackagePrice,
                    OnlineAmount = i.OnlineAmount,
                    DurationMinutes = i.DurationMinutes,
                    CommissionPercent = i.CommissionPercent,
                    ProviderShare = i.ProviderShare,
                    SiteShare = i.SiteShare,
                }).ToList(),
            });

        private static void Finish(CoachReserveVDto row, DateTime now) =>
            row.FreeCancelAvailable = row.IsReserved && !row.IsCancel && row.Status != (int)CoachReserveStatusEnum.Complete
                                      && CoachReserveRules.IsFreeCancel(now, row.DoDate);

        private static BaseResultDto<CoachReserveVDto> Fail(string message) => new BaseResultDto<CoachReserveVDto>(false, message, null);

        private async Task SafeAsync(Func<Task> action, long reserveId, string name)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post-commit action {Action} failed for coach reserve {ReserveId}.", name, reserveId);
            }
        }
    }
}
