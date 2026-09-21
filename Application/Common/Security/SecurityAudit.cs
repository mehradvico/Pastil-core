using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Common.Security
{
    /// <summary>
    /// نقطه‌ی واحد ثبت رویدادهای امنیتی (ورود، قفل، تغییر رمز/موبایل، سرقت توکن، ...).
    /// همه‌ی رویدادها با category ثابت <c>Security.Audit</c> و فیلدهای ساخت‌یافته لاگ می‌شوند تا در OTLP/لاگ‌سرور
    /// فقط با یک فیلتر (و برای alert روی Warning) قابل استفاده باشند. هیچ‌وقت رمز، کد OTP، توکن یا بدنه‌ی درخواست لاگ نمی‌شود.
    /// </summary>
    public interface ISecurityAudit
    {
        void Success(string eventName, long? userId = null, string subject = null, string detail = null);
        void Failure(string eventName, long? userId = null, string subject = null, string detail = null);
        void Observed(string eventName, long? userId = null, string subject = null, string detail = null);
    }

    public sealed class SecurityAudit : ISecurityAudit
    {
        public const string Category = "Security.Audit";

        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISecurityAlertService _alerts;

        public SecurityAudit(ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor, ISecurityAlertService alerts = null)
        {
            _logger = loggerFactory.CreateLogger(Category);
            _httpContextAccessor = httpContextAccessor;
            _alerts = alerts;
        }

        public void Success(string eventName, long? userId = null, string subject = null, string detail = null) =>
            Write(LogLevel.Information, eventName, "success", userId, subject, detail);

        public void Failure(string eventName, long? userId = null, string subject = null, string detail = null) =>
            Write(LogLevel.Warning, eventName, "failure", userId, subject, detail);

        public void Observed(string eventName, long? userId = null, string subject = null, string detail = null) =>
            Write(LogLevel.Information, eventName, "observed", userId, subject, detail);

        private void Write(LogLevel level, string eventName, string outcome, long? userId, string subject, string detail)
        {
            try
            {
                var context = _httpContextAccessor?.HttpContext;
                _logger.Log(
                    level,
                    "SecurityEvent {Event} {Outcome} User={UserId} Subject={Subject} Ip={Ip} Method={Method} Path={Path} Detail={Detail}",
                    eventName,
                    outcome,
                    userId,
                    MaskIdentifier(subject),
                    context?.Connection?.RemoteIpAddress?.ToString(),
                    context?.Request?.Method,
                    context?.Request?.Path.Value,
                    detail);

                _alerts?.Observe(new SecurityEvent
                {
                    Name = eventName,
                    IsFailure = outcome == "failure",
                    UserId = userId,
                    Ip = context?.Connection?.RemoteIpAddress?.ToString(),
                    Detail = detail,
                    Path = context?.Request?.Path.Value
                });
            }
            catch
            {
                // ثبت رویداد امنیتی هرگز نباید جریان اصلی را بشکند
            }
        }

        /// <summary>موبایل/ایمیل را برای لاگ ماسک می‌کند: 09121234567 → 0912***4567 ، a@b.com → a***@b.com</summary>
        public static string MaskIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var text = value.Trim();
            var at = text.IndexOf('@');
            if (at > 0)
                return text[0] + "***" + text.Substring(at);

            if (text.Length <= 6)
                return "***";

            return text.Substring(0, 4) + "***" + text.Substring(text.Length - 4);
        }
    }
}
