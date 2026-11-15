using Application.Services.ProductSrvs.AiProductMatchSrv.Iface;
using Application.Services.ProductSrvs.ProductImageSuggestionSrv;
using Application.Services.ProductSrvs.ProductImageSuggestionSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Application.Tests.Clinic;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using Xunit;

namespace Application.Tests;

// تصویر پیشنهادی فروشگاه برای محصول: ثبت (فقط برای محصولِ همین فروشگاه)، و تأیید/رد ادمین؛
// تأیید روی محصولِ دارای تصویر بدون تأیید صریح جایگزینی چیزی را عوض نمی‌کند.
public class ProductImageSuggestionTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;
    public ProductImageSuggestionTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    private sealed class FakeFileClient : IAiProductMatchFileClient
    {
        private readonly Func<long> _nextPictureId;
        public FakeFileClient(Func<long> nextPictureId) => _nextPictureId = nextPictureId;
        public Task<(bool Ok, long? PictureId, string Error)> UploadAsync(IFormFile image, string authorizationHeaderValue, CancellationToken cancellationToken)
            => Task.FromResult((true, (long?)_nextPictureId(), (string)null));
    }

    private record World(long ProductId, long StoreId, long OtherStoreId, long ProductWithPictureId, long OldPictureId);

    private World Seed()
    {
        using var c = _fixture.NewContext();
        var tag = Random.Shared.NextInt64(1_000_000, 9_999_999).ToString();
        var store = new Store { Name = "S" + tag, ReferralCode = "A" + tag, Active = true };
        var other = new Store { Name = "O" + tag, ReferralCode = "B" + tag, Active = true };
        c.Stores.AddRange(store, other);
        var oldPicture = new Picture { Name = "old" + tag + ".jpg", Url = "/Media/x", CreateDate = DateTime.Now };
        c.Pictures.Add(oldPicture);
        c.SaveChanges();

        // Product دو Code (وضعیت/نوع) را خودکار Include می‌کند؛ بدون این ردیف‌ها محصول در کوئری‌ها دیده نمی‌شود
        var available = new Code { Name = "av" + tag, Label = "ProductStatus_Available", Active = true };
        var normal = new Code { Name = "ty" + tag, Label = "ProductType_Normal", Active = true };
        c.Codes.AddRange(available, normal);
        c.SaveChanges();

        var bare = new Product { Name = "Bare" + tag, ProductLabel = "bare" + tag, Active = true, StatusId = available.Id, TypeId = normal.Id, CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
        var withPicture = new Product { Name = "Pic" + tag, ProductLabel = "pic" + tag, Active = true, PictureId = oldPicture.Id, StatusId = available.Id, TypeId = normal.Id, CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
        c.Products.AddRange(bare, withPicture);
        c.SaveChanges();

        // فقط فروشگاه اول هر دو محصول را دارد
        c.ProductItems.AddRange(
            new ProductItem { ProductId = bare.Id, StoreId = store.Id, BasePrice = 100, Price = 100, Quantity = 1, Active = true, SystemActive = true },
            new ProductItem { ProductId = withPicture.Id, StoreId = store.Id, BasePrice = 100, Price = 100, Quantity = 1, Active = true, SystemActive = true });
        c.SaveChanges();
        return new World(bare.Id, store.Id, other.Id, withPicture.Id, oldPicture.Id);
    }

    private long NewPicture()
    {
        using var c = _fixture.NewContext();
        var picture = new Picture { Name = "p" + Guid.NewGuid().ToString("N") + ".jpg", Url = "/Media/x", CreateDate = DateTime.Now };
        c.Pictures.Add(picture);
        c.SaveChanges();
        return picture.Id;
    }

    private ProductImageSuggestionService Service() => new(
        _fixture.NewContext(),
        new FakeFileClient(NewPicture),
        DispatchProxy.Create<INoticeService, StoreCommentVisibilityTests.NullProxy>(),
        NullLogger<ProductImageSuggestionService>.Instance);

    private static IFormFile PngFile()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0, 1, 2, 3, 4 };
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "image", "photo.png");
    }

    private async Task<long> SubmitAsync(long storeId, long productId)
    {
        var res = await Service().SubmitAsync(storeId, null, productId, PngFile(), "", CancellationToken.None);
        Assert.True(res.IsSuccess);
        return res.Data.Id;
    }

    [Fact]
    public async Task Seller_can_only_suggest_for_a_product_the_store_sells()
    {
        if (!_fixture.Available) return;
        var w = Seed();

        var rejected = await Service().SubmitAsync(w.OtherStoreId, null, w.ProductId, PngFile(), "", CancellationToken.None);
        Assert.False(rejected.IsSuccess);

        var accepted = await Service().SubmitAsync(w.StoreId, null, w.ProductId, PngFile(), "", CancellationToken.None);
        Assert.True(accepted.IsSuccess);
        Assert.Equal("pending", accepted.Data.Status);
    }

    [Fact]
    public async Task Rejects_a_file_that_is_not_an_image()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var bytes = new byte[32];
        var fake = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "image", "photo.png");
        var res = await Service().SubmitAsync(w.StoreId, null, w.ProductId, fake, "", CancellationToken.None);
        Assert.False(res.IsSuccess);
    }

    [Fact]
    public async Task A_second_submission_replaces_the_pending_one_instead_of_adding_a_row()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var first = await SubmitAsync(w.StoreId, w.ProductId);
        var second = await SubmitAsync(w.StoreId, w.ProductId);

        Assert.Equal(first, second);
        using var c = _fixture.NewContext();
        Assert.Equal(1, await c.ProductImageSuggestions.CountAsync(x => x.StoreId == w.StoreId && x.ProductId == w.ProductId));
    }

    [Fact]
    public async Task Approve_sets_the_picture_when_the_product_has_none()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var id = await SubmitAsync(w.StoreId, w.ProductId);

        var res = await Service().ApproveAsync(1, new ProductImageSuggestionApproveDto { Id = id });

        Assert.True(res.IsSuccess, string.Join("|", res.Messages.Select(m => m.Item1)));
        Assert.False(res.Data.Replaced);
        using var c = _fixture.NewContext();
        var suggestion = await c.ProductImageSuggestions.AsNoTracking().FirstAsync(x => x.Id == id);
        var product = await c.Products.AsNoTracking().FirstAsync(x => x.Id == w.ProductId);
        Assert.Equal(ProductImageSuggestionStatus.Approved, suggestion.Status);
        Assert.Equal(suggestion.PictureId, product.PictureId);
        Assert.Null(suggestion.ReplacedPictureId);
    }

    [Fact]
    public async Task Approve_on_a_product_that_already_has_a_picture_needs_explicit_confirmation()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var id = await SubmitAsync(w.StoreId, w.ProductWithPictureId);

        var refused = await Service().ApproveAsync(1, new ProductImageSuggestionApproveDto { Id = id });

        Assert.False(refused.IsSuccess);
        Assert.Equal(ProductImageSuggestionService.CodeProductHasPicture, refused.Code);
        using (var c = _fixture.NewContext())
        {
            // هیچ‌چیز عوض نشده: محصول همان عکس قبلی را دارد و پیشنهاد هنوز در صف است
            Assert.Equal(w.OldPictureId, (await c.Products.AsNoTracking().FirstAsync(x => x.Id == w.ProductWithPictureId)).PictureId);
            Assert.Equal(ProductImageSuggestionStatus.Pending, (await c.ProductImageSuggestions.AsNoTracking().FirstAsync(x => x.Id == id)).Status);
        }

        var confirmed = await Service().ApproveAsync(1, new ProductImageSuggestionApproveDto { Id = id, ConfirmReplace = true });

        Assert.True(confirmed.IsSuccess);
        Assert.True(confirmed.Data.Replaced);
        using var c2 = _fixture.NewContext();
        var suggestion = await c2.ProductImageSuggestions.AsNoTracking().FirstAsync(x => x.Id == id);
        Assert.Equal(suggestion.PictureId, (await c2.Products.AsNoTracking().FirstAsync(x => x.Id == w.ProductWithPictureId)).PictureId);
        Assert.Equal(w.OldPictureId, suggestion.ReplacedPictureId);
    }

    [Fact]
    public async Task A_suggestion_can_only_be_decided_once()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var id = await SubmitAsync(w.StoreId, w.ProductId);

        Assert.True((await Service().ApproveAsync(1, new ProductImageSuggestionApproveDto { Id = id })).IsSuccess);
        Assert.False((await Service().ApproveAsync(1, new ProductImageSuggestionApproveDto { Id = id, ConfirmReplace = true })).IsSuccess);
        Assert.False((await Service().RejectAsync(1, new ProductImageSuggestionRejectDto { Id = id, Reason = "x" })).IsSuccess);
    }

    [Fact]
    public async Task Reject_needs_a_reason_and_leaves_the_product_alone()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var id = await SubmitAsync(w.StoreId, w.ProductId);

        Assert.False((await Service().RejectAsync(1, new ProductImageSuggestionRejectDto { Id = id, Reason = "  " })).IsSuccess);
        var res = await Service().RejectAsync(1, new ProductImageSuggestionRejectDto { Id = id, Reason = "کیفیت پایین" });

        Assert.True(res.IsSuccess);
        Assert.Equal("rejected", res.Data.Status);
        using var c = _fixture.NewContext();
        Assert.Null((await c.Products.AsNoTracking().FirstAsync(x => x.Id == w.ProductId)).PictureId);
    }

    [Fact]
    public async Task Seller_can_delete_only_their_own_non_approved_suggestion()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var id = await SubmitAsync(w.StoreId, w.ProductId);

        Assert.False((await Service().DeleteAsync(w.OtherStoreId, id)).IsSuccess);
        Assert.True((await Service().DeleteAsync(w.StoreId, id)).IsSuccess);

        var approvedId = await SubmitAsync(w.StoreId, w.ProductId);
        await Service().ApproveAsync(1, new ProductImageSuggestionApproveDto { Id = approvedId });
        Assert.False((await Service().DeleteAsync(w.StoreId, approvedId)).IsSuccess);
    }

    [Fact]
    public async Task Admin_search_defaults_to_pending_and_reports_whether_the_product_has_a_picture()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var id = await SubmitAsync(w.StoreId, w.ProductWithPictureId);

        var res = await Service().SearchAsync(new ProductImageSuggestionAdminSearchDto { StoreId = w.StoreId, PageSize = 50 });

        Assert.True(res.IsSuccess);
        var item = Assert.Single(res.Data.Items, x => x.Id == id);
        Assert.True(item.ProductHasPicture);
        Assert.Equal("pending", item.Status);
        Assert.False((await Service().SearchAsync(new ProductImageSuggestionAdminSearchDto { Status = "bogus" })).IsSuccess);
    }
}
