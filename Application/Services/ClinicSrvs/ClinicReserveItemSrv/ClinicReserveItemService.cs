using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.ClinicSrvs.ClinicReserveItemSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveItemSrv.Iface;
using Application.Services.ClinicSrvs.ClinicReserveSrv;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.TripSrv.TripSrv.Iface;
using Entities.Entities.ClinicField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicReserveItemSrv
{
    /// <summary>
    /// تأیید/لغو تکی بسته‌های رزرو کلینیک + افزودن بسته حین ویزیت + بازپرداخت سهم بسته‌ی لغوشده به کیف پول کاربر
    /// (هم‌ارز CompanionReservePackageItemService برای رزرو کلینیک).
    /// </summary>
    public class ClinicReserveItemService : IClinicReserveItemService
    {
        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly IPushNotificationService _pushService;
        private readonly ITripService _tripService;
        private readonly ILogger<ClinicReserveItemService> _logger;

        public ClinicReserveItemService(
            IDataBaseContext context,
            IWalletService walletService,
            IPushNotificationService pushService,
            ITripService tripService,
            ILogger<ClinicReserveItemService> logger)
        {
            _context = context;
            _walletService = walletService;
            _pushService = pushService;
            _tripService = tripService;
            _logger = logger;
        }

        private const int CompleteStatus = (int)ClinicReserveStatusEnum.Complete;

        // ---------------------------------------------------------------- نمایش

        public async Task<BaseResultDto<ClinicReserveItemsVDto>> GetForBookerAsync(long reserveId, long bookerId)
        {
            var owns = await _context.ClinicReserves.AsNoTracking().AnyAsync(r => r.Id == reserveId && r.BookerId == bookerId);
            if (!owns)
                return Fail<ClinicReserveItemsVDto>(Resource.Notification.NothingFound);
            return new BaseResultDto<ClinicReserveItemsVDto>(true, await GetViewAsync(reserveId));
        }

        public async Task<BaseResultDto<ClinicReserveItemsVDto>> GetForManagerAsync(long reserveId, long actorUserId, bool asAdmin)
        {
            var reserve = await LoadAsync(reserveId, tracking: false);
            if (reserve == null)
                return Fail<ClinicReserveItemsVDto>(Resource.Notification.NothingFound);
            if (!await CanManageAsync(reserve, actorUserId, asAdmin))
                return Fail<ClinicReserveItemsVDto>(Resource.Notification.AccessDenied);
            return new BaseResultDto<ClinicReserveItemsVDto>(true, await GetViewAsync(reserveId));
        }

        private async Task<ClinicReserveItemsVDto> GetViewAsync(long reserveId)
        {
            var reserve = await _context.ClinicReserves.AsNoTracking()
                .Where(r => r.Id == reserveId)
                .Select(r => new { r.Id, r.IsReserved, r.IsCancel, r.Status, r.DoneDate })
                .FirstOrDefaultAsync();
            if (reserve == null)
                return new ClinicReserveItemsVDto { ReserveId = reserveId };

            var rows = await _context.ClinicReserveItems.AsNoTracking()
                .Where(i => i.ClinicReserveId == reserveId).OrderBy(i => i.Id).ToListAsync();
            var items = rows.Select(ToVDto).ToList();
            return new ClinicReserveItemsVDto
            {
                ReserveId = reserveId,
                CanManage = ClinicReserveItemRules.ReserveIsModifiable(reserve.IsReserved, reserve.IsCancel, reserve.Status, reserve.DoneDate, CompleteStatus),
                Items = items,
                Summary = Summarize(items),
            };
        }

        // ---------------------------------------------------------------- تأیید / لغو

        public async Task<BaseResultDto<ClinicReserveItemStatusResultVDto>> SetStatusAsync(
            long reserveId, long packageId, ClinicReserveItemStatusDto dto, long actorUserId, bool asAdmin)
        {
            try
            {
                if (dto == null || !ClinicReserveItemRules.IsValidTarget(dto.Status))
                    return Fail<ClinicReserveItemStatusResultVDto>(Resource.Notification.ClinicReserveItemStatusInvalid);

                var reserve = await LoadAsync(reserveId, tracking: true);
                if (reserve == null)
                    return Fail<ClinicReserveItemStatusResultVDto>(Resource.Notification.NothingFound);
                if (!await CanManageAsync(reserve, actorUserId, asAdmin))
                    return Fail<ClinicReserveItemStatusResultVDto>(Resource.Notification.AccessDenied);
                if (!reserve.IsReserved)
                    return Fail<ClinicReserveItemStatusResultVDto>(Resource.Notification.ClinicReserveItemNeedsPaidReserve);
                if (!ClinicReserveItemRules.ReserveIsModifiable(reserve.IsReserved, reserve.IsCancel, reserve.Status, reserve.DoneDate, CompleteStatus))
                    return Fail<ClinicReserveItemStatusResultVDto>(Resource.Notification.ClinicReserveItemNotModifiable);

                var cancelling = dto.Status == (int)ClinicReserveItemStatusEnum.Cancelled;
                var reason = Trim(await SanitizeTextHelper.ToSanitizeAsync(dto.Reason?.Trim()), 1000);
                if (cancelling && string.IsNullOrWhiteSpace(reason))
                    return Fail<ClinicReserveItemStatusResultVDto>(Resource.Notification.PleaseEnterCancelDetail);

                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                var rows = reserve.Items.OrderBy(i => i.Id).ToList();
                var item = rows.Where(i => i.ClinicServicePackageId == packageId && i.Status != (int)ClinicReserveItemStatusEnum.Cancelled)
                    .OrderByDescending(i => i.Id).FirstOrDefault();
                if (item == null)
                {
                    var any = rows.Any(i => i.ClinicServicePackageId == packageId);
                    await transaction.RollbackAsync();
                    return Fail<ClinicReserveItemStatusResultVDto>(any
                        ? Resource.Notification.ClinicReserveItemStatusInvalid
                        : Resource.Notification.ClinicReserveItemNotFound);
                }
                if (!ClinicReserveItemRules.CanTransition(item.Status, dto.Status))
                {
                    await transaction.RollbackAsync();
                    return Fail<ClinicReserveItemStatusResultVDto>(Resource.Notification.ClinicReserveItemStatusInvalid);
                }

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
                    var activeBefore = rows.Where(i => i.Id == item.Id || i.Status != (int)ClinicReserveItemStatusEnum.Cancelled).ToList();
                    var isLast = activeBefore.Count == 1;
                    var totalWeight = activeBefore.Sum(i => i.OnlineAmount);
                    refund = ClinicReserveItemRules.RefundShare(reserve.PaymentPrice, item.OnlineAmount, totalWeight, isLast);

                    item.RefundAmount = refund;
                    item.RefundDate = refund > 0 ? now : (DateTime?)null;

                    reserve.PackagePrice = Math.Max(0, reserve.PackagePrice - item.PackagePrice);
                    reserve.Price = Math.Max(0, reserve.Price - item.PackagePrice);
                    reserve.OnlineAmount = Math.Max(0, reserve.OnlineAmount - item.OnlineAmount);
                    reserve.PaymentPrice = Math.Max(0, reserve.PaymentPrice - refund);
                    reserve.WalletPrice = Math.Min(reserve.WalletPrice, reserve.PaymentPrice);
                    RecalculateCommission(reserve, rows);

                    if (isLast)
                    {
                        reserve.IsCancel = true;
                        reserve.CancelDetail = reason;
                        reserve.CancelDate = now;
                        reserve.CancelledByBooker = false;
                        reserveCancelled = true;
                    }

                    if (refund > 0)
                    {
                        var ledgerName = ClinicReserveItemRules.RefundLedgerName(item.Id);
                        var alreadyCredited = await _context.Wallets.AsNoTracking()
                            .AnyAsync(w => w.Name == ledgerName && w.UserId == reserve.BookerId && !w.Deleted);
                        if (!alreadyCredited)
                        {
                            await _context.SaveChangesAsync(); // شناسه‌ی item برای نام دفتر لازم است
                            var credit = await _walletService.InsertAsyncDto(new WalletDto
                            {
                                Name = ledgerName,
                                Amount = refund,
                                IsIncrease = true,
                                UserId = reserve.BookerId,
                                Painding = false,
                            });
                            if (!credit.IsSuccess)
                            {
                                await transaction.RollbackAsync();
                                return Fail<ClinicReserveItemStatusResultVDto>(Resource.Notification.ClinicReserveItemRefundFailed);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await RunSafeAsync(() => NotifyBookerAsync(reserve, item, cancelling, refund), "item status push");
                if (reserveCancelled)
                {
                    await RunSafeAsync(() => _tripService.CancelLinkedTripForClinicReserveAsync(reserve.Id), "linked trip cancel");
                    await RunSafeAsync(() => ClinicReserveReminders.RemoveAsync(_context, reserve.Id), "reminders remove");
                }

                return new BaseResultDto<ClinicReserveItemStatusResultVDto>(true, new ClinicReserveItemStatusResultVDto
                {
                    Item = ToVDto(item),
                    RefundAmount = refund,
                    ReserveCancelled = reserveCancelled,
                    Items = await GetViewAsync(reserve.Id),
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setting item status failed for clinic reserve {ReserveId} package {PackageId}.", reserveId, packageId);
                return Fail<ClinicReserveItemStatusResultVDto>(Resource.Notification.Unsuccess);
            }
        }

        // ---------------------------------------------------------------- افزودن

        public async Task<BaseResultDto<ClinicReserveItemsVDto>> AddAsync(long reserveId, ClinicReserveItemAddDto dto, long actorUserId, bool asAdmin)
        {
            try
            {
                if (dto == null || dto.PackageId <= 0)
                    return Fail<ClinicReserveItemsVDto>(Resource.Notification.InvalidData);

                var reserve = await LoadAsync(reserveId, tracking: true);
                if (reserve == null)
                    return Fail<ClinicReserveItemsVDto>(Resource.Notification.NothingFound);
                if (!await CanManageAsync(reserve, actorUserId, asAdmin))
                    return Fail<ClinicReserveItemsVDto>(Resource.Notification.AccessDenied);
                if (!ClinicReserveItemRules.ReserveIsModifiable(reserve.IsReserved, reserve.IsCancel, reserve.Status, reserve.DoneDate, CompleteStatus))
                    return Fail<ClinicReserveItemsVDto>(Resource.Notification.ClinicReserveItemNotModifiable);

                var package = await _context.ClinicServicePackages.AsNoTracking()
                    .Where(p => p.Id == dto.PackageId && !p.Deleted && p.Active
                                && !p.ClinicService.Deleted && p.ClinicService.Active && p.ClinicService.Approved
                                && p.ClinicService.ClinicId == reserve.ClinicId)
                    .Select(p => new
                    {
                        p.Id, p.Name, p.Price, p.PrePaymentPrice, p.PetSize, p.ClinicServiceId,
                        p.ClinicService.CommissionPercent, p.ClinicService.DurationMinutes,
                        Types = p.Types.Where(x => !x.Deleted).Select(x => new { x.Mode, x.Price, x.PrePaymentPrice }).ToList(),
                    })
                    .FirstOrDefaultAsync();
                if (package == null)
                    return Fail<ClinicReserveItemsVDto>(Resource.Notification.ClinicReserveItemNotFound);

                var petSize = await _context.UserPets.AsNoTracking().Where(p => p.Id == reserve.UserPetId).Select(p => p.Size).FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(package.PetSize) &&
                    !string.Equals(package.PetSize, petSize?.Trim(), StringComparison.OrdinalIgnoreCase))
                    return Fail<ClinicReserveItemsVDto>(Resource.Notification.ClinicReservePetSizeMismatch);

                var resolved = ClinicReserveRules.ResolveModePrice(
                    package.Price, package.PrePaymentPrice, package.Types.Select(x => (x.Mode, x.Price, x.PrePaymentPrice)).ToList(), reserve.Mode);
                if (!resolved.HasValue)
                    return Fail<ClinicReserveItemsVDto>(Resource.Notification.ClinicReservePackageNotOffered);

                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
                var active = reserve.Items.Where(i => i.Status != (int)ClinicReserveItemStatusEnum.Cancelled).ToList();
                if (active.Any(i => i.ClinicServicePackageId == package.Id))
                {
                    await transaction.RollbackAsync();
                    return Fail<ClinicReserveItemsVDto>(Resource.Notification.ClinicReserveDuplicateService);
                }
                if (active.Count >= ClinicReserveRules.MaxPackages)
                {
                    await transaction.RollbackAsync();
                    return Fail<ClinicReserveItemsVDto>(Resource.Notification.ClinicReserveTooManyPackages);
                }

                var clinicPercent = await _context.Clinics.AsNoTracking().Where(c => c.Id == reserve.ClinicId).Select(c => c.CommissionPercent).FirstAsync();
                var price = resolved.Value.Price;
                var item = new ClinicReserveItem
                {
                    ClinicReserveId = reserve.Id,
                    ClinicServiceId = package.ClinicServiceId,
                    ClinicServicePackageId = package.Id,
                    PackageName = Trim(package.Name, 300),
                    PackagePrice = price,
                    // بعد از پرداخت چیزی برای این بسته پیش‌پرداخت نشده؛ تفاوت حضوری در کلینیک پرداخت می‌شود (و در تسویه‌ی آنلاین نمی‌آید)
                    OnlineAmount = 0,
                    DurationMinutes = ClinicReserveRules.EffectiveDuration(package.DurationMinutes),
                    CommissionPercent = ClinicReserveRules.ResolveCommission(package.CommissionPercent, clinicPercent),
                    Status = (int)ClinicReserveItemStatusEnum.Pending,
                    AddedAfterPayment = true,
                    CreateDate = DateTime.Now,
                };
                await _context.ClinicReserveItems.AddAsync(item);
                reserve.PackagePrice += price;
                reserve.Price += price;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BaseResultDto<ClinicReserveItemsVDto>(true, await GetViewAsync(reserve.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding package to clinic reserve {ReserveId} failed.", reserveId);
                return Fail<ClinicReserveItemsVDto>(Resource.Notification.Unsuccess);
            }
        }

        // ---------------------------------------------------------------- کمکی‌ها

        private Task<ClinicReserve> LoadAsync(long id, bool tracking)
        {
            var query = _context.ClinicReserves.AsQueryable();
            query = tracking ? query.AsTracking() : query.AsNoTracking();
            return query.Include(r => r.Clinic).Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id);
        }

        private async Task<bool> CanManageAsync(ClinicReserve reserve, long actorUserId, bool asAdmin)
        {
            if (asAdmin)
                return true;
            if (reserve.Clinic == null || actorUserId <= 0)
                return false;
            if (reserve.Clinic.OwnerId == actorUserId)
                return true;
            if (reserve.AssigneeUserId != actorUserId)
                return false;
            return await _context.ClinicUsers.AsNoTracking().AnyAsync(s =>
                s.ClinicId == reserve.ClinicId && s.UserId == actorUserId && !s.Deleted && s.Active && s.UserAccept == true);
        }

        /// <summary>
        /// بعد از لغو یک بسته، سهم سایت/ارائه‌دهنده از مبلغ باقی‌مانده‌ی پرداخت دوباره بین بسته‌های فعال تقسیم می‌شود
        /// (وزن = سهم آنلاین، درصد = درصدِ ثبت‌شده‌ی خود بسته)؛ جمع سهم‌ها همیشه دقیقاً برابر PaymentPrice است.
        /// </summary>
        private static void RecalculateCommission(ClinicReserve reserve, List<ClinicReserveItem> rows)
        {
            var activeItems = rows.Where(i => i.Status != (int)ClinicReserveItemStatusEnum.Cancelled).ToList();
            foreach (var cancelled in rows.Where(i => i.Status == (int)ClinicReserveItemStatusEnum.Cancelled))
            {
                cancelled.ProviderShare = 0;
                cancelled.SiteShare = 0;
            }
            if (activeItems.Count == 0 || reserve.PaymentPrice <= 0)
            {
                foreach (var i in activeItems)
                {
                    i.ProviderShare = 0;
                    i.SiteShare = 0;
                }
                reserve.ProviderShare = 0;
                reserve.SiteShare = 0;
                return;
            }

            var parts = activeItems.Select(i => (Weight: i.OnlineAmount, Percent: i.CommissionPercent)).ToList();
            var shares = ClinicReserveRules.AllocateCommission(reserve.PaymentPrice, parts);
            for (var index = 0; index < activeItems.Count; index++)
            {
                activeItems[index].ProviderShare = shares[index].Provider;
                activeItems[index].SiteShare = shares[index].Site;
            }
            reserve.ProviderShare = shares.Sum(s => s.Provider);
            reserve.SiteShare = shares.Sum(s => s.Site);
            reserve.CommissionPercent = Math.Round((decimal)reserve.SiteShare * 100m / (decimal)reserve.PaymentPrice, 2);
        }

        private async Task NotifyBookerAsync(ClinicReserve reserve, ClinicReserveItem item, bool cancelled, double refund)
        {
            var type = cancelled ? PushTypeEnum.PushClinicReserveItemCancelled : PushTypeEnum.PushClinicReserveItemApproved;
            await _pushService.SendPushAsync(type, reserve.BookerId,
                token1: item.PackageName,
                token2: ClinicReserveItemRules.PushKey(item.Id, item.Status),
                token3: reserve.Id.ToString(),
                token4: refund.ToString("N0", CultureInfo.InvariantCulture));
        }

        private async Task RunSafeAsync(Func<Task> action, string what)
        {
            try { await action(); }
            catch (Exception ex) { _logger.LogError(ex, "Post-commit action failed: {Action}", what); }
        }

        private static ClinicReserveItemRowVDto ToVDto(ClinicReserveItem i) => new ClinicReserveItemRowVDto
        {
            Id = i.Id,
            PackageId = i.ClinicServicePackageId,
            ServiceId = i.ClinicServiceId,
            PackageName = i.PackageName,
            Price = i.PackagePrice,
            OnlineAmount = i.OnlineAmount,
            Status = i.Status,
            StatusReason = i.StatusReason,
            StatusChangedDate = i.StatusChangedDate,
            StatusChangedByAdmin = i.StatusChangedByAdmin,
            RefundAmount = i.RefundAmount,
            RefundDate = i.RefundDate,
            AddedAfterPayment = i.AddedAfterPayment,
            CreateDate = i.CreateDate,
        };

        private static ClinicReserveItemSummaryVDto Summarize(List<ClinicReserveItemRowVDto> items) => new ClinicReserveItemSummaryVDto
        {
            Total = items.Count,
            Pending = items.Count(i => i.Status == (int)ClinicReserveItemStatusEnum.Pending),
            Approved = items.Count(i => i.Status == (int)ClinicReserveItemStatusEnum.Approved),
            Cancelled = items.Count(i => i.Status == (int)ClinicReserveItemStatusEnum.Cancelled),
            RefundedTotal = items.Sum(i => i.RefundAmount),
        };

        private static BaseResultDto<T> Fail<T>(string message) where T : class => new BaseResultDto<T>(false, message, null);

        private static string Trim(string s, int max) => string.IsNullOrEmpty(s) ? s : (s.Length <= max ? s : s.Substring(0, max));
    }
}
