using Application.Common.Dto.Result;
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

            var sub = await _context.PushSubscriptions.FirstOrDefaultAsync(x => x.Endpoint == dto.Endpoint);

            if (sub == null)
            {
                sub = _mapper.Map<PushSubscription>(dto);

                sub.UserId = userId;
                sub.DeviceKey = userId.HasValue ? null : dto.DeviceKey;
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
            _context.PushSubscriptions.UpdateRange(subs);
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }
    }

}
