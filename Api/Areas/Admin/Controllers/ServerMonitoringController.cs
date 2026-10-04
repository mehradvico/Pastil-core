using Api.Authorization;
using Application.Common.Interface;
using Application.Common.Security;
using System.Text.RegularExpressions;
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
public sealed partial class ServerMonitoringController : ControllerBase
{
    private static readonly string[] ControlActions = ["start", "stop", "restart"];

    private readonly ServerMonitoringAgentClient _monitoringAgent;
    private readonly ICurrentUserHelper _currentUser;
    private readonly ISecurityAudit _audit;

    public ServerMonitoringController(
        ServerMonitoringAgentClient monitoringAgent,
        ICurrentUserHelper currentUser,
        ISecurityAudit audit)
    {
        _monitoringAgent = monitoringAgent;
        _currentUser = currentUser;
        _audit = audit;
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

    /// <summary>All containers (running or stopped) and whether control is enabled on the agent.</summary>
    [HttpGet("containers")]
    [EnableRateLimiting("ServerMonitoring")]
    public async Task<IActionResult> GetContainers(CancellationToken cancellationToken)
    {
        var result = await _monitoringAgent.ListContainersAsync(cancellationToken);
        if (result is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Server monitoring is temporarily unavailable."
            });
        }

        return Ok(result.Value);
    }

    public sealed record ContainerControlRequest(string? Confirm);

    /// <summary>
    /// Starts, stops or restarts one container. The body must echo the container name in
    /// <c>confirm</c> (guards against a mis-targeted or replayed call). Every attempt is audited.
    /// </summary>
    [HttpPost("containers/{name}/{action}")]
    [EnableRateLimiting("ServerControl")]
    public async Task<IActionResult> ControlContainer(
        string name,
        string action,
        [FromBody] ContainerControlRequest request,
        CancellationToken cancellationToken)
    {
        action = action.ToLowerInvariant();
        var userId = _currentUser.CurrentUser?.UserId;
        var subject = $"{name}:{action}";

        if (!ControlActions.Contains(action) || !ContainerNameRegex().IsMatch(name) ||
            !string.Equals(request?.Confirm, name, StringComparison.Ordinal))
        {
            _audit.Failure("ServerContainerControl", userId, subject, "invalid-request");
            return BadRequest(new { message = "Invalid container control request." });
        }

        var (status, outcome) = await _monitoringAgent.ControlContainerAsync(name, action, cancellationToken);
        if (status is >= 200 and < 300)
        {
            _audit.Success("ServerContainerControl", userId, subject, outcome);
            return Ok(new { success = true, outcome });
        }

        _audit.Failure("ServerContainerControl", userId, subject, outcome ?? $"agent-status-{status}");
        return StatusCode(status switch
        {
            404 => StatusCodes.Status404NotFound,
            403 => StatusCodes.Status403Forbidden,
            0 => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status502BadGateway
        }, new { success = false, outcome });
    }

    [GeneratedRegex("^[a-zA-Z0-9][a-zA-Z0-9_.-]{0,127}$")]
    private static partial Regex ContainerNameRegex();
}
