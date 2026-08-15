using Application.Common.Configuration;
using Application.Common.Helpers;
using Application.Configures;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interface;
using System.Globalization;
using System.Threading.RateLimiting;

DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);
SecretConfiguration.Apply(builder.Configuration, "PASTIL_PAYMENT_CONNECTION");

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
builder.Services.AddApplicationServices();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("fa"),
                    };
    options.DefaultRequestCulture = new RequestCulture("fa", "fa");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;
});
builder.Services.AddControllers().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();


var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Callback}/{action=Index}/{id?}");

app.Run();
