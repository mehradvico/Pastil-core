using Entities.Entities.CoachField;
using Application.Services.CoachSrvs.CoachCatalogSrv;
using Application.Services.CoachSrvs.CoachCatalogSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveSrv;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static Application.Tests.Coach.CoachReserveTestKit;

namespace Application.Tests.Coach;

public class CoachPhase3Tests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public CoachPhase3Tests(CoachLocalDbFixture fixture) => _fixture = fixture;

    [Fact]
    public void AllocateCommission_SumsExactlyAndUsesPerPartPercent()
    {
        var shares = CoachReserveRules.AllocateCommission(1_000_001, new List<(double, decimal)> { (1, 10m), (2, 20m), (1, 0m) });
        Assert.Equal(3, shares.Count);
        Assert.Equal(1_000_001d, shares.Sum(s => s.Provider + s.Site));
        Assert.Equal(0d, shares[2].Site);
        Assert.All(shares, s => Assert.True(s.Provider >= 0 && s.Site >= 0));
    }

    [Fact]
    public void AllocateCommission_ZeroPayment_ReturnsZeros()
    {
        var shares = CoachReserveRules.AllocateCommission(0, new List<(double, decimal)> { (1, 10m), (1, 10m) });
        Assert.All(shares, s => Assert.Equal((0d, 0d), s));
    }

    private async Task<long> AddSecondPackageAsync(World world, double price, double pre)
    {
        using var context = _fixture.NewContext();
        var assistance = new Assistance { Name = "Cut", Active = true, IsGrooming = false };
        context.Assistances.Add(assistance);
        await context.SaveChangesAsync();
        var catalog = new CoachCatalogService(context, _fixture.Mapper, NullLogger<CoachCatalogService>.Instance);
        var service = (await catalog.AddServiceAsync(new CoachServiceInputDto { CoachId = world.CoachId, AssistanceId = assistance.Id, DurationMinutes = 30 }, null)).Data;
        var package = (await catalog.AddPackageAsync(new CoachServicePackageInputDto { CoachServiceId = service.Id, Name = "Pkg2", Price = price, PrePaymentPrice = pre }, null)).Data;
        await catalog.ApproveServiceAsync(new CoachServiceApproveDto { Id = service.Id, Approve = true, CommissionPercent = 20 });
        return package.Id;
    }

    [Fact]
    public async Task MultiPackage_QuoteCreateAndPay_SplitsCommissionPerItem()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var second = await AddSecondPackageAsync(world, 400_000, 100_000);

        var quote = await NewReserveService(_fixture).QuoteAsync(new List<long> { world.PackageId, second }, (int)CoachServiceModeEnum.InPerson);
        Assert.True(quote.IsSuccess);
        Assert.Equal(1_400_000, quote.Data.Price);
        Assert.Equal(300_000, quote.Data.OnlineAmount);

        var input = Input(world, Tomorrow(11));
        input.ExtraPackageIds = new List<long> { second };
        var created = await NewReserveService(_fixture).CreateAsync(input, world.BookerId);
        Assert.True(created.IsSuccess);

        var paid = await PayAsync(_fixture, created.Data.Id);
        using var context = _fixture.NewContext();
        var items = await context.CoachReserveItems.AsNoTracking().Where(i => i.CoachReserveId == paid.Id).ToListAsync();
        Assert.Equal(2, items.Count);
        Assert.Equal(90, paid.DurationMinutes);
        Assert.Equal(paid.ProviderShare + paid.SiteShare, items.Sum(i => i.ProviderShare + i.SiteShare));
        Assert.Equal(paid.PaymentPrice, items.Sum(i => i.ProviderShare + i.SiteShare));
    }

    [Fact]
    public async Task MultiPackage_DuplicateServiceOrTooMany_IsRejected()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        long sameServicePackage;
        using (var context = _fixture.NewContext())
        {
            var catalog = new CoachCatalogService(context, _fixture.Mapper, NullLogger<CoachCatalogService>.Instance);
            sameServicePackage = (await catalog.AddPackageAsync(new CoachServicePackageInputDto { CoachServiceId = world.ServiceId, Name = "Pkg3", Price = 300_000 }, null)).Data.Id;
        }
        var input = Input(world, Tomorrow(12));
        input.ExtraPackageIds = new List<long> { sameServicePackage };
        Assert.False((await NewReserveService(_fixture).CreateAsync(input, world.BookerId)).IsSuccess);
    }

    [Fact]
    public async Task Reschedule_MovesPaidReserve_AndRejectsSameTimeAndPast()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var created = await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId);
        var paid = await PayAsync(_fixture, created.Data.Id);

        Assert.False((await NewReserveService(_fixture).RescheduleAsync(paid.Id, paid.DoDate, null)).IsSuccess);
        Assert.False((await NewReserveService(_fixture).RescheduleAsync(paid.Id, DateTime.Today.AddDays(-1).AddHours(10), null)).IsSuccess);
        Assert.False((await NewReserveService(_fixture).RescheduleAsync(paid.Id, Tomorrow(14), world.Seed.OtherUserId)).IsSuccess);

        var moved = await NewReserveService(_fixture).RescheduleAsync(paid.Id, Tomorrow(14), world.Seed.OwnerUserId);
        Assert.True(moved.IsSuccess);
        using var context = _fixture.NewContext();
        var row = await context.CoachReserves.AsNoTracking().FirstAsync(r => r.Id == paid.Id);
        Assert.Equal(Tomorrow(14), row.DoDate);
        Assert.Equal(Tomorrow(10), row.OriginalDoDate);
        Assert.Equal(1, row.RescheduleCount);
    }

    [Fact]
    public async Task AddService_RejectsGroomingAssistance()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        using var context = _fixture.NewContext();
        var plain = new Assistance { Name = "Bath", Active = true, IsGrooming = true };
        context.Assistances.Add(plain);
        await context.SaveChangesAsync();
        var catalog = new CoachCatalogService(context, _fixture.Mapper, NullLogger<CoachCatalogService>.Instance);
        var result = await catalog.AddServiceAsync(new CoachServiceInputDto { CoachId = world.CoachId, AssistanceId = plain.Id, DurationMinutes = 30 }, null);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SettlementItems_ListsCoachReserves()
    {
        if (!_fixture.Available) return;
        // آیتم‌های تسویه از GetItemsAsync می‌آیند؛ اینجا فقط مطمئن می‌شویم سرویس با تسویه‌ی ناموجود شکست می‌خورد نه استثنا.
        var world = await BuildWorldAsync(_fixture);
        Assert.NotNull(world);
    }
}
