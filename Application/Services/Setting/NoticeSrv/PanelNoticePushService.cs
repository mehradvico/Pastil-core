using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using WebPush;

namespace Application.Services.Setting.NoticeSrv
{
    public class PanelNoticePushService : IPanelNoticePushService
    {
        public const string PanelPlatform = "panel";
        private const int MaxUserAgentLength = 400;
        private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(15);

        private readonly IDataBaseContext _context;
        private readonly VapidKeysOption _vapid;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PanelNoticePushService> _logger;

        public PanelNoticePushService(IDataBaseContext context, IOptions<VapidKeysOption> vapid, IServiceScopeFactory scopeFactory, ILogger<PanelNoticePushService> logger)
        {
            _context = context;
            _vapid = vapid.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<BaseResultDto> SubscribeAsync(long userId, PushSubscribeDto dto)
        {
            if (userId <= 0 || dto == null || string.IsNullOrWhiteSpace(dto.Endpoint) || dto.Keys == null
                || string.IsNullOrWhiteSpace(dto.Keys.P256dh) || string.IsNullOrWhiteSpace(dto.Keys.Auth))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            // AsTracking اجباری: DataBaseContext سراسری NoTracking است (همان نکته‌ی PushSubscriptionService)
            var sub = await _context.PushSubscriptions.AsTracking().FirstOrDefaultAsync(x => x.Endpoint == dto.Endpoint);
            var userAgent = dto.UserAgent?.Length > MaxUserAgentLength ? dto.UserAgent[..MaxUserAgentLength] : dto.UserAgent;
            if (sub == null)
            {
                sub = new Entities.Entities.PushSubscription
                {
                    CreateDate = DateTime.UtcNow
                };
                _context.PushSubscriptions.Add(sub);
            }

            sub.UserId = userId;
            sub.DeviceKey = dto.DeviceKey == Guid.Empty ? null : dto.DeviceKey;
            sub.Provider = (long)PushProviderEnum.WebPush;
            sub.Platform = PanelPlatform;
            sub.Endpoint = dto.Endpoint;
            sub.P256dh = dto.Keys.P256dh;
            sub.Auth = dto.Keys.Auth;
            sub.UserAgent = userAgent;
            sub.IsActive = true;
            sub.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UnsubscribeAsync(long userId, string endpoint)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(endpoint))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var items = await _context.PushSubscriptions.AsTracking()
                .Where(x => x.Endpoint == endpoint && x.UserId == userId && x.Platform == PanelPlatform && x.IsActive)
                .ToListAsync();
            foreach (var item in items)
                item.IsActive = false;
            if (items.Count > 0)
                await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public void QueueNotice(long noticeId, string title, string message, string navigationUrl)
        {
            if (string.IsNullOrWhiteSpace(_vapid?.PublicKey) || string.IsNullOrWhiteSpace(_vapid?.PrivateKey))
                return;
            var scopeFactory = _scopeFactory;
            var logger = _logger;
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IPanelNoticePushService>();
                    await service.DeliverNoticeAsync(noticeId, title, message, navigationUrl);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Panel notice push failed for NoticeId {NoticeId}", noticeId);
                }
            });
        }

        public async Task DeliverNoticeAsync(long noticeId, string title, string message, string navigationUrl)
        {
            if (string.IsNullOrWhiteSpace(_vapid?.PublicKey) || string.IsNullOrWhiteSpace(_vapid?.PrivateKey))
                return;

            var subs = await _context.PushSubscriptions.AsTracking()
                .Where(x => x.IsActive && x.Provider == (long)PushProviderEnum.WebPush && x.Platform == PanelPlatform
                    && x.UserId != null && x.Endpoint != null && !x.User.Deleted)
                .ToListAsync();
            if (subs.Count == 0)
                return;

            var payload = BuildPayload(noticeId, title, message, navigationUrl);
            var vapid = new VapidDetails("mailto:admin@pastil.pet", _vapid.PublicKey, _vapid.PrivateKey);
            var client = new WebPushClient();

            // دستگاه‌ها هم‌زمان؛ یک endpoint کند/غیرقابل‌دسترس بقیه را عقب نمی‌اندازد
            var results = await Task.WhenAll(subs.Select(s => SendAsync(client, vapid, payload, s)));
            for (var i = 0; i < subs.Count; i++)
            {
                if (results[i] == SendResult.Success)
                    subs[i].LastSeen = DateTime.UtcNow;
                else if (results[i] == SendResult.Expired)
                    subs[i].IsActive = false;
            }
            await _context.SaveChangesAsync();
        }

        public static string BuildPayload(long noticeId, string title, string message, string navigationUrl)
        {
            var url = string.IsNullOrWhiteSpace(navigationUrl) || !navigationUrl.StartsWith("/") || navigationUrl.StartsWith("//")
                ? "/admin/notice"
                : navigationUrl;
            return JsonSerializer.Serialize(new
            {
                title = string.IsNullOrWhiteSpace(title) ? "اعلان جدید" : title,
                body = message ?? string.Empty,
                url,
                tag = $"notice-{noticeId}",
                icon = "/icons/icon-192.png"
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private enum SendResult { Success, Expired, Failed }

        private async Task<SendResult> SendAsync(WebPushClient client, VapidDetails vapid, string payload, Entities.Entities.PushSubscription subscription)
        {
            try
            {
                var target = new WebPush.PushSubscription(subscription.Endpoint, subscription.P256dh, subscription.Auth);
                using var timeout = new CancellationTokenSource(SendTimeout);
                await client.SendNotificationAsync(target, payload, vapid, timeout.Token);
                return SendResult.Success;
            }
            catch (WebPushException ex) when (ex.StatusCode == HttpStatusCode.Gone || ex.StatusCode == HttpStatusCode.NotFound)
            {
                return SendResult.Expired;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Panel notice push delivery failed for subscription {PushSubscriptionId}", subscription.Id);
                return SendResult.Failed;
            }
        }
    }
}
