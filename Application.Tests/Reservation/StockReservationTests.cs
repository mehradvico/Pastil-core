using Application.Services.Order.ProductOrderSrv;
using Xunit;

namespace Application.Tests.Reservation;

public class StockReservationTests
{
    private static Dictionary<long, int> Map(params (long Id, int Value)[] rows) =>
        rows.ToDictionary(row => row.Id, row => row.Value);

    [Fact]
    public void FindShortages_ReturnsEmpty_WhenStockCoversRequestedAndHolds()
    {
        var shortages = StockReservation.FindShortages(
            requested: Map((1, 2), (2, 1)),
            quantities: Map((1, 5), (2, 3)),
            heldByOthers: Map((1, 3)));

        Assert.Empty(shortages);
    }

    [Fact]
    public void FindShortages_FlagsItem_WhenAnotherBuyerHoldsTheLastUnits()
    {
        // ۱ واحد مانده و خریدار دیگری در حال پرداخت آن است → این خریدار نباید بتواند سفارش بدهد.
        var shortages = StockReservation.FindShortages(
            requested: Map((7, 1)),
            quantities: Map((7, 1)),
            heldByOthers: Map((7, 1)));

        Assert.Equal(new long[] { 7 }, shortages);
    }

    [Fact]
    public void FindShortages_FlagsItem_WhenRequestedExceedsQuantityWithNoHolds()
    {
        var shortages = StockReservation.FindShortages(
            requested: Map((3, 4)),
            quantities: Map((3, 3)),
            heldByOthers: Map());

        Assert.Equal(new long[] { 3 }, shortages);
    }

    [Fact]
    public void FindShortages_TreatsUnknownItemAsUnavailable_AndReportsAllShortagesSorted()
    {
        var shortages = StockReservation.FindShortages(
            requested: Map((9, 1), (2, 5), (4, 1)),
            quantities: Map((2, 1), (4, 10)),
            heldByOthers: Map());

        Assert.Equal(new long[] { 2, 9 }, shortages);
    }

    [Fact]
    public void FindShortages_AllowsExactlyTheAvailableAmount()
    {
        var shortages = StockReservation.FindShortages(
            requested: Map((5, 2)),
            quantities: Map((5, 5)),
            heldByOthers: Map((5, 3)));

        Assert.Empty(shortages);
    }
}
