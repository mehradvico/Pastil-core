using Application.Common.Enumerable.Code;
using Application.Services.Order.RebateSrv;
using Entities.Entities;
using Entities.Entities.PastilClubField;
using Xunit;

namespace Application.Tests;

public class RebateTargetTests
{
    private static Cart CartOf(long storeId) => new()
    {
        CartStores = new List<CartStore> { new() { StoreId = storeId, Active = true, CartItems = new List<CartItem>() } }
    };

    [Fact]
    public void StoreScopedCode_OnlyWorksForThatStore()
    {
        var rebate = new Rebate { TargetType = ClubRewardTargetTypeEnum.Store, TargetId = 5 };
        Assert.True(RebateService.IsClubTargetValid(rebate, CartOf(5)));
        Assert.False(RebateService.IsClubTargetValid(rebate, CartOf(6)));
        Assert.True(RebateService.IsClubTargetValid(new Rebate(), CartOf(6)));
    }

    [Fact]
    public void TargetMustMatchRebateType()
    {
        Assert.True(RebateService.TargetMatchesType(ClubRewardTargetTypeEnum.Store, RebateTypeLabels.Cart));
        Assert.True(RebateService.TargetMatchesType(ClubRewardTargetTypeEnum.Coach, RebateTypeLabels.CoachReserve));
        Assert.False(RebateService.TargetMatchesType(ClubRewardTargetTypeEnum.Store, RebateTypeLabels.CoachReserve));
        Assert.False(RebateService.TargetMatchesType(ClubRewardTargetTypeEnum.Coach, RebateTypeLabels.Cargo));
    }
}
