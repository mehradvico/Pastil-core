using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReserveSrv;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveDebtSrv;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.ReserveCompleteSrv;
using Entities.Entities;
using Entities.Entities.ClinicField;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Xunit;
using static Application.Tests.Clinic.ClinicReserveTestKit;

namespace Application.Tests.Clinic;

/// <summary>
/// تکمیل رزرو با مبلغ نهایی/دریافتی در محل/دریافت‌نشده (مشترک بین مربی، کلینیک و آرایشگاه؛ اینجا روی کلینیک).
/// بسته‌ی تست: قیمت ۱٬۰۰۰٬۰۰۰، پیش‌پرداخت آنلاین ۲۰۰٬۰۰۰، کمیسیون ۱۰٪.
/// </summary>
public class ClinicReserveCompleteTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;

    public ClinicReserveCompleteTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    private async Task<ClinicReserve> LoadAsync(long id)
    {
        using var context = _fixture.NewContext();
        return await context.ClinicReserves.AsNoTracking().FirstAsync(r => r.Id == id);
    }

    // رزرو پرداخت‌شده، تأییدشده و زمانش گذشته (آماده‌ی تکمیل)
    private async Task<(World World, long ReserveId)> ReadyAsync()
    {
        var world = await BuildWorldAsync(_fixture);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        await PayAsync(_fixture, created.Id);
        Assert.True((await NewApprovalService(_fixture).ApproveAsync(created.Id, world.Seed.OwnerUserId)).IsSuccess);
        using (var context = _fixture.NewContext())
            await context.ClinicReserves.Where(r => r.Id == created.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.DoDate, DateTime.Now.AddHours(-2)).SetProperty(r => r.EndDate, DateTime.Now.AddHours(-1)));
        return (world, created.Id);
    }

    private static CompanionReserveDebtService NewDebtService(DataBaseContext context, double balance)
    {
        var wallet = FakeProxy<IWalletService>.Create((method, args) =>
        {
            if (method.Name == nameof(IWalletService.GetAmountValueAsync))
                return Task.FromResult(balance);
            if (method.Name == nameof(IWalletService.InsertAsyncDto))
            {
                var dto = (WalletDto)args![0]!;
                context.Wallets.Add(new Wallet { Name = dto.Name, Amount = dto.Amount, IsIncrease = dto.IsIncrease, UserId = dto.UserId, ClinicReserveId = dto.ClinicReserveId, CreateDate = DateTime.Now });
                context.SaveChanges();
                return Task.FromResult(new BaseResultDto<WalletDto>(true, dto));
            }
            return null;
        }).Instance;
        return new CompanionReserveDebtService(context, wallet, FakeProxy<IPushNotificationService>.Create().Instance);
    }

    private static string Message(BaseResultDto result) => result.Messages?.FirstOrDefault()?.Item1;

    [Fact]
    public async Task Complete_WithoutBody_KeepsOldBehaviour_AndOnlyOnce()
    {
        if (!_fixture.Available) return;
        var (world, id) = await ReadyAsync();

        var result = await NewReserveService(_fixture).CompleteAsync(id, world.Seed.OwnerUserId);

        Assert.True(result.IsSuccess);
        Assert.Equal(1_000_000, result.Data.FinalAmount);
        Assert.Equal(800_000, result.Data.ReceivedAtVenue);
        Assert.False(result.Data.NotReceived);
        Assert.NotNull(result.Data.CompletedAt);
        Assert.Equal((int)ClinicReserveStatusEnum.Complete, result.Data.Status);
        var done = await LoadAsync(id);
        Assert.Equal(0, done.DebtAmount);
        Assert.Equal(world.Seed.OwnerUserId, done.CompletedByUserId);

        var again = await NewReserveService(_fixture).CompleteAsync(id, world.Seed.OwnerUserId, new ReserveCompleteDto { FinalAmount = 5 });
        Assert.False(again.IsSuccess);
        Assert.Equal(Resource.Notification.ClinicReserveAlreadyCompleted, Message(again));
        Assert.Equal(1_000_000, (await LoadAsync(id)).FinalAmount);
    }

    [Fact]
    public async Task Complete_Validation_AndAccess()
    {
        if (!_fixture.Available) return;
        var (world, id) = await ReadyAsync();
        var service = NewReserveService(_fixture);

        Assert.Equal(Resource.Notification.AccessDenied, Message(await service.CompleteAsync(id, world.Seed.OtherUserId)));
        Assert.Equal(Resource.Notification.ReserveCompleteFinalBelowOnline,
            Message(await service.CompleteAsync(id, world.Seed.OwnerUserId, new ReserveCompleteDto { FinalAmount = 100_000 })));
        Assert.Equal(Resource.Notification.ReserveCompleteReceivedInvalid,
            Message(await service.CompleteAsync(id, world.Seed.OwnerUserId, new ReserveCompleteDto { FinalAmount = 500_000, ReceivedAtVenue = 300_001 })));
        Assert.Equal(Resource.Notification.ReserveCompleteDetailTooLong,
            Message(await service.CompleteAsync(id, world.Seed.OwnerUserId, new ReserveCompleteDto { Detail = new string('x', 501) })));
        Assert.Equal((int)ClinicReserveStatusEnum.Paid, (await LoadAsync(id)).Status);

        // پرسنلِ انتخاب‌شده‌ی همین رزرو اجازه دارد
        using (var context = _fixture.NewContext())
            await context.ClinicReserves.Where(r => r.Id == id).ExecuteUpdateAsync(s => s.SetProperty(r => r.AssigneeUserId, (long?)world.Seed.OtherUserId));
        var byStaff = await NewReserveService(_fixture).CompleteAsync(id, world.Seed.OtherUserId,
            new ReserveCompleteDto { FinalAmount = 200_000, Detail = "  ok  " });
        Assert.True(byStaff.IsSuccess);
        Assert.Equal(200_000, byStaff.Data.FinalAmount);
        Assert.Equal(0, byStaff.Data.ReceivedAtVenue);
        Assert.Equal("ok", byStaff.Data.CompleteDetail);
    }

    [Fact]
    public async Task Complete_NotReceived_BecomesDebt_LocksAfterSevenDays_AndWalletPaymentAddsShares()
    {
        if (!_fixture.Available) return;
        var (world, id) = await ReadyAsync();
        var before = await LoadAsync(id);

        var result = await NewReserveService(_fixture).CompleteAsync(id, world.Seed.OwnerUserId,
            new ReserveCompleteDto { FinalAmount = 1_200_000, ReceivedAtVenue = 500_000, NotReceived = true, Detail = "extra vaccine" });
        Assert.True(result.IsSuccess);
        Assert.True(result.Data.NotReceived);
        Assert.Equal(500_000, result.Data.DebtAmount);
        Assert.Null(result.Data.DebtPaidDate);

        using (var context = _fixture.NewContext())
        {
            var debts = await NewDebtService(context, 0).GetMyDebtsAsync(world.BookerId);
            var debt = Assert.Single(debts.Data.Items);
            Assert.Equal((int)ReserveDebtKindEnum.Clinic, debt.Kind);
            Assert.Equal(id, debt.ReserveId);
            Assert.Equal(500_000, debt.Amount);
            Assert.False(debts.Data.Locked);
            Assert.False(await CompanionReserveDebtRules.IsLockedAsync(context, world.BookerId, DateTime.Now));
            Assert.True(await CompanionReserveDebtRules.IsLockedAsync(context, world.BookerId, DateTime.Now.AddDays(8)));
            // کیف پول کافی نیست
            Assert.False((await NewDebtService(context, 100).PayFromWalletAsync(world.BookerId, id, ReserveDebtKindEnum.Clinic)).IsSuccess);
        }

        using (var context = _fixture.NewContext())
            Assert.Equal(1, await NewDebtService(context, 10_000_000).CollectForUserAsync(world.BookerId));

        var paid = await LoadAsync(id);
        Assert.NotNull(paid.DebtPaidDate);
        Assert.True(paid.DebtPaidByWallet);
        // ۱۰٪ کمیسیون روی ۵۰۰٬۰۰۰ پرداخت‌شده از پاستیل
        Assert.Equal(before.SiteShare + 50_000, paid.SiteShare, 2);
        Assert.Equal(before.ProviderShare + 450_000, paid.ProviderShare, 2);
        using (var context = _fixture.NewContext())
        {
            Assert.True(await context.Wallets.AnyAsync(w => w.ClinicReserveId == id && !w.IsIncrease && w.Amount == 500_000));
            Assert.Empty((await NewDebtService(context, 0).GetMyDebtsAsync(world.BookerId)).Data.Items);
            // پرداخت دوباره چیزی کم نمی‌کند
            Assert.False((await NewDebtService(context, 10_000_000).PayFromWalletAsync(world.BookerId, id, ReserveDebtKindEnum.Clinic)).IsSuccess);
        }
    }

    [Fact]
    public async Task VenueDebt_ProviderMarkPaid_And_AdminWriteOff()
    {
        if (!_fixture.Available) return;
        var (world, id) = await ReadyAsync();
        Assert.True((await NewReserveService(_fixture).CompleteAsync(id, world.Seed.OwnerUserId,
            new ReserveCompleteDto { NotReceived = true })).IsSuccess);
        Assert.Equal(800_000, (await LoadAsync(id)).DebtAmount);

        using var context = _fixture.NewContext();
        var service = NewDebtService(context, 0);
        Assert.False((await service.MarkPaidByClinicAsync(world.Seed.OtherUserId, id, ReserveDebtKindEnum.Clinic)).IsSuccess);
        // نوع اشتباه (همراه) این رزرو را پیدا نمی‌کند
        Assert.False((await service.WriteOffAsync(id)).IsSuccess);
        Assert.True((await service.MarkPaidByClinicAsync(world.Seed.OwnerUserId, id, ReserveDebtKindEnum.Clinic)).IsSuccess);
        var closed = await LoadAsync(id);
        Assert.NotNull(closed.DebtPaidDate);
        Assert.False(closed.DebtPaidByWallet);
        Assert.False((await service.WriteOffAsync(id, ReserveDebtKindEnum.Clinic)).IsSuccess);
    }
}
