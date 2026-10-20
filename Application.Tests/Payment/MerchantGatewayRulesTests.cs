using Application.Common.Enumerable;
using Application.Services.Dto;
using Application.Services.Order.MerchantSrv;
using Xunit;

namespace Application.Tests.Payment;

public class MerchantGatewayRulesTests
{
    [Fact]
    public void MainAdmin_GetsZarinPalAndTestGateway_EvenThoughRoleLabelIsMainAdmin()
    {
        var mainAdmin = new CurrentUserDto { UserId = 1, RoleId = 2, RoleEnum = "main-admin" };

        Assert.True(MerchantGatewayRules.IsMainAdmin(mainAdmin));
        var allowed = MerchantGatewayRules.AllowedBankIds(mainAdmin);
        Assert.Contains((long)MerchantEnum.zarinpal, allowed);
        Assert.Contains((long)MerchantEnum.pastilTest, allowed);
    }

    [Theory]
    [InlineData(1L, "Customer")]
    [InlineData(3L, "Companion")]
    [InlineData(4L, "Operator")]
    [InlineData(5L, "Store")]
    [InlineData(6L, "Content-manager")]
    [InlineData(7L, "seo-agent")]
    public void OtherRoles_GetOnlyZarinPal(long roleId, string label)
    {
        var user = new CurrentUserDto { UserId = 9, RoleId = roleId, RoleEnum = label };

        Assert.False(MerchantGatewayRules.IsMainAdmin(user));
        Assert.Equal(new[] { (long)MerchantEnum.zarinpal }, MerchantGatewayRules.AllowedBankIds(user));
    }

    [Fact]
    public void Guest_GetsOnlyZarinPal()
    {
        Assert.Equal(new[] { (long)MerchantEnum.zarinpal }, MerchantGatewayRules.AllowedBankIds(null));
    }
}
