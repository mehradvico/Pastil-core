using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv
{
    public class PastilMatchCompatibilityScore
    {
        public int TotalPercent { get; set; }
        public int GoalsPercent { get; set; }
        public int? DistancePercent { get; set; }
        public int? AgePercent { get; set; }
        public int? BreedPercent { get; set; }
        public int EnergyPercent { get; set; }
        public int SocialPercent { get; set; }
        public double? DistanceInKilometers { get; set; }
        public int? AgeDifferenceInMonths { get; set; }
    }

    public static class PastilMatchCompatibilityCalculator
    {
        private const double DefaultDistanceRangeInKilometers = 50D;

        public static PastilMatchCompatibilityScore Calculate(
            DateTime? sourceBirthday,
            DateTime? candidateBirthday,
            long sourcePetId,
            long candidatePetId,
            IEnumerable<long> sourceBreedIds,
            IEnumerable<long> candidateBreedIds,
            IEnumerable<long> sourceGoalIds,
            IEnumerable<long> candidateGoalIds,
            long sourceEnergyLevelId,
            long candidateEnergyLevelId,
            long sourceSocialLevelId,
            long candidateSocialLevelId,
            double? sourceLongitude,
            double? sourceLatitude,
            double? candidateLongitude,
            double? candidateLatitude)
        {
            var sourceGoals = ToSet(sourceGoalIds);
            var candidateGoals = ToSet(candidateGoalIds);
            var sourceBreeds = ToSet(sourceBreedIds);
            var candidateBreeds = ToSet(candidateBreedIds);

            var goalPercent = CalculateJaccardPercent(sourceGoals, candidateGoals) ?? 0;
            var breedPercent = CalculateBreedPercent(sourcePetId, candidatePetId, sourceBreeds, candidateBreeds);
            var ageDifference = CalculateAgeDifferenceInMonths(sourceBirthday, candidateBirthday);
            int? agePercent = ageDifference.HasValue
                ? ClampPercent(100D - (ageDifference.Value * 100D / 96D))
                : null;

            var energyPercent = CalculateLevelPercent(sourceEnergyLevelId, candidateEnergyLevelId);
            var socialPercent = CalculateLevelPercent(sourceSocialLevelId, candidateSocialLevelId);
            var distance = CalculateDistanceInKilometers(sourceLongitude, sourceLatitude, candidateLongitude, candidateLatitude);
            int? distancePercent = distance.HasValue
                ? ClampPercent(100D - (distance.Value * 100D / DefaultDistanceRangeInKilometers))
                : null;

            var weightedSum = goalPercent * 25D;
            var availableWeight = 25D;

            if (distancePercent.HasValue)
            {
                weightedSum += distancePercent.Value * 25D;
                availableWeight += 25D;
            }

            if (agePercent.HasValue)
            {
                weightedSum += agePercent.Value * 20D;
                availableWeight += 20D;
            }

            if (breedPercent.HasValue)
            {
                weightedSum += breedPercent.Value * 15D;
                availableWeight += 15D;
            }

            weightedSum += energyPercent * 7.5D;
            weightedSum += socialPercent * 7.5D;
            availableWeight += 15D;

            return new PastilMatchCompatibilityScore
            {
                TotalPercent = ClampPercent(weightedSum / availableWeight),
                GoalsPercent = goalPercent,
                DistancePercent = distancePercent,
                AgePercent = agePercent,
                BreedPercent = breedPercent,
                EnergyPercent = energyPercent,
                SocialPercent = socialPercent,
                DistanceInKilometers = distance.HasValue ? Math.Round(distance.Value, 2) : null,
                AgeDifferenceInMonths = ageDifference
            };
        }

        public static double? CalculateDistanceInKilometers(
            double? sourceLongitude,
            double? sourceLatitude,
            double? candidateLongitude,
            double? candidateLatitude)
        {
            if (!sourceLongitude.HasValue || !sourceLatitude.HasValue ||
                !candidateLongitude.HasValue || !candidateLatitude.HasValue)
            {
                return null;
            }

            const double earthRadiusInKilometers = 6371.0088D;
            var latitude1 = ToRadians(sourceLatitude.Value);
            var latitude2 = ToRadians(candidateLatitude.Value);
            var latitudeDifference = ToRadians(candidateLatitude.Value - sourceLatitude.Value);
            var longitudeDifference = ToRadians(candidateLongitude.Value - sourceLongitude.Value);

            var a = Math.Sin(latitudeDifference / 2D) * Math.Sin(latitudeDifference / 2D) +
                    Math.Cos(latitude1) * Math.Cos(latitude2) *
                    Math.Sin(longitudeDifference / 2D) * Math.Sin(longitudeDifference / 2D);
            var c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));

            return earthRadiusInKilometers * c;
        }

        public static int CalculateAgeInMonths(DateTime birthday, DateTime? now = null)
        {
            var currentDate = (now ?? DateTime.UtcNow).Date;
            var birthDate = birthday.Date;

            if (birthDate > currentDate)
            {
                return 0;
            }

            var months = (currentDate.Year - birthDate.Year) * 12 + currentDate.Month - birthDate.Month;
            if (currentDate.Day < birthDate.Day)
            {
                months--;
            }

            return Math.Max(0, months);
        }

        private static int? CalculateAgeDifferenceInMonths(DateTime? firstBirthday, DateTime? secondBirthday)
        {
            if (!firstBirthday.HasValue || !secondBirthday.HasValue ||
                firstBirthday.Value.Year < 1900 || secondBirthday.Value.Year < 1900)
            {
                return null;
            }

            var firstMonth = firstBirthday.Value.Year * 12 + firstBirthday.Value.Month;
            var secondMonth = secondBirthday.Value.Year * 12 + secondBirthday.Value.Month;
            return Math.Abs(firstMonth - secondMonth);
        }

        private static int CalculateLevelPercent(long firstLevelId, long secondLevelId)
        {
            return ClampPercent(100D - Math.Abs(firstLevelId - secondLevelId) * 25D);
        }

        private static int? CalculateBreedPercent(
            long sourcePetId,
            long candidatePetId,
            HashSet<long> sourceBreedIds,
            HashSet<long> candidateBreedIds)
        {
            if (sourceBreedIds.Count == 0 || candidateBreedIds.Count == 0)
            {
                return sourcePetId == candidatePetId ? 35 : 0;
            }

            var jaccard = CalculateJaccardPercent(sourceBreedIds, candidateBreedIds);
            if (jaccard.GetValueOrDefault() > 0)
            {
                return ClampPercent(70D + jaccard.Value * 0.3D);
            }

            return sourcePetId == candidatePetId ? 35 : 0;
        }

        private static int? CalculateJaccardPercent(HashSet<long> first, HashSet<long> second)
        {
            var unionCount = first.Union(second).Count();
            if (unionCount == 0)
            {
                return null;
            }

            var intersectionCount = first.Intersect(second).Count();
            return ClampPercent(intersectionCount * 100D / unionCount);
        }

        private static HashSet<long> ToSet(IEnumerable<long> values)
        {
            return values?.Where(value => value > 0).ToHashSet() ?? new HashSet<long>();
        }

        private static int ClampPercent(double value)
        {
            return (int)Math.Round(Math.Clamp(value, 0D, 100D));
        }

        private static double ToRadians(double degree)
        {
            return degree * Math.PI / 180D;
        }
    }
}
