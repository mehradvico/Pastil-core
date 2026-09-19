using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Persistence.Interface;

namespace Payment.Health;

public sealed class DatabaseHealthCheck(
    IDataBaseContext context,
    ILogger<DatabaseHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken = default)
    {
        if (context is not DbContext dbContext)
            return HealthCheckResult.Unhealthy("The configured database context is invalid.");

        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("The database is reachable.")
                : HealthCheckResult.Unhealthy("The database is not reachable.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Database readiness check failed.");
            return HealthCheckResult.Unhealthy("The database is not reachable.", exception);
        }
    }
}
