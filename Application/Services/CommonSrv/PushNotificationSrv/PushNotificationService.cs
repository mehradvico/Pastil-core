using AngleSharp.Dom;
using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
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

namespace Application.Services.CommonSrv.PushNotificationSrv
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IDataBaseContext _context;
        private readonly VapidKeysOption _vapid;

        public PushNotificationService(IDataBaseContext context, IOptions<VapidKeysOption> vapid)
        {
            _context = context;
            _vapid = vapid.Value;
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
                    (x.SendDate == null || (x.SendDate.HasValue && x.SendDate.Value < now)))
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
            _context.PushNotifications.Update(notif);
            await _context.SaveChangesAsync();

            try
            {
                var titleTpl = GetPatternValue(pattern.Title);
                var bodyTpl = GetPatternValue(pattern.Body);

                var title = FormatTokens(titleTpl, notif.Token1, notif.Token2, notif.Token3, notif.Token4, notif.Token5);
                var body = FormatTokens(bodyTpl, notif.Token1, notif.Token2, notif.Token3, notif.Token4, notif.Token5);

                notif.Title = title;
                notif.Body = body;
                notif.Url = pattern.Url;
                notif.Icon = pattern.Icon;
                notif.Tag = pattern.Tag;

                var payload = JsonSerializer.Serialize(new PushPayloadDto
                {
                    Title = notif.Title,
                    Body = notif.Body,
                    Url = notif.Url,
                    Icon = notif.Icon,
                    Tag = notif.Tag
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

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
                        toDelete.Add(s);
                    }
                }

                if (toDelete.Count > 0)
                    _context.PushSubscriptions.RemoveRange(toDelete);

                notif.SentDate = DateTime.Now;
                notif.Status = sent > 0;
                notif.StatusText = sent > 0 ? "OK" : "FAILED";

                _context.PushNotifications.Update(notif);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await MarkFailedAsync(notif, ex.Message);
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

        private async Task MarkFailedAsync(PushNotification notif, string statusText)
        {
            notif.Status = false;
            notif.StatusText = statusText;
            notif.SentDate = DateTime.Now;

            _context.PushNotifications.Update(notif);
            await _context.SaveChangesAsync();
        }

        private static string GetPatternValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return Resource.Pattern.ResourceManager.GetString(key) ?? string.Empty;
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
        }
    }
}
