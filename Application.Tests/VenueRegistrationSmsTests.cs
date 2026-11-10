using Application.Services.Setting.MessageSenderSrv;
using System;
using Xunit;

namespace Application.Tests;

public class VenueRegistrationSmsTests
{
    [Theory]
    [InlineData(VenueRegistrationKind.Store, "پت شاپ")]
    [InlineData(VenueRegistrationKind.Clinic, "کلینیک")]
    [InlineData(VenueRegistrationKind.Coach, "مربی")]
    [InlineData(VenueRegistrationKind.Pansion, "پانسیون")]
    [InlineData(VenueRegistrationKind.School, "مدرسه")]
    [InlineData(VenueRegistrationKind.Groomer, "آرایشگاه")]
    public void KindToken_IsAlwaysPersian(VenueRegistrationKind kind, string expected)
    {
        Assert.Equal(expected, VenueRegistrationSms.KindToken(kind));
    }

    [Fact]
    public void KindToken_CoversEveryKind()
    {
        foreach (VenueRegistrationKind kind in Enum.GetValues(typeof(VenueRegistrationKind)))
            Assert.False(string.IsNullOrWhiteSpace(VenueRegistrationSms.KindToken(kind)));
    }
}
