using Application.Common.Enumerable;
using Application.Services.CommonSrv.PushNotificationSrv;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ServerMonitoringAlerts
{
    public interface IServerAlertPushSender
    {
        Task SendAsync(IReadOnlyList<ServerAlertMessage> messages, CancellationToken cancellationToken);
    }

    /// <summary>
    /// هشدار سرور را فقط به دستگاه‌های FCM ادمین‌هایی می‌فرستد که اپ «Pastil Monitor» را نصب کرده‌اند
    /// (userAgent ثبت‌شده با <see cref="MonitorUserAgentPrefix"/> شروع می‌شود) تا اپ نماینده/مشتری
    /// هیچ‌وقت هشدار زیرساخت نگیرد. هیچ تغییر اسکیمایی لازم نیست.
    /// </summary>
    public sealed class ServerAlertPushSender : IServerAlertPushSender
    {
        public const string MonitorUserAgentPrefix = "Pastil-Monitor";
        public const string PushType = "server-alert";

        private readonly IDataBaseContext _context;
        private readonly IFcmSender _fcmSender;
        private readonly ILogger<ServerAlertPushSender> _logger;

        public ServerAlertPushSender(IDataBaseContext context, IFcmSender fcmSender, ILogger<ServerAlertPushSender> logger)
        {
            _context = context;
            _fcmSender = fcmSender;
            _logger = logger;
        }

        public async Task SendAsync(IReadOnlyList<ServerAlertMessage> messages, CancellationToken cancellationToken)
        {
            if (messages.Count == 0) return;

            if (!_fcmSender.IsConfigured)
            {
                _logger.LogWarning("Server alert push skipped: FCM is not configured.");
                return;
            }

            var subscriptions = await _context.PushSubscriptions
                .AsTracking()
                .Where(x => x.IsActive
                    && x.Provider == (long)PushProviderEnum.Fcm
                    && x.FcmToken != null
                    && x.UserAgent != null
                    && x.UserAgent.StartsWith(MonitorUserAgentPrefix)
                    && x.UserId.HasValue
                    && x.User.RoleId == (long)RoleEnum.Admin)
                .ToListAsync(cancellationToken);

            if (subscriptions.Count == 0)
            {
                _logger.LogInformation("Server alert push skipped: no Pastil Monitor device is registered.");
                return;
            }

            var expired = new List<Entities.Entities.PushSubscription>();
            foreach (var message in messages)
            {
                foreach (var subscription in subscriptions.Where(s => !expired.Contains(s)))
                {
                    var result = await _fcmSender.SendAsync(
                        subscription.FcmToken,
                        message.Title,
                        message.Body,
                        url: string.Empty,
                        icon: string.Empty,
                        // tag هر هشدار ثابت است: یادآوری و «برطرف شد» جای اعلان قبلی همان هشدار را می‌گیرند.
                        tag: "server-alert-" + message.Key,
                        notificationId: Guid.NewGuid().ToString(),
                        type: PushType);

                    if (result == PushSendResult.Success)
                        subscription.LastSeen = DateTime.UtcNow;
                    else if (result == PushSendResult.Expired)
                        expired.Add(subscription);
                }
            }

            if (expired.Count > 0)
                _context.PushSubscriptions.RemoveRange(expired);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
