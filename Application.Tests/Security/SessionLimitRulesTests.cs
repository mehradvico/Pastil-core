using Application.Services.Accounting.UserTokenSrv;
using Xunit;

namespace Application.Tests.Security;

public class SessionLimitRulesTests
{
    [Theory]
    [InlineData("5", false, true, 4)]
    [InlineData("2", false, true, 1)]
    [InlineData("50", false, true, 9)]
    [InlineData("1", false, true, 0)]
    [InlineData(null, false, true, 0)]
    [InlineData("abc", false, true, 0)]
    [InlineData("5", true, true, 0)]
    [InlineData("5", false, false, 0)]
    public void SessionsToKeep(string configured, bool isPanelLogin, bool isCompanionOwner, int expected)
    {
        Assert.Equal(expected, SessionLimitRules.SessionsToKeepOnSignIn(configured, isPanelLogin, isCompanionOwner));
    }
}
