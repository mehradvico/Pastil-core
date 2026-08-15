using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Xunit;

namespace Application.Tests.PushNotification
{
    public class PushNotificationRetryModelTests
    {
        [Fact]
        public void RetryFieldsAndDispatchIndex_AreConfigured()
        {
            using var context = CreateContext();
            var entity = context.Model.FindEntityType(typeof(Entities.Entities.PushNotification));

            Assert.NotNull(entity!.FindProperty(nameof(Entities.Entities.PushNotification.AttemptCount)));
            Assert.NotNull(entity.FindProperty(nameof(Entities.Entities.PushNotification.NextAttemptDate)));
            Assert.Contains(entity.GetIndexes(), index =>
                index.Properties.Select(property => property.Name).SequenceEqual(new[]
                {
                    nameof(Entities.Entities.PushNotification.IsSend),
                    nameof(Entities.Entities.PushNotification.Status),
                    nameof(Entities.Entities.PushNotification.NextAttemptDate),
                    nameof(Entities.Entities.PushNotification.SendDate)
                }));
        }

        private static DataBaseContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=PushNotificationModelTest;Trusted_Connection=True;",
                    sql => sql.UseNetTopologySuite())
                .Options;
            return new DataBaseContext(options);
        }
    }
}
