using Application.Services.Content.DiscussionAnswerSrv.Dto;
using Application.Services.Content.DiscussionChatSrv;
using Application.Services.Content.DiscussionChatSrv.Dto;
using Application.Services.Content.DiscussionQuestionSrv.Dto;
using Application.Tests.Clinic;
using Entities.Entities;
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.ProductRelations;

// پرسش و پاسخ کالا به‌صورت چت‌روم: تأیید ادمین، بلاک پیام جدید تا تأیید، پاسخ به پاسخ، پاسخ فروشگاه، ویرایش/حذف پیام در انتظار.
public class DiscussionChatServiceTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;
    public DiscussionChatServiceTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    private (long ProductId, long UserA, long UserB, long StoreUser, long StoreId) Seed()
    {
        using var c = _fixture.NewContext();
        var tag = Random.Shared.NextInt64(1_000_000, 9_999_999).ToString();
        var a = new User { FirstName = "A", LastName = "A", Mobile = "0931" + tag };
        var b = new User { FirstName = "B", LastName = "B", Mobile = "0932" + tag };
        var s = new User { FirstName = "S", LastName = "S", Mobile = "0933" + tag };
        c.Users.AddRange(a, b, s);
        c.SaveChanges();
        var store = new Store { Name = "Shop" + tag, Active = true, ReferralCode = "R" + tag };
        store.Users = new List<User> { s };
        c.Stores.Add(store);
        c.SaveChanges();
        var status = new Code { Name = "st" + tag, Label = "st" + tag, Active = true };
        var type = new Code { Name = "ty" + tag, Label = "ty" + tag, Active = true };
        c.Codes.AddRange(status, type);
        c.SaveChanges();
        var product = new Product { Name = "P" + tag, ProductLabel = "p" + tag, Active = true, StatusId = status.Id, TypeId = type.Id, CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
        c.Products.Add(product);
        c.SaveChanges();
        c.ProductItems.Add(new ProductItem { ProductId = product.Id, StoreId = store.Id, Active = true, SystemActive = true, Quantity = 5, Price = 100 });
        c.SaveChanges();
        return (product.Id, a.Id, b.Id, s.Id, store.Id);
    }

    private DiscussionChatService Svc() => new(_fixture.NewContext());

    private async Task Approve(long questionId)
    {
        using var c = _fixture.NewContext();
        await c.DiscussionQuestions.Where(q => q.Id == questionId).ExecuteUpdateAsync(s => s.SetProperty(q => q.AdminConfirm, true).SetProperty(q => q.Active, true));
    }

    private async Task ApproveAnswer(long id, bool ok)
    {
        using var c = _fixture.NewContext();
        await c.DiscussionAnswers.Where(a => a.Id == id).ExecuteUpdateAsync(s => s.SetProperty(a => a.AdminConfirm, ok).SetProperty(a => a.Active, ok));
    }

    [Fact]
    public async Task Question_is_always_pending_even_if_the_client_asks_for_approval_and_blocks_the_next_message()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var r = await Svc().SubmitQuestionAsync(w.UserA, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "این کالا مناسب گربه است؟", AdminConfirm = true, Active = true, AnswerCount = 9 });
        Assert.True(r.IsSuccess);
        using (var c = _fixture.NewContext())
        {
            var row = await c.DiscussionQuestions.SingleAsync(q => q.Id == r.Data.Id);
            Assert.Null(row.AdminConfirm);
            Assert.False(row.Active);
            Assert.Equal(0, row.AnswerCount);
        }
        Assert.False((await Svc().SubmitQuestionAsync(w.UserA, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "پرسش دوم" })).IsSuccess);
        Assert.False((await Svc().SubmitQuestionAsync(w.UserA, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "ab" })).IsSuccess);
        Assert.False((await Svc().SubmitQuestionAsync(w.UserB, new DiscussionQuestionDto { ProductId = 0, Content = "پرسش بدون کالا" })).IsSuccess);
        var status = await Svc().MyStatusAsync(w.UserA, w.ProductId);
        Assert.True(status.HasPending);
        Assert.Single(status.Items);
        Assert.Equal(0, status.Items[0].Status);
        Assert.False((await Svc().MyStatusAsync(w.UserB, w.ProductId)).HasPending);
    }

    [Fact]
    public async Task Rejection_releases_the_block_and_rejected_stays_visible_to_the_author()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var q = (await Svc().SubmitQuestionAsync(w.UserA, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "پرسش اول من" })).Data;
        using (var c = _fixture.NewContext())
            await c.DiscussionQuestions.Where(x => x.Id == q.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.AdminConfirm, false));
        var status = await Svc().MyStatusAsync(w.UserA, w.ProductId);
        Assert.False(status.HasPending);
        Assert.Equal(2, status.Items.Single().Status);
        Assert.True((await Svc().SubmitQuestionAsync(w.UserA, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "پرسش بعدی من" })).IsSuccess);
    }

    [Fact]
    public async Task Anyone_can_reply_to_the_question_or_to_a_specific_answer_only_after_approval()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var q = (await Svc().SubmitQuestionAsync(w.UserA, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "این کالا مناسب سگ کوچک است؟" })).Data;
        Assert.False((await Svc().SubmitAnswerAsync(w.UserB, new DiscussionAnswerDto { DiscussionQuestionId = q.Id, Content = "بله مناسب است" })).IsSuccess);
        await Approve(q.Id);

        var a1 = await Svc().SubmitAnswerAsync(w.UserB, new DiscussionAnswerDto { DiscussionQuestionId = q.Id, Content = "بله مناسب است", Active = true });
        Assert.True(a1.IsSuccess);
        using (var c = _fixture.NewContext())
        {
            var row = await c.DiscussionAnswers.SingleAsync(x => x.Id == a1.Data.Id);
            Assert.Null(row.AdminConfirm);
            Assert.False(row.Active);
            Assert.Null(row.StoreId);
        }
        Assert.False((await Svc().SubmitAnswerAsync(w.UserB, new DiscussionAnswerDto { DiscussionQuestionId = q.Id, Content = "پاسخ دوم" })).IsSuccess);
        Assert.False((await Svc().SubmitQuestionAsync(w.UserB, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "پرسش تازه" })).IsSuccess);
        Assert.False((await Svc().SubmitAnswerAsync(w.UserA, new DiscussionAnswerDto { DiscussionQuestionId = q.Id, ReplyToAnswerId = a1.Data.Id, Content = "ممنون" })).IsSuccess);

        await ApproveAnswer(a1.Data.Id, true);
        var reply = await Svc().SubmitAnswerAsync(w.UserA, new DiscussionAnswerDto { DiscussionQuestionId = q.Id, ReplyToAnswerId = a1.Data.Id, Content = "ممنون از پاسخ شما" });
        Assert.True(reply.IsSuccess);
        Assert.Equal(a1.Data.Id, reply.Data.ReplyToAnswerId);
        Assert.True((await Svc().SubmitAnswerAsync(w.UserB, new DiscussionAnswerDto { DiscussionQuestionId = q.Id, Content = "پاسخ دوم من بعد از تأیید" })).IsSuccess);

        var other = (await Svc().SubmitQuestionAsync(w.StoreUser, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "پرسش دیگر از فروشگاه" })).Data;
        await Approve(other.Id);
        Assert.False((await Svc().SubmitAnswerAsync(w.StoreUser, new DiscussionAnswerDto { DiscussionQuestionId = other.Id, ReplyToAnswerId = a1.Data.Id, Content = "پاسخ نامعتبر" })).IsSuccess);
    }

    [Fact]
    public async Task Store_staff_answer_is_marked_with_the_store()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var q = (await Svc().SubmitQuestionAsync(w.UserA, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "ارسال دارید؟" })).Data;
        await Approve(q.Id);
        var r = await Svc().SubmitAnswerAsync(w.StoreUser, new DiscussionAnswerDto { DiscussionQuestionId = q.Id, Content = "بله، ارسال داریم" });
        Assert.True(r.IsSuccess);
        using var c = _fixture.NewContext();
        Assert.Equal(w.StoreId, (await c.DiscussionAnswers.SingleAsync(x => x.Id == r.Data.Id)).StoreId);
    }

    [Fact]
    public async Task Only_the_author_can_edit_or_delete_and_only_while_pending()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var q = (await Svc().SubmitQuestionAsync(w.UserA, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "متن اولیه پرسش" })).Data;
        Assert.False((await Svc().EditAsync(w.UserB, new DiscussionChatEditDto { Kind = DiscussionChatKind.Question, Id = q.Id, Content = "هک" })).IsSuccess);
        Assert.False((await Svc().EditAsync(w.UserA, new DiscussionChatEditDto { Kind = DiscussionChatKind.Question, Id = q.Id, Content = "ab" })).IsSuccess);
        Assert.True((await Svc().EditAsync(w.UserA, new DiscussionChatEditDto { Kind = DiscussionChatKind.Question, Id = q.Id, Content = "متن ویرایش‌شده پرسش" })).IsSuccess);
        using (var c = _fixture.NewContext())
            Assert.Equal("متن ویرایش‌شده پرسش", (await c.DiscussionQuestions.SingleAsync(x => x.Id == q.Id)).Content);

        Assert.False((await Svc().DeleteAsync(w.UserB, DiscussionChatKind.Question, q.Id)).IsSuccess);
        Assert.True((await Svc().DeleteAsync(w.UserA, DiscussionChatKind.Question, q.Id)).IsSuccess);
        Assert.False((await Svc().MyStatusAsync(w.UserA, w.ProductId)).HasPending);

        var q2 = (await Svc().SubmitQuestionAsync(w.UserA, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "پرسش دوم من" })).Data;
        await Approve(q2.Id);
        Assert.False((await Svc().EditAsync(w.UserA, new DiscussionChatEditDto { Kind = DiscussionChatKind.Question, Id = q2.Id, Content = "بعد از تأیید" })).IsSuccess);
        Assert.False((await Svc().DeleteAsync(w.UserA, DiscussionChatKind.Question, q2.Id)).IsSuccess);
    }

    [Fact]
    public async Task Public_search_shows_only_approved_answers_with_store_info_and_admin_sees_pending_counts()
    {
        if (!_fixture.Available) return;
        var w = Seed();
        var q = (await Svc().SubmitQuestionAsync(w.UserA, new DiscussionQuestionDto { ProductId = w.ProductId, Content = "پرسش برای نمایش عمومی" })).Data;
        await Approve(q.Id);
        var shown = (await Svc().SubmitAnswerAsync(w.StoreUser, new DiscussionAnswerDto { DiscussionQuestionId = q.Id, Content = "پاسخ تأییدشده فروشگاه" })).Data;
        await ApproveAnswer(shown.Id, true);
        var hidden = (await Svc().SubmitAnswerAsync(w.UserB, new DiscussionAnswerDto { DiscussionQuestionId = q.Id, ReplyToAnswerId = shown.Id, Content = "پاسخ در انتظار تأیید" })).Data;

        var service = new Application.Services.Content.DiscussionQuestionSrv.DiscussionQuestionService(_fixture.NewContext(), _fixture.Mapper, null);
        var publicResult = service.Search(new DiscussionQuestionInputDto { ProductId = w.ProductId, Available = true, PageIndex = 1, PageSize = 20 });
        var item = Assert.Single(publicResult.List);
        var answer = Assert.Single(item.DiscussionAnswers);
        Assert.Equal(shown.Id, answer.Id);
        Assert.True(answer.IsStoreAnswer);
        Assert.StartsWith("Shop", answer.StoreName);
        Assert.NotEqual(hidden.Id, answer.Id);

        var admin = service.Search(new DiscussionQuestionInputDto { ProductId = w.ProductId, HasPendingAnswers = true, PageIndex = 1, PageSize = 20 });
        Assert.Equal(1, Assert.Single(admin.List).PendingAnswerCount);
    }
}
