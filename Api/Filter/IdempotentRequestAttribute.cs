using Application.Common.Dto.Result;
using Application.Common.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Persistence.Interface;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Api.Filter
{
    /// <summary>
    /// جلوگیری از ثبت تکراری فرم‌های ایجاد (آدرس، پت، رزرو، تیکت، ...): در حساب تست دو آدرس کاملاً یکسان با دابل‌کلیک ثبت شده بود.
    /// - با هدر Idempotency-Key: همان کلید تا ۱۰ دقیقه همان پاسخ موفق اول را برمی‌گرداند (ثبت دوم انجام نمی‌شود).
    /// - بدون هدر: همان کاربر + همان مسیر + همان بدنه در چند ثانیه = تکراری؛ اگر اولی تمام شده پاسخش برمی‌گردد، اگر هنوز در جریان است پیام «تکراری».
    /// پاسخ ناموفق نگه داشته نمی‌شود تا تلاش دوباره ممکن باشد. فقط کاربر لاگین‌شده.
    /// کلید صریح (Idempotency-Key) در دیتابیس (IdempotencyRecords) ثبت می‌شود تا بین چند replica و بعد از restart هم پایدار باشد؛
    /// تشخیص «بدنه‌ی تکراری در چند ثانیه» (بدون هدر) همچنان در حافظه‌ی همین نمونه است چون عمداً کوتاه‌مدت است.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class IdempotentRequestAttribute : Attribute, IAsyncActionFilter
    {
        private const string HeaderName = "Idempotency-Key";
        private static readonly TimeSpan KeyedWindow = TimeSpan.FromMinutes(10);
        private static readonly object Gate = new();
        private static readonly JsonSerializerOptions HashJsonOptions = new() { WriteIndented = false };

        private readonly TimeSpan _bodyWindow;

        public IdempotentRequestAttribute(int duplicateWindowSeconds = 5)
        {
            _bodyWindow = TimeSpan.FromSeconds(Math.Max(1, duplicateWindowSeconds));
        }

        private sealed class Entry
        {
            public bool Completed { get; set; }
            public object Response { get; set; }
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
            long userId;
            try
            {
                userId = context.HttpContext.RequestServices.GetService<ICurrentUserHelper>()?.CurrentUser?.UserId ?? 0;
            }
            catch (Exception)
            {
                userId = 0;
            }
            if (cache == null || userId <= 0)
            {
                await next();
                return;
            }

            var request = context.HttpContext.Request;
            var path = request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            string key;
            TimeSpan window;
            if (request.Headers.TryGetValue(HeaderName, out var headerValues) && !string.IsNullOrWhiteSpace(headerValues.ToString()))
            {
                var headerKey = headerValues.ToString().Trim();
                if (headerKey.Length > 100)
                    headerKey = Hash(headerKey);
                var dbKey = $"idem:{userId}:{request.Method}:{path}:{headerKey}";
                if (dbKey.Length > 300)
                    dbKey = $"idem:{userId}:{Hash(dbKey)}";
                await HandleKeyedAsync(context, next, userId, dbKey);
                return;
            }
            else
            {
                string body;
                try
                {
                    body = JsonSerializer.Serialize(context.ActionArguments, HashJsonOptions);
                }
                catch (Exception)
                {
                    await next();
                    return;
                }
                key = $"dup:{userId}:{request.Method}:{path}:{Hash(body)}";
                window = _bodyWindow;
            }

            Entry existing = null;
            lock (Gate)
            {
                if (!cache.TryGetValue(key, out existing) || existing == null)
                {
                    existing = null;
                    cache.Set(key, new Entry(), new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = window, Size = 1 });
                }
            }

            if (existing != null)
            {
                context.Result = existing.Completed && existing.Response != null
                    ? new OkObjectResult(existing.Response)
                    : new OkObjectResult(new BaseResultDto(false, Resource.Notification.DuplicateRequest));
                return;
            }

            ActionExecutedContext executed = null;
            try
            {
                executed = await next();
            }
            finally
            {
                var value = (executed?.Result as ObjectResult)?.Value;
                var succeeded = executed?.Exception == null && value is BaseResultDto result && result.IsSuccess;
                lock (Gate)
                {
                    if (succeeded)
                        cache.Set(key, new Entry { Completed = true, Response = value }, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = window, Size = 1 });
                    else
                        cache.Remove(key);
                }
            }
        }

        private static readonly TimeSpan InProgressWindow = TimeSpan.FromMinutes(2);

        private static async Task HandleKeyedAsync(ActionExecutingContext context, ActionExecutionDelegate next, long userId, string key)
        {
            var services = context.HttpContext.RequestServices;
            // با scope جدا کار می‌کنیم تا تغییرات ردیابی‌شده‌ی خودِ درخواست با ثبت/به‌روزرسانی کلید قاطی نشود.
            await using var scope = services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<IDataBaseContext>();
            var now = DateTime.Now;

            await db.IdempotencyRecords.Where(r => r.Key == key && r.ExpireDate <= now).ExecuteDeleteAsync();
            try
            {
                db.IdempotencyRecords.Add(new Entities.Entities.IdempotencyRecord
                {
                    Key = key, UserId = userId, Completed = false, CreateDate = now, ExpireDate = now.Add(InProgressWindow),
                });
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var existing = await db.IdempotencyRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Key == key);
                if (existing == null)
                    throw;
                context.Result = existing.Completed && !string.IsNullOrEmpty(existing.ResponseJson)
                    ? new ContentResult { Content = existing.ResponseJson, ContentType = "application/json; charset=utf-8", StatusCode = 200 }
                    : new OkObjectResult(new BaseResultDto(false, Resource.Notification.DuplicateRequest));
                return;
            }

            ActionExecutedContext executed = null;
            try
            {
                executed = await next();
            }
            finally
            {
                var value = (executed?.Result as ObjectResult)?.Value;
                var succeeded = executed?.Exception == null && value is BaseResultDto result && result.IsSuccess;
                string json = null;
                if (succeeded)
                {
                    try
                    {
                        var jsonOptions = services.GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions;
                        json = JsonSerializer.Serialize(value, value.GetType(), jsonOptions);
                    }
                    catch (Exception)
                    {
                        succeeded = false;
                    }
                }
                if (succeeded)
                {
                    var expire = DateTime.Now.Add(KeyedWindow);
                    await db.IdempotencyRecords.Where(r => r.Key == key)
                        .ExecuteUpdateAsync(s => s.SetProperty(r => r.Completed, true).SetProperty(r => r.ResponseJson, json).SetProperty(r => r.ExpireDate, expire));
                }
                else
                {
                    await db.IdempotencyRecords.Where(r => r.Key == key).ExecuteDeleteAsync();
                }
            }
        }

        private static string Hash(string value) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)));
    }
}
