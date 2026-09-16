using Application.Services.Setting.NoticeSrv;
using Xunit;

namespace Application.Tests.Notice;

public class NoticeNavigationResolverTests
{
    [Theory]
    [InlineData("Trip", 41, "/admin/trip/detail-41")]
    [InlineData("CompanionReserve", 42, "/admin/companionreserve/detail-42")]
    [InlineData("PansionReserve", 43, "/admin/pansionreserve/detail-43")]
    [InlineData("ProductOrder", 44, "/admin/productorder/detail-44")]
    [InlineData("Driver", 45, "/admin/driver/edit-45")]
    [InlineData("User", 46, "/admin/user/profile-46")]
    [InlineData("UserBankCard", 47, "/admin/userbankcard?userBankCardId=47")]
    [InlineData("PastilMatchProfile", 48, "/admin/pastilmatchprofile?profileId=48")]
    [InlineData("PastilMatchReport", 49, "/admin/pastilmatchreport?reportId=49")]
    public void Resolve_UsesTheExactReferencedPanelItem(string referenceType, long referenceId, string expected)
    {
        var actual = NoticeNavigationResolver.Resolve("/admin/notice", "test", referenceType, referenceId);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Resolve_UsesMetadataForCommentAndNestedResources()
    {
        var route = NoticeNavigationResolver.Resolve(
            "/admin/notice",
            "Companion.CommentSubmitted",
            "CompanionComment",
            10,
            new Dictionary<string, string> { ["companionId"] = "25" });

        Assert.Equal("/admin/companion-comment?companionId=25", route);
    }

    [Fact]
    public void Resolve_KeepsConfiguredRouteForUnknownNoticeTypes()
    {
        var route = NoticeNavigationResolver.Resolve("/admin/custom-notices", "Custom.Created", "Custom", 1);

        Assert.Equal("/admin/custom-notices", route);
    }
}
