using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.PointEventSrv.Dto;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using Application.Services.PastilClubSrvs.PointSrv.Iface;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.PointEventSrv
{
    public class ClubPointEventService : IClubPointEventService
    {
        private readonly IDataBaseContext _context;
        private readonly IClubPointService _clubPointService;

        public ClubPointEventService(IDataBaseContext context, IClubPointService clubPointService)
        {
            _context = context;
            _clubPointService = clubPointService;
        }

        public async Task<BaseResultDto<ClubPointTransactionVDto>> AwardAsync(
            ClubPointEventDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!IsValid(dto))
                return new BaseResultDto<ClubPointTransactionVDto>(false, "CLUB_POINT_EVENT_INVALID", null);

            var idempotencyKey = ClubPointEventKeyFactory.BuildAwardKey(dto);
            var existing = await _context.ClubPointTransactions.AsNoTracking()
                .AnyAsync(item => item.IdempotencyKey == idempotencyKey, cancellationToken);
            if (existing)
                return new BaseResultDto<ClubPointTransactionVDto>(true, null);

            var now = DateTimeOffset.UtcNow;
            var rule = await _context.ClubPointRules.AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.EventType == dto.EventType &&
                    item.Active &&
                    (!item.StartDate.HasValue || item.StartDate <= now) &&
                    (!item.EndDate.HasValue || item.EndDate >= now),
                    cancellationToken);
            if (rule == null)
                return new BaseResultDto<ClubPointTransactionVDto>(true, null);

            return await _clubPointService.EarnAsync(new ClubPointChangeDto
            {
                UserId = dto.UserId,
                Amount = rule.PointAmount,
                TransactionType = dto.EventType is
                    ClubPointEventTypeEnum.UserReferralReferrer or
                    ClubPointEventTypeEnum.UserReferralReferee or
                    ClubPointEventTypeEnum.BusinessReferralUser
                        ? ClubPointTransactionTypeEnum.ReferralEarn
                        : ClubPointTransactionTypeEnum.Earn,
                SourceType = dto.SourceType,
                SourceId = dto.SourceId,
                PointRuleId = rule.Id,
                DailyLimit = rule.DailyLimit,
                MonthlyLimit = rule.MonthlyLimit,
                LifetimeLimit = rule.LifetimeLimit,
                Description = dto.Description,
                IdempotencyKey = idempotencyKey,
                CreatedByUserId = dto.UserId
            }, cancellationToken);
        }

        public async Task<BaseResultDto<ClubPointTransactionVDto>> ReverseAsync(
            ClubPointEventDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!IsValid(dto))
                return new BaseResultDto<ClubPointTransactionVDto>(false, "CLUB_POINT_EVENT_INVALID", null);

            var originalKey = ClubPointEventKeyFactory.BuildAwardKey(dto);
            var original = await _context.ClubPointTransactions.AsNoTracking()
                .FirstOrDefaultAsync(item => item.IdempotencyKey == originalKey, cancellationToken);
            if (original == null)
                return new BaseResultDto<ClubPointTransactionVDto>(true, null);

            return await _clubPointService.ReverseEarnAsync(new ClubPointChangeDto
            {
                UserId = dto.UserId,
                Amount = Math.Abs(original.Amount),
                SourceType = dto.SourceType,
                SourceId = dto.SourceId,
                PointRuleId = original.PointRuleId,
                ParentTransactionId = original.Id,
                Description = dto.Description,
                IdempotencyKey = ClubPointEventKeyFactory.BuildReverseKey(dto),
                CreatedByUserId = dto.UserId
            }, cancellationToken);
        }

        private static bool IsValid(ClubPointEventDto dto)
        {
            return dto != null &&
                   dto.UserId > 0 &&
                   Enum.IsDefined(dto.EventType) &&
                   Enum.IsDefined(dto.SourceType) &&
                   !string.IsNullOrWhiteSpace(dto.SourceKey);
        }

    }
}
