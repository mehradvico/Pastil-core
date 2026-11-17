using Application.Common.Enumerable;
using Application.Common.Security;
using Entities.Entities.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Application.Tests.Security;

public class SuperAdminIdentityTests
{
    private const string Mobile = "09339750767";

    private static IConfiguration Config(string mobile = Mobile, string? requireTwoFactor = null)
    {
        var values = new Dictionary<string, string?> { ["SuperAdmin:Mobile"] = mobile };
        if (requireTwoFactor != null) values["SuperAdmin:RequireTwoFactor"] = requireTwoFactor;
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static User Admin(string mobile = Mobile, bool deleted = false, bool locked = false, long roleId = (long)RoleEnum.Admin) =>
        new() { Id = 1, Mobile = mobile, RoleId = roleId, Deleted = deleted, Locked = locked };

    [Fact]
    public void Matches_the_configured_active_admin_role_user()
    {
        Assert.True(SuperAdminIdentity.Matches(Admin(), Config()));
    }

    [Theory]
    [InlineData("+989339750767")]
    [InlineData("989339750767")]
    [InlineData("9339750767")]
    [InlineData("۰۹۳۳۹۷۵۰۷۶۷")]
    public void Mobile_formats_are_normalized(string stored)
    {
        Assert.True(SuperAdminIdentity.Matches(Admin(stored), Config()));
    }

    [Fact]
    public void Other_mobile_never_matches()
    {
        Assert.False(SuperAdminIdentity.Matches(Admin("09120000000"), Config()));
        // نسخه‌ی ۱۲ رقمی اشتباه تایپی نباید با شماره‌ی اصلی یکی حساب شود
        Assert.False(SuperAdminIdentity.Matches(Admin("093397506767"), Config()));
    }

    [Theory]
    [InlineData((long)RoleEnum.Customer)]
    [InlineData((long)RoleEnum.Companion)]
    [InlineData(6L)]
    public void Same_mobile_without_the_manager_role_never_matches(long roleId)
    {
        Assert.False(SuperAdminIdentity.Matches(Admin(roleId: roleId), Config()));
    }

    [Fact]
    public void Deleted_or_locked_account_never_matches()
    {
        Assert.False(SuperAdminIdentity.Matches(Admin(deleted: true), Config()));
        Assert.False(SuperAdminIdentity.Matches(Admin(locked: true), Config()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_configuration_means_there_is_no_super_admin(string configured)
    {
        Assert.False(SuperAdminIdentity.Matches(Admin(), Config(configured)));
        Assert.False(SuperAdminIdentity.Matches(Admin(""), Config(configured)));
        Assert.False(SuperAdminIdentity.Matches(Admin(), null!));
    }

    [Fact]
    public void Null_user_never_matches()
    {
        Assert.False(SuperAdminIdentity.Matches(null!, Config()));
    }

    [Fact]
    public void Two_factor_is_required_unless_explicitly_disabled()
    {
        Assert.True(SuperAdminIdentity.RequireTwoFactor(Config()));
        Assert.True(SuperAdminIdentity.RequireTwoFactor(Config(requireTwoFactor: "true")));
        Assert.True(SuperAdminIdentity.RequireTwoFactor(Config(requireTwoFactor: "garbage")));
        Assert.False(SuperAdminIdentity.RequireTwoFactor(Config(requireTwoFactor: "false")));
    }

    [Fact]
    public void Verified_flag_is_read_only_from_the_server_set_context_item()
    {
        var http = new DefaultHttpContext();
        Assert.False(SuperAdminIdentity.IsVerified(http));
        http.Items[SuperAdminIdentity.VerifiedItemKey] = "true"; // نوع اشتباه = تأیید نیست
        Assert.False(SuperAdminIdentity.IsVerified(http));
        http.Items[SuperAdminIdentity.VerifiedItemKey] = true;
        Assert.True(SuperAdminIdentity.IsVerified(http));
        Assert.False(SuperAdminIdentity.IsVerified((HttpContext)null!));
    }
}
