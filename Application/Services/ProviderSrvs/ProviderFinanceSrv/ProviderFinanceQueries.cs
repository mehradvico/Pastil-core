using Application.Services.FinanceSrvs.SettlementSrv;
using Entities.Entities;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.ProviderSrvs.ProviderFinanceSrv
{
    public sealed class GroupRow
    {
        public long Key { get; set; }
        public int Count { get; set; }
        public int UnsettledCount { get; set; }
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }
        public double SettledShare { get; set; }
        public double UnsettledShare { get; set; }
    }

    public sealed class StoreOrderRow
    {
        public long StoreId { get; set; }
        public string OrderId { get; set; }
        public double StoreShare { get; set; }
        public double SiteShare { get; set; }
        public bool Permitted { get; set; }
        public bool? UserReceived { get; set; }
    }

    public sealed class OrderSpan
    {
        public string OrderId { get; set; }
        public int StoreCount { get; set; }
        public int ProviderCount { get; set; }
    }

    public sealed class SettlementStatRow
    {
        public long ProviderId { get; set; }
        public SettlementKindEnum Kind { get; set; }
        public double Paid { get; set; }
        public int Count { get; set; }
        public DateTime? Last { get; set; }
    }

    /// <summary>
    /// کوئری‌های تجمیعی حسابداری ارائه‌دهنده. جدا شده‌اند تا ترجمه‌ی آن‌ها به SQL بدون دیتابیس تست شود (ToQueryString).
    /// منبع «چه ردیف‌هایی قابل تسویه‌اند» همیشه SettlementSources است.
    /// </summary>
    public static class ProviderFinanceQueries
    {
        public static IQueryable<GroupRow> CompanionReserveGroups(IDataBaseContext context, IReadOnlyCollection<long> companionIds)
            => SettlementSources.CompanionReserves(context, companionIds)
                .GroupBy(r => r.CompanionAssistance.CompanionId)
                .Select(g => new GroupRow
                {
                    Key = g.Key,
                    Count = g.Count(),
                    UnsettledCount = g.Count(x => !x.Permitted),
                    ProviderShare = g.Sum(x => x.CompanionShare),
                    SiteShare = g.Sum(x => x.SiteShare),
                    SettledShare = g.Sum(x => x.Permitted ? x.CompanionShare : 0d),
                    UnsettledShare = g.Sum(x => x.Permitted ? 0d : x.CompanionShare),
                });

        public static IQueryable<GroupRow> GroomerReserveGroups(IDataBaseContext context, IReadOnlyCollection<long> groomerIds)
            => SettlementSources.GroomerReserves(context, groomerIds)
                .GroupBy(r => r.GroomerId)
                .Select(g => new GroupRow
                {
                    Key = g.Key,
                    Count = g.Count(),
                    UnsettledCount = g.Count(x => !x.Permitted),
                    ProviderShare = g.Sum(x => x.ProviderShare),
                    SiteShare = g.Sum(x => x.SiteShare),
                    SettledShare = g.Sum(x => x.Permitted ? x.ProviderShare : 0d),
                    UnsettledShare = g.Sum(x => x.Permitted ? 0d : x.ProviderShare),
                });

        public static IQueryable<GroupRow> ClinicReserveGroups(IDataBaseContext context, IReadOnlyCollection<long> clinicIds)
            => SettlementSources.ClinicReserves(context, clinicIds)
                .GroupBy(r => r.ClinicId)
                .Select(g => new GroupRow
                {
                    Key = g.Key,
                    Count = g.Count(),
                    UnsettledCount = g.Count(x => !x.Permitted),
                    ProviderShare = g.Sum(x => x.ProviderShare),
                    SiteShare = g.Sum(x => x.SiteShare),
                    SettledShare = g.Sum(x => x.Permitted ? x.ProviderShare : 0d),
                    UnsettledShare = g.Sum(x => x.Permitted ? 0d : x.ProviderShare),
                });
        public static IQueryable<GroupRow> CoachReserveGroups(IDataBaseContext context, IReadOnlyCollection<long> coachIds)
            => SettlementSources.CoachReserves(context, coachIds)
                .GroupBy(r => r.CoachId)
                .Select(g => new GroupRow
                {
                    Key = g.Key,
                    Count = g.Count(),
                    UnsettledCount = g.Count(x => !x.Permitted),
                    ProviderShare = g.Sum(x => x.ProviderShare),
                    SiteShare = g.Sum(x => x.SiteShare),
                    SettledShare = g.Sum(x => x.Permitted ? x.ProviderShare : 0d),
                    UnsettledShare = g.Sum(x => x.Permitted ? 0d : x.ProviderShare),
                });

        public static IQueryable<GroupRow> PansionReserveGroups(IDataBaseContext context, IReadOnlyCollection<long> companionIds)
            => SettlementSources.PansionReserves(context, companionIds)
                .GroupBy(r => r.Pansion.CompanionId)
                .Select(g => new GroupRow
                {
                    Key = g.Key,
                    Count = g.Count(),
                    UnsettledCount = g.Count(x => !x.Permitted),
                    ProviderShare = g.Sum(x => x.CompanionShare),
                    SiteShare = g.Sum(x => x.SiteShare),
                    SettledShare = g.Sum(x => x.Permitted ? x.CompanionShare : 0d),
                    UnsettledShare = g.Sum(x => x.Permitted ? 0d : x.CompanionShare),
                });

        public static IQueryable<GroupRow> ConsultationGroups(IDataBaseContext context, IReadOnlyCollection<long> companionIds)
            => SettlementSources.ConsultationPurchases(context, companionIds)
                .GroupBy(p => p.CompanionId.Value)
                .Select(g => new GroupRow
                {
                    Key = g.Key,
                    Count = g.Count(),
                    UnsettledCount = g.Count(x => !x.Permitted),
                    ProviderShare = g.Sum(x => x.CompanionShare),
                    SiteShare = g.Sum(x => x.SiteShare),
                    SettledShare = g.Sum(x => x.Permitted ? x.CompanionShare : 0d),
                    UnsettledShare = g.Sum(x => x.Permitted ? 0d : x.CompanionShare),
                });

        public static IQueryable<GroupRow> ClinicConsultationGroups(IDataBaseContext context, IReadOnlyCollection<long> clinicIds)
            => SettlementSources.ClinicConsultationPurchases(context, clinicIds)
                .GroupBy(p => p.ClinicId.Value)
                .Select(g => new GroupRow
                {
                    Key = g.Key,
                    Count = g.Count(),
                    UnsettledCount = g.Count(x => !x.Permitted),
                    ProviderShare = g.Sum(x => x.CompanionShare),
                    SiteShare = g.Sum(x => x.SiteShare),
                    SettledShare = g.Sum(x => x.Permitted ? x.CompanionShare : 0d),
                    UnsettledShare = g.Sum(x => x.Permitted ? 0d : x.CompanionShare),
                });

        public static IQueryable<GroupRow> CoachConsultationGroups(IDataBaseContext context, IReadOnlyCollection<long> coachIds)
            => SettlementSources.CoachConsultationPurchases(context, coachIds)
                .GroupBy(p => p.CoachId.Value)
                .Select(g => new GroupRow
                {
                    Key = g.Key,
                    Count = g.Count(),
                    UnsettledCount = g.Count(x => !x.Permitted),
                    ProviderShare = g.Sum(x => x.CompanionShare),
                    SiteShare = g.Sum(x => x.SiteShare),
                    SettledShare = g.Sum(x => x.Permitted ? x.CompanionShare : 0d),
                    UnsettledShare = g.Sum(x => x.Permitted ? 0d : x.CompanionShare),
                });

        public static IQueryable<GroupRow> SchoolReserveGroups(IDataBaseContext context, IReadOnlyCollection<long> companionIds)
            => SettlementSources.SchoolReserves(context, companionIds)
                .GroupBy(r => r.SchoolCourse.School.CompanionId)
                .Select(g => new GroupRow
                {
                    Key = g.Key,
                    Count = g.Count(),
                    UnsettledCount = g.Count(x => !x.Permitted),
                    ProviderShare = g.Sum(x => x.CompanionShare),
                    SiteShare = g.Sum(x => x.SiteShare),
                    SettledShare = g.Sum(x => x.Permitted ? x.CompanionShare : 0d),
                    UnsettledShare = g.Sum(x => x.Permitted ? 0d : x.CompanionShare),
                });

        /// <summary>هر (فروشگاه، سفارش) یک‌بار؛ سهم در سطح خودِ سفارش ذخیره شده است.</summary>
        public static IQueryable<StoreOrderRow> StoreOrderRows(IDataBaseContext context, IReadOnlyCollection<long> storeIds)
            => context.ProductOrderStores
                .Where(pos => storeIds.Contains(pos.StoreId) && !pos.ProductOrder.Deleted)
                .Select(pos => new StoreOrderRow
                {
                    StoreId = pos.StoreId,
                    OrderId = pos.ProductOrderId,
                    StoreShare = pos.ProductOrder.StoreShare,
                    SiteShare = pos.ProductOrder.SiteShare,
                    Permitted = pos.ProductOrder.Permitted,
                    UserReceived = pos.ProductOrder.UserReceived,
                })
                .Distinct();

        /// <summary>هر سفارش چند فروشگاه و چند ارائه‌دهنده‌ی متفاوت دارد (فروشگاه بدون ارائه‌دهنده = -1، یک ارائه‌دهنده‌ی جدا).</summary>
        public static IQueryable<OrderSpan> OrderSpans(IDataBaseContext context, IReadOnlyCollection<string> orderIds)
            => context.ProductOrderStores
                .Where(pos => orderIds.Contains(pos.ProductOrderId))
                .GroupBy(pos => pos.ProductOrderId)
                .Select(g => new OrderSpan
                {
                    OrderId = g.Key,
                    StoreCount = g.Select(x => x.StoreId).Distinct().Count(),
                    ProviderCount = g.Select(x => x.Store.ProviderId ?? -1).Distinct().Count(),
                });

        public static IQueryable<SettlementStatRow> SettlementStats(IDataBaseContext context, IReadOnlyCollection<long> providerIds)
            => context.Settlements
                .Where(s => s.ProviderId != null && providerIds.Contains(s.ProviderId.Value))
                .GroupBy(s => new { ProviderId = s.ProviderId.Value, s.Kind })
                .Select(g => new SettlementStatRow
                {
                    ProviderId = g.Key.ProviderId,
                    Kind = g.Key.Kind,
                    Paid = g.Sum(x => x.PaidPrice),
                    Count = g.Count(),
                    Last = g.Max(x => (DateTime?)x.CreateDate),
                });
    }
}
