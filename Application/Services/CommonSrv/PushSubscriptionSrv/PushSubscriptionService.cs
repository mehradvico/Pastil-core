using Application.Common.Dto.Result;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using Application.Services.CommonSrv.PushSubscriptionSrv.Iface;
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

        public PushSubscriptionService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto> SubscribeAsync(long? userId, PushSubscribeDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Endpoint) || dto.Keys == null)
                return new BaseResultDto(false, "Invalid payload");

            var sub = await _context.PushSubscriptions.FirstOrDefaultAsync(x => x.Endpoint == dto.Endpoint);

            if (sub == null)
            {
                sub = new PushSubscription
                {
                    Endpoint = dto.Endpoint,
                    P256dh = dto.Keys.P256dh,
                    Auth = dto.Keys.Auth,
                    UserAgent = dto.UserAgent,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow,
                    UserId = userId,
                    DeviceKey = userId.HasValue ? null : dto.DeviceKey
                };

                _context.PushSubscriptions.Add(sub);
            }
            else
            {
                sub.P256dh = dto.Keys.P256dh;
                sub.Auth = dto.Keys.Auth;
                sub.UserAgent = dto.UserAgent;
                sub.IsActive = true;
                sub.LastSeen = DateTime.UtcNow;

                if (!userId.HasValue)
                {
                    sub.DeviceKey = dto.DeviceKey;
                }
                else
                {
                    sub.UserId = userId.Value;
                    sub.DeviceKey = null;
                }
            }

            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> AttachAsync(long userId, Guid deviceKey)
        {
            if (userId <= 0) return new BaseResultDto(false, "Invalid user");
            if (deviceKey == Guid.Empty) return new BaseResultDto(false, "Invalid deviceKey");

            var subs = await _context.PushSubscriptions.Where(x => x.UserId == null && x.DeviceKey == deviceKey).ToListAsync();

            if (subs.Count == 0)
                return new BaseResultDto(true); 

            foreach (var s in subs)
            {
                s.UserId = userId;
                s.DeviceKey = null;
                s.IsActive = true;
                s.LastSeen = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }
    }
}
