using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Iface;
using Application.Services.Order.ShippingSrv;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveSrv
{
    public class CompanionReserveRescheduleService : ICompanionReserveRescheduleService
    {
        private readonly IDataBaseContext _context;
        private readonly IMessageSenderService _messageSender;
        private readonly IPushNotificationService _pushService;
        private readonly ILogger<CompanionReserveRescheduleService> _logger;

        public CompanionReserveRescheduleService(
            IDataBaseContext context,
            IMessageSenderService messageSender,
            IPushNotificationService pushService,
            ILogger<CompanionReserveRescheduleService> logger)
        {
            _context = context;
            _messageSender = messageSender;
            _pushService = pushService;
            _logger = logger;
        }

        private IQueryable<CompanionReserve> ReserveQuery() => _context.CompanionReserves
            .Include(r => r.Booker)
            .Include(r => r.CompanionAssistance).ThenInclude(a => a.Companion)
            .Include(r => r.CompanionTime)
            .Include(r => r.CompanionAssistanceTime)
            .Include(r => r.CompanionAssistancePackageOnlineSelection).ThenInclude(s => s.CompanionAssistancePackageOnline);

        private static bool IsInstantOnline(CompanionReserve r) =>
            r.CompanionAssistancePackageOnlineSelection?.CompanionAssistancePackageOnline?.IsInstant == true;

        private static BaseResultDto<T> Fail<T>(string message) => new(false, message, default);

        // نماینده فقط رزرو مرکز خودش را می‌بیند؛ ادمین (companionId == null) همه را
        private static string CheckAccess(CompanionReserve reserve, long? companionId)
        {
            if (reserve == null)
                return Resource.Notification.NothingFound;
            if (companionId.HasValue && reserve.CompanionAssistance?.CompanionId != companionId.Value)
                return Resource.Notification.AccessDenied;
            return null;
        }

        private static string CheckState(CompanionReserve reserve)
        {
            var isComplete = reserve.StateId == (long)CompanionReserveStateEnum.CompanianReserveState_Complete;
            return CompanionReserveRescheduleRules.CanReschedule(
                reserve.IsReserved, reserve.IsCancel, reserve.DoneDate, isComplete, IsInstantOnline(reserve))
                ? null
                : Resource.Notification.CompanionReserveRescheduleNotAllowed;
        }

        public async Task<BaseResultDto<CompanionReserveRescheduleSlotsVDto>> GetSlotsAsync(
            long reserveId, DateTime date, long? companionId, CancellationToken cancellationToken = default)
        {
            var reserve = await ReserveQuery().AsNoTracking().FirstOrDefaultAsync(r => r.Id == reserveId, cancellationToken);
            var error = CheckAccess(reserve, companionId) ?? CheckState(reserve);
            if (error != null)
                return Fail<CompanionReserveRescheduleSlotsVDto>(error);

            var day = date.Date;
            var times = await _context.CompanionTimes.AsNoTracking()
                .Include(t => t.WeekDay)
                .Where(t => t.CompanionId == reserve.CompanionAssistance.CompanionId && t.Active && !t.Deleted)
                .OrderBy(t => t.StartTime)
                .ToListAsync(cancellationToken);

            var result = new CompanionReserveRescheduleSlotsVDto { Date = day };
            var now = DateTime.Now;
            foreach (var time in times.Where(t => ReservationScheduleValidator.IsWeekDayMatch(day, t.WeekDay?.Label)))
            {
                var reservedByOthers = await CountOthersAsync(reserve.Id, time.Id, day, cancellationToken);
                var remaining = Math.Max((time.Capacity > 0 ? time.Capacity : 1) - reservedByOthers, 0);
                var startAt = CompanionReserveRescheduleRules.StartAt(day, time.StartTime);
                var isCurrent = CompanionReserveRescheduleRules.IsSameSlot(reserve.DoDate, reserve.CompanionTimeId, day, time.Id);

                string reason = null;
                if (isCurrent) reason = Resource.Notification.CompanionReserveRescheduleCurrentSlot;
                else if (startAt == null || !CompanionReserveRescheduleRules.IsFarEnough(startAt.Value, now)) reason = Resource.Notification.CompanionReserveRescheduleSlotTooSoon;
                else if (remaining <= 0) reason = Resource.Notification.CompanionReserveRescheduleSlotFull;
                else if (reserve.CompanionAssistanceUserId.HasValue &&
                         await HasAssigneeConflictAsync(reserve.Id, day, time.StartTime, time.EndTime, reserve.CompanionAssistanceUserId.Value, cancellationToken))
                    reason = Resource.Notification.CompanionReserveOperatorHasScheduleConflict;

                result.Slots.Add(new CompanionReserveRescheduleSlotVDto
                {
                    CompanionTimeId = time.Id,
                    StartTime = time.StartTime,
                    EndTime = time.EndTime,
                    Capacity = time.Capacity > 0 ? time.Capacity : 1,
                    RemainingCapacity = remaining,
                    Selectable = reason == null,
                    Reason = reason,
                    IsCurrent = isCurrent
                });
            }
            return new BaseResultDto<CompanionReserveRescheduleSlotsVDto>(true, result);
        }

        public async Task<BaseResultDto<CompanionReserveRescheduleResultVDto>> RescheduleAsync(
            CompanionReserveRescheduleDto dto, long actorUserId, int actorKind, long? companionId, CancellationToken cancellationToken = default)
        {
            if (dto == null || dto.Id <= 0 || dto.NewCompanionTimeId <= 0)
                return Fail<CompanionReserveRescheduleResultVDto>(Resource.Notification.InvalidData);

            var reason = CompanionReserveRescheduleRules.NormalizeReason(dto.Reason);
            if (string.IsNullOrWhiteSpace(reason))
                return Fail<CompanionReserveRescheduleResultVDto>(Resource.Notification.CompanionReserveRescheduleReasonRequired);

            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);

            var reserve = await ReserveQuery().AsTracking().FirstOrDefaultAsync(r => r.Id == dto.Id, cancellationToken);
            var error = CheckAccess(reserve, companionId) ?? CheckState(reserve);
            if (error != null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Fail<CompanionReserveRescheduleResultVDto>(error);
            }

            var newDay = dto.NewDoDate.Date;
            var newTime = await _context.CompanionTimes.AsNoTracking().Include(t => t.WeekDay)
                .FirstOrDefaultAsync(t => t.Id == dto.NewCompanionTimeId && t.CompanionId == reserve.CompanionAssistance.CompanionId && t.Active && !t.Deleted, cancellationToken);

            string validation = null;
            DateTime? newStartAt = null;
            if (newTime == null)
                validation = Resource.Notification.CompanionTimeNotBelongOrInactive;
            else if (!ReservationScheduleValidator.IsWeekDayMatch(newDay, newTime.WeekDay?.Label))
                validation = Resource.Notification.CompanionReserveDayMismatchWithSelectedTime;
            else if ((newStartAt = CompanionReserveRescheduleRules.StartAt(newDay, newTime.StartTime)) == null)
                validation = Resource.Notification.CompanionReserveServiceStartTimeNotValid;
            else if (CompanionReserveRescheduleRules.IsSameSlot(reserve.DoDate, reserve.CompanionTimeId, newDay, newTime.Id))
                validation = Resource.Notification.CompanionReserveRescheduleSameSlot;
            else if (!CompanionReserveRescheduleRules.IsFarEnough(newStartAt.Value, DateTime.Now))
                validation = Resource.Notification.CompanionReserveRescheduleSlotTooSoon;
            else if (!CompanionReserveRescheduleRules.HasCapacity(newTime.Capacity, await CountOthersAsync(reserve.Id, newTime.Id, newDay, cancellationToken)))
                validation = Resource.Notification.CompanionReserveRescheduleSlotFull;
            else if (reserve.CompanionAssistanceUserId.HasValue &&
                     await HasAssigneeConflictAsync(reserve.Id, newDay, newTime.StartTime, newTime.EndTime, reserve.CompanionAssistanceUserId.Value, cancellationToken))
                validation = Resource.Notification.CompanionReserveOperatorHasScheduleConflict;

            if (validation != null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Fail<CompanionReserveRescheduleResultVDto>(validation);
            }

            var now = DateTime.Now;
            var oldDoDate = reserve.DoDate;
            var oldTimeId = reserve.CompanionTimeId;
            var oldStartAt = CompanionReserveRescheduleRules.StartAt(reserve);

            reserve.DoDate = newDay;
            reserve.CompanionTimeId = newTime.Id;
            // رزروهای قدیمی با زمان‌بندیِ هر خدمت ثبت شده‌اند؛ از این پس ساعت از ساعت کاری مرکز (CompanionTime) می‌آید
            reserve.CompanionAssistanceTimeId = null;

            var linkedTrip = await AdjustLinkedTripAsync(reserve.Id, newStartAt.Value, now, cancellationToken);
            await ShiftOnlineRemindersAsync(reserve.Id, newStartAt.Value, cancellationToken);

            var history = new CompanionReserveReschedule
            {
                CompanionReserveId = reserve.Id,
                OldDoDate = oldDoDate,
                OldCompanionTimeId = oldTimeId,
                OldStartAt = oldStartAt,
                NewDoDate = newDay,
                NewCompanionTimeId = newTime.Id,
                NewStartAt = newStartAt.Value,
                Reason = reason,
                ActorKind = actorKind,
                ActorUserId = actorUserId,
                LinkedTripId = linkedTrip?.Id,
                CreateDate = now
            };
            _context.CompanionReserveReschedules.Add(history);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // بعد از commit: پیامک به کاربر و راننده. شکست پیامک رزرو را برنمی‌گرداند؛ وضعیتش در تاریخچه ثبت می‌شود.
            var centerName = ShipmentLifecycleRules.ForSmsToken(reserve.CompanionAssistance?.Companion?.Name, 60);
            var oldDateForSms = CompanionReserveRescheduleRules.JalaliDate((oldStartAt ?? oldDoDate));
            var newStart = newStartAt.Value;

            var newDateText = CompanionReserveRescheduleRules.JalaliDate(newStart);
            var newTimeText = CompanionReserveRescheduleRules.Time(newStart);
            var userPushed = await TryPushAsync(PushTypeEnum.PushCompanionReserveRescheduledUser, reserve.BookerId, reserve.Id,
                reserve.CompanionAssistance?.Companion?.Name, newDateText, newTimeText, reason);
            var userSms = await TrySmsAsync(MessageTypeEnum.CompanionReserveRescheduledUser, reserve.Booker?.Mobile,
                token1: centerName,
                token2: oldDateForSms,
                token3: CompanionReserveRescheduleRules.JalaliDate(newStart),
                token4: CompanionReserveRescheduleRules.SmsTime(newStart),
                token5: ShipmentLifecycleRules.ForSmsToken(reason, CompanionReserveRescheduleRules.MaxReasonLength),
                reserveId: reserve.Id);
            var userNotified = userSms || userPushed;

            var driverNotified = false;
            DateTime? departureAt = null;
            if (linkedTrip != null)
            {
                departureAt = linkedTrip.ScheduledDepartureAt;
                if (linkedTrip.Driver != null && departureAt.HasValue)
                {
                    var departureDate = CompanionReserveRescheduleRules.JalaliDate(departureAt.Value);
                    var departureTime = CompanionReserveRescheduleRules.Time(departureAt.Value);
                    var driverSms = await TrySmsAsync(MessageTypeEnum.CompanionReserveRescheduledDriver, linkedTrip.Driver.Phone,
                        token1: departureDate,
                        token2: CompanionReserveRescheduleRules.SmsTime(departureAt.Value),
                        token3: centerName,
                        reserveId: reserve.Id);
                    var driverPushed = await TryPushAsync(PushTypeEnum.PushCompanionReserveRescheduledDriver, linkedTrip.Driver.OwnerId, reserve.Id,
                        departureDate, departureTime, reserve.CompanionAssistance?.Companion?.Name, null);
                    driverNotified = driverSms || driverPushed;
                }
            }

            if (userNotified || driverNotified)
            {
                try
                {
                    await _context.CompanionReserveReschedules
                        .Where(h => h.Id == history.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(h => h.UserNotified, userNotified)
                            .SetProperty(h => h.DriverNotified, driverNotified), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Saving SMS flags for reserve reschedule {HistoryId} failed.", history.Id);
                }
            }

            return new BaseResultDto<CompanionReserveRescheduleResultVDto>(true, Resource.Notification.CompanionReserveRescheduledSuccessfully,
                new CompanionReserveRescheduleResultVDto
                {
                    ReserveId = reserve.Id,
                    NewDoDate = newDay,
                    NewCompanionTimeId = newTime.Id,
                    NewStartAt = newStart,
                    LinkedTripId = linkedTrip?.Id,
                    LinkedTripDepartureAt = departureAt,
                    UserNotified = userNotified,
                    DriverNotified = driverNotified
                });
        }

        public async Task<BaseResultDto<List<CompanionReserveRescheduleVDto>>> GetHistoryAsync(
            long reserveId, long? companionId, CancellationToken cancellationToken = default)
        {
            var reserve = await _context.CompanionReserves.AsNoTracking()
                .Include(r => r.CompanionAssistance)
                .FirstOrDefaultAsync(r => r.Id == reserveId, cancellationToken);
            var error = CheckAccess(reserve, companionId);
            if (error != null)
                return Fail<List<CompanionReserveRescheduleVDto>>(error);

            var rows = await _context.CompanionReserveReschedules.AsNoTracking()
                .Where(h => h.CompanionReserveId == reserveId)
                .OrderByDescending(h => h.Id)
                .ToListAsync(cancellationToken);
            var actorIds = rows.Select(r => r.ActorUserId).Distinct().ToList();
            var names = await _context.Users.AsNoTracking()
                .Where(u => actorIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim(), cancellationToken);

            var list = rows.Select(h => new CompanionReserveRescheduleVDto
            {
                Id = h.Id,
                CompanionReserveId = h.CompanionReserveId,
                OldDoDate = h.OldDoDate,
                OldCompanionTimeId = h.OldCompanionTimeId,
                OldStartAt = h.OldStartAt,
                NewDoDate = h.NewDoDate,
                NewCompanionTimeId = h.NewCompanionTimeId,
                NewStartAt = h.NewStartAt,
                Reason = h.Reason,
                ActorKind = h.ActorKind,
                ActorUserId = h.ActorUserId,
                ActorName = names.TryGetValue(h.ActorUserId, out var name) ? name : null,
                LinkedTripId = h.LinkedTripId,
                UserNotified = h.UserNotified,
                DriverNotified = h.DriverNotified,
                CreateDate = h.CreateDate
            }).ToList();
            return new BaseResultDto<List<CompanionReserveRescheduleVDto>>(true, list);
        }

        private Task<int> CountOthersAsync(long reserveId, long companionTimeId, DateTime day, CancellationToken ct) =>
            _context.CompanionReserves.AsNoTracking().CountAsync(s =>
                s.Id != reserveId && s.CompanionTimeId == companionTimeId && s.DoDate.Date == day.Date && !s.IsCancel, ct);

        // تداخل ساعتیِ همان نیروی تخصیص‌یافته با رزروهای دیگرش در همان روز
        private async Task<bool> HasAssigneeConflictAsync(
            long reserveId, DateTime day, string startTime, string endTime, long assistanceUserId, CancellationToken ct)
        {
            if (!ReservationScheduleValidator.TryGetServiceTimeRange(startTime, endTime, out var targetStart, out var targetEnd))
                return false;

            var dayStart = day.Date;
            var dayEnd = dayStart.AddDays(1);
            var others = await _context.CompanionReserves.AsNoTracking()
                .Include(s => s.CompanionTime)
                .Include(s => s.CompanionAssistanceTime)
                .Where(s => s.Id != reserveId && s.CompanionAssistanceUserId == assistanceUserId && s.IsReserved && !s.IsCancel
                            && s.DoDate >= dayStart && s.DoDate < dayEnd)
                .ToListAsync(ct);

            return others.Any(s =>
            {
                var start = s.CompanionTime?.StartTime ?? s.CompanionAssistanceTime?.StartTime;
                var end = s.CompanionTime?.EndTime ?? s.CompanionAssistanceTime?.EndTime;
                return start != null && end != null &&
                       ReservationScheduleValidator.TryGetServiceTimeRange(start, end, out var existingStart, out var existingEnd) &&
                       ReservationScheduleValidator.HasTimeRangeOverlap(existingStart, existingEnd, targetStart, targetEnd);
            });
        }

        // سفر پت‌رسانِ متصل (قبل از «پت سوار شد») همراه رزرو جابه‌جا می‌شود: زمان حرکت = زمان جدید شروع منهای فاصله‌ی انتخابیِ کاربر.
        // قیمت سفر عوض نمی‌شود. اگر راننده‌ای قبول کرده، همان می‌ماند و پیامک می‌گیرد؛ وگرنه دوباره به صف اعزام برمی‌گردد.
        private async Task<Trip> AdjustLinkedTripAsync(long reserveId, DateTime newStartAt, DateTime now, CancellationToken ct)
        {
            var requested = (long)TripStatusEnum.TripStatus_Requested;
            var accepted = (long)TripStatusEnum.TripStatus_Accepted;
            var petPickedUp = (int)TripProgressStageEnum.PetPickedUp;

            var trip = await _context.Trips.Include(t => t.Driver).AsTracking()
                .Where(t => t.CompanionReserveId == reserveId
                            && (t.TripStatusId == requested || t.TripStatusId == accepted)
                            && t.ProgressStageId < petPickedUp)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync(ct);
            if (trip == null)
                return null;

            var departure = CompanionReserveRescheduleRules.DepartureFor(newStartAt, trip.ScheduledLeadMinutes);
            if (departure < now)
                departure = now;

            trip.ScheduledDepartureAt = departure;
            trip.TripStartDateTime = departure;
            trip.DriverReminderSentDate = null;
            if (!trip.DriverId.HasValue)
                trip.ScheduledDispatched = false;
            return trip;
        }

        // یادآورهای Push زمان‌بندی‌شده‌ی رزرو آنلاین (۱۰ دقیقه قبل و سر ساعت) که هنوز نرفته‌اند با زمان جدید هماهنگ می‌شوند
        private async Task ShiftOnlineRemindersAsync(long reserveId, DateTime newStartAt, CancellationToken ct)
        {
            var before = (long)PushTypeEnum.PushOnlineReserveReminderBeforeCompanion;
            var atTime = (long)PushTypeEnum.PushOnlineReserveReminderAtTimeCompanion;
            var key = reserveId.ToString();

            var pending = await _context.PushNotifications.Include(n => n.PushPattern).AsTracking()
                .Where(n => n.Token4 == key && !n.IsSend && n.Status == null && n.SendDate != null
                            && (n.PushPattern.PushTypeId == before || n.PushPattern.PushTypeId == atTime))
                .ToListAsync(ct);
            foreach (var notification in pending)
                notification.SendDate = notification.PushPattern.PushTypeId == before ? newStartAt.AddMinutes(-10) : newStartAt;
        }

        private async Task<bool> TryPushAsync(PushTypeEnum type, long userId, long reserveId,
            string token1, string token2, string token3, string token4)
        {
            try
            {
                await _pushService.SendPushAsync(type, userId, token1: token1, token2: token2, token3: token3, token4: token4, token5: reserveId.ToString());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reschedule push {Type} for companion reserve {ReserveId} failed.", type, reserveId);
                return false;
            }
        }

        private async Task<bool> TrySmsAsync(MessageTypeEnum type, string mobile, long reserveId,
            string token1 = null, string token2 = null, string token3 = null, string token4 = null, string token5 = null)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return false;
            try
            {
                var label = type.ToString();
                // بدون ردیف تنظیم پیامک (SmsSetting) SmsService بی‌صدا هیچ‌چیز نمی‌فرستد؛ آن را «ارسال نشد» حساب می‌کنیم و لاگ می‌گذاریم
                if (!await _context.SmsSettings.AsNoTracking().AnyAsync(s => s.SmsType.Label == label))
                {
                    _logger.LogWarning("SMS type {Type} has no SmsSetting row; reschedule SMS for reserve {ReserveId} was not sent. Run SeedCompanionReserveRescheduleSms.sql.", type, reserveId);
                    return false;
                }

                var startedAt = DateTime.Now.AddSeconds(-5);
                await _messageSender.SendMessageAsync(type, mobile, null, token1: token1, token2: token2, token3: token3, token4: token4, token5: token5);

                // نتیجه‌ی واقعیِ تحویل به کاوه‌نگار (Status/StatusText) از خود ردیف Sms خوانده می‌شود
                var sms = await _context.Smses.AsNoTracking()
                    .Where(x => x.SmsType.Label == label && x.Receptor == mobile && x.CreateDate >= startedAt)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();
                if (sms?.Status != true)
                    _logger.LogWarning("Reschedule SMS {Type} for reserve {ReserveId} was not accepted by the provider: {StatusText}", type, reserveId, sms?.StatusText);
                return sms?.Status == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reschedule SMS {Type} for companion reserve {ReserveId} failed.", type, reserveId);
                return false;
            }
        }
    }
}
