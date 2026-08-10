using Application.Services.PastilClubSrvs.RewardOfferSrv;
using Entities.Entities.PastilClubField;
using System;
using Xunit;

namespace Application.Tests.PastilClub
{
    public class ClubRewardExpirationResolverTests
    {
        [Fact]
        public void Resolve_SevenDays_DoesNotChangeAfterGeneration()
        {
            var generated = new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);
            var template = new ClubRewardTemplate
            {
                ExpirationType = ClubRewardExpirationTypeEnum.SevenDays
            };

            Assert.Equal(generated.AddDays(7), ClubRewardExpirationResolver.Resolve(template, generated));
        }

        [Fact]
        public void Resolve_FixedDate_ReturnsConfiguredDate()
        {
            var generated = new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);
            var fixedDate = generated.AddDays(20);
            var template = new ClubRewardTemplate
            {
                ExpirationType = ClubRewardExpirationTypeEnum.FixedDate,
                FixedExpirationDate = fixedDate
            };

            Assert.Equal(fixedDate, ClubRewardExpirationResolver.Resolve(template, generated));
        }

        [Fact]
        public void Resolve_CustomExpiration_HasPriority()
        {
            var generated = new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);
            var custom = generated.AddDays(3);
            var template = new ClubRewardTemplate
            {
                ExpirationType = ClubRewardExpirationTypeEnum.ThirtyDays
            };

            Assert.Equal(custom, ClubRewardExpirationResolver.Resolve(template, generated, custom));
        }
    }
}
