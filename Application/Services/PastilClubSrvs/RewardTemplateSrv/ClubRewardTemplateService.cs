using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.PastilClubSrvs.RewardTemplateSrv.Dto;
using Application.Services.PastilClubSrvs.RewardTemplateSrv.Iface;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.RewardTemplateSrv
{
    public class ClubRewardTemplateService : IClubRewardTemplateService
    {
        private readonly IDataBaseContext _context;

        public ClubRewardTemplateService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<ClubRewardTemplateDto>> FindAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            var item = await Project(_context.ClubRewardTemplates.AsNoTracking())
                .FirstOrDefaultAsync(template => template.Id == id, cancellationToken);
            return item == null
                ? new BaseResultDto<ClubRewardTemplateDto>(false, Resource.Notification.NothingFound, null)
                : new BaseResultDto<ClubRewardTemplateDto>(true, item);
        }

        public async Task<ClubRewardTemplateSearchDto> SearchAsync(
            ClubRewardTemplateInputDto dto,
            CancellationToken cancellationToken = default)
        {
            dto.PageIndex = Math.Max(1, dto.PageIndex);
            dto.PageSize = Math.Clamp(dto.PageSize, 1, 100);
            var query = _context.ClubRewardTemplates.AsNoTracking().AsQueryable();

            if (dto.Available.HasValue)
                query = query.Where(item => item.Active == dto.Available.Value);
            if (dto.RewardType.HasValue)
                query = query.Where(item => item.RewardType == dto.RewardType.Value);
            if (dto.TargetType.HasValue)
                query = query.Where(item => item.Targets.Any(target => target.TargetType == dto.TargetType.Value));
            if (dto.PetTypeId.HasValue)
                query = query.Where(item => item.PetTypes.Any(pet => pet.PetTypeId == dto.PetTypeId.Value));
            if (dto.IsManualAllowed.HasValue)
                query = query.Where(item => item.IsManualAllowed == dto.IsManualAllowed.Value);
            if (dto.IsAutomationAllowed.HasValue)
                query = query.Where(item => item.IsAutomationAllowed == dto.IsAutomationAllowed.Value);
            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var text = dto.Q.Trim();
                query = query.Where(item =>
                    item.Name.Contains(text) ||
                    item.Title.Contains(text) ||
                    item.ShortDescription.Contains(text));
            }

            query = dto.SortBy == SortEnum.Old
                ? query.OrderBy(item => item.Id)
                : query.OrderByDescending(item => item.Id);

            return new ClubRewardTemplateSearchDto(dto)
            {
                TotalCount = await query.CountAsync(cancellationToken),
                List = await Project(query)
                    .Skip((dto.PageIndex - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .ToListAsync(cancellationToken)
            };
        }

        public async Task<BaseResultDto<ClubRewardTemplateDto>> InsertAsync(
            ClubRewardTemplateDto dto,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidateAsync(dto, cancellationToken);
            if (validation != null)
                return validation;

            var item = new ClubRewardTemplate { CreateDate = DateTime.UtcNow };
            Apply(item, dto);
            ReplaceChildren(item, dto);
            await _context.ClubRewardTemplates.AddAsync(item, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return await FindAsync(item.Id, cancellationToken);
        }

        public async Task<BaseResultDto<ClubRewardTemplateDto>> UpdateAsync(
            ClubRewardTemplateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto.Id <= 0)
                return new BaseResultDto<ClubRewardTemplateDto>(false, Resource.Notification.InvalidData, dto);

            var validation = await ValidateAsync(dto, cancellationToken);
            if (validation != null)
                return validation;

            var item = await _context.ClubRewardTemplates.AsTracking()
                .Include(template => template.Targets)
                .Include(template => template.PetTypes)
                .Include(template => template.PastilAITarget)
                .FirstOrDefaultAsync(template => template.Id == dto.Id, cancellationToken);
            if (item == null)
                return new BaseResultDto<ClubRewardTemplateDto>(false, Resource.Notification.NothingFound, null);

            Apply(item, dto);
            item.UpdateDate = DateTime.UtcNow;
            _context.ClubRewardTargets.RemoveRange(item.Targets);
            _context.ClubRewardPetTypes.RemoveRange(item.PetTypes);
            if (item.PastilAITarget != null)
                _context.ClubRewardPastilAITargets.Remove(item.PastilAITarget);
            ReplaceChildren(item, dto);
            await _context.SaveChangesAsync(cancellationToken);
            return await FindAsync(item.Id, cancellationToken);
        }

        private async Task<BaseResultDto<ClubRewardTemplateDto>> ValidateAsync(
            ClubRewardTemplateDto dto,
            CancellationToken cancellationToken)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Title) ||
                dto.PointCost <= 0 ||
                !Enum.IsDefined(dto.RewardType) ||
                !Enum.IsDefined(dto.ApplicationMethod) ||
                !Enum.IsDefined(dto.ExpirationType) ||
                !Enum.IsDefined(dto.NotificationLevel) ||
                dto.FundingType != ClubRewardFundingTypeEnum.Pastil ||
                dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate ||
                dto.ExpirationType == ClubRewardExpirationTypeEnum.FixedDate && !dto.FixedExpirationDate.HasValue ||
                dto.FixedExpirationDate.HasValue && dto.FixedExpirationDate <= DateTimeOffset.UtcNow ||
                dto.BenefitValue.HasValue && dto.BenefitValue <= 0 ||
                dto.MaximumBenefitValue.HasValue && dto.MaximumBenefitValue <= 0 ||
                IsPercentageReward(dto.RewardType) && dto.BenefitValue > 100)
            {
                return new BaseResultDto<ClubRewardTemplateDto>(false, Resource.Notification.InvalidData, dto);
            }

            if (dto.PictureId.HasValue &&
                !await _context.Pictures.AsNoTracking().AnyAsync(item => item.Id == dto.PictureId.Value, cancellationToken))
                return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_PICTURE_NOT_FOUND", dto);

            var requestedPetTypeIds = dto.PetTypeIds ?? [];
            var petTypeIds = requestedPetTypeIds.Distinct().ToList();
            if (petTypeIds.Count != requestedPetTypeIds.Count ||
                await _context.Pets.AsNoTracking().CountAsync(item => petTypeIds.Contains(item.Id), cancellationToken) != petTypeIds.Count)
                return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_PET_TYPE_INVALID", dto);

            var targets = dto.Targets ?? [];
            if (targets.Count == 0 || targets.Any(item => item == null || !Enum.IsDefined(item.TargetType)))
                return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_TARGET_INVALID", dto);

            if (IsPastilAIReward(dto.RewardType) &&
                (!targets.Any(item => item.TargetType == ClubRewardTargetTypeEnum.PastilAIPlan) ||
                 dto.ApplicationMethod != ClubRewardApplicationMethodEnum.PastilAI ||
                 dto.PastilAITarget == null))
                return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_PASTIL_AI_PLAN_REQUIRED", dto);

            if (dto.RewardType == ClubRewardTypeEnum.FreeDelivery &&
                (dto.ApplicationMethod != ClubRewardApplicationMethodEnum.ProductOrder ||
                 targets.Any(item => item.TargetType is not ClubRewardTargetTypeEnum.Global and
                     not ClubRewardTargetTypeEnum.Store and not ClubRewardTargetTypeEnum.City)))
                return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_FREE_DELIVERY_TARGET_INVALID", dto);

            if (dto.RewardType == ClubRewardTypeEnum.PromotionalWalletCredit && !dto.BenefitValue.HasValue)
                return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_BENEFIT_VALUE_REQUIRED", dto);

            if (dto.RewardType is ClubRewardTypeEnum.FixedDiscount or ClubRewardTypeEnum.PercentageDiscount &&
                dto.ApplicationMethod == ClubRewardApplicationMethodEnum.PastilAI)
                return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_APPLICATION_METHOD_INVALID", dto);

            if (!IsPastilAIReward(dto.RewardType) && dto.PastilAITarget != null)
                return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_PASTIL_AI_TARGET_NOT_ALLOWED", dto);

            if (dto.PastilAITarget != null)
            {
                var aiTarget = dto.PastilAITarget;
                if (aiTarget.PlanId <= 0 ||
                    !await _context.PastilAiPlans.AsNoTracking().AnyAsync(item => item.Id == aiTarget.PlanId, cancellationToken) ||
                    aiTarget.TargetPlanId.HasValue &&
                    !await _context.PastilAiPlans.AsNoTracking().AnyAsync(item => item.Id == aiTarget.TargetPlanId.Value, cancellationToken) ||
                    aiTarget.FreeDays.HasValue && aiTarget.FreeDays <= 0 ||
                    dto.RewardType == ClubRewardTypeEnum.PastilAIUpgrade &&
                    (!aiTarget.IsUpgrade || !aiTarget.TargetPlanId.HasValue))
                    return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_PASTIL_AI_TARGET_INVALID", dto);

                if (!targets.Any(item => item.TargetType == ClubRewardTargetTypeEnum.PastilAIPlan &&
                    item.TargetId == aiTarget.PlanId))
                    return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_PASTIL_AI_TARGET_MISMATCH", dto);
            }

            var duplicateTarget = targets.GroupBy(item => new { item.TargetType, item.TargetId }).Any(group => group.Count() > 1);
            if (duplicateTarget)
                return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_TARGET_DUPLICATED", dto);

            foreach (var target in targets)
            {
                if (!await TargetExistsAsync(target, cancellationToken))
                    return new BaseResultDto<ClubRewardTemplateDto>(false, "CLUB_REWARD_TARGET_NOT_FOUND", dto);
            }

            return null;
        }

        private async Task<bool> TargetExistsAsync(ClubRewardTargetDto target, CancellationToken cancellationToken)
        {
            if (target.TargetType is ClubRewardTargetTypeEnum.Global or ClubRewardTargetTypeEnum.PastilAI)
                return !target.TargetId.HasValue || target.TargetId == 0;
            if (!target.TargetId.HasValue || target.TargetId <= 0)
                return false;

            var id = target.TargetId.Value;
            return target.TargetType switch
            {
                ClubRewardTargetTypeEnum.Store => await _context.Stores.AsNoTracking().AnyAsync(item => item.Id == id, cancellationToken),
                ClubRewardTargetTypeEnum.Product => await _context.Products.AsNoTracking().AnyAsync(item => item.Id == id, cancellationToken),
                ClubRewardTargetTypeEnum.ProductCategory => await _context.Categories.IgnoreQueryFilters().AsNoTracking().AnyAsync(item => item.Id == id, cancellationToken),
                ClubRewardTargetTypeEnum.Companion => await _context.Companions.AsNoTracking().AnyAsync(item => item.Id == id, cancellationToken),
                ClubRewardTargetTypeEnum.Assistance => await _context.Assistances.AsNoTracking().AnyAsync(item => item.Id == id, cancellationToken),
                ClubRewardTargetTypeEnum.CompanionPackage => await _context.CompanionAssistancePackages.AsNoTracking().AnyAsync(item => item.Id == id, cancellationToken),
                ClubRewardTargetTypeEnum.Pansion => await _context.Pansions.AsNoTracking().AnyAsync(item => item.Id == id, cancellationToken),
                ClubRewardTargetTypeEnum.PastilAIPlan => await _context.PastilAiPlans.AsNoTracking().AnyAsync(item => item.Id == id, cancellationToken),
                ClubRewardTargetTypeEnum.City => await _context.Cities.AsNoTracking().AnyAsync(item => item.Id == id, cancellationToken),
                _ => false
            };
        }

        private static void Apply(ClubRewardTemplate item, ClubRewardTemplateDto dto)
        {
            item.Name = dto.Name.Trim();
            item.Title = dto.Title.Trim();
            item.ShortDescription = dto.ShortDescription?.Trim();
            item.Description = dto.Description?.Trim();
            item.RewardType = dto.RewardType;
            item.ApplicationMethod = dto.ApplicationMethod;
            item.PointCost = dto.PointCost;
            item.StartDate = dto.StartDate;
            item.EndDate = dto.EndDate;
            item.ExpirationType = dto.ExpirationType;
            item.ExpirationValue = dto.ExpirationValue;
            item.FixedExpirationDate = dto.FixedExpirationDate;
            item.BenefitValue = dto.BenefitValue;
            item.MaximumBenefitValue = dto.MaximumBenefitValue;
            item.FundingType = ClubRewardFundingTypeEnum.Pastil;
            item.IsAutomationAllowed = dto.IsAutomationAllowed;
            item.IsManualAllowed = dto.IsManualAllowed;
            item.Active = dto.Active;
            item.NotificationLevel = dto.NotificationLevel;
            item.PictureId = dto.PictureId;
            item.Terms = dto.Terms?.Trim();
        }

        private static void ReplaceChildren(ClubRewardTemplate item, ClubRewardTemplateDto dto)
        {
            item.Targets = (dto.Targets ?? []).Select(target => new ClubRewardTarget
            {
                TargetType = target.TargetType,
                TargetId = target.TargetId is 0 ? null : target.TargetId,
                IncludeChildren = target.IncludeChildren
            }).ToList();
            item.PetTypes = (dto.PetTypeIds ?? []).Distinct().Select(id => new ClubRewardPetType
            {
                PetTypeId = id
            }).ToList();
            if (dto.PastilAITarget != null)
            {
                item.PastilAITarget = new ClubRewardPastilAITarget
                {
                    PlanId = dto.PastilAITarget.PlanId,
                    TargetPlanId = dto.PastilAITarget.TargetPlanId,
                    FreeDays = dto.PastilAITarget.FreeDays,
                    IsUpgrade = dto.PastilAITarget.IsUpgrade
                };
            }
        }

        private static bool IsPercentageReward(ClubRewardTypeEnum rewardType) =>
            rewardType is ClubRewardTypeEnum.PercentageDiscount or
                ClubRewardTypeEnum.PastilAIPlanPercentageDiscount;

        private static bool IsPastilAIReward(ClubRewardTypeEnum rewardType) =>
            rewardType is ClubRewardTypeEnum.PastilAIPlanFixedDiscount or
                ClubRewardTypeEnum.PastilAIPlanPercentageDiscount or
                ClubRewardTypeEnum.PastilAIFreeDays or
                ClubRewardTypeEnum.PastilAIFreeMonth or
                ClubRewardTypeEnum.PastilAIUpgrade;

        private static IQueryable<ClubRewardTemplateDto> Project(IQueryable<ClubRewardTemplate> query) =>
            query.Select(item => new ClubRewardTemplateDto
            {
                Id = item.Id,
                Name = item.Name,
                Title = item.Title,
                ShortDescription = item.ShortDescription,
                Description = item.Description,
                RewardType = item.RewardType,
                ApplicationMethod = item.ApplicationMethod,
                PointCost = item.PointCost,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                ExpirationType = item.ExpirationType,
                ExpirationValue = item.ExpirationValue,
                FixedExpirationDate = item.FixedExpirationDate,
                BenefitValue = item.BenefitValue,
                MaximumBenefitValue = item.MaximumBenefitValue,
                FundingType = item.FundingType,
                IsAutomationAllowed = item.IsAutomationAllowed,
                IsManualAllowed = item.IsManualAllowed,
                Active = item.Active,
                NotificationLevel = item.NotificationLevel,
                PictureId = item.PictureId,
                Terms = item.Terms,
                Targets = item.Targets.Select(target => new ClubRewardTargetDto
                {
                    Id = target.Id,
                    TargetType = target.TargetType,
                    TargetId = target.TargetId,
                    IncludeChildren = target.IncludeChildren
                }).ToList(),
                PetTypeIds = item.PetTypes.Select(pet => pet.PetTypeId).ToList(),
                PastilAITarget = item.PastilAITarget == null ? null : new ClubRewardPastilAITargetDto
                {
                    PlanId = item.PastilAITarget.PlanId,
                    TargetPlanId = item.PastilAITarget.TargetPlanId,
                    FreeDays = item.PastilAITarget.FreeDays,
                    IsUpgrade = item.PastilAITarget.IsUpgrade
                }
            });
    }
}
