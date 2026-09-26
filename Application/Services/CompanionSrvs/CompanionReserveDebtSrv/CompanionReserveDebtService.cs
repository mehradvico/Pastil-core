using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveDebtSrv
{
    public class ReserveDebtItemDto
    {
        public long ReserveId { get; set; }
        public string ReserveCode { get; set; }
        public string CompanionName { get; set; }
        public string AssistanceName { get; set; }
        public double Amount { get; set; }
        public DateTime UnpaidDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysLeft { get; set; }
        public bool Overdue { get; set; }
    }

    public class ReserveDebtSummaryDto
    {
        public List<ReserveDebtItemDto> Items { get; set; } = new();
        public double TotalAmount { get; set; }
        // true = کاربر رزرو/خرید جدید نمی‌تواند انجام دهد
        public bool Locked { get; set; }
        public double WalletBalance { get; set; }
    }

    public class ReserveDebtPayDto { public long ReserveId { get; set; } }
    public class ReserveDebtMarkPaidDto { public long ReserveId { get; set; } }

    public interface ICompanionReserveDebtService
    {
        Task<BaseResultDto<ReserveDebtSummaryDto>> GetMyDebtsAsync(long userId);
        Task<BaseResultDto> PayFromWalletAsync(long userId, long reserveId);
        Task<BaseResultDto> MarkPaidByClinicAsync(long ownerUserId, long reserveId);
    }

    public class CompanionReserveDebtService : ICompanionReserveDebtService
    {
        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;

        public CompanionReserveDebtService(IDataBaseContext context, IWalletService walletService)
        {
            _context = context;
            _walletService = walletService;
        }

        public async Task<BaseResultDto<ReserveDebtSummaryDto>> GetMyDebtsAsync(long userId)
        {
            try
            {
                var now = DateTime.Now;
                var rows = await _context.CompanionReserves.AsNoTracking()
                    .Where(r => r.BookerId == userId && r.OperatorUnpaid && r.OperatorDebtPaidDate == null && !r.IsCancel && r.OperatorUnpaidDate != null)
                    .OrderBy(r => r.OperatorUnpaidDate)
                    .Select(r => new
                    {
                        r.Id, r.ReserveCode, r.OperatorUnpaidAmount, Date = r.OperatorUnpaidDate.Value,
                        Companion = r.CompanionAssistance.Companion.Name, Assistance = r.CompanionAssistance.Assistance.Name
                    })
                    .ToListAsync();

                var items = rows.Select(r => new ReserveDebtItemDto
                {
                    ReserveId = r.Id, ReserveCode = r.ReserveCode, CompanionName = r.Companion, AssistanceName = r.Assistance,
                    Amount = r.OperatorUnpaidAmount, UnpaidDate = r.Date,
                    DueDate = CompanionReserveDebtRules.DueDate(r.Date),
                    DaysLeft = CompanionReserveDebtRules.DaysLeft(r.Date, now),
                    Overdue = now >= CompanionReserveDebtRules.DueDate(r.Date)
                }).ToList();

                var balance = await _walletService.GetAmountValueAsync(userId);
                return new BaseResultDto<ReserveDebtSummaryDto>(true, new ReserveDebtSummaryDto
                {
                    Items = items,
                    TotalAmount = items.Sum(x => x.Amount),
                    Locked = items.Any(x => x.Overdue),
                    WalletBalance = balance
                });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ReserveDebtSummaryDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        // پرداخت بدهی از کیف پول (اگر کم بود، کاربر اول کیف پول را از درگاه شارژ می‌کند)
        public async Task<BaseResultDto> PayFromWalletAsync(long userId, long reserveId)
        {
            try
            {
                await using var tx = await _context.BeginTransactionAsync(IsolationLevel.Serializable);

                var reserve = await _context.CompanionReserves.AsNoTracking()
                    .Where(r => r.Id == reserveId && r.BookerId == userId && r.OperatorUnpaid && r.OperatorDebtPaidDate == null && !r.IsCancel)
                    .Select(r => new { r.Id, r.OperatorUnpaidAmount, r.Permitted, Commission = r.CompanionAssistance.CommissionPercent })
                    .FirstOrDefaultAsync();
                if (reserve == null)
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound);

                var balance = await _walletService.GetAmountValueAsync(userId);
                if (balance + 0.01 < reserve.OperatorUnpaidAmount)
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtWalletInsufficient);

                // گذار اتمی: پرداخت هم‌زمان دوباره چیزی تغییر نمی‌دهد
                var marked = await _context.CompanionReserves
                    .Where(r => r.Id == reserveId && r.OperatorUnpaid && r.OperatorDebtPaidDate == null)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.OperatorDebtPaidDate, (DateTime?)DateTime.Now)
                        .SetProperty(r => r.OperatorDebtPaidByWallet, true));
                if (marked == 0)
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound);

                var debit = await _walletService.InsertAsyncDto(new WalletDto
                {
                    Name = $"پرداخت هزینه‌ی خدمت (رزرو {reserveId})",
                    Amount = reserve.OperatorUnpaidAmount,
                    IsIncrease = false,
                    UserId = userId,
                    CompanionReserveId = reserveId,
                    Painding = false
                });
                if (!debit.IsSuccess)
                {
                    await tx.RollbackAsync();
                    return new BaseResultDto(false, Resource.Notification.OperationFailed);
                }

                // مبلغی که از طریق پاستیل پرداخت شد به سهم کلینیک/سایت اضافه می‌شود (اگر رزرو هنوز در تسویه نیامده)
                if (!reserve.Permitted)
                {
                    var site = CompanionReserveDebtRules.SiteShareOf(reserve.OperatorUnpaidAmount, reserve.Commission);
                    var partner = reserve.OperatorUnpaidAmount - site;
                    await _context.CompanionReserves
                        .Where(r => r.Id == reserveId && !r.Permitted)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(r => r.SiteShare, r => r.SiteShare + site)
                            .SetProperty(r => r.CompanionShare, r => r.CompanionShare + partner));
                }

                await tx.CommitAsync();
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        // کلینیک تأیید می‌کند مبلغ را مستقیم از کاربر گرفته است (بدهی بسته می‌شود، بدون تغییر در حسابداری پاستیل)
        public async Task<BaseResultDto> MarkPaidByClinicAsync(long ownerUserId, long reserveId)
        {
            try
            {
                var marked = await _context.CompanionReserves
                    .Where(r => r.Id == reserveId && r.OperatorUnpaid && r.OperatorDebtPaidDate == null
                        && r.CompanionAssistance.Companion.OwnerId == ownerUserId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.OperatorDebtPaidDate, (DateTime?)DateTime.Now)
                        .SetProperty(r => r.OperatorDebtPaidByWallet, false));
                return marked == 0
                    ? new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound)
                    : new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }
    }
}
