using System.Text.Json;
using Application.Services.Setting.NoticeSrv;
using Xunit;

namespace Application.Tests.Notice;

public class PanelNoticePushPayloadTests
{
    [Fact]
    public void BuildPayload_KeepsPersianText_AndRelativeUrl()
    {
        using var doc = JsonDocument.Parse(PanelNoticePushService.BuildPayload(42, "ثبت‌نام کاربر", "کاربر جدید ثبت‌نام کرد", "/admin/user/edit-5"));
        var root = doc.RootElement;
        Assert.Equal("ثبت‌نام کاربر", root.GetProperty("title").GetString());
        Assert.Equal("کاربر جدید ثبت‌نام کرد", root.GetProperty("body").GetString());
        Assert.Equal("/admin/user/edit-5", root.GetProperty("url").GetString());
        Assert.Equal("notice-42", root.GetProperty("tag").GetString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://evil.example/x")]
    [InlineData("//evil.example/x")]
    public void BuildPayload_FallsBackToNoticePage_ForMissingOrExternalUrls(string url)
    {
        using var doc = JsonDocument.Parse(PanelNoticePushService.BuildPayload(1, "t", "m", url));
        Assert.Equal("/admin/notice", doc.RootElement.GetProperty("url").GetString());
    }
}
