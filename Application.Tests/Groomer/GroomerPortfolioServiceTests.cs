using Application.Common.Dto.LocationPoint;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Dto;
using Application.Services.GroomerSrvs.GroomerSrv;
using Application.Services.GroomerSrvs.GroomerSrv.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests.Groomer;

public class GroomerPortfolioServiceTests : IClassFixture<GroomerLocalDbFixture>
{
    private readonly GroomerLocalDbFixture _fixture;

    public GroomerPortfolioServiceTests(GroomerLocalDbFixture fixture) => _fixture = fixture;

    private GroomerPortfolioService NewService() =>
        new(_fixture.NewContext(), _fixture.Mapper, NullLogger<GroomerPortfolioService>.Instance);

    private async Task<long> CreateApprovedGroomerAsync(GroomerLocalDbFixture.SeedData seed)
    {
        using var context = _fixture.NewContext();
        var service = new GroomerCoreService(context, _fixture.Mapper, NullLogger<GroomerCoreService>.Instance);
        var created = await service.CreateAsync(new GroomerDto
        {
            Name = "Salon",
            ProviderId = seed.ProviderId,
            Phone = "021",
            AddressValue = "addr",
            StateId = seed.StateId,
            CityId = seed.CityId,
            Location = new PointDto(51, 35),
        }, byAdmin: true, actorUserId: 1);
        Assert.True(created.IsSuccess);
        return created.Data.Id;
    }

    private static GroomerPortfolioPostInputDto PostInput(long groomerId, GroomerLocalDbFixture.SeedData seed, int pictures = 2) => new()
    {
        GroomerId = groomerId,
        Caption = "before and after",
        PetId = seed.PetId,
        Pictures = Enumerable.Range(0, pictures)
            .Select(i => new GroomerPortfolioPictureInputDto { PictureId = seed.PictureIds[i], Kind = i == 0 ? 1 : 2 })
            .ToList(),
    };

    private async Task<long> CreatePostAsync(long groomerId, GroomerLocalDbFixture.SeedData seed)
    {
        var created = await NewService().CreateAsync(PostInput(groomerId, seed), seed.OwnerUserId);
        Assert.True(created.IsSuccess);
        return created.Data.Id;
    }

    [Fact]
    public async Task Create_RequiresOwnerPicturesAndApprovedGroomer()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var groomerId = await CreateApprovedGroomerAsync(seed);

        Assert.False((await NewService().CreateAsync(PostInput(groomerId, seed), seed.OtherUserId)).IsSuccess);
        Assert.False((await NewService().CreateAsync(PostInput(groomerId, seed, pictures: 0), seed.OwnerUserId)).IsSuccess);

        var duplicatePictures = PostInput(groomerId, seed);
        duplicatePictures.Pictures[1].PictureId = duplicatePictures.Pictures[0].PictureId;
        Assert.False((await NewService().CreateAsync(duplicatePictures, seed.OwnerUserId)).IsSuccess);

        var longCaption = PostInput(groomerId, seed);
        longCaption.Caption = new string('x', 2001);
        Assert.False((await NewService().CreateAsync(longCaption, seed.OwnerUserId)).IsSuccess);

        var created = await NewService().CreateAsync(PostInput(groomerId, seed), seed.OwnerUserId);
        Assert.True(created.IsSuccess);
        Assert.Equal(2, created.Data.Pictures.Count);
        Assert.Equal(new[] { 0, 1 }, created.Data.Pictures.Select(p => p.SortOrder).ToArray());

