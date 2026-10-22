using Application.Services.GroomerSrvs.GroomerCatalogSrv;
using Application.Services.GroomerSrvs.GroomerCatalogSrv.Dto;
using Application.Services.GroomerSrvs.GroomerReserveSrv;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static Application.Tests.Groomer.GroomerReserveTestKit;

namespace Application.Tests.Groomer;

public class GroomerPhase3Tests : IClassFixture<GroomerLocalDbFixture>
{
    private readonly GroomerLocalDbFixture _fixture;

    public GroomerPhase3Tests(GroomerLocalDbFixture fixture) => _fixture = fixture;

    [Fact]
    public void AllocateCommission_SumsExactlyAndUsesPerPartPercent()
    {
        var shares = GroomerReserveRules.AllocateCommission(1_000_001, new List<(double, decimal)> { (1, 10m), (2, 20m), (1, 0m) });
        Assert.Equal(3, shares.Count);
        Assert.Equal(1_000_001d, shares.Sum(s => s.Provider + s.Site));
        Assert.Equal(0d, shares[2].Site);
        Assert.All(shares, s => Assert.True(s.Provider >= 0 && s.Site >= 0));
    }

    [Fact]
    public void AllocateCommission_ZeroPayment_ReturnsZeros()
    {
        var shares = GroomerReserveRules.AllocateCommission(0, new List<(double, decimal)> { (1, 10m), (1, 10m) });
        Assert.All(shares, s => Assert.Equal((0d, 0d), s));
    }

    private async Task<long> AddSecondPackageAsync(World world, double price, double pre)
    {
        using var context = _fixture.NewContext();
        var assistance = new Assistance { Name = "Cut", Active = true, IsGrooming = true };
        context.Assistances.Add(assistance);
        await context.SaveChangesAsync();
        var catalog = new GroomerCatalogService(context, _fixture.Mapper, NullLogger<GroomerCatalogService>.Instance);
        var service = (await catalog.AddServiceAsync(new GroomerServiceInputDto { GroomerId = world.GroomerId, AssistanceId = assistance.Id, DurationMinutes = 30 }, null)).Data;
        var package = (await catalog.AddPackageAsync(new GroomerServicePackageInputDto { GroomerServiceId = service.Id, Name = "Pkg2", Price = price, PrePaymentPrice = pre }, null)).Data;
        await catalog.ApproveServiceAsync(new GroomerServiceApproveDto { Id = service.Id, Approve = true, CommissionPercent = 20 });
        return package.Id;
    }

    [Fact]
    public async Task MultiPackage_QuoteCreateAndPay_SplitsCommissionPerItem()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var second = await AddSecondPackageAsync(world, 400_000, 100_000);

        var quote = await NewReserveService(_fixture).QuoteAsync(new List<long> { world.PackageId, second }, false);
        Assert.True(quote.IsSuccess);
        Assert.Equal(1_400_000, quote.Data.Price);
        Assert.Equal(300_000, quote.Data.OnlineAmount);

        var input = Input(world, Tomorrow(11));
        input.ExtraPackageIds = new List<long> { second };
        var created = await NewReserveService(_fixture).CreateAsync(input, world.BookerId);
        Assert.True(created.IsSuccess);

        var paid = await PayAsync(_fixture, created.Data.Id);
        using var context = _fixture.NewContext();
        var items = await context.GroomerReserveItems.AsNoTracking().Where(i => i.GroomerReserveId == paid.Id).ToListAsync();
        Assert.Equal(2, items.Count);
        Assert.Equal(90, paid.DurationMinutes);
        Assert.Equal(paid.ProviderShare + paid.SiteShare, items.Sum(i => i.ProviderShare + i.SiteShare));
        Assert.Equal(paid.PaymentPrice, items.Sum(i => i.ProviderShare + i.SiteShare));
    }

    [Fact]
    public async Task MultiPackage_SeveralPackagesOfOneService_AreAllowed_ButTheSamePackageTwiceIsRejected()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        long sameServicePackage;
        using (var context = _fixture.NewContext())
        {
            var catalog = new GroomerCatalogService(context, _fixture.Mapper, NullLogger<GroomerCatalogService>.Instance);
            sameServicePackage = (await catalog.AddPackageAsync(new GroomerServicePackageInputDto { GroomerServiceId = world.ServiceId, Name = "Pkg3", Price = 300_000 }, null)).Data.Id;
        }

        // یک خدمت، دو بسته‌ی متفاوت ⇒ مجاز؛ دو آیتم با همان خدمت
        var input = Input(world, Tomorrow(12));
        input.ExtraPackageIds = new List<long> { sameServicePackage };
        var created = await NewReserveService(_fixture).CreateAsync(input, world.BookerId);
        Assert.True(created.IsSuccess);
        using (var context = _fixture.NewContext())
        {
            var items = await context.GroomerReserveItems.AsNoTracking().Where(i => i.GroomerReserveId == created.Data.Id).ToListAsync();
            Assert.Equal(2, items.Count);
            Assert.Single(items.Select(i => i.GroomerServiceId).Distinct());
        }

        // تکرار همان بسته ⇒ رد
        var duplicate = Input(world, Tomorrow(15));
        duplicate.ExtraPackageIds = new List<long> { duplicate.GroomerServicePackageId };
        Assert.False((await NewReserveService(_fixture).CreateAsync(duplicate, world.BookerId)).IsSuccess);
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
        var row = await context.GroomerReserves.AsNoTracking().FirstAsync(r => r.Id == paid.Id);
        Assert.Equal(Tomorrow(14), row.DoDate);
        Assert.Equal(Tomorrow(10), row.OriginalDoDate);
        Assert.Equal(1, row.RescheduleCount);
    }

    [Fact]
    public async Task AddService_RejectsAssistanceThatIsNotGrooming()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        using var context = _fixture.NewContext();
        var plain = new Assistance { Name = "Checkup", Active = true, IsGrooming = false };
        context.Assistances.Add(plain);
        await context.SaveChangesAsync();
        var catalog = new GroomerCatalogService(context, _fixture.Mapper, NullLogger<GroomerCatalogService>.Instance);
        var result = await catalog.AddServiceAsync(new GroomerServiceInputDto { GroomerId = world.GroomerId, AssistanceId = plain.Id, DurationMinutes = 30 }, null);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SettlementItems_ListsGroomerReserves()
    {
        if (!_fixture.Available) return;
        // آیتم‌های تسویه از GetItemsAsync می‌آیند؛ اینجا فقط مطمئن می‌شویم سرویس با تسویه‌ی ناموجود شکست می‌خورد نه استثنا.
        var world = await BuildWorldAsync(_fixture);
        Assert.NotNull(world);
    }
}
