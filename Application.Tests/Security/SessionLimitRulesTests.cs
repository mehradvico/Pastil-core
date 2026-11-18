using Application.Services.Accounting.UserTokenSrv;
using Xunit;

namespace Application.Tests.Security;

public class SessionLimitRulesTests
{
    [Theory]
    [InlineData("3", false, true, 2)]
    [InlineData("5", false, true, 4)]
    [InlineData("2", false, true, 1)]
    [InlineData("50", false, true, 9)]
    [InlineData("1", false, true, 0)]
    [InlineData("0", false, true, 0)]
    [InlineData(null, false, true, 2)]
    [InlineData("", false, true, 2)]
    [InlineData("abc", false, true, 2)]
    [InlineData("3", true, true, 0)]
    [InlineData("3", false, false, 0)]
    public void SessionsToKeep(string configured, bool isPanelLogin, bool isPartner, int expected)
    {
        Assert.Equal(expected, SessionLimitRules.SessionsToKeepOnSignIn(configured, isPanelLogin, isPartner));
    }

    [Theory]
    [InlineData(null, true, false)]
    [InlineData(null, false, false)]
    [InlineData("1", true, true)]
    [InlineData("10", false, true)]
    public void Admin_keeps_two_existing_sessions_across_panel_and_client(string configured, bool isPanelLogin, bool isPartner)
    {
        Assert.Equal(2, SessionLimitRules.SessionsToKeepOnSignIn(configured, isPanelLogin, isPartner, isAdminAccount: true));
    }
}