        using var context = _fixture.NewContext();
        Assert.Equal(1, (await context.Groomers.AsNoTracking().FirstAsync(g => g.Id == groomerId)).PortfolioCount);
    }

    [Fact]
    public async Task Like_IsIdempotent_AndSurvivesConcurrentDuplicates()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var groomerId = await CreateApprovedGroomerAsync(seed);
        var postId = await CreatePostAsync(groomerId, seed);

        await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => NewService().LikeAsync(postId, seed.LikerUserId)));
        var again = await NewService().LikeAsync(postId, seed.LikerUserId);
        Assert.True(again.IsSuccess);
        Assert.Equal(1, again.Data.LikeCount);

        var second = await NewService().LikeAsync(postId, seed.OtherUserId);
        Assert.Equal(2, second.Data.LikeCount);

        var feed = await NewService().SearchPublicAsync(new GroomerPortfolioPostSearchInputDto { GroomerId = groomerId }, seed.LikerUserId);
        Assert.True(feed.List.Single().LikedByMe);
        var anonymous = await NewService().SearchPublicAsync(new GroomerPortfolioPostSearchInputDto { GroomerId = groomerId }, null);
        Assert.False(anonymous.List.Single().LikedByMe);

        var unliked = await NewService().UnlikeAsync(postId, seed.LikerUserId);
        Assert.Equal(1, unliked.Data.LikeCount);
        Assert.Equal(1, (await NewService().UnlikeAsync(postId, seed.LikerUserId)).Data.LikeCount);

        using var context = _fixture.NewContext();
        Assert.Equal(1, (await context.Groomers.AsNoTracking().FirstAsync(g => g.Id == groomerId)).PortfolioLikeCount);
    }

    [Fact]
    public async Task Comments_FlattenReplies_CountAndCascadeDelete()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var groomerId = await CreateApprovedGroomerAsync(seed);
        var postId = await CreatePostAsync(groomerId, seed);

        Assert.False((await NewService().AddCommentAsync(new GroomerPortfolioCommentInputDto { PostId = postId, Text = "   " }, seed.LikerUserId)).IsSuccess);
        Assert.False((await NewService().AddCommentAsync(new GroomerPortfolioCommentInputDto { PostId = postId, Text = new string('a', 1001) }, seed.LikerUserId)).IsSuccess);

        var root = await NewService().AddCommentAsync(new GroomerPortfolioCommentInputDto { PostId = postId, Text = "lovely" }, seed.LikerUserId);
        Assert.True(root.IsSuccess);
        var reply = await NewService().AddCommentAsync(new GroomerPortfolioCommentInputDto { PostId = postId, Text = "thanks", ParentId = root.Data.Id }, seed.OwnerUserId);
        var replyToReply = await NewService().AddCommentAsync(new GroomerPortfolioCommentInputDto { PostId = postId, Text = "welcome", ParentId = reply.Data.Id }, seed.LikerUserId);
        Assert.Equal(root.Data.Id, reply.Data.ParentId);
        Assert.Equal(root.Data.Id, replyToReply.Data.ParentId);
        Assert.False((await NewService().AddCommentAsync(new GroomerPortfolioCommentInputDto { PostId = postId, Text = "x", ParentId = 987654 }, seed.LikerUserId)).IsSuccess);

        var top = await NewService().SearchCommentsAsync(new GroomerPortfolioCommentSearchInputDto { PostId = postId }, seed.LikerUserId);
        Assert.Single(top.List);
        Assert.Equal(2, top.List[0].ReplyCount);
        Assert.True(top.List[0].IsMine);
        var replies = await NewService().SearchCommentsAsync(new GroomerPortfolioCommentSearchInputDto { PostId = postId, ParentId = root.Data.Id }, null);
        Assert.Equal(new[] { "thanks", "welcome" }, replies.List.Select(c => c.Text).ToArray());

        Assert.Equal(3, (await NewService().GetPublicAsync(postId, null)).Data.CommentCount);

        // یک کاربر دیگر نمی‌تواند نظر دیگری را حذف کند؛ مالک آرایشگاه و ادمین می‌توانند.
        Assert.False((await NewService().DeleteCommentAsync(root.Data.Id, seed.OtherUserId, isAdmin: false)).IsSuccess);
        Assert.True((await NewService().DeleteCommentAsync(root.Data.Id, seed.OwnerUserId, isAdmin: false)).IsSuccess);
        Assert.Equal(0, (await NewService().GetPublicAsync(postId, null)).Data.CommentCount);
        Assert.Empty((await NewService().SearchCommentsAsync(new GroomerPortfolioCommentSearchInputDto { PostId = postId }, null)).List);
    }

    [Fact]
    public async Task Moderation_HidesPostFromPublicFeed_AndUpdatesCounters()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var groomerId = await CreateApprovedGroomerAsync(seed);
        var postId = await CreatePostAsync(groomerId, seed);

        Assert.False((await NewService().ModerateAsync(new GroomerPortfolioModerationDto { Id = postId, Approved = false })).IsSuccess);
        Assert.True((await NewService().ModerateAsync(new GroomerPortfolioModerationDto { Id = postId, Approved = false, ApprovalValue = "inappropriate" })).IsSuccess);

        Assert.Empty((await NewService().SearchPublicAsync(new GroomerPortfolioPostSearchInputDto { GroomerId = groomerId }, null)).List);
        Assert.False((await NewService().GetPublicAsync(postId, null)).IsSuccess);
        Assert.False((await NewService().LikeAsync(postId, seed.LikerUserId)).IsSuccess);
        Assert.False((await NewService().AddCommentAsync(new GroomerPortfolioCommentInputDto { PostId = postId, Text = "hi" }, seed.LikerUserId)).IsSuccess);

        var manage = await NewService().SearchManageAsync(new GroomerPortfolioPostSearchInputDto { GroomerId = groomerId }, seed.OwnerUserId);
        Assert.Equal("inappropriate", manage.List.Single().ApprovalValue);
        Assert.Empty((await NewService().SearchManageAsync(new GroomerPortfolioPostSearchInputDto { GroomerId = groomerId }, seed.OtherUserId)).List);

        using (var context = _fixture.NewContext())
            Assert.Equal(0, (await context.Groomers.AsNoTracking().FirstAsync(g => g.Id == groomerId)).PortfolioCount);

        Assert.True((await NewService().ModerateAsync(new GroomerPortfolioModerationDto { Id = postId, Approved = true })).IsSuccess);
        Assert.Single((await NewService().SearchPublicAsync(new GroomerPortfolioPostSearchInputDto { GroomerId = groomerId }, null)).List);
    }

    [Fact]
    public async Task InactiveGroomer_PostsDisappearFromPublicFeed()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var groomerId = await CreateApprovedGroomerAsync(seed);
        await CreatePostAsync(groomerId, seed);

        using (var context = _fixture.NewContext())
        {
            var core = new GroomerCoreService(context, _fixture.Mapper, NullLogger<GroomerCoreService>.Instance);
            Assert.True((await core.UpdateActiveAsync(new GroomerActiveDto { Id = groomerId, Active = false }, seed.OwnerUserId)).IsSuccess);
        }

        Assert.Empty((await NewService().SearchPublicAsync(new GroomerPortfolioPostSearchInputDto { GroomerId = groomerId }, null)).List);
    }

    [Fact]
    public async Task UpdateAndDelete_RespectOwnershipAndKeepCounters()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var groomerId = await CreateApprovedGroomerAsync(seed);
        var postId = await CreatePostAsync(groomerId, seed);

        var edit = PostInput(groomerId, seed, pictures: 1);
        edit.Id = postId;
        edit.Caption = "edited";
        Assert.False((await NewService().UpdateAsync(edit, seed.OtherUserId)).IsSuccess);
        var updated = await NewService().UpdateAsync(edit, seed.OwnerUserId);
        Assert.True(updated.IsSuccess);
        Assert.Equal("edited", updated.Data.Caption);
        Assert.Single(updated.Data.Pictures);

        Assert.False((await NewService().DeleteAsync(postId, seed.OtherUserId)).IsSuccess);
        Assert.True((await NewService().DeleteAsync(postId, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().DeleteAsync(postId, seed.OwnerUserId)).IsSuccess);
        using var context = _fixture.NewContext();
        Assert.Equal(0, (await context.Groomers.AsNoTracking().FirstAsync(g => g.Id == groomerId)).PortfolioCount);
    }
}
