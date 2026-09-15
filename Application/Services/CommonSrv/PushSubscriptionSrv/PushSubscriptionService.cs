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
                sub.DeviceKey = userId.HasValue ? null : dto.DeviceKey;
                sub.Provider = (long)PushProviderEnum.WebPush;
                sub.IsActive = true;
                sub.CreateDate = DateTime.UtcNow;
                sub.LastSeen = DateTime.UtcNow;

                _context.PushSubscriptions.Add(sub);
            }
            else
            {
                _mapper.Map(dto, sub);

                sub.IsActive = true;
                sub.LastSeen = DateTime.UtcNow;

                if (userId.HasValue)
                {
                    sub.UserId = userId.Value;
                    sub.DeviceKey = null;
                }
                else
                {
                    sub.DeviceKey = dto.DeviceKey;
                    sub.UserId = null;
                }
            }

            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        // معادل SubscribeAsync، فقط برای اپ فلاتر (اندروید/iOS) — به‌جای Endpoint/P256dh/Auth
        // مرورگر، یک FcmToken از Firebase Cloud Messaging ثبت می‌شود. منطق Upsert/Device-Key/Attach
        // عمداً کپی شده نه به اشتراک‌گذاشته، چون شکل ورودی (Endpoint در برابر FcmToken به‌عنوان کلید
        // یکتای هر ردیف) کاملاً متفاوت است.
        public async Task<BaseResultDto> SubscribeFcmAsync(long? userId, PushSubscribeFcmDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.FcmToken))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            // AsTracking اجباری است - همان دلیل SubscribeAsync بالا.
            var sub = await _context.PushSubscriptions.AsTracking().FirstOrDefaultAsync(x => x.FcmToken == dto.FcmToken);

            if (sub == null)
            {
                sub = _mapper.Map<PushSubscription>(dto);

                sub.UserId = userId;
                sub.DeviceKey = userId.HasValue ? null : dto.DeviceKey;
                sub.Provider = (long)PushProviderEnum.Fcm;
                sub.IsActive = true;
                sub.CreateDate = DateTime.UtcNow;
                sub.LastSeen = DateTime.UtcNow;

                _context.PushSubscriptions.Add(sub);
            }
            else
            {
                _mapper.Map(dto, sub);

                sub.IsActive = true;
                sub.LastSeen = DateTime.UtcNow;

                if (userId.HasValue)
                {
                    sub.UserId = userId.Value;
                    sub.DeviceKey = null;
                }
                else
                {
                    sub.DeviceKey = dto.DeviceKey;
                    sub.UserId = null;
                }
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
