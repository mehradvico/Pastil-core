using Application.Common.Security;
using ClosedXML.Excel;
using Xunit;

namespace Application.Tests.Security;

public class UploadAndFileGuardTests
{
    // ---------- امضای فایل (File/Security/UploadSignature.cs) ----------

    private static readonly byte[] Jpeg = { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, (byte)'J', (byte)'F' };
    private static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0 };
    private static readonly byte[] Html = System.Text.Encoding.ASCII.GetBytes("<html><script>alert(1)</script></html>");

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    public void Jpeg_signature_is_accepted_only_for_jpeg_extensions(string extension)
    {
        Assert.True(UploadGuards.UploadSignature.Matches(Jpeg, extension));
        Assert.False(UploadGuards.UploadSignature.Matches(Jpeg, ".png"));
    }

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".png")]
    [InlineData(".gif")]
    [InlineData(".webp")]
    [InlineData(".pdf")]
    [InlineData(".mp4")]
    [InlineData(".mov")]
    [InlineData(".webm")]
    [InlineData(".ogg")]
    public void Html_content_is_rejected_for_every_allowed_extension(string extension)
    {
        Assert.False(UploadGuards.UploadSignature.Matches(Html, extension));
    }

    [Fact]
    public void Real_signatures_of_other_formats_are_accepted()
    {
        Assert.True(UploadGuards.UploadSignature.Matches(Png, ".png"));
        Assert.True(UploadGuards.UploadSignature.Matches("GIF89a...."u8, ".gif"));
        Assert.True(UploadGuards.UploadSignature.Matches("RIFF\0\0\0\0WEBPVP8 "u8, ".webp"));
        Assert.True(UploadGuards.UploadSignature.Matches("%PDF-1.7\n"u8, ".pdf"));
        Assert.True(UploadGuards.UploadSignature.Matches(new byte[] { 0, 0, 0, 0x18, (byte)'f', (byte)'t', (byte)'y', (byte)'p', (byte)'m', (byte)'p', (byte)'4', (byte)'2' }, ".mp4"));
        Assert.True(UploadGuards.UploadSignature.Matches(new byte[] { 0x1A, 0x45, 0xDF, 0xA3, 1 }, ".webm"));
        Assert.True(UploadGuards.UploadSignature.Matches("OggS\0"u8, ".ogg"));
    }

    [Theory]
    [InlineData(".svg")]
    [InlineData(".html")]
    [InlineData("")]
    [InlineData(null)]
    public void Unknown_extensions_never_match(string? extension)
    {
        Assert.False(UploadGuards.UploadSignature.Matches(Jpeg, extension!));
    }

    // ---------- آدرس ضمیمه‌ی چت ----------

    [Theory]
    [InlineData("/Media/2026/9/20/abc.webp")]
    [InlineData("Media/2026/9/20/abc.webp")]
    [InlineData("https://file.pastil.pet/Media/2026/9/20/abc.webp")]
    [InlineData("https://pastil.pet/x.png")]
    public void Attachment_urls_on_our_file_host_or_relative_are_allowed(string url)
    {
        Assert.True(AttachmentUrlPolicy.IsAllowed(url));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html;base64,PHNjcmlwdD4=")]
    [InlineData("http://file.pastil.pet/a.png")]                 // بدون TLS
    [InlineData("https://evil.example/a.png")]
    [InlineData("https://pastil.pet.evil.example/a.png")]
    [InlineData("https://user@file.pastil.pet/a.png")]
    [InlineData("//evil.example/a.png")]
    [InlineData("/Media/../../etc/passwd")]
    [InlineData("Media\\..\\x.png")]
    public void Attachment_urls_pointing_elsewhere_or_traversing_are_rejected(string url)
    {
        Assert.False(AttachmentUrlPolicy.IsAllowed(url));
    }

    [Fact]
    public void Optional_attachment_url_accepts_empty_but_not_bad_values()
    {
        Assert.True(AttachmentUrlPolicy.IsAllowedOptional(null!));
        Assert.True(AttachmentUrlPolicy.IsAllowedOptional(""));
        Assert.False(AttachmentUrlPolicy.IsAllowedOptional("javascript:alert(1)"));
    }

    // ---------- خروجی Excel (CSV/formula injection) ----------

    [Fact]
    public void ClosedXml_does_not_turn_assigned_text_starting_with_equals_into_a_formula()
    {
        // ادمین فایل خروجی را در Excel باز می‌کند؛ اگر ClosedXML متن کاربر را formula کند، نام کاربر (=HYPERLINK(...)) اجرا می‌شود.
        using var workbook = new XLWorkbook();
        var cell = workbook.AddWorksheet("t").Cell(1, 1);
        cell.Value = "=HYPERLINK(\"https://evil.example\",\"x\")";

        Assert.False(cell.HasFormula);
    }
}
