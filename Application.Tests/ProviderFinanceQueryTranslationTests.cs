using Application.Services.AgencySrvs.AgencySrv;
using Application.Services.AgencySrvs.AgencySrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv;
using Application.Services.ProviderSrvs.ProviderFinanceSrv;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Xunit;

namespace Application.Tests;

/// <summary>
/// بدون دیتابیس: ToQueryString مدل EF را می‌سازد و کوئری را به SQL ترجمه می‌کند؛ اگر عبارتی قابل ترجمه نباشد
/// (مثلاً Distinct/GroupBy اشتباه) همین‌جا InvalidOperationException می‌دهد، نه در محیط واقعی.
/// </summary>
public class ProviderFinanceQueryTranslationTests
{
    private static DataBaseContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseSqlServer("Server=(local);Database=TranslationOnly;Integrated Security=true;TrustServerCertificate=true", sql => sql.UseNetTopologySuite())
            .Options;
        return new DataBaseContext(options);
    }

    private static readonly long[] Ids = { 1, 2, 3 };
    private static readonly string[] OrderIds = { "a", "b" };

    [Fact]
    public void CompanionGroupQueries_TranslateToSql()
    {
        using var context = CreateContext();
        Assert.Contains("GROUP BY", ProviderFinanceQueries.CompanionReserveGroups(context, Ids).ToQueryString());
        Assert.Contains("GROUP BY", ProviderFinanceQueries.PansionReserveGroups(context, Ids).ToQueryString());
        Assert.Contains("GROUP BY", ProviderFinanceQueries.ConsultationGroups(context, Ids).ToQueryString());
        Assert.Contains("GROUP BY", ProviderFinanceQueries.SchoolReserveGroups(context, Ids).ToQueryString());
    }

    [Fact]
    public void StoreAndSettlementQueries_TranslateToSql()
    {
        using var context = CreateContext();
        Assert.Contains("DISTINCT", ProviderFinanceQueries.StoreOrderRows(context, Ids).ToQueryString());
        Assert.Contains("GROUP BY", ProviderFinanceQueries.OrderSpans(context, OrderIds).ToQueryString());
        Assert.Contains("GROUP BY", ProviderFinanceQueries.SettlementStats(context, Ids).ToQueryString());
    }

    [Fact]
    public void SettlementSources_TranslateToSql()
    {
        using var context = CreateContext();
        Assert.NotEmpty(SettlementSources.CompanionReserves(context, Ids).Where(r => !r.Permitted).ToQueryString());
        Assert.NotEmpty(SettlementSources.PansionReserves(context, Ids).Where(r => !r.Permitted).ToQueryString());
        Assert.NotEmpty(SettlementSources.ConsultationPurchases(context, Ids).Where(p => !p.Permitted).ToQueryString());
        Assert.NotEmpty(SettlementSources.SchoolReserves(context, Ids).Where(r => !r.Permitted).ToQueryString());
        Assert.NotEmpty(SettlementSources.StoreOrders(context, Ids).Where(o => !o.Permitted).ToQueryString());
    }

    [Fact]
    public void AgencyQueries_TranslateToSql()
    {
        using var context = CreateContext();
        var input = new AgencyInputDto { Q = "x", Available = true, Approved = true, ProviderRoleId = 1, ProviderId = 2 };
        Assert.Contains("UNION ALL", AgencyQueries.Combined(context, input).ToQueryString());
        Assert.NotEmpty(AgencyQueries.Companions(context, new AgencyInputDto { IsPersonal = true, GoldAccount = true, SilverAccount = true }).ToQueryString());
        Assert.NotEmpty(AgencyQueries.Stores(context, input).ToQueryString());
        // فیلتر مخصوص نمایندگی، فروشگاه‌ها را حذف می‌کند
        Assert.DoesNotContain("UNION ALL", AgencyQueries.Combined(context, new AgencyInputDto { IsPersonal = false, Kind = AgencyKindEnum.Companion }).ToQueryString());
        foreach (var sort in new[] { Application.Common.Enumerable.SortEnum.New, Application.Common.Enumerable.SortEnum.Old, Application.Common.Enumerable.SortEnum.MorePriority })
            Assert.Contains("OFFSET", AgencyQueries.Order(AgencyQueries.Combined(context, input), sort, DateTime.Now).Skip(20).Take(20).ToQueryString());
        var kinds = new[] { AgencyKindEnum.Companion, AgencyKindEnum.Store, AgencyKindEnum.Pansion, AgencyKindEnum.School, AgencyKindEnum.Groomer };
        Assert.NotEmpty(AgencyQueries.Groomers(context, input).ToQueryString());
        // فیلتر مخصوص نمایندگی، آرایشگاه را حذف نمی‌کند ولی فروشگاه/پانسیون/مدرسه را حذف می‌کند
        Assert.Contains("UNION ALL", AgencyQueries.Combined(context, new AgencyInputDto { IsPersonal = false }).ToQueryString());
        Assert.NotEmpty(AgencyQueries.Pansions(context, input).ToQueryString());
        Assert.NotEmpty(AgencyQueries.Schools(context, input).ToQueryString());
        Assert.Contains("UNION ALL", AgencyQueries.Combined(context, new AgencyInputDto()).ToQueryString());
        foreach (var kind in kinds)
            Assert.NotEmpty(AgencyQueries.Combined(context, new AgencyInputDto { Kind = kind }).ToQueryString());
    }
}
