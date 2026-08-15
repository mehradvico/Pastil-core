using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Persistence.Context;
using Xunit;

namespace Application.Tests.Reminder
{
    public class ReminderModelTests
    {
        [Fact]
        public void Reminder_Identity_HasFilteredUniqueIndex()
        {
            using var context = CreateContext();
            var entity = context.Model.FindEntityType(typeof(Entities.Entities.Reminder));
            var index = entity!.GetIndexes().Single(item =>
                item.Properties.Select(property => property.Name).SequenceEqual(new[]
                {
                    nameof(Entities.Entities.Reminder.UserPetId),
                    nameof(Entities.Entities.Reminder.ReminderTypeId),
                    nameof(Entities.Entities.Reminder.ReminderCycleId),
                    nameof(Entities.Entities.Reminder.StartDate)
                }));

            Assert.True(index.IsUnique);
            Assert.Equal("[Deleted] = 0", index.GetFilter());
        }

        [Fact]
        public void ReminderCycle_HasPositiveCycleCheckConstraint()
        {
            using var context = CreateContext();
            var model = context.GetService<IDesignTimeModel>().Model;
            var entity = model.FindEntityType(typeof(ReminderCycle));
            var constraint = entity!.GetCheckConstraints().Single(item =>
                item.Name == "CK_ReminderCycle_Cycle");

            Assert.Equal("[Cycle] > 0", constraint.Sql);
        }

        private static DataBaseContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=ReminderModelTest;Trusted_Connection=True;",
                    sql => sql.UseNetTopologySuite())
                .Options;
            return new DataBaseContext(options);
        }
    }
}
