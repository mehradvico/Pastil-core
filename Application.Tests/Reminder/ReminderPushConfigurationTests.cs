using Application.Common.Enumerable.Code;
using Xunit;

namespace Application.Tests.Reminder
{
    public class ReminderPushConfigurationTests
    {
        [Fact]
        public void ReminderPushTypes_HaveStableDistinctIdentifiers()
        {
            var values = new[]
            {
                (long)PushTypeEnum.PushReminderOneWeekBefore,
                (long)PushTypeEnum.PushReminderOneDayBefore,
                (long)PushTypeEnum.PushReminderOneDayAfter
            };

            Assert.Equal(new long[] { 24, 25, 26 }, values);
            Assert.Equal(values.Length, values.Distinct().Count());
        }

        [Fact]
        public void ReminderPushPattern_FormatsAllTokens()
        {
            var pattern = Resource.Pattern.ResourceManager.GetString("PushPetReminder");

            Assert.False(string.IsNullOrWhiteSpace(pattern));
            Assert.Contains("واکسن هاری", string.Format(pattern!, "پاستیل", "واکسن هاری", "فردا"));
        }
    }
}
