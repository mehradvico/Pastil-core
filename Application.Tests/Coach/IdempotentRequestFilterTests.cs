using Api.Filter;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Interface;
using Xunit;

namespace Application.Tests.Coach;

/// <summary>Idempotency-Key در دیتابیس ثبت می‌شود: کلید تکراری پاسخ موفق اول را برمی‌گرداند و اکشن دوباره اجرا نمی‌شود.</summary>
public class IdempotentRequestFilterTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public IdempotentRequestFilterTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private sealed class FakeCurrentUser : ICurrentUserHelper
    {
        public CurrentUserDto CurrentUser { get; } = new() { UserId = 4242 };
    }

    private (ActionExecutingContext Context, ServiceProvider Provider) NewContext(string key)
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddScoped<IDataBaseContext>(_ => _fixture.NewContext());
        services.AddSingleton<ICurrentUserHelper>(new FakeCurrentUser());
        var provider = services.BuildServiceProvider();

        var http = new DefaultHttpContext { RequestServices = provider };
        http.Request.Method = "POST";
        http.Request.Path = "/api/EndUser/PansionReserve";
        http.Request.Headers["Idempotency-Key"] = key;
        var actionContext = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
        return (context, provider);
    }

    private static ActionExecutionDelegate Executes(ActionExecutingContext context, object payload, Action onRun) => () =>
    {
        onRun();
        var executed = new ActionExecutedContext(context, new List<IFilterMetadata>(), new object()) { Result = new OkObjectResult(payload) };
        return Task.FromResult(executed);
    };

    [Fact]
    public async Task SameKey_ReturnsFirstSuccessAndDoesNotRunAgain_AndFailedRunsAreForgotten()
    {
        if (!_fixture.Available) return;
        var key = "k-" + Guid.NewGuid().ToString("N");
        var filter = new IdempotentRequestAttribute();

        var runs = 0;
        var (first, p1) = NewContext(key);
        await filter.OnActionExecutionAsync(first, Executes(first, new BaseResultDto(true, "ok"), () => runs++));
        Assert.Equal(1, runs);
        Assert.Null(first.Result);

        // «replica» دیگر (provider جدید) همان کلید
        var (second, p2) = NewContext(key);
        await filter.OnActionExecutionAsync(second, Executes(second, new BaseResultDto(true, "second"), () => runs++));
        Assert.Equal(1, runs);
        var content = Assert.IsType<ContentResult>(second.Result);
        Assert.Contains("ok", content.Content);

        // اجرای ناموفق نگه داشته نمی‌شود و تلاش دوباره ممکن است
        var failKey = "f-" + Guid.NewGuid().ToString("N");
        var (fail1, p3) = NewContext(failKey);
        await filter.OnActionExecutionAsync(fail1, Executes(fail1, new BaseResultDto(false, "no"), () => runs++));
        var (fail2, p4) = NewContext(failKey);
        await filter.OnActionExecutionAsync(fail2, Executes(fail2, new BaseResultDto(true, "yes"), () => runs++));
        Assert.Equal(3, runs);
        Assert.Null(fail2.Result);

        foreach (var p in new[] { p1, p2, p3, p4 }) await p.DisposeAsync();
    }
}
