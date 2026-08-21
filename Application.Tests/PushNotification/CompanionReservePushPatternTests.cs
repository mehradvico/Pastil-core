using Application.Common.Helpers;
using Xunit;

namespace Application.Tests.PushNotification
{
    public class CompanionReservePushPatternTests
    {
        [Fact]
        public void RegisterReserveUserPattern_IdentifiesUserServiceAndCompanion()
        {
            var pattern = PersianPushTextHelper.ResolvePattern("PushRegisterReserveUser", string.Empty);

            Assert.Equal(
                "مهراد عزیز، خدمات اصلاح و شستشو در پلی کلینیک پور شعیان برای پت شما رزرو گردید",
                string.Format(pattern, "مهراد", "اصلاح و شستشو", "پلی کلینیک پور شعیان"));
        }
    }
}
