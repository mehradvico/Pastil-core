using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Utility.Observability;

#nullable enable

public static class OpenTelemetryConfiguration
{
    /// <summary>
    /// Registers vendor-neutral APM telemetry.  An OTLP collector is opt-in so a
    /// missing monitoring configuration cannot affect checkout or trip flows.
    /// </summary>
    public static IServiceCollection AddPastilOpenTelemetry(
        this IServiceCollection services,
        string serviceName,
        string environmentName,
        string? otlpEndpoint,
        double traceSampleRatio)
    {
        var endpoint = ParseOtlpEndpoint(otlpEndpoint);
        var sampleRatio = Math.Clamp(traceSampleRatio, 0, 1);

        var telemetry = services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment.name"] = environmentName
                }));

        telemetry.WithTracing(tracing =>
        {
            tracing
                .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(sampleRatio)))
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    // Probe traffic is high-volume and not useful for diagnosing
                    // customer requests. The endpoint remains observable through
                    // its HTTP status and collector-side availability checks.
                    options.Filter = context => !context.Request.Path.StartsWithSegments("/health");
                })
                .AddHttpClientInstrumentation(options => options.RecordException = true)
                // The instrumentation's default avoids query-parameter capture;
                // duration, outcome, and dependency identity are retained for
                // database diagnosis without exporting parameter values.
                .AddSqlClientInstrumentation(options =>
                {
                    options.RecordException = true;
                });

            if (endpoint is not null)
                tracing.AddOtlpExporter(options => options.Endpoint = endpoint);
        });

        telemetry.WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();

            if (endpoint is not null)
                metrics.AddOtlpExporter(options => options.Endpoint = endpoint);
        });

        return services;
    }

    private static Uri? ParseOtlpEndpoint(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Uri.TryCreate(value, UriKind.Absolute, out var endpoint) &&
            (endpoint.Scheme == Uri.UriSchemeHttp || endpoint.Scheme == Uri.UriSchemeHttps))
            return endpoint;

        // A malformed monitoring URL must not prevent the customer-facing API
        // from starting. The deployment log still makes the misconfiguration clear.
        Console.Error.WriteLine("OpenTelemetry is disabled because the OTLP endpoint is not a valid HTTP(S) URI.");
        return null;
    }
}
