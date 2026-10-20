using Application.Services.Order.ShippingSrv;
using Entities.Entities.ShippingField;
using Xunit;

namespace Application.Tests;

public class ShipmentDeadlineRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 17, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(ShipmentStatusEnum.AwaitingSellerConfirm, null, true)]
    [InlineData(ShipmentStatusEnum.AwaitingSellerConfirm, "HTTP 403: deadline_too_soon", true)]
    [InlineData(ShipmentStatusEnum.Preparing, null, true)]
    [InlineData(ShipmentStatusEnum.Failed, "SellerConfirmExpired", true)]
    [InlineData(ShipmentStatusEnum.Failed, "Miare says no", false)]
    [InlineData(ShipmentStatusEnum.Failed, null, false)]
    [InlineData(ShipmentStatusEnum.Requested, null, false)]
    [InlineData(ShipmentStatusEnum.Accepted, null, false)]
    [InlineData(ShipmentStatusEnum.PickedUp, null, false)]
    [InlineData(ShipmentStatusEnum.Delivered, null, false)]
    [InlineData(ShipmentStatusEnum.Cancelled, null, false)]
    public void CanExtend(ShipmentStatusEnum status, string reason, bool expected)
    {
        Assert.Equal(expected, ShipmentDeadlineRules.CanExtend(status, reason));
    }

    [Theory]
    [InlineData(14, false)]
    [InlineData(15, true)]
    [InlineData(60, true)]
    [InlineData(480, true)]
    [InlineData(481, false)]
    [InlineData(0, false)]
    [InlineData(-30, false)]
    public void MinutesRange(int minutes, bool valid)
    {
        Assert.Equal(valid, ShipmentDeadlineRules.IsValidMinutes(minutes));
    }

    [Fact]
    public void NewDeadline_ExtendsALiveDeadline_ButRestartsFromNowWhenItAlreadyPassed()
    {
        Assert.Equal(Now.AddMinutes(30 + 60), ShipmentDeadlineRules.NewDeadline(Now.AddMinutes(30), Now, 60));
        Assert.Equal(Now.AddMinutes(60), ShipmentDeadlineRules.NewDeadline(Now.AddMinutes(-90), Now, 60));
        Assert.Equal(Now.AddMinutes(60), ShipmentDeadlineRules.NewDeadline(null, Now, 60));
    }
}
