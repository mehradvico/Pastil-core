using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using Application.Services.PastilClubSrvs.PointSrv.Iface;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.PointSrv
{
    public class ClubPointService : IClubPointService
    {
        private static readonly TimeZoneInfo TehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

        private readonly IDataBaseContext _context;

        public ClubPointService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<ClubPointBalanceVDto>> GetBalanceAsync(
            long userId,
            CancellationToken cancellationToken = default)
        {
            var account = await _context.ClubPointAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);

            return new BaseResultDto<ClubPointBalanceVDto>(true, account == null
                ? new ClubPointBalanceVDto { UserId = userId }
                : MapBalance(account));
        }

        public async Task<ClubPointTransactionSearchDto> SearchTransactionsAsync(
            ClubPointTransactionInputDto dto,
            long? userId = null,
            CancellationToken cancellationToken = default)
        {
            dto.PageIndex = Math.Max(1, dto.PageIndex);
            dto.PageSize = Math.Clamp(dto.PageSize, 1, 100);
            if (userId.HasValue)
                dto.UserId = userId;

            var query = _context.ClubPointTransactions
                .AsNoTracking()
                .Include(item => item.User)
                .AsQueryable();

            if (dto.UserId.HasValue)
                query = query.Where(item => item.UserId == dto.UserId.Value);
            if (dto.TransactionType.HasValue)
                query = query.Where(item => item.TransactionType == dto.TransactionType.Value);
            if (dto.SourceType.HasValue)
                query = query.Where(item => item.SourceType == dto.SourceType.Value);
            if (dto.FromDate.HasValue)
                query = query.Where(item => item.CreateDate >= dto.FromDate.Value);
            if (dto.ToDate.HasValue)
                query = query.Where(item => item.CreateDate < dto.ToDate.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var text = dto.Q.Trim();
                query = query.Where(item =>
                    item.Description.Contains(text) ||
                    item.IdempotencyKey.Contains(text) ||
                    item.User.Mobile.Contains(text) ||
                    item.User.FirstName.Contains(text) ||
                    item.User.LastName.Contains(text));
            }

            query = dto.SortBy == SortEnum.Old
                ? query.OrderBy(item => item.Id)
                : query.OrderByDescending(item => item.Id);

            var result = new ClubPointTransactionSearchDto(dto)
            {
                TotalCount = await query.CountAsync(cancellationToken),
                List = await query
                    .Skip((dto.PageIndex - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .Select(item => new ClubPointTransactionVDto
                    {
                        Id = item.Id,
                        UserId = item.UserId,
                        UserFullName = (item.User.FirstName + " " + item.User.LastName).Trim(),
                        UserMobile = item.User.Mobile,
                        TransactionType = item.TransactionType,
                        Amount = item.Amount,
                        AvailableBefore = item.AvailableBefore,
                        AvailableAfter = item.AvailableAfter,
                        DebtBefore = item.DebtBefore,
                        DebtAfter = item.DebtAfter,
                        SourceType = item.SourceType,
                        SourceId = item.SourceId,
                        PointRuleId = item.PointRuleId,
                        ParentTransactionId = item.ParentTransactionId,
                        Description = item.Description,
                        CreateDate = item.CreateDate,
                        CreatedByAdminId = item.CreatedByAdminId
                    })
                    .ToListAsync(cancellationToken)
            };

            return result;
        }

        public Task<BaseResultDto<ClubPointTransactionVDto>> EarnAsync(
            ClubPointChangeDto dto,
            CancellationToken cancellationToken = default)
        {
            dto.TransactionType = dto.TransactionType == ClubPointTransactionTypeEnum.ReferralEarn
                ? ClubPointTransactionTypeEnum.ReferralEarn
                : ClubPointTransactionTypeEnum.Earn;
            return ChangeAsync(dto, PointOperation.Earn, cancellationToken);
        }

        public Task<BaseResultDto<ClubPointTransactionVDto>> SpendAsync(
            ClubPointChangeDto dto,
            CancellationToken cancellationToken = default)
        {
            dto.TransactionType = ClubPointTransactionTypeEnum.Spend;
            return ChangeAsync(dto, PointOperation.Spend, cancellationToken);
        }

        public Task<BaseResultDto<ClubPointTransactionVDto>> ReverseEarnAsync(
            ClubPointChangeDto dto,
            CancellationToken cancellationToken = default)
        {
            dto.TransactionType = ClubPointTransactionTypeEnum.ReverseEarn;
            return ChangeAsync(dto, PointOperation.ReverseEarn, cancellationToken);
        }

        public Task<BaseResultDto<ClubPointTransactionVDto>> IncreaseManualAsync(
            ClubManualPointDto dto,
            long adminId,
            CancellationToken cancellationToken = default)
        {
            if (dto.RequestId == Guid.Empty)
                return Task.FromResult(new BaseResultDto<ClubPointTransactionVDto>(false, "CLUB_POINT_REQUEST_INVALID", null));

            return ChangeAsync(new ClubPointChangeDto
            {
                UserId = dto.UserId,
                Amount = dto.Amount,
                TransactionType = ClubPointTransactionTypeEnum.ManualIncrease,
                SourceType = ClubPointSourceTypeEnum.Admin,
                Description = dto.Reason,
                IdempotencyKey = $"club-point:manual-increase:{dto.RequestId:N}",
                CreatedByAdminId = adminId
            }, PointOperation.Earn, cancellationToken);
        }

        public Task<BaseResultDto<ClubPointTransactionVDto>> DecreaseManualAsync(
            ClubManualPointDto dto,
            long adminId,
            CancellationToken cancellationToken = default)
        {
            if (dto.RequestId == Guid.Empty)
                return Task.FromResult(new BaseResultDto<ClubPointTransactionVDto>(false, "CLUB_POINT_REQUEST_INVALID", null));

            return ChangeAsync(new ClubPointChangeDto
            {
                UserId = dto.UserId,
                Amount = dto.Amount,
                TransactionType = ClubPointTransactionTypeEnum.ManualDecrease,
                SourceType = ClubPointSourceTypeEnum.Admin,
                Description = dto.Reason,
                IdempotencyKey = $"club-point:manual-decrease:{dto.RequestId:N}",
                CreatedByAdminId = adminId
            }, PointOperation.Spend, cancellationToken);
        }

        private async Task<BaseResultDto<ClubPointTransactionVDto>> ChangeAsync(
            ClubPointChangeDto dto,
            PointOperation operation,
            CancellationToken cancellationToken)
        {
            if (dto.UserId <= 0 || dto.Amount <= 0 || string.IsNullOrWhiteSpace(dto.IdempotencyKey))
                return new BaseResultDto<ClubPointTransactionVDto>(false, "CLUB_POINT_REQUEST_INVALID", null);

            await using var transaction = await _context.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var duplicate = await _context.ClubPointTransactions
                .AsNoTracking()
                .Include(item => item.User)
                .FirstOrDefaultAsync(
                    item => item.IdempotencyKey == dto.IdempotencyKey,
                    cancellationToken);
            if (duplicate != null)
            {
                await transaction.CommitAsync(cancellationToken);
                return new BaseResultDto<ClubPointTransactionVDto>(true, MapTransaction(duplicate));
            }

            if (!await _context.Users.AsNoTracking().AnyAsync(item => item.Id == dto.UserId && !item.Deleted, cancellationToken))
                return new BaseResultDto<ClubPointTransactionVDto>(false, Resource.Notification.NothingFound, null);

            if (operation == PointOperation.Earn &&
                dto.PointRuleId.HasValue &&
                !await IsInsideLimitsAsync(dto, cancellationToken))
            {
                await transaction.CommitAsync(cancellationToken);
                return new BaseResultDto<ClubPointTransactionVDto>(true, null);
            }

            var account = await _context.ClubPointAccounts
                .AsTracking()
                .FirstOrDefaultAsync(item => item.UserId == dto.UserId, cancellationToken);
            if (account == null)
            {
                account = new ClubPointAccount
                {
                    UserId = dto.UserId,
                    CreateDate = DateTime.UtcNow,
                    LastUpdateDate = DateTime.UtcNow
                };
                await _context.ClubPointAccounts.AddAsync(account, cancellationToken);
            }

            var availableBefore = account.AvailablePoint;
            var debtBefore = account.DebtPoint;
            ClubPointBalanceChange change;
            try
            {
                change = operation switch
                {
                    PointOperation.Earn => ClubPointBalanceCalculator.Earn(availableBefore, debtBefore, dto.Amount),
                    PointOperation.Spend => ClubPointBalanceCalculator.Spend(availableBefore, debtBefore, dto.Amount),
                    PointOperation.ReverseEarn => ClubPointBalanceCalculator.ReverseEarn(availableBefore, debtBefore, dto.Amount),
                    _ => throw new ArgumentOutOfRangeException(nameof(operation))
                };
            }
            catch (InvalidOperationException exception)
            {
                return new BaseResultDto<ClubPointTransactionVDto>(false, exception.Message, null);
            }

            account.AvailablePoint = change.AvailablePoint;
            account.DebtPoint = change.DebtPoint;
            account.LastUpdateDate = DateTime.UtcNow;
            if (operation == PointOperation.Earn)
                account.LifetimeEarnedPoint = checked(account.LifetimeEarnedPoint + dto.Amount);
            else if (operation == PointOperation.Spend)
                account.LifetimeSpentPoint = checked(account.LifetimeSpentPoint + dto.Amount);
            else
                account.LifetimeReversedPoint = checked(account.LifetimeReversedPoint + dto.Amount);

            var pointTransaction = new ClubPointTransaction
            {
                UserId = dto.UserId,
                PointAccount = account,
                TransactionType = dto.TransactionType,
                Amount = operation == PointOperation.Earn ? dto.Amount : -dto.Amount,
                AvailableBefore = availableBefore,
                AvailableAfter = account.AvailablePoint,
                DebtBefore = debtBefore,
                DebtAfter = account.DebtPoint,
                SourceType = dto.SourceType,
                SourceId = dto.SourceId,
                PointRuleId = dto.PointRuleId,
                ParentTransactionId = dto.ParentTransactionId,
                Description = dto.Description?.Trim(),
                IdempotencyKey = dto.IdempotencyKey.Trim(),
                CreateDate = DateTime.UtcNow,
                CreatedByUserId = dto.CreatedByUserId,
                CreatedByAdminId = dto.CreatedByAdminId
            };
            await _context.ClubPointTransactions.AddAsync(pointTransaction, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            pointTransaction.User = await _context.Users.AsNoTracking()
                .FirstAsync(item => item.Id == dto.UserId, cancellationToken);
            return new BaseResultDto<ClubPointTransactionVDto>(true, MapTransaction(pointTransaction));
        }

        private static ClubPointBalanceVDto MapBalance(ClubPointAccount account)
        {
            return new ClubPointBalanceVDto
            {
                UserId = account.UserId,
                AvailablePoint = account.AvailablePoint,
                DebtPoint = account.DebtPoint,
                LifetimeEarnedPoint = account.LifetimeEarnedPoint,
                LifetimeSpentPoint = account.LifetimeSpentPoint,
                LifetimeReversedPoint = account.LifetimeReversedPoint
            };
        }

        private async Task<bool> IsInsideLimitsAsync(
            ClubPointChangeDto dto,
            CancellationToken cancellationToken)
        {
            if (!dto.DailyLimit.HasValue && !dto.MonthlyLimit.HasValue && !dto.LifetimeLimit.HasValue)
                return true;

            var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TehranTimeZone);
            var dayStart = ToUtc(localNow.Date);
            var monthStart = ToUtc(new DateTime(localNow.Year, localNow.Month, 1));
            var earned = _context.ClubPointTransactions.AsNoTracking().Where(item =>
                item.UserId == dto.UserId &&
                item.PointRuleId == dto.PointRuleId &&
                item.Amount > 0);

            var dailyCount = dto.DailyLimit.HasValue
                ? await earned.CountAsync(item => item.CreateDate >= dayStart, cancellationToken)
                : 0;
            var monthlyCount = dto.MonthlyLimit.HasValue
                ? await earned.CountAsync(item => item.CreateDate >= monthStart, cancellationToken)
                : 0;
            var lifetimeCount = dto.LifetimeLimit.HasValue
                ? await earned.CountAsync(cancellationToken)
                : 0;

            return ClubPointRuleLimitEvaluator.CanAward(
                dailyCount,
                monthlyCount,
                lifetimeCount,
                dto.DailyLimit,
                dto.MonthlyLimit,
                dto.LifetimeLimit);
        }

        private static DateTime ToUtc(DateTime localDate)
        {
            var unspecified = DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, TehranTimeZone);
        }

        private static ClubPointTransactionVDto MapTransaction(ClubPointTransaction item)
        {
            return new ClubPointTransactionVDto
            {
                Id = item.Id,
                UserId = item.UserId,
                UserFullName = item.User == null ? null : $"{item.User.FirstName} {item.User.LastName}".Trim(),
                UserMobile = item.User?.Mobile,
                TransactionType = item.TransactionType,
                Amount = item.Amount,
                AvailableBefore = item.AvailableBefore,
                AvailableAfter = item.AvailableAfter,
                DebtBefore = item.DebtBefore,
                DebtAfter = item.DebtAfter,
                SourceType = item.SourceType,
                SourceId = item.SourceId,
                PointRuleId = item.PointRuleId,
                ParentTransactionId = item.ParentTransactionId,
                Description = item.Description,
                CreateDate = item.CreateDate,
                CreatedByAdminId = item.CreatedByAdminId
            };
        }

        private enum PointOperation
        {
            Earn,
            Spend,
            ReverseEarn
        }
    }
}
