using Api.HangFire;
using Application.Common.Configuration;
using Application.Configures;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistence.Context;
using Persistence.Interface;
using RealTime.Swagger;
using System.Text;
using Utility.BackgroundTask.Iface;
using Utility.ExternalRequest.Iface;
using Utility.ExternalRequest.Service;
using Utility.Reflection;
using Utility.Reflection.Iface;

DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);
SecretConfiguration.Apply(builder.Configuration, "PASTIL_REALTIME_CONNECTION");

builder.Services.AddOutputCache();
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(5);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(10);
}); builder.Services.AddDbContext<IDataBaseContext, DataBaseContext>(p => p.UseSqlServer(builder.Configuration["connection"], x => x.UseNetTopologySuite()));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["JWtConfig:issuer"],
            ValidAudience = builder.Configuration["JWtConfig:audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWtConfig:Key"])),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddApplicationServices();
builder.Services.AddScoped<IRestSharpApi, RestSharpApi>();
builder.Services.AddScoped<IBackgroundTask, HangFireSchedule>();
builder.Services.AddScoped<IControllerActionDiscoveryService, ControllerActionDiscoveryService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{

    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "ZandShop.RealTime.xml"), true);
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ZandShop.RealTime.xml", Version = "v1" });
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
    c.OperationFilter<AddRequiredHeaderParameter>();
    c.SchemaFilter<AddSwaggerSchemaFilter>();
    //c.SchemaFilter<EnumSchemaFilter>();

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

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseRequestLocalization();
app.UseHangfireDashboard();

if (app.Environment.IsDevelopment())
{
}

app.UseStaticFiles();
app.UseCors("AllowAnyOrigin");
app.UseHttpsRedirection();


app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});

app.UseOutputCache();

app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseAuthentication();
app.UseAuthorization();
//app.MapHub<DriverHub>("/driverHub").RequireAuthorization();


app.Run();
