using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Common.Security
{
    public sealed class SecurityEvent
    {
        public string Name { get; set; }
        public bool IsFailure { get; set; }
        public long? UserId { get; set; }
        public string Ip { get; set; }
        public string Detail { get; set; }
        public string Path { get; set; }
    }

    /// <summary>
    /// قواعد هشدار (منطق خالص و قابل‌تست، بدون I/O). هر رویداد امنیتی به Evaluate داده می‌شود و در صورت رسیدن به آستانه پیام هشدار برمی‌گرداند.
    /// برای جلوگیری از سیل پیام، هر هشدار برای همان کلید (IP/کاربر) در بازه‌ی dedupe فقط یک بار صادر می‌شود.
    /// </summary>
    public sealed class SecurityAlertRules
    {
        private static readonly HashSet<string> AuthFlows = new(StringComparer.Ordinal) { "SignIn", "ChangePassword", "ResetPassword", "ChangeMobile" };

        private readonly Func<DateTime> _utcNow;
        private readonly Func<DateTime, DateTime> _toLocal;
        private readonly ConcurrentDictionary<string, List<DateTime>> _windows = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastAlert = new();

        public int AuthFailuresPerIp { get; set; } = 10;
        public int DeniedRequestsPerIp { get; set; } = 100;
        public int UserFailuresBeforePanelLogin { get; set; } = 3;
        public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan Dedupe { get; set; } = TimeSpan.FromMinutes(30);

        public SecurityAlertRules() : this(() => DateTime.UtcNow, ToTehran) { }

        public SecurityAlertRules(Func<DateTime> utcNow, Func<DateTime, DateTime> toLocal)
        {
            _utcNow = utcNow;
            _toLocal = toLocal;
        }

        public IReadOnlyList<string> Evaluate(SecurityEvent e)
        {
            var alerts = new List<string>();
            if (e == null || string.IsNullOrEmpty(e.Name))
                return alerts;

            var now = _utcNow();
            var ip = string.IsNullOrEmpty(e.Ip) ? "unknown" : e.Ip;

            if (e.Name == "RefreshTokenTheft")
            {
                Add(alerts, "theft:" + e.UserId, TimeSpan.FromMinutes(15),
                    $"سرقت/استفاده‌ی مجدد از refresh token کاربر {e.UserId} تشخیص داده شد و نشست‌ها باطل شد ({e.Detail}).");
            }

            if (e.IsFailure && AuthFlows.Contains(e.Name))
            {
                var count = Count("auth:" + ip, now);
                if (count >= AuthFailuresPerIp)
                    Add(alerts, "authip:" + ip, Dedupe, $"{count} تلاش ناموفق ورود/رمز/OTP از IP {ip} در {(int)Window.TotalMinutes} دقیقه (احتمال حمله‌ی brute force).");

                if (e.Detail == "password_throttled" && e.UserId.HasValue)
                    Add(alerts, "throttle:" + e.UserId, Dedupe, $"حساب کاربر {e.UserId} به‌خاطر تلاش‌های ناموفق رمز قفل موقت شد (IP {ip}).");

                if (e.UserId.HasValue)
                    Count("userfail:" + e.UserId, now, TimeSpan.FromMinutes(30));
            }

            if ((e.Name == "Http403" || e.Name == "Http429") && e.IsFailure)
            {
                var count = Count("denied:" + ip, now);
                if (count >= DeniedRequestsPerIp)
                    Add(alerts, "deniedip:" + ip, Dedupe, $"{count} پاسخ 403/429 برای IP {ip} در {(int)Window.TotalMinutes} دقیقه (اسکن یا سوءاستفاده).");
            }

            if (e.Name == "AdminWrite" && !e.IsFailure && e.UserId.HasValue)
            {
                var hour = _toLocal(now).Hour;
                if (hour < 6)
                    Add(alerts, "night:" + e.UserId, TimeSpan.FromMinutes(60), $"تغییر در پنل ادمین توسط کاربر {e.UserId} در ساعت غیرکاری ({e.Path}).");
            }

            if (e.Name == "SignIn" && !e.IsFailure && e.Detail == "panel" && e.UserId.HasValue)
            {
                var failures = Peek("userfail:" + e.UserId, now, TimeSpan.FromMinutes(30));
                if (failures >= UserFailuresBeforePanelLogin)
                    Add(alerts, "panelafterfail:" + e.UserId, Dedupe, $"ورود موفق به پنل برای کاربر {e.UserId} بعد از {failures} تلاش ناموفق اخیر (IP {ip}) — بررسی شود.");
            }

            return alerts;
        }

        private int Count(string key, DateTime now, TimeSpan? window = null)
        {
            var span = window ?? Window;
            var list = _windows.GetOrAdd(key, _ => new List<DateTime>());
            lock (list)
            {
                list.RemoveAll(t => now - t > span);
                list.Add(now);
                return list.Count;
            }
        }

        private int Peek(string key, DateTime now, TimeSpan span)
        {
            if (!_windows.TryGetValue(key, out var list))
                return 0;
            lock (list)
            {
                list.RemoveAll(t => now - t > span);
                return list.Count;
            }
        }

        private void Add(List<string> alerts, string key, TimeSpan dedupe, string message)
        {
            var now = _utcNow();
            if (_lastAlert.TryGetValue(key, out var last) && now - last < dedupe)
                return;

            _lastAlert[key] = now;
            alerts.Add(message);

            if (_lastAlert.Count > 20_000 || _windows.Count > 50_000)
            {
                _lastAlert.Clear();
                _windows.Clear();
            }
        }

        private static DateTime ToTehran(DateTime utc)
        {
            foreach (var id in new[] { "Iran Standard Time", "Asia/Tehran" })
            {
                try { return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById(id)); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return utc.AddHours(3.5);
        }
    }

    public interface ISecurityAlertService
    {
        void Observe(SecurityEvent securityEvent);
    }

    /// <summary>
    /// هشدار لحظه‌ای بدون وابستگی به Grafana: هر هشدار همیشه به‌صورت Critical در لاگ (category Security.Alert) ثبت می‌شود و اگر
    /// Security:AlertWebhookUrl (POST JSON {"text": "..."} — Slack/Mattermost/Discord-compatible یا relay) یا
    /// Security:AlertTelegramBotToken + Security:AlertTelegramChatId تنظیم باشد، فوراً ارسال هم می‌شود. خطای ارسال هیچ‌وقت جریان اصلی را نمی‌شکند.
    /// </summary>
    public sealed class SecurityAlertService : ISecurityAlertService
    {
        private readonly SecurityAlertRules _rules = new();
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SecurityAlertService(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger("Security.Alert");
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public void Observe(SecurityEvent securityEvent)
        {
            try
            {
                foreach (var message in _rules.Evaluate(securityEvent))
                {
                    _logger.LogCritical("SecurityAlert {Message}", message);
                    _ = Task.Run(() => SendAsync(message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Evaluating security alert rules failed.");
            }
        }

        private async Task SendAsync(string message)
        {
            var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "production";
            var text = $"🚨 پاستیل ({environment}): {message}";
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                var client = _httpClientFactory.CreateClient();

                var webhook = _configuration["Security:AlertWebhookUrl"];
                if (!string.IsNullOrWhiteSpace(webhook) && Uri.TryCreate(webhook, UriKind.Absolute, out var webhookUri) && webhookUri.Scheme == Uri.UriSchemeHttps)
                {
                    var body = new StringContent(JsonSerializer.Serialize(new { text }), Encoding.UTF8, "application/json");
                    await client.PostAsync(webhookUri, body, timeout.Token);
                }

                var token = _configuration["Security:AlertTelegramBotToken"];
                var chat = _configuration["Security:AlertTelegramChatId"];
                if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(chat))
                {
                    var body = new StringContent(JsonSerializer.Serialize(new { chat_id = chat, text }), Encoding.UTF8, "application/json");
                    await client.PostAsync($"https://api.telegram.org/bot{token}/sendMessage", body, timeout.Token);
                }
            }
            catch (Exception ex)
            {
                // آدرس/توکن در پیام خطا ممکن است باشد؛ فقط نوع خطا لاگ می‌شود
                _logger.LogWarning("Sending security alert failed: {ExceptionType}", ex.GetType().Name);
            }
        }
    }
}
