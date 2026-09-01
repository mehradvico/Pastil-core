using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.CommonSrv.PushBroadcastSrv.Dto;
using Application.Services.CommonSrv.PushBroadcastSrv.Iface;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public PushBroadcastService(IDataBaseContext context, IOptions<VapidKeysOption> vapid, IMapper mapper)
        {
            _context = context;
            _vapid = vapid.Value;
            _mapper = mapper;
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
            // Must match PushNotificationService's camelCase policy: the browser-side
            // service worker reads payload.url/icon/tag (lowercase) — without this,
            // Url/Icon/Tag serialize PascalCase and the SW's lookup silently misses
            // them, so every notification click falls back to the home route.
            var payload = JsonSerializer.Serialize(payloadDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var subsQuery = _context.Set<Entities.Entities.PushSubscription>().Include(x => x.User).Where(x => x.IsActive);

            subsQuery = msg.UserId.HasValue
                ? subsQuery.Where(x => x.UserId == msg.UserId.Value)
                : ApplyTypeFilter(subsQuery, (PushMessageTypeEnum)msg.PushMessageTypeId);

            var subs = await subsQuery.ToListAsync();

            int sent = 0, failed = 0;
            var toDelete = new List<Entities.Entities.PushSubscription>();

            foreach (var s in subs)
            {
                var ok = await TrySendAsync(client, vapid, payload, s);

                if (ok)
                {
                    sent++;
                    s.LastSeen = DateTime.UtcNow;
                }
                else
                {
                    failed++;
                    toDelete.Add(s);
                }
            }

            if (toDelete.Count > 0)
                _context.Set<Entities.Entities.PushSubscription>().RemoveRange(toDelete);

            await _context.SaveChangesAsync();
            return new BaseResultDto<PushBroadcastVDto>(true, new PushBroadcastVDto { Sent = sent, Failed = failed });
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

        private static async Task<bool> TrySendAsync(WebPushClient client, VapidDetails vapid, string payload, Entities.Entities.PushSubscription s)
        {
            try
            {
                var sub = new WebPush.PushSubscription(s.Endpoint, s.P256dh, s.Auth);
                await client.SendNotificationAsync(sub, payload, vapid);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
