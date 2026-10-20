using Application.Common.PostCommit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests;

public class PostCommitQueueTests
{
    private static PostCommitQueue Create() =>
        new(new ServiceCollection().BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(), NullLogger<PostCommitQueue>.Instance);

    [Fact]
    public async Task Flush_RunsQueuedActionsInBackground_AndFailureDoesNotStopTheRest()
    {
        var queue = Create();
        var done = new TaskCompletionSource();
        var order = new List<string>();
        queue.Add("first", _ => { order.Add("first"); throw new InvalidOperationException("boom"); });
        queue.Add("second", _ => { order.Add("second"); done.SetResult(); return Task.CompletedTask; });

        Assert.Empty(order);          // nothing runs before Flush
        queue.Flush();

        await done.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(new[] { "first", "second" }, order);
    }

    [Fact]
    public async Task Clear_DropsActions_SoARolledBackApplyNeverSendsNotifications()
    {
        var queue = Create();
        var ran = false;
        queue.Add("x", _ => { ran = true; return Task.CompletedTask; });
        queue.Clear();
        queue.Flush();

        await Task.Delay(200);
        Assert.False(ran);
    }

    [Fact]
    public void IsActive_IsOffByDefault_SoOnlyPaymentAppliesDeferPushes()
    {
        var queue = Create();
        Assert.False(queue.IsActive);
        queue.Activate();
        Assert.True(queue.IsActive);
        queue.Deactivate();
        Assert.False(queue.IsActive);
    }

    [Fact]
    public async Task Flush_TwiceDoesNotRunTheSameActionTwice()
    {
        var queue = Create();
        var count = 0;
        var done = new TaskCompletionSource();
        queue.Add("x", _ => { count++; done.TrySetResult(); return Task.CompletedTask; });
        queue.Flush();
        queue.Flush();

        await done.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Task.Delay(200);
        Assert.Equal(1, count);
    }
}
