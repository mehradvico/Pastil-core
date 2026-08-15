using Application.Common.Enumerable.Code;
using Xunit;

namespace Application.Tests.PastilMatch
{
    public class PastilMatchPushConfigurationTests
    {
        [Fact]
        public void PushTypes_HaveStableDistinctIdentifiers()
        {
            var values = new[]
            {
                (long)PushTypeEnum.PushPastilMatchRequestReceived,
                (long)PushTypeEnum.PushPastilMatchRequestAccepted,
                (long)PushTypeEnum.PushPastilMatchRequestRejected,
                (long)PushTypeEnum.PushPastilMatchNewMessage,
                (long)PushTypeEnum.PushPastilMatchProfileLiked,
                (long)PushTypeEnum.PushPastilMatchClosed,
                (long)PushTypeEnum.PushPastilMatchVerificationApproved,
                (long)PushTypeEnum.PushPastilMatchVerificationRejected,
                (long)PushTypeEnum.PushPastilMatchMessageReaction,
                (long)PushTypeEnum.PushPastilMatchRequestCancelled
            };

            Assert.Equal(Enumerable.Range(27, 10).Select(value => (long)value), values);
            Assert.Equal(values.Length, values.Distinct().Count());
        }

        [Theory]
        [InlineData("PushPastilMatchRequestReceived")]
        [InlineData("PushPastilMatchRequestAccepted")]
        [InlineData("PushPastilMatchRequestRejected")]
        [InlineData("PushPastilMatchNewMessage")]
        [InlineData("PushPastilMatchProfileLiked")]
        [InlineData("PushPastilMatchClosed")]
        [InlineData("PushPastilMatchVerificationApproved")]
        [InlineData("PushPastilMatchVerificationRejected")]
        [InlineData("PushPastilMatchMessageReaction")]
        [InlineData("PushPastilMatchRequestCancelled")]
        public void PushPattern_ExistsAndCanBeFormatted(string resourceKey)
        {
            var pattern = Resource.Pattern.ResourceManager.GetString(resourceKey);

            Assert.False(string.IsNullOrWhiteSpace(pattern));
            var exception = Record.Exception(() => string.Format(pattern!, "مهراد", "پاستیل"));
            Assert.Null(exception);
        }
    }
}
