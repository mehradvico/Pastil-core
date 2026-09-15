using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using Application.Services.CommonSrv.PushSubscriptionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushSubscriptionSrv
{
    public class PushSubscriptionService : IPushSubscriptionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;

        public PushSubscriptionService(IDataBaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BaseResultDto> SubscribeAsync(long? userId, PushSubscribeDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Endpoint) || dto.Keys == null)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            // AsTracking اجباری است: DataBaseContext به‌صورت سراسری NoTracking است، پس
            // بدون آن، شاخه‌ی آپدیت پایین (از جمله ست‌کردن UserId) بی‌صدا دور ریخته می‌شود و
            // SaveChanges هیچ‌چیز نمی‌نویسد؛ ردیف برای همیشه UserId=null می‌ماند و پوشِ
            // «کاربر خاص» هرگز به آن دستگاه نمی‌رسد.
            var sub = await _context.PushSubscriptions.AsTracking().FirstOrDefaultAsync(x => x.Endpoint == dto.Endpoint);

            if (sub == null)
            {
                sub = _mapper.Map<PushSubscription>(dto);

                sub.UserId = userId;
                sub.DeviceKey = dto.DeviceKey;
                sub.Provider = (long)PushProviderEnum.WebPush;
                sub.IsActive = true;
                sub.CreateDate = DateTime.UtcNow;
                sub.LastSeen = DateTime.UtcNow;

                _context.PushSubscriptions.Add(sub);
            }
            else
            {
                _mapper.Map(dto, sub);
                ApplyIdentity(sub, userId, dto.DeviceKey);
            }

            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        // هویت ردیف در هر بار sync:
        //  • DeviceKey همیشه نگه داشته می‌شود؛ این شناسه‌ی پایدارِ نصبِ اپ است و با آن
        //    unsubscribe هنگام logout و attach بعد از login ممکن می‌شود. (قبلاً بعد از
        //    وصل‌شدن به کاربر null می‌شد و در نتیجه دیگر با DeviceKey پیدا نمی‌شد.)
        //  • UserId فقط وقتی JWT معتبر وجود دارد ست می‌شود و با یک درخواست ناشناس
        //    هرگز پاک نمی‌شود؛ جداکردن دستگاه از کاربر فقط با unsubscribe صریح انجام
        //    می‌شود، وگرنه یک sync ناشناس اتصال کاربر لاگین‌شده را بی‌صدا از بین می‌برد.
        private static void ApplyIdentity(PushSubscription sub, long? userId, Guid deviceKey)
        {
            sub.IsActive = true;
            sub.LastSeen = DateTime.UtcNow;
            sub.DeviceKey = deviceKey;

            if (userId.HasValue)
                sub.UserId = userId.Value;
        }

        // معادل SubscribeAsync، فقط برای اپ فلاتر (اندروید/iOS) — به‌جای Endpoint/P256dh/Auth
        // مرورگر، یک FcmToken از Firebase Cloud Messaging ثبت می‌شود. منطق Upsert/Device-Key/Attach
        // عمداً کپی شده نه به اشتراک‌گذاشته، چون شکل ورودی (Endpoint در برابر FcmToken به‌عنوان کلید
        // یکتای هر ردیف) کاملاً متفاوت است.
        public async Task<BaseResultDto> SubscribeFcmAsync(long? userId, PushSubscribeFcmDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.FcmToken))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            // FCM فقط برای اپ نیتیو اندروید/iOS است. Windows روی FCM پشتیبانی نمی‌شود، پس
            // اجازه نمی‌دهیم ردیفی ساخته شود که هیچ‌وقت قابل تحویل نیست و فقط شمارنده‌ی
            // failed را بالا می‌برد.
            var platform = NormalizePlatform(dto.Platform);
            if (platform == null)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            // AsTracking اجباری است - همان دلیل SubscribeAsync بالا.
            var sub = await _context.PushSubscriptions.AsTracking().FirstOrDefaultAsync(x => x.FcmToken == dto.FcmToken);

            if (sub == null)
            {
                sub = _mapper.Map<PushSubscription>(dto);

                sub.UserId = userId;
                sub.DeviceKey = dto.DeviceKey;
                sub.Platform = platform;
                sub.Provider = (long)PushProviderEnum.Fcm;
                sub.IsActive = true;
                sub.CreateDate = DateTime.UtcNow;
                sub.LastSeen = DateTime.UtcNow;

                _context.PushSubscriptions.Add(sub);
            }
            else
            {
                _mapper.Map(dto, sub);
                sub.Platform = platform;
                sub.Provider = (long)PushProviderEnum.Fcm;
                ApplyIdentity(sub, userId, dto.DeviceKey);
            }

            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        private static string NormalizePlatform(string platform)
        {
            if (string.IsNullOrWhiteSpace(platform))
                return null;

            var value = platform.Trim().ToLowerInvariant();
            return value == "android" || value == "ios" ? value : null;
        }

        // هنگام logout اپ: ردیف همین دستگاه را غیرفعال و از کاربر جدا می‌کند تا اعلان
        // شخصی کاربر قبلی روی دستگاه مشترک دیده نشود. کاربر بعدی با subscribe-fcm یک
        // اتصال تازه می‌سازد. Idempotent است: نبودن ردیف هم موفقیت حساب می‌شود.
        public async Task<BaseResultDto> UnsubscribeFcmAsync(long userId, Guid deviceKey)
        {
            if (userId <= 0)
                return new BaseResultDto(false, Resource.Notification.InvalidUser);

            if (deviceKey == Guid.Empty)
                return new BaseResultDto(false, Resource.Notification.InvalidDeviceKey);

            var subs = await _context.PushSubscriptions.AsTracking()
                .Where(x => x.DeviceKey == deviceKey &&
                            x.UserId == userId &&
                            x.Provider == (long)PushProviderEnum.Fcm)
                .ToListAsync();

            if (subs.Count == 0)
                return new BaseResultDto(true);

            foreach (var s in subs)
            {
                s.IsActive = false;
                s.UserId = null;
                s.LastSeen = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> AttachAsync(long userId, Guid deviceKey)
        {
            if (userId <= 0)
                return new BaseResultDto(false, Resource.Notification.InvalidUser);

            if (deviceKey == Guid.Empty)
                return new BaseResultDto(false, Resource.Notification.InvalidDeviceKey);

            var subs = await _context.PushSubscriptions.AsTracking().Where(x => x.UserId == null && x.DeviceKey == deviceKey).ToListAsync();

            if (subs.Count == 0)
                return new BaseResultDto(true);

            foreach (var s in subs)
            {
                s.UserId = userId;
                s.DeviceKey = null;
                s.IsActive = true;
                s.LastSeen = DateTime.UtcNow;
            }
            _context.PushSubscriptions.UpdateRange(subs);
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }
    }

}
