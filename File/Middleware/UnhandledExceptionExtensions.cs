using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace File.Middleware
{
    /// <summary>
    /// هندلر سراسری خطاهای مدیریت‌نشده: به‌جای صفحه‌ی خطای خام فریمورک (که جزئیات داخلی
    /// را لو می‌دهد و با قرارداد پاسخ‌های برنامه نمی‌خواند) همان شکل استاندارد
    /// {IsSuccess, Code, Messages, Data} را با وضعیت 500 برمی‌گرداند. جزئیات خطا فقط در لاگ سرور می‌ماند.
    /// </summary>
    public static class UnhandledExceptionExtensions
    {
        public static IApplicationBuilder UseUnhandledExceptionResult(this IApplicationBuilder app)
        {
            return app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerFeature>();
                    if (feature?.Error != null)
                    {
                        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                            .CreateLogger("UnhandledException");
                        logger.LogError(feature.Error,
                            "Unhandled exception on {Method} {Path}",
                            context.Request.Method, context.Request.Path.Value);
                    }

                    if (context.Response.HasStarted)
                        return;

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json; charset=utf-8";
                    var body = JsonSerializer.Serialize(new
                    {
                        Code = 0,
                        IsSuccess = false,
                        Messages = new[] { new { Item1 = Resource.Notification.SomethingWentWrong, Item2 = "" } },
                        Data = (object?)null
                    });
                    await context.Response.WriteAsync(body);
                });
            });
        }
    }
}
