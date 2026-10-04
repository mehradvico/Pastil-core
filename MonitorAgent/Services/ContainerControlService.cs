using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MonitorAgent.Services;

/// <summary>
/// Start / stop / restart for Docker containers, used by the Pastil Monitor
/// mobile app (through the API). This is deliberately narrow:
///  - opt-in: nothing works unless MONITOR_CONTROL_ENABLED=true;
///  - only three verbs, mapped to fixed Docker endpoints;
///  - the caller-supplied name is only ever matched against names Docker itself
///    reports, and the Docker-reported id is what goes into the URL;
///  - protected containers (this agent, and optionally others) are refused.
/// </summary>
public sealed partial class ContainerControlService
{
    private static readonly string[] Actions = ["restart", "stop", "start"];

    private readonly HttpClient _docker;
    private readonly bool _enabled;
    private readonly string[] _neverControl;
    private readonly string[] _noStop;

    public ContainerControlService(HostSnapshotService snapshotService, IConfiguration configuration)
    {
        _docker = snapshotService.DockerClient;
        _enabled = string.Equals(configuration["MONITOR_CONTROL_ENABLED"], "true", StringComparison.OrdinalIgnoreCase);
        _neverControl = Split(configuration["MONITOR_CONTROL_NEVER"], "monitor-agent");
        // Stopping the API from the phone would remove the only path that can start it again.
        _noStop = Split(configuration["MONITOR_CONTROL_NO_STOP"], "pastil-api");
    }

    public bool Enabled => _enabled;

    public async Task<IReadOnlyList<ManagedContainer>> ListAsync(CancellationToken cancellationToken)
    {
        var containers = await ReadAllAsync(cancellationToken);
        return containers
            .Select(c => new ManagedContainer(
                c.Name,
                c.State,
                c.Status,
                IsProtected(c.Name),
                !IsProtected(c.Name) && !IsNoStop(c.Name)))
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<ControlResult> ExecuteAsync(string name, string action, CancellationToken cancellationToken)
    {
        if (!_enabled) return ControlResult.Fail(ControlOutcome.Disabled);
        action = action.ToLowerInvariant();
        if (!Actions.Contains(action) || !NameRegex().IsMatch(name)) return ControlResult.Fail(ControlOutcome.Invalid);

        var target = (await ReadAllAsync(cancellationToken))
            .FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.Ordinal));
        if (target is null) return ControlResult.Fail(ControlOutcome.NotFound);
        if (IsProtected(target.Name)) return ControlResult.Fail(ControlOutcome.Protected);
        if (action == "stop" && IsNoStop(target.Name)) return ControlResult.Fail(ControlOutcome.StopNotAllowed);

        var running = target.State == "running";
        if (action == "start" && running) return ControlResult.Ok(ControlOutcome.AlreadyInState);
        if (action == "stop" && !running) return ControlResult.Ok(ControlOutcome.AlreadyInState);

        // Restarting the API container from the API kills the request mid-flight, so give Docker
        // a few seconds and otherwise report "accepted" instead of blocking the caller.
        var call = PostAsync($"v1.41/containers/{target.Id}/{action}?t=10");
        var finished = await Task.WhenAny(call, Task.Delay(TimeSpan.FromSeconds(4), cancellationToken));
        if (finished != call) return ControlResult.Ok(ControlOutcome.Accepted);

        return await call;
    }

    private async Task<ControlResult> PostAsync(string path)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var response = await _docker.PostAsync(path, content: null, timeout.Token);
            return response.StatusCode switch
            {
                HttpStatusCode.NoContent => ControlResult.Ok(ControlOutcome.Done),
                HttpStatusCode.NotModified => ControlResult.Ok(ControlOutcome.AlreadyInState),
                HttpStatusCode.NotFound => ControlResult.Fail(ControlOutcome.NotFound),
                _ => ControlResult.Fail(ControlOutcome.DockerError)
            };
        }
        catch
        {
            return ControlResult.Fail(ControlOutcome.DockerError);
        }
    }

    private async Task<IReadOnlyList<DockerContainer>> ReadAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _docker.GetAsync("v1.41/containers/json?all=1", cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var result = new List<DockerContainer>();
            foreach (var item in document.RootElement.EnumerateArray())
            {
                var id = Str(item, "Id");
                var name = item.TryGetProperty("Names", out var names) && names.ValueKind == JsonValueKind.Array
                    ? names.EnumerateArray().Select(n => n.GetString()).FirstOrDefault()?.TrimStart('/')
                    : null;
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name)) continue;
                result.Add(new DockerContainer(id, name, Str(item, "State"), Str(item, "Status")));
            }

            return result;
        }
        catch
        {
            return Array.Empty<DockerContainer>();
        }
    }

    private bool IsProtected(string name) => _neverControl.Any(p => name.Contains(p, StringComparison.OrdinalIgnoreCase));

    private bool IsNoStop(string name) => _noStop.Any(p => name.Contains(p, StringComparison.OrdinalIgnoreCase));

    private static string[] Split(string? value, string fallback) =>
        (string.IsNullOrWhiteSpace(value) ? fallback : value)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string Str(JsonElement element, string property) =>
        element.TryGetProperty(property, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

    [GeneratedRegex("^[a-zA-Z0-9][a-zA-Z0-9_.-]{0,127}$")]
    private static partial Regex NameRegex();

    private sealed record DockerContainer(string Id, string Name, string State, string Status);
}

public sealed record ManagedContainer(string Name, string State, string Status, bool Protected, bool CanStop);

public enum ControlOutcome
{
    Done,
    Accepted,
    AlreadyInState,
    Disabled,
    Invalid,
    NotFound,
    Protected,
    StopNotAllowed,
    DockerError
}

public sealed record ControlResult(bool Success, ControlOutcome Outcome)
{
    public static ControlResult Ok(ControlOutcome outcome) => new(true, outcome);

    public static ControlResult Fail(ControlOutcome outcome) => new(false, outcome);
}
