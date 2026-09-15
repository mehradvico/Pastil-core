using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.CommonSrv.PushBroadcastSrv.Dto;
using Application.Services.CommonSrv.PushBroadcastSrv.Iface;
using Application.Services.CommonSrv.PushNotificationSrv;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using WebPush;

namespace Application.Services.CommonSrv.PushBroadcastSrv
{
    public class PushBroadcastService : IPushBroadcastService
    {
        private readonly IDataBaseContext _context;
        private readonly VapidKeysOption _vapid;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IFcmSender _fcmSender;
        private readonly ILogger<PushBroadcastService> _logger;

        public PushBroadcastService(
            IDataBaseContext context,
            IOptions<VapidKeysOption> vapid,
            IMapper mapper,
            IConfiguration configuration,
            IFcmSender fcmSender,
            ILogger<PushBroadcastService> logger)
        {
            _context = context;
            _vapid = vapid.Value;
            _mapper = mapper;
            _configuration = configuration;
            _fcmSender = fcmSender;
            _logger = logger;
        }

        public async Task<BaseResultDto> BroadcastAsync(PushBroadcastDto req)
        {
            var msg = await _context.PushMessages.Include(x => x.Picture).FirstOrDefaultAsync(x => x.Id == req.PushMessageId && !x.Deleted);

            if (msg == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            if (!PersianPushTextHelper.ContainsPersian(msg.Title) ||
                !PersianPushTextHelper.ContainsPersian(msg.Body))
                return new BaseResultDto(false, Resource.Notification.PushBroadcastTitleAndBodyMustBePersian);

            var client = new WebPushClient();
            var vapid = new VapidDetails("mailto:admin@pastil.pet", _vapid.PublicKey, _vapid.PrivateKey);

            var payloadDto = _mapper.Map<PushPayloadDto>(msg);
            // Icon مپ‌شده از AutoMapper همون Picture.Url خامه - یه مسیر نسبی روی فایل‌سرور
            // (مثلاً "/Media/2026/8/27")، بدون GuidName/Extension و بدون دامنه‌ی file.pastil.pet.
            // هر فرانت (پنل/وب‌اپ/سایت) قبل از نمایش این مسیر رو با showImageBaseUrl کامل می‌کنه،
            // ولی نوتیفیکیشن مستقیم توسط مرورگر/اندروید fetch میشه و کسی این کار رو براش
            // انجام نمی‌ده؛ در نتیجه آیکون‌های سفارشی آپلودشده از پنل همیشه ۴۰۴ می‌خوردن و
            // اندروید یه آواتار حرف اول دامنه ("A" از app.pastil.pet) نشون می‌داد.
            payloadDto.Icon = BuildAbsolutePictureUrl(msg.Picture);
            // Must match PushNotificationService's camelCase policy: the browser-side
            // service worker reads payload.url/icon/tag (lowercase) — without this,
            // Url/Icon/Tag serialize PascalCase and the SW's lookup silently misses
            // them, so every notification click falls back to the home route.
            var payload = JsonSerializer.Serialize(payloadDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var subsQuery = _context.Set<Entities.Entities.PushSubscription>().Include(x => x.User).Where(x => x.IsActive);

            subsQuery = msg.UserId.HasValue
                ? subsQuery.Where(x => x.UserId == msg.UserId.Value)
                : ApplyTypeFilter(subsQuery, (PushMessageTypeEnum)msg.PushMessageTypeId);

            var subs = await subsQuery.AsTracking().ToListAsync();

            // بدون این، حالت «مخاطب هیچ دستگاه ثبت‌شده‌ای ندارد» یک پاسخ موفق با
            // «موفق: ۰ | ناموفق: ۰» برمی‌گرداند و در پنل شبیه ارسال موفق دیده می‌شود،
            // در حالی که عملاً هیچ‌چیز نرفته. این حالت باید صریحاً خطا باشد.
            if (subs.Count == 0)
                return new BaseResultDto(false, Resource.Notification.PushBroadcastNoActiveSubscription);

            int sent = 0, failed = 0;
            var toDelete = new List<Entities.Entities.PushSubscription>();

            foreach (var s in subs)
            {
                var result = s.Provider == (long)PushProviderEnum.Fcm
                    ? await _fcmSender.SendAsync(s.FcmToken, payloadDto.Title, payloadDto.Body, payloadDto.Url, payloadDto.Icon, payloadDto.Tag)
                    : await TrySendAsync(client, vapid, payload, s);

                if (result == PushSendResult.Success)
                {
                    sent++;
                    s.LastSeen = DateTime.UtcNow;
                }
                else if (result == PushSendResult.Expired)
                {
                    failed++;
                    toDelete.Add(s);
                }
                else
                {
                    failed++;
                }
            }

            if (toDelete.Count > 0)
                _context.Set<Entities.Entities.PushSubscription>().RemoveRange(toDelete);

            await _context.SaveChangesAsync();
            return new BaseResultDto<PushBroadcastVDto>(true, new PushBroadcastVDto { Sent = sent, Failed = failed });
        }

        // دقیقاً همون ترکیب‌بندی که webapp/panel/website توی getPicUrl انجام می‌دن:
        // {FileBaseUrl}{Url}/{GuidName}{Extension} - بدون این یعنی مسیر نسبی خام به Notification API داده میشه.
        private string BuildAbsolutePictureUrl(Picture picture)
        {
            if (picture == null || string.IsNullOrWhiteSpace(picture.GuidName))
                return null;

            var fileBaseUrl = _configuration["Urls:FileBaseUrl"]?.TrimEnd('/') ?? "";
            var path = $"{picture.Url}/{picture.GuidName}{picture.Extension}".Replace("//", "/");
            return $"{fileBaseUrl}{(path.StartsWith("/") ? path : "/" + path)}";
        }

        private static IQueryable<Entities.Entities.PushSubscription> ApplyTypeFilter(IQueryable<Entities.Entities.PushSubscription> query, PushMessageTypeEnum type)
        {
            switch (type)
            {
                case PushMessageTypeEnum.PushMessageType_All:
                    return query;

                case PushMessageTypeEnum.PushMessageType_Admin:
                    return query.Where(x => x.UserId != null && x.User != null && x.User.RoleId == (long)RoleEnum.Admin);

                case PushMessageTypeEnum.PushMessageType_Companion:
                    return query.Where(x => x.UserId != null && x.User != null && x.User.RoleId == (long)RoleEnum.Companion);

                case PushMessageTypeEnum.PushMessageType_Store:
                    return query.Where(x => x.UserId != null && x.User != null && x.User.RoleId == (long)RoleEnum.Store);

                case PushMessageTypeEnum.PushMessageType_Operator:
                    return query.Where(x => x.UserId != null && x.User != null && x.User.RoleId == (long)RoleEnum.Operator);

                case PushMessageTypeEnum.PushMessageType_EndUser:
                    return query.Where(x => x.UserId != null && x.User != null && x.User.RoleId == (long)RoleEnum.Customer);

                case PushMessageTypeEnum.PushMessageType_Pansion:
                    return query.Where(x => x.UserId != null && x.User != null && x.User.RoleId == (long)RoleEnum.Companion);

                default:
                    return query;
            }
        }

        private async Task<PushSendResult> TrySendAsync(WebPushClient client, VapidDetails vapid, string payload, Entities.Entities.PushSubscription s)
        {
            try
            {
                var sub = new WebPush.PushSubscription(s.Endpoint, s.P256dh, s.Auth);
                await client.SendNotificationAsync(sub, payload, vapid);
                return PushSendResult.Success;
            }
            catch (WebPushException exception) when (
                exception.StatusCode == HttpStatusCode.Gone ||
                exception.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation(
                    "Removing expired push subscription {PushSubscriptionId}; endpoint returned {StatusCode}",
                    s.Id,
                    exception.StatusCode);
                return PushSendResult.Expired;
            }
            catch (WebPushException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Broadcast web push delivery failed for subscription {PushSubscriptionId} with status {StatusCode}",
                    s.Id,
                    exception.StatusCode);
                return PushSendResult.TransientFailure;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Broadcast push delivery failed for subscription {PushSubscriptionId}", s.Id);
                return PushSendResult.TransientFailure;
            }
        }
    }
}
