using Application.Common.Dto.Result;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
using Entities.Entities.ShippingField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv
{
    public class ShippingSlotService : IShippingSlotService
    {
        private readonly IDataBaseContext _context;
        private readonly ShippingOptions _options;

        public ShippingSlotService(IDataBaseContext context, IOptions<ShippingOptions> options)
        {
            _context = context;
            _options = options.Value;
        }

        public async Task<BaseResultDto<List<ShippingSlotDayVDto>>> GetAvailableAsync(long storeId, CancellationToken cancellationToken = default)
        {
            var nowUtc = DateTime.UtcNow;
            var today = ShippingSlotRules.ToTehran(nowUtc).Date;
            var prep = await GetPrepMinutesAsync(storeId, cancellationToken);
            var horizon = ShippingSlotRules.HorizonDays(_options.SlotHorizonDays, prep);
            var slots = await _context.ShippingSlots.AsNoTracking()
                .Where(slot => slot.Active && !slot.Deleted)
                .ToListAsync(cancellationToken);
            var dates = Enumerable.Range(1, horizon).Select(offset => today.AddDays(offset)).ToList();
            var used = await CountUsedAsync(slots.Select(slot => slot.Id).ToList(), dates, cancellationToken);

            var days = new List<ShippingSlotDayVDto>();
            foreach (var date in dates)
            {
                var options = slots
                    .Where(slot => slot.DayOfWeek == date.DayOfWeek
                                   && ShippingSlotRules.IsFeasible(date, slot.EndTime, nowUtc, prep, _options.PrepBufferMinutes, _options.MinDeliveryMinutes))
                    .OrderBy(slot => slot.StartTime)
                    .Select(slot =>
                    {
                        var remaining = Math.Max(0, slot.Capacity - used.GetValueOrDefault((slot.Id, date), 0));
                        return new ShippingSlotOptionVDto
                        {
                            SlotId = slot.Id,
                            StartTime = ShippingSlotRules.FormatTime(slot.StartTime),
                            EndTime = ShippingSlotRules.FormatTime(slot.EndTime),
                            Remaining = remaining,
                            Available = remaining > 0
                        };
                    })
                    .ToList();
                if (options.Count > 0)
                    days.Add(new ShippingSlotDayVDto { Date = date, DayOfWeek = date.DayOfWeek, Slots = options });
            }
            return new BaseResultDto<List<ShippingSlotDayVDto>>(true, days);
        }

        public async Task<BaseResultDto> ValidateSelectionAsync(long storeId, long slotId, DateTime date, CancellationToken cancellationToken = default)
        {
            var slot = await _context.ShippingSlots.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == slotId && item.Active && !item.Deleted, cancellationToken);
            date = date.Date;
            var nowUtc = DateTime.UtcNow;
            var prep = await GetPrepMinutesAsync(storeId, cancellationToken);
            if (slot == null
                || slot.DayOfWeek != date.DayOfWeek
                || !ShippingSlotRules.IsDateInWindow(date, nowUtc, ShippingSlotRules.HorizonDays(_options.SlotHorizonDays, prep))
                || !ShippingSlotRules.IsFeasible(date, slot.EndTime, nowUtc, prep, _options.PrepBufferMinutes, _options.MinDeliveryMinutes))
                return new BaseResultDto(false, Resource.Notification.ShippingSlotUnavailable);

            var used = await CountUsedAsync(new List<long> { slot.Id }, new List<DateTime> { date }, cancellationToken);
            return used.GetValueOrDefault((slot.Id, date), 0) >= slot.Capacity
                ? new BaseResultDto(false, Resource.Notification.ShippingSlotFull)
                : new BaseResultDto(true);
        }

        // حداکثر زمان آماده‌سازی فروشگاه (دقیقه)؛ فروشگاه ناشناخته → پیش‌فرض ۱۲۰
        private async Task<int> GetPrepMinutesAsync(long storeId, CancellationToken cancellationToken)
        {
            var minutes = await _context.Stores.AsNoTracking()
                .Where(store => store.Id == storeId)
                .Select(store => (int?)store.MaxPreparationMinutes)
                .FirstOrDefaultAsync(cancellationToken);
            return Math.Clamp(minutes ?? 120, 0, 10080);
        }

        // تعداد مرسوله‌های پرداخت‌شده (لغو‌نشده) هر (بازه، تاریخ)
        private async Task<Dictionary<(long, DateTime), int>> CountUsedAsync(
            List<long> slotIds, List<DateTime> dates, CancellationToken cancellationToken)
        {
            if (slotIds.Count == 0 || dates.Count == 0)
                return new Dictionary<(long, DateTime), int>();
            var from = dates.Min();
            var to = dates.Max();
            var rows = await _context.Shipments.AsNoTracking()
                .Where(shipment => shipment.Status != ShipmentStatusEnum.Cancelled
                                   && shipment.ProductOrderStore.ShippingSlotId != null
                                   && slotIds.Contains(shipment.ProductOrderStore.ShippingSlotId.Value)
                                   && shipment.ProductOrderStore.ShippingSlotDate >= from
                                   && shipment.ProductOrderStore.ShippingSlotDate <= to)
                .Select(shipment => new
                {
                    SlotId = shipment.ProductOrderStore.ShippingSlotId.Value,
                    Date = shipment.ProductOrderStore.ShippingSlotDate.Value
                })
                .ToListAsync(cancellationToken);
            return rows
                .GroupBy(row => (row.SlotId, row.Date.Date))
                .ToDictionary(group => group.Key, group => group.Count());
        }

        public async Task<BaseResultDto<List<ShippingSlotDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var items = await _context.ShippingSlots.AsNoTracking()
                .Where(slot => !slot.Deleted)
                .OrderBy(slot => slot.DayOfWeek).ThenBy(slot => slot.StartTime)
                .ToListAsync(cancellationToken);
            return new BaseResultDto<List<ShippingSlotDto>>(true, items.Select(ToDto).ToList());
        }

        public async Task<BaseResultDto> SaveAsync(ShippingSlotDto dto, CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(typeof(DayOfWeek), dto.DayOfWeek)
                || !ShippingSlotRules.TryParseTime(dto.StartTime, out var start)
                || !ShippingSlotRules.TryParseTime(dto.EndTime, out var end)
                || start >= end)
                return new BaseResultDto(false, Resource.Notification.ShippingSlotInvalidTimes);
            if (dto.Capacity < 1 || dto.Capacity > 10000)
                return new BaseResultDto(false, Resource.Notification.ShippingSlotInvalidCapacity);

            var overlap = await _context.ShippingSlots.AsNoTracking()
                .AnyAsync(slot => !slot.Deleted && slot.Id != dto.Id && slot.DayOfWeek == dto.DayOfWeek
                                  && slot.StartTime < end && start < slot.EndTime, cancellationToken);
            if (overlap)
                return new BaseResultDto(false, Resource.Notification.ShippingSlotOverlap);

            ShippingSlot entity;
            if (dto.Id > 0)
            {
                entity = await _context.ShippingSlots.AsTracking()
                    .FirstOrDefaultAsync(slot => slot.Id == dto.Id && !slot.Deleted, cancellationToken);
                if (entity == null)
                    return new BaseResultDto(false, Resource.Notification.ShippingSlotNotFound);
            }
            else
            {
                entity = new ShippingSlot { CreatedAtUtc = DateTime.UtcNow };
                await _context.ShippingSlots.AddAsync(entity, cancellationToken);
            }
            entity.DayOfWeek = dto.DayOfWeek;
            entity.StartTime = start;
            entity.EndTime = end;
            entity.Capacity = dto.Capacity;
            entity.Active = dto.Active;
            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ShippingSlots.AsTracking()
                .FirstOrDefaultAsync(slot => slot.Id == id && !slot.Deleted, cancellationToken);
            if (entity == null)
                return new BaseResultDto(false, Resource.Notification.ShippingSlotNotFound);
            // سفارش‌های قبلی snapshot ساعت را در Shipment دارند، پس حذف نرم امن است
            entity.Deleted = true;
            entity.Active = false;
            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto(true);
        }

        private static ShippingSlotDto ToDto(ShippingSlot slot) => new()
        {
            Id = slot.Id,
            DayOfWeek = slot.DayOfWeek,
            StartTime = ShippingSlotRules.FormatTime(slot.StartTime),
            EndTime = ShippingSlotRules.FormatTime(slot.EndTime),
            Capacity = slot.Capacity,
            Active = slot.Active
        };
    }
}
