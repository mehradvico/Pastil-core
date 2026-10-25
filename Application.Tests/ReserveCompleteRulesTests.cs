using Application.Services.CompanionSrvs.CompanionReserveDebtSrv;
using Application.Services.ReserveCompleteSrv;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Xunit;

namespace Application.Tests;

public class ReserveCompleteRulesTests
{
    // قیمت ۱٬۰۰۰٬۰۰۰، آنلاین ۲۰۰٬۰۰۰
    private static ReserveCompleteResult Eval(ReserveCompleteDto dto) => ReserveCompleteRules.Evaluate(1_000_000, 200_000, dto);

    [Fact]
    public void NoBody_FinalIsPrice_RemainderReceivedAtVenue()
    {
        foreach (var dto in new[] { null, new ReserveCompleteDto() })
        {
            var r = Eval(dto);
            Assert.Null(r.Error);
            Assert.Equal(1_000_000, r.FinalAmount);
            Assert.Equal(800_000, r.ReceivedAtVenue);
            Assert.False(r.NotReceived);
            Assert.Equal(0, r.DebtAmount);
        }
    }

    [Fact]
    public void NotReceived_RemainderBecomesDebt()
    {
        var r = Eval(new ReserveCompleteDto { FinalAmount = 1_200_000, ReceivedAtVenue = 300_000, NotReceived = true });
        Assert.Null(r.Error);
        Assert.Equal(700_000, r.DebtAmount);
        Assert.True(r.NotReceived);

        var all = Eval(new ReserveCompleteDto { NotReceived = true });
        Assert.Equal(0, all.ReceivedAtVenue);
        Assert.Equal(800_000, all.DebtAmount);
    }

    [Fact]
    public void NotReceived_WithNothingLeft_IsNoDebt()
    {
        var r = Eval(new ReserveCompleteDto { FinalAmount = 200_000, NotReceived = true });
        Assert.Null(r.Error);
        Assert.False(r.NotReceived);
        Assert.Equal(0, r.DebtAmount);
    }

    [Fact]
    public void Received_LessThanRemainder_WithoutNotReceived_IsWaivedNotDebt()
    {
        var r = Eval(new ReserveCompleteDto { ReceivedAtVenue = 600_000 });
        Assert.Null(r.Error);
        Assert.Equal(600_000, r.ReceivedAtVenue);
        Assert.Equal(0, r.DebtAmount);
    }

    [Theory]
    [InlineData(-1d, null)]
    [InlineData(199_999d, null)]
    [InlineData(500_000d, -1d)]
    [InlineData(500_000d, 300_001d)]
    public void Invalid_Amounts_Rejected(double final, double? received)
    {
        var r = Eval(new ReserveCompleteDto { FinalAmount = final, ReceivedAtVenue = received });
        Assert.NotNull(r.Error);
    }

    [Fact]
    public void FinalBelowOnline_HasSpecificError() =>
        Assert.Equal(Resource.Notification.ReserveCompleteFinalBelowOnline, Eval(new ReserveCompleteDto { FinalAmount = 100_000 }).Error);

    [Fact]
    public void Detail_TrimmedAndLimited()
    {
        Assert.Equal("hi", Eval(new ReserveCompleteDto { Detail = "  hi " }).Detail);
        Assert.Null(Eval(new ReserveCompleteDto { Detail = new string('x', 500) }).Error);
        Assert.Equal(Resource.Notification.ReserveCompleteDetailTooLong, Eval(new ReserveCompleteDto { Detail = new string('x', 501) }).Error);
    }

    [Fact]
    public void CanComplete_OwnerAssigneeOrAdmin()
    {
        Assert.True(ReserveCompleteRules.CanComplete(null, 1, null));
        Assert.True(ReserveCompleteRules.CanComplete(1, 1, null));
        Assert.True(ReserveCompleteRules.CanComplete(2, 1, 2));
        Assert.False(ReserveCompleteRules.CanComplete(3, 1, 2));
        Assert.False(ReserveCompleteRules.CanComplete(2, 1, null));
    }

    [Fact]
    public void CollectedKey_CompanionUnchanged_OthersDistinct()
    {
        Assert.Equal(CompanionReserveDebtRules.CollectedKey(5), CompanionReserveDebtRules.CollectedKey(5, ReserveDebtKindEnum.Companion));
        Assert.NotEqual(CompanionReserveDebtRules.CollectedKey(5, ReserveDebtKindEnum.Coach), CompanionReserveDebtRules.CollectedKey(5, ReserveDebtKindEnum.Clinic));
    }

    [Fact]
    public void VenueDebtQueries_TranslateToSql()
    {
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseSqlServer("Server=(local);Database=TranslationOnly;Integrated Security=true;TrustServerCertificate=true", sql => sql.UseNetTopologySuite())
            .Options;
        using var context = new DataBaseContext(options);
        Assert.Contains("[DebtPaidDate] IS NULL", VenueReserveDebtQueries.Overdue(context.CoachReserves, 1, DateTime.Now).ToQueryString());
        Assert.Contains("[NotReceived]", VenueReserveDebtQueries.Open(context.ClinicReserves).Select(r => r.BookerId).Distinct().ToQueryString());
        Assert.Contains("[DebtAmount]", VenueReserveDebtQueries.Open(context.GroomerReserves).Where(r => r.BookerId == 1)
            .Select(r => new { r.Id, r.Groomer.Name, r.DebtAmount, r.DoneDate }).ToQueryString());
    }
}
