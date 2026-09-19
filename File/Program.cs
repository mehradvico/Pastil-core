using Application.Common.Configuration;
using Application.Configures;
using Application.Common.Enumerable;
using File.Middleware;
using Application.Services.Accounting.UserTokenSrv.Iface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistence.Context;
using Persistence.Interface;
using System.Text;
using System.Threading.RateLimiting;
using Utility.Observability;

DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);
SecretConfiguration.Apply(builder.Configuration, "PASTIL_FILE_CONNECTION");

var otlpEndpoint = builder.Configuration["Observability:OtlpEndpoint"];
if (string.IsNullOrWhiteSpace(otlpEndpoint))
    otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

builder.Services.AddPastilOpenTelemetry(
    serviceName: "pastil-file",
    environmentName: builder.Environment.EnvironmentName,
    otlpEndpoint: otlpEndpoint,
    traceSampleRatio: builder.Configuration.GetValue<double?>("Observability:TraceSampleRatio") ?? 0.10);

var allowedCorsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?.Where(origin => Uri.TryCreate(origin, UriKind.Absolute, out _))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray()
    ?? [
        "http://localhost:3000",
        "http://localhost:3001",
        "https://panel.pastil.pet",
        "https://app.pastil.pet",
        "https://pastil.pet",
        "https://www.pastil.pet"
    ];

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MehradVico.Api", Version = "v1" });
    var security = new OpenApiSecurityScheme
    {
        Name = "JWT Auth",
        Description = Resource.Notification.PleaseEnterTheToken,
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
                    { security , new string[]{ } }
                });


});
builder.Services.AddControllers().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();
builder.Services.AddDbContext<IDataBaseContext, DataBaseContext>(p => p.UseSqlServer(builder.Configuration["connection"], x => x.UseNetTopologySuite()));
builder.Services.AddHealthChecks()
    .AddCheck<File.Health.DatabaseHealthCheck>("database", tags: ["ready"]);
builder.Services.AddApplicationServices();

builder.Services.AddAuthorization(options => options.AddPolicy(
    "AdminOnly",
    policy => policy.RequireClaim("RoleId", ((long)RoleEnum.Admin).ToString())));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("PictureUpload", context =>
        RateLimitPartition.GetTokenBucketLimiter(
            UploadPartitionKey(context),
            _ => new TokenBucketRateLimiterOptions
            {
                // The AI table-import flow can upload 40 screenshots in one request.
                // Leave room for that legitimate burst, while preventing an account
                // from continuously consuming image processing capacity.
                TokenLimit = 45,
                TokensPerPeriod = 9,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                AutoReplenishment = true,
                QueueLimit = 0
            }));
    options.AddPolicy("FileUpload", context =>
        RateLimitPartition.GetTokenBucketLimiter(
            UploadPartitionKey(context),
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 8,
                TokensPerPeriod = 4,
                ReplenishmentPeriod = TimeSpan.FromMinutes(5),
                AutoReplenishment = true,
                QueueLimit = 0
            }));
});

builder.Services.AddCors(options => options.AddPolicy("TrustedOrigins", policy => policy
    .WithOrigins(allowedCorsOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

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
                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWtConfig:Key"] ?? throw new InvalidOperationException("JWT signing key is not configured."))),
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
                     //log
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
                     return Task.CompletedTask;

                 },
                 OnForbidden = context =>
                 {
                     return Task.CompletedTask;

                 }
             };

         });
builder.Services.Configure<FormOptions>(x =>
{
    // Individual upload actions may set a smaller ceiling. The global cap is
    // a safe fallback for every future multipart endpoint.
    x.ValueLengthLimit = 1024 * 1024;
    x.MultipartBodyLengthLimit = 80 * 1024 * 1024;
});

var app = builder.Build();
app.UseUnhandledExceptionResult();
Application.Common.Helpers.ExceptionResultHelper.Initialize(app.Services.GetRequiredService<ILoggerFactory>());


if (app.Environment.IsDevelopment())
{

}
else
{
    app.UseHsts();
}
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("TrustedOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
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
app.UseSwaggerAccessControl();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});
app.Run();

static string UploadPartitionKey(HttpContext context)
{
    var userId = context.User.FindFirst("UserId")?.Value;
    return !string.IsNullOrWhiteSpace(userId)
        ? $"user:{userId}"
        : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
}

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
