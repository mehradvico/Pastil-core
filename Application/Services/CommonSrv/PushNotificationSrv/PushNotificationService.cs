using AngleSharp.Dom;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebPush;

namespace Application.Services.CommonSrv.PushNotificationSrv
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IDataBaseContext _context;
        private readonly VapidKeysOption _vapid;
        private readonly IFcmSender _fcmSender;
        private readonly ISignalRPushSender _signalRPushSender;
        private readonly ILogger<PushNotificationService> _logger;
        private const int MaxAttemptCount = 3;
        // برخی الگوهای Push (مثل PushMemoryReminder) هیچ‌وقت مقدار Icon نداشتن (NULL در دیتابیس)
        // و صرفاً به fallback سمت service worker وب‌اپ متکی بودن؛ اگه یه نسخه‌ی قدیمی‌تر sw.js
        // (که هنوز آپدیت نشده روی گوشی کاربر) اون fallback رو نداشته باشه، آیکون واقعی پاستیل
        // اصلاً نمایش داده نمی‌شه و اندروید یه آواتار حرف اول دامنه می‌سازه. برای دفاع در عمق،
        // همیشه یه مقدار پیش‌فرض معتبر از همینجا هم می‌فرستیم.
        private const string DefaultPushIcon = "/icon-192x192.png";

        public PushNotificationService(
            IDataBaseContext context,
            IOptions<VapidKeysOption> vapid,
            IFcmSender fcmSender,
            ISignalRPushSender signalRPushSender,
            ILogger<PushNotificationService> logger)
        {
            _context = context;
            _vapid = vapid.Value;
            _fcmSender = fcmSender;
            _signalRPushSender = signalRPushSender;
            _logger = logger;
        }

        public async Task SendPushAsync(
            string pushPatternLabel,
            long userId,
            string token1 = null,
            string token2 = null,
            string token3 = null,
            string token4 = null,
            string token5 = null,
            DateTime? sendDate = null)
        {

            if (string.IsNullOrWhiteSpace(pushPatternLabel))
                return;

            var pushType = await _context.PushTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Label == pushPatternLabel);
            if (pushType == null)
                return;

            await SendPushAsync((PushTypeEnum)pushType.Id, userId, token1, token2, token3, token4, token5, sendDate);
        }

        public async Task SendPushAsync(
            PushTypeEnum pushType,
            long userId,
            string token1 = null,
            string token2 = null,
            string token3 = null,
            string token4 = null,
            string token5 = null,
            DateTime? sendDate = null)
        {
            var pattern = await GetActivePatternAsync(pushType);
            if (pattern == null)
                return;

            var notif = new PushNotification
            {
                UserId = userId,
                PushPatternId = pattern.Id,
                IsSend = false,
                Status = null,
                StatusText = null,
                CreateDate = DateTime.Now,
                SendDate = sendDate,
                AttemptCount = 0,
                NextAttemptDate = null,

                Token1 = token1,
                Token2 = token2,
                Token3 = token3,
                Token4 = token4,
                Token5 = token5
            };

            await _context.PushNotifications.AddAsync(notif);
            await _context.SaveChangesAsync();

            if (sendDate == null)
                await SendSingleAsync(pattern, notif);
        }

        public async Task SendPushGroupAsync(int pageSize = 100)
        {
            var now = DateTime.Now;

            var items = await _context.PushNotifications
                .Include(x => x.PushPattern)
                .Where(x =>
                    x.IsSend == false &&
                    x.Status == null &&
                    (x.SendDate == null || x.SendDate.Value <= now) &&
                    (x.NextAttemptDate == null || x.NextAttemptDate.Value <= now))
                .OrderBy(x => x.Id)
                .Take(pageSize)
                .AsTracking()
                .ToListAsync();

            if (items.Count == 0)
                return;

            foreach (var group in items.GroupBy(x => x.PushPatternId))
            {
                var pattern = group.First().PushPattern;

                if (pattern == null || !pattern.IsActive || !await IsEnabledAsync(pattern.Id))
                {
                    foreach (var n in group)
                        await MarkFailedAsync(n, Resource.Notification.SettingsAreNotComplete);

                    continue;
                }

                foreach (var n in group)
                    await SendSingleAsync(pattern, n);
            }
        }

        public async Task SendNoticeToAdminsAsync(long noticeId, string title, string body, string url)
        {
            title = PersianPushTextHelper.EnsurePersian(title, PersianPushTextHelper.DefaultTitle);
            body = PersianPushTextHelper.EnsurePersian(body, PersianPushTextHelper.DefaultBody);

            var subscriptions = await _context.PushSubscriptions.Include(x => x.User).Where(x => x.IsActive && x.UserId.HasValue && x.User.RoleId == (long)RoleEnum.Admin).AsTracking().ToListAsync();
            if (subscriptions.Count == 0)
                return;
            var payload = JsonSerializer.Serialize(new PushPayloadDto { Title = title, Body = body, Url = url, Tag = $"notice-{noticeId}" }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var client = new WebPushClient();
            var vapid = new VapidDetails("mailto:admin@pastil.pet", _vapid.PublicKey, _vapid.PrivateKey);
            var invalidSubscriptions = new List<Entities.Entities.PushSubscription>();
            foreach (var subscription in subscriptions)
            {
                var result = await TrySendAsync(client, vapid, payload, subscription);
                if (result == PushSendResult.Success)
                    subscription.LastSeen = DateTime.UtcNow;
                else if (result == PushSendResult.Expired)
                    invalidSubscriptions.Add(subscription);
            }
            if (invalidSubscriptions.Count > 0)
                _context.PushSubscriptions.RemoveRange(invalidSubscriptions);
            await _context.SaveChangesAsync();
        }

        private async Task<PushPattern> GetActivePatternAsync(PushTypeEnum pushType)
        {
            var pushTypeId = (long)pushType;

            var pattern = await _context.PushPatterns
                .Include(x => x.PushType)
                .AsTracking()
                .FirstOrDefaultAsync(x => x.PushTypeId == pushTypeId && x.IsActive);

            if (pattern == null)
                return null;

            var enabled = await IsEnabledAsync(pattern.Id);
            return enabled ? pattern : null;
        }

        private async Task<bool> IsEnabledAsync(long pushPatternId)
        {
            var setting = await _context.PushSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PushPatternId == pushPatternId);

            return setting == null || setting.IsEnabled;
        }

        private async Task SendSingleAsync(PushPattern pattern, PushNotification notif)
        {
            notif.IsSend = true;
            notif.AttemptCount++;
            notif.NextAttemptDate = null;
            _context.PushNotifications.Update(notif);
            await _context.SaveChangesAsync();

            try
            {
                var titleTpl = PersianPushTextHelper.ResolvePattern(pattern.Title, PersianPushTextHelper.DefaultTitle);
                var bodyTpl = PersianPushTextHelper.ResolvePattern(pattern.Body, PersianPushTextHelper.DefaultBody);

                var title = FormatTokens(titleTpl, notif.Token1, notif.Token2, notif.Token3, notif.Token4, notif.Token5);
                var body = FormatTokens(bodyTpl, notif.Token1, notif.Token2, notif.Token3, notif.Token4, notif.Token5);

                notif.Title = title;
                notif.Body = body;
                notif.Url = FormatTokens(pattern.Url, notif.Token1, notif.Token2, notif.Token3, notif.Token4, notif.Token5);
                notif.Icon = string.IsNullOrWhiteSpace(pattern.Icon) ? DefaultPushIcon : pattern.Icon;
                notif.Tag = pattern.Tag;

                var payloadDto = new PushPayloadDto
                {
                    Title = notif.Title,
                    Body = notif.Body,
                    Url = notif.Url,
                    Icon = notif.Icon,
                    Tag = notif.Tag
                };

                // پوش «شروع تماس درون‌برنامه‌ای» علاوه بر متن عادی، باید مثل زنگ تلفن واقعی رفتار کند:
                // دکمه‌های پاسخ/رد روی خود نوتیفیکیشن (اندروید/دسکتاپ - iOS از اکشن نوتیفیکیشن پشتیبانی نمی‌کند)
                // و requireInteraction تا با ورود پیام دیگری بلافاصله از صفحه ناپدید نشود.
                var isSessionVideoCall = pattern.PushTypeId == (long)PushTypeEnum.PushOnlineSessionVideoCallStarted;
                var isSessionCall = pattern.PushTypeId == (long)PushTypeEnum.PushOnlineSessionCallStarted || isSessionVideoCall;
                if (pattern.PushTypeId == (long)PushTypeEnum.PushInAppCallStarted || isSessionCall)
                {
                    payloadDto.Type = "call";
                    payloadDto.RequireInteraction = true;
                    // تماس جلسه‌ی آنلاین (بدون رزرو) شناسه‌ی جلسه را در SessionId می‌فرستد، تماس رزرو در ReserveId
                    if (isSessionCall) payloadDto.SessionId = notif.Token2;
                    else payloadDto.ReserveId = notif.Token2;
                    payloadDto.CallerName = notif.Token1;
                    if (isSessionVideoCall) payloadDto.IsVideo = true;
                    payloadDto.Actions = new List<PushActionDto>
                    {
                        new PushActionDto { Action = "answer", Title = "پاسخ" },
                        new PushActionDto { Action = "decline", Title = "رد تماس" }
                    };
                }

                var payload = JsonSerializer.Serialize(payloadDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                // کانال ویندوز مستقل از وجود Subscription ذخیره‌شده است (فقط به اتصال زنده‌ی
                // SignalR نیاز دارد)، پس همیشه تلاش می‌شود و در شمارش موفقیت/شکست زیر شرکت نمی‌کند.
                try
                {
                    await _signalRPushSender.SendAsync(
                        notif.UserId, notif.Title, notif.Body, notif.Url, notif.Icon, notif.Tag,
                        notificationId: notif.Id.ToString(),
                        type: pattern.PushType?.Label);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "SignalR live push delivery failed for user {UserId}.", notif.UserId);
                }

                var subs = await _context.PushSubscriptions
                    .Where(x => x.IsActive && x.UserId == notif.UserId)
                    .AsTracking()
                    .ToListAsync();

                if (subs.Count == 0)
                {
                    await MarkFailedAsync(notif, Resource.Notification.NothingFound);
                    return;
                }

                var client = new WebPushClient();
                var vapid = new VapidDetails("mailto:admin@pastil.pet", _vapid.PublicKey, _vapid.PrivateKey);

                int sent = 0;
                int transientFailures = 0;
                var toDelete = new List<Entities.Entities.PushSubscription>();

                foreach (var s in subs)
                {
                    var result = s.Provider == (long)PushProviderEnum.Fcm
                        ? await _fcmSender.SendAsync(
                            s.FcmToken, notif.Title, notif.Body, notif.Url, notif.Icon, notif.Tag,
                            notificationId: notif.Id.ToString(),
                            type: pattern.PushType?.Label)
                        : await TrySendAsync(client, vapid, payload, s);

                    if (result == PushSendResult.Success)
                    {
                        sent++;
                        s.LastSeen = DateTime.UtcNow;
                    }
                    else if (result == PushSendResult.Expired)
                    {
                        toDelete.Add(s);
                    }
                    else
                    {
                        transientFailures++;
                    }
                }

                if (toDelete.Count > 0)
                    _context.PushSubscriptions.RemoveRange(toDelete);

                if (sent > 0)
                {
                    notif.SentDate = DateTime.Now;
                    notif.Status = true;
                    notif.StatusText = "OK";
                    notif.NextAttemptDate = null;
                    _context.PushNotifications.Update(notif);
                    await _context.SaveChangesAsync();
                }
                else if (transientFailures > 0 && notif.AttemptCount < MaxAttemptCount)
                {
                    await MarkForRetryAsync(notif);
                }
                else
                {
                    await MarkFailedAsync(notif, Resource.Notification.Unsuccess);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending push notification {PushNotificationId} failed", notif.Id);
                if (notif.AttemptCount < MaxAttemptCount)
                    await MarkForRetryAsync(notif);
                else
                    await MarkFailedAsync(notif, ex.Message);
            }
        }

        private async Task<PushSendResult> TrySendAsync(
            WebPushClient client,
            VapidDetails vapid,
            string payload,
            Entities.Entities.PushSubscription subscription)
        {
            try
            {
                var sub = new WebPush.PushSubscription(
                    subscription.Endpoint,
                    subscription.P256dh,
                    subscription.Auth);
                await client.SendNotificationAsync(sub, payload, vapid);
                return PushSendResult.Success;
            }
            catch (WebPushException exception) when (
                exception.StatusCode == HttpStatusCode.Gone ||
                exception.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation(
                    "Removing expired push subscription {PushSubscriptionId}; endpoint returned {StatusCode}",
                    subscription.Id,
                    exception.StatusCode);
                return PushSendResult.Expired;
            }
            catch (WebPushException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Web push delivery failed for subscription {PushSubscriptionId} with status {StatusCode}",
                    subscription.Id,
                    exception.StatusCode);
                return PushSendResult.TransientFailure;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Web push delivery failed for subscription {PushSubscriptionId}",
                    subscription.Id);
                return PushSendResult.TransientFailure;
            }
        }

        private async Task MarkForRetryAsync(PushNotification notif)
        {
            notif.IsSend = false;
            notif.Status = null;
            notif.StatusText = $"RETRY {notif.AttemptCount}/{MaxAttemptCount}";
            notif.SentDate = null;
            notif.NextAttemptDate = DateTime.Now.AddMinutes(5 * notif.AttemptCount);

            _context.PushNotifications.Update(notif);
            await _context.SaveChangesAsync();
        }

        private async Task MarkFailedAsync(PushNotification notif, string statusText)
        {
            notif.Status = false;
            notif.StatusText = statusText;
            notif.SentDate = DateTime.Now;
            notif.IsSend = true;
            notif.NextAttemptDate = null;

            _context.PushNotifications.Update(notif);
            await _context.SaveChangesAsync();
        }

        private static string FormatTokens(string template, string t1, string t2, string t3, string t4, string t5)
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            return string.Format(template,
                (object)t1 ?? string.Empty,
                (object)t2 ?? string.Empty,
                (object)t3 ?? string.Empty,
                (object)t4 ?? string.Empty,
                (object)t5 ?? string.Empty);
        }

        private class PushPayloadDto
        {
            public string Title { get; set; }
            public string Body { get; set; }
            public string Url { get; set; }
            public string Icon { get; set; }
            public string Tag { get; set; }
            public string Type { get; set; }
            public bool? RequireInteraction { get; set; }
            public string ReserveId { get; set; }
            public string SessionId { get; set; }
            public bool? IsVideo { get; set; }
            public string CallerName { get; set; }
            public List<PushActionDto> Actions { get; set; }
        }

        private class PushActionDto
        {
            public string Action { get; set; }
            public string Title { get; set; }
        }
    }
}
