using Application.Services.FinanceSrvs.SettlementSrv;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Xunit;

namespace Application.Tests;

/// <summary>رزرو پانسیون فقط وقتی وارد استخر تسویه می‌شود که «تکمیل‌شده» و تأییدشده (یا بدون نیاز به تأیید) باشد.</summary>
public class PansionSettlementSourceTests
{
    [Fact]
    public void PansionReserves_RequireCompleteStatusAndApprovedOrNotRequiredDecision()
    {
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseSqlServer("Server=(local);Database=TranslationOnly;Integrated Security=true;TrustServerCertificate=true", sql => sql.UseNetTopologySuite())
            .Options;
        using var context = new DataBaseContext(options);

        var sql = SettlementSources.PansionReserves(context, new long[] { 1, 2 }).ToQueryString();

        Assert.Contains("StatusId", sql);
        Assert.Contains("OwnerDecision", sql);
    }
}
