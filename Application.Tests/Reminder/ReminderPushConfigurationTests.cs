using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
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
                (long)PushTypeEnum.PushReminderOneDayAfter,
                (long)PushTypeEnum.PushReminderToday
            };

            Assert.Equal(new long[] { 24, 25, 26, 75 }, values);
            Assert.Equal(values.Length, values.Distinct().Count());
        }

        [Fact]
        public void ReminderPushPattern_FormatsAllTokens()
        {
            var pattern = PersianPushTextHelper.ResolvePattern("PushPetReminder", string.Empty);

            Assert.False(string.IsNullOrWhiteSpace(pattern));
            Assert.Equal(
                "یادآوری! موعد «واکسن هاری» برای پت «پاستیل» فردا است.",
                string.Format(pattern!, "پاستیل", "واکسن هاری", "فردا است."));
        }

        [Theory]
        [InlineData("PushReminderOneWeekBefore", "یادآوری! یک هفته دیگر موعد «واکسن هاری» برای پت «پاستیل» است.")]
        [InlineData("PushReminderOneDayBefore", "یادآوری! فردا موعد «واکسن هاری» برای پت «پاستیل» است.")]
        [InlineData("PushReminderOneDayAfter", "یادآوری! دیروز موعد «واکسن هاری» برای پت «پاستیل» بوده است.")]
        [InlineData("PushReminderToday", "یادآوری! امروز موعد «واکسن هاری» برای پت «پاستیل» است.")]
        public void ReminderMomentPattern_IdentifiesReminderTypeAndPet(string resourceKey, string expected)
        {
            var pattern = PersianPushTextHelper.ResolvePattern(resourceKey, string.Empty);

            Assert.Equal(expected, string.Format(pattern, "پاستیل", "واکسن هاری", string.Empty));
        }

        [Fact]
        public void ReminderTodayText_IsAPersianWhenText()
        {
            var previous = System.Globalization.CultureInfo.CurrentUICulture;
            try
            {
                System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("fa");
                Assert.Equal("امروز است.", Resource.Notification.ReminderTodayText);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentUICulture = previous;
            }
        }

        [Fact]
        public void UserReminderToday_IsAppendedAfterAllPreviousMessageTypes_NeverRenumberingThem()
        {
            // این enum ordinal تلویحی است؛ اگر یک روز مقداری وسط لیست اضافه شود همه‌ی مقادیر بعدی جابه‌جا می‌شوند.
            // این تست فقط تضمین می‌کند UserReminderToday آخرین مقدار enum است.
            var names = System.Enum.GetNames(typeof(Application.Common.Enumerable.Message.MessageTypeEnum));

            Assert.Equal("UserReminderToday", names[^1]);
        }
    }
}
