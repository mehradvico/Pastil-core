using Application.Common.Enumerable.Code;
using Xunit;

namespace Application.Tests.PartnerApplications
{
    public class PartnerApplicationPushConfigurationTests
    {
        [Fact]
        public void PartnerDecisionPushTypes_HaveStableDistinctIdentifiers()
        {
            var values = new[]
            {
                (long)PushTypeEnum.PushCompanionRequestApproved,
                (long)PushTypeEnum.PushCompanionRequestRejected,
                (long)PushTypeEnum.PushDriverRequestApproved,
                (long)PushTypeEnum.PushDriverRequestRejected,
                (long)PushTypeEnum.PushStoreRequestApproved,
                (long)PushTypeEnum.PushStoreRequestRejected,
                (long)PushTypeEnum.PushPansionRequestApproved,
                (long)PushTypeEnum.PushPansionRequestRejected
            };

            Assert.Equal(Enumerable.Range(37, 8).Select(value => (long)value), values);
            Assert.Equal(values.Length, values.Distinct().Count());
        }

        [Theory]
        [InlineData("PushCompanionRequestApproved")]
        [InlineData("PushCompanionRequestRejected")]
        [InlineData("PushDriverRequestApproved")]
        [InlineData("PushDriverRequestRejected")]
        [InlineData("PushStoreRequestApproved")]
        [InlineData("PushStoreRequestRejected")]
        [InlineData("PushPansionRequestApproved")]
        [InlineData("PushPansionRequestRejected")]
        public void PartnerDecisionPushPattern_ExistsAndCanBeFormatted(string resourceKey)
        {
            var pattern = Resource.Pattern.ResourceManager.GetString(resourceKey);

            Assert.False(string.IsNullOrWhiteSpace(pattern));
            Assert.Null(Record.Exception(() => string.Format(pattern!, "پاستیل", "نیاز به اصلاح اطلاعات")));
        }
    }
}
