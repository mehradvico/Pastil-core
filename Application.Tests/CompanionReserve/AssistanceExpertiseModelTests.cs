using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Xunit;

namespace Application.Tests.CompanionReserve
{
    public class AssistanceExpertiseModelTests
    {
        [Fact]
        public void AssistanceExpertise_PairIsUnique_SoAServiceCannotListTheSameExpertiseTwice()
        {
            using var context = CreateContext();
            var entity = context.Model.FindEntityType(typeof(AssistanceExpertise));
            var index = entity!.GetIndexes().Single(item =>
                item.Properties.Select(property => property.Name).SequenceEqual(new[]
                {
                    nameof(AssistanceExpertise.AssistanceId),
                    nameof(AssistanceExpertise.ExpertiseId)
                }));

            Assert.True(index.IsUnique);
        }

        [Fact]
        public void AssistanceExpertise_DeletingAnExpertiseInUseIsRestricted_ButDeletingTheServiceCascades()
        {
            using var context = CreateContext();
            var entity = context.Model.FindEntityType(typeof(AssistanceExpertise));

            var toExpertise = entity!.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Expertise));
            var toAssistance = entity.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Entities.Entities.Assistance));

            Assert.Equal(DeleteBehavior.Restrict, toExpertise.DeleteBehavior);
            Assert.Equal(DeleteBehavior.Cascade, toAssistance.DeleteBehavior);
        }

        private static DataBaseContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=AssistanceExpertiseModelTest;Trusted_Connection=True;",
                    sql => sql.UseNetTopologySuite())
                .Options;
            return new DataBaseContext(options);
        }
    }
}
