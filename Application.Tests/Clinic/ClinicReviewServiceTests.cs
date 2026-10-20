using Application.Services.ClinicSrvs.ClinicReserveSrv;
using Application.Services.ClinicSrvs.ClinicReviewSrv;
using Application.Services.ClinicSrvs.ClinicSrv;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static Application.Tests.Clinic.ClinicReserveTestKit;

namespace Application.Tests.Clinic;

public class ClinicReviewServiceTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;

    public ClinicReviewServiceTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    private ClinicReviewService NewService()
    {
        var context = _fixture.NewContext();
        var core = new ClinicCoreService(context, _fixture.Mapper, NullLogger<ClinicCoreService>.Instance);
        return new ClinicReviewService(context, core, NullLogger<ClinicReviewService>.Instance);
    }

    private async Task<(World World, long ReserveId)> CompletedReserveAsync(bool complete = true)
    {
        var world = await BuildWorldAsync(_fixture, capacity: 5);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        await PayAsync(_fixture, created.Id);
        if (complete)
        {
            using var context = _fixture.NewContext();
            await context.ClinicReserves.Where(r => r.Id == created.Id).ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, (int)ClinicReserveStatusEnum.Complete)
                .SetProperty(r => r.OwnerDecision, (int)ClinicReserveOwnerDecisionEnum.Approved));
        }
        return (world, created.Id);
    }

    [Fact]
    public async Task Create_OnlyBookerOfCompletedReserve_OncePerReserve_WithValidInput()
    {
        if (!_fixture.Available) return;
        var (world, reserveId) = await CompletedReserveAsync();
        var (_, unfinished) = await CompletedReserveAsync(complete: false);

        Assert.False((await NewService().CreateAsync(new ClinicReviewInputDto { ClinicReserveId = reserveId, Rate = 5, Text = "great" }, world.Seed.OtherUserId)).IsSuccess);
        Assert.False((await NewService().CreateAsync(new ClinicReviewInputDto { ClinicReserveId = unfinished, Rate = 5, Text = "great" }, world.BookerId)).IsSuccess);
        Assert.False((await NewService().CreateAsync(new ClinicReviewInputDto { ClinicReserveId = reserveId, Rate = 0, Text = "x" }, world.BookerId)).IsSuccess);
        Assert.False((await NewService().CreateAsync(new ClinicReviewInputDto { ClinicReserveId = reserveId, Rate = 6, Text = "x" }, world.BookerId)).IsSuccess);
        Assert.False((await NewService().CreateAsync(new ClinicReviewInputDto { ClinicReserveId = reserveId, Rate = 4, Text = "  " }, world.BookerId)).IsSuccess);
        Assert.False((await NewService().CreateAsync(new ClinicReviewInputDto { ClinicReserveId = reserveId, Rate = 4, Text = new string('a', 1001) }, world.BookerId)).IsSuccess);

        var created = await NewService().CreateAsync(new ClinicReviewInputDto { ClinicReserveId = reserveId, Rate = 4, Text = "good" }, world.BookerId);
        Assert.True(created.IsSuccess);
        Assert.Equal((long)Application.Common.Enumerable.CommentEnum.Comment_NotChecked, created.Data.StatusId);
        Assert.False((await NewService().CreateAsync(new ClinicReviewInputDto { ClinicReserveId = reserveId, Rate = 5, Text = "again" }, world.BookerId)).IsSuccess);
    }

    [Fact]
    public async Task Moderation_ControlsPublicVisibility_AndRecomputesRating()
    {
        if (!_fixture.Available) return;
        var (world, reserveId) = await CompletedReserveAsync();
        var review = (await NewService().CreateAsync(new ClinicReviewInputDto { ClinicReserveId = reserveId, Rate = 4, Text = "good" }, world.BookerId)).Data;

        // تا تأیید دیده نمی‌شود و امتیاز کلینیک عوض نمی‌شود
        Assert.Empty((await NewService().SearchPublicAsync(new ClinicReviewSearchInputDto { ClinicId = world.ClinicId })).List);
        Assert.Single((await NewService().SearchManageAsync(new ClinicReviewSearchInputDto { ClinicId = world.ClinicId })).List);
        using (var context = _fixture.NewContext())
        {
            var clinic = await context.Clinics.AsNoTracking().FirstAsync(g => g.Id == world.ClinicId);
            Assert.Equal(0, clinic.RateCount);
        }

        Assert.True((await NewService().ModerateAsync(review.Id, accept: true)).IsSuccess);
        var visible = await NewService().SearchPublicAsync(new ClinicReviewSearchInputDto { ClinicId = world.ClinicId });
        Assert.Single(visible.List);
        using (var context = _fixture.NewContext())
        {
            var clinic = await context.Clinics.AsNoTracking().FirstAsync(g => g.Id == world.ClinicId);
            Assert.Equal(1, clinic.RateCount);
            Assert.Equal(1, clinic.CommentCount);
            Assert.Equal(4, clinic.RateAvg);
        }

        Assert.True((await NewService().ModerateAsync(review.Id, accept: false)).IsSuccess);
        Assert.Empty((await NewService().SearchPublicAsync(new ClinicReviewSearchInputDto { ClinicId = world.ClinicId })).List);
        using (var context = _fixture.NewContext())
            Assert.Equal(0, (await context.Clinics.AsNoTracking().FirstAsync(g => g.Id == world.ClinicId)).RateCount);
        Assert.False((await NewService().ModerateAsync(987654, true)).IsSuccess);
    }

    [Fact]
    public async Task Answer_OnlyOwnerOrAdmin_AndPublicSearchIgnoresStatusFilter()
    {
        if (!_fixture.Available) return;
        var (world, reserveId) = await CompletedReserveAsync();
        var review = (await NewService().CreateAsync(new ClinicReviewInputDto { ClinicReserveId = reserveId, Rate = 5, Text = "nice" }, world.BookerId)).Data;

        Assert.False((await NewService().AnswerAsync(review.Id, "thanks", world.Seed.OtherUserId)).IsSuccess);
        Assert.False((await NewService().AnswerAsync(review.Id, "  ", world.Seed.OwnerUserId)).IsSuccess);
        Assert.True((await NewService().AnswerAsync(review.Id, "thanks", world.Seed.OwnerUserId)).IsSuccess);
        Assert.True((await NewService().AnswerAsync(review.Id, "edited by admin", null)).IsSuccess);

        // فیلتر وضعیتی که کلاینت فرستاده در جستجوی عمومی نادیده گرفته می‌شود
        var hacked = await NewService().SearchPublicAsync(new ClinicReviewSearchInputDto { ClinicId = world.ClinicId, StatusId = (long)Application.Common.Enumerable.CommentEnum.Comment_NotChecked });
        Assert.Empty(hacked.List);

        var own = await NewService().SearchManageAsync(new ClinicReviewSearchInputDto { OwnerUserId = world.Seed.OwnerUserId });
        Assert.Equal("edited by admin", own.List.Single().Answer);
        Assert.Empty((await NewService().SearchManageAsync(new ClinicReviewSearchInputDto { OwnerUserId = world.Seed.OtherUserId })).List);
    }
}
