using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushNotificationSrv
{
    public class FcmSender : IFcmSender
    {
        private static readonly object InitLock = new();
        private static FirebaseApp _app;
        private static bool _initAttempted;

        private readonly FcmOptions _options;
        private readonly ILogger<FcmSender> _logger;

        public FcmSender(IOptions<FcmOptions> options, ILogger<FcmSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public bool IsConfigured => ResolveApp() != null;

        // FirebaseApp.Create باید فقط یک‌بار در طول عمر پروسه صدا زده شود (SDK خودش برای فراخوانی
        // دوباره Exception می‌دهد)؛ چون چند نمونه از FcmSender ممکن است ساخته شوند (Scoped)، این بخش
        // با یک قفل ساده و پرچم "قبلاً تلاش شد" محافظت می‌شود تا هم Thread-safe باشد و هم اگر تنظیم
        // نامعتبر بود، هر بار دوباره تلاش بی‌فایده برای Parse نکند.
        private FirebaseApp ResolveApp()
        {
            if (_app != null || _initAttempted)
                return _app;

            lock (InitLock)
            {
                if (_app != null || _initAttempted)
                    return _app;

                _initAttempted = true;

                if (string.IsNullOrWhiteSpace(_options.ServiceAccountJson))
                {
                    _logger.LogWarning("FCM is not configured (Fcm:ServiceAccountJson / PASTIL_FCM_SERVICE_ACCOUNT_JSON is empty); push to Flutter apps will be skipped.");
                    return null;
                }

                try
                {
                    var credential = GoogleCredential.FromJson(_options.ServiceAccountJson);
                    _app = FirebaseApp.Create(new AppOptions { Credential = credential }, "pastil-fcm");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize FirebaseApp from Fcm:ServiceAccountJson; push to Flutter apps will be skipped.");
                    _app = null;
                }

                return _app;
            }
        }

        public async Task<PushSendResult> SendAsync(
            string fcmToken,
            string title,
            string body,
            string url,
            string icon,
            string tag)
        {
            var app = ResolveApp();
            if (app == null || string.IsNullOrWhiteSpace(fcmToken))
                return PushSendResult.TransientFailure;

            var data = new Dictionary<string, string>
            {
                ["title"] = title ?? string.Empty,
                ["body"] = body ?? string.Empty,
                ["url"] = url ?? string.Empty,
                ["icon"] = icon ?? string.Empty,
                ["tag"] = tag ?? string.Empty
            };

            var message = new Message
            {
                Token = fcmToken,
                // فقط Data می‌فرستیم (نه Notification payload) تا اپ فلاتر خودش، حتی وقتی
                // در پیش‌زمینه است، تصمیم بگیرد نوتیفیکیشن را چطور نمایش دهد؛ روی iOS این یعنی
                // اپ باید یک Background Notification Handler ثبت کند وگرنه در پس‌زمینه نمایش داده نمی‌شود
                // (نگاه کنید به یادداشت‌های پیاده‌سازی فلاتر برای APNs content-available).
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        ContentAvailable = true,
                        Sound = "default"
                    }
                }
            };

            try
            {
                await FirebaseMessaging.GetMessaging(app).SendAsync(message);
                return PushSendResult.Success;
            }
            catch (FirebaseMessagingException exception) when (
                exception.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                exception.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
            {
                _logger.LogInformation(exception, "Removing invalid/unregistered FCM token.");
                return PushSendResult.Expired;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "FCM delivery failed.");
                return PushSendResult.TransientFailure;
            }
        }
    }
}
