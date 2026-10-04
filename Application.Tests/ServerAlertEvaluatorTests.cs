using Application.Services.ServerMonitoringAlerts;
using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Application.Tests;

public class ServerAlertEvaluatorTests
{
    private static readonly DateTime T0 = new(2026, 10, 3, 10, 0, 0, DateTimeKind.Utc);

    private static ServerAlertOptions Options() => new()
    {
        BreachChecks = 3, ContainerDownChecks = 2, AgentDownChecks = 3, ClearChecks = 2, RepeatMinutes = 60
    };

    private static ServerSample Sample(int memUsed = 1000, double disk = 40, double load = 0.5, params string[] containers) =>
        new(4, load, 8000, memUsed, "10 GiB", "100 GiB", disk,
            containers.Select(c => new ServerContainerSample(c, 10, "100 MiB / 1 GiB")).ToList());

    [Fact]
    public void Healthy_server_produces_no_alerts()
    {
        var evaluator = new ServerAlertEvaluator(Options());
        for (var i = 0; i < 10; i++)
            Assert.Empty(evaluator.Evaluate(Sample(containers: "api"), T0.AddSeconds(30 * i)));
    }

    [Fact]
    public void Memory_alert_fires_only_after_required_consecutive_breaches()
    {
        var evaluator = new ServerAlertEvaluator(Options());
        Assert.Empty(evaluator.Evaluate(Sample(memUsed: 7600), T0));
        Assert.Empty(evaluator.Evaluate(Sample(memUsed: 7600), T0.AddSeconds(30)));
        var fired = evaluator.Evaluate(Sample(memUsed: 7600), T0.AddSeconds(60));
        var alert = Assert.Single(fired);
        Assert.Equal("memory", alert.Key);
        Assert.Equal(ServerAlertKind.Firing, alert.Kind);
    }

    [Fact]
    public void A_single_spike_resets_the_breach_counter()
    {
        var evaluator = new ServerAlertEvaluator(Options());
        evaluator.Evaluate(Sample(memUsed: 7600), T0);
        evaluator.Evaluate(Sample(memUsed: 7600), T0.AddSeconds(30));
        evaluator.Evaluate(Sample(memUsed: 1000), T0.AddSeconds(60));
        Assert.Empty(evaluator.Evaluate(Sample(memUsed: 7600), T0.AddSeconds(90)));
    }

    [Fact]
    public void Firing_alert_is_not_repeated_until_the_repeat_interval_then_resolves_once()
    {
        var evaluator = new ServerAlertEvaluator(Options());
        for (var i = 0; i < 3; i++) evaluator.Evaluate(Sample(disk: 95), T0.AddSeconds(30 * i));

        Assert.Empty(evaluator.Evaluate(Sample(disk: 95), T0.AddMinutes(10)));
        Assert.Single(evaluator.Evaluate(Sample(disk: 95), T0.AddMinutes(61)));

        Assert.Empty(evaluator.Evaluate(Sample(disk: 40), T0.AddMinutes(62)));
        var resolved = Assert.Single(evaluator.Evaluate(Sample(disk: 40), T0.AddMinutes(63)));
        Assert.Equal(ServerAlertKind.Resolved, resolved.Kind);
        Assert.Empty(evaluator.Evaluate(Sample(disk: 40), T0.AddMinutes(64)));
    }

    [Fact]
    public void Container_that_disappears_alerts_then_recovers()
    {
        var evaluator = new ServerAlertEvaluator(Options());
        evaluator.Evaluate(Sample(containers: new[] { "api", "db" }), T0);

        Assert.Empty(evaluator.Evaluate(Sample(containers: "api"), T0.AddSeconds(30)));
        var down = Assert.Single(evaluator.Evaluate(Sample(containers: "api"), T0.AddSeconds(60)));
        Assert.Equal("container-down:db", down.Key);
        Assert.Contains("db", down.Title);

        Assert.Empty(evaluator.Evaluate(Sample(containers: new[] { "api", "db" }), T0.AddSeconds(90)));
        var up = Assert.Single(evaluator.Evaluate(Sample(containers: new[] { "api", "db" }), T0.AddSeconds(120)));
        Assert.Equal(ServerAlertKind.Resolved, up.Kind);
        Assert.Contains("db", up.Title);
    }

    [Fact]
    public void Ignored_container_never_alerts()
    {
        var options = Options();
        options.IgnoredContainers = new[] { "migrator" };
        var evaluator = new ServerAlertEvaluator(options);
        evaluator.Evaluate(Sample(containers: new[] { "api", "migrator" }), T0);
        for (var i = 1; i < 6; i++)
            Assert.Empty(evaluator.Evaluate(Sample(containers: "api"), T0.AddSeconds(30 * i)));
    }

    [Fact]
    public void Container_missing_for_longer_than_forget_window_is_forgotten()
    {
        var evaluator = new ServerAlertEvaluator(Options());
        evaluator.Evaluate(Sample(containers: new[] { "api", "old" }), T0);
        evaluator.Evaluate(Sample(containers: "api"), T0.AddSeconds(30));
        evaluator.Evaluate(Sample(containers: "api"), T0.AddSeconds(60)); // fires
        Assert.Empty(evaluator.Evaluate(Sample(containers: "api"), T0.AddHours(25))); // forgotten, silent
        Assert.Empty(evaluator.Evaluate(Sample(containers: "api"), T0.AddHours(26)));
    }

    [Fact]
    public void Agent_outage_alerts_and_recovery_is_reported_when_snapshot_returns()
    {
        var evaluator = new ServerAlertEvaluator(Options());
        Assert.Empty(evaluator.EvaluateUnavailable(T0));
        Assert.Empty(evaluator.EvaluateUnavailable(T0.AddSeconds(30)));
        Assert.Equal("agent-down", Assert.Single(evaluator.EvaluateUnavailable(T0.AddSeconds(60))).Key);

        Assert.Empty(evaluator.Evaluate(Sample(), T0.AddSeconds(90)));
        Assert.Equal(ServerAlertKind.Resolved, Assert.Single(evaluator.Evaluate(Sample(), T0.AddSeconds(120))).Kind);
    }

    [Fact]
    public void Sample_parses_the_monitor_agent_json()
    {
        const string json = """
        {"cpuCores":4,"load1":1.5,"memTotalMb":8000,"memUsedMb":4000,"diskUsed":"10 GiB","diskTotal":"100 GiB",
         "diskPercent":"10%","containers":[{"name":"api","cpu":"1%","memUsage":"1 MiB / 2 MiB","memPercent":"50.5%"},
         {"name":"x","cpu":"0%","memUsage":"1 MiB","memPercent":"—"}]}
        """;
        using var doc = JsonDocument.Parse(json);
        var sample = ServerSample.FromJson(doc.RootElement);
        Assert.Equal(10, sample.DiskPercent);
        Assert.Equal(50.5, sample.Containers[0].MemoryPercent);
        Assert.Null(sample.Containers[1].MemoryPercent);
    }
}
