using Application.Common.Helpers;
using Xunit;

namespace Application.Tests.Security;

public class ExceptionAndOtpHelperTests
{
    [Fact]
    public void ToClientMessage_DoesNotLeakExceptionDetails()
    {
        var exception = new InvalidOperationException("Cannot insert duplicate key in object 'dbo.Users' — secret internals");

        var message = exception.ToClientMessage();

        Assert.Equal(Resource.Notification.SomethingWentWrong, message);
        Assert.DoesNotContain("dbo.Users", message);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    public void RandomDigit_ReturnsRequestedLengthOfDigitsWithNonZeroFirstDigit(int length)
    {
        for (var i = 0; i < 500; i++)
        {
            var code = GenerateHelper.RandomDigit(length);

            Assert.Equal(length, code.Length);
            Assert.All(code, c => Assert.InRange(c, '0', '9'));
            Assert.NotEqual('0', code[0]);
        }
    }

    [Fact]
    public void RandomDigit_UsesTheWholeDigitRangeIncludingNine()
    {
        var seen = new HashSet<char>();
        for (var i = 0; i < 2000; i++)
        {
            foreach (var c in GenerateHelper.RandomDigit(4))
                seen.Add(c);
        }

        Assert.Contains('9', seen);
        Assert.Equal(10, seen.Count);
    }
}
