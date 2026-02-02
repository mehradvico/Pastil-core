using Application.Common.Dto.Result;
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
using System.Text;
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
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            var client = new WebPushClient();
            var vapid = new VapidDetails("mailto:admin@pastil.pet", _vapid.PublicKey, _vapid.PrivateKey);
            var payloadDto = _mapper.Map<PushPayloadDto>(msg);
            var payload = JsonSerializer.Serialize(payloadDto);
            var subs = await _context.Set<Entities.Entities.PushSubscription>().Where(x => x.IsActive).ToListAsync();

            int sent = 0, failed = 0;
            foreach (var s in subs)
            {
                try
                {
                    var sub = new WebPush.PushSubscription(s.Endpoint, s.P256dh, s.Auth);
                    await client.SendNotificationAsync(sub, payload, vapid);

                    sent++;
                    s.LastSeen = DateTime.UtcNow;
                }
                catch (WebPushException ex)
                {
                    failed++;
                    if (ex.StatusCode == System.Net.HttpStatusCode.Gone || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        s.IsActive = false;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return new BaseResultDto<PushBroadcastVDto>(true, new PushBroadcastVDto{Sent = sent, Failed = failed});
        }
    }

}
