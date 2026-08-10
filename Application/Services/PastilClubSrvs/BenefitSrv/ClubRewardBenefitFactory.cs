using Application.Common.Enumerable.Code;
using Application.Services.PastilClubSrvs.BenefitSrv.Iface;
using Entities.Entities;
using Entities.Entities.PastilAIField;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.BenefitSrv
{
    public class ClubRewardBenefitFactory : IClubRewardBenefitFactory
    {
        private readonly IDataBaseContext _context;

        public ClubRewardBenefitFactory(IDataBaseContext context)
        {
            _context = context;
        }

        public Task<ClubRewardBenefitResult> CreateAsync(
            ClubRewardRedemption redemption,
            ClubRewardTemplate template,
            CancellationToken cancellationToken = default) => template.RewardType switch
            {
                ClubRewardTypeEnum.FixedDiscount or ClubRewardTypeEnum.PercentageDiscount =>
                    CreateCouponAsync(redemption, template, cancellationToken),
                ClubRewardTypeEnum.FreeDelivery =>
                    CreateFreeDeliveryAsync(redemption, template, cancellationToken),
                ClubRewardTypeEnum.PromotionalWalletCredit =>
                    CreatePromotionalCreditAsync(redemption, template, cancellationToken),
                ClubRewardTypeEnum.PastilAIPlanFixedDiscount or ClubRewardTypeEnum.PastilAIPlanPercentageDiscount =>
                    CreateCouponAsync(redemption, template, cancellationToken),
                ClubRewardTypeEnum.PastilAIFreeDays or ClubRewardTypeEnum.PastilAIFreeMonth or ClubRewardTypeEnum.PastilAIUpgrade =>
                    CreatePastilAISubscriptionAsync(redemption, template, cancellationToken),
                _ => throw new InvalidOperationException("CLUB_REWARD_BENEFIT_TYPE_INVALID")
            };

        private async Task<ClubRewardBenefitResult> CreateCouponAsync(
            ClubRewardRedemption redemption,
            ClubRewardTemplate template,
            CancellationToken cancellationToken)
        {
            var typeLabel = ResolveRebateTypeLabel(template.ApplicationMethod);
            var typeId = await _context.Codes.AsNoTracking()
                .Where(item => item.Label == typeLabel)
                .Select(item => (long?)item.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (!typeId.HasValue)
                throw new InvalidOperationException("CLUB_REWARD_REBATE_TYPE_NOT_FOUND");

            var value = template.BenefitValue ?? throw new InvalidOperationException("CLUB_REWARD_BENEFIT_VALUE_REQUIRED");
            var code = $"club-{redemption.UserId:x}-{Guid.NewGuid():N}"[..32].ToLowerInvariant();
            var productTarget = template.Targets.FirstOrDefault(item => item.TargetType == ClubRewardTargetTypeEnum.Product);
            var rebate = new Rebate
            {
                Name = template.Title,
                UserId = redemption.UserId,
                TypeId = typeId.Value,
                CodeValue = code,
                PriceValue = decimal.ToDouble(value),
                MinCartPrice = 0,
                StartDatetime = redemption.RedeemedDate.UtcDateTime,
                EndDatetime = redemption.ExpiresAt.UtcDateTime,
                IsPriceRebate = template.RewardType is ClubRewardTypeEnum.FixedDiscount or ClubRewardTypeEnum.PastilAIPlanFixedDiscount,
                Active = true,
                Deleted = false,
                UseCount = 1,
                UsedCount = 0,
                MaxUsePerUser = 1,
                ProductId = productTarget?.TargetId
            };
            await _context.Rebate.AddAsync(rebate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var coupon = new ClubCoupon
            {
                RewardRedemptionId = redemption.Id,
                UserId = redemption.UserId,
                RebateId = rebate.Id,
                Code = code,
                ExpiresAt = redemption.ExpiresAt,
                CreateDate = DateTime.UtcNow
            };
            await _context.ClubCoupons.AddAsync(coupon, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return new ClubRewardBenefitResult(ClubRewardBenefitTypeEnum.Discount, coupon.Id);
        }

        private async Task<ClubRewardBenefitResult> CreateFreeDeliveryAsync(
            ClubRewardRedemption redemption,
            ClubRewardTemplate template,
            CancellationToken cancellationToken)
        {
            var storeId = template.Targets.FirstOrDefault(item => item.TargetType == ClubRewardTargetTypeEnum.Store)?.TargetId;
            var cityId = template.Targets.FirstOrDefault(item => item.TargetType == ClubRewardTargetTypeEnum.City)?.TargetId;
            var benefit = new ClubFreeDeliveryBenefit
            {
                RewardRedemptionId = redemption.Id,
                UserId = redemption.UserId,
                StoreId = storeId,
                CityId = cityId,
                MaximumDeliveryAmount = template.MaximumBenefitValue ?? template.BenefitValue,
                RemainingUsageCount = 1,
                ExpiresAt = redemption.ExpiresAt,
                CreateDate = DateTime.UtcNow
            };
            await _context.ClubFreeDeliveryBenefits.AddAsync(benefit, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return new ClubRewardBenefitResult(ClubRewardBenefitTypeEnum.FreeDelivery, benefit.Id);
        }

        private async Task<ClubRewardBenefitResult> CreatePromotionalCreditAsync(
            ClubRewardRedemption redemption,
            ClubRewardTemplate template,
            CancellationToken cancellationToken)
        {
            var amount = template.BenefitValue ?? throw new InvalidOperationException("CLUB_REWARD_BENEFIT_VALUE_REQUIRED");
            var target = template.Targets.FirstOrDefault(item => item.TargetType != ClubRewardTargetTypeEnum.Global);
            var credit = new ClubPromotionalWalletCredit
            {
                UserId = redemption.UserId,
                RewardRedemptionId = redemption.Id,
                OriginalAmount = amount,
                RemainingAmount = amount,
                ServiceScopeType = target?.TargetType ?? ClubRewardTargetTypeEnum.Global,
                ServiceScopeId = target?.TargetId,
                ExpiresAt = redemption.ExpiresAt,
                Status = ClubPromotionalCreditStatusEnum.Active,
                CreateDate = DateTime.UtcNow
            };
            await _context.ClubPromotionalWalletCredits.AddAsync(credit, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await AddCostAsync(redemption, template, amount, target, cancellationToken);
            return new ClubRewardBenefitResult(ClubRewardBenefitTypeEnum.PromotionalWalletCredit, credit.Id);
        }

        private async Task<ClubRewardBenefitResult> CreatePastilAISubscriptionAsync(
            ClubRewardRedemption redemption,
            ClubRewardTemplate template,
            CancellationToken cancellationToken)
        {
            var target = template.PastilAITarget ?? await _context.ClubRewardPastilAITargets.AsNoTracking()
                .FirstOrDefaultAsync(item => item.RewardTemplateId == template.Id, cancellationToken);
            if (target == null)
                throw new InvalidOperationException("CLUB_REWARD_PASTIL_AI_TARGET_REQUIRED");

            var planId = target.IsUpgrade ? target.TargetPlanId : target.PlanId;
            if (!planId.HasValue)
                throw new InvalidOperationException("CLUB_REWARD_PASTIL_AI_PLAN_REQUIRED");
            var plan = await _context.PastilAiPlans.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == planId.Value && item.Active && !item.Deleted, cancellationToken);
            if (plan == null)
                throw new InvalidOperationException("CLUB_REWARD_PASTIL_AI_PLAN_NOT_FOUND");

            var start = DateTime.UtcNow;
            var days = target.FreeDays ?? (template.RewardType == ClubRewardTypeEnum.PastilAIFreeMonth ? plan.DurationDays : 1);
            var end = ClubBenefitPolicy.ResolvePastilAIEnd(start, days, redemption.ExpiresAt);
            if (end <= start)
                throw new InvalidOperationException("CLUB_REWARD_EXPIRED");

            var subscription = new PastilAiSubscription
            {
                UserId = redemption.UserId,
                PlanId = plan.Id,
                Status = PastilAiSubscriptionStatus.Active,
                PriceSnapshot = 0,
                RebatePrice = plan.Price,
                FromWallet = false,
                WalletPrice = 0,
                CreateDateUtc = start,
                StartDateUtc = start,
                EndDateUtc = end,
                ClubRewardRedemptionId = redemption.Id
            };
            await _context.PastilAiSubscriptions.AddAsync(subscription, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await AddCostAsync(redemption, template, plan.Price, null, cancellationToken);
            return new ClubRewardBenefitResult(ClubRewardBenefitTypeEnum.PastilAI, subscription.Id);
        }

        private async Task AddCostAsync(
            ClubRewardRedemption redemption,
            ClubRewardTemplate template,
            decimal amount,
            ClubRewardTarget target,
            CancellationToken cancellationToken)
        {
            await _context.ClubRewardCostTransactions.AddAsync(new ClubRewardCostTransaction
            {
                RewardRedemptionId = redemption.Id,
                UserId = redemption.UserId,
                BusinessType = target?.TargetType ?? ClubRewardTargetTypeEnum.Global,
                BusinessId = target?.TargetId,
                RewardType = template.RewardType,
                GrossValue = amount,
                PastilFundedValue = amount,
                CreateDate = DateTime.UtcNow
            }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static string ResolveRebateTypeLabel(ClubRewardApplicationMethodEnum method) => method switch
        {
            ClubRewardApplicationMethodEnum.ProductOrder => RebateTypeLabels.Cart,
            ClubRewardApplicationMethodEnum.CompanionReservation => RebateTypeLabels.CompanionReserve,
            ClubRewardApplicationMethodEnum.PansionReservation => RebateTypeLabels.PansionReserve,
            ClubRewardApplicationMethodEnum.PastilAI => RebateTypeLabels.PastilAI,
            _ => throw new InvalidOperationException("CLUB_REWARD_APPLICATION_METHOD_INVALID")
        };
    }
}
