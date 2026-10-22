using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.Accounting.ScoreTransactionSrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveDebtSrv;
using Application.Services.GroomerSrvs.GroomerReserveSrv.Dto;
using Application.Services.GroomerSrvs.GroomerReserveSrv.Iface;
using Application.Services.Order.PaymentSrv;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Entities.Entities.GroomerField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.GroomerSrvs.GroomerReserveSrv
{
    public class GroomerReserveService : IGroomerReserveService
    {
        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly IRebateService _rebateService;
        private readonly IGroomerNotifier _notifier;
        private readonly IScoreTransactionService _scoreService;
        private readonly IClubPointIntegrationService _clubPoints;
        private readonly ILogger<GroomerReserveService> _logger;

        public GroomerReserveService(
            IDataBaseContext context,
            IWalletService walletService,
            IRebateService rebateService,
            IGroomerNotifier notifier,
            IScoreTransactionService scoreService,
            IClubPointIntegrationService clubPoints,
            ILogger<GroomerReserveService> logger)
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

        public Task<BaseResultDto<GroomerReserveQuoteDto>> QuoteAsync(long packageId, bool pickupRequested) =>
            QuoteAsync(new[] { packageId }, pickupRequested);

        public async Task<BaseResultDto<GroomerReserveQuoteDto>> QuoteAsync(IReadOnlyList<long> packageIds, bool pickupRequested)
        {
            var (set, error) = await BuildSetAsync(packageIds, null);
            if (error != null)
                return new BaseResultDto<GroomerReserveQuoteDto>(false, error, null);
            if (pickupRequested && !set.Primary.PickupAvailable)
                return new BaseResultDto<GroomerReserveQuoteDto>(false, Resource.Notification.GroomerReservePickupUnavailable, null);

            var pickupFee = pickupRequested ? set.Primary.PickupFee : 0;
            var (price, online) = GroomerReserveRules.Amounts(set.Packages.Select(p => (p.Price, p.PrePaymentPrice)), pickupFee);
            return new BaseResultDto<GroomerReserveQuoteDto>(true, new GroomerReserveQuoteDto
            {
                GroomerServicePackageId = set.Primary.Id,
                PackageIds = set.Packages.Select(p => p.Id).ToList(),
                PackagePrice = set.Packages.Sum(p => p.Price),
                PickupFee = pickupFee,
                Price = price,
                OnlineAmount = online,
                PayAtVenue = price - online,
                DurationMinutes = set.DurationMinutes,
            });
        }

        public async Task<BaseResultDto<GroomerAvailabilityDto>> GetAvailabilityAsync(
            long groomerId, DateTime date, long packageId, long? assigneeUserId, IReadOnlyList<long> extraPackageIds = null)
        {
            var (set, error) = await BuildSetAsync(new[] { packageId }.Concat(extraPackageIds ?? Array.Empty<long>()).ToList(), groomerId);
            if (error != null)
                return new BaseResultDto<GroomerAvailabilityDto>(false, error, null);
            var package = set.Primary;

            var day = date.Date;
            var duration = set.DurationMinutes;
            var result = new GroomerAvailabilityDto { Date = day, DurationMinutes = duration };

            var weekDay = GroomerReserveRules.WeekDayNumber(day);
            var rows = await _context.GroomerTimes.AsNoTracking()
                .Where(t => t.GroomerId == groomerId && !t.Deleted && t.Active && t.WeekDay.Number == weekDay)
                .Select(t => new { t.Id, t.StartTime, t.EndTime, t.Capacity })
                .ToListAsync();
            if (rows.Count == 0)
                return new BaseResultDto<GroomerAvailabilityDto>(true, result);

            var now = DateTime.Now;
            var holdFrom = now - GroomerReserveRules.UnpaidHold;
            var dayEnd = day.AddDays(1);
            var existing = await _context.GroomerReserves.AsNoTracking()
                .Where(r => r.GroomerId == groomerId && !r.IsCancel && r.DoDate < dayEnd && r.EndDate > day
                            && (r.IsReserved || r.CreateDate > holdFrom))
                .Select(r => new { r.DoDate, r.EndDate, r.AssigneeUserId })
                .ToListAsync();

            foreach (var row in rows.OrderBy(r => r.StartTime, StringComparer.Ordinal))
            {
                if (!GroomerReserveRules.TryParseTime(row.StartTime, out var rowStart) || !GroomerReserveRules.TryParseTime(row.EndTime, out var rowEnd))
                    continue;
                var capacity = GroomerReserveRules.EffectiveCapacity(row.Capacity, package.MaxConcurrentPets);
                for (var offset = rowStart; offset + TimeSpan.FromMinutes(duration) <= rowEnd; offset += TimeSpan.FromMinutes(GroomerReserveRules.SlotStepMinutes))
                {
                    var start = day + offset;
                    if (start < now + GroomerReserveRules.MinLead)
                        continue;
                    var end = start.AddMinutes(duration);
                    var overlapping = existing.Where(r => GroomerReserveRules.Overlaps(r.DoDate, r.EndDate, start, end)).ToList();
                    var concurrent = GroomerReserveRules.MaxConcurrency(overlapping.Select(r => (r.DoDate, r.EndDate)), start, end);
                    var available = Math.Max(0, capacity - concurrent);
                    if (assigneeUserId.HasValue && overlapping.Any(r => r.AssigneeUserId == assigneeUserId))
                        available = 0;
                    result.Slots.Add(new GroomerAvailabilitySlotDto
                    {
                        Time = $"{(int)offset.TotalHours:00}:{offset.Minutes:00}",
                        StartDate = start,
                        Capacity = capacity,
                        Available = available,
                    });
                }
            }
            return new BaseResultDto<GroomerAvailabilityDto>(true, result);
        }

        // ------------------------------------------------------------------ ساخت رزرو

        public async Task<BaseResultDto<GroomerReserveVDto>> CreateAsync(GroomerReserveInputDto dto, long bookerId)
        {
            try
            {
                if (dto == null || dto.GroomerId <= 0 || dto.GroomerServicePackageId <= 0 || dto.UserPetId <= 0 || bookerId <= 0)
                    return Fail(Resource.Notification.InvalidData);

                var now = DateTime.Now;
                if (await CompanionReserveDebtRules.IsLockedAsync(_context, bookerId, now))
                    return Fail(Resource.Notification.UnpaidDebtLocked);

                var (set, setError) = await BuildSetAsync(new[] { dto.GroomerServicePackageId }.Concat(dto.ExtraPackageIds ?? new List<long>()).ToList(), dto.GroomerId);
                if (setError != null)
                    return Fail(setError);
                var package = set.Primary;

                var pet = await _context.UserPets.AsNoTracking()
                    .Where(p => p.Id == dto.UserPetId && p.UserId == bookerId && !p.Deleted)
                    .Select(p => new { p.Id, p.PetId, p.Size })
                    .FirstOrDefaultAsync();
                if (pet == null)
                    return Fail(Resource.Notification.GroomerReservePetInvalid);

                var accepted = await _context.GroomerPets.AsNoTracking()
                    .Where(p => p.GroomerId == dto.GroomerId && !p.Deleted).Select(p => p.PetId).ToListAsync();
                if (accepted.Count > 0 && !accepted.Contains(pet.PetId))
                    return Fail(Resource.Notification.GroomerReservePetTypeNotAccepted);
                // هر بسته‌ی انتخابی باید با سایز حیوان بخواند.
                if (set.Packages.Any(p => !string.IsNullOrWhiteSpace(p.PetSize) &&
                                          !string.Equals(p.PetSize, pet.Size?.Trim(), StringComparison.OrdinalIgnoreCase)))
                    return Fail(Resource.Notification.GroomerReservePetSizeMismatch);

                var modeError = await ValidateModeAsync(dto, package, bookerId);
                if (modeError != null)
                    return Fail(modeError);

                var doDate = new DateTime(dto.DoDate.Year, dto.DoDate.Month, dto.DoDate.Day, dto.DoDate.Hour, dto.DoDate.Minute, 0);
                if (doDate < now + GroomerReserveRules.MinLead)
                    return Fail(Resource.Notification.GroomerReserveTimeInPast);
                var duration = set.DurationMinutes;
                var endDate = doDate.AddMinutes(duration);
                if (endDate.Date != doDate.Date)
                    return Fail(Resource.Notification.GroomerReserveOutsideWorkingHours);

                var assigneeError = await ValidateAssigneeAsync(dto.AssigneeUserId, package);
                if (assigneeError != null)
                    return Fail(assigneeError);

                var pickupRequested = dto.PickupRequested && !dto.IsHomeService;
                var pickupFee = pickupRequested ? package.PickupFee : 0;
                var (price, online) = GroomerReserveRules.Amounts(set.Packages.Select(p => (p.Price, p.PrePaymentPrice)), pickupFee);

                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);

                // نوبتِ پرداخت‌نشده‌ی قبلیِ همین کاربر برای همین پت در همین بازه، با نوبت جدید جایگزین می‌شود (کاربر پرداخت را رها کرده و دوباره می‌خواهد).
                var staleHolds = await _context.GroomerReserves.AsTracking()
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

                var slot = await CheckSlotAsync(dto.GroomerId, doDate, endDate, dto.AssigneeUserId, dto.UserPetId, includeHolds: true, excludeReserveId: null, package.MaxConcurrentPets);
                if (slot.Error != null)
                {
                    await transaction.RollbackAsync();
                    return Fail(slot.Error);
                }

                var reserve = new GroomerReserve
                {
                    GroomerId = dto.GroomerId,
                    GroomerServiceId = package.GroomerServiceId,
                    GroomerServicePackageId = package.Id,
                    BookerId = bookerId,
                    UserPetId = dto.UserPetId,
                    AssigneeUserId = dto.AssigneeUserId,
                    GroomerTimeId = slot.GroomerTimeId,
                    DoDate = doDate,
                    DurationMinutes = duration,
                    EndDate = endDate,
                    IsHomeService = dto.IsHomeService,
                    AddressId = dto.IsHomeService ? dto.AddressId : null,
                    PickupRequested = pickupRequested,
                    BookerNote = string.IsNullOrWhiteSpace(dto.BookerNote) ? null : dto.BookerNote.Trim(),
                    PackagePrice = set.Packages.Sum(p => p.Price),
                    PickupFee = pickupFee,
                    Price = price,
                    OnlineAmount = online,
                    PaymentPrice = online,
                    Status = (int)GroomerReserveStatusEnum.Registered,
                    OwnerDecision = (int)GroomerReserveOwnerDecisionEnum.NotRequired,
                    CreateDate = now,
                    Items = set.Packages.Select(p => new GroomerReserveItem
                    {
                        GroomerServiceId = p.GroomerServiceId,
                        GroomerServicePackageId = p.Id,
                        PackagePrice = p.Price,
                        OnlineAmount = GroomerReserveRules.Deposit(p.Price, p.PrePaymentPrice),
                        DurationMinutes = GroomerReserveRules.EffectiveDuration(p.DurationMinutes),
                    }).ToList(),
                };
                reserve.ReserveCode = PaymentCodeGenerator.Create(
                    PaymentCallbackTypeEnum.GroomerReserve, now, await _context.GetNextBusinessCodeNumberAsync());

                await _context.GroomerReserves.AddAsync(reserve);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (await FindAsync(reserve.Id, bookerId, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating groomer reserve failed for groomer {GroomerId}, booker {BookerId}.", dto?.GroomerId, bookerId);
                return Fail(Resource.Notification.Unsuccess);
            }
        }

        // ------------------------------------------------------------------ تغییر زمان

        public async Task<BaseResultDto<GroomerReserveVDto>> RescheduleAsync(long id, DateTime newDoDate, long? ownerUserId)
        {
            try
            {
                var now = DateTime.Now;
                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                var reserve = await _context.GroomerReserves.AsTracking()
                    .Include(r => r.Groomer)
                    .FirstOrDefaultAsync(r => r.Id == id);
                if (reserve == null)
                {
                    await transaction.RollbackAsync();
                    return Fail(Resource.Notification.NothingFound);
                }
                if (ownerUserId.HasValue && reserve.Groomer.OwnerId != ownerUserId.Value)
                {
                    await transaction.RollbackAsync();
                    return Fail(Resource.Notification.AccessDenied);
                }
                if (!reserve.IsReserved || reserve.IsCancel || reserve.Status == (int)GroomerReserveStatusEnum.Complete)
                {
                    await transaction.RollbackAsync();
                    return Fail(Resource.Notification.GroomerReserveRescheduleNotAllowed);
                }

                var start = new DateTime(newDoDate.Year, newDoDate.Month, newDoDate.Day, newDoDate.Hour, newDoDate.Minute, 0);
                if (start == reserve.DoDate)
                {
                    await transaction.RollbackAsync();
                    return Fail(Resource.Notification.GroomerReserveRescheduleSameTime);
                }
                if (start < now + GroomerReserveRules.MinLead)
                {
                    await transaction.RollbackAsync();
                    return Fail(Resource.Notification.GroomerReserveTimeInPast);
                }
                var end = start.AddMinutes(reserve.DurationMinutes);
                var slot = await CheckSlotAsync(reserve.GroomerId, start, end, reserve.AssigneeUserId, reserve.UserPetId,
                    includeHolds: false, excludeReserveId: reserve.Id, reserve.Groomer.MaxConcurrentPets);
                if (slot.Error != null)
                {
                    await transaction.RollbackAsync();
                    return Fail(slot.Error);
                }

                reserve.OriginalDoDate ??= reserve.DoDate;
                reserve.DoDate = start;
                reserve.EndDate = end;
                reserve.GroomerTimeId = slot.GroomerTimeId;
                reserve.RescheduleCount++;
                reserve.LastRescheduleDate = now;
                // مهلت پاسخ آرایشگاه به نوبتِ جدید نسبت داده می‌شود.
                if (GroomerReserveRules.IsPending(reserve.OwnerDecision))
                    reserve.OwnerApprovalDeadline = GroomerReserveRules.Deadline(now, start);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _notifier.ReserveRescheduledAsync(reserve.Id, byOwner: ownerUserId.HasValue);
                return await FindAsync(reserve.Id, null, ownerUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rescheduling groomer reserve {ReserveId} failed.", id);
                return Fail(Resource.Notification.Unsuccess);
            }
        }

        // ------------------------------------------------------------------ جستجو و نمایش

        public GroomerReservePage Search(GroomerReserveSearchInputDto input)
        {
            var query = _context.GroomerReserves.AsNoTracking().AsQueryable();
            if (input.BookerId.HasValue)
                query = query.Where(r => r.BookerId == input.BookerId.Value);
            if (input.GroomerId.HasValue)
                query = query.Where(r => r.GroomerId == input.GroomerId.Value);
            if (input.ProviderId.HasValue)
                query = query.Where(r => r.Groomer.ProviderId == input.ProviderId.Value);
            if (input.OwnerUserId.HasValue)
                query = query.Where(r => r.Groomer.OwnerId == input.OwnerUserId.Value);
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
                                         r.Booker.Mobile.Contains(q) || r.Groomer.Name.Contains(q));
            }

            query = input.SortBy == SortEnum.Old ? query.OrderBy(r => r.Id) : query.OrderByDescending(r => r.Id);
            var total = query.Count();
            var rows = Project(query.Skip((input.PageIndex - 1) * input.PageSize).Take(input.PageSize)).ToList();
            var now = DateTime.Now;
            foreach (var row in rows)
                Finish(row, now);
            return new GroomerReservePage { PageIndex = input.PageIndex, PageSize = input.PageSize, TotalCount = total, List = rows };
        }

        public async Task<BaseResultDto<GroomerReserveVDto>> FindAsync(long id, long? bookerId, long? ownerUserId)
        {
            var query = _context.GroomerReserves.AsNoTracking().Where(r => r.Id == id);
            if (bookerId.HasValue)
                query = query.Where(r => r.BookerId == bookerId.Value);
            if (ownerUserId.HasValue)
                query = query.Where(r => r.Groomer.OwnerId == ownerUserId.Value);
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
            return new BaseResultDto<GroomerReserveVDto>(true, item);
        }

        // ------------------------------------------------------------------ تخفیف و کیف پول

        public async Task<BaseResultDto> SetRebateCodeAsync(long id, string rebateCode, long bookerId)
        {
            if (string.IsNullOrWhiteSpace(rebateCode))
                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            var reserve = await _context.GroomerReserves.AsTracking()
                .Include(r => r.GroomerService)
                .FirstOrDefaultAsync(r => r.Id == id && r.BookerId == bookerId && !r.IsReserved && !r.IsCancel);
            if (reserve == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var locked = await GuardFinancialChangeAsync(reserve.Id);
            if (locked != null)
                return locked;

            var rebate = _rebateService.GetRebateByCodeAsync(reserve, reserve.GroomerService.AssistanceId, rebateCode.Trim());
            if (!rebate.IsSuccess)
                return new BaseResultDto(false, rebate.Messages);

            // پایه‌ی تخفیف همان OnlineAmount سرور است؛ تخفیف از مبلغ آنلاین کم می‌شود نه از باقی‌مانده‌ی حضوری.
            reserve.RebateId = rebate.Data.Id;
            reserve.RebatePrice = Math.Min(rebate.Data.FinalPrice, reserve.OnlineAmount);
            reserve.PaymentPrice = GroomerReserveRules.PaymentAfterRebate(reserve.OnlineAmount, reserve.RebatePrice);
            if (reserve.FromWallet)
                reserve.WalletPrice = reserve.PaymentPrice;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task<BaseResultDto> ClearRebateAsync(long id, long bookerId)
        {
            var reserve = await _context.GroomerReserves.AsTracking()
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
            var reserve = await _context.GroomerReserves.AsTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.BookerId == bookerId);
            if (reserve == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (reserve.IsCancel)
                return new BaseResultDto(false, Resource.Notification.GroomerReserveCannotCancel);
            if (reserve.IsReserved || reserve.Status != (int)GroomerReserveStatusEnum.Registered)
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
                _context, PaymentCallbackTypeEnum.GroomerReserve.ToString(), reserveId.ToString(),
                Resource.Notification.PaymentReserveAlreadyPaid);

        // ------------------------------------------------------------------ پرداخت

        // از داخل PaymentService.ApplyAndMapPaymentAsync (که تراکنش Serializable باز کرده) صدا زده می‌شود؛ بیرون از آن خودمان تراکنش باز می‌کنیم.
        public async Task<BaseResultDto> GroomerReservePaymentCallback(long? reserveId, bool fromWallet = false)
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
                var reserve = await _context.GroomerReserves
                    .Include(r => r.Groomer).Include(r => r.GroomerService).Include(r => r.Rebate)
                    .AsTracking().FirstOrDefaultAsync(r => r.Id == reserveId);
                if (reserve == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (reserve.IsReserved)
                    return new BaseResultDto(true);
                if (reserve.IsCancel)
                    return new BaseResultDto(false, Resource.Notification.GroomerReserveCannotCancel);

                var now = DateTime.Now;
                // بین ساخت رزرو و برگشت از درگاه ممکن است زمان نوبت گذشته باشد؛ اعمال نمی‌شود و پول درگاه به کیف پول برمی‌گردد.
                if (reserve.DoDate <= now)
                    return new BaseResultDto(false, Resource.Notification.GroomerReserveDatePassed);
                if (!reserve.Groomer.Active || !reserve.Groomer.Approved || reserve.Groomer.Deleted)
                    return new BaseResultDto(false, Resource.Notification.GroomerReserveGroomerUnavailable);

                // ظرفیت دوباره بررسی می‌شود (فقط رزروهای پرداخت‌شده؛ نگه‌داشت‌های موقت دیگران جلوی پرداخت موفق را نمی‌گیرند).
                var slot = await CheckSlotAsync(reserve.GroomerId, reserve.DoDate, reserve.EndDate, reserve.AssigneeUserId, reserve.UserPetId,
                    includeHolds: false, excludeReserveId: reserve.Id, reserve.Groomer.MaxConcurrentPets);
                if (slot.Error != null)
                    return new BaseResultDto(false, slot.Error);

                if (fromWallet && reserve.FromWallet && reserve.WalletPrice > 0)
                {
                    var wallet = await _walletService.InsertUpdateGroomerReserveAsync(new WalletDto
                    {
                        Painding = false,
                        Amount = reserve.WalletPrice,
                        UserId = reserve.BookerId,
                        GroomerReserveId = reserve.Id,
                    }, true);
                    if (!wallet.IsSuccess)
                        return new BaseResultDto(false);
                }
                if (reserve.Rebate != null)
                    _rebateService.IncreaseUseCount(reserve);

                reserve.IsReserved = true;
                reserve.Status = (int)GroomerReserveStatusEnum.Paid;
                reserve.PaidDate = now;
                reserve.OwnerDecision = (int)GroomerReserveOwnerDecisionEnum.Pending;
                reserve.OwnerDecisionDate = null;
                reserve.OwnerDecisionReason = null;
                reserve.OwnerApprovalDeadline = GroomerReserveRules.Deadline(now, reserve.DoDate);

                await ApplyCommissionAsync(reserve);

                // امتیاز خرید (مثل پانسیون: هر ۱۰٬۰۰۰ تومان = ۱ امتیاز). نبودن ردیف نوع امتیاز نباید پرداخت را خراب کند.
                var earnedScore = Math.Floor(reserve.PaymentPrice / 10000);
                if (earnedScore > 0)
                {
                    try
                    {
                        await _scoreService.AddScoreAsync(reserve.BookerId, earnedScore, ScoreTransactionType.ScoreTransactionType_GroomerReserve, reserve.Id.ToString());
                    }
                    catch (Exception scoreEx)
                    {
                        _logger.LogError(scoreEx, "Adding score for groomer reserve {ReserveId} failed.", reserve.Id);
                    }
                }
                await _context.SaveChangesAsync();

                await _notifier.ReservePaidAsync(reserve.Id);
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groomer reserve payment callback failed for reserve {ReserveId}.", reserveId);
                return new BaseResultDto(false);
            }
        }

        // ------------------------------------------------------------------ تکمیل

        public async Task<BaseResultDto> CompleteAsync(long id, long? ownerUserId)
        {
            var reserve = await _context.GroomerReserves.AsTracking()
                .Include(r => r.Groomer)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (reserve == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && reserve.Groomer.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            if (reserve.Status == (int)GroomerReserveStatusEnum.Complete)
                return new BaseResultDto(false, Resource.Notification.GroomerReserveAlreadyCompleted);
            if (!reserve.IsReserved || reserve.IsCancel)
                return new BaseResultDto(false, Resource.Notification.GroomerReserveNotPaid);
            if (!GroomerReserveRules.CanComplete(reserve.OwnerDecision))
                return new BaseResultDto(false, Resource.Notification.GroomerReserveAwaitingOwnerApproval);
            if (reserve.DoDate > DateTime.Now)
                return new BaseResultDto(false, Resource.Notification.GroomerReserveNotStartedYet);

            reserve.Status = (int)GroomerReserveStatusEnum.Complete;
            reserve.DoneDate = DateTime.Now;
            await _context.SaveChangesAsync();
            await SafeAsync(() => _clubPoints.GroomerReserveCompletedAsync(reserve.BookerId, reserve.Id), reserve.Id, "club points");
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        // ------------------------------------------------------------------ کمکی

        private sealed record PackageSet(List<PackageInfo> Packages, PackageInfo Primary, int DurationMinutes);

        /// <summary>
        /// بسته‌ی اصلی + بسته‌های اضافه‌ی یک رزرو: همه رزروشدنی، از یک آرایشگاه، هر خدمت حداکثر یک‌بار، حداکثر ۵ بسته؛ مجموع مدت از سقف یک روز کمتر.
        /// expectedGroomerId پر باشد باید با آرایشگاهِ همه‌ی بسته‌ها یکی باشد.
        /// </summary>
        private async Task<(PackageSet Set, string Error)> BuildSetAsync(IReadOnlyList<long> packageIds, long? expectedGroomerId)
        {
            var requested = (packageIds ?? Array.Empty<long>()).Where(id => id > 0).ToList();
            var ids = requested.Distinct().ToList();
            // یکتایی بر اساس شناسه‌ی «بسته» است: چند بسته از یک خدمت مجاز است، ولی تکرار همان بسته رد می‌شود.
            if (ids.Count != requested.Count)
                return (null, Resource.Notification.GroomerReserveDuplicateService);
            if (ids.Count == 0)
                return (null, Resource.Notification.GroomerReservePackageInvalid);
            if (ids.Count > GroomerReserveRules.MaxPackages)
                return (null, Resource.Notification.GroomerReserveTooManyPackages);

            var packages = new List<PackageInfo>();
            foreach (var id in ids)
            {
                var package = await LoadBookablePackageAsync(id);
                if (package == null || (expectedGroomerId.HasValue && package.GroomerId != expectedGroomerId.Value))
                    return (null, Resource.Notification.GroomerReservePackageInvalid);
                packages.Add(package);
            }
            if (packages.Select(p => p.GroomerId).Distinct().Count() > 1)
                return (null, Resource.Notification.GroomerReservePackageInvalid);
            var duration = packages.Sum(p => GroomerReserveRules.EffectiveDuration(p.DurationMinutes));
            if (duration > GroomerReserveRules.MaxTotalMinutes)
                return (null, Resource.Notification.GroomerReserveTooLong);
            return (new PackageSet(packages, packages[0], duration), null);
        }

        /// <summary>
        /// سهم هر بسته: مبلغ پرداخت‌شده بین بسته‌ها (و هزینه‌ی دریافت/تحویل) به نسبت سهمشان از مبلغ آنلاین تقسیم می‌شود و کمیسیون هر بخش
        /// با درصد خودش (خدمت، یا پیش‌فرض آرایشگاه) حساب می‌شود؛ مجموع سهم‌ها همیشه دقیقاً برابر مبلغ پرداخت است.
        /// </summary>
        private async Task ApplyCommissionAsync(GroomerReserve reserve)
        {
            var items = await _context.GroomerReserveItems.AsTracking()
                .Where(i => i.GroomerReserveId == reserve.Id).OrderBy(i => i.Id).ToListAsync();
            var groomerPercent = reserve.Groomer.CommissionPercent;
            var servicePercents = await _context.GroomerServices.AsNoTracking()
                .Where(s => items.Select(i => i.GroomerServiceId).Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.CommissionPercent);

            if (items.Count == 0)
            {
                // رزرو قدیمیِ بدون آیتم (نباید پیش بیاید): همان قاعده‌ی تک‌خدمتی
                reserve.CommissionPercent = GroomerReserveRules.ResolveCommission(reserve.GroomerService.CommissionPercent, groomerPercent);
                (reserve.ProviderShare, reserve.SiteShare) = GroomerReserveRules.Split(reserve.PaymentPrice, reserve.CommissionPercent);
                return;
            }

            var parts = items.Select(i => (Weight: i.OnlineAmount, Percent: GroomerReserveRules.ResolveCommission(servicePercents.GetValueOrDefault(i.GroomerServiceId), groomerPercent))).ToList();
            if (reserve.PickupFee > 0)
                parts.Add((reserve.PickupFee, groomerPercent));

            var shares = GroomerReserveRules.AllocateCommission(reserve.PaymentPrice, parts);
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

        private sealed record PackageInfo(
            long Id, long GroomerServiceId, long GroomerId, double Price, double PrePaymentPrice, string PetSize,
            int DurationMinutes, bool ServiceAtHome, bool ServiceInSalon, bool PickupAvailable, double PickupFee,
            int MaxConcurrentPets, long AssistanceId, long CityId);

        private Task<PackageInfo> LoadBookablePackageAsync(long packageId) =>
            _context.GroomerServicePackages.AsNoTracking()
                .Where(p => p.Id == packageId && !p.Deleted && p.Active
                            && !p.GroomerService.Deleted && p.GroomerService.Active && p.GroomerService.Approved
                            && !p.GroomerService.Groomer.Deleted && p.GroomerService.Groomer.Active && p.GroomerService.Groomer.Approved)
                .Select(p => new PackageInfo(
                    p.Id, p.GroomerServiceId, p.GroomerService.GroomerId, p.Price, p.PrePaymentPrice, p.PetSize,
                    p.GroomerService.DurationMinutes, p.GroomerService.Groomer.ServiceAtHome, p.GroomerService.Groomer.ServiceInSalon,
                    p.GroomerService.Groomer.PickupAvailable, p.GroomerService.Groomer.PickupFee,
                    p.GroomerService.Groomer.MaxConcurrentPets, p.GroomerService.AssistanceId, p.GroomerService.Groomer.CityId))
                .FirstOrDefaultAsync();

        /// <summary>null = معتبر.</summary>
        private async Task<string> ValidateModeAsync(GroomerReserveInputDto dto, PackageInfo package, long bookerId)
        {
            if (dto.IsHomeService)
            {
                if (!package.ServiceAtHome)
                    return Resource.Notification.GroomerReserveHomeServiceUnavailable;
                if (dto.PickupRequested)
                    return Resource.Notification.GroomerReservePickupUnavailable;
                if (!dto.AddressId.HasValue)
                    return Resource.Notification.GroomerReserveAddressRequired;
                var address = await _context.Addresses.AsNoTracking()
                    .Where(a => a.Id == dto.AddressId.Value && a.UserId == bookerId && !a.Deleted)
                    .Select(a => new { a.CityId }).FirstOrDefaultAsync();
                if (address == null)
                    return Resource.Notification.GroomerReserveAddressRequired;
                var zones = await _context.GroomerZones.AsNoTracking()
                    .Where(z => z.GroomerId == package.GroomerId && !z.Deleted).Select(z => z.CityId).ToListAsync();
                var inZone = zones.Count > 0 ? zones.Contains(address.CityId) : address.CityId == package.CityId;
                return inZone ? null : Resource.Notification.GroomerReserveAddressOutOfZone;
            }

            if (!package.ServiceInSalon)
                return Resource.Notification.GroomerReserveSalonServiceUnavailable;
            if (dto.PickupRequested && !package.PickupAvailable)
                return Resource.Notification.GroomerReservePickupUnavailable;
            return null;
        }

        private async Task<string> ValidateAssigneeAsync(long? assigneeUserId, PackageInfo package)
        {
            if (!assigneeUserId.HasValue)
                return null;
            var serviceUsers = await _context.GroomerServiceUsers.AsNoTracking()
                .Where(u => u.GroomerServiceId == package.GroomerServiceId && !u.Deleted && u.Active).Select(u => u.UserId).ToListAsync();
            if (serviceUsers.Count > 0)
                return serviceUsers.Contains(assigneeUserId.Value) ? null : Resource.Notification.GroomerReserveAssigneeInvalid;

            var allowed = await _context.GroomerUsers.AsNoTracking()
                .Where(u => u.GroomerId == package.GroomerId && !u.Deleted && u.Active && u.UserAccept == true).Select(u => u.UserId).ToListAsync();
            allowed.Add(await _context.Groomers.AsNoTracking().Where(g => g.Id == package.GroomerId).Select(g => g.OwnerId).FirstAsync());
            return allowed.Contains(assigneeUserId.Value) ? null : Resource.Notification.GroomerReserveAssigneeInvalid;
        }

        private sealed record SlotCheck(string Error, long? GroomerTimeId);

        /// <summary>
        /// ساعت کاری، ظرفیت هم‌زمان، تداخل گرومر انتخابی و تداخل خودِ پت را بررسی می‌کند.
        /// includeHolds = رزروهای پرداخت‌نشده‌ی تازه هم ظرفیت را نگه می‌دارند (فقط هنگام ساخت).
        /// </summary>
        private async Task<SlotCheck> CheckSlotAsync(
            long groomerId, DateTime start, DateTime end, long? assigneeUserId, long userPetId,
            bool includeHolds, long? excludeReserveId, int groomerMaxConcurrent)
        {
            var weekDay = GroomerReserveRules.WeekDayNumber(start);
            var rows = await _context.GroomerTimes.AsNoTracking()
                .Where(t => t.GroomerId == groomerId && !t.Deleted && t.Active && t.WeekDay.Number == weekDay)
                .Select(t => new { t.Id, t.StartTime, t.EndTime, t.Capacity }).ToListAsync();

            var from = start.TimeOfDay;
            var to = end.TimeOfDay;
            var row = rows.FirstOrDefault(r =>
                GroomerReserveRules.TryParseTime(r.StartTime, out var s) && GroomerReserveRules.TryParseTime(r.EndTime, out var e) &&
                from >= s && to <= e && end.Date == start.Date);
            if (row == null)
                return new SlotCheck(Resource.Notification.GroomerReserveOutsideWorkingHours, null);

            var now = DateTime.Now;
            var holdFrom = now - GroomerReserveRules.UnpaidHold;
            var overlapping = await _context.GroomerReserves.AsNoTracking()
                .Where(r => r.GroomerId == groomerId && !r.IsCancel && r.DoDate < end && r.EndDate > start
                            && (excludeReserveId == null || r.Id != excludeReserveId)
                            && (r.IsReserved || (includeHolds && r.CreateDate > holdFrom)))
                .Select(r => new { r.DoDate, r.EndDate, r.AssigneeUserId })
                .ToListAsync();

            var capacity = GroomerReserveRules.EffectiveCapacity(row.Capacity, groomerMaxConcurrent);
            var concurrent = GroomerReserveRules.MaxConcurrency(overlapping.Select(r => (r.DoDate, r.EndDate)), start, end);
            if (concurrent >= capacity)
                return new SlotCheck(Resource.Notification.GroomerReserveSlotFull, null);
            if (assigneeUserId.HasValue && overlapping.Any(r => r.AssigneeUserId == assigneeUserId))
                return new SlotCheck(Resource.Notification.GroomerReserveSlotFull, null);

            // یک پت هم‌زمان در دو جا نوبت نمی‌گیرد (در هر آرایشگاهی؛ فقط رزروهای پرداخت‌شده).
            var petBusy = await _context.GroomerReserves.AsNoTracking().AnyAsync(r =>
                r.UserPetId == userPetId && r.IsReserved && !r.IsCancel && r.DoDate < end && r.EndDate > start &&
                (excludeReserveId == null || r.Id != excludeReserveId));
            if (petBusy)
                return new SlotCheck(Resource.Notification.GroomerReservePetBusy, null);

            return new SlotCheck(null, row.Id);
        }

        private IQueryable<GroomerReserveVDto> Project(IQueryable<GroomerReserve> query) =>
            query.Select(r => new GroomerReserveVDto
            {
                Id = r.Id,
                ReserveCode = r.ReserveCode,
                GroomerId = r.GroomerId,
                GroomerName = r.Groomer.Name,
                GroomerPhone = r.Groomer.Phone,
                GroomerAddress = r.Groomer.AddressValue,
                GroomerServiceId = r.GroomerServiceId,
                ServiceName = r.GroomerService.Assistance.Name,
                GroomerServicePackageId = r.GroomerServicePackageId,
                PackageName = r.GroomerServicePackage.Name,
                PackagePetSize = r.GroomerServicePackage.PetSize,
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
                IsHomeService = r.IsHomeService,
                AddressId = r.AddressId,
                AddressValue = r.Address == null ? null : r.Address.AddressValue,
                PickupRequested = r.PickupRequested,
                BookerNote = r.BookerNote,
                PackagePrice = r.PackagePrice,
                PickupFee = r.PickupFee,
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
                Items = r.Items.OrderBy(i => i.Id).Select(i => new GroomerReserveItemVDto
                {
                    Id = i.Id,
                    GroomerServiceId = i.GroomerServiceId,
                    ServiceName = i.GroomerService.Assistance.Name,
                    GroomerServicePackageId = i.GroomerServicePackageId,
                    PackageName = i.GroomerServicePackage.Name,
                    PackagePrice = i.PackagePrice,
                    OnlineAmount = i.OnlineAmount,
                    DurationMinutes = i.DurationMinutes,
                    CommissionPercent = i.CommissionPercent,
                    ProviderShare = i.ProviderShare,
                    SiteShare = i.SiteShare,
                }).ToList(),
            });

        private static void Finish(GroomerReserveVDto row, DateTime now) =>
            row.FreeCancelAvailable = row.IsReserved && !row.IsCancel && row.Status != (int)GroomerReserveStatusEnum.Complete
                                      && GroomerReserveRules.IsFreeCancel(now, row.DoDate);

        private static BaseResultDto<GroomerReserveVDto> Fail(string message) => new BaseResultDto<GroomerReserveVDto>(false, message, null);

        private async Task SafeAsync(Func<Task> action, long reserveId, string name)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post-commit action {Action} failed for groomer reserve {ReserveId}.", name, reserveId);
            }
        }
    }
}
