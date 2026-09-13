using Application.Common.DayToDate.Dto;
using Application.Common.DayToDate.Iface;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CompanionSrv.CompanionTimeSrv.Dto;
using Application.Services.CompanionSrv.CompanionTimeSrv.Iface;
using Application.Services.WeekDaySrv.WeekDaySrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrv.CompanionTimeSrv
{
    public class CompanionTimeService : CommonSrv<CompanionTime, CompanionTimeDto>, ICompanionTimeService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IWeekDayService _weekDayService;
        private readonly IDayToDateService _dayToDateService;
        public CompanionTimeService(IDataBaseContext _context, IWeekDayService weekDayService, IDayToDateService dayToDateService, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            _weekDayService = weekDayService;
            _dayToDateService = dayToDateService;
        }

        public async Task<BaseResultDto<CompanionTimeVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionTimes.Where(s => s.Deleted == false).Include(s => s.Companion).ThenInclude(s => s.Picture).Include(s => s.WeekDay).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<CompanionTimeVDto>(true, mapper.Map<CompanionTimeVDto>(item));
            }
            return new BaseResultDto<CompanionTimeVDto>(false, mapper.Map<CompanionTimeVDto>(item));
        }

        public CompanionTimeSearchDto Search(CompanionTimeInputDto baseSearchDto)
        {
            var model = _context.CompanionTimes.Where(s => s.Deleted == false).Include(s => s.Companion).ThenInclude(s => s.Picture).Include(s => s.WeekDay).AsQueryable();

            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);
            }
            if (baseSearchDto.WeekDayId.HasValue)
            {
                model = model.Where(s => s.WeekDayId == baseSearchDto.WeekDayId.Value);
            }
            if (baseSearchDto.Active.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Active.Value);
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    break;
            }
            return new CompanionTimeSearchDto(baseSearchDto, model, mapper);
        }

        // نسخه‌ی مخصوص نمایش مشتری‌محور (رزرو نوبت): علاوه بر همان نتیجه‌ی Search، برای هر
        // بازه‌ی زمانی، ظرفیت خالی‌اش را نسبت به نزدیک‌ترین وقوع آینده‌اش (مثلاً «شنبه بعدی»)
        // بر اساس تعداد رزروهای ثبت‌شده‌ی همان روز محاسبه و روی IsFull/RemainingCapacity می‌گذارد.
        public async Task<CompanionTimeSearchDto> SearchWithAvailabilityAsync(CompanionTimeInputDto baseSearchDto)
        {
            var result = Search(baseSearchDto);
            if (result.List == null || result.List.Count == 0)
                return result;

            foreach (var item in result.List)
            {
                if (item.WeekDay == null)
                    continue;

                var occurrenceDate = _dayToDateService.GetNextDateByDayNumber(new DayToDateDto { DayNumber = item.WeekDay.Number }).Date;

                var reservedCount = await _context.CompanionReserves.CountAsync(s =>
                    s.CompanionTimeId == item.Id &&
                    s.DoDate.Date == occurrenceDate.Date &&
                    !s.IsCancel);

                var capacity = item.Capacity > 0 ? item.Capacity : 1;
                item.RemainingCapacity = Math.Max(capacity - reservedCount, 0);
                item.IsFull = item.RemainingCapacity <= 0;
            }

            return result;
        }

        public async Task<BaseResultDto<CompanionTimeUpdateListDto>> GetListAsync(long companionId)
        {
            var group = await _context.CompanionTimes.Where(s => s.Deleted == false && s.CompanionId == companionId).OrderBy(s => s.WeekDayId).ThenBy(s => s.StartTime).GroupBy(g => g.WeekDay).ToListAsync();
            var list = mapper.Map<List<IGrouping<WeekDay, CompanionTime>>, List<CompanionTimeUpdateDto>>(group);
            if (list.Count < 7)
            {
                var weekDays = _weekDayService.GetWeekDays();
                foreach (var weekDay in weekDays)
                {
                    if (!list.Any(a => a.WeekDay.Id == weekDay.Id))
                    {
                        list.Add(new CompanionTimeUpdateDto() { WeekDay = weekDay });
                    }
                }
                list = list.OrderBy(s => s.WeekDay.Id).ToList();
            }
            var result = new CompanionTimeUpdateListDto()
            {
                CompanionId = companionId,
                CompanionTimeUpdateList = list
            };
            return new BaseResultDto<CompanionTimeUpdateListDto>(true, result);
        }

        public async Task<BaseResultDto> InsertUpdateListAsync(CompanionTimeUpdateListDto dto, long? companionOwnerId = null)
        {
            try
            {
                if (companionOwnerId.HasValue && dto.CompanionId != companionOwnerId.Value)
                {
                    return new BaseResultDto<CompanionTimeUpdateListDto>(false, Resource.Notification.AccessDenied, dto);
                }

                foreach (var item in dto.CompanionTimeUpdateList)
                {
                    var dbTimes = await _context.CompanionTimes
                        .Where(s => s.CompanionId == dto.CompanionId
                                    && s.WeekDayId == item.WeekDay.Id
                                    && !s.Deleted)
                        .ToListAsync();

                    var newTimes = item.CompanionTimes.Where(s => s.Id == 0).ToList();

                    foreach (var newTime in newTimes)
                    {
                        var startTime = TimeSpan.Parse(newTime.StartTime);
                        var endTime = TimeSpan.Parse(newTime.EndTime);

                        var hasOverlap = dbTimes.Any(db =>
                        {
                            var dbStart = TimeSpan.Parse(db.StartTime);
                            var dbEnd = TimeSpan.Parse(db.EndTime);
                            return startTime < dbEnd && endTime > dbStart;
                        });

                        if (hasOverlap)
                            return new BaseResultDto<CompanionTimeUpdateListDto>(false, Resource.Notification.TimesHaveOverlap, dto);

                        foreach (var other in newTimes.Where(x => x != newTime))
                        {
                            var oStart = TimeSpan.Parse(other.StartTime);
                            var oEnd = TimeSpan.Parse(other.EndTime);
                            if (startTime < oEnd && endTime > oStart)
                                return new BaseResultDto<CompanionTimeUpdateListDto>(false, Resource.Notification.TimesHaveOverlap, dto);
                        }
                    }
                }

                foreach (var item in dto.CompanionTimeUpdateList)
                {
                    foreach (var item2 in item.CompanionTimes.Where(s => s.Id == 0))
                    {
                        item2.Active = true;
                        var i = mapper.Map<CompanionTime>(item2);
                        await _context.CompanionTimes.AddAsync(i);
                    }
                }

                var existList = dto.CompanionTimeUpdateList
                    .SelectMany(s => s.CompanionTimes.Where(a => a.Id > 0).Select(a => a.Id))
                    .ToList();

                await _context.CompanionTimes
                    .Where(s => s.CompanionId == dto.CompanionId && !existList.Contains(s.Id))
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.Deleted, true));

                await _context.SaveChangesAsync();
                return new BaseResultDto(true);
            }
            catch
            {
                return new BaseResultDto(false);
            }
        }
    }
}
