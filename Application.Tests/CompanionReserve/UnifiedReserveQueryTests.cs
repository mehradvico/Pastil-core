using Application.Services.CompanionSrvs.CompanionReserveUnifiedSrv;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Xunit;

namespace Application.Tests.CompanionReserve
{
    public class UnifiedReserveQueryTests
    {
        // ترجمه‌ی UNION رزروهای خدمت + خریدهای مشاوره به SQL (بدون اتصال به دیتابیس) بررسی می‌شود
        [Fact]
        public void MergedQuery_TranslatesToUnionAllWithPaging()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=UnifiedReserveTest;Trusted_Connection=True;",
                    sql => sql.UseNetTopologySuite())
                .Options;
            using var context = new DataBaseContext(options);

            var sql = UnifiedReserveService.BuildMergedQuery(
                    "all",
                    UnifiedReserveService.ServiceRows(context.CompanionReserves.AsNoTracking()),
                    UnifiedReserveService.ConsultationRows(context.ConsultationPurchases.AsNoTracking()))
                .OrderByDescending(r => r.CreateDate)
                .ThenByDescending(r => r.Id)
                .Skip(20)
                .Take(20)
                .ToQueryString();

            Assert.Contains("UNION ALL", sql);
            Assert.Contains("OFFSET", sql);
        }
    }
}
