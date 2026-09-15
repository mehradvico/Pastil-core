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
        // شناسه‌ی کانال نوتیفیکیشن اندروید - اپ فلاتر باید دقیقاً همین کانال را بسازد.
        public const string AndroidChannelId = "pastil_default";

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
            string tag,
            string notificationId = null,
            string type = null)
        {
            var app = ResolveApp();
            if (app == null || string.IsNullOrWhiteSpace(fcmToken))
                return PushSendResult.TransientFailure;

            // تمام مقدارهای data باید string باشند (الزام FCM).
            var data = new Dictionary<string, string>
            {
                ["title"] = title ?? string.Empty,
                ["body"] = body ?? string.Empty,
                ["url"] = url ?? string.Empty,
                ["icon"] = icon ?? string.Empty,
                ["tag"] = tag ?? string.Empty,
                ["notificationId"] = notificationId ?? string.Empty,
                ["type"] = type ?? string.Empty
            };

            // تصویر فقط وقتی به FCM داده می‌شود که یک URL مطلق http/https معتبر باشد.
            // URL نامعتبر باعث InvalidArgument از سمت FCM می‌شود و آن هم در این کلاس
            // به‌عنوان Expired تفسیر شده و توکن سالم کاربر را حذف می‌کند.
            var hasImageUrl = Uri.TryCreate(icon, UriKind.Absolute, out var iconUri) &&
                (iconUri.Scheme == Uri.UriSchemeHttp || iconUri.Scheme == Uri.UriSchemeHttps);

            var apns = new ApnsConfig
            {
                Aps = new Aps
                {
                    // ContentAvailable در کنار alert نگه داشته می‌شود تا هندلر داده‌ای اپ هم بیدار شود.
                    ContentAvailable = true,
                    Sound = "default",
                    MutableContent = true
                }
            };

            if (!string.IsNullOrWhiteSpace(tag))
            {
                // معادل iOS برای رفتار tag در وب‌پوش: نوتیفیکیشن تکراری جایگزین قبلی می‌شود.
                apns.Headers = new Dictionary<string, string>
                {
                    ["apns-collapse-id"] = tag.Length > 64 ? tag.Substring(0, 64) : tag
                };
            }

            var message = new Message
            {
                Token = fcmToken,
                // هم Notification و هم Data فرستاده می‌شود:
                //  • Notification باعث می‌شود سیستم‌عامل وقتی اپ در پس‌زمینه یا بسته است
                //    خودش نوتیفیکیشن را نمایش دهد. قبلاً فقط Data فرستاده می‌شد و نتیجه‌اش
                //    این بود که روی iOS هیچ‌وقت چیزی نمایش داده نمی‌شد (پیام data-only یک
                //    silent notification است) و روی اندروید هم فقط اگر خودِ اپ نوتیفیکیشن
                //    محلی می‌ساخت دیده می‌شد - یعنی «نوتیف توی اپ نمیاد».
                //  • Data برای deep-link (url) و نمایش سفارشی داخل اپ در پیش‌زمینه باقی می‌ماند.
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                    ImageUrl = hasImageUrl ? icon : null
                },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        // این کانال باید در خود اپ فلاتر با همین شناسه ساخته شده باشد؛
                        // در غیر این صورت روی اندروید ۸+ نوتیفیکیشن با تنظیمات پیش‌فرض
                        // (یا بی‌صدا) نمایش داده می‌شود. نگاه کنید به docs/ai/PUSH_MOBILE_SETUP.md
                        ChannelId = AndroidChannelId,
                        Sound = "default",
                        Tag = string.IsNullOrWhiteSpace(tag) ? null : tag,
                        ImageUrl = hasImageUrl ? icon : null
                    }
                },
                Apns = apns
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
