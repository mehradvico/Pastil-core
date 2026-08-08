using System.Collections.Generic;

namespace Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv.Dto
{
    public class PastilMatchSuggestionInputDto
    {
        public PastilMatchSuggestionInputDto()
        {
            SamePetTypeOnly = true;
            ExcludedProfileIds = new List<long>();
            RequiredGoalIds = new List<long>();
            PetBreedIds = new List<long>();
            EnergyLevelIds = new List<long>();
            SocialLevelIds = new List<long>();
        }

        public long SourceProfileId { get; set; }
        public List<long> ExcludedProfileIds { get; set; }
        public List<long> RequiredGoalIds { get; set; }
        public List<long> PetBreedIds { get; set; }
        public List<long> EnergyLevelIds { get; set; }
        public List<long> SocialLevelIds { get; set; }
        public double? MaxDistanceInKilometers { get; set; }
        public int? MinAgeInMonths { get; set; }
        public int? MaxAgeInMonths { get; set; }
        public long? CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public bool? IsMale { get; set; }
        public bool? IsSterile { get; set; }
        public bool VerifiedOnly { get; set; }
        public bool SamePetTypeOnly { get; set; }
        public int? MinimumCompatibilityPercent { get; set; }
    }
}
