using Application.Common.Dto.Result;
using Application.Common.Helpers.Iface;
using Application.Common.Security;
using Application.Common.Security.Iface;
using Application.Services.Accounting.OtpVerifySrv;
using Application.Services.UserSrv;
using Application.Services.Accounting.UserSrv.Dto;
using Application.Services.Accounting.UserTokenSrv.Iface;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.Setting.BaseDetailSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Services.Setting.NoticeSrv.Iface;
using Entities.Entities;
using Entities.Entities.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Application.Tests.Coach;

/// <summary>changepassword / forgetpassword / resetpassword فقط برای ارائه‌دهنده‌ها (Provider) کار می‌کنند و کد OTP یک‌بار مصرف است.</summary>
public class ProviderPasswordTests : IClassFixture<CoachLocalDbFixture>
{
    private const string OldPassword = "Old12345!";
    private readonly CoachLocalDbFixture _fixture;

    public ProviderPasswordTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private sealed class Harness
    {
        public UserService Users = null!;
        public FakeProxy<IMessageSenderService> Sender = null!;
        public UserPasswordService Passwords = null!;
    }

    private Harness NewHarness()
    {
        var sender = FakeProxy<IMessageSenderService>.Create();
        var regix = FakeProxy<IRegixHelper>.Create((m, _) => m.ReturnType == typeof(Task<bool>) ? Task.FromResult(true) : null);
        var passwords = new UserPasswordService(new PasswordHasher<User>(), new ConfigurationBuilder().Build());
        var otp = new OtpVerifyService(_fixture.NewContext(), _fixture.Mapper, sender.Instance, regix.Instance, new MemoryCache(new MemoryCacheOptions()));
        var tokens = FakeProxy<IUserTokenService>.Create((m, _) =>
            m.Name == nameof(IUserTokenService.ResetTokenAsync) ? Task.FromResult(new BaseResultDto(true)) : null);
        var users = new UserService(_fixture.NewContext(), FakeProxy<IPushNotificationService>.Create().Instance, tokens.Instance, otp, _fixture.Mapper,
            new ConfigurationBuilder().Build(), regix.Instance, FakeProxy<IBaseDetailService>.Create().Instance, sender.Instance,
            FakeProxy<INoticeService>.Create().Instance, passwords, FakeProxy<IClubPointIntegrationService>.Create().Instance,
            FakeProxy<ISecurityAudit>.Create().Instance, new LoginThrottle());
        return new Harness { Users = users, Sender = sender.Proxy, Passwords = passwords };
    }

    private async Task SetPasswordAsync(Harness h, long userId)
    {
        using var context = _fixture.NewContext();
        var user = await context.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
        var hash = h.Passwords.HashPassword(user, OldPassword);
        await context.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(s => s.SetProperty(u => u.Password, hash));
    }

    private async Task<(CoachLocalDbFixture.SeedData Seed, string ProviderMobile, string OtherMobile)> BuildAsync(Harness h)
    {
        var seed = _fixture.Seed(); // owner = ارائه‌دهنده (Provider.UserId)، other = کاربر عادی
        await SetPasswordAsync(h, seed.OwnerUserId);
        await SetPasswordAsync(h, seed.OtherUserId);
        return (seed, seed.OwnerMobile, seed.OtherMobile);
    }

    private async Task<bool> HasPasswordAsync(Harness h, long userId, string password)
    {
        using var context = _fixture.NewContext();
        var user = await context.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
        return h.Passwords.VerifyPassword(user, user.Password, password).Succeeded;
    }

