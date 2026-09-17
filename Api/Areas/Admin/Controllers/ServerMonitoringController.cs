using Api.Authorization;
using Api.Services.ServerMonitoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Areas.Admin.Controllers;

/// <summary>
/// Read-only, live host and Docker statistics for the operations monitor.
/// The endpoint is restricted to administrator JWTs; it never exposes the
/// private agent token used to read the host.
/// </summary>
[Area("Admin")]
[Route("api/[area]/server-monitoring")]
[ApiController]
[Authorize(Policy = PolicyNames.AdminOnly)]
public sealed class ServerMonitoringController : ControllerBase
{
    private readonly ServerMonitoringAgentClient _monitoringAgent;

    public ServerMonitoringController(ServerMonitoringAgentClient monitoringAgent)
    {
        _monitoringAgent = monitoringAgent;
    }

    /// <summary>
    /// Gets a current server snapshot. Intended for a low-frequency (for
    /// example, every three seconds) authenticated monitoring poll.
    /// </summary>
    [HttpGet]
    [EnableRateLimiting("ServerMonitoring")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _monitoringAgent.GetSnapshotAsync(cancellationToken);
        if (!result.IsAvailable)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Server monitoring is temporarily unavailable."
            });
        }

        return Ok(result.Snapshot!.Value);
    }
}
