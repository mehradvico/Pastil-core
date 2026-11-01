using Application.Common.Dto.Result;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
using Entities.Entities.ShippingField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv
{
    // ساعت کاری هفتگی هر فروشگاه؛ خارج از آن «ارسال اکسپرس» به مشتری ارائه/پذیرفته نمی‌شود
    public class StoreWorkingHourService : IStoreWorkingHourService
    {
        private readonly IDataBaseContext _context;

        public StoreWorkingHourService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<List<StoreWorkingHourDto>>> GetAsync(long storeId, CancellationToken cancellationToken = default)
        {
            var rows = await _context.StoreWorkingHours.AsNoTracking()
                .Where(item => item.StoreId == storeId)
                .ToListAsync(cancellationToken);
            var list = rows
                // ترتیب هفته‌ی ایرانی: شنبه اول
                .OrderBy(item => ((int)item.DayOfWeek + 1) % 7)
                .Select(item => new StoreWorkingHourDto
                {
                    DayOfWeek = item.DayOfWeek,
                    OpenTime = ShippingSlotRules.FormatTime(item.OpenTime),
                    CloseTime = ShippingSlotRules.FormatTime(item.CloseTime)
                })
                .ToList();
            return new BaseResultDto<List<StoreWorkingHourDto>>(true, list);
        }

        public async Task<BaseResultDto> SaveAsync(long storeId, List<StoreWorkingHourDto> hours, CancellationToken cancellationToken = default)
        {
            if (!await _context.Stores.AnyAsync(item => item.Id == storeId && !item.Deleted, cancellationToken))
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            hours ??= new List<StoreWorkingHourDto>();
            var parsed = new List<StoreWorkingHour>();
            foreach (var hour in hours)
            {
                if (!Enum.IsDefined(typeof(DayOfWeek), hour.DayOfWeek) ||
                    !ShippingSlotRules.TryParseTime(hour.OpenTime, out var open) ||
                    !ShippingSlotRules.TryParseTime(hour.CloseTime, out var close) ||
                    close <= open ||
                    parsed.Any(item => item.DayOfWeek == hour.DayOfWeek))
                    return new BaseResultDto(false, Resource.Notification.StoreWorkingHoursInvalid);

                parsed.Add(new StoreWorkingHour { StoreId = storeId, DayOfWeek = hour.DayOfWeek, OpenTime = open, CloseTime = close });
            }

            var existing = await _context.StoreWorkingHours
                .Where(item => item.StoreId == storeId)
                .ToListAsync(cancellationToken);
            _context.StoreWorkingHours.RemoveRange(existing);
            await _context.StoreWorkingHours.AddRangeAsync(parsed, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task<bool> IsOpenAsync(long storeId, DateTime nowUtc, CancellationToken cancellationToken = default)
        {
            var hours = await _context.StoreWorkingHours.AsNoTracking()
                .Where(item => item.StoreId == storeId)
                .Select(item => new { item.DayOfWeek, item.OpenTime, item.CloseTime })
                .ToListAsync(cancellationToken);
            return ShippingExpressRules.IsStoreOpen(
                nowUtc,
                hours.Select(item => (item.DayOfWeek, item.OpenTime, item.CloseTime)).ToList());
        }
    }
}