    private async Task VerifyOtpAsync(string mobile, string code)
    {
        using var context = _fixture.NewContext();
        context.OtpVerifies.RemoveRange(context.OtpVerifies.Where(o => o.Mobile == mobile));
        context.OtpVerifies.Add(new OtpVerify { Mobile = mobile, Code = code, Verify = true, TryCount = 0, CreateDate = DateTime.Now, UpdateDate = DateTime.Now });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task ForgetPassword_SendsCodeOnlyToProviders_WithExactlyOneIdentifier()
    {
        if (!_fixture.Available) return;
        var h = NewHarness();
        var (_, providerMobile, otherMobile) = await BuildAsync(h);

        Assert.False((await h.Users.ForgetPassword(new ForgetPasswordDto { Mobile = otherMobile })).IsSuccess); // کاربر عادی
        Assert.False((await h.Users.ForgetPassword(new ForgetPasswordDto { Mobile = "09990000000" })).IsSuccess); // ناشناس
        Assert.False((await h.Users.ForgetPassword(new ForgetPasswordDto { Mobile = providerMobile, Email = "x@y.com" })).IsSuccess); // دو شناسه
        Assert.False((await h.Users.ForgetPassword(new ForgetPasswordDto())).IsSuccess);
        Assert.DoesNotContain(nameof(IMessageSenderService.SendMessageAsync), h.Sender.Calls);

        Assert.True((await h.Users.ForgetPassword(new ForgetPasswordDto { Mobile = providerMobile })).IsSuccess);
        Assert.Contains(nameof(IMessageSenderService.SendMessageAsync), h.Sender.Calls);
    }

    [Fact]
    public async Task ChangePassword_WorksOnlyForProviders_WithValidInput()
    {
        if (!_fixture.Available) return;
        var h = NewHarness();
        var (seed, providerMobile, otherMobile) = await BuildAsync(h);

        // ورودی ناقص (قبلاً موبایل null اولین کاربر بدون ایمیل را پیدا می‌کرد)
        Assert.False((await h.Users.ChangePassword(new ChangePasswordDto { Mobile = null, OldPassword = OldPassword, NewPassword = "New12345!" })).IsSuccess);
        Assert.False((await h.Users.ChangePassword(new ChangePasswordDto { Mobile = providerMobile, OldPassword = null, NewPassword = "New12345!" })).IsSuccess);
        // رمز جدید = رمز قدیم
        Assert.False((await h.Users.ChangePassword(new ChangePasswordDto { Mobile = providerMobile, OldPassword = OldPassword, NewPassword = OldPassword })).IsSuccess);
        // کاربر عادی
        Assert.False((await h.Users.ChangePassword(new ChangePasswordDto { Mobile = otherMobile, OldPassword = OldPassword, NewPassword = "New12345!" })).IsSuccess);
        Assert.True(await HasPasswordAsync(h, seed.OtherUserId, OldPassword));
        // رمز قدیم اشتباه
        Assert.False((await h.Users.ChangePassword(new ChangePasswordDto { Mobile = providerMobile, OldPassword = "Wrong12345!", NewPassword = "New12345!" })).IsSuccess);
        Assert.True(await HasPasswordAsync(h, seed.OwnerUserId, OldPassword));

        Assert.True((await h.Users.ChangePassword(new ChangePasswordDto { Mobile = providerMobile, OldPassword = OldPassword, NewPassword = "New12345!" })).IsSuccess);
        Assert.True(await HasPasswordAsync(h, seed.OwnerUserId, "New12345!"));
        Assert.False(await HasPasswordAsync(h, seed.OwnerUserId, OldPassword));
    }

    [Fact]
    public async Task ResetPassword_NeedsVerifiedCodeOfTheSameIdentifier_AndCodeIsSingleUse()
    {
        if (!_fixture.Available) return;
        var h = NewHarness();
        var (seed, providerMobile, otherMobile) = await BuildAsync(h);

        // مهاجم: موبایل خودش (کد تأییدشده) + ایمیل قربانی ← رد می‌شود
        await VerifyOtpAsync(otherMobile, "123456");
        using (var ctx = _fixture.NewContext())
            await ctx.Users.Where(u => u.Id == seed.OwnerUserId).ExecuteUpdateAsync(s => s.SetProperty(u => u.Email, "victim@example.com"));
        Assert.False((await NewHarness().Users.ResetPassword(new ResetPasswordDto { Mobile = otherMobile, Email = "victim@example.com", Code = "123456", Password = "Hacked123!" })).IsSuccess);
        Assert.True(await HasPasswordAsync(h, seed.OwnerUserId, OldPassword));

        // کاربر عادی با کد معتبر خودش هم نمی‌تواند (فقط ارائه‌دهنده)
        Assert.False((await NewHarness().Users.ResetPassword(new ResetPasswordDto { Mobile = otherMobile, Code = "123456", Password = "Hacked123!" })).IsSuccess);
        Assert.True(await HasPasswordAsync(h, seed.OtherUserId, OldPassword));

        // ارائه‌دهنده: کد اشتباه رد، کد درست قبول، و همان کد دوباره قبول نمی‌شود
        await VerifyOtpAsync(providerMobile, "654321");
        Assert.False((await NewHarness().Users.ResetPassword(new ResetPasswordDto { Mobile = providerMobile, Code = "000000", Password = "New12345!" })).IsSuccess);
        Assert.True((await NewHarness().Users.ResetPassword(new ResetPasswordDto { Mobile = providerMobile, Code = "654321", Password = "New12345!" })).IsSuccess);
        Assert.True(await HasPasswordAsync(h, seed.OwnerUserId, "New12345!"));
        Assert.False((await NewHarness().Users.ResetPassword(new ResetPasswordDto { Mobile = providerMobile, Code = "654321", Password = "Again12345!" })).IsSuccess);
        Assert.True(await HasPasswordAsync(h, seed.OwnerUserId, "New12345!"));
    }
}
