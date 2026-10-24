using Application.Services.ClinicSrvs.ClinicCatalogSrv;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveItemSrv;
using Application.Services.ClinicSrvs.ClinicReserveItemSrv.Dto;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.TripSrv.TripSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static Application.Tests.Clinic.ClinicReserveTestKit;

namespace Application.Tests.Clinic;

public class ClinicReserveItemTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;

    public ClinicReserveItemTests(ClinicLocalDbFixture fixture) => _fixture = fixture;


    private ClinicReserveItemService NewService(out FakeProxy<ITripService> trip, out FakeProxy<IPushNotificationService> push)
    {
        var context = _fixture.NewContext();
        var wallet = NewWallet(_fixture, true, context).Wallet;
        trip = FakeProxy<ITripService>.Create().Proxy;
        push = FakeProxy<IPushNotificationService>.Create().Proxy;
        var tripInstance = (ITripService)(object)trip;
        var pushInstance = (IPushNotificationService)(object)push;
        return new ClinicReserveItemService(context, wallet, pushInstance, tripInstance, NullLogger<ClinicReserveItemService>.Instance);
    }

    private async Task<long> AddPackageAsync(World world, string assistanceName, double price, double pre)
    {
        using var context = _fixture.NewContext();
        var assistance = new Assistance { Name = assistanceName, Active = true, IsGrooming = false, ProviderRoleId = await AssistanceTestRoles.IdAsync(context, "Clinic") };
        context.Assistances.Add(assistance);
        await context.SaveChangesAsync();
        var catalog = new ClinicCatalogService(context, _fixture.Mapper, NullLogger<ClinicCatalogService>.Instance);
        var service = (await catalog.AddServiceAsync(new ClinicServiceInputDto { ClinicId = world.ClinicId, AssistanceId = assistance.Id, DurationMinutes = 30 }, null)).Data;
        var package = (await catalog.AddPackageAsync(new ClinicServicePackageInputDto { ClinicServiceId = service.Id, Name = "P-" + assistanceName, Price = price, PrePaymentPrice = pre }, null)).Data;
        await catalog.ApproveServiceAsync(new ClinicServiceApproveDto { Id = service.Id, Approve = true, CommissionPercent = 20 });
        return package.Id;
    }

    /// <summary>رزرو پرداخت‌شده با دو بسته: بسته‌ی اصلی (پیش‌پرداخت ۲۰۰٬۰۰۰) + بسته‌ی دوم (قیمت ۴۰۰٬۰۰۰، پیش‌پرداخت ۱۰۰٬۰۰۰).</summary>
    private async Task<(World World, long ReserveId, long SecondPackage)> PaidReserveAsync()
    {
        var world = await BuildWorldAsync(_fixture);
        var second = await AddPackageAsync(world, "Cut" + Guid.NewGuid().ToString("N")[..6], 400_000, 100_000);
        var input = Input(world, Tomorrow(10));
        input.ExtraPackageIds = new List<long> { second };
        var created = await NewReserveService(_fixture).CreateAsync(input, world.BookerId);
        Assert.True(created.IsSuccess);
        await PayAsync(_fixture, created.Data.Id);
        return (world, created.Data.Id, second);
    }

    [Fact]
    public async Task Cancel_OneOfTwo_RefundsItsShare_KeepsReserve_AndRecomputesShares()
    {
        if (!_fixture.Available) return;
        var (world, reserveId, second) = await PaidReserveAsync();
        var service = NewService(out var trip, out _);

        var result = await service.SetStatusAsync(reserveId, second, new ClinicReserveItemStatusDto { Status = (int)ClinicReserveItemStatusEnum.Cancelled, Reason = "x" }, world.Seed.OwnerUserId, false);
        Assert.True(result.IsSuccess);
        Assert.False(result.Data.ReserveCancelled);
        // پرداخت ۳۰۰٬۰۰۰ = ۲۰۰ + ۱۰۰ → سهم بسته‌ی دوم ۱۰۰٬۰۰۰
        Assert.Equal(100_000, result.Data.RefundAmount);

        using var context = _fixture.NewContext();
        var reserve = await context.ClinicReserves.AsNoTracking().Include(r => r.Items).FirstAsync(r => r.Id == reserveId);
        Assert.False(reserve.IsCancel);
        Assert.Equal(200_000, reserve.PaymentPrice);
        Assert.Equal(1_000_000, reserve.Price);
        Assert.Equal(200_000, reserve.OnlineAmount);
        Assert.Equal(reserve.PaymentPrice, reserve.ProviderShare + reserve.SiteShare);
        var cancelled = reserve.Items.Single(i => i.Status == (int)ClinicReserveItemStatusEnum.Cancelled);
        Assert.Equal(0, cancelled.ProviderShare + cancelled.SiteShare);
        Assert.Equal(reserve.PaymentPrice, reserve.Items.Where(i => i.Status != (int)ClinicReserveItemStatusEnum.Cancelled).Sum(i => i.ProviderShare + i.SiteShare));

        var credit = await context.Wallets.AsNoTracking().Where(w => w.Name == "ClinicPackageRefund:" + cancelled.Id).ToListAsync();
        Assert.Single(credit);
        Assert.Equal(100_000, credit[0].Amount);
        Assert.DoesNotContain("CancelLinkedTripForClinicReserveAsync", trip.Calls);
    }

    [Fact]
    public async Task Cancel_LastItem_CancelsReserve_RefundsRemainder_AndCancelsLinkedTrip()
    {
        if (!_fixture.Available) return;
        var (world, reserveId, second) = await PaidReserveAsync();
        var service = NewService(out var trip, out _);
        var cancel = new ClinicReserveItemStatusDto { Status = (int)ClinicReserveItemStatusEnum.Cancelled, Reason = "x" };

        Assert.True((await service.SetStatusAsync(reserveId, second, cancel, world.Seed.OwnerUserId, false)).IsSuccess);
        var last = await service.SetStatusAsync(reserveId, world.PackageId, cancel, world.Seed.OwnerUserId, false);
        Assert.True(last.IsSuccess);
        Assert.True(last.Data.ReserveCancelled);
        Assert.Equal(200_000, last.Data.RefundAmount);
        Assert.Contains("CancelLinkedTripForClinicReserveAsync", trip.Calls);

        using var context = _fixture.NewContext();
        var reserve = await context.ClinicReserves.AsNoTracking().FirstAsync(r => r.Id == reserveId);
        Assert.True(reserve.IsCancel);
        Assert.Equal(0, reserve.PaymentPrice);
        Assert.Equal(0, reserve.ProviderShare + reserve.SiteShare);
        var ledgerNames = (await context.ClinicReserveItems.AsNoTracking().Where(i => i.ClinicReserveId == reserveId).Select(i => i.Id).ToListAsync())
            .Select(id => "ClinicPackageRefund:" + id).ToList();
        Assert.Equal(300_000, await context.Wallets.AsNoTracking().Where(w => ledgerNames.Contains(w.Name)).SumAsync(w => w.Amount));

        // دوباره لغو/تأیید: رد می‌شود
        Assert.False((await service.SetStatusAsync(reserveId, world.PackageId, cancel, world.Seed.OwnerUserId, false)).IsSuccess);
    }

    [Fact]
    public async Task Status_RequiresReasonValidTransitionAndPermission()
    {
        if (!_fixture.Available) return;
        var (world, reserveId, second) = await PaidReserveAsync();
        var service = NewService(out _, out _);

        Assert.False((await service.SetStatusAsync(reserveId, second, new ClinicReserveItemStatusDto { Status = (int)ClinicReserveItemStatusEnum.Cancelled }, world.Seed.OwnerUserId, false)).IsSuccess); // بدون دلیل
        Assert.False((await service.SetStatusAsync(reserveId, second, new ClinicReserveItemStatusDto { Status = 1 }, world.Seed.OwnerUserId, false)).IsSuccess); // هدف نامعتبر
        Assert.False((await service.SetStatusAsync(reserveId, second, new ClinicReserveItemStatusDto { Status = (int)ClinicReserveItemStatusEnum.Approved }, world.Seed.OtherUserId, false)).IsSuccess); // کاربر غریبه
        Assert.False((await service.SetStatusAsync(reserveId, second, new ClinicReserveItemStatusDto { Status = (int)ClinicReserveItemStatusEnum.Approved }, world.BookerId, false)).IsSuccess); // خود مشتری مدیر نیست
        Assert.True((await service.SetStatusAsync(reserveId, second, new ClinicReserveItemStatusDto { Status = (int)ClinicReserveItemStatusEnum.Approved }, world.Seed.OwnerUserId, false)).IsSuccess);
        Assert.False((await service.SetStatusAsync(reserveId, second, new ClinicReserveItemStatusDto { Status = (int)ClinicReserveItemStatusEnum.Approved }, world.Seed.OwnerUserId, false)).IsSuccess); // تأیید دوباره
        // ادمین می‌تواند لغو کند
        Assert.True((await service.SetStatusAsync(reserveId, second, new ClinicReserveItemStatusDto { Status = (int)ClinicReserveItemStatusEnum.Cancelled, Reason = "x" }, 0, true)).IsSuccess);

        // کاربر فقط فهرست رزرو خودش را می‌بیند
        Assert.True((await service.GetForBookerAsync(reserveId, world.BookerId)).IsSuccess);
        Assert.False((await service.GetForBookerAsync(reserveId, world.Seed.OtherUserId)).IsSuccess);
    }

    [Fact]
    public async Task Add_AfterPayment_IsPayAtVenue_AndRejectsDuplicatesAndUnpaid()
    {
        if (!_fixture.Available) return;
        var (world, reserveId, _) = await PaidReserveAsync();
        var third = await AddPackageAsync(world, "Scan" + Guid.NewGuid().ToString("N")[..6], 150_000, 50_000);
        var service = NewService(out _, out _);

        var added = await service.AddAsync(reserveId, new ClinicReserveItemAddDto { PackageId = third }, world.Seed.OwnerUserId, false);
        Assert.True(added.IsSuccess);
        var item = added.Data.Items.Single(i => i.PackageId == third);
        Assert.True(item.AddedAfterPayment);
        Assert.Equal(0, item.OnlineAmount);

        using (var context = _fixture.NewContext())
        {
            var reserve = await context.ClinicReserves.AsNoTracking().FirstAsync(r => r.Id == reserveId);
            Assert.Equal(1_550_000, reserve.Price);        // ۱٬۰۰۰٬۰۰۰ + ۴۰۰٬۰۰۰ + ۱۵۰٬۰۰۰
            Assert.Equal(300_000, reserve.OnlineAmount);   // آنلاین تغییر نمی‌کند
            Assert.Equal(300_000, reserve.PaymentPrice);
        }

        // همان خدمت دوباره، یا پکیج کلینیک دیگر
        Assert.False((await service.AddAsync(reserveId, new ClinicReserveItemAddDto { PackageId = third }, world.Seed.OwnerUserId, false)).IsSuccess);
        Assert.False((await service.AddAsync(reserveId, new ClinicReserveItemAddDto { PackageId = third }, world.Seed.OtherUserId, false)).IsSuccess);

        // رزرو پرداخت‌نشده
        var unpaid = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(15)), world.BookerId)).Data;
        Assert.False((await service.AddAsync(unpaid.Id, new ClinicReserveItemAddDto { PackageId = third }, world.Seed.OwnerUserId, false)).IsSuccess);
    }

    [Theory]
    [InlineData(300_000, 200_000, 300_000, false, 200_000)]
    [InlineData(300_000, 100_000, 300_000, false, 100_000)]
    [InlineData(100_000, 100_000, 100_000, true, 100_000)]
    [InlineData(0, 100_000, 300_000, false, 0)]
    [InlineData(300_000, 0, 300_000, false, 0)]
    public void RefundShare_IsProportionalAndNeverExceedsRemaining(double remaining, double weight, double total, bool last, double expected) =>
        Assert.Equal(expected, ClinicReserveItemRules.RefundShare(remaining, weight, total, last));
}
