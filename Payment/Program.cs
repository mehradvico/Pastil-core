using Application.Common.Security;
using Application.Common.Configuration;
using Application.Common.Helpers;
using Application.Configures;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Payment.Health;
using Persistence.Context;
using Persistence.Interface;
using System.Globalization;
using System.Threading.RateLimiting;
using Utility.Observability;

DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);
SecretConfiguration.Apply(builder.Configuration, "PASTIL_PAYMENT_CONNECTION");

var otlpEndpoint = builder.Configuration["Observability:OtlpEndpoint"];
if (string.IsNullOrWhiteSpace(otlpEndpoint))
    otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

builder.Services.AddPastilOpenTelemetry(
    serviceName: "pastil-payment",
    environmentName: builder.Environment.EnvironmentName,
    otlpEndpoint: otlpEndpoint,
    traceSampleRatio: builder.Configuration.GetValue<double?>("Observability:TraceSampleRatio") ?? 0.10);

builder.Services.AddControllersWithViews();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PaymentCallback", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});
builder.Services.AddDbContext<IDataBaseContext, DataBaseContext>(p => p.UseSqlServer(builder.Configuration["connection"], x => x.UseNetTopologySuite()));
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });
builder.Services.AddApplicationServices();
builder.Services.Configure<Application.Services.CommonSrv.PushNotificationSrv.FcmOptions>(
    builder.Configuration.GetSection(Application.Services.CommonSrv.PushNotificationSrv.FcmOptions.SectionName));
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // Persian text formatting, but invariant (dot-separator, Western digit)
    // number parsing/formatting - see Application/Configures/ConfigureServices.cs
    // for why: fa's own NumberFormat breaks parsing of hardcoded invariant
    // numeric strings, e.g. [Range(typeof(decimal), "0.01", ...)].
    var faCulture = new CultureInfo("fa");
    faCulture.NumberFormat = CultureInfo.InvariantCulture.NumberFormat;

    var supportedCultures = new List<CultureInfo>
                    {
                        faCulture,
                    };
    options.DefaultRequestCulture = new RequestCulture(faCulture, faCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;
});
builder.Services.AddControllers().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();


var app = builder.Build();
app.UseBackendSecurityHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApplication =>
    {
        errorApplication.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync("""
                <!doctype html>
                <html lang="fa" dir="rtl">
                <head><meta charset="utf-8"><title>خطای پرداخت</title></head>
                <body style="font-family:sans-serif;text-align:center;padding:48px;background:#111827;color:#fff">
                    <h2>خطایی هنگام پردازش پرداخت رخ داد</h2>
                    <p>لطفاً چند لحظه دیگر دوباره تلاش کنید.</p>
                </body>
                </html>
                """);
        });
    });
    app.UseHsts();
}
app.UseRequestLocalization();
AppSettingsHelper.Initialize(builder.Configuration);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();

app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthCheckResponseAsync
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponseAsync
}).AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Callback}/{action=Index}/{id?}");

app.Run();

static Task WriteHealthCheckResponseAsync(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json; charset=utf-8";
    return context.Response.WriteAsJsonAsync(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => new
            {
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description
            })
    }, cancellationToken: context.RequestAborted);
}
