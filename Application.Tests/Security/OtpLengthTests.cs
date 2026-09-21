using Application.Common.Helpers;
using Xunit;

namespace Application.Tests.Security;

[Collection("SensitiveDataProtector")] // GenerateHelper.OtpLength هم ایستا است؛ موازی با بقیه‌ی تست‌های ایستا اجرا نشود
public class OtpLengthTests
{
    [Theory]
    [InlineData(null, 4)]
    [InlineData("", 4)]
    [InlineData("abc", 4)]
    [InlineData("3", 4)]
    [InlineData("4", 4)]
    [InlineData("6", 6)]
    [InlineData("8", 8)]
    [InlineData("9", 4)]
    public void Configured_length_is_clamped_to_a_safe_range_and_defaults_to_four(string? configured, int expected)
    {
        GenerateHelper.ConfigureOtpLength(configured!);

        Assert.Equal(expected, GenerateHelper.OtpLength);
        GenerateHelper.ConfigureOtpLength(null!);
    }

    [Fact]
    public void Generated_code_has_the_requested_length_digits_only_and_no_leading_zero()
    {
        for (var i = 0; i < 200; i++)
        {
            var code = GenerateHelper.RandomDigit(6);

            Assert.Equal(6, code.Length);
            Assert.All(code, c => Assert.InRange(c, '0', '9'));
            Assert.NotEqual('0', code[0]);
        }
    }
}
