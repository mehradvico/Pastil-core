using Application.Services.ServerMonitoringAlerts;
using Microsoft.Extensions.Options;

namespace Api.Services.ServerMonitoring;

/// <summary>
/// هر چند ثانیه یک‌بار snapshot عامل مانیتورینگ را می‌خواند، با <see cref="ServerAlertEvaluator"/> بررسی
/// می‌کند و در صورت لزوم هشدار پوش به اپ Pastil Monitor می‌فرستد. وضعیت هشدارها فقط در حافظه است؛
/// با ری‌استارت API، هشدارهای فعال دوباره از صفر شمرده می‌شوند.
/// </summary>
public sealed class ServerAlertBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServerAlertOptions _options;
    private readonly ServerAlertEvaluator _evaluator;
    private readonly ILogger<ServerAlertBackgroundService> _logger;

    public ServerAlertBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<ServerAlertOptions> options,
        ILogger<ServerAlertBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _evaluator = new ServerAlertEvaluator(_options);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Server alerts are disabled (ServerMonitoring:Alerts:Enabled=false).");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(10, _options.PollSeconds));
        using var timer = new PeriodicTimer(interval);
        do
        {
            try
            {
                await CheckOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                // یک بررسی ناموفق نباید حلقه‌ی هشدار را بکشد.
                _logger.LogError(exception, "Server alert check failed.");
            }
        }
        while (await WaitAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task CheckOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<ServerMonitoringAgentClient>();
        var result = await client.GetSnapshotAsync(cancellationToken);

        var messages = result.IsAvailable
            ? _evaluator.Evaluate(ServerSample.FromJson(result.Snapshot!.Value), DateTime.UtcNow)
            : _evaluator.EvaluateUnavailable(DateTime.UtcNow);

        if (messages.Count == 0) return;

        _logger.LogWarning("Server alerts: {Alerts}",
            string.Join("; ", messages.Select(m => $"{m.Kind}:{m.Key}")));
        await scope.ServiceProvider.GetRequiredService<IServerAlertPushSender>()
            .SendAsync(messages, cancellationToken);
    }
}
