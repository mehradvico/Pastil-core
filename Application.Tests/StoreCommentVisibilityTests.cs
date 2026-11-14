using Application.Services.ProductSrvs.StoreCommentSrv;
using Application.Services.ProductSrvs.StoreCommentSrv.Dto;
using Application.Tests.Clinic;
using Entities.Entities;
using Entities.Entities.Security;
using System.Reflection;
using Xunit;

namespace Application.Tests;

// صفحه‌ی فروشگاه فقط نظرات «تأییدشده» را نشان می‌دهد؛ در انتظار/ردشده فقط برای صف تأیید (Available=null) دیده می‌شود.
public class StoreCommentVisibilityTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;
    public StoreCommentVisibilityTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    public class NullProxy : DispatchProxy
    {
        protected override object Invoke(MethodInfo method, object[] args)
        {
            var type = method.ReturnType;
            if (type == typeof(Task)) return Task.CompletedTask;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var inner = type.GetGenericArguments()[0];
                return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(inner).Invoke(null, new[] { inner.IsValueType ? Activator.CreateInstance(inner) : null })!;
            }
            return type.IsValueType && type != typeof(void) ? Activator.CreateInstance(type) : null;
        }
    }

    private StoreCommentService Service() => new(_fixture.NewContext(), _fixture.Mapper,
        DispatchProxy.Create<Application.Services.Setting.CodeSrv.Iface.ICodeService, NullProxy>(),
        DispatchProxy.Create<Application.Services.StoreSrv.Iface.IStoreService, NullProxy>());

    private long SeedStoreWithComments()
    {
        using var c = _fixture.NewContext();
        var tag = Random.Shared.NextInt64(1_000_000, 9_999_999).ToString();
        var user = new User { FirstName = "U", LastName = "U", Mobile = "0941" + tag };
        var store = new Store { Name = "Shop" + tag, ReferralCode = "R" + tag, Active = true };
        c.Users.Add(user);
        c.Stores.Add(store);
        var pending = new Code { Name = "p" + tag, Label = "Comment_NotChecked", Active = true };
        var accepted = new Code { Name = "a" + tag, Label = "Comment_Accept", Active = true };
        var rejected = new Code { Name = "r" + tag, Label = "Comment_Reject", Active = true };
        c.Codes.AddRange(pending, accepted, rejected);
        c.SaveChanges();
        foreach (var (text, code) in new[] { ("در انتظار", pending), ("تأییدشده", accepted), ("ردشده", rejected) })
            c.StoreComments.Add(new StoreComment { StoreId = store.Id, UserId = user.Id, Text = text, Rate = 5, StatusId = code.Id, CreateDate = DateTime.Now });
        c.SaveChanges();
        return store.Id;
    }

    [Fact]
    public void Public_search_returns_only_approved_comments_even_if_client_asks_for_everything()
    {
        if (!_fixture.Available) return;
        var storeId = SeedStoreWithComments();
        var publicResult = Service().Search(new StoreCommentInputDto { StoreId = storeId, Available = true, AllStatus = false, PageIndex = 1, PageSize = 20 });
        var only = Assert.Single(publicResult.List);
        Assert.Equal("تأییدشده", only.Text);
        Assert.Equal(1, publicResult.TotalCount);
    }

    [Fact]
    public void Moderation_queue_without_available_still_sees_all_and_rejected_filter_works()
    {
        if (!_fixture.Available) return;
        var storeId = SeedStoreWithComments();
        var all = Service().Search(new StoreCommentInputDto { StoreId = storeId, AllStatus = false, PageIndex = 1, PageSize = 20 });
        Assert.Equal(3, all.List.Count);
        var rejected = Service().Search(new StoreCommentInputDto { StoreId = storeId, Available = false, AllStatus = false, PageIndex = 1, PageSize = 20 });
        Assert.Equal("ردشده", Assert.Single(rejected.List).Text);
    }
}
