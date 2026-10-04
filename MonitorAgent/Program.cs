using System.Security.Cryptography;
using System.Text;
using MonitorAgent.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<HostSnapshotService>();
builder.Services.AddSingleton<ContainerControlService>();

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

// Container control (opt-in via MONITOR_CONTROL_ENABLED). Same token check as /snapshot.
app.MapGet("/containers", async (
    HttpRequest request,
    ContainerControlService control,
    CancellationToken cancellationToken) =>
{
    if (!TokensMatch(agentToken, request.Headers["X-Pastil-Monitor-Token"].ToString()))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        controlEnabled = control.Enabled,
        containers = await control.ListAsync(cancellationToken)
    });
});

app.MapPost("/containers/{name}/{action}", async (
    string name,
    string action,
    HttpRequest request,
    ContainerControlService control,
    CancellationToken cancellationToken) =>
{
    if (!TokensMatch(agentToken, request.Headers["X-Pastil-Monitor-Token"].ToString()))
    {
        return Results.Unauthorized();
    }

    var result = await control.ExecuteAsync(name, action, cancellationToken);
    var body = new { success = result.Success, outcome = result.Outcome.ToString() };
    return result.Success
        ? Results.Ok(body)
        : Results.Json(body, statusCode: result.Outcome switch
        {
            ControlOutcome.NotFound => StatusCodes.Status404NotFound,
            ControlOutcome.DockerError => StatusCodes.Status502BadGateway,
            ControlOutcome.Invalid => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status403Forbidden
        });
});

app.Run();

static bool TokensMatch(string expected, string supplied)
{
    var expectedBytes = Encoding.UTF8.GetBytes(expected);
    var suppliedBytes = Encoding.UTF8.GetBytes(supplied);

    return expectedBytes.Length == suppliedBytes.Length &&
           CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
}
