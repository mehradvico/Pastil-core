using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.PastilClubSrvs.PointSrv;
using Application.Services.PastilClubSrvs.BenefitSrv.Iface;
using Application.Services.PastilClubSrvs.BenefitSrv;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Iface;
using Application.Services.PastilClubSrvs.RewardOfferSrv;
using Application.Services.PastilClubSrvs.RewardRedemptionSrv.Dto;
using Application.Services.PastilClubSrvs.RewardRedemptionSrv.Iface;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.RewardRedemptionSrv
{
    public class ClubRewardRedemptionService : IClubRewardRedemptionService
    {
        private readonly IDataBaseContext _context;
        private readonly IClubRewardEligibilityService _eligibilityService;
        private readonly IClubRewardBenefitFactory _benefitFactory;

        public ClubRewardRedemptionService(
            IDataBaseContext context,
            IClubRewardEligibilityService eligibilityService,
            IClubRewardBenefitFactory benefitFactory)
        {
            _context = context;
            _eligibilityService = eligibilityService;
            _benefitFactory = benefitFactory;
        }

        public async Task<BaseResultDto<ClubRewardRedemptionVDto>> FindAdminAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            var item = await Project(_context.ClubRewardRedemptions.AsNoTracking())
                .FirstOrDefaultAsync(redemption => redemption.Id == id, cancellationToken);
            return Result(item);
        }

        public Task<ClubRewardRedemptionSearchDto> SearchAdminAsync(
            ClubRewardRedemptionInputDto dto,
            CancellationToken cancellationToken = default) =>
            SearchAsync(_context.ClubRewardRedemptions.AsNoTracking(), dto, cancellationToken);

        public async Task<BaseResultDto<ClubRewardRedemptionVDto>> FindUserAsync(
            long id,
            long userId,
            CancellationToken cancellationToken = default)
        {
            var item = await Project(_context.ClubRewardRedemptions.AsNoTracking())
                .FirstOrDefaultAsync(redemption => redemption.Id == id && redemption.UserId == userId, cancellationToken);
            return item == null
                ? new BaseResultDto<ClubRewardRedemptionVDto>(false, "CLUB_REWARD_NOT_FOUND", null)
                : new BaseResultDto<ClubRewardRedemptionVDto>(true, item);
        }

        public Task<ClubRewardRedemptionSearchDto> SearchUserAsync(
            ClubRewardRedemptionInputDto dto,
            long userId,
            CancellationToken cancellationToken = default)
        {
            dto.UserId = userId;
            return SearchAsync(
                _context.ClubRewardRedemptions.AsNoTracking().Where(item => item.UserId == userId),
                dto,
                cancellationToken);
        }

        public async Task<BaseResultDto<ClubRewardRedemptionVDto>> RedeemAsync(
            long userId,
            long offerId,
            CancellationToken cancellationToken = default)
        {
            if (userId <= 0 || offerId <= 0)
                return Failure("CLUB_REWARD_REQUEST_INVALID");

            var redemptionKey = $"club-reward-redeem:{offerId}";
            await using var databaseTransaction = await _context.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var offer = await _context.ClubRewardOffers
                .FromSqlInterpolated($"SELECT * FROM ClubRewardOffers WITH (UPDLOCK, ROWLOCK) WHERE Id = {offerId} AND UserId = {userId}")
                .AsTracking()
                .FirstOrDefaultAsync(cancellationToken);
            if (offer == null)
                return Failure("CLUB_REWARD_NOT_FOUND");

            var existing = await _context.ClubRewardRedemptions.AsNoTracking()
                .FirstOrDefaultAsync(item => item.IdempotencyKey == redemptionKey, cancellationToken);
            if (existing != null)
            {
                if (existing.UserId != userId)
                    return Failure("CLUB_REWARD_NOT_FOUND");

                await databaseTransaction.CommitAsync(cancellationToken);
                return await FindUserAsync(existing.Id, userId, cancellationToken);
            }

            var now = DateTimeOffset.UtcNow;
            offer.RewardTemplate = await _context.ClubRewardTemplates.AsNoTracking()
                .Include(item => item.Targets)
                .Include(item => item.PastilAITarget)
                .FirstAsync(item => item.Id == offer.RewardTemplateId, cancellationToken);
            if (offer.Status != ClubRewardOfferStatusEnum.Approved)
                return Failure("CLUB_REWARD_NOT_APPROVED");
            if (offer.ExpiresAt <= now)
                return Failure("CLUB_REWARD_EXPIRED");
            if (!ClubRewardOfferPolicy.IsVisible(
                    offer.Status,
                    offer.ExpiresAt,
                    offer.RewardTemplate.Active,
                    offer.RewardTemplate.StartDate,
                    offer.RewardTemplate.EndDate,
                    now))
                return Failure("CLUB_REWARD_TEMPLATE_NOT_AVAILABLE");
            var petEligible = await _eligibilityService.IsPetEligibleAsync(
                userId,
                offer.RewardTemplateId,
                cancellationToken);
            if (!petEligible)
                return Failure("CLUB_REWARD_PET_NOT_ELIGIBLE");

            var account = await _context.ClubPointAccounts
                .FromSqlInterpolated($"SELECT * FROM ClubPointAccounts WITH (UPDLOCK, ROWLOCK) WHERE UserId = {userId}")
                .AsTracking()
                .FirstOrDefaultAsync(cancellationToken);
            if (account == null)
                return Failure("CLUB_POINT_NOT_ENOUGH");
            if (!ClubRewardOfferPolicy.CanRedeem(
                    account.AvailablePoint,
                    account.DebtPoint,
                    offer.PointCostSnapshot,
                    petEligible))
                return Failure("CLUB_POINT_NOT_ENOUGH");

            ClubPointBalanceChange balance;
            try
            {
                balance = ClubPointBalanceCalculator.Spend(
                    account.AvailablePoint,
                    account.DebtPoint,
                    offer.PointCostSnapshot);
            }
            catch (InvalidOperationException exception)
            {
                return Failure(exception.Message);
            }

            var availableBefore = account.AvailablePoint;
            var debtBefore = account.DebtPoint;
            account.AvailablePoint = balance.AvailablePoint;
            account.DebtPoint = balance.DebtPoint;
            account.LifetimeSpentPoint = checked(account.LifetimeSpentPoint + offer.PointCostSnapshot);
            account.LastUpdateDate = DateTime.UtcNow;

            var pointTransaction = new ClubPointTransaction
            {
                UserId = userId,
                PointAccountId = account.Id,
                PointAccount = account,
                TransactionType = ClubPointTransactionTypeEnum.Spend,
                Amount = -offer.PointCostSnapshot,
                AvailableBefore = availableBefore,
                AvailableAfter = account.AvailablePoint,
                DebtBefore = debtBefore,
                DebtAfter = account.DebtPoint,
                SourceType = ClubPointSourceTypeEnum.RewardRedemption,
                SourceId = offer.Id,
                Description = $"Redeem Pastil Club reward offer {offer.Id}",
                IdempotencyKey = $"club-point:reward-redeem:{offer.Id}",
                CreateDate = DateTime.UtcNow,
                CreatedByUserId = userId
            };
            await _context.ClubPointTransactions.AddAsync(pointTransaction, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var redemption = new ClubRewardRedemption
            {
                UserId = userId,
                RewardOfferId = offer.Id,
                RewardTemplateId = offer.RewardTemplateId,
                PointTransactionId = pointTransaction.Id,
                PointSpent = offer.PointCostSnapshot,
                RedeemedDate = now,
                ExpiresAt = offer.ExpiresAt,
                Status = ClubRewardRedemptionStatusEnum.Completed,
                IdempotencyKey = redemptionKey
            };
            await _context.ClubRewardRedemptions.AddAsync(redemption, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            ClubRewardBenefitResult benefit;
            try
            {
                benefit = await _benefitFactory.CreateAsync(
                    redemption,
                    offer.RewardTemplate,
                    cancellationToken);
            }
            catch (InvalidOperationException exception)
            {
                await databaseTransaction.RollbackAsync(cancellationToken);
                return Failure(exception.Message);
            }

            redemption.BenefitType = benefit.BenefitType;
            redemption.BenefitReferenceId = benefit.ReferenceId;

            offer.Status = ClubRewardOfferStatusEnum.Redeemed;
            offer.RedeemedDate = now;
            offer.UpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            pointTransaction.RewardRedemptionId = redemption.Id;
            await _context.SaveChangesAsync(cancellationToken);
            await databaseTransaction.CommitAsync(cancellationToken);

            return await FindUserAsync(redemption.Id, userId, cancellationToken);
        }

        private static async Task<ClubRewardRedemptionSearchDto> SearchAsync(
            IQueryable<ClubRewardRedemption> query,
            ClubRewardRedemptionInputDto dto,
            CancellationToken cancellationToken)
        {
            dto.PageIndex = Math.Max(1, dto.PageIndex);
            dto.PageSize = Math.Clamp(dto.PageSize, 1, 100);
            if (dto.UserId.HasValue)
                query = query.Where(item => item.UserId == dto.UserId.Value);
            if (dto.RewardTemplateId.HasValue)
                query = query.Where(item => item.RewardTemplateId == dto.RewardTemplateId.Value);
            if (dto.Status.HasValue)
                query = query.Where(item => item.Status == dto.Status.Value);
            if (dto.RewardType.HasValue)
                query = query.Where(item => item.RewardTemplate.RewardType == dto.RewardType.Value);
            if (dto.FromDate.HasValue)
                query = query.Where(item => item.RedeemedDate >= dto.FromDate.Value);
            if (dto.ToDate.HasValue)
                query = query.Where(item => item.RedeemedDate <= dto.ToDate.Value);
            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var text = dto.Q.Trim();
                query = query.Where(item =>
                    item.User.Mobile.Contains(text) ||
                    item.User.FirstName.Contains(text) ||
                    item.User.LastName.Contains(text) ||
                    item.RewardTemplate.Title.Contains(text) ||
                    item.IdempotencyKey.Contains(text));
            }

            query = dto.SortBy == SortEnum.Old
                ? query.OrderBy(item => item.Id)
                : query.OrderByDescending(item => item.Id);
            return new ClubRewardRedemptionSearchDto(dto)
            {
                TotalCount = await query.CountAsync(cancellationToken),
                List = await Project(query)
                    .Skip((dto.PageIndex - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .ToListAsync(cancellationToken)
            };
        }

        private static IQueryable<ClubRewardRedemptionVDto> Project(IQueryable<ClubRewardRedemption> query) =>
            query.Select(item => new ClubRewardRedemptionVDto
            {
                Id = item.Id,
                RewardOfferId = item.RewardOfferId,
                RewardTemplateId = item.RewardTemplateId,
                UserId = item.UserId,
                UserFullName = (item.User.FirstName + " " + item.User.LastName).Trim(),
                UserMobile = item.User.Mobile,
                RewardTitle = item.RewardTemplate.Title,
                RewardType = item.RewardTemplate.RewardType,
                PointTransactionId = item.PointTransactionId,
                PointSpent = item.PointSpent,
                RemainingPoint = item.PointTransaction.AvailableAfter,
                BenefitType = item.BenefitType,
                BenefitReferenceId = item.BenefitReferenceId,
                RedeemedDate = item.RedeemedDate,
                ExpiresAt = item.ExpiresAt,
                Status = item.Status
            });

        private static BaseResultDto<ClubRewardRedemptionVDto> Result(ClubRewardRedemptionVDto item) =>
            item == null
                ? new BaseResultDto<ClubRewardRedemptionVDto>(false, Resource.Notification.NothingFound, null)
                : new BaseResultDto<ClubRewardRedemptionVDto>(true, item);

        private static BaseResultDto<ClubRewardRedemptionVDto> Failure(string error) =>
            new(false, error, null);
    }
}
