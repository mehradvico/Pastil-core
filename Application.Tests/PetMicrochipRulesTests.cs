using Application.Services.Accounting.PetMicrochipSrv;
using System;
using Xunit;

namespace Application.Tests;

public class PetMicrochipRulesTests
{
    [Theory]
    [InlineData("981020000123456", "981020000123456")]
    [InlineData("۹۸۱ ۰۲۰-۰۰۰۱۲۳۴۵۶", "981020000123456")]
    [InlineData("  123456789 ", "123456789")]
    public void Microchip_is_normalized_to_ascii_digits(string input, string expected)
        => Assert.Equal(expected, PetMicrochipRules.NormalizeMicrochip(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345678")]
    [InlineData("1234567890123456")]
    [InlineData("98102000012345a")]
    [InlineData("' OR 1=1 --")]
    public void Invalid_microchip_is_rejected(string input)
        => Assert.Null(PetMicrochipRules.NormalizeMicrochip(input));

    [Theory]
    [InlineData("09121234567", "09121234567")]
    [InlineData("+989121234567", "09121234567")]
    [InlineData("989121234567", "09121234567")]
    [InlineData("۰۹۱۲۱۲۳۴۵۶۷", "09121234567")]
    [InlineData("9121234567", "09121234567")]
    [InlineData("0912-123 4567", "09121234567")]
    public void Mobile_is_normalized(string input, string expected)
        => Assert.Equal(expected, PetMicrochipRules.NormalizeMobile(input));

    [Theory]
    [InlineData("02112345678")]
    [InlineData("0912123")]
    [InlineData("abc")]
    [InlineData(null)]
    public void Invalid_mobile_is_rejected(string input)
        => Assert.Null(PetMicrochipRules.NormalizeMobile(input));

    [Fact]
    public void Token_is_valid_for_24_hours_only()
    {
        var created = new DateTime(2026, 10, 4, 10, 0, 0);
        Assert.True(PetMicrochipRules.IsTokenFresh(created, created.AddHours(23)));
        Assert.False(PetMicrochipRules.IsTokenFresh(created, created.AddHours(25)));
    }

    [Theory]
    [InlineData(2, 3, true)]
    [InlineData(2, 4, true)]
    [InlineData(2, 5, true)]
    [InlineData(3, 4, true)]
    [InlineData(4, 3, true)]
    [InlineData(5, 3, true)]
    [InlineData(3, 2, false)]
    [InlineData(1, 3, false)]
    [InlineData(4, 5, false)]
    public void Admin_status_transitions(int from, int to, bool allowed)
        => Assert.Equal(allowed, PetMicrochipRules.CanTransition(from, to));

    [Fact]
    public void Tokens_are_unique_and_unguessable_length()
    {
        var a = PetMicrochipRules.NewToken();
        Assert.NotEqual(a, PetMicrochipRules.NewToken());
        Assert.Equal(32, a.Length);
    }
}
