using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationBookingSrv
{
    // برنامه‌ی هفتگی کلینیک + محاسبه‌ی اسلات‌ها + بررسی نهایی اسلات هنگام خرید.
    // طراحی: backend/Docs/ONLINE_CONSULTATION_BOOKING_FA.md
    public class ConsultationBookingService : IConsultationBookingService
    {
        private const int MaxWindows = 50;

        private readonly IDataBaseContext _context;

        public ConsultationBookingService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<List<ConsultationAvailabilityWindowDto>>> GetAvailabilityAsync(long companionId)
        {
            try
            {
                return new BaseResultDto<List<ConsultationAvailabilityWindowDto>>(true, await LoadWindowsAsync(companionId));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationAvailabilityWindowDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<List<ConsultationAvailabilityWindowDto>>> SaveAvailabilityAsync(long companionId, List<ConsultationAvailabilityWindowDto> windows)
        {
            try
            {
                windows ??= new List<ConsultationAvailabilityWindowDto>();
                if (windows.Count > MaxWindows)
                    return new BaseResultDto<List<ConsultationAvailabilityWindowDto>>(false, Resource.Notification.ConsultationAvailabilityInvalid, null);

                var parsed = new List<(int WeekDayId, int Start, int End, int Capacity)>();
                foreach (var w in windows)
                {
                    var start = ConsultationBookingRules.ParseTime(w?.Start);
                    var end = ConsultationBookingRules.ParseTime(w?.End);
                    if (w == null || !start.HasValue || !end.HasValue ||
                        ConsultationBookingRules.ValidateWindow(w.WeekDayId, start.Value, end.Value, w.Capacity) != ConsultationBookingRules.WindowProblem.None)
                        return new BaseResultDto<List<ConsultationAvailabilityWindowDto>>(false, Resource.Notification.ConsultationAvailabilityInvalid, null);
                    parsed.Add((w.WeekDayId, start.Value, end.Value, w.Capacity));
                }
                if (ConsultationBookingRules.HasOverlap(parsed.Select(p => (p.WeekDayId, p.Start, p.End))))
                    return new BaseResultDto<List<ConsultationAvailabilityWindowDto>>(false, Resource.Notification.ConsultationAvailabilityInvalid, null);

                var companionExists = await _context.Companions.AsNoTracking().AnyAsync(s => s.Id == companionId && !s.Deleted);
                if (!companionExists)
                    return new BaseResultDto<List<ConsultationAvailabilityWindowDto>>(false, Resource.Notification.NothingFound, null);

                await using var transaction = _context.CurrentTransaction == null
                    ? await _context.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                    : null;

                // جایگزینی کامل: ردیف‌های قبلی حذف نرم می‌شوند (رزروهای انجام‌شده به ردیف وابسته نیستند)
                await _context.ConsultationAvailabilities
                    .Where(s => s.CompanionId == companionId && !s.Deleted)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Deleted, true).SetProperty(s => s.Active, false));

                var now = DateTime.Now;
                await _context.ConsultationAvailabilities.AddRangeAsync(parsed.Select(p => new ConsultationAvailability
                {
                    CompanionId = companionId,
                    WeekDayId = p.WeekDayId,
                    StartMinute = p.Start,
                    EndMinute = p.End,
                    Capacity = p.Capacity,
                    Active = true,
                    CreateDate = now
                }));
                await _context.SaveChangesAsync();
                if (transaction != null)
                    await transaction.CommitAsync();

                return new BaseResultDto<List<ConsultationAvailabilityWindowDto>>(true, await LoadWindowsAsync(companionId));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationAvailabilityWindowDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationSlotsVDto>> GetSlotsAsync(long packageId, DateTime date)
        {
            try
            {
                var package = await LoadBookablePackageAsync(packageId);
                if (package == null)
                    return new BaseResultDto<ConsultationSlotsVDto>(false, Resource.Notification.NothingFound, null);

                var now = DateTime.Now;
                var day = date.Date;
                var windows = await WindowsOfDayAsync(package.CompanionId, day);
                var existing = await ExistingAsync(package.CompanionId, day, day.AddDays(1).AddMinutes(package.DurationMinutes), now);
                var slots = ConsultationBookingRules.BuildSlots(day, package.DurationMinutes, now, windows, existing)
                    .Select(s => new ConsultationSlotVDto { Start = s.Start, End = s.End, Available = s.Available })
                    .ToList();

                return new BaseResultDto<ConsultationSlotsVDto>(true, new ConsultationSlotsVDto
                {
                    PackageId = package.Id,
                    DurationMinutes = package.DurationMinutes,
                    Date = day,
                    Slots = slots,
                    ServerNow = now
                });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationSlotsVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationBookingDaysVDto>> GetDaysAsync(long packageId)
        {
            try
            {
                var package = await LoadBookablePackageAsync(packageId);
                if (package == null)
                    return new BaseResultDto<ConsultationBookingDaysVDto>(false, Resource.Notification.NothingFound, null);

                var now = DateTime.Now;
                var first = now.Date;
                var last = first.AddDays(ConsultationRules.BookingMaxDaysAhead);
                var allWindows = await _context.ConsultationAvailabilities.AsNoTracking()
                    .Where(s => s.CompanionId == package.CompanionId && !s.Deleted && s.Active)
                    .Select(s => new { s.WeekDayId, s.StartMinute, s.EndMinute, s.Capacity })
                    .ToListAsync();
                var existing = await ExistingAsync(package.CompanionId, first, last.AddDays(1).AddMinutes(package.DurationMinutes), now);

                var days = new List<ConsultationBookingDayVDto>();
                for (var d = first; d <= last; d = d.AddDays(1))
                {
                    var weekDay = ConsultationBookingRules.WeekDayId(d);
                    var windows = allWindows.Where(w => w.WeekDayId == weekDay)
                        .Select(w => new ConsultationBookingRules.Window(w.StartMinute, w.EndMinute, w.Capacity)).ToList();
                    var available = ConsultationBookingRules.BuildSlots(d, package.DurationMinutes, now, windows, existing).Count(s => s.Available);
                    days.Add(new ConsultationBookingDayVDto { Date = d, WeekDayId = weekDay, AvailableSlots = available });
                }

                return new BaseResultDto<ConsultationBookingDaysVDto>(true, new ConsultationBookingDaysVDto
                {
                    PackageId = package.Id,
                    DurationMinutes = package.DurationMinutes,
                    Days = days,
                    ServerNow = now
                });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationBookingDaysVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<ConsultationBookingRules.SlotProblem> CheckSlotAsync(long companionId, int durationMinutes, DateTime start, DateTime now)
        {
            var windows = await WindowsOfDayAsync(companionId, start.Date);
            var end = start.AddMinutes(durationMinutes);
            var existing = await ExistingAsync(companionId, start, end, now);
            var overlaps = ConsultationBookingRules.CountOverlaps(existing, start, end);
            return ConsultationBookingRules.CheckSlot(start, durationMinutes, now, windows, overlaps);
        }

        public Task<bool> UserHasOverlapAsync(long userId, DateTime start, DateTime end, DateTime now)
        {
            var holdSince = now - ConsultationRules.BookingPendingHold;
            var pending = (int)ConsultationPurchaseStatusEnum.PendingPayment;
            var paid = (int)ConsultationPurchaseStatusEnum.Paid;
            var active = (int)ConsultationPurchaseStatusEnum.Active;
            return _context.ConsultationPurchases.AsNoTracking().AnyAsync(s =>
                s.UserId == userId && s.ScheduledStart != null && s.ScheduledEnd != null &&
                s.ScheduledStart < end && s.ScheduledEnd > start &&
                (s.Status == paid || s.Status == active || (s.Status == pending && s.CreateDate >= holdSince)));
        }

        // ---- کمکی‌ها

        private async Task<ConsultationPackage> LoadBookablePackageAsync(long packageId)
        {
            var package = await _context.ConsultationPackages.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == packageId && !s.Deleted && s.Active && s.Bookable && s.Price > 0);
            if (package == null)
                return null;
            var open = await _context.Companions.AsNoTracking()
                .AnyAsync(s => s.Id == package.CompanionId && !s.Deleted && s.Active && s.Approved);
            return open ? package : null;
        }

        private async Task<List<ConsultationBookingRules.Window>> WindowsOfDayAsync(long companionId, DateTime day)
        {
            var weekDay = ConsultationBookingRules.WeekDayId(day);
            return await _context.ConsultationAvailabilities.AsNoTracking()
                .Where(s => s.CompanionId == companionId && s.WeekDayId == weekDay && !s.Deleted && s.Active)
                .Select(s => new ConsultationBookingRules.Window(s.StartMinute, s.EndMinute, s.Capacity))
                .ToListAsync();
        }

        // رزروهای ساعت‌دارِ ظرفیت‌گیر: پرداخت‌شده، در جریان، و در انتظار پرداختِ تازه (تا BookingPendingHold)
        private async Task<List<(DateTime Start, DateTime End)>> ExistingAsync(long companionId, DateTime from, DateTime to, DateTime now)
        {
            var holdSince = now - ConsultationRules.BookingPendingHold;
            var pending = (int)ConsultationPurchaseStatusEnum.PendingPayment;
            var paid = (int)ConsultationPurchaseStatusEnum.Paid;
            var active = (int)ConsultationPurchaseStatusEnum.Active;
            var rows = await _context.ConsultationPurchases.AsNoTracking()
                .Where(s => s.CompanionId == companionId && s.ScheduledStart != null && s.ScheduledEnd != null &&
                            s.ScheduledStart < to && s.ScheduledEnd > from &&
                            (s.Status == paid || s.Status == active || (s.Status == pending && s.CreateDate >= holdSince)))
                .Select(s => new { Start = s.ScheduledStart.Value, End = s.ScheduledEnd.Value })
                .ToListAsync();
            return rows.Select(r => (r.Start, r.End)).ToList();
        }

        private async Task<List<ConsultationAvailabilityWindowDto>> LoadWindowsAsync(long companionId)
        {
            var rows = await _context.ConsultationAvailabilities.AsNoTracking()
                .Where(s => s.CompanionId == companionId && !s.Deleted && s.Active)
                .OrderBy(s => s.WeekDayId).ThenBy(s => s.StartMinute)
                .ToListAsync();
            return rows.Select(s => new ConsultationAvailabilityWindowDto
            {
                WeekDayId = s.WeekDayId,
                Start = ConsultationBookingRules.FormatTime(s.StartMinute),
                End = ConsultationBookingRules.FormatTime(s.EndMinute),
                Capacity = s.Capacity
            }).ToList();
        }
    }
}
