using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Xunit;

namespace Application.Tests.PastilClub
{
    public class ClubPointModelTests
    {
        [Fact]
        public void PointTransaction_IdempotencyKey_HasUniqueIndex()
        {
            using var context = CreateContext();
            var entity = context.Model.FindEntityType(typeof(ClubPointTransaction));
            var index = entity!.GetIndexes().Single(item =>
                item.Properties.Count == 1 &&
                item.Properties[0].Name == nameof(ClubPointTransaction.IdempotencyKey));

            Assert.True(index.IsUnique);
        }

        [Fact]
        public void PointAccount_UserId_HasUniqueIndex()
        {
            using var context = CreateContext();
            var entity = context.Model.FindEntityType(typeof(ClubPointAccount));
            var index = entity!.GetIndexes().Single(item =>
                item.Properties.Count == 1 &&
                item.Properties[0].Name == nameof(ClubPointAccount.UserId));

            Assert.True(index.IsUnique);
        }

        [Fact]
        public void PointTransaction_RuleLimitLookup_HasCompositeIndex()
        {
            using var context = CreateContext();
            var entity = context.Model.FindEntityType(typeof(ClubPointTransaction));
            var index = entity!.GetIndexes().Single(item =>
                item.Properties.Select(property => property.Name).SequenceEqual(new[]
                {
                    nameof(ClubPointTransaction.UserId),
                    nameof(ClubPointTransaction.PointRuleId),
                    nameof(ClubPointTransaction.CreateDate)
                }));

            Assert.False(index.IsUnique);
        }

        [Fact]
        public void RewardOffer_UserAndTemplate_HasUniqueIndex()
        {
            using var context = CreateContext();
            var entity = context.Model.FindEntityType(typeof(ClubRewardOffer));
            var index = entity!.GetIndexes().Single(item =>
                item.Properties.Select(property => property.Name).SequenceEqual(new[]
                {
                    nameof(ClubRewardOffer.UserId),
                    nameof(ClubRewardOffer.RewardTemplateId)
                }));

            Assert.True(index.IsUnique);
        }

        [Fact]
        public void RewardRedemption_OfferAndIdempotency_AreUnique()
        {
            using var context = CreateContext();
            var entity = context.Model.FindEntityType(typeof(ClubRewardRedemption));

            Assert.True(entity!.GetIndexes().Single(item =>
                item.Properties.Count == 1 &&
                item.Properties[0].Name == nameof(ClubRewardRedemption.RewardOfferId)).IsUnique);
            Assert.True(entity.GetIndexes().Single(item =>
                item.Properties.Count == 1 &&
                item.Properties[0].Name == nameof(ClubRewardRedemption.IdempotencyKey)).IsUnique);
        }

        private static DataBaseContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=PastilClubModelTest;Trusted_Connection=True;",
                    sql => sql.UseNetTopologySuite())
                .Options;
            return new DataBaseContext(options);
        }
    }
}
