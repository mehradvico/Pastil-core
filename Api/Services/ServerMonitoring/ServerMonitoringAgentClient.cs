using System.Text.Json;

namespace Api.Services.ServerMonitoring;

/// <summary>
/// Reads a snapshot from the private monitoring agent. The agent token is
/// server-only and is never returned to a browser or mobile application.
/// </summary>
public sealed class ServerMonitoringAgentClient
{
    public const string HttpClientName = "ServerMonitoringAgent";

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ServerMonitoringAgentClient> _logger;

    public ServerMonitoringAgentClient(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<ServerMonitoringAgentClient> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ServerMonitoringAgentResult> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        var agentUrl = _configuration["ServerMonitoring:AgentUrl"];
        var agentToken = _configuration["PASTIL_MONITOR_AGENT_TOKEN"];

        if (string.IsNullOrWhiteSpace(agentUrl) || string.IsNullOrWhiteSpace(agentToken))
        {
            _logger.LogWarning("The private server-monitoring agent is not configured.");
            return ServerMonitoringAgentResult.Unavailable();
        }

        if (!Uri.TryCreate(agentUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            _logger.LogError("Server monitoring agent URL is invalid.");
            return ServerMonitoringAgentResult.Unavailable();
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("X-Pastil-Monitor-Token", agentToken);

            using var response = await _httpClientFactory
                .CreateClient(HttpClientName)
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "The private server-monitoring agent returned status {StatusCode}.",
                    (int)response.StatusCode);
                return ServerMonitoringAgentResult.Unavailable();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return ServerMonitoringAgentResult.Success(document.RootElement.Clone());
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("The private server-monitoring agent timed out.");
            return ServerMonitoringAgentResult.Unavailable();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "The private server-monitoring agent could not be reached.");
            return ServerMonitoringAgentResult.Unavailable();
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "The private server-monitoring agent returned invalid JSON.");
            return ServerMonitoringAgentResult.Unavailable();
        }
    }

    /// <summary>Lists every container (running or not) with the agent's control flags.</summary>
    public async Task<JsonElement?> ListContainersAsync(CancellationToken cancellationToken)
    {
        var (status, body) = await SendToAgentAsync(HttpMethod.Get, "containers", cancellationToken);
        return status == 200 ? body : null;
    }

    /// <summary>Runs start/stop/restart on one container. Returns the HTTP status and the agent's outcome.</summary>
    public async Task<(int Status, string? Outcome)> ControlContainerAsync(
        string name, string action, CancellationToken cancellationToken)
    {
        var (status, body) = await SendToAgentAsync(
            HttpMethod.Post,
            $"containers/{Uri.EscapeDataString(name)}/{Uri.EscapeDataString(action)}",
            cancellationToken);
        string? outcome = null;
        if (body is { ValueKind: JsonValueKind.Object } obj && obj.TryGetProperty("outcome", out var o))
            outcome = o.GetString();
        return (status, outcome);
    }

    private async Task<(int Status, JsonElement? Body)> SendToAgentAsync(
        HttpMethod method, string relativePath, CancellationToken cancellationToken)
    {
        var agentUrl = _configuration["ServerMonitoring:AgentUrl"];
        var agentToken = _configuration["PASTIL_MONITOR_AGENT_TOKEN"];
        if (string.IsNullOrWhiteSpace(agentUrl) || string.IsNullOrWhiteSpace(agentToken) ||
            !Uri.TryCreate(agentUrl, UriKind.Absolute, out var snapshotUri))
        {
            return (0, null);
        }

        // AgentUrl points at ".../snapshot"; the sibling routes live next to it.
        var uri = new Uri(snapshotUri, relativePath);
        try
        {
            using var request = new HttpRequestMessage(method, uri);
            request.Headers.Add("X-Pastil-Monitor-Token", agentToken);
            // Restart can take several seconds (the agent itself caps this at ~4s before replying).
            using var response = await _httpClientFactory.CreateClient(HttpClientName).SendAsync(request, cancellationToken);

            JsonElement? body = null;
            try
            {
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                body = document.RootElement.Clone();
            }
            catch (JsonException)
            {
            }

            return ((int)response.StatusCode, body);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "The private server-monitoring agent could not be reached for {Path}.", relativePath);
            return (0, null);
        }
    }
}

public sealed record ServerMonitoringAgentResult(JsonElement? Snapshot)
{
    public bool IsAvailable => Snapshot.HasValue;

    public static ServerMonitoringAgentResult Success(JsonElement snapshot) => new(snapshot);

    public static ServerMonitoringAgentResult Unavailable() => new((JsonElement?)null);
}
