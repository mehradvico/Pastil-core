using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv;
using Application.Services.CompanionSrvs.CompanionReservePackageItemSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReservePackageItemSrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReservePackageItemSrv
{
    // تأیید/لغو تکی پکیج‌های رزرو نماینده + افزودن/حذف پکیج + بازپرداخت سهم پکیج لغوشده به کیف پول کاربر.
    // طراحی: backend/Docs/COMPANION_RESERVE_PACKAGE_ITEMS_FA.md
    public class CompanionReservePackageItemService : ICompanionReservePackageItemService
    {
        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly IPushNotificationService _pushService;
        private readonly IClubPointIntegrationService _clubPointIntegrationService;
        private readonly Application.Services.TripSrv.TripSrv.Iface.ITripService _tripService;
        private readonly ILogger<CompanionReservePackageItemService> _logger;

        public CompanionReservePackageItemService(
            IDataBaseContext context,
            IWalletService walletService,
            IPushNotificationService pushService,
            IClubPointIntegrationService clubPointIntegrationService,
            Application.Services.TripSrv.TripSrv.Iface.ITripService tripService,
            ILogger<CompanionReservePackageItemService> logger)
        {
            _context = context;
            _walletService = walletService;
            _pushService = pushService;
            _clubPointIntegrationService = clubPointIntegrationService;
            _tripService = tripService;
            _logger = logger;
        }

        // ---------------------------------------------------------------- نمایش

        public async Task<CompanionReservePackageItemsVDto> GetViewAsync(long reserveId)
        {
            var reserve = await _context.CompanionReserves.AsNoTracking()
                .Include(r => r.UserPets).Include(r => r.CompanionAssistancePackages)
                .FirstOrDefaultAsync(r => r.Id == reserveId);
            if (reserve == null)
                return new CompanionReservePackageItemsVDto { ReserveId = reserveId };

            var rows = await _context.CompanionReservePackageItems.AsNoTracking()
                .Where(i => i.CompanionReserveId == reserveId).OrderBy(i => i.Id).ToListAsync();
            var have = rows.Select(i => i.CompanionAssistancePackageId).ToHashSet();
            var missing = reserve.CompanionAssistancePackages.Where(p => !have.Contains(p.Id)).ToList();
            var virtualRows = missing.Any() ? await BuildRowsAsync(reserve, missing) : new List<CompanionReservePackageItem>();

            var items = rows.Concat(virtualRows).Select(ToVDto).ToList();
            return new CompanionReservePackageItemsVDto
            {
                ReserveId = reserveId,
                CanManage = reserve.IsReserved && CompanionReservePackageItemRules.ReserveIsModifiable(reserve.IsCancel, reserve.StateId, reserve.OperatorStateId, reserve.DoneDate),
                Items = items,
                Summary = Summarize(items)
            };
        }

        public async Task<BaseResultDto<CompanionReservePackageItemsVDto>> GetForBookerAsync(long reserveId, long bookerId)
        {
            var owns = await _context.CompanionReserves.AsNoTracking().AnyAsync(r => r.Id == reserveId && r.BookerId == bookerId);
            if (!owns)
                return new BaseResultDto<CompanionReservePackageItemsVDto>(false, Resource.Notification.NothingFound, null);
            return new BaseResultDto<CompanionReservePackageItemsVDto>(true, await GetViewAsync(reserveId));
        }

        public async Task<BaseResultDto<CompanionReservePackageItemsVDto>> GetForManagerAsync(long reserveId, long actorUserId, bool asAdmin)
        {
            var reserve = await LoadAsync(reserveId, tracking: false);
            if (reserve == null)
                return new BaseResultDto<CompanionReservePackageItemsVDto>(false, Resource.Notification.NothingFound, null);
            if (!await CanManageAsync(reserve, actorUserId, asAdmin))
                return new BaseResultDto<CompanionReservePackageItemsVDto>(false, Resource.Notification.AccessDenied, null);
            return new BaseResultDto<CompanionReservePackageItemsVDto>(true, await GetViewAsync(reserveId));
        }

        // ---------------------------------------------------------------- تأیید / لغو

        public async Task<BaseResultDto<CompanionReservePackageStatusResultVDto>> SetStatusAsync(long reserveId, long packageId, CompanionReservePackageStatusDto dto, long actorUserId, bool asAdmin)
        {
            try
            {
                if (dto == null || !CompanionReservePackageItemRules.IsValidTarget(dto.Status))
                    return Fail<CompanionReservePackageStatusResultVDto>(Resource.Notification.CompanionReservePackageStatusInvalid);

                var reserve = await LoadAsync(reserveId, tracking: true);
                if (reserve == null)
                    return Fail<CompanionReservePackageStatusResultVDto>(Resource.Notification.NothingFound);
                if (!await CanManageAsync(reserve, actorUserId, asAdmin))
                    return Fail<CompanionReservePackageStatusResultVDto>(Resource.Notification.AccessDenied);
                if (!reserve.IsReserved)
                    return Fail<CompanionReservePackageStatusResultVDto>(Resource.Notification.CompanionReservePackageNeedsPaidReserve);
                if (!CompanionReservePackageItemRules.ReserveIsModifiable(reserve.IsCancel, reserve.StateId, reserve.OperatorStateId, reserve.DoneDate))
                    return Fail<CompanionReservePackageStatusResultVDto>(Resource.Notification.CompanionReservePackageNotModifiable);

                var cancelling = dto.Status == (int)CompanionReservePackageItemStatusEnum.Cancelled;
                var reason = Trim(await SanitizeTextHelper.ToSanitizeAsync(dto.Reason?.Trim()), 1000);
                if (cancelling && string.IsNullOrWhiteSpace(reason))
                    return Fail<CompanionReservePackageStatusResultVDto>(Resource.Notification.PleaseEnterCancelDetail);

                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                var rows = await EnsureRowsAsync(reserve);
                var item = rows.Where(i => i.CompanionAssistancePackageId == packageId && i.Status != (int)CompanionReservePackageItemStatusEnum.Cancelled)
                    .OrderByDescending(i => i.Id).FirstOrDefault();
                if (item == null)
                {
                    var any = rows.Any(i => i.CompanionAssistancePackageId == packageId);
                    return Fail<CompanionReservePackageStatusResultVDto>(any
                        ? Resource.Notification.CompanionReservePackageStatusInvalid
                        : Resource.Notification.CompanionReservePackageNotFound);
                }
                if (!CompanionReservePackageItemRules.CanTransition(item.Status, dto.Status))
                    return Fail<CompanionReservePackageStatusResultVDto>(Resource.Notification.CompanionReservePackageStatusInvalid);

                var now = DateTime.Now;
                double refund = 0;
                var reserveCancelled = false;

                item.Status = dto.Status;
                item.StatusReason = reason;
                item.StatusChangedDate = now;
                item.StatusChangedByUserId = actorUserId > 0 ? actorUserId : (long?)null;
                item.StatusChangedByAdmin = asAdmin;

                if (cancelling)
                {
                    // item همین الآن Cancelled شده؛ فعال‌های قبل از لغو = بقیه + خودش
                    var stillActiveBefore = rows.Where(i => i.Id == item.Id || i.Status != (int)CompanionReservePackageItemStatusEnum.Cancelled).ToList();
                    var isLast = stillActiveBefore.Count == 1;
                    var totalWeight = stillActiveBefore.Sum(i => i.PrePaymentPrice);
                    refund = CompanionReservePackageItemRules.RefundShare(reserve.PaymentPrice, item.PrePaymentPrice, totalWeight, isLast);

                    item.RefundAmount = refund;
                    item.RefundDate = refund > 0 ? now : (DateTime?)null;

                    reserve.PackagePrice = Math.Max(0, reserve.PackagePrice - item.Price);
                    reserve.PrePaymentPrice = Math.Max(0, reserve.PrePaymentPrice - refund);
                    reserve.PaymentPrice = Math.Max(0, reserve.PaymentPrice - refund);
                    RecalculateCommission(reserve);

                    if (isLast)
                    {
                        reserve.IsCancel = true;
                        reserve.CancelDetail = reason;
                        reserve.CancelDate = now;
                        reserveCancelled = true;
                    }

                    if (refund > 0)
                    {
                        var ledgerName = CompanionReservePackageItemRules.RefundLedgerName(item.Id);
                        await _context.SaveChangesAsync(); // شناسه‌ی item برای نام دفتر لازم است (ردیف‌های مجازیِ تازه‌ساخته‌شده هم اینجا شناسه می‌گیرند)
                        var alreadyCredited = await _context.Wallets.AsNoTracking()
                            .AnyAsync(w => w.Name == ledgerName && w.UserId == reserve.BookerId && !w.Deleted);
                        if (!alreadyCredited)
                        {
                            var credit = await _walletService.InsertAsyncDto(new WalletDto
                            {
                                Name = ledgerName,
                                Amount = refund,
                                IsIncrease = true,
                                UserId = reserve.BookerId,
                                Painding = false
                            });
                            if (!credit.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return Fail<CompanionReservePackageStatusResultVDto>(Resource.Notification.CompanionReservePackageRefundFailed);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await RunSafeAsync(() => NotifyBookerAsync(reserve, item, cancelling, refund), "package status push");
                if (reserveCancelled)
                {
                    await RunSafeAsync(() => _clubPointIntegrationService.CompanionReserveReversedAsync(reserve.BookerId, reserve.Id), "club reversal");
                    await RunSafeAsync(() => _tripService.CancelLinkedTripForCompanionReserveAsync(reserve.Id), "linked trip cancel");
                }

                return new BaseResultDto<CompanionReservePackageStatusResultVDto>(true, new CompanionReservePackageStatusResultVDto
                {
                    Item = ToVDto(item),
                    RefundAmount = refund,
                    ReserveCancelled = reserveCancelled,
                    Items = await GetViewAsync(reserve.Id)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setting package status failed for reserve {ReserveId} package {PackageId}.", reserveId, packageId);
                return Fail<CompanionReservePackageStatusResultVDto>(Resource.Notification.Unsuccess);
            }
        }

        // ---------------------------------------------------------------- افزودن

        public async Task<BaseResultDto<CompanionReservePackageItemsVDto>> AddAsync(long reserveId, CompanionReservePackageAddDto dto, long actorUserId, bool asAdmin)
        {
            try
            {
                if (dto == null || dto.PackageId <= 0)
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.InvalidData);

                var reserve = await LoadAsync(reserveId, tracking: true);
                if (reserve == null)
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.NothingFound);
                if (!await CanManageAsync(reserve, actorUserId, asAdmin))
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.AccessDenied);
                if (!CompanionReservePackageItemRules.ReserveIsModifiable(reserve.IsCancel, reserve.StateId, reserve.OperatorStateId, reserve.DoneDate))
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.CompanionReservePackageNotModifiable);
                if (!reserve.IsReserved && await HasActivePaymentAsync(reserve))
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.CompanionReservePaymentStartedFinancialDataLocked);
                if (reserve.CompanionAssistance.IsSinglePackage)
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.CompanionReservePackageSingleOnly);

                var package = await _context.CompanionAssistancePackages.AsTracking().Include(p => p.PackageTypes)
                    .FirstOrDefaultAsync(p => p.Id == dto.PackageId && p.CompanionAssistanceId == reserve.CompanionAssistanceId && p.Active && !p.Deleted);
                if (package == null)
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.CompanionReservePackageNotFound);

                var quote = CompanionPackagePricing.Resolve(package.PackageTypes?.Where(t => !t.Deleted) ?? Enumerable.Empty<CompanionAssistancePackageType>(), package.Price, package.PrePaymentPrice, reserve.CompanionAssistanceTypeId);
                if (!quote.Offered)
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.CompanionReserveAssistanceTypeNotBelongToService);

                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                var rows = await EnsureRowsAsync(reserve);
                if (rows.Any(i => i.CompanionAssistancePackageId == package.Id && i.Status != (int)CompanionReservePackageItemStatusEnum.Cancelled))
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.CompanionReservePackageAlreadyActive);

                var pets = Math.Max(1, reserve.UserPets?.Count ?? 0);
                var price = quote.Price * pets;
                var prepay = quote.PrePaymentPrice * pets;
                var paid = reserve.IsReserved;
                var item = new CompanionReservePackageItem
                {
                    CompanionReserveId = reserve.Id,
                    CompanionAssistancePackageId = package.Id,
                    PackageName = Trim(package.Name, 300),
                    PetCount = pets,
                    Price = price,
                    // بعد از پرداخت چیزی برای این پکیج پیش‌پرداخت نشده؛ تفاوت در مبلغ نهایی خدمت تسویه می‌شود
                    PrePaymentPrice = paid ? 0 : prepay,
                    Status = (int)CompanionReservePackageItemStatusEnum.Pending,
                    AddedAfterPayment = paid,
                    CreateDate = DateTime.Now
                };
                await _context.CompanionReservePackageItems.AddAsync(item);

                if (!reserve.CompanionAssistancePackages.Any(p => p.Id == package.Id))
                    reserve.CompanionAssistancePackages.Add(package);

                reserve.PackagePrice += price;
                if (!paid)
                {
                    reserve.PrePaymentPrice += prepay;
                    reserve.PaymentPrice = reserve.PrePaymentPrice;
                    if (reserve.WalletPrice != 0)
                        reserve.WalletPrice = reserve.PrePaymentPrice;
                    RecalculateCommission(reserve);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BaseResultDto<CompanionReservePackageItemsVDto>(true, await GetViewAsync(reserve.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding package to reserve {ReserveId} failed.", reserveId);
                return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.Unsuccess);
            }
        }

        // ---------------------------------------------------------------- حذف (فقط رزرو پرداخت‌نشده)

        public async Task<BaseResultDto<CompanionReservePackageItemsVDto>> RemoveUnpaidAsync(long reserveId, long packageId, long actorUserId, bool asAdmin)
        {
            try
            {
                var reserve = await LoadAsync(reserveId, tracking: true);
                if (reserve == null)
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.NothingFound);
                if (!await CanManageAsync(reserve, actorUserId, asAdmin))
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.AccessDenied);
                if (reserve.IsReserved)
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.CompanionReservePackageRemoveOnlyUnpaid);
                if (!CompanionReservePackageItemRules.ReserveIsModifiable(reserve.IsCancel, reserve.StateId, reserve.OperatorStateId, reserve.DoneDate))
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.CompanionReservePackageNotModifiable);
                if (await HasActivePaymentAsync(reserve))
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.CompanionReservePaymentStartedFinancialDataLocked);

                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                var rows = await EnsureRowsAsync(reserve);
                var item = rows.Where(i => i.CompanionAssistancePackageId == packageId).OrderByDescending(i => i.Id).FirstOrDefault();
                if (item == null)
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.CompanionReservePackageNotFound);
                if (rows.Count(i => i.Status != (int)CompanionReservePackageItemStatusEnum.Cancelled) <= 1)
                    return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.CompanionReservePackageLastRemain);

                reserve.PackagePrice = Math.Max(0, reserve.PackagePrice - item.Price);
                reserve.PrePaymentPrice = Math.Max(0, reserve.PrePaymentPrice - item.PrePaymentPrice);
                reserve.PaymentPrice = reserve.PrePaymentPrice;
                if (reserve.WalletPrice != 0)
                    reserve.WalletPrice = reserve.PrePaymentPrice;
                RecalculateCommission(reserve);

                var link = reserve.CompanionAssistancePackages.FirstOrDefault(p => p.Id == packageId);
                if (link != null)
                    reserve.CompanionAssistancePackages.Remove(link);
                _context.CompanionReservePackageItems.RemoveRange(rows.Where(i => i.CompanionAssistancePackageId == packageId));

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BaseResultDto<CompanionReservePackageItemsVDto>(true, await GetViewAsync(reserve.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Removing package {PackageId} from reserve {ReserveId} failed.", packageId, reserveId);
                return Fail<CompanionReservePackageItemsVDto>(Resource.Notification.Unsuccess);
            }
        }

        // ---------------------------------------------------------------- کمکی‌ها

        private Task<CompanionReserve> LoadAsync(long id, bool tracking)
        {
            var query = _context.CompanionReserves.AsQueryable();
            query = tracking ? query.AsTracking() : query.AsNoTracking();
            return query
                .Include(r => r.CompanionAssistance).ThenInclude(a => a.Companion)
                .Include(r => r.CompanionAssistance).ThenInclude(a => a.Assistance)
                .Include(r => r.CompanionAssistanceUser)
                .Include(r => r.Booker).Include(r => r.UserPets).Include(r => r.CompanionAssistancePackages)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        private async Task<bool> CanManageAsync(CompanionReserve reserve, long actorUserId, bool asAdmin)
        {
            if (asAdmin)
                return true;
            var companion = reserve.CompanionAssistance?.Companion;
            if (companion == null || actorUserId <= 0)
                return false;
            if (companion.OwnerId == actorUserId)
                return true;
            var assigned = reserve.CompanionAssistanceUser != null && reserve.CompanionAssistanceUser.Active
                && !reserve.CompanionAssistanceUser.Deleted && reserve.CompanionAssistanceUser.UserId == actorUserId;
            if (!assigned)
                return false;
            return await _context.CompanionUsers.AsNoTracking().AnyAsync(s =>
                s.CompanionId == companion.Id && s.UserId == actorUserId && !s.Deleted && s.Active && s.UserAccept == true);
        }

        private Task<bool> HasActivePaymentAsync(CompanionReserve reserve)
        {
            var callbackId = reserve.Id.ToString();
            var batchCallbackId = reserve.BatchId?.ToString();
            return _context.Payments.AsNoTracking().AnyAsync(s =>
                (s.IsSuccess == null || s.IsSuccess == true) &&
                (
                    s.CallBackTypeLabel == PaymentCallbackTypeEnum.CompanionReserve.ToString() && s.CallBackId == callbackId ||
                    batchCallbackId != null &&
                        s.CallBackTypeLabel == PaymentCallbackTypeEnum.CompanionReserveBatch.ToString() &&
                        s.CallBackId == batchCallbackId
                ));
        }

        // برای رزروهای قدیمی (و هر پکیجی که ردیف ندارد) ردیف می‌سازد؛ داخل تراکنش صدا زده می‌شود و ردیف‌های tracked را برمی‌گرداند.
        private async Task<List<CompanionReservePackageItem>> EnsureRowsAsync(CompanionReserve reserve)
        {
            var rows = await _context.CompanionReservePackageItems.AsTracking()
                .Where(i => i.CompanionReserveId == reserve.Id).OrderBy(i => i.Id).ToListAsync();
            var have = rows.Select(i => i.CompanionAssistancePackageId).ToHashSet();
            var missing = reserve.CompanionAssistancePackages.Where(p => !have.Contains(p.Id)).ToList();
            if (missing.Any())
            {
                var created = await BuildRowsAsync(reserve, missing);
                await _context.CompanionReservePackageItems.AddRangeAsync(created);
                await _context.SaveChangesAsync();
                rows.AddRange(created);
            }
            return rows;
        }

        // ردیف‌های (ذخیره‌نشده) برای پکیج‌هایی که هنوز ردیف ندارند؛ قیمت از همان منطق قیمت‌گذاری رزرو
        private async Task<List<CompanionReservePackageItem>> BuildRowsAsync(CompanionReserve reserve, List<CompanionAssistancePackage> packages)
        {
            var ids = packages.Select(p => p.Id).ToList();
            var types = await _context.CompanionAssistancePackageTypes.AsNoTracking()
                .Where(t => ids.Contains(t.CompanionAssistancePackageId) && !t.Deleted).ToListAsync();
            var pets = Math.Max(1, reserve.UserPets?.Count ?? 0);
            var legacyStatus = reserve.IsCancel
                ? (int)CompanionReservePackageItemStatusEnum.Cancelled
                : reserve.OperatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Complete
                    ? (int)CompanionReservePackageItemStatusEnum.Approved
                    : (int)CompanionReservePackageItemStatusEnum.Pending;
            var list = new List<CompanionReservePackageItem>();
            foreach (var p in packages)
            {
                var quote = CompanionPackagePricing.Resolve(types.Where(t => t.CompanionAssistancePackageId == p.Id), p.Price, p.PrePaymentPrice, reserve.CompanionAssistanceTypeId);
                // پکیجی که بعداً حالتش حذف شده نباید قیمت را صفر کند؛ به قیمت پایه برمی‌گردیم (همان رفتار ویرایش رزرو)
                if (!quote.Offered)
                    quote = new CompanionPackagePricing.Quote(true, p.Price, p.PrePaymentPrice, true);
                list.Add(new CompanionReservePackageItem
                {
                    CompanionReserveId = reserve.Id,
                    CompanionAssistancePackageId = p.Id,
                    PackageName = Trim(p.Name, 300),
                    PetCount = pets,
                    Price = quote.Price * pets,
                    PrePaymentPrice = quote.PrePaymentPrice * pets,
                    Status = legacyStatus,
                    StatusChangedDate = null,
                    CreateDate = reserve.CreateDate
                });
            }
            return list;
        }

        private static CompanionReservePackageItemVDto ToVDto(CompanionReservePackageItem i) => new CompanionReservePackageItemVDto
        {
            Id = i.Id,
            PackageId = i.CompanionAssistancePackageId,
            PackageName = i.PackageName,
            PetCount = i.PetCount,
            Price = i.Price,
            Status = i.Status,
            StatusReason = i.StatusReason,
            StatusChangedDate = i.StatusChangedDate,
            StatusChangedByAdmin = i.StatusChangedByAdmin,
            RefundAmount = i.RefundAmount,
            RefundDate = i.RefundDate,
            AddedAfterPayment = i.AddedAfterPayment,
            CreateDate = i.CreateDate
        };

        private static CompanionReservePackageSummaryVDto Summarize(List<CompanionReservePackageItemVDto> items) => new CompanionReservePackageSummaryVDto
        {
            Total = items.Count,
            Pending = items.Count(i => i.Status == (int)CompanionReservePackageItemStatusEnum.Pending),
            Approved = items.Count(i => i.Status == (int)CompanionReservePackageItemStatusEnum.Approved),
            Cancelled = items.Count(i => i.Status == (int)CompanionReservePackageItemStatusEnum.Cancelled),
            RefundedTotal = items.Sum(i => i.RefundAmount)
        };

        private static void RecalculateCommission(CompanionReserve reserve)
        {
            if (reserve.CompanionAssistance == null)
                return;
            decimal total = (decimal)reserve.PaymentPrice;
            decimal siteShare = (total * reserve.CompanionAssistance.CommissionPercent) / 100m;
            reserve.SiteShare = (double)siteShare;
            reserve.CompanionShare = (double)(total - siteShare);
        }

        private async Task NotifyBookerAsync(CompanionReserve reserve, CompanionReservePackageItem item, bool cancelled, double refund)
        {
            var type = cancelled ? PushTypeEnum.PushCompanionReservePackageCancelled : PushTypeEnum.PushCompanionReservePackageApproved;
            await _pushService.SendPushAsync(type, reserve.BookerId,
                token1: item.PackageName,
                token2: CompanionReservePackageItemRules.PushKey(item.Id, item.Status),
                token3: reserve.Id.ToString(),
                token4: refund.ToString("N0", CultureInfo.InvariantCulture));
        }

        private async Task RunSafeAsync(Func<Task> action, string what)
        {
            try { await action(); }
            catch (Exception ex) { _logger.LogError(ex, "Post-commit action failed: {Action}", what); }
        }

        private static BaseResultDto<T> Fail<T>(string message) where T : class => new BaseResultDto<T>(false, message, null);

        private static string Trim(string s, int max) => string.IsNullOrEmpty(s) ? s : (s.Length <= max ? s : s.Substring(0, max));
    }
}
