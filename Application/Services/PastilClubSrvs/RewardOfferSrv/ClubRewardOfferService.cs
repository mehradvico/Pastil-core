using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Dto;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Iface;
using Application.Services.PastilClubSrvs.RewardTemplateSrv.Dto;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.RewardOfferSrv
{
    public class ClubRewardOfferService : IClubRewardOfferService
    {
        private readonly IDataBaseContext _context;
        private readonly IClubRewardEligibilityService _eligibilityService;

        public ClubRewardOfferService(
            IDataBaseContext context,
            IClubRewardEligibilityService eligibilityService)
        {
            _context = context;
            _eligibilityService = eligibilityService;
        }

        public async Task<BaseResultDto<ClubRewardOfferVDto>> FindAdminAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            var item = await Project(_context.ClubRewardOffers.AsNoTracking(), DateTimeOffset.UtcNow)
                .FirstOrDefaultAsync(offer => offer.Id == id, cancellationToken);
            return Result(item);
        }

        public async Task<ClubRewardOfferSearchDto> SearchAdminAsync(
            ClubRewardOfferInputDto dto,
            CancellationToken cancellationToken = default)
        {
            dto.PageIndex = Math.Max(1, dto.PageIndex);
            dto.PageSize = Math.Clamp(dto.PageSize, 1, 100);
            var now = DateTimeOffset.UtcNow;
            var query = Filter(_context.ClubRewardOffers.AsNoTracking(), dto, now);
            return await SearchAsync(query, dto, now, cancellationToken);
        }

        public async Task<BaseResultDto<ClubRewardOfferVDto>> CreateManualAsync(
            ClubRewardOfferCreateDto dto,
            long adminId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var template = await _context.ClubRewardTemplates.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == dto.RewardTemplateId, cancellationToken);
            if (template == null || !template.Active || !template.IsManualAllowed ||
                template.StartDate.HasValue && template.StartDate > now ||
                template.EndDate.HasValue && template.EndDate < now)
            {
                return new BaseResultDto<ClubRewardOfferVDto>(false, "CLUB_REWARD_TEMPLATE_NOT_AVAILABLE", null);
            }

            if (!await _context.Users.AsNoTracking().AnyAsync(
                    item => item.Id == dto.UserId && !item.Deleted && !item.Locked,
                    cancellationToken))
                return new BaseResultDto<ClubRewardOfferVDto>(false, Resource.Notification.NothingFound, null);

            if (await _context.ClubRewardOffers.AsNoTracking().AnyAsync(item =>
                    item.UserId == dto.UserId && item.RewardTemplateId == dto.RewardTemplateId,
                    cancellationToken))
                return new BaseResultDto<ClubRewardOfferVDto>(false, "CLUB_REWARD_TEMPLATE_ALREADY_OFFERED", null);

            DateTimeOffset expiresAt;
            try
            {
                expiresAt = ClubRewardExpirationResolver.Resolve(template, now, dto.CustomExpiresAt);
            }
            catch (InvalidOperationException exception)
            {
                return new BaseResultDto<ClubRewardOfferVDto>(false, exception.Message, null);
            }

            if (expiresAt <= now)
                return new BaseResultDto<ClubRewardOfferVDto>(false, "CLUB_REWARD_EXPIRATION_INVALID", null);

            var offer = new ClubRewardOffer
            {
                UserId = dto.UserId,
                RewardTemplateId = dto.RewardTemplateId,
                SourceType = ClubRewardOfferSourceEnum.ManualAdmin,
                Status = dto.ApproveImmediately
                    ? ClubRewardOfferStatusEnum.Approved
                    : ClubRewardOfferStatusEnum.PendingApproval,
                PointCostSnapshot = template.PointCost,
                GeneratedDate = now,
                ApprovedDate = dto.ApproveImmediately ? now : null,
                ApprovedByAdminId = dto.ApproveImmediately ? adminId : null,
                ExpiresAt = expiresAt,
                CreateDate = DateTime.UtcNow
            };

            await _context.ClubRewardOffers.AddAsync(offer, cancellationToken);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return new BaseResultDto<ClubRewardOfferVDto>(false, "CLUB_REWARD_TEMPLATE_ALREADY_OFFERED", null);
            }

            return await FindAdminAsync(offer.Id, cancellationToken);
        }

        public Task<BaseResultDto<ClubRewardOfferVDto>> ApproveAsync(
            long offerId,
            long adminId,
            CancellationToken cancellationToken = default) =>
            DecideAsync(offerId, adminId, true, null, cancellationToken);

        public Task<BaseResultDto<ClubRewardOfferVDto>> RejectAsync(
            long offerId,
            string reason,
            long adminId,
            CancellationToken cancellationToken = default) =>
            DecideAsync(offerId, adminId, false, reason, cancellationToken);

        public async Task<BaseResultDto> BulkApproveAsync(
            ClubRewardOfferBulkDecisionDto dto,
            long adminId,
            CancellationToken cancellationToken = default)
        {
            foreach (var id in dto.RewardOfferIds.Distinct())
            {
                var result = await ApproveAsync(id, adminId, cancellationToken);
                if (!result.IsSuccess)
                    return new BaseResultDto(false, $"CLUB_REWARD_BULK_FAILED:{id}");
            }

            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> BulkRejectAsync(
            ClubRewardOfferBulkDecisionDto dto,
            long adminId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return new BaseResultDto(false, "CLUB_REWARD_REJECT_REASON_REQUIRED");

            foreach (var id in dto.RewardOfferIds.Distinct())
            {
                var result = await RejectAsync(id, dto.Reason, adminId, cancellationToken);
                if (!result.IsSuccess)
                    return new BaseResultDto(false, $"CLUB_REWARD_BULK_FAILED:{id}");
            }

            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto<ClubRewardOfferVDto>> FindUserAsync(
            long id,
            long userId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var query = UserVisibleQuery(userId, now).Where(item => item.Id == id);
            var item = await Project(query, now).FirstOrDefaultAsync(cancellationToken);
            if (item == null || !await _eligibilityService.IsPetEligibleAsync(userId, item.RewardTemplateId, cancellationToken))
                return new BaseResultDto<ClubRewardOfferVDto>(false, "CLUB_REWARD_NOT_FOUND", null);
            return new BaseResultDto<ClubRewardOfferVDto>(true, item);
        }

        public async Task<ClubRewardOfferSearchDto> SearchUserAsync(
            ClubRewardOfferInputDto dto,
            long userId,
            CancellationToken cancellationToken = default)
        {
            dto.PageIndex = Math.Max(1, dto.PageIndex);
            dto.PageSize = Math.Clamp(dto.PageSize, 1, 100);
            dto.UserId = userId;
            dto.Status = ClubRewardOfferStatusEnum.Approved;
            var now = DateTimeOffset.UtcNow;
            var userPetTypeIds = _context.UserPets.AsNoTracking()
                .Where(item => item.UserId == userId && item.Active && !item.Deleted)
                .Select(item => item.PetId);
            var query = UserVisibleQuery(userId, now).Where(item =>
                !item.RewardTemplate.PetTypes.Any() ||
                item.RewardTemplate.PetTypes.Any(pet => userPetTypeIds.Contains(pet.PetTypeId)));

            if (dto.RewardType.HasValue)
                query = query.Where(item => item.RewardTemplate.RewardType == dto.RewardType.Value);
            if (dto.PetTypeId.HasValue)
                query = query.Where(item => item.RewardTemplate.PetTypes.Any(pet => pet.PetTypeId == dto.PetTypeId.Value));
            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var text = dto.Q.Trim();
                query = query.Where(item =>
                    item.RewardTemplate.Title.Contains(text) ||
                    item.RewardTemplate.ShortDescription.Contains(text));
            }

            return await SearchAsync(query, dto, now, cancellationToken);
        }

        private async Task<BaseResultDto<ClubRewardOfferVDto>> DecideAsync(
            long offerId,
            long adminId,
            bool approve,
            string reason,
            CancellationToken cancellationToken)
        {
            if (!approve && string.IsNullOrWhiteSpace(reason))
                return new BaseResultDto<ClubRewardOfferVDto>(false, "CLUB_REWARD_REJECT_REASON_REQUIRED", null);

            var offer = await _context.ClubRewardOffers.AsTracking()
                .FirstOrDefaultAsync(item => item.Id == offerId, cancellationToken);
            if (offer == null)
                return new BaseResultDto<ClubRewardOfferVDto>(false, Resource.Notification.NothingFound, null);
            if (offer.Status != ClubRewardOfferStatusEnum.PendingApproval)
                return new BaseResultDto<ClubRewardOfferVDto>(false, "CLUB_REWARD_OFFER_NOT_PENDING", null);
            if (offer.ExpiresAt <= DateTimeOffset.UtcNow)
                return new BaseResultDto<ClubRewardOfferVDto>(false, "CLUB_REWARD_EXPIRED", null);

            var now = DateTimeOffset.UtcNow;
            if (approve)
            {
                offer.Status = ClubRewardOfferStatusEnum.Approved;
                offer.ApprovedDate = now;
                offer.ApprovedByAdminId = adminId;
            }
            else
            {
                offer.Status = ClubRewardOfferStatusEnum.Rejected;
                offer.RejectedDate = now;
                offer.RejectedByAdminId = adminId;
                offer.RejectReason = reason.Trim();
            }
            offer.UpdateDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new BaseResultDto<ClubRewardOfferVDto>(false, "CLUB_REWARD_CONCURRENCY_CONFLICT", null);
            }

            return await FindAdminAsync(offer.Id, cancellationToken);
        }

        private IQueryable<ClubRewardOffer> UserVisibleQuery(long userId, DateTimeOffset now) =>
            _context.ClubRewardOffers.AsNoTracking().Where(item =>
                item.UserId == userId &&
                item.Status == ClubRewardOfferStatusEnum.Approved &&
                item.ExpiresAt > now &&
                item.RewardTemplate.Active &&
                (!item.RewardTemplate.StartDate.HasValue || item.RewardTemplate.StartDate <= now) &&
                (!item.RewardTemplate.EndDate.HasValue || item.RewardTemplate.EndDate >= now));

        private static IQueryable<ClubRewardOffer> Filter(
            IQueryable<ClubRewardOffer> query,
            ClubRewardOfferInputDto dto,
            DateTimeOffset now)
        {
            if (dto.UserId.HasValue)
                query = query.Where(item => item.UserId == dto.UserId.Value);
            if (dto.RewardTemplateId.HasValue)
                query = query.Where(item => item.RewardTemplateId == dto.RewardTemplateId.Value);
            if (dto.Status == ClubRewardOfferStatusEnum.Expired)
                query = query.Where(item =>
                    item.ExpiresAt <= now &&
                    (item.Status == ClubRewardOfferStatusEnum.PendingApproval ||
                     item.Status == ClubRewardOfferStatusEnum.Approved));
            else if (dto.Status.HasValue)
                query = query.Where(item => item.Status == dto.Status.Value);
            if (dto.SourceType.HasValue)
                query = query.Where(item => item.SourceType == dto.SourceType.Value);
            if (dto.RewardType.HasValue)
                query = query.Where(item => item.RewardTemplate.RewardType == dto.RewardType.Value);
            if (dto.PetTypeId.HasValue)
                query = query.Where(item => item.RewardTemplate.PetTypes.Any(pet => pet.PetTypeId == dto.PetTypeId.Value));
            if (dto.FromDate.HasValue)
                query = query.Where(item => item.GeneratedDate >= dto.FromDate.Value);
            if (dto.ToDate.HasValue)
                query = query.Where(item => item.GeneratedDate <= dto.ToDate.Value);
            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var text = dto.Q.Trim();
                query = query.Where(item =>
                    item.User.Mobile.Contains(text) ||
                    item.User.FirstName.Contains(text) ||
                    item.User.LastName.Contains(text) ||
                    item.RewardTemplate.Title.Contains(text));
            }
            return query;
        }

        private static async Task<ClubRewardOfferSearchDto> SearchAsync(
            IQueryable<ClubRewardOffer> query,
            ClubRewardOfferInputDto dto,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            query = dto.SortBy == SortEnum.Old
                ? query.OrderBy(item => item.Id)
                : query.OrderByDescending(item => item.Id);
            return new ClubRewardOfferSearchDto(dto)
            {
                TotalCount = await query.CountAsync(cancellationToken),
                List = await Project(query, now)
                    .Skip((dto.PageIndex - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .ToListAsync(cancellationToken)
            };
        }

        private static IQueryable<ClubRewardOfferVDto> Project(
            IQueryable<ClubRewardOffer> query,
            DateTimeOffset now) =>
            query.Select(item => new ClubRewardOfferVDto
            {
                Id = item.Id,
                UserId = item.UserId,
                UserFullName = (item.User.FirstName + " " + item.User.LastName).Trim(),
                UserMobile = item.User.Mobile,
                RewardTemplateId = item.RewardTemplateId,
                TemplateName = item.RewardTemplate.Name,
                Title = item.RewardTemplate.Title,
                ShortDescription = item.RewardTemplate.ShortDescription,
                Description = item.RewardTemplate.Description,
                Terms = item.RewardTemplate.Terms,
                RewardType = item.RewardTemplate.RewardType,
                SourceType = item.SourceType,
                Status = item.ExpiresAt <= now &&
                    (item.Status == ClubRewardOfferStatusEnum.PendingApproval ||
                     item.Status == ClubRewardOfferStatusEnum.Approved)
                    ? ClubRewardOfferStatusEnum.Expired
                    : item.Status,
                PointCost = item.PointCostSnapshot,
                BenefitValue = item.RewardTemplate.BenefitValue,
                MaximumBenefitValue = item.RewardTemplate.MaximumBenefitValue,
                GeneratedDate = item.GeneratedDate,
                ApprovedDate = item.ApprovedDate,
                RejectedDate = item.RejectedDate,
                ExpiresAt = item.ExpiresAt,
                RedeemedDate = item.RedeemedDate,
                RejectReason = item.RejectReason,
                Picture = item.RewardTemplate.Picture == null ? null : new PictureVDto
                {
                    Id = item.RewardTemplate.Picture.Id,
                    Url = item.RewardTemplate.Picture.Url + "/" + item.RewardTemplate.Picture.Name,
                    BaseUrl = item.RewardTemplate.Picture.Url,
                    GuidName = item.RewardTemplate.Picture.GuidName,
                    Extension = item.RewardTemplate.Picture.Extension,
                    OrginalName = item.RewardTemplate.Picture.OrginalName
                },
                Targets = item.RewardTemplate.Targets.Select(target => new ClubRewardTargetDto
                {
                    Id = target.Id,
                    TargetType = target.TargetType,
                    TargetId = target.TargetId,
                    IncludeChildren = target.IncludeChildren
                }).ToList(),
                PetTypeIds = item.RewardTemplate.PetTypes.Select(pet => pet.PetTypeId).ToList(),
                CanRedeem = item.Status == ClubRewardOfferStatusEnum.Approved &&
                    item.ExpiresAt > now &&
                    item.User.ClubPointAccount != null &&
                    item.User.ClubPointAccount.DebtPoint == 0 &&
                    item.User.ClubPointAccount.AvailablePoint >= item.PointCostSnapshot
            });

        private static BaseResultDto<ClubRewardOfferVDto> Result(ClubRewardOfferVDto item) =>
            item == null
                ? new BaseResultDto<ClubRewardOfferVDto>(false, Resource.Notification.NothingFound, null)
                : new BaseResultDto<ClubRewardOfferVDto>(true, item);
    }
}
