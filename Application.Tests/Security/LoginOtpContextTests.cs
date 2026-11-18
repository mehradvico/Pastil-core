using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Services.Accounting.OtpVerifySrv;
using Application.Services.Accounting.OtpVerifySrv.Dto;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Tests.Coach;
using Entities.Entities;
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Application.Tests.Security;

[Collection("SensitiveDataProtector")]
public class LoginOtpContextTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;
    public LoginOtpContextTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData(false, false, false, 4, MessageTypeEnum.Otp)]
    [InlineData(true, false, false, 4, MessageTypeEnum.Otp)]
    [InlineData(true, true, false, 6, MessageTypeEnum.AdminOtp)]
    [InlineData(true, true, true, 6, MessageTypeEnum.AdminOtp)]
    public async Task Login_source_selects_length_template_and_recipient(
        bool adminAccount, bool adminLogin, bool emailLogin, int length, MessageTypeEnum template)
    {
        Assert.True(_fixture.Available, "LocalDB is required to exercise OTP persistence and SMS routing.");
        using var context = _fixture.NewContext();
        var user = NewUser(adminAccount);
        context.Users.Add(user);
        if (adminLogin)
            context.SmsSettings.Add(new SmsSetting { SmsType = new MessageType { Label = "AdminOtp", Name = "Admin OTP" } });
        await context.SaveChangesAsync();

        object?[]? sent = null;
        var sender = FakeProxy<IMessageSenderService>.Create((_, args) => { sent = args; return Task.CompletedTask; });
        var service = NewService(context, sender.Instance);
        // Login codes stay four/six digits even if recovery OTPs are configured longer.
        GenerateHelper.ConfigureOtpLength("8");
        try
        {
            var dto = new OtpVerifyVDto { Mobile = emailLogin ? null! : user.Mobile, Email = emailLogin ? user.Email : null!, IsAdmin = adminLogin };
            var result = await service.InsertAsyncDto(dto);
            Assert.True(result.IsSuccess);
            Assert.Equal(length, dto.CodeLength);
            Assert.NotNull(sent);
            Assert.Equal(template, sent[0]);
            Assert.Equal(user.Mobile, sent[1]);
            Assert.Null(sent[2]);
            var code = Assert.IsType<string>(sent[4]);
            Assert.Equal(length, code.Length);
            Assert.All(code, digit => Assert.InRange(digit, '0', '9'));
            Assert.Equal(code, (await context.OtpVerifies.SingleAsync(o => o.Mobile == user.Mobile)).Code);
        }
        finally { GenerateHelper.ConfigureOtpLength(null!); }
    }

    [Fact]
    public async Task Ordinary_account_cannot_request_admin_sms()
    {
        Assert.True(_fixture.Available, "LocalDB is required to exercise OTP persistence and SMS routing.");
        using var context = _fixture.NewContext();
        var user = NewUser(false);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var sender = FakeProxy<IMessageSenderService>.Create();
        var result = await NewService(context, sender.Instance).InsertAsyncDto(new OtpVerifyVDto { Mobile = user.Mobile, IsAdmin = true });
        Assert.False(result.IsSuccess);
        Assert.Empty(sender.Proxy.Calls);
        Assert.False(await context.OtpVerifies.AnyAsync(o => o.Mobile == user.Mobile));
    }

    [Theory]
    [InlineData("1234", false, true)]
    [InlineData("1234", true, false)]
    [InlineData("123456", true, true)]
    public async Task Admin_verification_rejects_a_verified_webapp_code(string code, bool adminLogin, bool expected)
    {
        Assert.True(_fixture.Available, "LocalDB is required to exercise OTP persistence and SMS routing.");
        using var context = _fixture.NewContext();
        var mobile = NewUser(false).Mobile;
        context.OtpVerifies.Add(new OtpVerify { Mobile = mobile, Code = code, Verify = true, UpdateDate = DateTime.Now, CreateDate = DateTime.Now });
        await context.SaveChangesAsync();
        var result = await NewService(context, FakeProxy<IMessageSenderService>.Create().Instance)
            .IsVerified(new OtpVerifyVDto { Mobile = mobile, Code = code, IsAdmin = adminLogin });
        Assert.Equal(expected, result.IsSuccess);
    }

    private OtpVerifyService NewService(Persistence.Context.DataBaseContext context, IMessageSenderService sender)
    {
        var regix = FakeProxy<IRegixHelper>.Create((m, _) => m.ReturnType == typeof(Task<bool>) ? Task.FromResult(true) : null);
        return new OtpVerifyService(context, _fixture.Mapper, sender, regix.Instance, new MemoryCache(new MemoryCacheOptions()));
    }

    private static User NewUser(bool admin) => new()
    {
        Mobile = "091" + Random.Shared.NextInt64(10_000_000, 99_999_999),
        Email = Guid.NewGuid().ToString("N") + "@example.com",
        FirstName = "OTP", LastName = "Test",
        Role = new Role
        {
            Name = "OTP test", Label = "otp-" + Guid.NewGuid().ToString("N"),
            Permissions = admin ? new List<Permission> { new() { Area = "Admin", Name = "OTP test" } } : new List<Permission>()
        }
    };
}
