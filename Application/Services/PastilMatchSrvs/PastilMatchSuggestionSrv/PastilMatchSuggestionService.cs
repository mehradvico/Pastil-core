using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv
{
    public class PastilMatchSuggestionService : IPastilMatchSuggestionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserHelper _currentUser;

        public PastilMatchSuggestionService(
            IDataBaseContext context,
            IMapper mapper,
            ICurrentUserHelper currentUser)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<BaseResultDto<PastilMatchSuggestionVDto>> FindNextAsync(
            PastilMatchSuggestionInputDto dto)
        {
            try
            {
                var validationResult = ValidateInput(dto);
                if (!validationResult.IsSuccess)
                {
                    return new BaseResultDto<PastilMatchSuggestionVDto>(
                        false,
                        validationResult.Messages,
                        null
                    );
                }

                var userId = _currentUser.CurrentUser.UserId;
                var sourceProfile = await GetProfileQuery()
                    .FirstOrDefaultAsync(profile =>
                        profile.Id == dto.SourceProfileId &&
                        profile.IsActive &&
                        !profile.Deleted &&
                        profile.UserPet.Active &&
                        !profile.UserPet.Deleted
                    );

                if (sourceProfile == null)
                {
                    return new BaseResultDto<PastilMatchSuggestionVDto>(
                        false,
                        Resource.Notification.NothingFound,
                        null
                    );
                }

                if (sourceProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto<PastilMatchSuggestionVDto>(
                        false,
                        Resource.Notification.AccessDenied,
                        null
                    );
                }

                var sourceGoalIds = sourceProfile.PastilMatchProfileGoals
                    .Where(goal => !goal.Deleted)
                    .Select(goal => goal.PastilMatchGoalId)
                    .ToHashSet();

                if (!sourceGoalIds.Any())
                {
                    return new BaseResultDto<PastilMatchSuggestionVDto>(
                        false,
                        Resource.Notification.PleaseSelectPastilMatchGoal,
                        null
                    );
                }

                var requiredGoalIds = (dto.RequiredGoalIds ?? new List<long>())
                    .Where(id => id > 0)
                    .Distinct()
                    .ToHashSet();

                if (requiredGoalIds.Any(id => !sourceGoalIds.Contains(id)))
                {
                    return new BaseResultDto<PastilMatchSuggestionVDto>(
                        false,
                        Resource.Notification.PastilMatchGoalNotSelected,
                        null
                    );
                }

                var excludedProfileIds = (dto.ExcludedProfileIds ?? new List<long>())
                    .Where(id => id > 0)
                    .Distinct()
                    .ToHashSet();

                excludedProfileIds.Add(sourceProfile.Id);

                var ownProfileIds = await _context.PastilMatchProfiles
                    .Where(profile => profile.UserPet.UserId == userId)
                    .Select(profile => profile.Id)
                    .ToListAsync();

                excludedProfileIds.UnionWith(ownProfileIds);

                var blockedUserIds = await _context.PastilMatchBlocks
                    .Where(block =>
                        !block.Deleted &&
                        (block.BlockerUserId == userId || block.BlockedUserId == userId)
                    )
                    .Select(block =>
                        block.BlockerUserId == userId
                            ? block.BlockedUserId
                            : block.BlockerUserId
                    )
                    .Distinct()
                    .ToListAsync();

                var pendingStatusId =
                    (long)PastilMatchRequestStatusEnum.PastilMatchRequestStatus_Pending;

                var requestProfileIds = await _context.PastilMatchRequests
                    .Where(request =>
                        request.StatusId == pendingStatusId &&
                        (request.SenderProfileId == sourceProfile.Id ||
                         request.ReceiverProfileId == sourceProfile.Id)
                    )
                    .Select(request =>
                        request.SenderProfileId == sourceProfile.Id
                            ? request.ReceiverProfileId
                            : request.SenderProfileId
                    )
                    .Distinct()
                    .ToListAsync();

                excludedProfileIds.UnionWith(requestProfileIds);

                var matchedProfileIds = await _context.PastilMatches
                    .Where(match =>
                        match.FirstProfileId == sourceProfile.Id ||
                        match.SecondProfileId == sourceProfile.Id
                    )
                    .Select(match =>
                        match.FirstProfileId == sourceProfile.Id
                            ? match.SecondProfileId
                            : match.FirstProfileId
                    )
                    .Distinct()
                    .ToListAsync();

                excludedProfileIds.UnionWith(matchedProfileIds);

                var candidateQuery = GetProfileQuery()
                    .Where(profile =>
                        profile.IsActive &&
                        !profile.Deleted &&
                        profile.UserPet.Active &&
                        !profile.UserPet.Deleted &&
                        profile.UserPet.UserId != userId &&
                        !excludedProfileIds.Contains(profile.Id) &&
                        !blockedUserIds.Contains(profile.UserPet.UserId) &&
                        profile.PastilMatchProfileGoals.Any(goal =>
                            !goal.Deleted &&
                            sourceGoalIds.Contains(goal.PastilMatchGoalId)
                        )
                    );

                if (dto.SamePetTypeOnly)
                {
                    candidateQuery = candidateQuery.Where(profile =>
                        profile.UserPet.PetId == sourceProfile.UserPet.PetId
                    );
                }

                if (dto.VerifiedOnly)
                {
                    candidateQuery = candidateQuery.Where(profile =>
                        profile.IsVerified == true
                    );
                }

                if (dto.CityId.HasValue)
                {
                    candidateQuery = candidateQuery.Where(profile =>
                        profile.CityId == dto.CityId.Value
                    );
                }

                if (dto.NeighborhoodId.HasValue)
                {
                    candidateQuery = candidateQuery.Where(profile =>
                        profile.NeighborhoodId == dto.NeighborhoodId.Value
                    );
                }

                if (dto.IsMale.HasValue)
                {
                    candidateQuery = candidateQuery.Where(profile =>
                        profile.UserPet.IsMale == dto.IsMale.Value
                    );
                }

                if (dto.IsSterile.HasValue)
                {
                    candidateQuery = candidateQuery.Where(profile =>
                        profile.UserPet.IsSterile == dto.IsSterile.Value
                    );
                }

                var breedIds = (dto.PetBreedIds ?? new List<long>())
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                if (breedIds.Any())
                {
                    candidateQuery = candidateQuery.Where(profile =>
                        (profile.UserPet.PetBreedId.HasValue &&
                         breedIds.Contains(profile.UserPet.PetBreedId.Value)) ||
                        (profile.UserPet.PetBreed2Id.HasValue &&
                         breedIds.Contains(profile.UserPet.PetBreed2Id.Value))
                    );
                }

                var energyLevelIds = (dto.EnergyLevelIds ?? new List<long>())
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                if (energyLevelIds.Any())
                {
                    candidateQuery = candidateQuery.Where(profile =>
                        energyLevelIds.Contains(profile.EnergyLevelId)
                    );
                }

                var socialLevelIds = (dto.SocialLevelIds ?? new List<long>())
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                if (socialLevelIds.Any())
                {
                    candidateQuery = candidateQuery.Where(profile =>
                        socialLevelIds.Contains(profile.SocialLevelId)
                    );
                }

                if (requiredGoalIds.Any())
                {
                    candidateQuery = candidateQuery.Where(profile =>
                        profile.PastilMatchProfileGoals.Any(goal =>
                            !goal.Deleted &&
                            requiredGoalIds.Contains(goal.PastilMatchGoalId)
                        )
                    );
                }

                var candidates = await candidateQuery.ToListAsync();
                var sourceLongitude = sourceProfile.LiveLocation?.X;
                var sourceLatitude = sourceProfile.LiveLocation?.Y;
                var minimumPercent = dto.MinimumCompatibilityPercent ?? 0;
                var scoredCandidates = new List<ScoredCandidate>();

                foreach (var candidate in candidates)
                {
                    var candidateAgeInMonths =
                        PastilMatchCompatibilityCalculator.CalculateAgeInMonths(
                            candidate.UserPet.Birthday
                        );

                    if (dto.MinAgeInMonths.HasValue &&
                        candidateAgeInMonths < dto.MinAgeInMonths.Value)
                    {
                        continue;
                    }

                    if (dto.MaxAgeInMonths.HasValue &&
                        candidateAgeInMonths > dto.MaxAgeInMonths.Value)
                    {
                        continue;
                    }

                    var score = PastilMatchCompatibilityCalculator.Calculate(
                        sourceProfile.UserPet.Birthday,
                        candidate.UserPet.Birthday,
                        sourceProfile.UserPet.PetId,
                        candidate.UserPet.PetId,
                        GetBreedIds(sourceProfile),
                        GetBreedIds(candidate),
                        sourceGoalIds,
                        candidate.PastilMatchProfileGoals
                            .Where(goal => !goal.Deleted)
                            .Select(goal => goal.PastilMatchGoalId),
                        sourceProfile.EnergyLevelId,
                        candidate.EnergyLevelId,
                        sourceProfile.SocialLevelId,
                        candidate.SocialLevelId,
                        sourceLongitude,
                        sourceLatitude,
                        candidate.LiveLocation?.X,
                        candidate.LiveLocation?.Y
                    );

                    if (dto.MaxDistanceInKilometers.HasValue &&
                        (!score.DistanceInKilometers.HasValue ||
                         score.DistanceInKilometers.Value >
                         dto.MaxDistanceInKilometers.Value))
                    {
                        continue;
                    }

                    if (score.TotalPercent < minimumPercent)
                    {
                        continue;
                    }

                    var candidateGoalIds = candidate.PastilMatchProfileGoals
                        .Where(goal => !goal.Deleted)
                        .Select(goal => goal.PastilMatchGoalId)
                        .ToHashSet();

                    var recommendedGoalId = requiredGoalIds
                        .FirstOrDefault(candidateGoalIds.Contains);

                    if (recommendedGoalId == 0)
                    {
                        recommendedGoalId = sourceGoalIds
                            .FirstOrDefault(candidateGoalIds.Contains);
                    }

                    if (recommendedGoalId == 0)
                    {
                        recommendedGoalId = sourceGoalIds.First();
                    }

                    scoredCandidates.Add(new ScoredCandidate
                    {
                        Profile = candidate,
                        Score = score,
                        RecommendedGoalId = recommendedGoalId
                    });
                }

                var bestCandidate = scoredCandidates
                    .OrderByDescending(item => item.Score.TotalPercent)
                    .ThenBy(item =>
                        item.Score.DistanceInKilometers ?? double.MaxValue)
                    .ThenByDescending(item => item.Profile.LastActiveDate)
                    .ThenBy(item => item.Profile.Id)
                    .FirstOrDefault();

                if (bestCandidate == null)
                {
                    return new BaseResultDto<PastilMatchSuggestionVDto>(
                        true,
                        new PastilMatchSuggestionVDto
                        {
                            Found = false,
                            Message =
                                Resource.Notification.PastilMatchSuggestionNotFound,
                            SourceProfileId = sourceProfile.Id,
                            ExcludedProfileIds =
                                excludedProfileIds.OrderBy(id => id).ToList()
                        }
                    );
                }

                excludedProfileIds.Add(bestCandidate.Profile.Id);

                return new BaseResultDto<PastilMatchSuggestionVDto>(
                    true,
                    new PastilMatchSuggestionVDto
                    {
                        Found = true,
                        Message = null,
                        SourceProfileId = sourceProfile.Id,
                        CandidateProfileId = bestCandidate.Profile.Id,
                        CompatibilityPercent =
                            bestCandidate.Score.TotalPercent,
                        RecommendedGoalId =
                            bestCandidate.RecommendedGoalId,
                        DistanceInKilometers =
                            bestCandidate.Score.DistanceInKilometers,
                        AgeDifferenceInMonths =
                            bestCandidate.Score.AgeDifferenceInMonths,
                        Score = new PastilMatchSuggestionScoreVDto
                        {
                            GoalsPercent =
                                bestCandidate.Score.GoalsPercent,
                            DistancePercent =
                                bestCandidate.Score.DistancePercent,
                            AgePercent = bestCandidate.Score.AgePercent,
                            BreedPercent =
                                bestCandidate.Score.BreedPercent,
                            EnergyPercent =
                                bestCandidate.Score.EnergyPercent,
                            SocialPercent =
                                bestCandidate.Score.SocialPercent
                        },
                        Profile =
                            _mapper.Map<PastilMatchProfileVDto>(
                                bestCandidate.Profile
                            ),
                        ExcludedProfileIds =
                            excludedProfileIds.OrderBy(id => id).ToList()
                    }
                );
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchSuggestionVDto>(
                    false,
                    ex.Message,
                    null
                );
            }
        }

        private static BaseResultDto ValidateInput(
            PastilMatchSuggestionInputDto dto)
        {
            if (dto == null || dto.SourceProfileId <= 0)
            {
                return new BaseResultDto(
                    false,
                    Resource.Notification.NothingFound
                );
            }

            if (dto.MaxDistanceInKilometers.HasValue &&
                dto.MaxDistanceInKilometers.Value <= 0)
            {
                return new BaseResultDto(
                    false,
                    Resource.Notification.TheRangeEnteredIsNotCorrect
                );
            }

            if (dto.MinAgeInMonths < 0 || dto.MaxAgeInMonths < 0 ||
                (dto.MinAgeInMonths.HasValue &&
                 dto.MaxAgeInMonths.HasValue &&
                 dto.MinAgeInMonths.Value > dto.MaxAgeInMonths.Value))
            {
                return new BaseResultDto(
                    false,
                    Resource.Notification.TheRangeEnteredIsNotCorrect
                );
            }

            if (dto.MinimumCompatibilityPercent < 0 ||
                dto.MinimumCompatibilityPercent > 100)
            {
                return new BaseResultDto(
                    false,
                    Resource.Notification.TheRangeEnteredIsNotCorrect
                );
            }

            if ((dto.RequiredGoalIds ?? new List<long>()).Any(id =>
                    !Enum.IsDefined(typeof(PastilMatchGoalEnum), (int)id)) ||
                (dto.EnergyLevelIds ?? new List<long>()).Any(id =>
                    !Enum.IsDefined(typeof(EnergyLevelEnum), (int)id)) ||
                (dto.SocialLevelIds ?? new List<long>()).Any(id =>
                    !Enum.IsDefined(typeof(SocialLevelEnum), (int)id)))
            {
                return new BaseResultDto(
                    false,
                    Resource.Notification.TheRangeEnteredIsNotCorrect
                );
            }

            return new BaseResultDto(true);
        }

        private IQueryable<PastilMatchProfile> GetProfileQuery()
        {
            return _context.PastilMatchProfiles
                .AsNoTracking()
                .Include(profile => profile.UserPet)
                    .ThenInclude(userPet => userPet.User)
                .Include(profile => profile.UserPet)
                    .ThenInclude(userPet => userPet.Picture)
                .Include(profile => profile.UserPet)
                    .ThenInclude(userPet => userPet.Pet)
                .Include(profile => profile.UserPet)
                    .ThenInclude(userPet => userPet.PetBreed)
                .Include(profile => profile.UserPet)
                    .ThenInclude(userPet => userPet.PetBreed2)
                .Include(profile => profile.EnergyLevel)
                .Include(profile => profile.SocialLevel)
                .Include(profile => profile.City)
                .Include(profile => profile.Neighborhood)
                .Include(profile => profile.PastilMatchProfileGoals
                    .Where(goal => !goal.Deleted))
                    .ThenInclude(goal => goal.PastilMatchGoal);
        }

        private static IEnumerable<long> GetBreedIds(
            PastilMatchProfile profile)
        {
            if (profile.UserPet.PetBreedId.HasValue)
            {
                yield return profile.UserPet.PetBreedId.Value;
            }

            if (profile.UserPet.PetBreed2Id.HasValue)
            {
                yield return profile.UserPet.PetBreed2Id.Value;
            }
        }

        private sealed class ScoredCandidate
        {
            public PastilMatchProfile Profile { get; set; }
            public PastilMatchCompatibilityScore Score { get; set; }
            public long RecommendedGoalId { get; set; }
        }
    }
}
