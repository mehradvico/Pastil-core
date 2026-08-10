using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.BenefitSrv.Dto;
using Application.Services.PastilClubSrvs.BenefitSrv.Iface;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.BenefitSrv
{
    public class ClubBenefitWalletService : IClubBenefitWalletService
    {
        private readonly IDataBaseContext _context;

        public ClubBenefitWalletService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<ClubBenefitWalletVDto>> GetAsync(
            long userId,
            bool includeConsumed,
            CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var coupons = _context.ClubCoupons.AsNoTracking().Where(item => item.UserId == userId);
            var deliveries = _context.ClubFreeDeliveryBenefits.AsNoTracking().Where(item => item.UserId == userId);
            var credits = _context.ClubPromotionalWalletCredits.AsNoTracking().Where(item => item.UserId == userId);
            var aiBenefits = _context.PastilAiSubscriptions.AsNoTracking()
                .Where(item => item.UserId == userId && item.ClubRewardRedemptionId.HasValue);
            if (!includeConsumed)
            {
                coupons = coupons.Where(item => !item.Used && item.ExpiresAt > now);
                deliveries = deliveries.Where(item => item.RemainingUsageCount > 0 && item.ExpiresAt > now);
                credits = credits.Where(item => item.Status == ClubPromotionalCreditStatusEnum.Active &&
                    item.RemainingAmount > 0 && item.ExpiresAt > now);
                aiBenefits = aiBenefits.Where(item => item.EndDateUtc > DateTime.UtcNow);
            }

            var result = new ClubBenefitWalletVDto
            {
                Coupons = await coupons.OrderBy(item => item.ExpiresAt).Select(item => new ClubCouponVDto
                {
                    Id = item.Id,
                    RewardTitle = item.RewardRedemption.RewardTemplate.Title,
                    Code = item.Code,
                    ApplicationMethod = item.RewardRedemption.RewardTemplate.ApplicationMethod,
                    RewardType = item.RewardRedemption.RewardTemplate.RewardType,
                    BenefitValue = item.RewardRedemption.RewardTemplate.BenefitValue,
                    MaximumBenefitValue = item.RewardRedemption.RewardTemplate.MaximumBenefitValue,
                    ExpiresAt = item.ExpiresAt,
                    Used = item.Used
                }).ToListAsync(cancellationToken),
                FreeDeliveries = await deliveries.OrderBy(item => item.ExpiresAt).Select(item => new ClubFreeDeliveryVDto
                {
                    Id = item.Id,
                    RewardTitle = item.RewardRedemption.RewardTemplate.Title,
                    StoreId = item.StoreId,
                    CityId = item.CityId,
                    MaximumDeliveryAmount = item.MaximumDeliveryAmount,
                    RemainingUsageCount = item.RemainingUsageCount,
                    ExpiresAt = item.ExpiresAt
                }).ToListAsync(cancellationToken),
                PromotionalCredits = await credits.OrderBy(item => item.ExpiresAt).Select(item => new ClubPromotionalCreditVDto
                {
                    Id = item.Id,
                    RewardTitle = item.RewardRedemption.RewardTemplate.Title,
                    OriginalAmount = item.OriginalAmount,
                    RemainingAmount = item.RemainingAmount,
                    ServiceScopeType = item.ServiceScopeType,
                    ServiceScopeId = item.ServiceScopeId,
                    ExpiresAt = item.ExpiresAt,
                    Status = item.Status
                }).ToListAsync(cancellationToken),
                PastilAIBenefits = await aiBenefits.OrderBy(item => item.EndDateUtc).Select(item => new ClubPastilAIBenefitVDto
                {
                    Id = item.Id,
                    RewardTitle = item.ClubRewardRedemption.RewardTemplate.Title,
                    PlanId = item.PlanId,
                    PlanName = item.Plan.Name,
                    Status = item.Status,
                    StartDateUtc = item.StartDateUtc,
                    EndDateUtc = item.EndDateUtc
                }).ToListAsync(cancellationToken)
            };
            return new BaseResultDto<ClubBenefitWalletVDto>(true, result);
        }
    }
}
