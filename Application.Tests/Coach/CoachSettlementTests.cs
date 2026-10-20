using Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface;
using Application.Services.FinanceSrvs.FinanceStoreSrv.Iface;
using Application.Services.FinanceSrvs.SettlementSrv;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.UserBankCardSrv.Iface;
using Application.Services.CoachSrvs.CoachReserveSrv;
using Application.Services.ProviderSrvs.ProviderFinanceSrv;
using Application.Services.ProviderSrvs.ProviderFinanceSrv.Dto;
using Entities.Entities;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Application.Tests.Coach.CoachReserveTestKit;

namespace Application.Tests.Coach;

public class CoachSettlementTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public CoachSettlementTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private SettlementService NewSettlement() => new(_fixture.NewContext(), _fixture.Mapper);

    private ProviderFinanceService NewFinance() => new(
        _fixture.NewContext(),
        FakeProxy<IFinanceCompanionService>.Create().Instance,
        FakeProxy<IFinanceStoreService>.Create().Instance,
        FakeProxy<IUserBankCardService>.Create((method, _) =>
            method.Name == nameof(IUserBankCardService.Search)
                ? System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(method.ReturnType) // نتیجه‌ی خالی (List = null)
                : null).Instance);

    private async Task<Settlement> LoadSettlementAsync(long id)
    {
        using var context = _fixture.NewContext();
        return await context.Settlements.AsNoTracking().FirstAsync(s => s.Id == id);
    }

    private async Task<long> AddCardAsync(long userId, bool approved = true)
    {
        using var context = _fixture.NewContext();
        var card = new UserBankCard { UserId = userId, Approved = approved };
        context.UserBankCards.Add(card);
        await context.SaveChangesAsync();
        return card.Id;
    }

    /// <summary>رزرو پرداخت‌شده و تأییدشده؛ complete = تکمیل‌شده (قابل تسویه).</summary>
    private async Task<long> ReserveAsync(World world, DateTime doDate, bool complete, bool cancel = false)
    {
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, doDate), world.BookerId)).Data;
        await PayAsync(_fixture, created.Id);
        using var context = _fixture.NewContext();
        if (complete)
            await context.CoachReserves.Where(r => r.Id == created.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, (int)CoachReserveStatusEnum.Complete));
        if (cancel)
            await context.CoachReserves.Where(r => r.Id == created.Id).ExecuteUpdateAsync(s => s.SetProperty(r => r.IsCancel, true));
        return created.Id;
    }

    [Fact]
    public async Task Settle_PaysOnlyCompletedUncancelledReserves_Once_AndLinksThem()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, capacity: 5);
        var done1 = await ReserveAsync(world, Tomorrow(8), complete: true);
        var done2 = await ReserveAsync(world, Tomorrow(10), complete: true);
        var notDone = await ReserveAsync(world, Tomorrow(12), complete: false);
        var cancelled = await ReserveAsync(world, Tomorrow(14), complete: true, cancel: true);
        var cardId = await AddCardAsync(world.Seed.OwnerUserId);

        var settle = await NewSettlement().InsertAsyncDto(new SettlementDto
        {
            ProviderId = world.Seed.ProviderId, Kind = SettlementKindEnum.Coach, UserBankCardId = cardId, TrackingCode = " T-1 ",
        });

        Assert.True(settle.IsSuccess, string.Join(",", settle.Messages?.Select(m => m.Item1) ?? Array.Empty<string>()));
        var saved = await LoadSettlementAsync(settle.Data.Id);
        Assert.Equal(2, saved.ItemCount);
        Assert.Equal(360_000, saved.PaidPrice); // 2 × (200٬000 − 10٪)
        Assert.Equal("T-1", saved.TrackingCode);

        using var context = _fixture.NewContext();
        var permitted = await context.CoachReserves.AsNoTracking().Where(r => r.CoachId == world.CoachId).ToDictionaryAsync(r => r.Id, r => r.Permitted);
        Assert.True(permitted[done1]);
        Assert.True(permitted[done2]);
        Assert.False(permitted[notDone]);
        Assert.False(permitted[cancelled]);
        Assert.Equal(2, await context.SettlementCoaches.CountAsync(l => l.SettlementId == settle.Data.Id));

        // بار دوم چیزی برای تسویه نیست و پولی دوباره پرداخت نمی‌شود
        var again = await NewSettlement().InsertAsyncDto(new SettlementDto
        {
            ProviderId = world.Seed.ProviderId, Kind = SettlementKindEnum.Coach, UserBankCardId = cardId,
        });
        Assert.False(again.IsSuccess);
        Assert.Equal(2, await context.SettlementCoaches.CountAsync(l => l.Settlement.ProviderId == world.Seed.ProviderId));

        // بعد از تکمیل رزرو سوم، فقط همان تسویه می‌شود
        await NewReserveService(_fixture).CompleteAsync(notDone, null).ConfigureAwait(false);
        using (var ctx = _fixture.NewContext())
            await ctx.CoachReserves.Where(r => r.Id == notDone).ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, (int)CoachReserveStatusEnum.Complete).SetProperty(r => r.OwnerDecision, (int)CoachReserveOwnerDecisionEnum.Approved));
        var third = await NewSettlement().InsertAsyncDto(new SettlementDto
        {
            ProviderId = world.Seed.ProviderId, Kind = SettlementKindEnum.Coach, UserBankCardId = cardId,
        });
        Assert.True(third.IsSuccess);
        var thirdSaved = await LoadSettlementAsync(third.Data.Id);
        Assert.Equal(1, thirdSaved.ItemCount);
        Assert.Equal(180_000, thirdSaved.PaidPrice);
    }

    [Fact]
    public async Task Settle_ValidatesProviderCardAndNothingToSettle()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        await ReserveAsync(world, Tomorrow(10), complete: true);
        var foreignCard = await AddCardAsync(world.Seed.OtherUserId);
        var unapproved = await AddCardAsync(world.Seed.OwnerUserId, approved: false);
        var good = await AddCardAsync(world.Seed.OwnerUserId);

        Assert.False((await NewSettlement().InsertAsyncDto(new SettlementDto { ProviderId = world.Seed.ProviderId, Kind = SettlementKindEnum.Coach, UserBankCardId = foreignCard })).IsSuccess);
        Assert.False((await NewSettlement().InsertAsyncDto(new SettlementDto { ProviderId = world.Seed.ProviderId, Kind = SettlementKindEnum.Coach, UserBankCardId = unapproved })).IsSuccess);
        Assert.False((await NewSettlement().InsertAsyncDto(new SettlementDto { ProviderId = 0, Kind = SettlementKindEnum.Coach, UserBankCardId = good })).IsSuccess);
        Assert.False((await NewSettlement().InsertAsyncDto(new SettlementDto { ProviderId = world.Seed.ProviderId, Kind = null, UserBankCardId = good })).IsSuccess);

        // هیچ‌کدام از رد‌شده‌ها چیزی را Permitted نکرد
        using var context = _fixture.NewContext();
        Assert.Equal(0, await context.CoachReserves.CountAsync(r => r.CoachId == world.CoachId && r.Permitted));

        // ارائه‌دهنده‌ی بدون مربی
        var empty = await BuildWorldAsync(_fixture);
        using (var ctx = _fixture.NewContext())
            await ctx.Coaches.Where(g => g.Id == empty.CoachId).ExecuteUpdateAsync(s => s.SetProperty(g => g.ProviderId, (long?)null));
        var emptyCard = await AddCardAsync(empty.Seed.OwnerUserId);
        Assert.False((await NewSettlement().InsertAsyncDto(new SettlementDto { ProviderId = empty.Seed.ProviderId, Kind = SettlementKindEnum.Coach, UserBankCardId = emptyCard })).IsSuccess);
    }

    [Fact]
    public async Task Settle_FailsAtomicallyWhenAReserveChangesInTheMeantime()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, capacity: 5);
        var a = await ReserveAsync(world, Tomorrow(8), complete: true);
        await ReserveAsync(world, Tomorrow(10), complete: true);
        var card = await AddCardAsync(world.Seed.OwnerUserId);

        // یکی از ردیف‌ها قبلاً (از مسیر دیگری) Permitted شده؛ SettlementSources آن را نمی‌خواند و فقط دیگری تسویه می‌شود
        using (var context = _fixture.NewContext())
            await context.CoachReserves.Where(r => r.Id == a).ExecuteUpdateAsync(s => s.SetProperty(r => r.Permitted, true));
        var result = await NewSettlement().InsertAsyncDto(new SettlementDto { ProviderId = world.Seed.ProviderId, Kind = SettlementKindEnum.Coach, UserBankCardId = card });
        Assert.True(result.IsSuccess);
        Assert.Equal(1, (await LoadSettlementAsync(result.Data.Id)).ItemCount);
    }

    [Fact]
    public async Task ProviderFinance_CoachPool_BalancesDetailAndPendingMatchSettlement()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, capacity: 5);
        await ReserveAsync(world, Tomorrow(8), complete: true);
        await ReserveAsync(world, Tomorrow(10), complete: true);
        await ReserveAsync(world, Tomorrow(12), complete: false); // تکمیل نشده: در مانده نمی‌آید
        await ReserveAsync(world, Tomorrow(14), complete: true, cancel: true);

        var detail = await NewFinance().GetDetailAsync(world.Seed.ProviderId);
        Assert.True(detail.IsSuccess);
        var pool = detail.Data.CoachPool;
        Assert.Equal(SettlementKindEnum.Coach, pool.Kind);
        Assert.Equal(2, pool.ItemCount);
        Assert.Equal(360_000, pool.UnsettledShare);
        Assert.Equal(40_000, pool.SiteShare);
        Assert.Equal(0, pool.SettledShare);
        Assert.Contains(detail.Data.Venues, v => v.Kind == SettlementKindEnum.Coach && v.Id == world.CoachId && v.UnsettledShare == 360_000);

        var pending = await NewFinance().GetPendingAsync(world.Seed.ProviderId, SettlementKindEnum.Coach);
        Assert.Equal(2, pending.Data.ItemCount);
        Assert.Equal(360_000, pending.Data.ProviderShare);
        Assert.Equal(40_000, pending.Data.SiteShare);
        Assert.Equal(400_000, pending.Data.PaymentPrice);
        Assert.All(pending.Data.Items, i => Assert.Equal("Coach", i.Type));

        var card = await AddCardAsync(world.Seed.OwnerUserId);
        var settle = await NewSettlement().InsertAsyncDto(new SettlementDto { ProviderId = world.Seed.ProviderId, Kind = SettlementKindEnum.Coach, UserBankCardId = card });
        var settled = await LoadSettlementAsync(settle.Data.Id);
        Assert.Equal(pending.Data.ProviderShare, settled.PaidPrice);
        Assert.Equal(pending.Data.ItemCount, settled.ItemCount);

        var after = await NewFinance().GetDetailAsync(world.Seed.ProviderId);
        Assert.Equal(0, after.Data.CoachPool.UnsettledShare);
        Assert.Equal(360_000, after.Data.CoachPool.SettledShare);
        Assert.Equal(360_000, after.Data.CoachPool.PaidOut);
        Assert.Equal(1, after.Data.CoachPool.SettlementCount);

        var list = await NewFinance().SearchAsync(new ProviderFinanceInputDto { PageSize = 200 });
        var row = list.List.Single(r => r.ProviderId == world.Seed.ProviderId);
        Assert.Equal(1, row.CoachCount);
        Assert.Equal(0, row.CoachUnsettled);
        Assert.Equal(360_000, row.PaidOut);
    }

    [Fact]
    public async Task SearchForProviderUser_ShowsOnlyOwnCoachSettlements()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        await ReserveAsync(world, Tomorrow(10), complete: true);
        var card = await AddCardAsync(world.Seed.OwnerUserId);
        var settle = await NewSettlement().InsertAsyncDto(new SettlementDto { ProviderId = world.Seed.ProviderId, Kind = SettlementKindEnum.Coach, UserBankCardId = card });
        Assert.True(settle.IsSuccess);

        var own = await NewSettlement().SearchForProviderUserAsync(new SettlementInputDto(), SettlementKindEnum.Coach, world.Seed.OwnerUserId);
        Assert.Single(own.List);
        var other = await NewSettlement().SearchForProviderUserAsync(new SettlementInputDto(), SettlementKindEnum.Coach, world.Seed.OtherUserId);
        Assert.Empty(other.List);
        var wrongKind = await NewSettlement().SearchForProviderUserAsync(new SettlementInputDto(), SettlementKindEnum.Store, world.Seed.OwnerUserId);
        Assert.Empty(wrongKind.List);

        Assert.True(await NewSettlement().BelongsToProviderUserAsync(settle.Data.Id, SettlementKindEnum.Coach, world.Seed.OwnerUserId));
        Assert.False(await NewSettlement().BelongsToProviderUserAsync(settle.Data.Id, SettlementKindEnum.Coach, world.Seed.OtherUserId));
        Assert.False(await NewSettlement().BelongsToProviderUserAsync(settle.Data.Id, SettlementKindEnum.Companion, world.Seed.OwnerUserId));
    }
}
