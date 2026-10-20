using Application.Services.GroomerSrvs.GroomerReserveSrv;
using Entities.Entities;
using Entities.Entities.GroomerField;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Application.Tests.Groomer.GroomerReserveTestKit;

namespace Application.Tests.Groomer;

public class GroomerReserveApprovalTests : IClassFixture<GroomerLocalDbFixture>
{
    private readonly GroomerLocalDbFixture _fixture;

    public GroomerReserveApprovalTests(GroomerLocalDbFixture fixture) => _fixture = fixture;

    private async Task<(World World, long ReserveId)> PaidReserveAsync(DateTime? doDate = null, int capacity = 2)
    {
        var world = await BuildWorldAsync(_fixture, capacity: capacity);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, doDate ?? Tomorrow(10)), world.BookerId)).Data;
        await PayAsync(_fixture, created.Id);
        return (world, created.Id);
    }

    private async Task<GroomerReserve> LoadAsync(long id)
    {
        using var context = _fixture.NewContext();
        return await context.GroomerReserves.AsNoTracking().FirstAsync(r => r.Id == id);
    }

    private async Task<List<Wallet>> RefundsAsync(long reserveId)
    {
        using var context = _fixture.NewContext();
        var name = GroomerReserveRules.RefundLedgerName(reserveId);
        return await context.Wallets.AsNoTracking().Where(w => w.Name == name).ToListAsync();
    }

    [Fact]
    public async Task Approve_OnlyPendingAndOwnerOrAdmin()
    {
        if (!_fixture.Available) return;
        var (world, id) = await PaidReserveAsync();
        var (push, proxy) = NewPush();

        Assert.False((await NewApprovalService(_fixture, push: push).ApproveAsync(id, world.Seed.OtherUserId)).IsSuccess);
        Assert.True((await NewApprovalService(_fixture, push: push).ApproveAsync(id, world.Seed.OwnerUserId)).IsSuccess);
        var approved = await LoadAsync(id);
        Assert.Equal((int)GroomerReserveOwnerDecisionEnum.Approved, approved.OwnerDecision);
        Assert.NotNull(approved.OwnerDecisionDate);
        Assert.False(approved.IsCancel);
        Assert.NotEmpty(proxy.Calls);

        // دوباره تأیید/رد نمی‌شود
        Assert.False((await NewApprovalService(_fixture).ApproveAsync(id, null)).IsSuccess);
        Assert.False((await NewApprovalService(_fixture).RejectAsync(id, "late", null)).IsSuccess);
    }

    [Fact]
    public async Task Approve_RejectsUnpaidAndCancelled()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var unpaid = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        Assert.False((await NewApprovalService(_fixture).ApproveAsync(unpaid.Id, null)).IsSuccess);
        Assert.False((await NewApprovalService(_fixture).ApproveAsync(987654, null)).IsSuccess);
    }

    [Fact]
    public async Task Reject_RequiresReason_CancelsAndRefundsFullPaymentOnce()
    {
        if (!_fixture.Available) return;
        var (world, id) = await PaidReserveAsync();

        Assert.False((await NewApprovalService(_fixture).RejectAsync(id, "  ", world.Seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewApprovalService(_fixture).RejectAsync(id, "busy", world.Seed.OtherUserId)).IsSuccess);
        Assert.Empty(await RefundsAsync(id));

        Assert.True((await NewApprovalService(_fixture).RejectAsync(id, "we are closed", world.Seed.OwnerUserId)).IsSuccess);
        var rejected = await LoadAsync(id);
        Assert.True(rejected.IsCancel);
        Assert.False(rejected.CancelledByBooker);
        Assert.Equal("we are closed", rejected.CancelDetail);
        Assert.Equal((int)GroomerReserveOwnerDecisionEnum.Rejected, rejected.OwnerDecision);
        var refunds = await RefundsAsync(id);
        Assert.Single(refunds);
        Assert.Equal(rejected.PaymentPrice, refunds[0].Amount);
        Assert.True(refunds[0].IsIncrease);
        Assert.Equal(world.BookerId, refunds[0].UserId);

        // دومین رد، دومین برگشت پول نمی‌سازد
        Assert.False((await NewApprovalService(_fixture).RejectAsync(id, "again", null)).IsSuccess);
        Assert.Single(await RefundsAsync(id));
    }

    [Fact]
    public async Task Reject_RollsBackWhenRefundFails()
    {
        if (!_fixture.Available) return;
        var (world, id) = await PaidReserveAsync();

        var failing = NewApprovalService(_fixture, wallet: NewWallet(_fixture, succeed: false).Wallet);
        var result = await failing.RejectAsync(id, "no", world.Seed.OwnerUserId);

        Assert.False(result.IsSuccess);
        var untouched = await LoadAsync(id);
        Assert.False(untouched.IsCancel);
        Assert.Equal((int)GroomerReserveOwnerDecisionEnum.Pending, untouched.OwnerDecision);
        Assert.Empty(await RefundsAsync(id));
    }

    [Fact]
    public async Task Reject_FreesTheSlot()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture, capacity: 1);
        var second = new UserPet { Name = "B", PetId = world.Seed.PetId, UserId = world.Seed.OtherUserId, Size = "Small" };
        using (var context = _fixture.NewContext())
        {
            context.UserPets.Add(second);
            await context.SaveChangesAsync();
        }
        var a = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        await PayAsync(_fixture, a.Id);

        var other = Input(world, Tomorrow(10));
        other.UserPetId = second.Id;
        Assert.False((await NewReserveService(_fixture).CreateAsync(other, world.Seed.OtherUserId)).IsSuccess);
        await NewApprovalService(_fixture).RejectAsync(a.Id, "x", null);
        Assert.True((await NewReserveService(_fixture).CreateAsync(other, world.Seed.OtherUserId)).IsSuccess);
    }

    [Fact]
    public async Task ExpireOverdue_CancelsOnlyOverduePendingAndRefunds()
    {
        if (!_fixture.Available) return;
        var (world, overdue) = await PaidReserveAsync();
        var (_, fresh) = await PaidReserveAsync();
        var (_, approved) = await PaidReserveAsync();
        using (var context = _fixture.NewContext())
        {
            await context.GroomerReserves.Where(r => r.Id == overdue || r.Id == approved)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.OwnerApprovalDeadline, DateTime.Now.AddMinutes(-5)));
        }
        await NewApprovalService(_fixture).ApproveAsync(approved, null);
        var (push, proxy) = NewPush();

        await NewApprovalService(_fixture, push: push).ExpireOverdueAsync();

        var expired = await LoadAsync(overdue);
        Assert.True(expired.IsCancel);
        Assert.Equal((int)GroomerReserveOwnerDecisionEnum.Expired, expired.OwnerDecision);
        Assert.Single(await RefundsAsync(overdue));
        Assert.False((await LoadAsync(fresh)).IsCancel);
        Assert.False((await LoadAsync(approved)).IsCancel);
        Assert.Empty(await RefundsAsync(fresh));
        Assert.Empty(await RefundsAsync(approved));
        Assert.NotEmpty(proxy.Calls);

        // اجرای دوباره چیز تازه‌ای نمی‌کند
        await NewApprovalService(_fixture).ExpireOverdueAsync();
        Assert.Single(await RefundsAsync(overdue));
    }

    [Fact]
    public async Task BookerCancel_FreeWindowRefunds_LateAfterApprovalDoesNot_PendingAlwaysRefunds()
    {
        if (!_fixture.Available) return;

        // لغو رایگان: بیش از ۱۲ ساعت مانده
        var (world, free) = await PaidReserveAsync(DateTime.Today.AddDays(3).AddHours(10));
        Assert.False((await NewApprovalService(_fixture).CancelByBookerAsync(free, world.Seed.OtherUserId)).IsSuccess);
        Assert.True((await NewApprovalService(_fixture).CancelByBookerAsync(free, world.BookerId)).IsSuccess);
        var cancelled = await LoadAsync(free);
        Assert.True(cancelled.IsCancel);
        Assert.True(cancelled.CancelledByBooker);
        Assert.Single(await RefundsAsync(free));
        Assert.False((await NewApprovalService(_fixture).CancelByBookerAsync(free, world.BookerId)).IsSuccess);
        Assert.Single(await RefundsAsync(free));

        // لغو دیرهنگام بعد از تأیید: لغو می‌شود ولی بدون برگشت پول
        var (world2, late) = await PaidReserveAsync();
        await NewApprovalService(_fixture).ApproveAsync(late, null);
        using (var context = _fixture.NewContext())
            await context.GroomerReserves.Where(r => r.Id == late)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.DoDate, DateTime.Now.AddHours(3)).SetProperty(r => r.EndDate, DateTime.Now.AddHours(4)));
        Assert.True((await NewApprovalService(_fixture).CancelByBookerAsync(late, world2.BookerId)).IsSuccess);
        Assert.True((await LoadAsync(late)).IsCancel);
        Assert.Empty(await RefundsAsync(late));

        // هنوز منتظر تأیید است: حتی نزدیک نوبت، کل مبلغ برمی‌گردد
        var (world3, pending) = await PaidReserveAsync();
        using (var context = _fixture.NewContext())
            await context.GroomerReserves.Where(r => r.Id == pending)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.DoDate, DateTime.Now.AddHours(3)).SetProperty(r => r.EndDate, DateTime.Now.AddHours(4)));
        Assert.True((await NewApprovalService(_fixture).CancelByBookerAsync(pending, world3.BookerId)).IsSuccess);
        Assert.Single(await RefundsAsync(pending));
    }

    [Fact]
    public async Task BookerCancel_UnpaidJustClosesAndNeverRefunds()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;

        Assert.True((await NewApprovalService(_fixture).CancelByBookerAsync(created.Id, world.BookerId)).IsSuccess);
        Assert.True((await LoadAsync(created.Id)).IsCancel);
        Assert.Empty(await RefundsAsync(created.Id));
    }

    [Fact]
    public async Task BookerCancel_NotAllowedAfterStartOrCompletion()
    {
        if (!_fixture.Available) return;
        var (world, started) = await PaidReserveAsync();
        using (var context = _fixture.NewContext())
            await context.GroomerReserves.Where(r => r.Id == started)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.DoDate, DateTime.Now.AddMinutes(-30)).SetProperty(r => r.EndDate, DateTime.Now.AddMinutes(30)));
        Assert.False((await NewApprovalService(_fixture).CancelByBookerAsync(started, world.BookerId)).IsSuccess);
        Assert.False((await LoadAsync(started)).IsCancel);

        var (world2, done) = await PaidReserveAsync();
        using (var context = _fixture.NewContext())
            await context.GroomerReserves.Where(r => r.Id == done)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, (int)GroomerReserveStatusEnum.Complete));
        Assert.False((await NewApprovalService(_fixture).CancelByBookerAsync(done, world2.BookerId)).IsSuccess);
    }

    [Fact]
    public async Task AdminCancel_RequiresReason_RefundOptional_NotAfterCompletion()
    {
        if (!_fixture.Available) return;
        var (world, withRefund) = await PaidReserveAsync();
        var (_, noRefund) = await PaidReserveAsync();

        Assert.False((await NewApprovalService(_fixture).CancelByAdminAsync(withRefund, " ", true)).IsSuccess);
        Assert.True((await NewApprovalService(_fixture).CancelByAdminAsync(withRefund, "fraud check", true)).IsSuccess);
        Assert.True((await LoadAsync(withRefund)).IsCancel);
        Assert.Single(await RefundsAsync(withRefund));

        Assert.True((await NewApprovalService(_fixture).CancelByAdminAsync(noRefund, "ops decision", false)).IsSuccess);
        Assert.Empty(await RefundsAsync(noRefund));
        Assert.False((await NewApprovalService(_fixture).CancelByAdminAsync(noRefund, "again", true)).IsSuccess);

        var (_, done) = await PaidReserveAsync();
        using (var context = _fixture.NewContext())
            await context.GroomerReserves.Where(r => r.Id == done)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, (int)GroomerReserveStatusEnum.Complete));
        Assert.False((await NewApprovalService(_fixture).CancelByAdminAsync(done, "too late", true)).IsSuccess);
        Assert.False((await NewApprovalService(_fixture).CancelByAdminAsync(987654, "x", true)).IsSuccess);
    }

    [Fact]
    public async Task ZeroPayment_CancelDoesNotCreateRefundRow()
    {
        if (!_fixture.Available) return;
        var world = await BuildWorldAsync(_fixture);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        Assert.True((await NewReserveService(_fixture, rebate: NewRebate(finalPrice: 999_999_999, rebateId: 1)).SetRebateCodeAsync(created.Id, "FULL", world.BookerId)).IsSuccess);
        await PayAsync(_fixture, created.Id);
        Assert.Equal(0, (await LoadAsync(created.Id)).PaymentPrice);

        Assert.True((await NewApprovalService(_fixture).RejectAsync(created.Id, "no", null)).IsSuccess);
        Assert.Empty(await RefundsAsync(created.Id));
    }
}
