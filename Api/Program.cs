using Api.HangFire;
using Api.Authorization;
using Api.Hubs;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistence.Context;
using Persistence.Interface;
using System.Text;
using Utility.BackgroundTask.Iface;
using Utility.ExternalRequest.Iface;
using Utility.ExternalRequest.Service;
using Utility.Reflection;
using Utility.Reflection.Iface;
using NetTopologySuite.IO.Converters;
using Application.Services.PastilAISrv.Provider;
using Application.Services.MemorySrvs.MemorySrv.Iface;
using Application.Services.ReminderSrvs.ReminderSrv.Iface;
using System.Threading.RateLimiting;

DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);
SecretConfiguration.Apply(
    builder.Configuration,
    "PASTIL_API_CONNECTION",
    includeVapidKeys: true);

builder.Services.AddOutputCache();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = 1;
});
builder.Services.AddRateLimiter(options =>
{
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
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddSignalR();
builder.Services.AddAuthorization(options => options.AddPolicy(PolicyNames.AdminOnly, policy => policy.RequireClaim("RoleId", ((long)RoleEnum.Admin).ToString())));
builder.Services
    .AddControllersWithViews()
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
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "https://panel.pastil.pet",
                "https://app.pastil.pet",
                "https://pastil.pet",
                "https://www.pastil.pet"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddDbContext<IDataBaseContext, DataBaseContext>(p => p.UseSqlServer(builder.Configuration["connection"], x => x.UseNetTopologySuite()));
builder.Services.AddApplicationServices();
builder.Services.Configure<PastilAiProviderOptions>(
    builder.Configuration.GetSection(PastilAiProviderOptions.SectionName));
builder.Services.Configure<Application.Services.CommonSrv.SearchSrv.SearchHybridOptions>(
    builder.Configuration.GetSection(Application.Services.CommonSrv.SearchSrv.SearchHybridOptions.SectionName));
builder.Services.AddScoped<INoticeRealtimePublisher, NoticeRealtimePublisher>();
builder.Services.AddScoped<IRestSharpApi, RestSharpApi>();
builder.Services.AddScoped<IBackgroundTask, HangFireSchedule>();
builder.Services.AddScoped<IControllerActionDiscoveryService, ControllerActionDiscoveryService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<VapidKeysOption>(
builder.Configuration.GetSection("VapidKeys"));
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
                     if (!string.IsNullOrWhiteSpace(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs/notices"))
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
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<INoticeService>("ArchiveNotices", x => x.ArchiveExpiredAsync(), Cron.Hourly);
var tehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
    OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");
recurringJobManager.AddOrUpdate<IMemoryService>(
    "MemoryDailyReminder",
    service => service.SendDailyReminderAsync(CancellationToken.None),
    Cron.Hourly,
    new RecurringJobOptions { TimeZone = tehranTimeZone });
recurringJobManager.AddOrUpdate<IReminderService>(
    "Reminder",
    service => service.SyncReminderAsync(),
    Cron.Hourly,
    new RecurringJobOptions { TimeZone = tehranTimeZone });
recurringJobManager.AddOrUpdate<Application.Services.CommonSrv.PushNotificationSrv.Iface.IPushNotificationService>(
    "PushNotificationDispatch",
    service => service.SendPushGroupAsync(100),
    "*/5 * * * *");

app.UseRequestLocalization();
app.UseHangfireDashboard();
if (app.Environment.IsDevelopment())
{

}
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowPanel");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseOutputCache();
app.MapControllers();
app.MapHub<NoticeHub>("/hubs/notices");
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
