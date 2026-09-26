using Application.Common.Security;
using Api.HangFire;
using Api.Authorization;
using Api.Filters;
using Api.Health;
using Api.Hubs;
using Api.Middleware;
using Api.Swagger;
using Application.Common.Configuration;
using Application.Common.Enumerable;
using Application.Configures;
using Application.Services.Accounting.UserTokenSrv.Iface;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistence.Context;
using Persistence.Interface;
using System.Text;
using Utility.BackgroundTask.Iface;
using Utility.ExternalRequest.Iface;
using Utility.ExternalRequest.Service;
using Utility.Observability;
using Utility.Reflection;
using Utility.Reflection.Iface;
using NetTopologySuite.IO.Converters;
using Application.Services.PastilAISrv.Provider;
using Application.Services.ProductSrvs.AiProductMatchSrv;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.MemorySrvs.MemorySrv.Iface;
using Application.Services.ReminderSrvs.ReminderSrv.Iface;
using System.Threading.RateLimiting;

DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);
SecretConfiguration.Apply(
    builder.Configuration,
    "PASTIL_API_CONNECTION",
    includeVapidKeys: true);

var otlpEndpoint = builder.Configuration["Observability:OtlpEndpoint"];
if (string.IsNullOrWhiteSpace(otlpEndpoint))
    otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

builder.Services.AddPastilOpenTelemetry(
    serviceName: "pastil-api",
    environmentName: builder.Environment.EnvironmentName,
    otlpEndpoint: otlpEndpoint,
    traceSampleRatio: builder.Configuration.GetValue<double?>("Observability:TraceSampleRatio") ?? 0.10);

builder.Services.AddOutputCache();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    ForwardedHeadersTrust.Apply(options, builder.Configuration));
// Per-IP limits are only meaningful when the API can see each visitor's real IP.
// For webapp traffic that requires the attestation key (see BffClientIpMiddleware):
// without it every visitor arrives from the webapp server's address, so strict per-IP
// buckets would throttle the whole site at once. In that case only flood-level limits apply.
var perClientIpVisible = !string.IsNullOrWhiteSpace(builder.Configuration["Security:ClientIpAttestationKey"]);
int IpLimit(int strict, int relaxed) => perClientIpVisible ? strict : relaxed;

builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, cancellationToken) =>
    {
        int? retryAfterSeconds = null;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
            context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.Value.ToString();
        }

        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            isSuccess = false,
            messages = new[]
            {
                new { item1 = "تعداد درخواست‌ها زیاد است. لطفاً کمی بعد دوباره تلاش کنید." }
            },
            retryAfterSeconds
        }, cancellationToken);
    };
    options.AddPolicy("ServerMonitoring", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                // A mobile client polling every three seconds consumes 20
                // requests per minute. This leaves room for short retries
                // while containing an accidentally aggressive client.
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    options.AddPolicy("Search", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    options.AddPolicy("ContactUs", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    options.AddPolicy("AccountSignIn", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    options.AddPolicy("AccountRecovery", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    options.AddPolicy("OtpSend", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new TokenBucketRateLimiterOptions
            {
                // The web client has a two-minute resend timer. A small initial burst
                // tolerates a lost response, then only one new SMS may be requested
                // every two minutes from the same client IP.
                TokenLimit = IpLimit(3, 300),
                TokensPerPeriod = IpLimit(1, 150),
                ReplenishmentPeriod = TimeSpan.FromMinutes(2),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    options.AddPolicy("OtpVerify", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new TokenBucketRateLimiterOptions
            {
                // The database-level failed-code lock still protects an individual
                // recipient; this limiter prevents a single client from brute-forcing
                // the endpoint or consuming API capacity.
                TokenLimit = IpLimit(6, 300),
                TokensPerPeriod = IpLimit(3, 150),
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    options.AddPolicy("AccountLookup", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new TokenBucketRateLimiterOptions
            {
                // userdetail/userrole reveal whether a mobile number has an account and
                // what role it holds. The login form needs one call per attempt, so a
                // small burst plus a slow refill is plenty for a person but makes
                // walking a list of numbers impractical.
                TokenLimit = IpLimit(20, 300),
                TokensPerPeriod = IpLimit(10, 150),
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    // refresh token یک credential بلندمدت است؛ حدس/تکرار انبوه آن باید محدود باشد (کلاینت واقعی حداکثر چند refresh در دقیقه دارد)
    options.AddPolicy("RefreshToken", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = IpLimit(30, 3000),
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    // استعلام عمومی کد قلاده شماره‌ی مالک را برمی‌گرداند؛ حدس‌زدن انبوه کدها باید عملاً غیرممکن باشد.
    options.AddPolicy("PetTagLookup", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = IpLimit(20, 2000),
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    options.AddPolicy("MapSearch", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = IpLimit(120, 6000),
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    options.AddPolicy("TripPrice", httpContext =>
    {
        var userId = httpContext.User.FindFirst("UserId")?.Value;
        var partitionKey = !string.IsNullOrWhiteSpace(userId)
            ? $"user:{userId}"
            : $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey,
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 6,
                TokensPerPeriod = 3,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });
    options.AddPolicy("AiProductMatch", httpContext =>
    {
        var userId = httpContext.User.FindFirst("UserId")?.Value;
        var partitionKey = !string.IsNullOrWhiteSpace(userId)
            ? $"ai-product-match:user:{userId}"
            : $"ai-product-match:ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey,
            _ => new TokenBucketRateLimiterOptions
            {
                // This is an expensive multimodal provider call. Six attempts are
                // enough for a seller to retry a failed import without permitting
                // a continuous account-level cost attack.
                TokenLimit = 6,
                TokensPerPeriod = 1,
                ReplenishmentPeriod = TimeSpan.FromMinutes(2),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });
    // سقف سراسری برای هر IP (پشتوانه‌ی policyهای اختصاصی): endpointهایی که policy جدا ندارند هم بی‌نهایت قابل‌فراخوانی نباشند.
    // health و SignalR معاف‌اند؛ سقف عمداً بالاست تا کاربر واقعی هیچ‌وقت به آن نخورد. بدون ClientIpAttestationKey همه‌ی کاربران وب‌اپ یک IP دارند،
    // پس فقط سقف «سیل» اعمال می‌شود (همان منطق IpLimit).
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var path = httpContext.Request.Path;
        if (path.StartsWithSegments("/health") || path.StartsWithSegments("/hubs"))
            return RateLimitPartition.GetNoLimiter("exempt");

        return RateLimitPartition.GetSlidingWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = IpLimit(600, 30000),
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddSignalR();
builder.Services.AddSingleton<CallSessionTracker>();
builder.Services.AddSingleton<CallDurationRecorder>();
builder.Services.AddSingleton<CallWindowScheduler>();
builder.Services.AddSingleton<Api.Services.AiProductMatch.AiProductMatchExecutionGate>();
builder.Services.AddHttpClient(Api.Services.ServerMonitoring.ServerMonitoringAgentClient.HttpClientName,
    client => client.Timeout = TimeSpan.FromSeconds(5));
builder.Services.AddScoped<Api.Services.ServerMonitoring.ServerMonitoringAgentClient>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Api.Authorization.AdminAreaAuthorizationHandler>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Api.Authorization.AreaMembershipAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.AdminOnly, policy => policy.RequireClaim("RoleId", ((long)RoleEnum.Admin).ToString()));
    options.AddPolicy(PolicyNames.AdminArea, policy => policy.RequireAuthenticatedUser().AddRequirements(new Api.Authorization.AdminAreaRequirement()));
    foreach (var membershipArea in new[] { "Seller", "Companion", "Driver" })
        options.AddPolicy(PolicyNames.AreaMember(membershipArea), policy => policy.RequireAuthenticatedUser().AddRequirements(new Api.Authorization.AreaMembershipRequirement(membershipArea)));
});
builder.Services
    .AddControllersWithViews(options =>
    {
        options.Conventions.Add(new AdminAreaAuthorizationConvention());
        // endpoint بدون احراز هویت هیچ‌وقت اطلاعات شخصی کاربران دیگر را برنمی‌گرداند (ScrubAnonymousUserPiiFilter)
        options.Filters.Add<ScrubAnonymousUserPiiFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new GeoJsonConverterFactory()
        );
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPanel", policy =>
    {
        // origin محلی (localhost) همراه با credentials فقط در توسعه؛ در production فقط دامنه‌های واقعی.
        var panelOrigins = new List<string>
        {
            "https://panel.pastil.pet",
            "https://app.pastil.pet",
            "https://pastil.pet",
            "https://www.pastil.pet"
        };
        if (builder.Environment.IsDevelopment())
        {
            panelOrigins.Add("http://localhost:3000");
            panelOrigins.Add("http://localhost:3001");
        }
        policy
            .WithOrigins(panelOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddDbContext<IDataBaseContext, DataBaseContext>(p => p.UseSqlServer(
    builder.Configuration["connection"],
    x => x.UseNetTopologySuite().MigrationsAssembly(typeof(DataBaseContext).Assembly.FullName)));
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });
builder.Services.AddApplicationServices();
builder.Services.Configure<PastilAiProviderOptions>(
    builder.Configuration.GetSection(PastilAiProviderOptions.SectionName));
builder.Services.Configure<AiProductMatchOptions>(
    builder.Configuration.GetSection(AiProductMatchOptions.SectionName));
builder.Services.Configure<Application.Services.ProductSrvs.ProductImageEnhanceSrv.ProductImageEnhanceOptions>(
    builder.Configuration.GetSection(Application.Services.ProductSrvs.ProductImageEnhanceSrv.ProductImageEnhanceOptions.SectionName));
builder.Services.Configure<Application.Services.CommonSrv.SearchSrv.SearchHybridOptions>(
    builder.Configuration.GetSection(Application.Services.CommonSrv.SearchSrv.SearchHybridOptions.SectionName));
builder.Services.AddScoped<INoticeRealtimePublisher, NoticeRealtimePublisher>();
builder.Services.AddScoped<ISignalRPushSender, SignalRPushSender>();
builder.Services.AddScoped<IRestSharpApi, RestSharpApi>();
builder.Services.AddScoped<IBackgroundTask, HangFireSchedule>();
builder.Services.AddScoped<IControllerActionDiscoveryService, ControllerActionDiscoveryService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<VapidKeysOption>(
builder.Configuration.GetSection("VapidKeys"));
builder.Services.Configure<Application.Services.CommonSrv.PushNotificationSrv.FcmOptions>(
    builder.Configuration.GetSection(Application.Services.CommonSrv.PushNotificationSrv.FcmOptions.SectionName));
builder.Services.AddSwaggerGen(c =>
{
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "MehradVico.Api.xml"), true);
    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Pastil.Api",
        Version = "v2",
    });
    var security = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(security.Reference.Id, security);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { security, Array.Empty<string>() }
    });

    c.OperationFilter<AddRequiredHeaderParameter>();
    c.SchemaFilter<AddSwaggerSchemaFilter>();
    c.SchemaFilter<EnumSchemaFilter>();
    c.DocumentFilter<AlphabeticalTagsDocumentFilter>();
});
builder.Services.AddAuthentication(Options =>
{
    Options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    Options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    Options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
         .AddJwtBearer(configureOptions =>
         {
             configureOptions.TokenValidationParameters = new TokenValidationParameters()
             {
                 ValidIssuer = builder.Configuration["JWtConfig:issuer"],
                 ValidAudience = builder.Configuration["JWtConfig:audience"],
                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWtConfig:key"] ?? throw new InvalidOperationException("JWT signing key is not configured."))),
                 ValidateIssuerSigningKey = true,
                 ValidateLifetime = true,
                 ValidateIssuer = true,
                 ValidateAudience = true,
                 RequireExpirationTime = true,
                 RequireSignedTokens = true,
                 // فقط HS256 (الگوریتمی که خودمان با آن امضا می‌کنیم)؛ الگوریتم‌های دیگر/none رد می‌شوند
                 ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                 // پیش‌فرض ۵ دقیقه بود و هر توکن را عملاً ۵ دقیقه بیشتر زنده نگه می‌داشت؛ بررسی دقیق‌تر انقضا در دیتابیس (CheckUser) هم هست
                 ClockSkew = TimeSpan.FromSeconds(30),

             };
             configureOptions.SaveToken = true;
             configureOptions.Events = new JwtBearerEvents
             {
                 OnAuthenticationFailed = context =>
                 {
                     var tokenValidatorService = context.HttpContext.RequestServices.GetRequiredService<IOnTokenNotValidService>();
                     return tokenValidatorService.Execute(context);

                 },
                 OnTokenValidated = context =>
                 {
                     var tokenValidatorService = context.HttpContext.RequestServices.GetRequiredService<IOnTokenValidatedService>();
                     return tokenValidatorService.Execute(context);

                 },
                 OnChallenge = context =>
                 {
                     var tokenValidatorService = context.HttpContext.RequestServices.GetRequiredService<IOnTokenChallenge>();
                     return tokenValidatorService.Execute(context);
                 },
                 OnMessageReceived = context =>
                 {
                     var accessToken = context.Request.Query["access_token"];
                     if (!string.IsNullOrWhiteSpace(accessToken) &&
                         (context.HttpContext.Request.Path.StartsWithSegments("/hubs/notices") ||
                          context.HttpContext.Request.Path.StartsWithSegments("/hubs/call") ||
                          context.HttpContext.Request.Path.StartsWithSegments("/hubs/push")))
                         context.Token = accessToken;
                     return Task.CompletedTask;

                 },
                 OnForbidden = context =>
                 {
                     return Task.CompletedTask;

                 }
             };

         });



builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddHangfire(configuration => configuration
       .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseSqlServerStorage(builder.Configuration["connection"], new SqlServerStorageOptions
       {
           CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
           SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
           QueuePollInterval = TimeSpan.Zero,
           UseRecommendedIsolationLevel = true,
           DisableGlobalLocks = true
       }));

builder.Services.AddHangfireServer();

var app = builder.Build();
app.UseBackendSecurityHeaders();
app.UseUnhandledExceptionResult();
Application.Common.Helpers.ExceptionResultHelper.Initialize(app.Services.GetRequiredService<ILoggerFactory>());

// پرچم‌های تست فقط هشدار می‌دهند (رفتار را عوض نمی‌کنند): اگر روی سرور غیر Development روشن بمانند، در لاگ
// استارتاپ دیده می‌شوند تا «پرداخت/ارسال تستی» بی‌خبر وارد لانچ نشود.
if (Application.Common.Security.JwtKeyPolicy.IsWeak(app.Configuration["JWtConfig:key"]))
    app.Logger.LogWarning("JWtConfig:key is shorter than {Bytes} bytes; use a random key of at least 32 bytes (a short HS256 key can be brute-forced offline from any issued token).", Application.Common.Security.JwtKeyPolicy.RecommendedMinimumBytes);

if (!app.Environment.IsDevelopment())
{
    if (app.Configuration.GetValue<bool>("PaymentTestMode:Enabled"))
        app.Logger.LogWarning("PaymentTestMode:Enabled is TRUE outside Development — payments are simulated, no real gateway is charged.");
    if (app.Configuration.GetValue<bool>("Shipping:TestMode"))
        app.Logger.LogWarning("Shipping:TestMode is TRUE outside Development — shipment quotes/orders are simulated, no real courier is booked.");
}
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<INoticeService>("ArchiveNotices", x => x.ArchiveExpiredAsync(), Cron.Hourly);
recurringJobManager.AddOrUpdate<Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Iface.IConsultationPurchaseService>(
    "ConsultationExpireOverdue", x => x.ExpireOverdueAsync(), Cron.Minutely);
recurringJobManager.AddOrUpdate<Application.Services.ConsultationSrvs.ConsultationSessionSrv.Iface.IConsultationSessionService>(
    "ConsultationCompleteExpired", x => x.CompleteExpiredAsync(), Cron.Minutely);
recurringJobManager.AddOrUpdate<Application.Services.ConsultationSrvs.ConsultationNotificationSrv.Iface.IConsultationNotificationService>(
    "ConsultationNotifyPurchases", x => x.NotifyPendingPurchasesAsync(), Cron.Minutely);
recurringJobManager.AddOrUpdate<Application.Services.ConsultationSrvs.ConsultationNotificationSrv.Iface.IConsultationNotificationService>(
    "ConsultationEndingSoon", x => x.NotifyEndingSoonAsync(), Cron.Minutely);
recurringJobManager.AddOrUpdate<Application.Services.ConsultationSrvs.ConsultationNotificationSrv.Iface.IConsultationNotificationService>(
    "ConsultationNotifyUnclaimed", x => x.NotifyUnclaimedAsync(), Cron.Minutely);
var tehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
    OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");
// Removed: memory-reminder push is now sent from the panel's own push
// message flow instead of this backend job — see PushMessage/PushBroadcastSrv.
recurringJobManager.RemoveIfExists("MemoryDailyReminder");
recurringJobManager.AddOrUpdate<IReminderService>(
    "Reminder",
    service => service.SyncReminderAsync(),
    Cron.Minutely,
    new RecurringJobOptions { TimeZone = tehranTimeZone });
recurringJobManager.AddOrUpdate<Application.Services.CommonSrv.PushNotificationSrv.Iface.IPushNotificationService>(
    "PushNotificationDispatch",
    service => service.SendPushGroupAsync(100),
    "*/5 * * * *");
recurringJobManager.AddOrUpdate<Application.Services.TripSrv.TripSrv.Iface.ITripService>(
    "DispatchScheduledTrips",
    service => service.DispatchScheduledTripsAsync(),
    "* * * * *");
recurringJobManager.AddOrUpdate<Application.Services.TripSrv.TripSrv.Iface.ITripService>(
    "AutoCancelUnansweredInstantTrips",
    service => service.AutoCancelUnansweredInstantTripsAsync(),
    "* * * * *");
// هشدار زودهنگام (قبل از لغو خودکار بالا): وقتی سفر فوری چند دقیقه است ثبت شده و هنوز راننده‌ای قبول نکرده،
// به کاربر پوش می‌دهد که ادمین در جریان است - بدون لغو سفر.
recurringJobManager.AddOrUpdate<Application.Services.TripSrv.TripSrv.Iface.ITripService>(
    "NotifyAdminForSlowInstantTrips",
    service => service.NotifyAdminForSlowInstantTripsAsync(),
    "* * * * *");
// توکن‌های منقضی‌شده (بیش از ۳۰ روز بعد از انقضای refresh) پاک می‌شوند؛ جدول UserTokens قبلاً هیچ‌وقت کوچک نمی‌شد
recurringJobManager.AddOrUpdate<Application.Services.Accounting.UserTokenSrv.Iface.IUserTokenService>(
    "PurgeExpiredUserTokens",
    service => service.PurgeExpiredAsync(),
    "30 3 * * *",
    new RecurringJobOptions { TimeZone = tehranTimeZone });
// رمزنگاری رکوردهای قدیمی کارت بانکی (idempotent؛ بدون کلید کاری نمی‌کند). یک بار هم بلافاصله بعد از استارتاپ اجرا می‌شود.
recurringJobManager.AddOrUpdate<Application.Services.FinanceSrvs.UserBankCardSrv.Iface.IUserBankCardProtectionService>(
    "ProtectBankCardData",
    service => service.ProtectExistingAsync(),
    "40 3 * * *",
    new RecurringJobOptions { TimeZone = tehranTimeZone });
if (Persistence.Security.SensitiveDataProtector.IsConfigured)
    Hangfire.BackgroundJob.Enqueue<Application.Services.FinanceSrvs.UserBankCardSrv.Iface.IUserBankCardProtectionService>(service => service.ProtectExistingAsync());
else
    app.Logger.LogWarning("Security:BankCardEncryptionKey (PASTIL_BANKCARD_ENCRYPTION_KEY) is not configured: bank card numbers and sheba are stored WITHOUT encryption.");
recurringJobManager.AddOrUpdate<Application.Services.Accounting.UserPetSrv.Iface.IUserPetService>(
    "PetBirthdayPush",
    service => service.SendBirthdayPushesAsync(CancellationToken.None),
    "0 0 * * *",
    new RecurringJobOptions { TimeZone = tehranTimeZone });
recurringJobManager.AddOrUpdate<Application.Services.CommonSrv.PushBroadcastSrv.Iface.IPushScheduleService>(
    "PushMessageScheduleDispatch",
    service => service.DispatchDueAsync(CancellationToken.None),
    "*/5 * * * *",
    new RecurringJobOptions { TimeZone = tehranTimeZone });
recurringJobManager.AddOrUpdate<Application.Services.SchoolSrvs.SchoolReserveSrv.Iface.ISchoolReserveService>(
    "SchoolClassStartingPush",
    service => service.SendClassStartingPushesAsync(CancellationToken.None),
    "*/5 * * * *",
    new RecurringJobOptions { TimeZone = tehranTimeZone });
// هر شب ساعت ۲۰، سفرهای «فردا»ی سرویس‌های پت‌رسان هفتگی رو می‌سازه — تا صبح فردا ادمین وقت
// تخصیص راننده داشته باشه (TripChooseDriver در پنل).
recurringJobManager.AddOrUpdate<Application.Services.TripSrv.TripSrv.Iface.ITripService>(
    "GeneratePetResanServiceTrips",
    service => service.GeneratePetResanServiceTripsAsync(),
    "0 20 * * *",
    new RecurringJobOptions { TimeZone = tehranTimeZone });

// جاب‌های دسته‌ی دوم (SMS زمان‌بندی‌شده، بستن تیکت، انقضای تخفیف، یادآوری عدم‌پذیرش راننده) قبلاً فقط با یک
// فراخوانی دستی GET api/BackgroundTask بعد از هر deploy ثبت می‌شدند؛ حالا در استارتاپ ثبت می‌شوند.
using (var jobScope = app.Services.CreateScope())
{
    var backgroundTask = jobScope.ServiceProvider.GetRequiredService<IBackgroundTask>();
    backgroundTask.StartSyncSmsAsync().GetAwaiter().GetResult();
    backgroundTask.StartSyncCloseTicketAsync().GetAwaiter().GetResult();
    backgroundTask.StartSyncExpiredDiscountAsync().GetAwaiter().GetResult();
    backgroundTask.StartSyncDriverAcceptAsync().GetAwaiter().GetResult();
}

app.UseRequestLocalization();
if (app.Environment.IsDevelopment())
{

}
app.UseForwardedHeaders();
app.UseMiddleware<BffClientIpMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowPanel");
app.UseAuthentication();
app.UseMiddleware<Api.Middleware.SecurityAuditMiddleware>();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new AdminDashboardAuthorizationFilter()]
});
app.UseRateLimiter();
app.UseOutputCache();
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
app.MapControllers();
app.MapHub<NoticeHub>("/hubs/notices");
app.MapHub<CallHub>("/hubs/call");
app.MapHub<PushHub>("/hubs/push");
app.UseSwaggerAccessControl();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Pastil API v2");
    options.DocExpansion(
        Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None
    );
    options.DefaultModelsExpandDepth(-1);
    options.DefaultModelExpandDepth(-1);
    options.EnableFilter();
    options.EnableDeepLinking();
});
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
