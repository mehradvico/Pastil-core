using Application.Common.Helpers;
using Application.Common.Enumerable.Code;
using System.Globalization;
using Xunit;

namespace Application.Tests.PushNotification
{
    public class PersianPushTextHelperTests
    {
        [Theory]
        [InlineData("en-US")]
        [InlineData("fa-IR")]
        public void ResolvePattern_AlwaysReturnsPersianResource(string currentCulture)
        {
            var previousCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(currentCulture);

                var title = PersianPushTextHelper.ResolvePattern("Push_Title", PersianPushTextHelper.DefaultTitle);
                var body = PersianPushTextHelper.ResolvePattern("PushSignUpUser", PersianPushTextHelper.DefaultBody);

                Assert.Equal("پاستیل", title);
                Assert.Contains("به پاستیل خوش اومدی", body);
            }
            finally
            {
                CultureInfo.CurrentUICulture = previousCulture;
            }
        }

        [Fact]
        public void ResolvePattern_PreservesPersianLiteral()
        {
            const string literal = "رزرو جدید برای شما";

            var result = PersianPushTextHelper.ResolvePattern(literal, PersianPushTextHelper.DefaultTitle);

            Assert.Equal(literal, result);
        }

        [Fact]
        public void ResolvePattern_ReplacesUnknownEnglishTextWithPersianFallback()
        {
            var result = PersianPushTextHelper.ResolvePattern("Unknown English notification", PersianPushTextHelper.DefaultBody);

            Assert.Equal(PersianPushTextHelper.DefaultBody, result);
        }

        [Fact]
        public void EveryPushType_HasAPersianResource()
        {
            foreach (var resourceKey in Enum.GetNames<PushTypeEnum>())
            {
                var result = PersianPushTextHelper.ResolvePattern(resourceKey, string.Empty);

                Assert.True(
                    PersianPushTextHelper.ContainsPersian(result),
                    $"Persian push resource is missing for {resourceKey}.");
            }
        }

        [Fact]
        public void SignInPushType_IsRemoved()
        {
            Assert.DoesNotContain("PushSignInUser", Enum.GetNames<PushTypeEnum>());
        }
    }
}
