using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Geography.Services
{
    /// <summary>
    /// Bounds OSRM work for this API instance. It intentionally fails fast when OSRM is saturated so
    /// ASP.NET request threads do not accumulate behind an unbounded upstream call.
    /// </summary>
    public sealed class OsrmRequestCoordinator : IDisposable
    {
        private readonly RestClient _client;
        private readonly SemaphoreSlim _concurrency;
        private readonly TimeSpan _queueTimeout;
        private readonly TimeSpan _requestTimeout;
        private readonly ILogger<OsrmRequestCoordinator> _logger;

        public OsrmRequestCoordinator(IConfiguration configuration, ILogger<OsrmRequestCoordinator> logger)
        {
            _logger = logger;
            _concurrency = new SemaphoreSlim(GetBoundedValue(configuration, "Osrm:MaxConcurrentRequests", 24, 1, 200));
            _queueTimeout = TimeSpan.FromMilliseconds(GetBoundedValue(configuration, "Osrm:QueueTimeoutMilliseconds", 750, 0, 5000));
            _requestTimeout = TimeSpan.FromSeconds(GetBoundedValue(configuration, "Osrm:RequestTimeoutSeconds", 6, 1, 30));
            _client = new RestClient(new RestClientOptions(configuration["Osrm:BaseUrl"] ?? "http://osrm:5000")
            {
                Timeout = _requestTimeout
            });
        }

        public Task<RestResponse> ExecuteOsrmAsync(RestRequest request) =>
            ExecuteAsync(cancellationToken => _client.ExecuteAsync(request, cancellationToken));

        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation)
        {
            if (!await _concurrency.WaitAsync(_queueTimeout))
            {
                _logger.LogWarning("OSRM request rejected because the local concurrency limit is saturated.");
                throw new GeographyDependencyUnavailableException("OSRM is currently saturated.");
            }

            try
            {
                using var timeout = new CancellationTokenSource(_requestTimeout);
                try
                {
                    return await operation(timeout.Token);
                }
                catch (OperationCanceledException exception) when (timeout.IsCancellationRequested)
                {
                    _logger.LogWarning(exception, "OSRM request timed out after {TimeoutSeconds} seconds.", _requestTimeout.TotalSeconds);
                    throw new GeographyDependencyUnavailableException("OSRM did not respond in time.", exception);
                }
            }
            finally
            {
                _concurrency.Release();
            }
        }

        private static int GetBoundedValue(IConfiguration configuration, string key, int fallback, int minimum, int maximum)
        {
            var configured = configuration.GetValue<int?>(key) ?? fallback;
            return Math.Clamp(configured, minimum, maximum);
        }

        public void Dispose()
        {
            _client.Dispose();
            _concurrency.Dispose();
        }
    }
}
