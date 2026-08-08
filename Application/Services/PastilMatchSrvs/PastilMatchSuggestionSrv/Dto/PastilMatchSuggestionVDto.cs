using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv.Dto
{
    public class PastilMatchSuggestionVDto
    {
        public bool Found { get; set; }
        public string Message { get; set; }
        public long SourceProfileId { get; set; }
        public long? CandidateProfileId { get; set; }
        public int? CompatibilityPercent { get; set; }
        public long? RecommendedGoalId { get; set; }
        public double? DistanceInKilometers { get; set; }
        public int? AgeDifferenceInMonths { get; set; }
        public PastilMatchSuggestionScoreVDto Score { get; set; }
        public PastilMatchProfileVDto Profile { get; set; }
        public List<long> ExcludedProfileIds { get; set; } = new List<long>();
    }
}
