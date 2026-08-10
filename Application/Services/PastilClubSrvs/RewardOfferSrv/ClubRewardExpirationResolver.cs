using Entities.Entities.PastilClubField;
using System;

namespace Application.Services.PastilClubSrvs.RewardOfferSrv
{
    public static class ClubRewardExpirationResolver
    {
        private static readonly TimeZoneInfo TehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

        public static DateTimeOffset Resolve(
            ClubRewardTemplate template,
            DateTimeOffset generatedDate,
            DateTimeOffset? customExpiresAt = null)
        {
            if (customExpiresAt.HasValue)
                return customExpiresAt.Value;

            return template.ExpirationType switch
            {
                ClubRewardExpirationTypeEnum.EndOfDay => EndOfTehranDay(generatedDate),
                ClubRewardExpirationTypeEnum.SevenDays => generatedDate.AddDays(7),
                ClubRewardExpirationTypeEnum.TenDays => generatedDate.AddDays(10),
                ClubRewardExpirationTypeEnum.ThirtyDays => generatedDate.AddDays(30),
                ClubRewardExpirationTypeEnum.FixedDate when template.FixedExpirationDate.HasValue =>
                    template.FixedExpirationDate.Value,
                _ when template.ExpirationValue.HasValue => generatedDate.AddDays(template.ExpirationValue.Value),
                _ => throw new InvalidOperationException("CLUB_REWARD_EXPIRATION_INVALID")
            };
        }

        private static DateTimeOffset EndOfTehranDay(DateTimeOffset value)
        {
            var localDate = TimeZoneInfo.ConvertTime(value, TehranTimeZone).Date;
            var end = DateTime.SpecifyKind(localDate.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);
            return new DateTimeOffset(end, TehranTimeZone.GetUtcOffset(end));
        }
    }
}
