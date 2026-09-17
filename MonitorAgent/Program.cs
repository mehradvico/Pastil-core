using System.Security.Cryptography;
using System.Text;
using MonitorAgent.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<HostSnapshotService>();

var agentToken = builder.Configuration["PASTIL_MONITOR_AGENT_TOKEN"];
if (string.IsNullOrWhiteSpace(agentToken))
{
    throw new InvalidOperationException(
        "PASTIL_MONITOR_AGENT_TOKEN must be configured before the monitoring agent starts.");
}

var app = builder.Build();

// This service is only attached to the internal Docker network. It has no
// public port; the API service is its only caller and supplies this token.
app.MapGet("/snapshot", async (
    HttpRequest request,
    HostSnapshotService snapshotService,
    CancellationToken cancellationToken) =>
{
    var suppliedToken = request.Headers["X-Pastil-Monitor-Token"].ToString();
    if (!TokensMatch(agentToken, suppliedToken))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await snapshotService.CollectAsync(cancellationToken));
});

app.Run();

static bool TokensMatch(string expected, string supplied)
{
    var expectedBytes = Encoding.UTF8.GetBytes(expected);
    var suppliedBytes = Encoding.UTF8.GetBytes(supplied);

    return expectedBytes.Length == suppliedBytes.Length &&
           CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
}
