using Application.Common.Dto.LocationPoint;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Geography.Iface;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Entities.Entities;
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.TripSrv.TripOngoingSrv
{
    public class TripOngoingNotificationService : ITripOngoingNotificationService
    {
        private readonly IDataBaseContext _context;
        private readonly IPushNotificationService _pushService;
        private readonly IGeographyService _geographyService;
        private readonly ILogger<TripOngoingNotificationService> _logger;

        public TripOngoingNotificationService(
            IDataBaseContext context,
            IPushNotificationService pushService,
            IGeographyService geographyService,
            ILogger<TripOngoingNotificationService> logger)
        {
            _context = context;
            _pushService = pushService;
            _geographyService = geographyService;
            _logger = logger;
        }

        public async Task SyncAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            try
            {
                await UpdateActiveTripsAsync(now, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Updating ongoing trip notifications failed.");
            }

            try
            {
                await EndFinishedTripsAsync(now, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ending ongoing trip notifications failed.");
            }
        }

        private async Task UpdateActiveTripsAsync(DateTime now, CancellationToken ct)
        {
            // پت سوار شده و سفر هنوز تمام نشده (مسیر رفت یا برگشت)
            var trips = await _context.Trips.AsNoTracking()
                .Include(t => t.Driver)
                .Include(t => t.UserPet).ThenInclude(p => p.Pet)
                .Where(t => t.TripStatusId == (long)TripStatusEnum.TripStatus_Accepted
                            && t.ProgressStageId == (int)TripProgressStageEnum.PetPickedUp
                            && t.Driver != null)
                .ToListAsync(ct);
            if (trips.Count == 0) return;

            var ongoingTypeId = (long)PushTypeEnum.PushTripOngoing;
            var tripKeys = trips.Select(t => TripOngoingRules.TripKey(t.Id, t.IsReturnLeg)).ToList();
            var endTypeId = (long)PushTypeEnum.PushTripOngoingEnd;
            var alreadyEnded = (await _context.PushNotifications.AsNoTracking()
                .Where(n => n.PushPattern.PushTypeId == endTypeId && n.Token3 != null && tripKeys.Contains(n.Token3))
                .Select(n => n.Token3)
                .Distinct()
                .ToListAsync(ct)).ToHashSet();
            var recent = await _context.PushNotifications.AsNoTracking()
                .Where(n => n.PushPattern.PushTypeId == ongoingTypeId && n.Token3 != null && tripKeys.Contains(n.Token3))
                .Select(n => new { n.Id, n.Token3, n.Token2, n.CreateDate })
                .ToListAsync(ct);
            var lastByTrip = recent
                .GroupBy(n => n.Token3)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(n => n.Id).First());

            var ownerIds = trips.Select(t => t.Driver.OwnerId).Distinct().ToList();
            var locations = await _context.UserCurrentLocations.AsNoTracking()
                .Where(l => ownerIds.Contains(l.UserId))
                .ToDictionaryAsync(l => l.UserId, ct);

            foreach (var trip in trips)
            {
                try
                {
                    var key = TripOngoingRules.TripKey(trip.Id, trip.IsReturnLeg);
                    // اعلان این مسیر قبلاً بسته شده (مثلاً سقف عمر اعلان): دوباره باز نمی‌شود
                    if (alreadyEnded.Contains(key)) continue;
                    lastByTrip.TryGetValue(key, out var last);

                    // قبل از محاسبه‌ی مسیر (که API بیرونی را صدا می‌زند) اگر هنوز زود است رد شو
                    if (last != null && now - last.CreateDate < TripOngoingRules.MinInterval) continue;

                    var minutes = await EstimateMinutesAsync(trip, locations, now);
                    var text = TripOngoingRules.EtaText(minutes, now);
                    if (!TripOngoingRules.ShouldSendUpdate(last?.Token2, last?.CreateDate, text, now)) continue;

                    await _pushService.SendPushAsync(PushTypeEnum.PushTripOngoing, trip.UserId,
                        token1: trip.UserPet?.Pet?.Name, token2: text, token3: key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ongoing trip notification for trip {TripId} failed.", trip.Id);
                }
            }
        }

        private async Task<int?> EstimateMinutesAsync(Trip trip, Dictionary<long, UserCurrentLocation> locations, DateTime now)
        {
            if (trip.Destination == null) return null;
            if (!locations.TryGetValue(trip.Driver.OwnerId, out var location) || location?.Location == null) return null;
            if (now - location.LastUpdateDate > TripOngoingRules.LocationMaxAge) return null;

            var km = await _geographyService.GetDrivingDistanceAsync(
                new PointDto(location.Location.X, location.Location.Y),
                new PointDto(trip.Destination.X, trip.Destination.Y));
            return TripOngoingRules.EtaMinutes(km);
        }

        // اعلان‌هایی که سفرشان دیگر «پت سوار شده/در جریان» نیست (رسید، لغو شد، ...) یا خیلی قدیمی شده‌اند: یک پوش پایان
        private async Task EndFinishedTripsAsync(DateTime now, CancellationToken ct)
        {
            var ongoingTypeId = (long)PushTypeEnum.PushTripOngoing;
            var endTypeId = (long)PushTypeEnum.PushTripOngoingEnd;
            var since = now - TripOngoingRules.MaxLifetime;

            var rows = await _context.PushNotifications.AsNoTracking()
                .Where(n => n.PushPattern.PushTypeId == ongoingTypeId && n.Token3 != null && n.CreateDate >= since)
                .Select(n => new { n.Token3, n.UserId, n.CreateDate })
                .ToListAsync(ct);
            if (rows.Count == 0) return;

            var started = rows
                .GroupBy(n => new { n.Token3, n.UserId })
                .Select(g => new { g.Key.Token3, g.Key.UserId, First = g.Min(n => n.CreateDate) })
                .ToList();

            var keys = started.Select(s => s.Token3).Distinct().ToList();
            var ended = (await _context.PushNotifications.AsNoTracking()
                .Where(n => n.PushPattern.PushTypeId == endTypeId && n.Token3 != null && keys.Contains(n.Token3))
                .Select(n => n.Token3)
                .Distinct()
                .ToListAsync(ct)).ToHashSet();
            var pending = started.Where(s => !ended.Contains(s.Token3)).ToList();
            if (pending.Count == 0) return;

            var tripIds = pending.Select(p => TripOngoingRules.TryParseTripId(p.Token3, out var id) ? id : 0).Where(id => id > 0).Distinct().ToList();
            var activeRows = await _context.Trips.AsNoTracking()
                .Where(t => tripIds.Contains(t.Id)
                            && t.TripStatusId == (long)TripStatusEnum.TripStatus_Accepted
                            && t.ProgressStageId == (int)TripProgressStageEnum.PetPickedUp)
                .Select(t => new { t.Id, t.IsReturnLeg })
                .ToListAsync(ct);
            var active = activeRows.Select(t => TripOngoingRules.TripKey(t.Id, t.IsReturnLeg)).ToHashSet();

            foreach (var item in pending)
            {
                if (!TripOngoingRules.TryParseTripId(item.Token3, out var tripId)) continue;
                // هنوز در جریان است و از حداکثر عمر اعلان نگذشته
                if (active.Contains(item.Token3) && now - item.First < TripOngoingRules.MaxLifetime) continue;

                try
                {
                    var petName = await _context.Trips.AsNoTracking()
                        .Where(t => t.Id == tripId)
                        .Select(t => t.UserPet.Pet.Name)
                        .FirstOrDefaultAsync(ct);
                    await _pushService.SendPushAsync(PushTypeEnum.PushTripOngoingEnd, item.UserId,
                        token1: petName, token3: item.Token3);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ending ongoing trip notification for trip {TripId} failed.", tripId);
                }
            }
        }
    }
}
