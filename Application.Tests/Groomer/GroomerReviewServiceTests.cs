using Application.Services.GroomerSrvs.GroomerReserveSrv;
using Application.Services.GroomerSrvs.GroomerReviewSrv;
using Application.Services.GroomerSrvs.GroomerSrv;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static Application.Tests.Groomer.GroomerReserveTestKit;

namespace Application.Tests.Groomer;

public class GroomerReviewServiceTests : IClassFixture<GroomerLocalDbFixture>
{
    private readonly GroomerLocalDbFixture _fixture;

    public GroomerReviewServiceTests(GroomerLocalDbFixture fixture) => _fixture = fixture;

    private GroomerReviewService NewService()
    {
        var context = _fixture.NewContext();
        var core = new GroomerCoreService(context, _fixture.Mapper, NullLogger<GroomerCoreService>.Instance);
        return new GroomerReviewService(context, core, NullLogger<GroomerReviewService>.Instance);
    }

    private async Task<(World World, long ReserveId)> CompletedReserveAsync(bool complete = true)
    {
        var world = await BuildWorldAsync(_fixture, capacity: 5);
        var created = (await NewReserveService(_fixture).CreateAsync(Input(world, Tomorrow(10)), world.BookerId)).Data;
        await PayAsync(_fixture, created.Id);
        if (complete)
        {
            using var context = _fixture.NewContext();
            await context.GroomerReserves.Where(r => r.Id == created.Id).ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, (int)GroomerReserveStatusEnum.Complete)
                .SetProperty(r => r.OwnerDecision, (int)GroomerReserveOwnerDecisionEnum.Approved));
        }
        return (world, created.Id);
    }

    [Fact]
    public async Task Create_OnlyBookerOfCompletedReserve_OncePerReserve_WithValidInput()
    {
        if (!_fixture.Available) return;
        var (world, reserveId) = await CompletedReserveAsync();
        var (_, unfinished) = await CompletedReserveAsync(complete: false);

        Assert.False((await NewService().CreateAsync(new GroomerReviewInputDto { GroomerReserveId = reserveId, Rate = 5, Text = "great" }, world.Seed.OtherUserId)).IsSuccess);
        Assert.False((await NewService().CreateAsync(new GroomerReviewInputDto { GroomerReserveId = unfinished, Rate = 5, Text = "great" }, world.BookerId)).IsSuccess);
        Assert.False((await NewService().CreateAsync(new GroomerReviewInputDto { GroomerReserveId = reserveId, Rate = 0, Text = "x" }, world.BookerId)).IsSuccess);
        Assert.False((await NewService().CreateAsync(new GroomerReviewInputDto { GroomerReserveId = reserveId, Rate = 6, Text = "x" }, world.BookerId)).IsSuccess);
        Assert.False((await NewService().CreateAsync(new GroomerReviewInputDto { GroomerReserveId = reserveId, Rate = 4, Text = "  " }, world.BookerId)).IsSuccess);
        Assert.False((await NewService().CreateAsync(new GroomerReviewInputDto { GroomerReserveId = reserveId, Rate = 4, Text = new string('a', 1001) }, world.BookerId)).IsSuccess);

        var created = await NewService().CreateAsync(new GroomerReviewInputDto { GroomerReserveId = reserveId, Rate = 4, Text = "good" }, world.BookerId);
        Assert.True(created.IsSuccess);
        Assert.Equal((long)Application.Common.Enumerable.CommentEnum.Comment_NotChecked, created.Data.StatusId);
        Assert.False((await NewService().CreateAsync(new GroomerReviewInputDto { GroomerReserveId = reserveId, Rate = 5, Text = "again" }, world.BookerId)).IsSuccess);
    }

    [Fact]
    public async Task Moderation_ControlsPublicVisibility_AndRecomputesRating()
    {
        if (!_fixture.Available) return;
        var (world, reserveId) = await CompletedReserveAsync();
        var review = (await NewService().CreateAsync(new GroomerReviewInputDto { GroomerReserveId = reserveId, Rate = 4, Text = "good" }, world.BookerId)).Data;

        // تا تأیید دیده نمی‌شود و امتیاز آرایشگاه عوض نمی‌شود
        Assert.Empty((await NewService().SearchPublicAsync(new GroomerReviewSearchInputDto { GroomerId = world.GroomerId })).List);
        Assert.Single((await NewService().SearchManageAsync(new GroomerReviewSearchInputDto { GroomerId = world.GroomerId })).List);
        using (var context = _fixture.NewContext())
        {
            var groomer = await context.Groomers.AsNoTracking().FirstAsync(g => g.Id == world.GroomerId);
            Assert.Equal(0, groomer.RateCount);
        }

        Assert.True((await NewService().ModerateAsync(review.Id, accept: true)).IsSuccess);
        var visible = await NewService().SearchPublicAsync(new GroomerReviewSearchInputDto { GroomerId = world.GroomerId });
        Assert.Single(visible.List);
        using (var context = _fixture.NewContext())
        {
            var groomer = await context.Groomers.AsNoTracking().FirstAsync(g => g.Id == world.GroomerId);
            Assert.Equal(1, groomer.RateCount);
            Assert.Equal(1, groomer.CommentCount);
            Assert.Equal(4, groomer.RateAvg);
        }

        Assert.True((await NewService().ModerateAsync(review.Id, accept: false)).IsSuccess);
        Assert.Empty((await NewService().SearchPublicAsync(new GroomerReviewSearchInputDto { GroomerId = world.GroomerId })).List);
        using (var context = _fixture.NewContext())
            Assert.Equal(0, (await context.Groomers.AsNoTracking().FirstAsync(g => g.Id == world.GroomerId)).RateCount);
        Assert.False((await NewService().ModerateAsync(987654, true)).IsSuccess);
    }

    [Fact]
    public async Task Answer_OnlyOwnerOrAdmin_AndPublicSearchIgnoresStatusFilter()
    {
        if (!_fixture.Available) return;
        var (world, reserveId) = await CompletedReserveAsync();
        var review = (await NewService().CreateAsync(new GroomerReviewInputDto { GroomerReserveId = reserveId, Rate = 5, Text = "nice" }, world.BookerId)).Data;

        Assert.False((await NewService().AnswerAsync(review.Id, "thanks", world.Seed.OtherUserId)).IsSuccess);
        Assert.False((await NewService().AnswerAsync(review.Id, "  ", world.Seed.OwnerUserId)).IsSuccess);
        Assert.True((await NewService().AnswerAsync(review.Id, "thanks", world.Seed.OwnerUserId)).IsSuccess);
        Assert.True((await NewService().AnswerAsync(review.Id, "edited by admin", null)).IsSuccess);

        // فیلتر وضعیتی که کلاینت فرستاده در جستجوی عمومی نادیده گرفته می‌شود
        var hacked = await NewService().SearchPublicAsync(new GroomerReviewSearchInputDto { GroomerId = world.GroomerId, StatusId = (long)Application.Common.Enumerable.CommentEnum.Comment_NotChecked });
        Assert.Empty(hacked.List);

        var own = await NewService().SearchManageAsync(new GroomerReviewSearchInputDto { OwnerUserId = world.Seed.OwnerUserId });
        Assert.Equal("edited by admin", own.List.Single().Answer);
        Assert.Empty((await NewService().SearchManageAsync(new GroomerReviewSearchInputDto { OwnerUserId = world.Seed.OtherUserId })).List);
    }
}
