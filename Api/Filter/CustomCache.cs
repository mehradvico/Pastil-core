using Application.Common.Enumerable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

public class CustomOutputCacheAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _key;

    public CustomOutputCacheAttribute(CacheTypeEnum key)
    {
        _key = key.ToString();
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // دسترسی به سرویس‌ها از طریق HttpContext.RequestServices
        var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
        var cache = context.HttpContext.RequestServices.GetService<IMemoryCache>();

        if (configuration == null || cache == null)
        {
            await next(); // اگر سرویس‌ها موجود نبودند، اکشن را ادامه دهید
            return;
        }

        // خواندن مدت زمان کش از تنظیمات
        // DisableAll همه را خاموش می‌کند مگر کلید در CacheSettings:EnabledKeys باشد (مثلاً فقط ProductSearch با مدت کوتاه)
        if (!Application.Common.Helpers.OutputCachePolicy.IsEnabled(configuration, _key))
        {
            await next();
            return;
        }
        int duration = configuration.GetValue<int>($"CacheSettings:Durations:{_key}");
        if (duration < 1)
        {
            duration = configuration.GetValue<int>($"CacheSettings:Durations:DefaultAll");
        }
        if (duration < 1)
        {
            await next(); // اگر سرویس‌ها موجود نبودند، اکشن را ادامه دهید
            return;
        }
        if (duration == -1)
        {
            await next();
            return;
        }
        // تولید یک کلید یکتا برای کش بر اساس مسیر و کوئری استرینگ
        string cacheKey = $"{_key}-{context.HttpContext.Request.Path}-{context.HttpContext.Request.QueryString}-{context.HttpContext.Request.Headers["Accept-Language"]}";

        if (cache.TryGetValue(cacheKey, out string? cachedResponse) && cachedResponse != null)
        {
            // اگر داده در کش موجود باشد، مستقیماً آن را باز می‌گردانیم
            context.HttpContext.Response.ContentType = "application/json"; // مثال: JSON
            await context.HttpContext.Response.WriteAsync(cachedResponse);
            return;
        }

        // ادامه اکشن و گرفتن نتیجه
        var executedContext = await next();

        if (executedContext.Result is ObjectResult result)
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = new CamelCaseNamingPolicy(),
                WriteIndented = false // اختیاری: برای زیباتر کردن JSON
            };

            string responseBody = System.Text.Json.JsonSerializer.Serialize(result.Value, options);
            // MemoryCache با SizeLimit ثبت شده؛ ورودی بدون Size خطا می‌دهد (روشن کردن CacheSettings قبلاً همین‌جا می‌شکست).
            // Size به حجم پاسخ بستگی دارد و پاسخ خیلی بزرگ کش نمی‌شود تا حافظه‌ی سرور پر نشود.
            if (Application.Common.Helpers.OutputCachePolicy.ShouldStore(responseBody.Length))
            {
                cache.Set(cacheKey, responseBody, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(duration),
                    Size = Application.Common.Helpers.OutputCachePolicy.EntrySize(responseBody.Length)
                });
            }
        }
    }
    public class CamelCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length < 2)
                return name.ToLower();

            // تبدیل نام به camelCase
            return char.ToLower(name[0]) + name.Substring(1);
        }
    }
}
