using Application.Services.Accounting.UserTokenSrv;
using Xunit;

namespace Application.Tests.Security;

public class PanelSessionDeviceTests
{
    [Fact]
    public void BuildName_StoresOnlyAHashOfTheDeviceId()
    {
        var name = PanelSessionDevice.BuildName("my-secret-device-id");

        Assert.StartsWith(PanelSessionDevice.Prefix, name);
        Assert.DoesNotContain("my-secret-device-id", name);
        Assert.True(name.Length > PanelSessionDevice.Prefix.Length);
    }

    [Fact]
    public void Matches_AllowsTheSameDevice_AndRejectsAnotherOne()
    {
        var stored = PanelSessionDevice.BuildName("device-A");

        Assert.True(PanelSessionDevice.Matches(stored, "device-A"));
        Assert.True(PanelSessionDevice.Matches(stored, "  device-A  "));
        Assert.False(PanelSessionDevice.Matches(stored, "device-B"));
        Assert.False(PanelSessionDevice.Matches(stored, null));
        Assert.False(PanelSessionDevice.Matches(stored, ""));
    }

    [Fact]
    public void Matches_DoesNotBindSessionsWithoutADeviceId_OrNonPanelSessions()
    {
        // پنل قدیمی که deviceId نمی‌فرستاد: فقط برچسب panel: بدون گره
        var unbound = PanelSessionDevice.BuildName(null);
        Assert.Equal(PanelSessionDevice.Prefix, unbound);
        Assert.True(PanelSessionDevice.Matches(unbound, "anything"));
        Assert.True(PanelSessionDevice.Matches(unbound, null));

        // وب‌اپ/اپ موبایل (DeviceName قدیمی) و توکن‌های بدون DeviceName
        Assert.True(PanelSessionDevice.Matches("zand", "anything"));
        Assert.True(PanelSessionDevice.Matches(null, null));
    }

    [Fact]
    public void Matches_RejectsAbsurdlyLongDeviceIds()
    {
        var stored = PanelSessionDevice.BuildName("device-A");

        Assert.False(PanelSessionDevice.Matches(stored, new string('x', 5000)));
        Assert.Equal(PanelSessionDevice.Prefix, PanelSessionDevice.BuildName(new string('x', 5000)));
    }

    [Fact]
    public void IsPanelSession_RecognizesOnlyThePanelPrefix()
    {
        Assert.True(PanelSessionDevice.IsPanelSession("panel:abc"));
        Assert.True(PanelSessionDevice.IsPanelSession("panel:"));
        Assert.False(PanelSessionDevice.IsPanelSession("zand"));
        Assert.False(PanelSessionDevice.IsPanelSession(null));
    }
}
